using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace TCrossCuttingConcerns
{    
    public class TNlog
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        
        public void LogError()
        {
            var innerException = new ApplicationException("Inner exception message");
            var testException = new ApplicationException("Test exception message", innerException);
            if (!System.Diagnostics.EventLog.SourceExists("CrossCuttingConcerns"))
            {
                System.Diagnostics.EventLog.CreateEventSource("CrosscuttingConcerns", "Application");
            }
            _logger.Error(testException);
            _logger.Info("Test Info");
        }
    }
}
