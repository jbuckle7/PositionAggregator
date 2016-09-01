using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services;
using NUnit.Framework;
using Newtonsoft.Json;

namespace TPowerService
{
    //test the Power Service to confirm it is retrieving the correct trades
    [TestFixture]
    public class TPowerService
    {
        DateTime _tradesDate = new DateTime(2016, 8, 26);
        //Assuming the powerservice should return consistent trades per queried date the below method returns the trades for the 8th of August 2016      
        //PowerTrade double has same Json Layout as Powertrade so can use Json rather than Mock objects for brevity. 
        private List<PowerTradeDouble> GetTestTrades()
        {
            var testTradesText = @"[{ ""Date"":""2016-08-26T00: 00:00"",""Periods"":[{""Period"":1,""Volume"":714.96528280664484},{""Period"":2,""Volume"":485.72779096929719},{""Period"":3,""Volume"":419.37258114077736},{""Period"":4,""Volume"":887.06266781644081},{""Period"":5,""Volume"":231.83281870178544},{""Period"":6,""Volume"":640.35029599459392},{""Period"":7,""Volume"":553.6636093415616},{""Period"":8,""Volume"":469.24158673232495},{""Period"":9,""Volume"":556.447121108159},{""Period"":10,""Volume"":494.14869094926337},{""Period"":11,""Volume"":102.30647218521055},{""Period"":12,""Volume"":138.9309201105176},{""Period"":13,""Volume"":657.57628048657273},{""Period"":14,""Volume"":253.22991435100784},{""Period"":15,""Volume"":150.10957007767149},{""Period"":16,""Volume"":555.49549989192542},{""Period"":17,""Volume"":801.16261439452069},{""Period"":18,""Volume"":332.50705727027122},{""Period"":19,""Volume"":34.160674565546529},{""Period"":20,""Volume"":869.23375812789141},{""Period"":21,""Volume"":735.78836011504211},{""Period"":22,""Volume"":917.8869174410994},{""Period"":23,""Volume"":130.69040241217724},{""Period"":24,""Volume"":46.672559830673293}]},
                                    {""Date"":""2016-08-26T00:00:00"",""Periods"":[{""Period"":1,""Volume"":142.19456312348814},{""Period"":2,""Volume"":239.73087837907062},{""Period"":3,""Volume"":956.09774205651968},{""Period"":4,""Volume"":458.995552947277},{""Period"":5,""Volume"":915.14304602292509},{""Period"":6,""Volume"":502.85064266196014},{""Period"":7,""Volume"":384.33739840255},{""Period"":8,""Volume"":944.23851042251965},{""Period"":9,""Volume"":709.78514231265763},{""Period"":10,""Volume"":664.58808521953779},{""Period"":11,""Volume"":126.4783177182443},{""Period"":12,""Volume"":470.82246861924534},{""Period"":13,""Volume"":872.344269357782},{""Period"":14,""Volume"":852.83344790937076},{""Period"":15,""Volume"":9.123263884858817},{""Period"":16,""Volume"":349.10930150612688},{""Period"":17,""Volume"":364.80489064231739},{""Period"":18,""Volume"":991.282667029315},{""Period"":19,""Volume"":245.72369607431989},{""Period"":20,""Volume"":929.28066986113822},{""Period"":21,""Volume"":925.223890191514},{""Period"":22,""Volume"":784.7561956312303},{""Period"":23,""Volume"":92.901898591267823},{""Period"":24,""Volume"":982.77401550802119}]},
                                    {""Date"":""2016-08-26T00:00:00"",""Periods"":[{""Period"":1,""Volume"":163.83504875182874},{""Period"":2,""Volume"":95.598227388969718},{""Period"":3,""Volume"":681.3029626762974},{""Period"":4,""Volume"":922.35510327031614},{""Period"":5,""Volume"":220.50827006833083},{""Period"":6,""Volume"":860.68686743298861},{""Period"":7,""Volume"":94.114326915756948},{""Period"":8,""Volume"":294.60129062393742},{""Period"":9,""Volume"":359.80986727392758},{""Period"":10,""Volume"":763.416045235198},{""Period"":11,""Volume"":791.66470085813876},{""Period"":12,""Volume"":173.83571023765751},{""Period"":13,""Volume"":212.64448771842032},{""Period"":14,""Volume"":868.528869873159},{""Period"":15,""Volume"":209.09361597574019},{""Period"":16,""Volume"":816.93549212856942},{""Period"":17,""Volume"":228.28582312366265},{""Period"":18,""Volume"":141.21402806658949},{""Period"":19,""Volume"":197.95605270096849},{""Period"":20,""Volume"":529.945736066413},{""Period"":21,""Volume"":291.50965683698172},{""Period"":22,""Volume"":332.51723522903268},{""Period"":23,""Volume"":444.32057740367975},{""Period"":24,""Volume"":630.50637330417817}]}]";
            var testTrades = JsonConvert.DeserializeObject<List<PowerTradeDouble>>(testTradesText);         
            return testTrades;
        }
                   
