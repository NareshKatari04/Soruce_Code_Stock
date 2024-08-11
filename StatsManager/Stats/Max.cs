using EODDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class Max : Stat
    {
        public Dictionary<Tuple<string, DateOnly>, double> MAX = new Dictionary<Tuple<string, DateOnly>, double>();
        public Max(string symbol) : base(symbol) { OptInPeriod = 90; } //Default value
        public Max(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }
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
            double[] outReal = new double[endIdx - begIdx + 1];

            Core.Max(begIdx, endIdx, inReal.ToArray(), OptInPeriod, out outBegIdx, out outNBElement, outReal);

            for (int i = 0; i < outNBElement; i++)
            {
                //Console.WriteLine( "DEBUG: EMA {0} {1} {2}", Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE), outReal[i]);
                MAX.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outReal[i]);
            }
        }
        private void IdentifyPatterns()
        {
            //1. List of all EMA values
            foreach (KeyValuePair<Tuple<string, DateOnly>, double> item in MAX)
            {
                //2. Get current equity's history
                Equity currentEquity = History.HISTORY[item.Key];

                //Cross occures
                if (Utilities.PriceInRange((int)currentEquity.OPENPRICE, (int)currentEquity.CLOSEPRICE, (int)item.Value))
                {
                    //Bull cross
                    if (currentEquity.OPENPRICE < currentEquity.CLOSEPRICE)
                    {
                        //Check if the trend was rally bullish after 6 days [HARD CODED to 6]
                        summary.BullOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                    }
                    //Bear cross
                    else if (currentEquity.OPENPRICE > currentEquity.CLOSEPRICE)
                    {
                        summary.BearOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                    }
                }
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.MAX;
            base.UpdateSummary();
        }
    }
}
