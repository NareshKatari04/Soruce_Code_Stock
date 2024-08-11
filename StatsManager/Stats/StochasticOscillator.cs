using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class StochasticOscillator : Stat
    {
        private int OptInFastK_Period;
        private int OptInSlowK_Period;
        private Core.MAType MATypeSlowK;
        private int OptInSlowD_Period;
        private Core.MAType MATypeSlowD;

        public static Dictionary<Tuple<string, DateOnly>, double> STOCHK = new Dictionary<Tuple<string, DateOnly>, double>();
        public static Dictionary<Tuple<string, DateOnly>, double> STOCHD = new Dictionary<Tuple<string, DateOnly>, double>();

        public StochasticOscillator(string symbol) : base(symbol)
        {
            OptInPeriod = 14;
            TypeOfPrice = PriceType.ClosePrice;

            OptInFastK_Period = OptInPeriod;
            OptInSlowK_Period = 3;
            MATypeSlowK = Core.MAType.Sma;
            OptInSlowD_Period = 3;
            MATypeSlowD = Core.MAType.Sma;
        }
        public StochasticOscillator(string symbol, 
            DateOnly from, 
            DateOnly to, 
            int optInFastK_Period,
            int optInSlowK_Period,
            Core.MAType maTypeSlowK,
            int optInSlowD_Period,
            Core.MAType maTypeSlowD) : base(symbol) 
        {
            OptInFastK_Period = optInFastK_Period;
            OptInSlowK_Period = optInSlowK_Period;
            MATypeSlowK = maTypeSlowK;
            OptInSlowD_Period = optInSlowD_Period;
            MATypeSlowD = maTypeSlowD;
        }

        //Methods
        public void Process()
        {
            Calculate();
            IdentifyPatterns();
            UpdateSummary();
        }
        private void Calculate()
        {
            //1. Prepare the input parameters
            List<double> inRealClose = GetInputPriceList(TypeOfPrice);
            List<double> inRealHigh = GetInputPriceList(PriceType.HighPrice);
            List<double> inRealLow = GetInputPriceList(PriceType.LowPrice);

            int begIdx = 0;
            int endIdx = inRealClose.Count - 1;
            int outBegIdx = 0;
            int outNBElement = 0;
            double[] outRealK = new double[endIdx - begIdx + 1];
            double[] outRealD = new double[endIdx - begIdx + 1];

            Core.Stoch(begIdx, 
                endIdx, 
                inRealHigh.ToArray(), 
                inRealLow.ToArray(), 
                inRealClose.ToArray(), 
                OptInFastK_Period, 
                OptInSlowK_Period, 
                Core.MAType.Sma, 
                OptInSlowD_Period, 
                Core.MAType.Sma, 
                out outBegIdx, 
                out outNBElement, 
                outRealK, 
                outRealD);

            for (int i = 0; i < outNBElement; i++)
            {
                //Console.WriteLine("Debug: symbol={0} | date={1} | k={2} | d={3}", Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE), outRealK[i], outRealD[i]);
                STOCHK.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outRealK[i]);
                STOCHD.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outRealD[i]);
            }
        }
        private void IdentifyPatterns()
        {
            int overSoldPoint = 0;
            int overBoughtPoint = 100;
            int crossPoint = 5;
            int nthDay = 3;

            //1. For each STOCHK STOCHD 
            foreach(var item in STOCHK)
            {
                if(STOCHD.ContainsKey(item.Key) )
                //2. Bull pattern
                if (STOCHD.ContainsKey(item.Key) &&
                    STOCHD[item.Key] <= overSoldPoint &&
                    STOCHK[item.Key] <= overSoldPoint &&
                    Math.Abs((int)STOCHD[item.Key] - (int)STOCHK[item.Key]) <= crossPoint
                    )
                {
                    //2.a) Go to the 3rd next day from current data 
                    DateOnly futureDate = Utilities.GetNthTradedDate(Symbol, item.Key.Item2, nthDay);
                    if (
                        STOCHD.ContainsKey(new Tuple<string, DateOnly>(Symbol, futureDate)) &&
                        STOCHK.ContainsKey(new Tuple<string, DateOnly>(Symbol, futureDate))
                        )
                    {
                        if (STOCHK[new Tuple<string, DateOnly>(Symbol, futureDate)] - STOCHD[new Tuple<string, DateOnly>(Symbol, futureDate)] > 0)
                            summary.BullOccurances.TryAdd(futureDate, Trend.GetTrend(Symbol, item.Key.Item2));
                    }
                    //2.b) In case, the future date is not available, we don't miss to add the current cross point
                    else
                    {
                        DateOnly pastDate = Utilities.GetNthTradedDate(Symbol, item.Key.Item2, -1);
                        //if (STOCHK[new Tuple<string, DateOnly>(Symbol, pastDate)] < STOCHD[new Tuple<string, DateOnly>(Symbol, pastDate)])
                        summary.BullOccurances.TryAdd(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2));
                    }
                }
                //3. Bear pattern
                else if (
                    STOCHD.ContainsKey(item.Key) &&
                    STOCHD[item.Key] >= overBoughtPoint &&
                    STOCHK[item.Key] >= overBoughtPoint &&
                    Math.Abs((int)STOCHK[item.Key] - (int)STOCHD[item.Key]) <= crossPoint
                    )
                {
                    //3.a) Go to the 3rd next day from current data 
                    DateOnly futureDate = Utilities.GetNthTradedDate(Symbol, item.Key.Item2, nthDay);
                    if (
                        STOCHD.ContainsKey(new Tuple<string, DateOnly>(Symbol, futureDate)) &&
                        STOCHK.ContainsKey(new Tuple<string, DateOnly>(Symbol, futureDate))
                        )
                    {
                        if (STOCHD[new Tuple<string, DateOnly>(Symbol, futureDate)] - STOCHK[new Tuple<string, DateOnly>(Symbol, futureDate)] > 0)
                            summary.BearOccurances.TryAdd(futureDate, Trend.GetTrend(Symbol, item.Key.Item2));
                    }
                    //3.b) In a case where future date is not available we still want track the current cross over
                    else
                    {
                        DateOnly pastDate = Utilities.GetNthTradedDate(Symbol, item.Key.Item2, -1);
                        //if (STOCHK[new Tuple<string, DateOnly>(Symbol, pastDate)] > STOCHD[new Tuple<string, DateOnly>(Symbol, pastDate)])
                        summary.BearOccurances.TryAdd(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2));

                    }
                }
            }
        }
        private new void UpdateSummary()
        {
            summary.Indicator = IndicatorType.STOCHASTIC_OSCILLATOR;
            base.UpdateSummary();
        }
    }
}
