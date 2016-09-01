using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services;

namespace Domain
{
    internal sealed class Position
    {
        private readonly IEnumerable<PowerTrade> _powerTrades;
        private readonly DateTime _forDate;

        private Dictionary<int, HourlyPosition> _hourlyPositions;
            
        public DateTime ForDate { get { return _forDate; } }
        public Dictionary<int, HourlyPosition> HourlyPositions
        {
            get
            {
                if (_hourlyPositions == null)
                {
                    _hourlyPositions = GetPosition(_powerTrades);
                }
                return _hourlyPositions;
            }
        }

        public Position(IEnumerable<PowerTrade>  powertrades, DateTime forDate)
        {
            _powerTrades = powertrades;
            _forDate = forDate;
        }

        //Take the list of powertrades and aggregate the volume into the hourly positions
        private Dictionary<int, HourlyPosition> GetPosition(IEnumerable<PowerTrade> powerTrades )
        {
            var hourlyPositions = new Dictionary<int, HourlyPosition>();
            //first initialize the dictionary to 0
            for (int i = 1; i <= 24; i++)
            {
                var addHourlyPosition = new HourlyPosition(i, 0);
                hourlyPositions.Add(addHourlyPosition.Hour, addHourlyPosition);
            }
            //now cycle through the powertrades and update the positions by hour
            foreach (PowerTrade trade in powerTrades)
            {
                foreach (PowerPeriod period in trade.Periods)
                {
                    var setPosition = hourlyPositions[period.Period];
                    setPosition.Amount += Convert.ToDecimal(period.Volume);
                }                  
            }
            return hourlyPositions;
        }
    }
}
