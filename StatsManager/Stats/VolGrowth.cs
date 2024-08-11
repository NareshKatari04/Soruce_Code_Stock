using EODDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class VolGrowth : Stat
    {
        public Dictionary<Tuple<string, DateOnly>, double> VOLGROWTH = new Dictionary<Tuple<string, DateOnly>, double>();
        public VolGrowth(string symbol) : base(symbol) { OptInPeriod = 10; } //Default value
        public VolGrowth(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }
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

            //TODO
            TA_VOLGROWTH(begIdx,
                endIdx,
                inRealOpen.ToArray(),
                inRealHigh.ToArray(),
                inRealLow.ToArray(),
                inRealClose.ToArray(),
                inRealVol.ToArray(),
                out outBegIdx,
                out outNBElement,
                outReal);

            for (int i = 0; i < outNBElement; i++)
            {
                //if(outReal[i] == 100 || outReal[i] == -100 )
                    //Console.WriteLine( "DEBUG: EMA {0} {1} {2}", Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE), outReal[i]);
                VOLGROWTH.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outReal[i]);
            }
        }

        private void TA_VOLGROWTH(int begIdx,
            int endIdx,
            double[] openPrice,
            double[] highPrice,
            double[] lowPrice,
            double[] closePrice,
            double[] totalVol,
            out int outBegIdx,
            out int outNBElement,
            double[] outReal)
        {
            outBegIdx = 0;
            outNBElement = 0;
            int itr = 0;
                       
            double volumei = totalVol[begIdx];
            double volumej = totalVol[begIdx+1];
            double volumek = totalVol[begIdx+2];

            for (itr = 2; itr <= endIdx; itr++)
            {
                if (
                    Utilities.isGreenCandle(openPrice[itr - 2], highPrice[itr - 2], lowPrice[itr - 2], closePrice[itr - 2]) &&
                    Utilities.isGreenCandle(openPrice[itr - 1], highPrice[itr - 1], lowPrice[itr - 1], closePrice[itr - 1]) &&
                    Utilities.isGreenCandle(openPrice[itr], highPrice[itr], lowPrice[itr], closePrice[itr]) &&
                    openPrice[itr] > closePrice[itr-1] && //Gap-up or Flat opening
                    closePrice[itr] > closePrice[itr - 1] //Closes above
                    )
                {
                    volumek = totalVol[itr];
                    volumej = totalVol[itr - 1];
                    volumei = totalVol[itr - 2];
                    if (volumei < volumej && volumej < volumek)
                    {
                        outReal[itr] = 100;
                    }
                }
                else if (
                    Utilities.isRedCandle(openPrice[itr-2], highPrice[itr-2], lowPrice[itr-2], closePrice[itr-2]) &&
                    Utilities.isRedCandle(openPrice[itr - 1], highPrice[itr - 1], lowPrice[itr - 1], closePrice[itr - 1]) &&
                    Utilities.isRedCandle(openPrice[itr], highPrice[itr], lowPrice[itr], closePrice[itr]) &&
                    closePrice[itr] < closePrice[itr - 1] &&  //Closes below
                    openPrice[itr] < closePrice[itr - 1]   //Gap-down or flat opening
                    )
                {
                    volumek = totalVol[itr];
                    volumej = totalVol[itr - 1];
                    volumei = totalVol[itr - 2];
                    if (volumei > volumej && volumej > volumek)
                    {
                        outReal[itr] = -100;
                    }
                }
            }

            outNBElement = endIdx + 1;
        }
        private void IdentifyPatterns()
        {
            //1. List of all EMA values
            foreach (KeyValuePair<Tuple<string, DateOnly>, double> item in VOLGROWTH)
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
            summary.Indicator = IndicatorType.VOLGROWTH;
            base.UpdateSummary();
        }
    }
}
