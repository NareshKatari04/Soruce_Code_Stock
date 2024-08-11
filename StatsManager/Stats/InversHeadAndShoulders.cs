using EODDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StatsManager
{
    public class InversHeadAndShoulders : Stat
    {
        public Dictionary<Tuple<string, DateOnly>, double> INVHEADSHOULDERS = new Dictionary<Tuple<string, DateOnly>, double>();
        public InversHeadAndShoulders(string symbol) : base(symbol) { OptInPeriod = 4; } //Default value
        public InversHeadAndShoulders(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }
        public void Process()
        {
            Calculate();
            IdentifyPatterns();
            UpdateSummary();
        }
        public void Calculate()
        {
            //1. Prepare the input parameters
            List<double> inRealOpen = GetInputPriceList(PriceType.OpenPrice);
            List<double> inRealHigh = GetInputPriceList(PriceType.HighPrice);
            List<double> inRealLow = GetInputPriceList(PriceType.LowPrice);
            List<double> inRealClose = GetInputPriceList(PriceType.ClosePrice);
            List<double> inRealVol = GetInputPriceList(PriceType.Volume);

            int begIdx = 0;
            int endIdx = inRealClose.Count - 1;
            int outBegIdx = 0;
            int outNBElement = 0;
            double[] outReal = new double[endIdx - begIdx + 1];

            TA_INVHEADSHOULDERS();
        }

        private void TA_INVHEADSHOULDERS()
        {
            Trend trend = new Trend(Symbol);
            trend.Process();

            Dictionary<Tuple<string,DateOnly>,double> dct = new ( trend.TREND.Where<KeyValuePair<Tuple<string, DateOnly>, double>>(x => (x.Value == 100 || x.Value == -100) ));

            for(int i=0; i< dct.Count; i++) 
            {
                if (dct.ElementAt(i).Value == -100 &&                            
                    i + 1 < dct.Count && dct.ElementAt(i+1).Value == 100 &&
                    i + 2 < dct.Count && dct.ElementAt(i+2).Value == -100 &&
                    i + 3 < dct.Count && dct.ElementAt(i+3).Value == 100 &&
                    History.HISTORY[dct.ElementAt(i+3).Key].HIGHPRICE < History.HISTORY[dct.ElementAt(i+1).Key].LOWPRICE &&
                    i + 4 < dct.Count && dct.ElementAt(i+4).Value == -100 &&
                    i + 5 < dct.Count &&
                    History.HISTORY[dct.ElementAt(i+5).Key].HIGHPRICE < History.HISTORY[dct.ElementAt(i+3).Key].LOWPRICE
                    )
                {
                    Console.WriteLine("DEBUG: {0}", dct.ElementAt(i).Key);
                    INVHEADSHOULDERS.Add(dct.ElementAt(i + 5).Key, 100);
                }
            }
        }
        private void IdentifyPatterns()
        {
            //1. List of all EMA values
            foreach (KeyValuePair<Tuple<string, DateOnly>, double> item in INVHEADSHOULDERS)
            {
                //2. Get current equity's history
                //Equity currentEquity = History.HISTORY[item.Key];
                {
                    //Bull cross
                    if (item.Value == 100)
                    {
                        //Check if the trend was rally bullish after 6 days [HARD CODED to 6]
                        summary.BullOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));

                    }
                    //Bear cross
                    else if (item.Value == -100)
                    {
                        summary.BearOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                    }
                }
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.INV_HEAD_SHOULDERS;
            base.UpdateSummary();
        }
    }
}
