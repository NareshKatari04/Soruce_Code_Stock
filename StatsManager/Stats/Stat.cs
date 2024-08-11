using EODDownloader;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace StatsManager
{
    public class Stat
    {
        /* Static members */
        public static Dictionary< Tuple<string, IndicatorType>, Stat> STATS = new Dictionary< Tuple<string, IndicatorType>, Stat>();

        /* Data Fields */
        public List<Equity> equities;

        public List<Equity> wequities;

        public Dictionary<Tuple<string, DateOnly>, Equity> WISTORY = new Dictionary<Tuple<string, DateOnly>, Equity>();
        public string Symbol { get; set; }
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public int OptInPeriod { get; set; }
        public int UpperLimit { get; set; }
        public int LowerLimit { get; set; }

        public Candle candle;
        public PriceType TypeOfPrice { get; set; }
        public Summary summary { get; set; }
        /* Constructor */
        public Stat(string symbol, Candle cdl=Candle.Day) 
        { 
            Symbol = symbol;
            From = DateOnly.Parse("01JAN2023");
            To = DateOnly.FromDateTime( DateTime.Today );
            OptInPeriod = 14;
            TypeOfPrice = PriceType.ClosePrice;
            candle = cdl;

            summary = new Summary();
            equities = new List<Equity>(History.HData.Where(x => (x.SYMBOL == Symbol &&
                                                DateOnly.FromDateTime(x.DATE) >= From &&
                                                DateOnly.FromDateTime(x.DATE) <= To)).ToList<Equity>());

            wequities = PrepareWeeklyCharts(equities);
        }
        /* Constructor */
        public Stat(string symbol, DateOnly from, DateOnly to, int optInPeriod, PriceType priceType)
        {
            Symbol = symbol;
            From = from;
            To = to;
            OptInPeriod = optInPeriod;
            TypeOfPrice = priceType;
            summary = new Summary();
            equities = new List<Equity>();
            equities = History.HData.Where(x => (x.SYMBOL == Symbol &&
                                                DateOnly.FromDateTime(x.DATE) >= From &&
                                                DateOnly.FromDateTime(x.DATE) <= To)).ToList<Equity>();
            wequities = PrepareWeeklyCharts(equities);
        }
        public List<double> GetVolumeList()
        {
            List<double> volume = new List<double>();
            List<Equity> eq = new List<Equity>();
            switch (candle)
            {
                case Candle.Week:
                    eq = wequities.Where(x => (x.SYMBOL == Symbol &&
                                                DateOnly.FromDateTime(x.DATE) >= From &&
                                                DateOnly.FromDateTime(x.DATE) <= To)).ToList<Equity>();
                    break;
                default:
                    eq = equities.Where(x => (x.SYMBOL == Symbol &&
                                                DateOnly.FromDateTime(x.DATE) >= From &&
                                                DateOnly.FromDateTime(x.DATE) <= To)).ToList<Equity>();
                    break;
            }

            return eq.Select(x => (double) x.TOTTRDQTY).ToList();
        }
        public List<double> GetInputPriceList(PriceType priceType, Candle _candle=Candle.Day)
        {
            List<double> inReal = new List<double>();
            List<Equity> eq = new List<Equity>();

            switch (_candle)
            {
                case Candle.Week:
                    eq = wequities.Where(x => (x.SYMBOL == Symbol &&
                                                DateOnly.FromDateTime(x.DATE) >= From &&
                                                DateOnly.FromDateTime(x.DATE) <= To)).ToList<Equity>();
                    break;
                default:
                    eq = equities.Where(x => (x.SYMBOL == Symbol &&
                                                DateOnly.FromDateTime(x.DATE) >= From &&
                                                DateOnly.FromDateTime(x.DATE) <= To)).ToList<Equity>();
                    break;
            }
            switch (priceType)
            {
                case PriceType.OpenPrice:
                    inReal = eq.Select(x => x.OPENPRICE).ToList();
                    break;
                case PriceType.LowPrice:
                    inReal = eq.Select(x => x.LOWPRICE).ToList();
                    break;
                case PriceType.HighPrice:
                    inReal = eq.Select(x => x.HIGHPRICE).ToList();
                    break;
                case PriceType.ClosePrice:
                    inReal = eq.Select(x => x.CLOSEPRICE).ToList();
                    break;
                case PriceType.Volume:
                    inReal = eq.Select(x => x.TOTTRDVAL).ToList();
                    break;
                case PriceType.DayAveragePrice:
                    List<double> inRealOP = new List<double>();
                    inRealOP = eq.Select(x => x.OPENPRICE).ToList();

                    List<double> inRealLP = new List<double>();
                    inRealLP = eq.Select(x => x.LOWPRICE).ToList();

                    List<double> inRealHP = new List<double>();
                    inRealHP = eq.Select(x => x.HIGHPRICE).ToList();

                    List<double> inRealCP = new List<double>();
                    inRealCP = eq.Select(x => x.CLOSEPRICE).ToList();

                    var DayAvg = (double[] op, double[] hp, double[] lp, double[] cp) =>
                    {
                            double[] outReal = new double[op.Length];
                            for (int i = 0; i < op.Length; i++)
                            {
                                outReal[i] = (op[i] + hp[i] + lp[i] + cp[i]) / 4;
                            }
                            return outReal;
                    };
                    inReal = DayAvg(inRealOP.ToArray(), inRealHP.ToArray(), inRealLP.ToArray(), inRealCP.ToArray()).ToList();
                    break;
            }
            return inReal;
        }
        protected void UpdateSummary()
        {
            //Console.WriteLine("INFO: {0} {1}", Symbol, summary.Indicator );
            int numBullOccurances = summary.BullOccurances.Count();
            int numBullSuccessful = summary.BullOccurances.Where(x => x.Value == 100).ToList().Count();

            int numBearOccurances = summary.BearOccurances.Count();
            int numBearSuccessful = summary.BearOccurances.Where(x => x.Value == -100).ToList().Count();

            summary.Symbol = Symbol;
            summary.Price = TypeOfPrice;
            summary.NumBullOccurances = numBullOccurances;
            summary.NumBearOccurances = numBearOccurances;
            summary.BullSuccessRatio = numBullOccurances != 0 ? (numBullSuccessful * 100) / numBullOccurances : 0;
            summary.BearSuccessRatio = numBearOccurances != 0 ? (numBearSuccessful * 100) / numBearOccurances : 0;

            summary.LastDateBullOccured = numBullOccurances > 0 ? summary.BullOccurances.Last().Key : null;
            summary.LastDateBearOccured = numBearOccurances > 0 ? summary.BearOccurances.Last().Key : null;

            STATS.Add(new Tuple<string, IndicatorType>(Symbol, summary.Indicator), this);
        }
        //Finds if  two lines on a graph makes a cross !
        bool IsCross(double line1Val1, double line1Val2, double line2Val1, double line2Val2)
        {
            bool crossFound = false;

            double firstDifference = Math.Abs(line1Val1 - line2Val1);
            double secondDiffrence = Math.Abs(line1Val2 - line2Val2);

            //TODO
            
            return crossFound;
        }
        public List<Equity> PrepareWeeklyCharts(List<Equity> src)
        {
            List<Equity> dst = new List<Equity>();

            Equity e = new Equity();

            bool weekOver = true;

            for (int i = 0; i < src.Count; i++)
            {
                if (Utilities.GetLastDayOfWeek(Symbol, DateOnly.FromDateTime(src[i].DATE)) == DateOnly.FromDateTime(src[i].DATE))
                {
                    if (i != 0)
                    {
                        e.LOWPRICE = src[i].LOWPRICE < e.LOWPRICE ? src[i].LOWPRICE : e.LOWPRICE;
                        e.HIGHPRICE = src[i].HIGHPRICE > e.HIGHPRICE ? src[i].HIGHPRICE : e.HIGHPRICE;
                        e.CLOSEPRICE = src[i].CLOSEPRICE;
                        e.TOTTRDQTY += src[i].TOTTRDQTY;
                        e.TOTALTRADES += src[i].TOTALTRADES;
                        e.TOTTRDVAL += src[i].TOTTRDVAL;
                        e.DATE = src[i].DATE;
                    }
                    else
                    {
                        e = new Equity();
                        e.SYMBOL = src[0].SYMBOL;
                        e.SERIES = src[0].SERIES;
                        e.OPENPRICE = src[0].OPENPRICE;
                        e.LOWPRICE = src[0].LOWPRICE;
                        e.HIGHPRICE = src[0].HIGHPRICE;
                        e.CLOSEPRICE = src[0].CLOSEPRICE;
                        e.TOTTRDQTY = src[0].TOTTRDQTY;
                        e.TOTTRDVAL = src[0].TOTTRDVAL;
                        e.TOTALTRADES = src[0].TOTALTRADES;
                        e.DATE = src[0].DATE;
                    }

                    WISTORY.Add(new Tuple<string, DateOnly>(e.SYMBOL, DateOnly.FromDateTime(e.DATE)), e);
                    dst.Add(e);
                    //The last day of week's update over here
                    weekOver = true;
                }
                else
                {
                    if (weekOver)
                    {
                        e = new Equity();
                        e.SYMBOL = src[0].SYMBOL;
                        e.SERIES = src[0].SERIES;
                        e.OPENPRICE = src[0].OPENPRICE;
                        e.LOWPRICE = src[0].LOWPRICE;
                        e.HIGHPRICE = src[0].HIGHPRICE;
                        e.CLOSEPRICE = src[0].CLOSEPRICE;
                        e.TOTTRDQTY = src[0].TOTTRDQTY;
                        e.TOTTRDVAL = src[0].TOTTRDVAL;
                        e.TOTALTRADES = src[0].TOTALTRADES;
                        e.DATE = src[0].DATE;

                        //It is a new week
                        weekOver = false;
                    }
                    else
                    {
                        e.LOWPRICE = src[i].LOWPRICE < e.LOWPRICE ? src[i].LOWPRICE : e.LOWPRICE;
                        e.HIGHPRICE = src[i].HIGHPRICE > e.HIGHPRICE ? src[i].HIGHPRICE : e.HIGHPRICE;
                        e.CLOSEPRICE = src[i].CLOSEPRICE;
                        e.TOTTRDQTY += src[i].TOTTRDQTY;
                        e.TOTALTRADES += src[i].TOTALTRADES;
                        e.TOTTRDVAL += src[i].TOTTRDVAL;
                        e.DATE = src[i].DATE;

                        //It is a continution of the week
                    }
                }//End of if branch
            }//End of loop

            return dst;
        } // End of method
    }// End of class
} //End of namespace
