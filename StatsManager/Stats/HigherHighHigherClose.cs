using EODDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class HigherHighHigherClose : Stat
    {
        public Dictionary<Tuple<string, DateOnly>, double> HHHC = new Dictionary<Tuple<string, DateOnly>, double>();
        public HigherHighHigherClose(string symbol) : base(symbol) { OptInPeriod = 20; } //Default value
        public HigherHighHigherClose(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }
        public void Process()
        {
            Calculate();
            IdentifyPatterns();
            UpdateSummary();
        }
        public void Calculate()
        {
            //1. Prepare the input parameters
            List<double> inRealClose = GetInputPriceList(PriceType.ClosePrice);
            List<double> inRealHigh = GetInputPriceList(PriceType.HighPrice);

            int begIdx = 0;
            int endIdx = inRealClose.Count - 1;
            int outBegIdx = 0;
            int outNBElement = 0;
            double[] outReal = new double[endIdx - begIdx + 1];

            TA_hhhc(begIdx, endIdx, inRealHigh.ToArray(), inRealClose.ToArray(), OptInPeriod, out outBegIdx, out outNBElement, outReal);

            for (int i = 0; i < outNBElement; i++)
            {
                //Console.WriteLine( "DEBUG: EMA {0} {1} {2}", Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE), outReal[i]);
                HHHC.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outReal[i]);
            }
        }

        private void TA_hhhc(int startIdx, int endIdx, double[] inRealHigh, double[] inRealClose, int optInTimePeriod, out int outBegIdx, out int outNbElement, double[] outReal)
        {
            double[] outRealTmp = new double[endIdx + 1];
            
            for(var i = startIdx; i <= endIdx; i++) 
            {
                if (i + 1 <= endIdx)
                {
                    if ( inRealHigh[i] < inRealHigh[i + 1] && inRealClose[i] < inRealClose[i + 1] )
                    {
                        outReal[i + 1] = 1;
                    }
                    else
                    {
                        outReal[i + 1] = -1;
                    }
                }
            }

            outBegIdx = 0;
            outNbElement = endIdx+1;
        }

        private void IdentifyPatterns()
        {
            //1. List of all EMA values
            foreach (KeyValuePair<Tuple<string, DateOnly>, double> item in HHHC)
            {
                //2. Get current equity's history
                Equity currentEquity = History.HISTORY[item.Key];

                //Cross occures
                //if (Utilities.PriceInRange((int)currentEquity.OPENPRICE, (int)currentEquity.CLOSEPRICE, (int)item.Value))
                {
                    //Bull cross
                    //if (currentEquity.OPENPRICE < currentEquity.CLOSEPRICE)
                    if( item.Value == 1 )
                    {
                        //Check if the trend was rally bullish after 6 days [HARD CODED to 6]
                        summary.BullOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                    }
                    //Bear cross
                    else if ( item.Value == -1 )
                    {
                        summary.BearOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                    }
                }
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.HHHC;
            base.UpdateSummary();
        }
    }
}
