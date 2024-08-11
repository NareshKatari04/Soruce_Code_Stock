using EODDownloader;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class WilliamsR : Stat
    {
        //Static data
        public static Dictionary<Tuple<string, DateOnly>, double> WILLIAMSR = new Dictionary<Tuple<string, DateOnly>, double>();

        //Constructors
        public WilliamsR(string symbol) : base(symbol)
        {
            OptInPeriod = 14;
            TypeOfPrice = PriceType.ClosePrice;
        }
        public WilliamsR(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }

        //Methods
        public void Process()
        {
            Calculate();
            IdentifyPatterns();
            UpdateSummary();
        }
        private void Calculate()
        {
            //1. Prepare the input parameters
            List<double> inRealClose = GetInputPriceList(TypeOfPrice);
            List<double> inRealHigh = GetInputPriceList(PriceType.HighPrice);
            List<double> inRealLow = GetInputPriceList(PriceType.LowPrice);

            int begIdx = 0;
            int endIdx = inRealClose.Count - 1;
            int outBegIdx = 0;
            int outNBElement = 0;
            double[] outReal = new double[endIdx - begIdx + 1];

            Core.WillR(begIdx, endIdx, inRealHigh.ToArray(), inRealLow.ToArray(), inRealClose.ToArray(), OptInPeriod, out outBegIdx, out outNBElement, outReal);
            
            for (int i = 0; i < outNBElement; i++)
            {
                WILLIAMSR.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outReal[i]);
            }
        }
        private void IdentifyPatterns()
        {
            int overSoldPoint = -100;
            int overSoldPointPost = -85;
            int overBoughtPoint = 0;
            int overBoughtPointPost = -15;

            //1. For each WillR% captured
            foreach (var item in WILLIAMSR)
            {
                //2. WillR% touches overSoldPoint
                if (item.Value == overSoldPoint)
                {
                    //3. Look for the next 5th day's WillsR%
                    DateOnly futureDateToBuy = Utilities.GetNthTradedDate(Symbol, item.Key.Item2, 5);

                    //4. if 5th day's WillsR% is above -85 or -95
                    if (
                        History.HISTORY.ContainsKey(new Tuple<string, DateOnly>(Symbol, futureDateToBuy)) &&
                        WILLIAMSR.ContainsKey(new Tuple<string, DateOnly>(Symbol, futureDateToBuy)) &&
                        WILLIAMSR[new Tuple<string, DateOnly>(Symbol, futureDateToBuy)] >= overSoldPointPost
                        )
                    {
                        //5. Possible bull trend
                        summary.BullOccurances.TryAdd(futureDateToBuy, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                        //if (futureDateToBuy >= DateOnly.FromDateTime(DateTime.Now).AddDays(-15))
                            //Console.WriteLine("Symbol:[{0}]Date:[{1}] Trend:[{2}]", Symbol, futureDateToBuy, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                    }
                }
                else if (item.Value == overBoughtPoint )
                {
                    DateOnly futureDateToSsell = Utilities.GetNthTradedDate(Symbol, item.Key.Item2, 5);
                    if (
                        History.HISTORY.ContainsKey(new Tuple<string, DateOnly>(Symbol, futureDateToSsell)) &&
                        WILLIAMSR.ContainsKey(new Tuple<string, DateOnly>(Symbol, futureDateToSsell)) &&
                        WILLIAMSR[new Tuple<string, DateOnly>(Symbol, futureDateToSsell)] <= overBoughtPointPost
                        )
                        summary.BearOccurances.TryAdd(futureDateToSsell, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                }
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.WILLIAMSR;
            base.UpdateSummary();
        }
    }
}
