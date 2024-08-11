using EODDownloader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StatsManager
{
    public class HigherHighLowerLow : Stat
    {
        /* Data members */
        public List< Tuple<DateOnly, int>> HigherHighs = new List<Tuple<DateOnly, int>>();
        public List< Tuple<DateOnly, int>> LowerLows = new List<Tuple<DateOnly, int>>();
        public List< Tuple<DateOnly, int>> HigherLows = new List< Tuple<DateOnly, int>>();
        public List< Tuple<DateOnly, int>> LowerHighs = new List<Tuple<DateOnly, int>>();

        /* Constructor */
        public HigherHighLowerLow(string symbol):base(symbol) 
        {
            OptInPeriod = 4;
        }
        public HigherHighLowerLow(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }
        /* Methods */
        public void Process()
        {
            try
            {
                Calculate();
                IdentifyPatterns();
                UpdateSummary();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private void Calculate()
        {
            //[NOTE] : Touch this core logic, your day sucks

            // 1. It depends on trend line
            Trend trend = new Trend(Symbol);
            trend.Process();

            //3. Higher highs detection logic
            DateOnly? currentHighFormedDate = trend.TREND.Where(x => x.Value == -100).First().Key.Item2;
            DateOnly? previousHighFormedDate = currentHighFormedDate;

            DateOnly? currentLowFormedDate = trend.TREND.Where( x => x.Value == 100).First().Key.Item2;
            DateOnly? previousLowFormedDate = currentLowFormedDate;

            DateOnly? previousHigherLowFormedDate = currentLowFormedDate;
            DateOnly? previousLowerHHighFormedDate = currentHighFormedDate;

            int higherHighCount = 0;
            int lowerLowCount = 0;

            int higherLowCount = 0;
            int lowerHighCount = 0;
            if (trend.TREND.Count > 1)
            {
                foreach (var trendItem in trend.TREND)
                {
                    //4. High found : -100 is a detection where Bear trend begins from -or- Bull trend ends
                    if (-100 == trendItem.Value)
                    {
                        lowerHighCount = 0;

                        currentHighFormedDate = trendItem.Key.Item2;
                        double previousHigherHighPrice = History.HISTORY[new Tuple<string, DateOnly>(Symbol, (DateOnly)previousHighFormedDate)].HIGHPRICE;
                        double currentHigherHighPrice = History.HISTORY[new Tuple<string, DateOnly>(Symbol, (DateOnly) currentHighFormedDate)].HIGHPRICE;
                        //5. Higher high found
                        if (previousHigherHighPrice < currentHigherHighPrice)
                        {
                            higherHighCount++;
                            HigherHighs.Add( new Tuple<DateOnly, int>((DateOnly)currentHighFormedDate, higherHighCount));

                            previousHighFormedDate = currentHighFormedDate;
                            //Console.WriteLine("DEBUG: HH {0} {1} {2}", Symbol, currentHighFormedDate, higherHighCount);
                        }
                        else
                        {
                            higherHighCount = 0;
                            previousHighFormedDate = trendItem.Key.Item2;
                            //5. Lower high case handling
                            if (currentHigherHighPrice < previousHigherHighPrice)
                            {
                                lowerHighCount++;
                                previousLowerHHighFormedDate = currentHighFormedDate;
                                LowerHighs.Add(new Tuple<DateOnly, int>((DateOnly)currentHighFormedDate, lowerHighCount));
                                //Console.WriteLine("DEBUG: LH {0} {1} {2}", Symbol, currentHighFormedDate, lowerHighCount);
                            }
                        }
                    }
                    //4. Low found - TODO THIS LOGIC ISN't WORKING REVIEW LATER
                    else if( 100 == trendItem.Value ) 
                    {
                        currentLowFormedDate = trendItem.Key.Item2;
                        double previousLowPrice = History.HISTORY[new Tuple<string, DateOnly>(Symbol, (DateOnly)previousLowFormedDate)].LOWPRICE;
                        double currentLowPrice = History.HISTORY[new Tuple<string, DateOnly>(Symbol, (DateOnly)currentLowFormedDate)].LOWPRICE;
                        //5. First low or the Lower Low found
                        if (currentLowPrice < previousLowPrice)
                        {
                            higherLowCount = 0;

                            //6. Lower low case handling
                            lowerLowCount++;
                            LowerLows.Add(new Tuple<DateOnly, int>((DateOnly)currentLowFormedDate, lowerLowCount));

                            previousLowFormedDate = currentLowFormedDate;
                            /*
                            if (currentLowFormedDate >= DateOnly.FromDateTime(DateTime.Now).AddDays(-15))
                                Console.WriteLine("DEBUG: LL {0} {1} {2}", Symbol, currentLowFormedDate, lowerLowCount);
                            */
                        }
                        else 
                        {
                            lowerLowCount = 0;
                            previousLowFormedDate = trendItem.Key.Item2;
                            //6. Higher low case handling
                            if (currentLowPrice > previousLowPrice)
                            {
                                higherLowCount++;
                                HigherLows.Add(new Tuple<DateOnly, int>((DateOnly)currentLowFormedDate, higherLowCount) );
                                previousHigherLowFormedDate = currentLowFormedDate;
                                /*
                                if( currentLowFormedDate >= DateOnly.FromDateTime(DateTime.Now).AddDays(-15)) 
                                    Console.WriteLine("DEBUG: HL {0} {1} {2}", Symbol, currentLowFormedDate, higherLowCount);
                                */
                            }

                        }
                    }
                }
            }
        }
        private void IdentifyPatterns()
        {
            int level = 2;
            //1. Loop through higherHighs list
            for (int i = 0; i < HigherLows.Count; i++)
            {
                if (HigherLows[i].Item2 <= level)
                {
                    if (i + 1 < HigherLows.Count && HigherLows[i+1].Item2 > level)
                    {
                        summary.BullOccurances.Add(HigherLows[i].Item1, 100);
                    }
                    else if (i + 1 < HigherLows.Count && HigherLows[i+1].Item2 == level)
                    {
                        continue;
                    }
                    else
                        summary.BullOccurances.Add(HigherLows[i].Item1, -100);
                }
            }

            //2. Loop through LowerHighs list
            for (int i = 0; i < LowerHighs.Count; i++)
            {
                if (LowerHighs[i].Item2 == level)
                {
                    if (i + 1 < LowerHighs.Count && LowerHighs[i + 1].Item2 > level)
                    {
                        summary.BearOccurances.Add(LowerHighs[i].Item1, -100);
                    }
                    else if (i + 1 < LowerHighs.Count && LowerHighs[i + 1].Item2 == level)
                    {
                        continue;
                    }
                    else
                    {
                        summary.BearOccurances.Add(LowerHighs[i].Item1, 100);
                    }
                }
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.HHLL;
            base.UpdateSummary();
        }
    }
}
