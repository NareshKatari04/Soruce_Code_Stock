using EODDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace StatsManager
{
    public class Miscellaneous : Stat
    {
        private double WIDTH_GREEN, WIDTH_RED;
        private double HEIGHT_GREEN, HEIGHT_RED;
        private double DEPTH_GREEN, DEPTH_RED;

        public double ROC, ROCP, ROCR, ROCR100, 
            POPEN_GREEN, POPEN_RED, 
            PCLOSE_GREEN, PCLOSE_RED, 
            PHIGH_GREEN, PHIGH_RED, 
            PLOW_GREEN, PLOW_RED;

        public Miscellaneous(string symbol) : base(symbol)
        {
            OptInPeriod = 4;
            TypeOfPrice = PriceType.ClosePrice;
        }
        public Miscellaneous(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType price) : base(symbol, from, to, optInPeriod, price) { }
        public void Process()
        {
            //1. As a first step I need trend to calculate the miscellaneous stats
            Trend trend = new Trend(Symbol);
            trend.Process();

            //2. Calculate the miscellaneous stats here in
            Calculate(trend);

            //3. Finally update all the miscellaneous stats to summarize
            UpdateSummary();
        }
        public void Calculate(Trend trend)
        {
            //1. Prepare the input parameters
            //HEADS-UP : The begin index is not zero, as ROC makes sense for the recent period
            List<double> inRealClose = GetInputPriceList(TypeOfPrice);
            int begIdx = inRealClose.Count - 20; 
            int endIdx = inRealClose.Count - 1;
            int outBegIdx = 0;
            int outNBElement = 0;
            double[] outReal = new double[endIdx - begIdx + 1];

            DateOnly _fromDate = trend.TREND.Where(x => (x.Value == 100 || x.Value == -100 )).Last().Key.Item2;
            DateOnly _toDate = DateOnly.FromDateTime(History.LastUpdatedDate);

            //Miscellaneous roc = new Miscellaneous(Symbol, _fromDate, _toDate, OptInPeriod, PriceType.OpenPrice);
            Core.Roc(begIdx, endIdx, inRealClose.ToArray(), OptInPeriod, out outBegIdx, out outNBElement, outReal);
            ROC = outReal[outNBElement - 1];
            
            Core.RocP(begIdx, endIdx, inRealClose.ToArray(), OptInPeriod, out outBegIdx, out outNBElement, outReal);
            ROCP = outReal[outNBElement - 1];
            
            Core.RocR(begIdx, endIdx, inRealClose.ToArray(), OptInPeriod, out outBegIdx, out outNBElement, outReal);
            ROCR = outReal[outNBElement - 1];
            
            Core.RocR100(begIdx, endIdx, inRealClose.ToArray(), OptInPeriod, out outBegIdx, out outNBElement, outReal);
            ROCR100 = outReal[outNBElement - 1];

            //Console.WriteLine("INFO: {0} {1} {2} {3}", ROC, ROCP, ROCR, ROCR100 ) ;
            
            double greenOpenSum = 0, redOpenSum = 0;
            int greenOpenCount = 0, redOpenCount = 0;

            double widthGreenSum = 0, widthRedSum = 0;
            int widthGreenCount = 0, widthRedCount = 0;

            double greenHighSum = 0, redHighSum = 0;
            int redHighCount = 0, greenHighCount = 0;

            double greenDepthSum = 0, redDepthSum = 0;
            int greenDepthCount = 0, redDepthCount = 0;

            DateOnly lastDate = trend.TREND.Where(x => x.Value == -100 || x.Value == 100).Select(x => x.Key.Item2).Last();
            List<double> inOpen = History.HISTORY.Where(x => x.Key.Item1 == Symbol && x.Key.Item2 >= lastDate).Select(x => x.Value.OPENPRICE).ToList();
            List<double> inClose= History.HISTORY.Where(x => x.Key.Item1 == Symbol && x.Key.Item2 >= lastDate).Select(x => x.Value.CLOSEPRICE).ToList();
            List<double> inHigh = History.HISTORY.Where(x => x.Key.Item1 == Symbol && x.Key.Item2 >= lastDate).Select(x => x.Value.HIGHPRICE).ToList();
            List<double> inLow  = History.HISTORY.Where(x => x.Key.Item1 == Symbol && x.Key.Item2 >= lastDate).Select(x => x.Value.LOWPRICE).ToList();
            for (int i = 0; i< inClose.Count; i++)
            {
                if (i > 0)
                {
                    //Green open
                    if (inClose[i - 1] < inOpen[i])
                    {
                        greenOpenSum += inOpen[i] - inClose[i - 1];
                        greenOpenCount++;

                    }
                    //Red open
                    else
                    {
                        redOpenSum += inOpen[i - 1] - inClose[i];
                        redOpenCount++;
                    }
                }
                if (inClose[i] >= inOpen[i])
                {
                    widthGreenSum += inClose[i] - inOpen[i];
                    widthGreenCount++;

                    greenHighSum += inHigh[i] - inClose[i];
                    greenHighCount++;

                    greenDepthSum += inOpen[i] - inLow[i];
                    greenDepthCount++;
                }
                else if (inClose[i] < inOpen[i])
                {
                    widthRedSum += inOpen[i] - inClose[i];
                    widthRedCount++;

                    redHighSum += inHigh[i] - inOpen[i];
                    redHighCount++;

                    redDepthSum += inClose[i] - inLow[i];
                    redDepthCount++;
                }
            }
            POPEN_GREEN = inClose.Last() + (greenOpenSum + greenOpenCount);
            POPEN_RED = inRealClose.Last() - (redOpenSum + redOpenCount);
            WIDTH_GREEN = widthGreenSum / widthGreenCount;
            WIDTH_RED = widthRedSum / widthRedCount;
            HEIGHT_GREEN = greenHighSum / greenHighCount;
            HEIGHT_RED = redHighSum / redHighCount;
            DEPTH_GREEN = greenDepthSum / greenDepthCount;
            DEPTH_RED = redDepthSum / redDepthCount;


        }
        public void UpdateSummary()
        {
            summary.Indicator = IndicatorType.MISC;
            summary.ProjOpenPrice[0] = POPEN_GREEN;
            summary.ProjOpenPrice[1] = POPEN_RED;

            summary.ProjClosePrice[0] = summary.ProjOpenPrice[0] + WIDTH_GREEN;
            summary.ProjClosePrice[1] = summary.ProjOpenPrice[1] + WIDTH_RED;

            summary.ProjHighPrice[0] = summary.ProjClosePrice[0] + HEIGHT_GREEN;
            summary.ProjHighPrice[1] = summary.ProjOpenPrice[1] + HEIGHT_RED;

            summary.ProjLowPrice[0] = summary.ProjClosePrice[0] - DEPTH_GREEN;
            summary.ProjLowPrice[1] = summary.ProjOpenPrice[1] - DEPTH_RED;
            
            base.UpdateSummary();
        }
    }
}
