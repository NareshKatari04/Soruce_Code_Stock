using StatsManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace EquityManager
{
    public class CdlThreeWhiteSoldiers : Stat
    {
        //Static data
        public static Dictionary<Tuple<string, DateOnly>, double> THREE_WHITE_SOLDIERS = new Dictionary<Tuple<string, DateOnly>, double>();

        //Constructors
        public CdlThreeWhiteSoldiers(string symbol) : base(symbol)
        {
            summary.Indicator = IndicatorType.CDL_THREE_WHITE_SOLDIERS;
            OptInPeriod = 4;
            TypeOfPrice = PriceType.ClosePrice;
        }
        public CdlThreeWhiteSoldiers(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) 
        {
            summary.Indicator = IndicatorType.CDL_THREE_WHITE_SOLDIERS;
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
            List<double> inRealOpen = GetInputPriceList(PriceType.OpenPrice);
            List<double> inRealHigh = GetInputPriceList(PriceType.HighPrice);
            List<double> inRealLow = GetInputPriceList(PriceType.LowPrice);
            List<double> inRealClose = GetInputPriceList(PriceType.ClosePrice);

            int begIdx = 0;
            int endIdx = inRealClose.Count - 1;
            int outBegIdx = 0;
            int outNBElement = 0;
            int[] outInt = new int[endIdx - begIdx + 1];

            Core.Cdl3WhiteSoldiers(begIdx, endIdx, inRealOpen.ToArray(), inRealHigh.ToArray(), inRealLow.ToArray(), inRealClose.ToArray(), out outBegIdx, out outNBElement, outInt);

            for (int i = 0; i < outNBElement; i++)
            {
                if (outInt[i] == 100 || outInt[i] == -100)
                    THREE_WHITE_SOLDIERS.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outInt[i]);
            }
        }
        private void IdentifyPatterns()
        {
            //1. For each Rsi captured
            foreach (var item in THREE_WHITE_SOLDIERS)
            {
                //2. Rsi below 30 is bull
                if (item.Value == 100)
                {
                    summary.BullOccurances.TryAdd(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
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
    }
}
