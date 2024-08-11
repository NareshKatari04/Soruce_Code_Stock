using EODDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class BBands : Stat
    {
        public Dictionary<Tuple<string, DateOnly>, double> BBANDS_UPPER = new Dictionary<Tuple<string, DateOnly>, double>();
        public Dictionary<Tuple<string, DateOnly>, double> BBANDS_MIDDLE = new Dictionary<Tuple<string, DateOnly>, double>();
        public Dictionary<Tuple<string, DateOnly>, double> BBANDS_LOWER = new Dictionary<Tuple<string, DateOnly>, double>();
        public BBands(string symbol) : base(symbol) { OptInPeriod = 30; } //Default value
        public BBands(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }
        public void Process()
        {
            Calculate();
            IdentifyPatterns();
            UpdateSummary();
        }
        public void Calculate()
        {
            //1. Prepare the input parameters
            List<double> inReal = GetInputPriceList(TypeOfPrice);

            int begIdx = 0;
            int endIdx = inReal.Count - 1;
            int outBegIdx = 0;
            int outNBElement = 0;
            double[] outRealUpperBand = new double[endIdx - begIdx + 1];
            double[] outRealMiddleBand = new double[endIdx - begIdx + 1];
            double[] outRealLowerBand = new double[endIdx - begIdx + 1];

            Core.Bbands(begIdx, endIdx, inReal.ToArray(), OptInPeriod, 2, 2, Core.MAType.Sma, out outBegIdx, out outNBElement, outRealUpperBand, outRealMiddleBand, outRealLowerBand);
            
            for (int i = 0; i < outNBElement; i++)
            {
                //Console.WriteLine( "DEBUG: EMA {0} {1} {2}", Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE), outReal[i]);
                BBANDS_UPPER.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outRealUpperBand[i]);
                BBANDS_MIDDLE.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outRealMiddleBand[i]);
                BBANDS_LOWER.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outRealLowerBand[i]);
            }
        }
        private void IdentifyPatterns()
        {
            //1. List of all EMA values
            foreach (KeyValuePair<Tuple<string, DateOnly>, double> item in BBANDS_MIDDLE)
            {
                //2. Get current equity's history
                Equity currentEquity = History.HISTORY[item.Key];

                //1. If the price is below /or/ on the lower band - BULLISH
                if (BBANDS_LOWER[item.Key] >= currentEquity.HIGHPRICE)
                {
                    summary.BullOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                    //Console.WriteLine("DEBUG: Bullish {0} {1} {2}", Symbol, BBANDS_LOWER[item.Key], item.Key.Item2);
                }
                //2. If the price is above /or/ on the upper band - BEARISH
                else if(BBANDS_UPPER[item.Key] <= currentEquity.LOWPRICE)
                {
                    summary.BearOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                    //Console.WriteLine("DEBUG: Bearish {0} {1} {2}", Symbol, BBANDS_LOWER[item.Key], item.Key.Item2);
                }
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.BBANDS;
            base.UpdateSummary();
        }
    }
}
