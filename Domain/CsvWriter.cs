using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Services;
using NLog;
using Utility;
using System.Threading;

namespace Domain
{
    public sealed class CsvWriter
    {
        
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly int _maxTryWriteCsv = 5;

        //Generate a Csv for a position
        internal void WriteCsv(string writeFile, string delimiter, string columnOneName,
            string columnTwoName, string dataDateFormat, Position position)
        {
            try
            {
                using (var streamWriter = new StreamWriter(writeFile))
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(columnOneName + delimiter + " " + columnTwoName);
                    foreach (var hourlyPosition in position.HourlyPositions)
                    {
                        var writeAmount = hourlyPosition.Value.Amount;
                        var hour = (hourlyPosition.Key + 22)%24;
                        var writeHour = new DateTime(1, 1, 1, hour, 0, 0).ToString(dataDateFormat);
                        var line = writeHour + delimiter + writeAmount;
                        sb.AppendLine(line);
                    }
                    //write whole file at once so no incomplete files are written
                    streamWriter.Write(sb);
                }
            }
            catch (Exception ex)
            {
                throw new WriteCsvException(
                    "An error has occured attempting to write the csv file.  File name : " + writeFile + ".", ex);
            }
        }

        private string GetFullFileName(string fileFolder, string fileName, string fileDateFormat, string fileSuffix,
            DateTime forTime)
        {
            return fileFolder + @"\" + fileName + forTime.ToString(fileDateFormat) + fileSuffix;
        }

        //Generate a csv for position at a set interval
        public void StartCsvMinuteInterval(string fileFolder, string fileName, string fileDateFormat, string fileSuffix,
            string delimiter, string columnOneName,
            string columnTwoName, string dataDateFormat, int minuteInterval, IPowerService powerService,
            TimeSpan timeout)
        {
            //var tokenSource = new CancellationTokenSource();
            var lastWroteCsv = new DateTime?();
            
            var task = Task.Run(async () =>
            {
                while (true)
                {

                    lastWroteCsv = TryWriteCsvMinuteInterval(fileFolder, fileName, fileDateFormat, fileSuffix, delimiter,
                        columnOneName,
                        columnTwoName,
                        dataDateFormat, minuteInterval, null, powerService, timeout, lastWroteCsv, 0);
                    await Task.Delay(10000);
                }
            });            
        }

        //Generate historical position csv files
        public void WriteCsvMinuteIntervalHistorical(string fileFolder, string fileName, string fileDateFormat,
            string fileSuffix, string delimiter, string columnOneName,
            string columnTwoName, string dataDateFormat, int minuteInterval, IPowerService powerService,
            TimeSpan timeout, DateTime fromDate, DateTime toDate, bool overwriteExisting)
        {
            var lastWroteCsv = new DateTime?(fromDate.AddHours(-1));
            
            for (DateTime checkDateTime = fromDate;
                checkDateTime <= toDate;
                checkDateTime = checkDateTime.AddMinutes(1))
            {
                lastWroteCsv = TryWriteCsvMinuteInterval(fileFolder, fileName, fileDateFormat, fileSuffix, delimiter,
                    columnOneName,
                    columnTwoName, dataDateFormat, minuteInterval, checkDateTime, powerService, timeout, lastWroteCsv,
                    0, overwriteExisting);
            }

        }

        //Write the csv file if the current time is at the minute Interval and no other file has been written by the process.  Otherwise write the file.
        internal DateTime? TryWriteCsvMinuteInterval(string fileFolder, string fileName, string fileDateFormat,
            string fileSuffix, string delimiter, string columnOneName,
            string columnTwoName, string dataDateFormat, int minuteInterval, DateTime? asAt, IPowerService powerService,
            TimeSpan timeout, DateTime? lastWroteCsv,
            int tryWriteCsv, bool overwriteExisting = true)
        {
            try
            {
                tryWriteCsv++;
                return WriteCsvMinuteInterval(fileFolder, fileName, fileDateFormat, fileSuffix, delimiter, columnOneName,
                    columnTwoName, dataDateFormat,
                    minuteInterval, asAt, powerService, timeout, lastWroteCsv);
            }
            catch (Exception ex)
                when (ex is PowerServiceException || ex is WriteCsvException || ex is AggregateException)
            {
                //Will retry the procedure a specified number of times when the above errors are called because they are potentially recoverable
                if (tryWriteCsv <= _maxTryWriteCsv)
                {
                    //log and allow retry on next timer interval    
                    var exceptionMessages = ex.GetExceptionMessages();
                    var errorMessage =
                        "An error has occured trying to write the csv file.  This is attempt number " + tryWriteCsv +
                        ".  A maximum of " + _maxTryWriteCsv + " attempts will be made to write the file " +
                        ".  Detailed information is below. \r\n\r\n"
                        + exceptionMessages;
                    _logger.Error(ex, errorMessage);
                    return TryWriteCsvMinuteInterval(fileFolder, fileName, fileDateFormat, fileSuffix, delimiter,
                        columnOneName, columnTwoName, dataDateFormat,
                        minuteInterval, asAt, powerService, timeout, lastWroteCsv, tryWriteCsv);
                }
                else
                {
                    throw;
                }
            }
        }

        //Write the csv file if the current time is at the minute Interval and no other file has been written by the process.  Otherwise write the file.
        internal DateTime? WriteCsvMinuteInterval(string fileFolder, string fileName, string fileDateFormat, string fileSuffix, string delimiter, string columnOneName,
            string columnTwoName, string dataDateFormat, int minuteInterval, DateTime? asAt, IPowerService powerService, TimeSpan timeout, DateTime? lastWroteCsv,
            bool overwriteExisting = true)
        {
            //use at parameter to override what time the method believes it is.  This is useful for testing.
            //using Universal time means do not have to worry about clocks changing
            DateTime nowDateTime = asAt ?? DateTime.Now.ToUniversalTime();

            //if null then this is a first run so write the file if 
            if (lastWroteCsv == null || (nowDateTime.Minute % minuteInterval == 0 &&
               (nowDateTime.Day != lastWroteCsv.Value.Day || nowDateTime.Hour != lastWroteCsv.Value.Hour || nowDateTime.Minute != lastWroteCsv.Value.Minute)))
            {                
                var writeFile = GetFullFileName(fileFolder, fileName, fileDateFormat, fileSuffix, nowDateTime);
                var fileExists = File.Exists(writeFile);
                if (overwriteExisting == true || (!fileExists))
                {                        
                    var tradesTask = powerService.GetTradesAsync(nowDateTime);
                    if (!tradesTask.Wait(timeout))
                    {
                        var exceptionMessage =
                            "The operation to retrieve power trades has timed out.  Timeout value used was " +
                            timeout.Seconds + " seconds.  The date parameter passed was " +
                            nowDateTime.ToString("R");
                        throw new WriteCsvException(exceptionMessage);
                    }
                    var trades = tradesTask.Result;
                    var position = new Position(trades, nowDateTime);
                    WriteCsv(writeFile, delimiter, columnOneName, columnTwoName, dataDateFormat, position);                     
                    return nowDateTime;                        
                }
            }
            // by default assume csv not written               
            return lastWroteCsv;            
        }
    }
}
