using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Services;
using NLog;
using Utility;

namespace Domain
{
    public sealed class CsvWriter : IDisposable 
    {
        //   private static System.Timers.Timer _timer;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private System.Threading.Timer _timer;
        private DateTime? _lastWroteCsv;
        private int _tryWriteCsv;
        private readonly int _maxTryWriteCsv = 5;

        // pure sealed class does not need virtual dispose method or suppress finalizer.  No finalizer as timer is managed resource.
        public void Dispose()
        {
            _timer?.Dispose();            
        }
        
        //Generate a Csv for a position
        internal void WriteCsv(string fileFolder, string fileName, string fileDateFormat, string fileSuffix, string delimiter, string columnOneName, 
            string columnTwoName, string dataDateFormat, Position position)
        {
            var writeFile = GetFullFileName(fileFolder, fileName, fileDateFormat, fileSuffix, position.ForDate);
            try
            {
                using (var streamWriter = new StreamWriter(writeFile))
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(columnOneName + delimiter + " " + columnTwoName);                    
                    foreach (var hourlyPosition in position.HourlyPositions)
                    {
                        var writeAmount = hourlyPosition.Value.Amount;
                        var hour = (hourlyPosition.Key + 22) % 24;
                        var writeHour = new DateTime(1, 1, 1, hour, 0, 0).ToString(dataDateFormat);
                        var line = writeHour + delimiter + writeAmount;
                        sb.AppendLine(line);
                    }
                    //write whole file at once so no incomplete files are written
                    streamWriter.Write(sb);
                }
            }
            catch(Exception ex)
            {                
                throw new WriteCsvException("An error has occured attempting to write the csv file.  File name : " + writeFile + ".", ex);
            }
        }

        private string GetFullFileName(string fileFolder, string fileName, string fileDateFormat, string fileSuffix, DateTime forTime)
        {
            return fileFolder + @"\" + fileName + forTime.ToString(fileDateFormat) + fileSuffix;
        }

        //Generate a csv for position at a set interval
        public void StartCsvMinuteInterval(string fileFolder, string fileName, string fileDateFormat, string fileSuffix, string delimiter, string columnOneName,
            string columnTwoName, string dataDateFormat, int minuteInterval, IPowerService powerService, TimeSpan timeout)
        {            
            _timer = new System.Threading.Timer(t => 
            WriteCsvMinuteInterval(fileFolder, fileName, fileDateFormat, fileSuffix, delimiter, columnOneName, columnTwoName,
                  dataDateFormat, minuteInterval, null, powerService, timeout)                  
                  , null, 0, 10000);                        
        }

        //Write the csv file if the current time is at the minute Interval and no other file has been written by the process.  Otherwise write the file.
        public void WriteCsvMinuteInterval(string fileFolder, string fileName, string fileDateFormat, string fileSuffix, string delimiter, string columnOneName,
            string columnTwoName, string dataDateFormat, int minuteInterval, DateTime? asAt, IPowerService powerService, TimeSpan timeout)
        {
            //use at parameter to override what time the method believes it is.  This is useful for testing.
            //using Universal time means do not have to worry about clocks changing
            DateTime nowDateTime = asAt ?? DateTime.Now.ToUniversalTime();
            
            //if null then this is a first run so write the file if 
            if (_lastWroteCsv == null || (nowDateTime.Minute % minuteInterval == 0 && 
                (nowDateTime.Day != _lastWroteCsv.Value.Day || nowDateTime.Hour != _lastWroteCsv.Value.Hour || nowDateTime.Minute != _lastWroteCsv.Value.Minute)))
            {
                try
                {
                    _tryWriteCsv++;                    
                    var tradesTask = powerService.GetTradesAsync(nowDateTime);
                    if (!tradesTask.Wait(timeout))
                    {
                        var exceptionMessage =
                            "The operation to retrieve power trades has timed out.  Timeout value used was " +
                            timeout.Seconds + " seconds.  The date parameter passed was " + nowDateTime.ToString("R");
                        throw new WriteCsvException(exceptionMessage);
                    }
                    var trades  = tradesTask.Result;
                    var position = new Position(trades, nowDateTime);
                    WriteCsv(fileFolder, fileName, fileDateFormat, fileSuffix, delimiter, columnOneName, columnTwoName,
                        dataDateFormat, position);
                    _lastWroteCsv = nowDateTime;
                    _tryWriteCsv = 0;
                }
                catch (Exception ex)
                    when (ex is PowerServiceException || ex is WriteCsvException || ex is AggregateException)
                {
                    //Will retry the procedure a specified number of times when the above errors are called because they are potentially recoverable
                    if (_tryWriteCsv <= _maxTryWriteCsv)
                    {
                        //log and allow retry on next timer interval    
                        var exceptionMessages = ex.GetExceptionMessages();
                        var errorMessage =
                            "An error has occured trying to write the csv file.  This is attempt number " + _tryWriteCsv +
                            ".  Another attempt will be made to write the file up to a maximum of " + _maxTryWriteCsv +
                            ".  Detailed information is below.\r\n\r\n"
                            + exceptionMessages;
                        _logger.Error(ex, errorMessage);
                    }
                    else
                    {
                        throw;
                    }
                }         
            } 
        }

    }

}
