using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace PositionAggregator
{
    static class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionEvent);                
            StartService();    
        }

        static void StartService()
        {            
            var servicesToRun = new ServiceBase[]
            {
                new PositionAggregator()
            };
            ServiceBase.Run(servicesToRun);            
        }

        static void UnhandledExceptionEvent(object sender, UnhandledExceptionEventArgs args)
        {                        
            _logger.Error(args.ExceptionObject);            
        }
    }
}
