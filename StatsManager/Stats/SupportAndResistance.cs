using EODDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class SupportAndResistance : Stat
    {
        public Dictionary<Tuple<string, DateOnly>, double> SUPPORT = new Dictionary<Tuple<string, DateOnly>, double>();
        public Dictionary<Tuple<string, DateOnly>, double> RESISTNACE = new Dictionary<Tuple<string, DateOnly>, double>();
        public SupportAndResistance(string symbol) : base(symbol) { OptInPeriod = 4; } //Default value
        public SupportAndResistance(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }
        public void Process()
        {
            Calculate();
            IdentifyPatterns();
            UpdateSummary();
            //AddOnMore();
        }

        private void AddOnMore()
        {
            DateOnly currentDateOnly = DateOnly.FromDateTime(DateTime.Now.AddDays(-2));
            Tuple<string, DateOnly> currentKey = new Tuple<string, DateOnly>(Symbol, currentDateOnly);
            double currentPrice = History.HISTORY[currentKey].CLOSEPRICE;

            //1. Loop through the Support point dates
            foreach (KeyValuePair<Tuple<string, DateOnly>, double> item in SUPPORT)
            {
                if (History.HISTORY.ContainsKey(item.Key))
                {
                    Equity eq = History.HISTORY[item.Key];
                    double suppPrice = eq.CLOSEPRICE;

                    double percentDiff = 0;
                    percentDiff = suppPrice > currentPrice ? 
                        ((suppPrice - currentPrice) / ((suppPrice + currentPrice) / 2)) * 100 :
                        ((currentPrice - suppPrice) / ((suppPrice + currentPrice) / 2)) * 100 ;
                    
                    if (percentDiff == 0 || percentDiff < 2 )
                        Console.WriteLine("DEBUG: SUPPPORT: MATCH {0} {1} {2} {3} {4}", Symbol, item.Key, suppPrice, currentPrice, percentDiff);
                }
            }
        }

        private bool PriceRangeMatch(Tuple<string, DateOnly> left, Tuple<string, DateOnly> right)
        {
            double permittedPercentageOfDifference = 2.0;
            bool matched = false;

            Equity eqLeft = History.HISTORY[left];
            Equity eqRight = History.HISTORY[right];

            double priceLeft = (eqLeft.OPENPRICE + eqLeft.HIGHPRICE + eqLeft.LOWPRICE + eqLeft.CLOSEPRICE) / 4;
            double priceRight = (eqRight.OPENPRICE + eqRight.HIGHPRICE + eqRight.LOWPRICE + eqRight.CLOSEPRICE) / 4;

            double diff = priceLeft > priceRight ? priceLeft - priceRight : priceRight - priceLeft;
            double actualPercentageOfDifference = ( ( diff / ((priceLeft + priceRight)/2) ) ) * 100;
            matched = actualPercentageOfDifference <= permittedPercentageOfDifference ? true : false;

            return matched;
        }
        public void Calculate()
        {
            //0. Trend determination is needed
            Trend trend = new Trend(Symbol, 4);
            trend.Process();

            List< Tuple<string, DateOnly> > possibleSupports = (from t in trend.TREND where t.Value == 100 select t.Key ).ToList();
            possibleSupports = possibleSupports.OrderByDescending( x => x.Item2 ).ToList();
            
            List< Tuple<string, DateOnly> > possibleResistances = (from t in trend.TREND where t.Value == -100 select t.Key).ToList();
            possibleResistances = possibleResistances.OrderByDescending(x => x.Item2).ToList();

            for (int i = possibleResistances.Count - 1; i >= 0; i--)
            {
                if (!RESISTNACE.ContainsKey(possibleResistances[i]))
                    RESISTNACE.Add(possibleResistances[i], 1);

                for (int j = i - 1; j >= 1; j--)
                {
                    if (PriceRangeMatch(possibleResistances[i], possibleResistances[j]))
                    {
                        if (RESISTNACE.ContainsKey(possibleResistances[i]))
                        {
                            RESISTNACE[possibleResistances[i]]++;
                        }
                    }
                }
            }

            for (int i = possibleSupports.Count - 1; i >= 0; i--)
            {
                if(!SUPPORT.ContainsKey(possibleSupports[i]) )
                    SUPPORT.Add( possibleSupports[i], 1 );
                
                for (int j = i - 1; j >= 1; j--)
                {
                    //Console.WriteLine("Range comparision {0} vs {1}", possibleSupports[i].Item2 , possibleSupports[j].Item2);
                    if (PriceRangeMatch(possibleSupports[i], possibleSupports[j]))
                    {
                        //Console.WriteLine("Price range matches {0} {1}", possibleSupports[i].Item2, possibleSupports[j].Item2);
                        if (SUPPORT.ContainsKey(possibleSupports[i]))
                        {
                            SUPPORT[possibleSupports[i]]++;
                        }
                    }
                }
            }
        }
        private void IdentifyPatterns()
        {
            //1. List to bull occurances
            foreach (KeyValuePair<Tuple<string, DateOnly>, double> item in SUPPORT)
            {
                //Check if the trend was rally bullish after 6 days [HARD CODED to 6]
                summary.BullOccurances.Add(item.Key.Item2, item.Value >= 2 ? 100 : -100 );
                //Console.WriteLine("DEBUG: SUPP_RESS: Bull: symbol={0}|date={1}", Symbol, item.Key.Item2);
            }
            //2. List to beear occurances
            foreach (KeyValuePair<Tuple<string, DateOnly>, double> item in RESISTNACE)
            {
                summary.BearOccurances.Add(item.Key.Item2, item.Value >= 2 ? -100 : 100 );
                //Console.WriteLine("DEBUG: SUPP_RESS: Bear: symbol={0}|date={1}", Symbol, item.Key.Item2);
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.SUPPORT_RESISTANCE;
            base.UpdateSummary();
        }
        private void TA_SUPPORTRESIST(int startIdx, int endIdx, double[] inRealOpen, double[] inRealHigh, double[] inRealLow, double[] inRealClose,
            int optInTimePeriod, out int outBegIdx, out int outNbElement, double[] outReal)
        {
            outBegIdx = 0;
            outNbElement = 0;

            //0. Trend determination is needed
            Trend trend = new Trend(Symbol);
            trend.Process();

            List<DateOnly> possibleSupports = (from t in trend.TREND where t.Value == 100 select t.Key.Item2).ToList();
            List<DateOnly> possibleResistances = (from t in trend.TREND where t.Value == -100 select t.Key.Item2).ToList();

            //NOTE: Lets take the day's avg price for comfort this fits goot to compare against supp/resist price
            double avgDayPrice = 0; 

            //1. Loop by each day's price
            for (var i = 0; i <= endIdx; ++i)
            {
                //2. Take the day's average price to compare against the support/resistance range in the history
                avgDayPrice = ( inRealOpen[i] + inRealHigh[i] + inRealLow[i] + inRealClose[i] ) / 4;
            }
        }
    }
}
