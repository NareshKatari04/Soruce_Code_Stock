using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsManager
{
    public class EmaCross : Stat
    {
        //Data members
        private int OptInSlowPeriod;

        private int OptInFastPeriod;

        Ema EmaFast;
        Ema EmaSlow;

        public Dictionary<Tuple<string, DateOnly>, double> EMACROSS = new Dictionary<Tuple<string, DateOnly>, double>();

        //Constructors
        public EmaCross(string symbol) : base(symbol)
        {
            Symbol = symbol;
            OptInPeriod = 4;
            OptInSlowPeriod = 26;
            OptInFastPeriod = 13;
        }
        public EmaCross(string symbol, int optInFastPeriod, int optInSlowPeriod, int optInSignalPeriod) : base(symbol)
        {
            Symbol = symbol;
            OptInPeriod = 4;
            OptInFastPeriod = optInFastPeriod;
            OptInSlowPeriod = optInSlowPeriod;
        }
        public EmaCross(string symbol, DateOnly from, DateOnly to, int optInFastPeriod, int optInSlowPeriod, PriceType price) : base(symbol, from, to, 4, price)
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
            EmaFast = new Ema(Symbol, From, To, OptInFastPeriod, TypeOfPrice);
            EmaFast.Calculate();

            //2. EMA with slow period
            EmaSlow = new Ema(Symbol, From, To, OptInSlowPeriod, TypeOfPrice);
            EmaSlow.Calculate();

            //3. Loop through thhe fast period EMA list
            foreach (var fastItem in EmaFast.EMA)
            {
                //4. Cross found
                if (EmaSlow.EMA.ContainsKey(fastItem.Key))
                {
                    EMACROSS.Add(new Tuple<string, DateOnly>(Symbol, fastItem.Key.Item2), (int)fastItem.Value - (int)EmaSlow.EMA[fastItem.Key]);
                }
            }
        }
        private void IdentifyPatterns()
        {
            DateOnly previousTradedDate;
            Tuple<string, DateOnly> prevKey = null;

            //1. List of all EMA values
            foreach (var item in EMACROSS)
            {
                //Cross occures
                if (item.Value <= 1 && item.Value >= -1)
                {
                    //Get the previous traded date
                    previousTradedDate = Utilities.GetNthTradedDate(Symbol, item.Key.Item2, -1 * OptInPeriod);
                    prevKey = new Tuple<string, DateOnly>(Symbol, previousTradedDate);

                    if (EmaFast.EMA.ContainsKey(prevKey) && EmaSlow.EMA.ContainsKey(prevKey))
                    {
                        //Bull occurance
                        if (EmaFast.EMA[prevKey] < EmaSlow.EMA[prevKey])
                        {
                            //Console.WriteLine("DEBUG: BULL Cross current date={0} currentFastEma={1} CurrentSlowEma={2} PreviousFastEma={3} PreviousSlowEMA={4}", item.Key.Item2, EmaFast.EMA[item.Key], EmaSlow.EMA[item.Key], EmaFast.EMA[prevKey] , EmaSlow.EMA[prevKey]);
                            summary.BullOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                        }
                        //Bear occurance
                        else if (EmaFast.EMA[prevKey] > EmaSlow.EMA[prevKey])
                        {
                            //Console.WriteLine("DEBUG: BEAR Cross current date={0} currentFastEma={1} CurrentSlowEma={2} PreviousFastEma={3} PreviousSlowEMA={4}", item.Key.Item2, EmaFast.EMA[item.Key], EmaSlow.EMA[item.Key], EmaFast.EMA[prevKey], EmaSlow.EMA[prevKey]);
                            summary.BearOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                        }
                    }
                }
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.EMACROSS;
            base.UpdateSummary();
        }
    }
}