        //Confirm the same trades are being returned consistently for a given date and the GetTrades method runs without error
        // Currently this test is failing as trades are randomly generated
        [Test]
        public void GetTrades_method__returns_same_trades_for_same_query_datetime()
        {
            
            var powerService = new PowerService();
            var currentTrades = powerService.GetTrades(_tradesDate);
            var matchTrades = GetTestTrades();           
            var matchTradeIndex = 0;
            var currentTradecount = 0;
            //Assume ordering is always the same then trades queried for same date should be the exact same and in the same order
            foreach(PowerTrade currentTrade in currentTrades)
            {
                var currentTradeText = JsonConvert.SerializeObject(currentTrade);
                var matchTradeText = JsonConvert.SerializeObject(matchTrades[matchTradeIndex]);
                Assert.AreEqual(currentTradeText, matchTradeText);
                matchTradeIndex++;
                currentTradecount++;
            }
            //confirm count is the same
            Assert.AreEqual(currentTradecount, matchTrades.Count);            
        }

        //Confirm the same trades are being returned consistently for a given date and the GetTrades method runs without error
        // Currently this test is failing as trades are randomly generated
        [Test]
        public void GetTradesAsync_method__returns_same_trades_for_same_query_datetime()
        {
            var powerService = new PowerService();
            var currentTradesTask = powerService.GetTradesAsync(_tradesDate);
            var currentTrades = currentTradesTask.Result;
            var matchTrades = GetTestTrades();
            var matchTradeIndex = 0;
            var currentTradecount = 0;
            //Assume ordering is always the same then trades queried for same date should be the exact same and in the same order
            foreach (PowerTrade currentTrade in currentTrades)
            {
                var currentTradeText = JsonConvert.SerializeObject(currentTrade);
                var matchTradeText = JsonConvert.SerializeObject(matchTrades[matchTradeIndex]);
                Assert.AreEqual(currentTradeText, matchTradeText);
                matchTradeIndex++;
                currentTradecount++;
            }
            //confirm count is the same in case of zero trades returned
            Assert.AreEqual(currentTradecount, matchTrades.Count);
        }

        //Confirm the trades are being returned within performance parameters.  Sixty seconds seems reasonable.
        [Test]
        public void GetTrades_method_returns_data_in_under_a_minute()
        {          
            var powerService = new PowerService();
            var maxRunTime = new TimeSpan(0, 0, 60);
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            var currentTrades = powerService.GetTrades(_tradesDate);
            stopWatch.Stop();
            var runTime = stopWatch.Elapsed;
            Assert.GreaterOrEqual(maxRunTime, runTime);            
        }

        //Confirm the trades are being returned within performance parameters.  Sixty seconds seems reasonable.
        [Test]
        public void GetTradesAsync_method_returns_data_in_under_a_minute()
        {
            var powerService = new PowerService();
            var maxRunTime = new TimeSpan(0, 0, 60);
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            var currentTrades = (powerService.GetTradesAsync(_tradesDate)).Result;
            stopWatch.Stop();
            var runTime = stopWatch.Elapsed;
            Assert.GreaterOrEqual(maxRunTime, runTime);
        }

        //I don't know if the service will always work correctly with the power trade going forward but I can confirm it runs with historical data
        // I'll assume the data has been fed in over the last 180 days
        // Currently failing as random error being thrown deliberately in PowerService dll
        [Test]
        public void Gettrades_method_returns_one_hundred_and_eighty_days_of_historical_data()
        {
            var powerService = new PowerService();
            for (int i = 0; i < 180; i++)
            {                
                var checkDate = _tradesDate.AddDays(i*-1);
                var currentTrades = powerService.GetTrades(_tradesDate);
            }            
        }

        //PowerPeriod is Immutable outside it's containing dll use this class as test double
        private class PowerPeriodDouble
        {
            public int Period { get; set; }
            public double Volume { get; set; }
        }
        //PowerTrade is Immutable outside it's containing dll use this class as test double
        private class PowerTradeDouble
        {                        
            public DateTime Date { get; set; }
            public IList<PowerPeriodDouble> Periods { get; set; }
        } 
    }  
}
