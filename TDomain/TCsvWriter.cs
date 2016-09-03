using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using NUnit.Framework;
using Services;
using Telerik.JustMock;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace TDomain
{
    [TestFixture]
    public class TCsvWriter
    {
        private readonly DateTime _testDateTime = new DateTime(2016, 8, 31, 1, 1, 1);
        private readonly IPowerService _powerService = Mock.Create<IPowerService>();
        private string _csvExampleText;
        private readonly string _fileFolder = @"C:\Position Files";
        private readonly string _fileName = "PowerPosition";
        private readonly string _fileDateFormat = "_yyyyMMdd_HHmm";
        private readonly string _fileSuffix = ".csv";
        private readonly string _delimiter = ",";
        private readonly string _columnOneName = "LocalTime";
        private readonly string _columnTwoName = "Volume";
        private readonly string _dataDateFormat = "HH:mm";
        private readonly TimeSpan _timeOut = new TimeSpan(0,0,5);        
        private List<PowerTrade> _powerTrades;

        [OneTimeSetUp]
        public void Setup()
        {
            //Set up the power service
            var testPeriods = new PowerPeriod[]
                {
                    new PowerPeriod() {Period=1, Volume = 1000},
                    new PowerPeriod() {Period=2, Volume = 2000},
                    new PowerPeriod() {Period=3, Volume = 3000},
                    new PowerPeriod() {Period=4, Volume = 4000},
                    new PowerPeriod() {Period=5, Volume = 5000},
                    new PowerPeriod() {Period=6, Volume = 6000},
                    new PowerPeriod() {Period=7, Volume = 7000},
                    new PowerPeriod() {Period=8, Volume = 8000},
                    new PowerPeriod() {Period=9, Volume = 9000},
                    new PowerPeriod() {Period=10, Volume = 10000},
                    new PowerPeriod() {Period=11, Volume = 11000},
                    new PowerPeriod() {Period=12, Volume = 12000},
                    new PowerPeriod() {Period=13, Volume = 13000},
                    new PowerPeriod() {Period=14, Volume = 14000},
                    new PowerPeriod() {Period=15, Volume = 15000},
                    new PowerPeriod() {Period=16, Volume = 16000},
                    new PowerPeriod() {Period=17, Volume = 17000},
                    new PowerPeriod() {Period=18, Volume = 18000},
                    new PowerPeriod() {Period=19, Volume = 19000},
                    new PowerPeriod() {Period=20, Volume = 20000},
                    new PowerPeriod() {Period=21, Volume = 21000},
                    new PowerPeriod() {Period=22, Volume = 22000},
                    new PowerPeriod() {Period=23, Volume = 23000},
                    new PowerPeriod() {Period=24, Volume = 24000},
                };

            var tradeOne = Mock.Create<PowerTrade>();
            Mock.Arrange(() => tradeOne.Date).Returns(_testDateTime);
            Mock.Arrange(() => tradeOne.Periods).Returns(testPeriods);

            var tradeTwo = Mock.Create<PowerTrade>();
            Mock.Arrange(() => tradeTwo.Date).Returns(_testDateTime.AddHours(1));
            Mock.Arrange(() => tradeTwo.Periods).Returns(testPeriods);

            var tradeThree = Mock.Create<PowerTrade>();
            Mock.Arrange(() => tradeThree.Date).Returns(_testDateTime.AddHours(2));
            Mock.Arrange(() => tradeThree.Periods).Returns(testPeriods);

            _powerTrades = new List<PowerTrade>() {tradeOne, tradeTwo, tradeThree};
            //simulate a short call time on the PowerService.GetTradesAsync
            var powerTradesTask = new Task<IEnumerable<PowerTrade>>(() => _powerTrades);
            powerTradesTask.Start();
            powerTradesTask.Wait();
            Mock.Arrange(() => _powerService.GetTradesAsync(_testDateTime)).Returns(powerTradesTask);
            Mock.Arrange(() => _powerService.GetTrades(_testDateTime)).Returns(_powerTrades);
            //set up the expected data to be written by the csv writer
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "TDomain.PositionExample.csv";
            
            using (StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
            {
                _csvExampleText = reader.ReadToEnd();
            }

            if (!System.IO.Directory.Exists(_fileFolder))
            {
                Directory.CreateDirectory(_fileFolder);
            }
        }

        [Test]
        //Confirm that the csv is written with the correct filename
        public void WriteCsv_file_has_correct_data_and_format()
        {
            
            var powerTrades = _powerService.GetTrades(_testDateTime);
            var position = new Position(powerTrades, _testDateTime);
            var csvWriter = new CsvWriter();            
            //confirm file was written
            string expectedFileName = @"C:\Position Files\PowerPosition_20160831_0101.csv";
            System.IO.File.Delete(expectedFileName);
            csvWriter.WriteCsv(expectedFileName, _delimiter, _columnOneName, _columnTwoName, _dataDateFormat, position);
            Assert.IsTrue(System.IO.File.Exists(expectedFileName));
            var writtenCsvText = File.ReadAllText(expectedFileName);
            Assert.AreEqual(_csvExampleText, writtenCsvText);
        }

        [Test]
        //Confirm that the logic for writing at the set minute interval is working correctly
        public void WriteCsvMinuteInterval_file_written_on_start()
        {            
            var minuteInterval = 30;
            var asAt = _testDateTime;
            var csvWriter = new CsvWriter();
            //confirm file was written
            string expectedFileName = @"C:\Position Files\PowerPosition_20160831_0101.csv";
            //First confirm the File writes
            System.IO.File.Delete(expectedFileName);
            DateTime? lastWroteCsv = null;
            csvWriter.WriteCsvMinuteInterval(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                _dataDateFormat, minuteInterval, asAt, _powerService, _timeOut, lastWroteCsv);
            Assert.IsTrue(System.IO.File.Exists(expectedFileName));            
        }

        [Test]
        //Confirm that the logic for writing at the set minute interval is working correctly
        public void WriteCsvMinuteInterval_file_not_written_interval_not_elapsed()
        {            
            var minuteInterval = 30;
            var asAt = _testDateTime.AddMinutes(20);
            var csvWriter = new CsvWriter();
            DateTime? lastWroteCsv = null;
            //have to write one file first
            lastWroteCsv = csvWriter.WriteCsvMinuteInterval(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                _dataDateFormat, minuteInterval, _testDateTime, _powerService, _timeOut, lastWroteCsv);            
            //now confirm the file will not be created if the minute interval has not elapsed                        
            string expectedFileName = @"C:\Position Files\PowerPosition_20160831_0121.csv";
            System.IO.File.Delete(expectedFileName);
            csvWriter.WriteCsvMinuteInterval(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                _dataDateFormat, minuteInterval, asAt, _powerService, _timeOut, lastWroteCsv);
            Assert.IsTrue(System.IO.File.Exists(expectedFileName) == false);            
        }

        [Test]
        //Confirm that the logic for writing at the set minute interval is working correctly
        public void WriteCsvMinuteInterval_file_written_interval_elapsed()
        {
            var csvWriter = new CsvWriter();
            var minuteInterval = 30;
            var asAt = _testDateTime.AddMinutes(29);
            //confirm file was written
            string expectedFileName = @"C:\Position Files\PowerPosition_20160831_0130.csv";
            DateTime? lastWroteCsv = null;

            //have to write one file first
            lastWroteCsv = csvWriter.WriteCsvMinuteInterval(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                _dataDateFormat, minuteInterval, _testDateTime, _powerService, _timeOut, lastWroteCsv);
            //now confirm the file will be created if the minute interval haselapsed                        
            System.IO.File.Delete(expectedFileName);
            csvWriter.WriteCsvMinuteInterval(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                _dataDateFormat, minuteInterval, asAt, _powerService, _timeOut, lastWroteCsv);
            Assert.IsTrue(System.IO.File.Exists(expectedFileName));
        }

        [Test]
        //Confirm the WriteCvException is being logged in the correct format
        public void WriteCsvMinuteInterval_PowerServiceException_is_caught_and_logged()
        {
            var csvWriter = new CsvWriter();
            var minuteInterval = 30;          
            //confirm file was written
            var logFileName = "PositionAggregatorLogFile.txt";
            var maxTrys = 5;
            DateTime? lastWroteCsv = null;
            File.Delete(logFileName);
            IPowerService powerService = Mock.Create<IPowerService>();
            Mock.Arrange(() => powerService.GetTradesAsync(_testDateTime)).Throws<PowerServiceException>("Power Service Exception");
            //first try five times and confirm log file is written correctly
            for(int i = 1;i<=maxTrys;i++)
            { 
                lastWroteCsv = csvWriter.WriteCsvMinuteInterval(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                    _dataDateFormat, minuteInterval, _testDateTime, powerService, _timeOut, lastWroteCsv);
            }
            //confirm the log file has been written
            Assert.IsTrue(File.Exists(logFileName));
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "TDomain.PositionAggregatorLogFile.txt";
            var logFileExampleText = "";
            using (StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
            {
                logFileExampleText = reader.ReadToEnd();
            }
            var logFileText = File.ReadAllText(logFileName);
            //confirm the log file has been written correctly.  contents will differ due to timing of runs so realistically good enough to just check the lengths
            Assert.AreEqual(logFileExampleText.Length, logFileText.Length);
        }

        [Test]
        //Confirm the WriteCvException is being logged in the correct format
        public void WriteCsvMinuteInterval_PowerServiceException_raised_sequentially_over_maximum_trys_throws_error()
        {
            var minuteInterval = 30;
            var asAt = _testDateTime.AddMinutes(29);
            //confirm file was written            
            var maxTrys = 5;
            var csvWriter = new CsvWriter();
            DateTime? lastWroteCsv = null;

            IPowerService powerService = Mock.Create<IPowerService>();
            Mock.Arrange(() => powerService.GetTradesAsync(_testDateTime)).Throws<AggregateException>("Power Service exception");
            //first try five times no error should be thrown it will be logged and a retry allowed
            for (int i = 1; i <= maxTrys; i++)
            {
                Assert.DoesNotThrow(() => lastWroteCsv = csvWriter.WriteCsvMinuteInterval(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                    _dataDateFormat, minuteInterval, _testDateTime, powerService, _timeOut, lastWroteCsv));
            }
            //final time is over maximum number of retries so should throw an exception
            Assert.Throws<AggregateException>(() => lastWroteCsv = csvWriter.WriteCsvMinuteInterval(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                    _dataDateFormat, minuteInterval, _testDateTime, powerService, _timeOut, lastWroteCsv));
        }
        [Test]
        public void WriteCsvMinuteInterval_over_timeout_raises_WriteCsvException()
        {
            var csvWriter = new CsvWriter();
            var minuteInterval = 30;
            //confirm file was written            
            var maxTrys = 5;
            DateTime? lastWroteCsv = null;

            IPowerService powerService = Mock.Create<IPowerService>();
                        
            Mock.Arrange(() => powerService.GetTradesAsync(_testDateTime)).Returns(() => new Task<IEnumerable<PowerTrade>>(() =>
            {
                var waitTimepan = new TimeSpan(0, 0, _timeOut.Seconds + 2);
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while (stopwatch.ElapsedMilliseconds < waitTimepan.TotalMilliseconds)
                {
                    Thread.Sleep(0);
                }
                return _powerTrades;
            }));

            //first try five times no error should be thrown it will be logged and a retry allowed
            for (int i = 1; i <= maxTrys; i++)
            {
                Assert.DoesNotThrow(() => lastWroteCsv = csvWriter.WriteCsvMinuteInterval(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                    _dataDateFormat, minuteInterval, _testDateTime, powerService, _timeOut, lastWroteCsv));
            }
            //final time is over maximum number of retries so should throw an exception
            Assert.Throws<WriteCsvException>(() => lastWroteCsv = csvWriter.WriteCsvMinuteInterval(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                    _dataDateFormat, minuteInterval, _testDateTime, powerService, _timeOut, lastWroteCsv));
        }

        //Confirm that the logic for writing the historical csv set runs correctly
        public void WriteCsvMinuteIntervalHistorical_writes_correct_number_of_files()
        {
            var csvWriter = new CsvWriter();
            var minuteInterval = 1;
            var createFiles = 3;

            //First clear out the folder
            Array.ForEach(Directory.GetFiles(_fileFolder), File.Delete);
            var fromDate = new DateTime(2016,1,1,1,1,0);
            var toDate = fromDate.AddMinutes(2);

            csvWriter.WriteCsvMinuteIntervalHistorical(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                _dataDateFormat, minuteInterval, _powerService, _timeOut, fromDate, toDate, true);
          
            //Confirm the correct number of files has been written
            var writtenFiles = Directory.GetFiles(_fileFolder).Count();
            Assert.AreEqual(createFiles, writtenFiles);
        }

        //Confirm that the logic for writing the historical csv set runs correctly
        public void WriteCsvMinuteIntervalHistorical_does_not_overwrite_previous_files_when_false()
        {
            var csvWriter = new CsvWriter();
            var minuteInterval = 1;
            var createFiles = 3;

            var fromDate = new DateTime(2016, 1, 1, 1, 1, 0);
            var toDate = fromDate.AddMinutes(2);

            //Setup by writing clearing the folder and writing three new files
            Array.ForEach(Directory.GetFiles(_fileFolder), File.Delete);
            csvWriter.WriteCsvMinuteIntervalHistorical(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                _dataDateFormat, minuteInterval, _powerService, _timeOut, fromDate, toDate, false);

            var writtenFiles = Directory.GetFiles(_fileFolder).Count();
            Assert.AreEqual(createFiles, writtenFiles);

            var directory = new DirectoryInfo(_fileFolder);
            var files = directory.GetFiles();
            var originalQuery = from file in files select file.LastWriteTime;
            
            // rewrite the files and confirm the files were not overrwritten
            csvWriter.WriteCsvMinuteIntervalHistorical(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                _dataDateFormat, minuteInterval, _powerService, _timeOut, fromDate, toDate, false);

            writtenFiles = Directory.GetFiles(_fileFolder).Count();
            Assert.AreEqual(createFiles, writtenFiles);

            var modifiedFiles = directory.GetFiles();
            var modifiedQuery = from file in modifiedFiles select file.LastWriteTime;

            Assert.IsTrue(originalQuery.SequenceEqual(modifiedQuery));

        }
        
        public void WriteCsvMinuteIntervalHistorical_does_overwrite_previous_files_when_true()
        {
            var csvWriter = new CsvWriter();
            var minuteInterval = 1;
            var createFiles = 3;

            var fromDate = new DateTime(2016, 1, 1, 1, 1, 0);
            var toDate = fromDate.AddMinutes(2);

            //Setup by writing clearing the folder and writing three new files
            Array.ForEach(Directory.GetFiles(_fileFolder), File.Delete);
            csvWriter.WriteCsvMinuteIntervalHistorical(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                _dataDateFormat, minuteInterval, _powerService, _timeOut, fromDate, toDate, true);

            var writtenFiles = Directory.GetFiles(_fileFolder).Count();
            Assert.AreEqual(createFiles, writtenFiles);

            var directory = new DirectoryInfo(_fileFolder);
            var files = directory.GetFiles();
            var originalQuery = from file in files select file.LastWriteTime;
            
            // rewrite the files and confirm the files were not overrwritten
            csvWriter.WriteCsvMinuteIntervalHistorical(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                _dataDateFormat, minuteInterval, _powerService, _timeOut, fromDate, toDate, true);

            writtenFiles = Directory.GetFiles(_fileFolder).Count();
            Assert.AreEqual(createFiles, writtenFiles);

            var modifiedFiles = directory.GetFiles();
            var modifiedQuery = from file in modifiedFiles select file.LastWriteTime;

            Assert.IsFalse(originalQuery.SequenceEqual(modifiedQuery));

        }
        [Test]
        //Confirm that the logic for writing at the set minute interval is working correctly
        public void StartCsvMinuteInterval_writes_correct_number_of_files()
        {
            var csvWriter = new CsvWriter();
            var minuteInterval = 1;
            var createFiles = 3;          

            //First clear out the folder
            Array.ForEach(Directory.GetFiles(_fileFolder), File.Delete);
            csvWriter.StartCsvMinuteInterval(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                _dataDateFormat, minuteInterval, _powerService, _timeOut);
            var stopwatch = new Stopwatch ();
            stopwatch.Start();
            //One file is written on startup so must account for this by adjusting createFiles variable down by one
            while(stopwatch.ElapsedMilliseconds < minuteInterval * 60000 * (createFiles - 1))
            {
                // relenquish time slice
                Thread.Sleep(0);
            }      
            stopwatch.Stop();
            //Confirm the correct number of files has been written
            var writtenFiles = Directory.GetFiles(_fileFolder).Count();
            Assert.AreEqual(createFiles, writtenFiles);
        }

        [Test]        
        public void StartCsvMinuteInterval_writes_one_file_on_first_call()
        {
            var csvWriter = new CsvWriter();
            var minuteInterval = 60;
            var createFiles = 1;

            //First clear out the folder
            Array.ForEach(Directory.GetFiles(_fileFolder), File.Delete);
            csvWriter.StartCsvMinuteInterval(_fileFolder, _fileName, _fileDateFormat, _fileSuffix, _delimiter, _columnOneName, _columnTwoName,
                _dataDateFormat, minuteInterval, _powerService, _timeOut);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            //give a few seconds for timer to write the first file
            while (stopwatch.ElapsedMilliseconds < 5000)
            {
                // relenquish time slice
                Thread.Sleep(0);
            }
            stopwatch.Stop();
            //Confirm the correct number of files has been written
            var writtenFiles = Directory.GetFiles(_fileFolder).Count();
            Assert.AreEqual(createFiles, writtenFiles);
        }

        public void Scratch()
        {
            var stopwatch = new Stopwatch();
            var waitTimepan = new TimeSpan(0, 0, _timeOut.Seconds + 2);
            Console.WriteLine(waitTimepan.TotalMilliseconds);
            var tradesTask = new Task<IEnumerable<PowerTrade>>(() =>
            {
                stopwatch.Start();
                while (stopwatch.ElapsedMilliseconds < waitTimepan.TotalMilliseconds)
                {
                    Thread.Sleep(0);
                }
                return _powerTrades;
            });
            var stopwatchTwo = new Stopwatch();
            stopwatchTwo.Start();
            tradesTask.Start();
            tradesTask.Wait();
            stopwatchTwo.Stop();
            Console.WriteLine("Task time is " + stopwatchTwo.Elapsed.TotalMilliseconds);
            var trades = tradesTask.Result;
            Console.WriteLine(trades.ToString());
        }
       
        public void ScratchTwo()
        {
            var dateTimeString = "1-Sep-2016 21:05:00";
            var dateTime = Convert.ToDateTime(dateTimeString);
            Console.WriteLine(dateTime.ToLongTimeString());
            Console.WriteLine(dateTime.ToLongDateString());
        }
    }
}
