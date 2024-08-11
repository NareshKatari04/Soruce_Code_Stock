using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsManager
{
    public enum PriceType
    {
        None,
        OpenPrice,
        HighPrice,
        LowPrice,
        ClosePrice,
        DayAveragePrice,
        Volume
    }

    public enum Candle
    {
        None,
        Day,
        Week,
        Month,
        Year
    }
}
