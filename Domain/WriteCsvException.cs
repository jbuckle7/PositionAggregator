using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    [Serializable]
    internal sealed class WriteCsvException : Exception
    {
        public WriteCsvException()
        {

        }

        public WriteCsvException(string message) : base(message)
        {
        }

        public WriteCsvException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
