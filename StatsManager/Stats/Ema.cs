using EODDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class Ema : Stat
    {
        public Dictionary<Tuple<string, DateOnly>, double> EMA = new Dictionary<Tuple<string, DateOnly>, double>();
        public Ema(string symbol) : base(symbol) { OptInPeriod = 20; } //Default value
        public Ema(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }
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

            Core.Ema(begIdx, endIdx, inReal.ToArray(), OptInPeriod, out outBegIdx, out outNBElement, outReal);

            for (int i = 0; i < outNBElement; i++)
            {
                //Console.WriteLine( "DEBUG: EMA {0} {1} {2}", Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE), outReal[i]);
                EMA.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outReal[i]);
            }
        }
        private void IdentifyPatterns()
        {
            int crossFormed = 0;
            //1. List of all EMA values
            foreach (KeyValuePair<Tuple<string, DateOnly>, double> item in EMA)
            {
                //2. Get current equity's history
                Equity currentEquity = History.HISTORY[item.Key];

                if (crossFormed > 0)
                {
                    //a) Cross happend again
                    if (Utilities.PriceInRange((int)currentEquity.LOWPRICE, (int)currentEquity.HIGHPRICE, (int)item.Value))
                    {
                        crossFormed++;
                    }
                    //b) Crosses above - Bullish
                    else if (item.Value < currentEquity.LOWPRICE)
                    {
                        //Console.WriteLine("DEBUG: BULL {0} {1} {2} ", item.Key.Item2, item.Value, currentEquity.CLOSEPRICE );
                        summary.BullOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                        //Reset the cross happened
                        crossFormed = 0;
                    }
                    //c) Crosses below - Bearish
                    else if (item.Value > currentEquity.HIGHPRICE)
                    {
                        //Console.WriteLine("DEBUG: BEAR {0} {1} {2} ", item.Key.Item2, item.Value, currentEquity.CLOSEPRICE);
                        summary.BearOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                        //Reset the cross happened
                        crossFormed = 0;
                    }
                }
                else
                {
                    //a) Check its a new cross
                    if ((Utilities.PriceInRange((int)currentEquity.LOWPRICE, (int)currentEquity.HIGHPRICE, (int)item.Value)))
                    {
                        crossFormed++;
                    }
                    //b) No cross just continue to look for new cross points
                    else
                    {
                        continue;
                    }
                }
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.EMA;
            base.UpdateSummary();
        }
    }
}
