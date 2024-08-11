using EODDownloader;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace StatsManager
{
    public class Trend : Stat
    {
        public Dictionary< Tuple<string, DateOnly>, double> TREND = new Dictionary<Tuple<string, DateOnly>, double>();
        
        /* Constructors */
        public Trend(string symbol) : base(symbol) { OptInPeriod = 4; } //HARD-CODED

        public Trend(string symbol, int optInPeriod) :base(symbol) { OptInPeriod = optInPeriod; }
        public Trend(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }

        /* Member functions */
        public void Process()
        {
            Calculate();
            IdentifyPatterns();
        }
        private void Calculate()
        {
            //1. Prepare the input parameters
            List<double> inReal = GetInputPriceList(TypeOfPrice);
            int begIdx = 0;
            int endIdx = inReal.Count - 1;

            int outBegIdx = 0;
            int outNBElement = 0;
            double[] outReal = new double[endIdx - begIdx + 1];
            TA_TREND(begIdx, endIdx, inReal.ToArray(), this.OptInPeriod, out outBegIdx, out outNBElement, outReal);
            for (int i = 0; i < outNBElement; i++)
            {
                /*
                if (outReal[i] == 100 || outReal[i] == -100)
                    Console.WriteLine("OUTPUT: {0} {1} {2}", Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE), outReal[i]);
                */
                TREND.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outReal[i]);
            }
        }
        private void IdentifyPatterns()
        {
            foreach (KeyValuePair<Tuple<string, DateOnly>, double> item in TREND)
            {
                //Check if Trend reversal signal occures 100 | -100
                if (item.Value == 100)
                {
                    summary.BullOccurances.Add(item.Key.Item2, 0);
                    //Console.WriteLine("DEBUG: TEST: BULL {0} {1} {2} {3}", item.Key.Item1, item.Key.Item2, item.Value, History.HISTORY[item.Key].HIGHPRICE);
                }
                else if (item.Value == -100)
                {
                    summary.BearOccurances.Add(item.Key.Item2, 0);
                    //Console.WriteLine("DEBUG: TEST: BEAR {0} {1} {2} {3}", item.Key.Item1, item.Key.Item2, item.Value, History.HISTORY[item.Key].LOWPRICE);
                }
            }
        }
        /* DON'T YOU DARE TOUCH THIS METHOD CODE */
        private void TA_TREND(int startIdx, int endIdx, double[] inReal,
            int optInTimePeriod, out int outBegIdx, out int outNbElement, double[] outReal)
        {

            int upTrend = 0, downTrend = 0;
            double prevAvg = 0, nextAvg = 0;
            double[] outRealTmp = new double[endIdx + 1];

            //1. For each day's price
            for (var i = 0; i <= endIdx; ++i)
            {
                upTrend = 0; downTrend = 0;
                //2. calculate the previous 5 days average
                prevAvg = GetAverageofLastNDays(inReal, optInTimePeriod, i, endIdx);
                //3. calculate the next 5 days average
                nextAvg = GetAverageofNextNDays(inReal, optInTimePeriod, i, endIdx);

                //4. Check if it is down trend
                if (prevAvg > inReal[i])
                {
                    ++downTrend;
                }
                //5. Check if it is up trend
                else if (prevAvg < inReal[i])
                {
                    ++upTrend;
                }
                //6. Check if it is down trend
                if (nextAvg < inReal[i])
                {
                    ++downTrend;
                }
                //7. Check if it is up trend
                else if (nextAvg > inReal[i])
                {
                    ++upTrend;
                }

                //8. Check final trend
                if (downTrend > upTrend)
                {
                    outRealTmp[i] = -1;
                }
                else if (upTrend > downTrend)
                {
                    outRealTmp[i] = 1;
                }
                else
                {
                    outRealTmp[i] = 0;
                }
            }

            //Smoothen the trend
            for (var itr = startIdx; itr <= endIdx; ++itr)
            {
                //1. Check if next elemennt is present AND
                //1. Its zero
                if (itr + 1 <= endIdx && outRealTmp[itr + 1] == 0)
                {
                    if (itr + 2 <= endIdx && (outRealTmp[itr + 2] == 0 || outRealTmp[itr + 2] == outRealTmp[itr]))
                        outRealTmp[itr + 1] = outRealTmp[itr];
                }
            }

            //Apply indicator of trend
            double trendContinuity = 0;
            for (var i = 1; i <= endIdx; ++i)
            {
                if (i - 1 <= endIdx && i + 1 <= endIdx)
                {
                    if (outRealTmp[i] == 0 && outRealTmp[i - 1] < 0 && outRealTmp[i + 1] > 0)
                    {
                        //std::cout << "Reversal from down to up at " << day[i] << std::endl;
                        outReal[i] = 100;
                        trendContinuity = 100;
                    }
                    else if (outRealTmp[i] == 0 && outRealTmp[i - 1] > 0 && outRealTmp[i + 1] < 0)
                    {
                        //std::cout << "Reversal from up to down at " << day[i] << std::endl;
                        outReal[i] = -100;
                        trendContinuity = -100;
                    }
                    else
                    {
                        if (trendContinuity == 100) outReal[i] = 1;
                        else if (trendContinuity == -100) outReal[i] = -1;
                        else outReal[i] = 0;
                    }
                }
            }
            outBegIdx = 0;
            outNbElement = endIdx + 1;
        }
        private double GetAverageofLastNDays(double[] inReal, int optInPeriod, int begIdx, int endIdx)
        {
            double avg = 0;
            int count = 0;

            for (var i = 0; i < optInPeriod; i++)
            {
                if (begIdx - i - 1 > 0)
                {
                    avg += inReal[begIdx - i - 1];
                    ++count;
                }
            }
            return count != 0 ? (avg / count) : 0;
        }
        private double GetAverageofNextNDays(double[] inReal, int optInPeriod, int begIdx, int endIdx)
        {
            double avg = 0;
            int count = 0;

            for (var i = 0; i < optInPeriod; i++)
            {
                if (begIdx + i + 1 <= endIdx)
                {
                    avg += inReal[begIdx + i + 1];
                    ++count;
                }
            }
            return count != 0 ? (avg / count) : 0;
        }

        public static int GetTrend(string symbol, DateOnly givenDate, int distance=5)
        {
            int retStatus = 0;

            //0. Prepare key to look in history
            Tuple<string, DateOnly> key = new Tuple<string, DateOnly>(symbol, givenDate);
            if (!History.HISTORY.ContainsKey(key))
            {
                //Console.WriteLine("Debug not found {0} {1}", symbol, givenDate);
                return 0;
            }

            //1. Given date's lowest price
            double currentLowestPrice = History.HISTORY[key].LOWPRICE;

            //2. Given date's highest price
            double currentHighestPrice = History.HISTORY[key].HIGHPRICE;

            //3. The next traded date to compare against
            DateOnly futureDate = givenDate.AddDays(1);

            //4. The next traded date's lowest price to compare against
            double futureLowestPrice = 0;

            //5. The next traded date's highest price to compare against
            double futureHighestPrice = 0;

            int count = 0;
            do
            {
                Tuple<string, DateOnly> futKey = new Tuple<string, DateOnly>(symbol, futureDate);

                if (History.HISTORY.ContainsKey(futKey))
                {
                    futureLowestPrice = futureLowestPrice + History.HISTORY[futKey].LOWPRICE;
                    futureHighestPrice = futureHighestPrice + History.HISTORY[futKey].HIGHPRICE;
                    count++;
                }
                futureDate = futureDate.AddDays(1);
            } while (count < distance && futureDate <= DateOnly.FromDateTime(History.LastUpdatedDate));

            futureLowestPrice = futureLowestPrice / count;
            futureHighestPrice = futureHighestPrice / count;

            if (futureHighestPrice != 0 && futureLowestPrice != 0)
            {
                //Bull trend
                if (currentHighestPrice < futureHighestPrice)
                    retStatus = 100;
                // Bear trend
                else if (currentLowestPrice > futureLowestPrice)
                    retStatus = -100;
            }
            return retStatus;
        }
    }
}
