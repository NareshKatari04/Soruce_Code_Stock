using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class Rsi : Stat
    {
        //Static data
        public static Dictionary< Tuple<string, DateOnly>, double> RSI = new Dictionary<Tuple<string, DateOnly>, double>();

        //Constructors
        public Rsi(string symbol) : base(symbol)
        {
            OptInPeriod = 14;
            TypeOfPrice = PriceType.ClosePrice;
        }
        public Rsi(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }

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
            List<double> inReal = GetInputPriceList(TypeOfPrice);

            int begIdx = 0;
            int endIdx = inReal.Count - 1;
            int outBegIdx = 0;
            int outNBElement = 0;
            double[] outReal = new double[endIdx - begIdx + 1];

            Core.Rsi(begIdx, endIdx, inReal.ToArray(), OptInPeriod, out outBegIdx, out outNBElement, outReal);
            
            for (int i = 0; i < outNBElement; i++)
            {
                RSI.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outReal[i]);
            }
        }
        private void IdentifyPatterns()
        {
            //1. For each Rsi captured
            foreach (var item in RSI)
            {
                //2. Rsi below 30 is bull
                if (item.Value < 20)
                {
                    summary.BullOccurances.TryAdd(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                    //Console.WriteLine("DEBUG: RSI: Bull: symbol={0}|date={1}", Symbol, item.Key.Item2);
                }
                else if (item.Value > 70)
                {
                    summary.BearOccurances.TryAdd(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInPeriod));
                    //Console.WriteLine("DEBUG: RSI: Bear: symbol={0}|date={1}", Symbol, item.Key.Item2);
                }
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.RSI;
            base.UpdateSummary();
        }
    }
}
