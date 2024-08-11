using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsManager
{
    public class Summary
    {
        public string Symbol { get; set; }
        public DateOnly Date { get; set; }
        public Summary() 
        { 
            BullOccurances = new Dictionary<DateOnly, int>();  
            BearOccurances = new Dictionary<DateOnly, int>();
        }
        public Summary(string name, DateOnly date) 
        {
            Symbol = Symbol; 
            Date = date;
            BullOccurances = new Dictionary<DateOnly, int>();
            BearOccurances = new Dictionary<DateOnly, int>();
        }
        public IndicatorType Indicator { get; set; }
        public PriceType Price { get; set; }
        public Dictionary<DateOnly, int> BullOccurances { get; set; }
        public double BullSuccessRatio { get; set; }
        public int NumBullOccurances { get; set; }
        public Dictionary<DateOnly, int> BearOccurances { get; set; }
        public double BearSuccessRatio { get; set; }
        public int NumBearOccurances { get; set; }
        public DateOnly? LastDateBullOccured { get; set; }
        public DateOnly? LastDateBearOccured { get; set; }
        //Data members for Miscellaneous
        public double [] ProjOpenPrice = new double[2];
        public double [] ProjClosePrice = new double[2];
        public double [] ProjHighPrice = new double[2];
        public double [] ProjLowPrice = new double[2];

        public static Dictionary<Tuple<string, IndicatorType, DateOnly>, Summary> SUMMARY = new Dictionary<Tuple<string, IndicatorType, DateOnly>, Summary>();

    }
}
