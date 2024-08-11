using EODDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class Macd : Stat
    {
        /* Data members */
        private int OptInFastPeriod;
        private int OptInSlowPeriod;
        private int OptInSignalPeriod;
        private Dictionary<Tuple<string, DateOnly>, double> macd = new Dictionary<Tuple<string, DateOnly>, double>();
        private Dictionary<Tuple<string, DateOnly>, double> macdSignal = new Dictionary<Tuple<string, DateOnly>, double>();
        private Dictionary<Tuple<string, DateOnly>, double> macdHistory = new Dictionary<Tuple<string, DateOnly>, double>();
        
        public Dictionary<DateOnly, int> MACD_CROSS_BULL = new Dictionary<DateOnly, int>();
        public Dictionary<DateOnly, int> MACD_CROSS_BEAR = new Dictionary<DateOnly, int>();

        public Macd(string symbol) : base(symbol)
        {
            Symbol = symbol;
            OptInFastPeriod = 12;
            OptInSlowPeriod = 26;
            OptInSignalPeriod = 6;
        }
        public Macd(string symbol, int optInFastPeriod, int optInSlowPeriod, int optInSignalPeriod) : base(symbol)
        {
            Symbol = symbol;
            OptInFastPeriod = optInFastPeriod;
            OptInSlowPeriod = optInSlowPeriod;
            OptInSignalPeriod = optInSignalPeriod;
        }
        public void Process()
        {
            Calculate();
            IdentifyPatterns();
            UpdateSummary();
        }
        private void Calculate()
        {
            //1. Prepare input parameters
            List<double> inReal = GetInputPriceList(TypeOfPrice);
            int begIdx = 0;
            int endIdx = inReal.Count - 1;

            //3. outMACD, outMACDSignal, outMACDHist, outReal
            double[] outMACD = new double[inReal.Count];
            double[] outMACDSignal = new double[inReal.Count];
            double[] outMACDHist = new double[inReal.Count];

            int outBegIdx = 0;
            int outNBElement = 0;

            Core.Macd(begIdx, endIdx, inReal.ToArray(), OptInFastPeriod, OptInSlowPeriod, OptInSignalPeriod, out outBegIdx, out outNBElement, outMACD, outMACDSignal, outMACDHist);

            for (int i = 0; i < outNBElement; i++)
            {
                macd.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outMACD[i]);
                macdSignal.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outMACDSignal[i]);
                macdHistory.Add(new Tuple<string, DateOnly>(Symbol, DateOnly.FromDateTime(equities[outBegIdx + i].DATE)), outMACDHist[i]);
            }
        }
        private void IdentifyPatterns()
        {
            //1. For each signal value captured
            foreach (KeyValuePair< Tuple<string, DateOnly>, double> item in macdSignal)
            {
                //1. Cross occured
                if( macdSignal.ContainsKey(item.Key) && 
                    macd.ContainsKey(item.Key) &&
                    0 == (int) macdSignal[item.Key] - (int) macd[item.Key]
                    ) 
                {
                    //2. Check if previous histogram exists
                    Tuple<string, DateOnly> previousDayKey =new Tuple<string, DateOnly>(item.Key.Item1, item.Key.Item2.AddDays(-1 * OptInSignalPeriod));
                    if( macdHistory.ContainsKey( previousDayKey ) )
                    {
                        //3. Bull cross occurance
                        if (macdHistory[previousDayKey] < 0 )
                        {
                            summary.BullOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInSignalPeriod));
                            //Console.WriteLine("DEBUG: BULL {0} signal={1} macd={2}", item.Key.Item2, macdSignal[item.Key], macd[item.Key]);
                        }
                        //4. Bear cross occurance
                        else if (macdHistory[previousDayKey] > 0)
                        {
                            summary.BearOccurances.Add(item.Key.Item2, Trend.GetTrend(Symbol, item.Key.Item2, OptInSignalPeriod));
                            //Console.WriteLine("DEBUG: BEAR {0} signal={1} macd={2}", item.Key.Item2, macdSignal[item.Key], macd[item.Key]);
                        }
                    }
                }
            }
        }
        private void UpdateSummary()
        {
            summary.Indicator = IndicatorType.MACD;
            base.UpdateSummary();
        }
    }

}
