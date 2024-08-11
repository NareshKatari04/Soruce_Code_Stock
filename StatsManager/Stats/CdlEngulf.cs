using TicTacTec.TA.Library;

namespace StatsManager
{
    public class CdlEngulf : Stat
    {
        //Static data
        public static Dictionary<Tuple<string, DateOnly>, double> ENGULF = new Dictionary<Tuple<string, DateOnly>, double>();

        //Constructors
        public CdlEngulf(string symbol) : base(symbol)
        {
            summary.Indicator = IndicatorType.CDL_ENGULF;
            OptInPeriod = 4;
            TypeOfPrice = PriceType.ClosePrice;
        }
        public CdlEngulf(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price)
        {
            summary.Indicator = IndicatorType.CDL_ENGULF;
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
            List<float> inRealOpen = GetInputPriceList(PriceType.OpenPrice).ConvertAll(x => ((float)x));    //inRealOpen.Reverse();
            List<float> inRealHigh = GetInputPriceList(PriceType.HighPrice).ConvertAll(x => ((float)x));    //inRealHigh.Reverse();
            List<float> inRealLow = GetInputPriceList(PriceType.LowPrice).ConvertAll(x => ((float)x));      //inRealLow.Reverse();
            List<float> inRealClose = GetInputPriceList(PriceType.ClosePrice).ConvertAll(x => ((float)x));  //inRealClose.Reverse();

            int begIdx = 0;
            int endIdx = inRealClose.Count - 1;
            int outBegIdx = 0;
            int outNBElement = 0;
            int[] outInt = new int[endIdx - begIdx + 1];

            Core.CdlEngulfing(begIdx, endIdx, inRealOpen.ToArray(), inRealHigh.ToArray(), inRealLow.ToArray(), inRealClose.ToArray(), out outBegIdx, out outNBElement, outInt);
            Core.CdlEngulfingLookback();
            //Core.CdlDoji(begIdx, endIdx, inRealOpen.ToArray(), inRealHigh.ToArray(), inRealLow.ToArray(), inRealClose.ToArray(), out outBegIdx, out outNBElement, outInt);

            for (int i = 0; i < outNBElement; i++)
            {
                if (outInt[i] == 100 || outInt[i] == -100)
                {
                    //Console.WriteLine("DEBUG: {0} {1} {2}", Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE), outInt[i]);
                    ENGULF.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outInt[i]);
                }
            }
        }
        private void IdentifyPatterns()
        {
            //1. For each Rsi captured
            foreach (var item in ENGULF)
            {
                //2. Rsi below 30 is bull
                if (item.Value == -100)
                {
                    summary.BearOccurances.TryAdd(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                }
                else if (item.Value == 100)
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
