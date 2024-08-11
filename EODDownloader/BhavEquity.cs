using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EODDownloader
{
    public class BhavEquity
    {
        public DateTime TradDt;
        public DateTime BizDt;
        public string Sgmt;
        public string Src;
        public string FinInstrmTp;
        public string FinInstrmId;
        public string ISIN;
        public string TckrSymb;
        public string SctySrs;
        public string XpryDt;
        public string FininstrmActlXpryDt;
        public string StrkPric;
        public string OptnTp;
        public string FinInstrmNm;
        public double OpnPric;
        public double HghPric;
        public double LwPric;
        public double ClsPric;
        public double LastPric;
        public double PrvsClsgPric;
        public double UndrlygPric;
        public double SttlmPric;
        public double OpnIntrst;
        public double ChngInOpnIntrst;
        public double TtlTradgVol;
        public double TtlTrfVal;
        public int TtlNbOfTxsExctd;
        public string SsnId;
        public double NewBrdLotQty;
        public string Rmks;
        public string Rsvd1;
        public string Rsvd2;
        public string Rsvd3;
        public string Rsvd4;


        /*Constructors*/
        public BhavEquity() { }

        public BhavEquity(string[] Field)
        {
            int i = 0;

            TradDt = DateTime.Parse(Field[i++]).Date;
            i++;//BizDt = DateTime.Parse(Field[i++]).Date;
            i++;//Sgmt = Field[i++];
            i++;// Src = Field[i++];
            i++;// FinInstrmTp = Field[i++];
            i++;// FinInstrmId = Field[i++];
            ISIN = Field[i++];
            TckrSymb = Field[i++];
            SctySrs = Field[i++];
            i++;// XpryDt = Field[i++];
            i++;// FininstrmActlXpryDt = Field[i++];
            i++;// StrkPric = Field[i++];
            i++;// OptnTp = Field[i++];
            i++;// FinInstrmNm = Field[i++];
            OpnPric = Convert.ToDouble(Field[i++]);
            HghPric = Convert.ToDouble(Field[i++]);
            LwPric = Convert.ToDouble(Field[i++]);
            ClsPric = Convert.ToDouble(Field[i++]);
            LastPric = Convert.ToDouble(Field[i++]);
            PrvsClsgPric = Convert.ToDouble(Field[i++]);
            i++;// UndrlygPric = 0;
            i++;// SttlmPric;
            i++;// OpnIntrst;
            i++;// ChngInOpnIntrst;
            TtlTradgVol = Convert.ToDouble(Field[i++]);
            TtlTrfVal = Convert.ToDouble(Field[i++]);
            TtlNbOfTxsExctd = Convert.ToInt32(Field[i++]);
            i++;// SsnId;
            i++;// NewBrdLotQty;
            i++;// Rmks;
            i++;// Rsvd1;
            i++;// Rsvd2;
            i++;// Rsvd3;
            i++;// Rsvd4;
        }
    }

}
