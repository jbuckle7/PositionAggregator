using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.JustMock;
using Services;
using NUnit.Framework;
using Domain;
using Utility;

namespace TDomain
{
    [TestFixture]
    public class TPosition
    {
        IPowerService _powerService;
        readonly DateTime _testDateTime = new DateTime(2016, 8, 31, 1, 1, 1);

        [OneTimeSetUp]
        public void Setup()
        {
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

            _powerService = Mock.Create<IPowerService>();
            Mock.Arrange(() => _powerService.GetTrades(_testDateTime)).Returns(new PowerTrade[] { tradeOne, tradeTwo, tradeThree });

        }

        [Test]
        public void Property_HourlyPositions_constructs_from_powertrades_correctly()
        {
            var trades = _powerService.GetTrades(_testDateTime);
            var position = new Position(trades, _testDateTime);
            var expectedForDate = _testDateTime;
            var expectedHourlyPosition = new Dictionary<int, HourlyPosition>();
            
            expectedHourlyPosition.Add(1, new HourlyPosition(1, 1000 * 3));
            expectedHourlyPosition.Add(2, new HourlyPosition(2, 2000 * 3));
            expectedHourlyPosition.Add(3, new HourlyPosition(3, 3000 * 3));
            expectedHourlyPosition.Add(4, new HourlyPosition(4, 4000 * 3));
            expectedHourlyPosition.Add(5, new HourlyPosition(5, 5000 * 3));
            expectedHourlyPosition.Add(6, new HourlyPosition(6, 6000 * 3));
            expectedHourlyPosition.Add(7, new HourlyPosition(7, 7000 * 3));
            expectedHourlyPosition.Add(8, new HourlyPosition(8, 8000 * 3));
            expectedHourlyPosition.Add(9, new HourlyPosition(9, 9000 * 3));
            expectedHourlyPosition.Add(10, new HourlyPosition(10, 10000 * 3));
            expectedHourlyPosition.Add(11, new HourlyPosition(11, 11000 * 3));
            expectedHourlyPosition.Add(12, new HourlyPosition(12, 12000 * 3));
            expectedHourlyPosition.Add(13, new HourlyPosition(13, 13000 * 3));
            expectedHourlyPosition.Add(14, new HourlyPosition(14, 14000 * 3));
            expectedHourlyPosition.Add(15, new HourlyPosition(15, 15000 * 3));
            expectedHourlyPosition.Add(16, new HourlyPosition(16, 16000 * 3));
            expectedHourlyPosition.Add(17, new HourlyPosition(17, 17000 * 3));
            expectedHourlyPosition.Add(18, new HourlyPosition(18, 18000 * 3));
            expectedHourlyPosition.Add(19, new HourlyPosition(19, 19000 * 3));
            expectedHourlyPosition.Add(20, new HourlyPosition(20, 20000 * 3));
            expectedHourlyPosition.Add(21, new HourlyPosition(21, 21000 * 3));
            expectedHourlyPosition.Add(22, new HourlyPosition(22, 22000 * 3));
            expectedHourlyPosition.Add(23, new HourlyPosition(23, 23000 * 3));
            expectedHourlyPosition.Add(24, new HourlyPosition(24, 24000 * 3));

            var actualHourlyPosition = position.HourlyPositions;
            Assert.IsTrue(expectedHourlyPosition.ContentEquals(actualHourlyPosition));                                      
        }
        
        [Test]
        public void Property_ForDate_constructs_correctly()
        {
            var trades = _powerService.GetTrades(_testDateTime);
            var position = new Position(trades, _testDateTime);
            var expectedForDate = _testDateTime;
            var actualForDate = position.ForDate;
            Assert.AreEqual(expectedForDate, actualForDate);
        }        
    }
}
