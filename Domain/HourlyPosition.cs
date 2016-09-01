using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Domain
{
    internal sealed class HourlyPosition
    {
        private int _hour;
        private decimal _amount;

        public int Hour{ get { return _hour; } set { Contract.Requires(value >= 1 && value <= 24); _hour = value; } }
        public decimal Amount { get { return _amount; } set { _amount = value; } }

        public HourlyPosition(int hour, decimal amount)
        {
            Contract.Requires<ApplicationException>(hour >= 1 && hour <= 24, "Hour outside of 24 Hours");
            _hour = hour;
            _amount = amount;
        }

        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            HourlyPosition p = (HourlyPosition)obj;
            return (_hour == p.Hour) && (_amount == p.Amount);
        }

        public override int GetHashCode()
        {            
            //should minimize collisions for cases where amount is 0
            return Convert.ToInt32(_hour  + _amount);
        }
    }
}
