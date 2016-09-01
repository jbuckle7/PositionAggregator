﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class ExceptionExtensions
    {
        public static string GetExceptionMessages(this Exception e, string msgs = "")
        {
            if (e == null) return string.Empty;
            if (msgs == "") msgs = e.Message;
            if (e.InnerException != null)
                msgs += "\r\nInnerException: " + GetExceptionMessages(e.InnerException);
            return msgs;
        }
    }
}
