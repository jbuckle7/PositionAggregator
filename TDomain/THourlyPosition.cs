using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using NUnit.Framework;

namespace TDomain
{

    [TestFixture]
    public class THourlyPosition
    {

        [Test]
        //Confirm that the object constructs correctly
        public void HourlyPosition_contructs_correctly()
        {
            var hourlyPosition = new HourlyPosition(1, 1000);
            Assert.AreEqual(1, hourlyPosition.Hour);
            Assert.AreEqual(1000, hourlyPosition.Amount);
        }

        [Test]
        //Confirm that the set hour is within the range one to twenty four
        public void Exception_raised_when_range_set_outside_twenty_four_hours()
        {
            HourlyPosition hourlyPosition;
            Assert.Throws<ApplicationException>(() => hourlyPosition = new HourlyPosition(25, 1000)); 
        }
    }
}
