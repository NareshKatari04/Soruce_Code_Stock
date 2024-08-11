using EODDownloader;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class SmaCross : Stat
    {
        //Data members
        private int OptInSlowPeriod;

        private int OptInFastPeriod;

        Sma SmaFast;
        Sma SmaSlow;

        public Dictionary<Tuple<string, DateOnly>, double> SMACROSS = new Dictionary<Tuple<string, DateOnly>, double>();
        
        //Constructors
        public SmaCross(string symbol) : base(symbol) 
        { 
            Symbol = symbol;
            OptInPeriod = 4;
            OptInSlowPeriod = 26;
            OptInFastPeriod = 9;
        }
        public SmaCross(string symbol, int optInFastPeriod, int optInSlowPeriod, int optInSignalPeriod) : base(symbol)
        {
            Symbol = symbol;
            OptInPeriod = 4;
            OptInFastPeriod = optInFastPeriod;
            OptInSlowPeriod = optInSlowPeriod;
        }
        public SmaCross(string symbol, DateOnly from, DateOnly to, int optInFastPeriod, int optInSlowPeriod, PriceType price) : base(symbol, from, to, 4, price) 
        {
            OptInSlowPeriod = optInSlowPeriod;
            OptInFastPeriod = optInFastPeriod;
        }
        public void Process()
        {
            Calculate();
            IdentifyPatterns();
            UpdateSummary();
        }
        public void Calculate()
        {
            //1. EMA with fast period
            SmaFast = new Sma(Symbol, From, To, OptInFastPeriod, TypeOfPrice);
            SmaFast.Calculate();

            //2. EMA with slow period
            SmaSlow = new Sma(Symbol, From, To, OptInSlowPeriod, TypeOfPrice);
            SmaSlow.Calculate();

            //3. Loop through thhe fast period EMA list
            foreach(var fastItem in SmaFast.SMA)
            {
                //4. Cross found
                if (SmaSlow.SMA.ContainsKey(fastItem.Key) )
                {
                    SMACROSS.Add( new Tuple<string, DateOnly>(Symbol, fastItem.Key.Item2), (int)fastItem.Value - (int)SmaSlow.SMA[fastItem.Key] );
                }
            }
        }
        private void IdentifyPatterns()
        {
            DateOnly previousTradedDate;
            Tuple<string, DateOnly> prevKey = null;

            //1. List of all SMA values
            foreach (var item in SMACROSS)
            {
                //Cross occures
                if( item.Value <= 1 && item.Value >= -1)
                {
                    //Get the previous traded date
                    previousTradedDate = Utilities.GetNthTradedDate(Symbol, item.Key.Item2, -1 * OptInPeriod );
                    prevKey = new Tuple<string, DateOnly>(Symbol, previousTradedDate);

                    if( SmaFast.SMA.ContainsKey(prevKey) && SmaSlow.SMA.ContainsKey(prevKey) )
                    {
                        //Bull occurance
                        if (SmaFast.SMA[prevKey] < SmaSlow.SMA[prevKey])
                        {
                            //Console.WriteLine("DEBUG: BULL Cross current date={0} currentFastSma={1} CurrentSlowSma={2} PreviousFastSma={3} PreviousSlowSMA={4} PreviousDate={5}", item.Key.Item2, SmaFast.SMA[item.Key], SmaSlow.SMA[item.Key], SmaFast.SMA[prevKey] , SmaSlow.SMA[prevKey], prevKey.Item2);
                            summary.BullOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                        }
                        //Bear occurance
                        else if (SmaFast.SMA[prevKey] > SmaSlow.SMA[prevKey])
                        {
                            //Console.WriteLine("DEBUG: BEAR Cross currentDate={0} CurrentFastSMA={1} CurrentSlowSMA={2} PreviousFastSMA={3} PreviousSlowSMA={4}", item.Key.Item2, EmaFast.EMA[item.Key], EmaSlow.EMA[item.Key], EmaFast.EMA[prevKey], EmaSlow.EMA[prevKey]);
                            summary.BearOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                        }
                    }
                }
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.SMACROSS;
            base.UpdateSummary();
            /*
            foreach(var item in SMACROSS.Keys)
                Console.WriteLine( item.Item2 + ":" + SMACROSS[item]);
            */
        }
    }
}
