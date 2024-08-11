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
    public class ParabolicSAR : Stat
    {
        private enum DotPlace
        {
            None,
            AboveTheCandle,
            BelowThecandle
        }
        public Dictionary<Tuple<string, DateOnly>, double> SAR = new Dictionary<Tuple<string, DateOnly>, double>();
        public ParabolicSAR(string symbol, Candle cdl = Candle.Day) : base(symbol, cdl) { OptInPeriod = 4; } //Default value
        public ParabolicSAR(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }
        public void Process()
        {
            Calculate();
            IdentifyPatterns();
            UpdateSummary();
        }
        public void Calculate()
        {
            //1. Prepare the input parameters
            List<double> inRealOpen = GetInputPriceList(PriceType.OpenPrice);
            List<double> inRealLow = GetInputPriceList(PriceType.LowPrice);
            List<double> inRealHigh = GetInputPriceList(PriceType.HighPrice);
            List<double> inRealClose = GetInputPriceList(PriceType.ClosePrice);

            int begIdx = 0;
            int endIdx = inRealClose.Count - 1;
            int outBegIdx = 0;
            int outNBElement = 0;
            double[] outReal = new double[endIdx - begIdx + 1];

            Core.Sar(begIdx, endIdx, inRealHigh.ToArray(), inRealLow.ToArray(), 0.02, 0.2, out outBegIdx, out outNBElement, outReal);

            for (int i = 0; i < outNBElement; i++)
            {
                SAR.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outReal[i]);
            }
        }
        private void IdentifyPatterns()
        {
            DotPlace previousDotPlace = DotPlace.None;
            DotPlace currentDotPlace = DotPlace.None;

            Equity prevEquity = new Equity(); 
            Equity currEquity = new Equity();

            //1. PREV Equity is the first one in SAR dictionary
            if (candle == Candle.Day)
                prevEquity = History.HISTORY[SAR.Keys.First()];
            else if (candle == Candle.Week)
            {
                DateOnly dt = Utilities.GetLastDayOfWeek(Symbol, SAR.ElementAt(0).Key.Item2);
                prevEquity = WISTORY[new Tuple<string, DateOnly>(Symbol, dt)];
            }

            //2. Detect the previous day's dot placement
            if (prevEquity.HIGHPRICE < SAR.ElementAt(0).Value)
            {
                //4.a) DOT is above the candle
                previousDotPlace = DotPlace.AboveTheCandle;
            }
            else if (prevEquity.LOWPRICE > SAR.ElementAt(0).Value)
            {
                //4.b) DOT is below the candle
                previousDotPlace = DotPlace.BelowThecandle;
            }

            //2. Loop through all SAR values
            for (int i = 1; i < SAR.Count; i++)
            {
                //3. Current equity object
                if( candle == Candle.Day)
                    currEquity = History.HISTORY[ SAR.ElementAt(i).Key ];
                else if( candle == Candle.Week)
                {
                    DateOnly dt = Utilities.GetLastDayOfWeek(Symbol, SAR.ElementAt(i).Key.Item2);
                    currEquity = WISTORY[new Tuple<string, DateOnly>(Symbol, dt)];
                }

                //5. Detect the current day's dot placement
                if (currEquity.HIGHPRICE < SAR.ElementAt(i).Value)
                {
                    //5.a) Dot is above the candle
                    currentDotPlace = DotPlace.AboveTheCandle;
                }
                else if (currEquity.LOWPRICE > SAR.ElementAt(i).Value)
                {
                    //5.b) Dot is below the candle
                    currentDotPlace= DotPlace.BelowThecandle;
                }

                //BULLISH pattern
                if (previousDotPlace == DotPlace.BelowThecandle && currentDotPlace == DotPlace.AboveTheCandle
                    )
                {
                    summary.BearOccurances.Add(DateOnly.FromDateTime(currEquity.DATE), Trend.GetTrend(Symbol, DateOnly.FromDateTime(currEquity.DATE), OptInPeriod));
                }
                //BEARISH pattern
                else if (previousDotPlace == DotPlace.AboveTheCandle && currentDotPlace == DotPlace.BelowThecandle)
                {
                    summary.BullOccurances.Add(DateOnly.FromDateTime(currEquity.DATE), Trend.GetTrend(Symbol, DateOnly.FromDateTime(currEquity.DATE), OptInPeriod));
                }

                //Last step in iteration
                prevEquity = currEquity;
                previousDotPlace = currentDotPlace;
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.SAR;
            base.UpdateSummary();
        }
    }
}
