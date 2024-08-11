using EODDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class Stochastic : Stat
    {
        //Private data members
        private int OptInFastK_Period;
        private int OptInSlowK_Period;
        private int OptInSlowK_MAType;
        private int OptInSlowD_Period;
        private int OptInSlowD_MAType;

        //Public data members
        public Dictionary<Tuple<string, DateOnly>, double> STOCHK = new Dictionary<Tuple<string, DateOnly>, double>();
        public Dictionary<Tuple<string, DateOnly>, double> STOCHD = new Dictionary<Tuple<string, DateOnly>, double>();

        //Constructors
        public Stochastic(string symbol) : base(symbol) 
        { 
            OptInPeriod = 4;

            OptInFastK_Period = 5;
            OptInSlowK_Period = 3;
            OptInSlowK_MAType = (int) Core.MAType.Sma;
            OptInSlowD_Period = 3;
            OptInSlowD_MAType = (int)Core.MAType.Sma;
            UpperLimit = 80;
            LowerLimit = 20;
        } 
        public Stochastic(string symbol, 
            DateOnly from, 
            DateOnly to, 
            int optInPeriod, 
            PriceType price,
            int optInFastK_Period,
            int optInSlowK_Period,
            int optInSlowK_MAType,
            int optInSlowD_Period,
            int optInSlowD_MAType,
            int upperLimit,
            int lowerLimit) : base(symbol, from, to, optInPeriod, price) 
        {
            OptInPeriod = optInPeriod;
            OptInFastK_Period = optInFastK_Period;
            OptInSlowK_Period = optInSlowK_Period;
            OptInSlowK_MAType = (int)optInSlowK_MAType;
            OptInSlowD_Period = optInSlowD_Period;
            OptInSlowD_MAType = (int)optInSlowD_MAType;
            UpperLimit = upperLimit;
            LowerLimit = lowerLimit;
        }
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
            List<double> inRealLow = GetInputPriceList(PriceType.LowPrice);
            List<double> inRealClose = GetInputPriceList(PriceType.ClosePrice);

            int begIdx = 0;
            int endIdx = inRealClose.Count - 1;
            int outBegIdx = 0;
            int outNBElement = 0;
            double[] outSlowK = new double[endIdx - begIdx + 1];
            double[] outSlowD = new double[endIdx - begIdx + 1];

            Core.Stoch(begIdx, 
                endIdx, 
                inRealHigh.ToArray(), 
                inRealLow.ToArray(), 
                inRealClose.ToArray(),
                this.OptInFastK_Period,
                this.OptInSlowK_Period, 
                (Core.MAType) OptInSlowK_MAType, 
                OptInSlowD_Period, 
                (Core.MAType) OptInSlowD_MAType, 
                out outBegIdx, 
                out outNBElement, 
                outSlowK, 
                outSlowD);

            for (int i = 0; i < outNBElement; i++)
            {
                //Console.WriteLine( "DEBUG: STOCH {0} {1} {2}", Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE), outSlowK[i]);
                this.STOCHK.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outSlowK[i]);
                //Console.WriteLine( "DEBUG: STOCH {0} {1} {2}", Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE), outSlowD[i]);
                this.STOCHD.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outSlowD[i]);
            }
        }
        private void IdentifyPatterns()
        {
            /*
            DateOnly previousTradedDate;
            Tuple<string, DateOnly> prevKey = null;

            //1. List of all SMA values
            foreach (var item in STOCHK)
            {
                //Cross occures
                if (item.Value <= 1 && item.Value >= -1)
                if( STOCHD.ContainsKey(item.Key) &&
                        Math.Abs(STOCHD[item.Key] - STOCHK[item.Key]) <= OptInPeriod )
                {
                    //Get the previous traded date
                    previousTradedDate = Utilities.GetNthTradeDate(Symbol, item.Key.Item2, -1 * OptInPeriod);
                    prevKey = new Tuple<string, DateOnly>(Symbol, previousTradedDate);

                    if (SmaFast.SMA.ContainsKey(prevKey) && SmaSlow.SMA.ContainsKey(prevKey))
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
            */
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.SMA;
            base.UpdateSummary();
        }
    }
}
