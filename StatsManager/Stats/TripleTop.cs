using EODDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using EODDownloader;
using TicTacTec.TA.Library;
using System.ComponentModel.Design;

namespace StatsManager
{
    public class TripleTop : Stat
    {
        public Dictionary<Tuple<string, DateOnly>, double> TRIPLETOP = new Dictionary<Tuple<string, DateOnly>, double>();
        public TripleTop(string symbol) : base(symbol) { OptInPeriod = 14; } //Default value
        public TripleTop(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }

        public void Process()
        {
            Calculate();
            IdentifyPatterns();
            UpdateSummary();
        }
        public void Calculate()
        {
            /*
            //1. Prepare the input parameters
            List<double> inReal = GetInputPriceList(TypeOfPrice);

            int begIdx = 0;
            int endIdx = inReal.Count - 1;
            int outBegIdx = 0;
            int outNBElement = 0;
            int[] outReal = new int[endIdx - begIdx + 1];
            */
            //2. Invoke the core function
            TA_TripleTop(/*begIdx, endIdx, inReal.ToArray(), OptInPeriod, outBegIdx, outNBElement, outReal*/);
        }
        private void TA_TripleTop(/*int begIdx, int endIdx, double[] doubles, int optInPeriod, int outBegIdx, int outNBElement, int[] outReal*/)
        {
            //Triple top pattern indicator is based on support and resistances
            Trend trend = new Trend(Symbol, From, To, OptInPeriod, TypeOfPrice);
            trend.Process();

            bool firstTop = false, secondTop = false, thirdTop = false;
            Tuple<string, DateOnly> firstTopKey = null;
            Tuple<string, DateOnly> secondTopKey = null;

            bool firstBottom = false, secondBottom = false;
            Tuple<string, DateOnly> firstBottomKey = null;

            //Tuning variables
            float priceDifferenceAtmost = (float) 2.5;
            
            //Loop through the trend dictionary
            foreach(var item in trend.TREND.Keys)
            {
                //1. The sequence of graph is -100, 100, -100, 100, -100
                if (trend.TREND[item] == -100)
                {
                    if (!firstTop && !secondTop && !thirdTop)
                    {
                        firstTop = true;
                        firstTopKey = item;
                        Console.WriteLine("DEBUG: [1] First top price {0} {1}", item.Item1, item.Item2);
                    }
                    else if (firstTop && firstBottom && !secondTop && !thirdTop)
                    {
                        //Second top touched price should be on the same range of first top touched price
                        if (firstTopKey != null &
                            Utilities.ArePricesInSameRange(History.HISTORY[item].CLOSEPRICE, History.HISTORY[firstTopKey].CLOSEPRICE, priceDifferenceAtmost))
                        {
                            secondTop = true;
                            secondTopKey = item;
                            Console.WriteLine("DEBUG: [2] Second top price {0} {1}", item.Item1, item.Item2);
                        }
                        else
                        {
                            //Rule failed - reset the first top - this could be the first top touched price
                            firstTop = true;
                            firstTopKey = item;
                            Console.WriteLine("DEBUG: [3] First top price {0} {1}", item.Item1, item.Item2);
                            Console.WriteLine("DEBUG: {0} VS {1}", History.HISTORY[item].CLOSEPRICE, History.HISTORY[firstTopKey].CLOSEPRICE);
                        }
                    }
                    else if (firstTop && firstBottom && secondTop && secondBottom && !thirdTop)
                    {
                        //Third top price touched shoud be on the same range of the second top touched price
                        if (secondTopKey != null &&
                            Utilities.ArePricesInSameRange(History.HISTORY[secondTopKey].CLOSEPRICE, History.HISTORY[item].CLOSEPRICE, priceDifferenceAtmost)
                            )
                        {
                            // HURREY -- Third top pattern found
                            thirdTop = true;
                            TRIPLETOP.Add(item, 100);
                            Console.WriteLine("DEBUG: [4] *Triple top price {0} {1}", item.Item1, item.Item2);
                        }
                        else
                        {
                            //Rule failed- reset all other markers - this is the last checkpoint of indicator
                            //This could be the first top
                            firstTop = true;
                            firstTopKey = item;
                            secondTop = false; secondTopKey = null;
                            thirdTop = false;
                            firstBottom = false; firstBottomKey = null;
                            Console.WriteLine("DEBUG: [5] First top price {0} {1}", item.Item1, item.Item2);
                        }
                    }
                }
                else if (trend.TREND[item] == 100)
                {
                    //if (!firstBottom && !secondBottom && firstTop)
                    if( firstTop && !firstBottom && !secondTop && !secondBottom && !thirdTop)
                    {
                        firstBottom = true;
                        firstBottomKey = item;
                        Console.WriteLine("DEBUG: [6] First bottom price {0} {1}", item.Item1, item.Item2);
                    }
                    else if( firstTop && firstBottom && secondTop && !secondBottom && !thirdTop)
                    {
                        //Second bottom should be on the same price range of first bottom
                        if (firstBottomKey != null &&
                            Utilities.ArePricesInSameRange(History.HISTORY[firstBottomKey].CLOSEPRICE, History.HISTORY[item].CLOSEPRICE, priceDifferenceAtmost)
                            )
                        {
                            secondBottom = true;
                            Console.WriteLine("DEBUG: [7] Second bottom price {0} {1}", item.Item1, item.Item2);
                        }
                        else
                        {
                            //Rule failed - this is not the proper second bottom - reset everything
                            firstTop = false; firstTopKey = null;
                            secondTop = false; secondTopKey = null;
                            thirdTop = false;
                            firstBottom = false; firstBottomKey = null;
                            Console.WriteLine("DEBUG: [8] Total reset {0} {1}", item.Item1, item.Item2);
                        }
                    }
                }
            }
        }
        private void IdentifyPatterns()
        {
            //1. List of all EMA values
            foreach (KeyValuePair<Tuple<string, DateOnly>, double> item in TRIPLETOP)
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
            summary.Indicator = IndicatorType.TRIPLE_TOP;
            base.UpdateSummary();
        }
    }
}
