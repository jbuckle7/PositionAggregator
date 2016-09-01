using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Domain;
using Services;

namespace PositionAggregator
{
    public partial class PositionAggregator : ServiceBase
    {
        private readonly CsvWriter _csvWriter = new CsvWriter();

        public PositionAggregator()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {            
            var fileFolder = ConfigurationManager.AppSettings["fileFolder"];
            var fileName = ConfigurationManager.AppSettings["fileName"];
            var fileDateFormat = ConfigurationManager.AppSettings["fileDateFormat"]; 
            var fileSuffix = ConfigurationManager.AppSettings["fileSuffix"];
            var delimiter = ConfigurationManager.AppSettings["delimiter"];
            var columnOneName = ConfigurationManager.AppSettings["columnOneName"];
            var columnTwoName = ConfigurationManager.AppSettings["columnTwoName"];
            var dataDateFormat = ConfigurationManager.AppSettings["dataDateFormat"];
            var minuteInterval = Convert.ToInt32(ConfigurationManager.AppSettings["minuteInterval"]);
            var timeoutSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["powerServiceTimeOutSeconds"]);
            var timeout = new TimeSpan(0,0,timeoutSeconds);
            var powerService = new PowerService();


            _csvWriter.StartCsvMinuteInterval(fileFolder, fileName, fileDateFormat, fileSuffix, delimiter, columnOneName, columnTwoName, dataDateFormat,
                minuteInterval, powerService, timeout);
        }

        protected override void OnStop()
        {
        }
    }
}
