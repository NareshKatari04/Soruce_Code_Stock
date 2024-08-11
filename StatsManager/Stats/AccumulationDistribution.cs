using EODDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class AccumulationDistribution : Stat
    {
        public Dictionary<Tuple<string, DateOnly>, double> AD = new Dictionary<Tuple<string, DateOnly>, double>();
        public AccumulationDistribution(string symbol) : base(symbol) { OptInPeriod = 20; } //Default value
        public AccumulationDistribution(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }
        public void Process()
        {
            Calculate();
            IdentifyPatterns();
            UpdateSummary();
        }
        public void Calculate()
        {
            //1. Prepare the input parameters
            List<double> inRealHigh = GetInputPriceList(PriceType.HighPrice);
            List<double> inRealClose = GetInputPriceList(PriceType.ClosePrice);
            List<double> inRealLow = GetInputPriceList(PriceType.LowPrice);
            List<double> inRealVolume = GetVolumeList();

            int begIdx = 0;
            int endIdx = inRealHigh.Count - 1;
            int outBegIdx = 0;
            int outNBElement = 0;
            double[] outReal = new double[endIdx - begIdx + 1];

            Core.Ad(begIdx, endIdx, inRealHigh.ToArray(), inRealLow.ToArray(), inRealClose.ToArray(), inRealVolume.ToArray(), out outBegIdx, out outNBElement, outReal.ToArray());
            
            for (int i = 0; i < outNBElement; i++)
            {
                //Console.WriteLine( "DEBUG: ACCUMULATION/DISTRIBUTION {0} {1} {2}", Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE), outReal[i]);
                AD.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outReal[i]);
            }
        }
        private void IdentifyPatterns()
        {
            Equity previousEquity = null;

            //1. Loop over AD values
            foreach (KeyValuePair<Tuple<string, DateOnly>, double> item in AD)
            {
                //2. Get current day equity's history
                Equity currentEquity = History.HISTORY[item.Key];

                //TODO
                OptInPeriod = 4;
                double averageTradedQuantity = 0;
                double averageClosePrice = 0;
                int count = 0;

                //3. Get the last n days (OptInPeriod) avg TotalTradedQuantity
                for(var x = 1; x<= OptInPeriod; x++)
                {
                    DateOnly backDate = Utilities.GetNthTradedDate(Symbol, DateOnly.FromDateTime(currentEquity.DATE), -1 *x);
                    if (backDate != DateOnly.FromDateTime(currentEquity.DATE))
                    {
                        averageTradedQuantity += History.HISTORY[new Tuple<string, DateOnly>(Symbol, backDate)].TOTTRDQTY;
                        averageClosePrice += History.HISTORY[new Tuple<string, DateOnly>(Symbol, backDate)].CLOSEPRICE;
                        count++;
                    }
                    else
                    {
                        averageTradedQuantity = 0;
                        averageClosePrice = 0;
                        break;
                    }
                }
                if (count != 0)
                {
                    //Console.WriteLine(averageTradedQuantity);
                    averageTradedQuantity = averageTradedQuantity / count;
                    averageClosePrice = averageClosePrice / count;
                }
                else
                    averageTradedQuantity = 0;
                //averageTradedQuantity = count != 0 ?  (averageTradedQuantity / count) : 0 ;
                
                //3.Check for bull pattern
                if( previousEquity != null && averageTradedQuantity != 0 &&
                    /*(int) currentEquity.OPENPRICE < (int) currentEquity.CLOSEPRICE && */  //3.1 Green candle check 
                    (int) currentEquity.CLOSEPRICE < (int) averageClosePrice &&  //3.2 Lower than previous day close
                    (int) averageTradedQuantity > (int) currentEquity.TOTTRDQTY         //3.3 Total Traded Quantity is down compared last day
                    )
                {
                    double percentDownInVol = ((  averageTradedQuantity - currentEquity.TOTTRDQTY ) / currentEquity.TOTTRDQTY) *100;
                    double percentDownInClosePrice = (( averageClosePrice - currentEquity.CLOSEPRICE) / currentEquity.CLOSEPRICE ) * 100;

                    //Console.WriteLine("DEBUG: ACCUMULATION/DISTRIBUTION SYMBOL={0} DATE={1} PREV_CLOSE={2} CURR_CLOSE={3} PECENT_DIFF_CLOSE={4} PERCENT_DIFF_VOL={5} AVG_TTQ={6} CURR_TTQ={7}", Symbol, item.Key.Item2, previousEquity.CLOSEPRICE, currentEquity.CLOSEPRICE, percentDownInClosePrice, percentDownInVol, (int)averageTradedQuantity, (int)currentEquity.TOTTRDQTY);

                    if ( percentDownInVol >= 80 && percentDownInClosePrice >= 10 )
                    {
                        //Console.WriteLine("DEBUG: ACCUMULATION/DISTRIBUTION {0} {1}", Symbol, item.Key.Item2);
                        summary.BullOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                    }
                }
                previousEquity = currentEquity;
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.ACCUMULATIONDISTRIBUTION;
            base.UpdateSummary();
        }
    }
}
/* 
 * DOCUMENTATION
 * Assume a stock gaps down 20% on huge volume.
 * The price finishes in the upper portion of its daily range, 
 * but is still down 18% from the prior close. Such a move would actually cause the A/D to rise. 
*/    