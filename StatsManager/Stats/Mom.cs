using EODDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class Mom : Stat
    {
        public Dictionary<Tuple<string, DateOnly>, double> MOM = new Dictionary<Tuple<string, DateOnly>, double>();
        public Mom(string symbol) : base(symbol) { OptInPeriod = 10; } //Default value
        public Mom(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }
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

            Core.Mom(begIdx, endIdx, inReal.ToArray(), OptInPeriod, out outBegIdx, out outNBElement, outReal);

            for (int i = 0; i < outNBElement; i++)
            {
                //Console.WriteLine( "DEBUG: EMA {0} {1} {2}", Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE), outReal[i]);
                MOM.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outReal[i]);
            }
        }
        private void IdentifyPatterns()
        {
            int crossFormed = 0;
            //1. List of all EMA values
            foreach (KeyValuePair<Tuple<string, DateOnly>, double> item in MOM)
            {
                //2. Get current equity's history
                Equity currentEquity = History.HISTORY[item.Key];

                if (item.Value > 0 && item.Value < 1)
                {
                    summary.BullOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                }
                else if (item.Value < 0 && item.Value > -1)
                {
                    summary.BearOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                }
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.MOM;
            base.UpdateSummary();
        }
    }
}
