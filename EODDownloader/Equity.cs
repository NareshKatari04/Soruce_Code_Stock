using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EODDownloader
{
    public class Equity
    {
        public string SYMBOL { get; set; }
        public string SERIES { get; set; }
        public double OPENPRICE { get; set; }
        public double HIGHPRICE { get; set; }
        public double LOWPRICE { get; set; }
        public double CLOSEPRICE { get; set; }
        public double LASTTRADEDPRICE { get; set; }
        public double PREVIOUSCLOSEPRICE { get; set; }
        public double TOTTRDQTY { get; set; }
        public double TOTTRDVAL { get; set; }
        public DateTime DATE { get; set; }
        public int TOTALTRADES { get; set; }
        public string ISIN { get; set; }
        public string? NULLKYE { get; set; }

        /*Constructors*/
        public Equity() { }

        public Equity(string[] Field) 
        {
            int i = 0;

            SYMBOL = Field[i++];
            SERIES = Field[i++];
            OPENPRICE = Convert.ToDouble(Field[i++]);
            HIGHPRICE = Convert.ToDouble(Field[i++]);
            LOWPRICE = Convert.ToDouble(Field[i++]);
            CLOSEPRICE = Convert.ToDouble(Field[i++]);
            LASTTRADEDPRICE = Convert.ToDouble(Field[i++]);
            PREVIOUSCLOSEPRICE = Convert.ToDouble(Field[i++]);
            TOTTRDQTY = Convert.ToDouble(Field[i++]);
            TOTTRDVAL = Convert.ToDouble(Field[i++]);
            DATE = DateTime.Parse(Field[i++]).Date;
            TOTALTRADES = Convert.ToInt32(Field[i++]);
            ISIN = Field[i++];
            NULLKYE = Field[i++];
        }

        public Equity(BhavEquity bhavEquity)
        {
            SYMBOL = bhavEquity.TckrSymb;
            SERIES = bhavEquity.SctySrs;
            OPENPRICE = bhavEquity.OpnPric;
            HIGHPRICE = bhavEquity.HghPric;
            LOWPRICE = bhavEquity.LwPric;
            CLOSEPRICE = bhavEquity.ClsPric;
            LASTTRADEDPRICE = bhavEquity.LastPric;
            PREVIOUSCLOSEPRICE = bhavEquity.PrvsClsgPric;
            TOTTRDQTY = bhavEquity.TtlTradgVol;
            TOTTRDVAL = bhavEquity.TtlTrfVal;
            DATE = bhavEquity.TradDt;
            TOTALTRADES = bhavEquity.TtlNbOfTxsExctd;
            ISIN = bhavEquity.ISIN;
            NULLKYE = null;
        }
    }
}
