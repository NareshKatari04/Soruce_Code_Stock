using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class CdlAbandonBaby : Stat
    {
        //Static data
        public static Dictionary<Tuple<string, DateOnly>, double> ABANDON_BABY = new Dictionary<Tuple<string, DateOnly>, double>();

        private double OptInPenetration;

        //Constructors
        public CdlAbandonBaby(string symbol) : base(symbol)
        {
            summary.Indicator = IndicatorType.CDL_ABANDON_BABY;
            OptInPeriod = 4;
            OptInPenetration = (-4e+37);
            TypeOfPrice = PriceType.ClosePrice;
        }
        public CdlAbandonBaby(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) 
        {
            summary.Indicator = IndicatorType.CDL_ABANDON_BABY;
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
            List<float> inRealOpen = Array.ConvertAll(GetInputPriceList(PriceType.OpenPrice).ToArray(), new Converter<double, float>(doubleToFloat)).ToList();
            List<float> inRealHigh = Array.ConvertAll(GetInputPriceList(PriceType.HighPrice).ToArray(), new Converter<double, float>(doubleToFloat)).ToList();
            List<float> inRealLow = Array.ConvertAll(GetInputPriceList(PriceType.LowPrice).ToArray(), new Converter<double, float>(doubleToFloat)).ToList();
            List<float> inRealClose = Array.ConvertAll(GetInputPriceList(PriceType.ClosePrice).ToArray(), new Converter<double, float>(doubleToFloat)).ToList();

            int begIdx = 0;
            int endIdx = inRealClose.Count - 1;
            int outBegIdx = 0;
            int outNBElement = 0;
            int[] outInt = new int[endIdx - begIdx + 1];

            Core.CdlAbandonedBaby(begIdx, endIdx, inRealOpen.ToArray(), inRealHigh.ToArray(), inRealLow.ToArray(), inRealClose.ToArray(), OptInPenetration, out outBegIdx, out outNBElement, outInt);

            for (int i = 0; i < outNBElement; i++)
            {
                if (outInt[i] == 100 || outInt[i] == -100 )
                    ABANDON_BABY.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outInt[i]);
            }
        }
        private void IdentifyPatterns()
        {
            //1. For each Rsi captured
            foreach (var item in ABANDON_BABY)
            {
                //2. Rsi below 30 is bull
                if (item.Value == 100)
                {
                    summary.BullOccurances.TryAdd(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                    //Console.WriteLine("DEBUG: {0} {1} {2}", Symbol, item.Key.Item2);
                }
                else if( item.Value == -100)
                {
                    summary.BullOccurances.TryAdd(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                }
            }
        }
        private void UpdateSummary()
        {
            base.UpdateSummary();
        }

        private float doubleToFloat(double d)
        {
            return (float)d;
        }
    }
}
