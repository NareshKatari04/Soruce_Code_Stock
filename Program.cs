using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using EODDownloader;
using StatsManager;

namespace EquityManager
{
    public class Program
    {
        static void Main(string[] args)
        {
            //1. Update history
            History.RefreshHistory();
            
            //2. Analyze stats
            Analyse();

            //3. Summarize stats
            //Summarize();
            //QueryBuilder();
            ExportToExcel();
        }

        public static void Analyse()
        {
            List<string> derivatives = new List<string> { "APOLLOTYRE", "ASTRAL", "ABFRL", "ALKEM", "ABB", "BEL", "AUBANK", "AXISBANK", "BAJFINANCE", "BALKRISIND", "AARTIIND", "BATAINDIA", "BERGEPAINT", "ASHOKLEY", "COFORGE", "CUMMINSIND", "ABCAPITAL", "ADANIENT", "ADANIPORTS", "DIVISLAB", "AMBUJACEM", "DIXON", "APOLLOHOSP", "EICHERMOT", "GODREJCP", "GRASIM", "GUJGASLTD", "ATUL", "HCLTECH", "HDFCLIFE", "BALRAMCHIN", "BANDHANBNK", "BANKBARODA", "BHARATFORG", "IDFC", "BHARTIARTL", "BHEL", "BOSCHLTD", "BPCL", "BRITANNIA", "BSOFT", "IDFCFIRSTB", "CANFINHOME", "INDHOTEL", "CHOLAFIN", "COLPAL", "COROMANDEL", "CROMPTON", "DABUR", "DALBHARAT", "INDIGO", "DEEPAKNTR", "DELTACORP", "DLF", "INFY", "DRREDDY", "ESCORTS", "MARUTI", "FEDERALBNK", "MCDOWELL-N", "GAIL", "MFSL", "MPHASIS", "GLENMARK", "GMRINFRA", "GNFC", "NATIONALUM", "HAL", "HAVELLS", "HDFCBANK", "NAUKRI", "HEROMOTOCO", "HINDALCO", "HINDCOPPER", "HINDPETRO", "ABBOTINDIA", "IBULHSGFIN", "ICICIGI", "ICICIPRULI", "IEX", "IGL", "INDIACEM", "INDIAMART", "INDUSTOWER", "IOC", "IPCALAB", "IRCTC", "OFSS", "PAGEIND", "JKCEMENT", "PERSISTENT", "PIIND", "KOTAKBANK", "SBICARD", "SBILIFE", "L&TFH", "LALPATHLAB", "LAURUSLABS", "LICHSGFIN", "LT", "LTIM", "LTTS", "AUROPHARMA", "LUPIN", "M&M", "M&MFIN", "MARICO", "METROPOLIS", "ASIANPAINT", "BAJAJFINSV", "CIPLA", "MRF", "EXIDEIND", "MUTHOOTFIN", "NAVINFLUOR", "TCS", "TECHM", "TRENT", "TVSMOTOR", "NESTLEIND", "WIPRO", "NTPC", "OBEROIRLTY", "ONGC", "CHAMBLFERT", "PETRONET", "PIDILITIND", "PNB", "POWERGRID", "RAMCOCEM", "RBLBANK", "RECLTD", "RELIANCE", "SAIL", "SBIN", "SHREECEM", "SHRIRAMFIN", "SRF", "SUNPHARMA", "COALINDIA", "CUB", "SYNGENE", "TATACHEM", "TATACOMM", "TATACONSUM", "TATAMOTORS", "TATAPOWER", "TITAN", "TORNTPHARM", "UBL", "ULTRACEMCO", "UPL", "VOLTAS", "ZEEL", "ZYDUSLIFE", "INDUSINDBK", "ACC", "BAJAJ-AUTO", "HINDUNILVR", "BIOCON", "JINDALSTEL", "JSWSTEEL", "CONCOR", "NMDC", "POLYCAB", "SIEMENS", "MCX", "JUBLFOOD", "MGL", "MOTHERSON", "PVRINOX", "SUNTV", "ITC", "CANBK", "GODREJPROP", "GRANULES", "HDFCAMC", "ICICIBANK", "IDEA", "MANAPPURAM", "PEL", "PFC", "TATASTEEL", "VEDL" };
            List<string> govtCompanies = new List<string> { "HAVELLS", "ICICIBANK", "MCDOWELL-N", "SBILIFE", "POWERGRID", "COALINDIA", "VEDL" };
            List<string> testCompanies = new List<string> { "AARTIIND" };
            List<string> nifty200 = new List<string> { "ABB", "ACC", "APLAPOLLO", "AUBANK", "ADANIENSOL", "ADANIENT", "ADANIGREEN", "ADANIPORTS", "ADANIPOWER", "ATGL", "AWL", "ABCAPITAL", "ABFRL", "ALKEM", "AMBUJACEM", "APOLLOHOSP", "APOLLOTYRE", "ASHOKLEY", "ASIANPAINT", "ASTRAL", "AUROPHARMA", "DMART", "AXISBANK", "BAJAJ-AUTO", "BAJFINANCE", "BAJAJFINSV", "BAJAJHLDNG", "BALKRISIND", "BANDHANBNK", "BANKBARODA", "BANKINDIA", "BATAINDIA", "BERGEPAINT", "BDL", "BEL", "BHARATFORG", "BHEL", "BPCL", "BHARTIARTL", "BIOCON", "BOSCHLTD", "BRITANNIA", "CGPOWER", "CANBK", "CHOLAFIN", "CIPLA", "COALINDIA", "COFORGE", "COLPAL", "CONCOR", "COROMANDEL", "CROMPTON", "CUMMINSIND", "DLF", "DABUR", "DALBHARAT", "DEEPAKNTR", "DELHIVERY", "DEVYANI", "DIVISLAB", "DIXON", "LALPATHLAB", "DRREDDY", "EICHERMOT", "ESCORTS", "NYKAA", "FEDERALBNK", "FACT", "FORTIS", "GAIL", "GLAND", "GODREJCP", "GODREJPROP", "GRASIM", "FLUOROCHEM", "GUJGASLTD", "HCLTECH", "HDFCAMC", "HDFCBANK", "HDFCLIFE", "HAVELLS", "HEROMOTOCO", "HINDALCO", "HAL", "HINDPETRO", "HINDUNILVR", "ICICIBANK", "ICICIGI", "ICICIPRULI", "IDFCFIRSTB", "ITC", "INDIANB", "INDHOTEL", "IOC", "IRCTC", "IRFC", "IGL", "INDUSTOWER", "INDUSINDBK", "NAUKRI", "INFY", "INDIGO", "IPCALAB", "JSWENERGY", "JSWSTEEL", "JINDALSTEL", "JUBLFOOD", "KPITTECH", "KOTAKBANK", "L&TFH", "LTTS", "LICHSGFIN", "LTIM", "LT", "LAURUSLABS", "LICI", "LUPIN", "MRF", "LODHA", "M&MFIN", "M&M", "MANKIND", "MARICO", "MARUTI", "MFSL", "MAXHEALTH", "MAZDOCK", "MSUMI", "MPHASIS", "MUTHOOTFIN", "NHPC", "NMDC", "NTPC", "NAVINFLUOR", "NESTLEIND", "OBEROIRLTY", "ONGC", "OIL", "PAYTM", "POLICYBZR", "PIIND", "PAGEIND", "PATANJALI", "PERSISTENT", "PETRONET", "PIDILITIND", "PEL", "POLYCAB", "POONAWALLA", "PFC", "POWERGRID", "PRESTIGE", "PGHH", "PNB", "RECLTD", "RVNL", "RELIANCE", "SBICARD", "SBILIFE", "SRF", "MOTHERSON", "SHREECEM", "SHRIRAMFIN", "SIEMENS", "SONACOMS", "SBIN", "SAIL", "SUNPHARMA", "SUNTV", "SYNGENE", "TVSMOTOR", "TATACHEM", "TATACOMM", "TCS", "TATACONSUM", "TATAELXSI", "TATAMTRDVR", "TATAMOTORS", "TATAPOWER", "TATASTEEL", "TECHM", "RAMCOCEM", "TITAN", "TORNTPHARM", "TORNTPOWER", "TRENT", "TIINDIA", "UPL", "ULTRACEMCO", "UNIONBANK", "UBL", "MCDOWELL-N", "VBL", "VEDL", "IDEA", "VOLTAS", "WIPRO", "YESBANK", "ZEEL", "ZOMATO", "ZYDUSLIFE" };
            List<string> nifty500 = new List<string> { "360ONE", "3MINDIA", "ABB", "ACC", "AIAENG", "APLAPOLLO", "AUBANK", "AARTIDRUGS", "AARTIIND", "AAVAS", "ABBOTINDIA", "ADANIENSOL", "ADANIENT", "ADANIGREEN", "ADANIPORTS", "ATGL", "AWL", "ABCAPITAL", "ABFRL", "AEGISCHEM", "AETHER", "AFFLE", "AJANTPHARM", "APLLTD", "ALKEM", "ALKYLAMINE", "ALLCARGO", "ALOKINDS", "ARE&M", "AMBER", "AMBUJACEM", "ANGELONE", "ANURAS", "APARINDS", "APOLLOHOSP", "APOLLOTYRE", "APTUS", "ACI", "ASAHIINDIA", "ASHOKLEY", "ASIANPAINT", "ASTERDM", "ASTRAL", "ATUL", "AUROPHARMA", "AVANTIFEED", "DMART", "AXISBANK", "BEML", "BLS", "BSE", "BAJAJ-AUTO", "BAJFINANCE", "BAJAJFINSV", "BAJAJHLDNG", "BALAMINES", "BALKRISIND", "BALRAMCHIN", "BANDHANBNK", "BANKBARODA", "BANKINDIA", "MAHABANK", "BATAINDIA", "BAYERCROP", "BERGEPAINT", "BDL", "BEL", "BHARATFORG", "BHEL", "BPCL", "BHARTIARTL", "BIKAJI", "BIOCON", "BIRLACORPN", "BSOFT", "BLUEDART", "BLUESTARCO", "BBTC", "BORORENEW", "BOSCHLTD", "BRIGADE", "BCG", "BRITANNIA", "MAPMYINDIA", "CCL", "CESC", "CGPOWER", "CIEINDIA", "CRISIL", "CSBBANK", "CAMPUS", "CANFINHOME", "CANBK", "CGCL", "CARBORUNIV", "CASTROLIND", "CEATLTD", "CENTRALBK", "CDSL", "CENTURYPLY", "CENTURYTEX", "CERA", "CHALET", "CHAMBLFERT", "CHEMPLASTS", "CHOLAHLDNG", "CHOLAFIN", "CIPLA", "CUB", "CLEAN", "COALINDIA", "COCHINSHIP", "COFORGE", "COLPAL", "CAMS", "CONCORDBIO", "CONCOR", "COROMANDEL", "CRAFTSMAN", "CREDITACC", "CROMPTON", "CUMMINSIND", "CYIENT", "DCMSHRIRAM", "DLF", "DABUR", "DALBHARAT", "DATAPATTNS", "DEEPAKFERT", "DEEPAKNTR", "DELHIVERY", "DELTACORP", "DEVYANI", "DIVISLAB", "DIXON", "LALPATHLAB", "DRREDDY", "EIDPARRY", "EIHOTEL", "EPL", "EASEMYTRIP", "EICHERMOT", "ELGIEQUIP", "EMAMILTD", "ENDURANCE", "ENGINERSIN", "EPIGRAL", "EQUITASBNK", "ERIS", "ESCORTS", "EXIDEIND", "FDC", "NYKAA", "FEDERALBNK", "FACT", "FINEORG", "FINCABLES", "FINPIPE", "FSL", "FIVESTAR", "FORTIS", "GRINFRA", "GAIL", "GMMPFAUDLR", "GMRINFRA", "GALAXYSURF", "GICRE", "GILLETTE", "GLAND", "GLAXO", "GLS", "GLENMARK", "MEDANTA", "GOCOLORS", "GPIL", "GODFRYPHLP", "GODREJCP", "GODREJIND", "GODREJPROP", "GRANULES", "GRAPHITE", "GRASIM", "GESHIP", "GRINDWELL", "GUJALKALI", "GAEL", "FLUOROCHEM", "GUJGASLTD", "GNFC", "GPPL", "GSFC", "GSPL", "HEG", "HCLTECH", "HDFCAMC", "HDFCBANK", "HDFCLIFE", "HFCL", "HLEGLAS", "HAPPSTMNDS", "HAVELLS", "HEROMOTOCO", "HINDALCO", "HAL", "HINDCOPPER", "HINDPETRO", "HINDUNILVR", "HINDZINC", "POWERINDIA", "HOMEFIRST", "HONAUT", "HUDCO", "ICICIBANK", "ICICIGI", "ICICIPRULI", "ISEC", "IDBI", "IDFCFIRSTB", "IDFC", "IIFL", "IRB", "IRCON", "ITC", "ITI", "INDIACEM", "IBULHSGFIN", "INDIAMART", "INDIANB", "IEX", "INDHOTEL", "IOC", "IOB", "IRCTC", "IRFC", "INDIGOPNTS", "IGL", "INDUSTOWER", "INDUSINDBK", "INFIBEAM", "NAUKRI", "INFY", "INGERRAND", "INTELLECT", "INDIGO", "IPCALAB", "JBCHEPHARM", "JKCEMENT", "JBMA", "JKLAKSHMI", "JKPAPER", "JMFINANCIL", "JSWENERGY", "JSWSTEEL", "JAMNAAUTO", "JINDALSAW", "JSL", "JINDALSTEL", "JUBLFOOD", "JUBLINGREA", "JUBLPHARMA", "JUSTDIAL", "JYOTHYLAB", "KPRMILL", "KEI", "KNRCON", "KPITTECH", "KRBL", "KSB", "KAJARIACER", "KPIL", "KALYANKJIL", "KANSAINER", "KARURVYSYA", "KAYNES", "KEC", "KFINTECH", "KOTAKBANK", "KIMS", "L&TFH", "LTTS", "LICHSGFIN", "LTIM", "LAXMIMACH", "LT", "LATENTVIEW", "LAURUSLABS", "LXCHEM", "LEMONTREE", "LICI", "LINDEINDIA", "LUPIN", "LUXIND", "MMTC", "MRF", "MTARTECH", "LODHA", "MGL", "M&MFIN", "M&M", "MHRIL", "MAHLIFE", "MANAPPURAM", "MRPL", "MANKIND", "MARICO", "MARUTI", "MASTEK", "MFSL", "MAXHEALTH", "MAZDOCK", "MEDPLUS", "METROBRAND", "METROPOLIS", "MINDACORP", "MSUMI", "MOTILALOFS", "MPHASIS", "MCX", "MUTHOOTFIN", "NATCOPHARM", "NBCC", "NCC", "NHPC", "NLCINDIA", "NMDC", "NSLNISP", "NTPC", "NH", "NATIONALUM", "NAVINFLUOR", "NAZARA", "NESTLEIND", "NETWORK18", "NAM-INDIA", "NUVOCO", "OBEROIRLTY", "ONGC", "OIL", "OLECTRA", "PAYTM", "OFSS", "ORIENTELEC", "POLICYBZR", "PCBL", "PIIND", "PNBHOUSING", "PNCINFRA", "PVRINOX", "PAGEIND", "PATANJALI", "PERSISTENT", "PETRONET", "PFIZER", "PHOENIXLTD", "PIDILITIND", "PEL", "PPLPHARMA", "POLYMED", "POLYCAB", "POLYPLEX", "POONAWALLA", "PFC", "POWERGRID", "PRAJIND", "PRESTIGE", "PRINCEPIPE", "PRSMJOHNSN", "PGHL", "PGHH", "PNB", "QUESS", "RBLBANK", "RECLTD", "RHIM", "RITES", "RADICO", "RVNL", "RAIN", "RAINBOW", "RAJESHEXPO", "RALLIS", "RCF", "RATNAMANI", "RTNINDIA", "RAYMOND", "REDINGTON", "RELAXO", "RELIANCE", "RBA", "ROSSARI", "ROUTE", "SBICARD", "SBILIFE", "SJVN", "SKFINDIA", "SRF", "SAFARI", "MOTHERSON", "SANOFI", "SAPPHIRE", "SAREGAMA", "SCHAEFFLER", "SHARDACROP", "SFL", "SHOPERSTOP", "SHREECEM", "RENUKA", "SHRIRAMFIN", "SHYAMMETL", "SIEMENS", "SOBHA", "SOLARINDS", "SONACOMS", "SONATSOFTW", "STARHEALTH", "SBIN", "SAIL", "SWSOLAR", "STLTECH", "SUMICHEM", "SPARC", "SUNPHARMA", "SUNTV", "SUNDARMFIN", "SUNDRMFAST", "SUNTECK", "SUPRAJIT", "SUPREMEIND", "SUVENPHAR", "SWANENERGY", "SYMPHONY", "SYNGENE", "SYRMA", "TTKPRESTIG", "TV18BRDCST", "TVSMOTOR", "TANLA", "TATACHEM", "TATACOMM", "TCS", "TATACONSUM", "TATAELXSI", "TATAINVEST", "TATAMTRDVR", "TATAMOTORS", "TATAPOWER", "TATASTEEL", "TTML", "TEAMLEASE", "TECHM", "TEJASNET", "NIACL", "RAMCOCEM", "THERMAX", "TIMKEN", "TITAN", "TORNTPHARM", "TORNTPOWER", "TRENT", "TRIDENT", "TRIVENI", "TRITURBINE", "TIINDIA", "UCOBANK", "UNOMINDA", "UPL", "UTIAMC", "UJJIVANSFB", "ULTRACEMCO", "UNIONBANK", "UBL", "MCDOWELL-N", "USHAMART", "VGUARD", "VMART", "VIPIND", "VAIBHAVGBL", "VTL", "VARROC", "VBL", "MANYAVAR", "VEDL", "VIJAYA", "VINATIORGA", "IDEA", "VOLTAS", "WELCORP", "WELSPUNLIV", "WESTLIFE", "WHIRLPOOL", "WIPRO", "YESBANK", "ZFCVINDIA", "ZEEL", "ZENSARTECH", "ZOMATO", "ZYDUSLIFE", "ZYDUSWELL", "ECLERX" };
            List<string> modiStocks = new List<string> { "CONCOR", "RECLTD", "ADANIPORTS", "ADANIENT", "BEL", "HINDPETRO", "HAL", "BPCL", "LT", "PNB", "BANKBARODA", "ONGC", "BHEL", "SBIN", "POWERGRID", "HINDCOPPER", "SAIL", "NTPC", "GAIL", "AMBUJACEM", "ABB", "CANBK", "IRCTC", "ACC", "COALINDIA", "IOC", "SIEMENS", "NATIONALUM", "PETRONET", "INDUSTOWER", "NMDC", "TATAPOWER", "JINDALSTEL", "RELIANCE", "CUMMINSIND", "DALBHARAT", "ASHOKLEY", "GMRINFRA", "BHARATFORG", "POLYCAB", "IGL", "INDIACEM", "IDEA", "ULTRACEMCO", "MGL", "DIXON", "INDHOTEL", "INDIGO", "BHARTIARTL", "SHREECEM", "GUJGASLTD", "JKCEMENT", "RAMCOCEM" };
            foreach (var symbol in derivatives)
            {   
                Console.WriteLine("INFO: Analyzing {0}", symbol);

                Macd macd = new Macd(symbol);
                macd.Process();

                HigherHighLowerLow hhll = new HigherHighLowerLow(symbol);
                hhll.Process();

                SmaCross smacross = new SmaCross(symbol);
                smacross.Process();

                EmaCross ema = new EmaCross(symbol);
                ema.Process();

                Min min = new Min(symbol);
                min.Process();

                Rsi rsi = new Rsi(symbol);
                rsi.Process();

                Max max = new Max(symbol);
                max.Process();

                ParabolicSAR parabolicSAR = new ParabolicSAR(symbol);
                parabolicSAR.Process();

                Miscellaneous miscellaneous = new Miscellaneous(symbol);
                miscellaneous.Process();

                SupportAndResistance supportAndResistance = new SupportAndResistance(symbol);
                supportAndResistance.Process();

                WilliamsR williamsR = new WilliamsR(symbol);
                williamsR.Process();

                Mom mom = new Mom(symbol);
                mom.Process();

                StochasticOscillator stochasticOscillator = new StochasticOscillator(symbol);
                stochasticOscillator.Process();

                //HTSine hTSine = new HTSine(symbol);
                //hTSine.Process();

                CdlThreeBlackCrows threeBlackCrows = new CdlThreeBlackCrows(symbol);
                threeBlackCrows.Process();

                CdlThreeInsideUpDown cdlThreeInsideUpDown = new CdlThreeInsideUpDown(symbol);
                cdlThreeInsideUpDown.Process();

                CdlThreeOutSideUpDown cdlThreeOutSideUpDown = new CdlThreeOutSideUpDown(symbol);
                cdlThreeOutSideUpDown.Process();

                CdlThreeStarsSouth cdlThreeStarsSouth = new CdlThreeStarsSouth(symbol);
                cdlThreeStarsSouth.Process();

                CdlThreeWhiteSoldiers threeWhiteSoldiers = new CdlThreeWhiteSoldiers(symbol);
                threeWhiteSoldiers.Process();

                CdlAbandonBaby cdlAbandonBaby = new CdlAbandonBaby(symbol);
                cdlAbandonBaby.Process();

                CdlAdvanceBlock cdlAdvanceBlock = new CdlAdvanceBlock(symbol);
                cdlAdvanceBlock.Process();

                CdlBreakAway cdlBreakAway = new CdlBreakAway(symbol);
                cdlBreakAway.Process();

                CdlClosingMarubozu cdlClosingMarubozu = new CdlClosingMarubozu(symbol);
                cdlClosingMarubozu.Process();

                CdlConcealingBabySwallow cdlConcealingBabySwallow = new CdlConcealingBabySwallow(symbol);
                cdlConcealingBabySwallow.Process();

                CdlDarkCloudCover cdlDarkCloudCover = new CdlDarkCloudCover(symbol);
                cdlDarkCloudCover.Process();

                CdlDoji cdlDoji = new CdlDoji(symbol);
                cdlDoji.Process();

                CdlHammer cdlHammer = new CdlHammer(symbol);
                cdlHammer.Process();

                CdlHangingMan cdlHangingMan = new CdlHangingMan(symbol);
                cdlHangingMan.Process();

                CdlHarami cdlHarami = new CdlHarami(symbol);
                cdlHarami.Process();

                CdlHaramiCross cdlHaramiCross = new CdlHaramiCross(symbol);
                cdlHaramiCross.Process();

                CdlHikkake cdlHikkake = new CdlHikkake(symbol);
                cdlHikkake.Process();

                CdlModifiedHikkake cdlModifiedHikkake = new CdlModifiedHikkake(symbol);
                cdlModifiedHikkake.Process();

                CdlHomingPedion cdlHomingPedion = new CdlHomingPedion(symbol);
                cdlHomingPedion.Process();

                CdlIdenticalThreeCrows cdlIdenticalThreeCrows = new CdlIdenticalThreeCrows(symbol);
                cdlIdenticalThreeCrows.Process();

                CdlInNeck cdlInNeck = new CdlInNeck(symbol);
                cdlInNeck.Process();

                CdlInvertedHammer cdlInvertedHammer = new CdlInvertedHammer(symbol);
                cdlInvertedHammer.Process();

                CdlKicker cdlKicker = new CdlKicker(symbol);
                cdlKicker.Process();

                CdlKickerByLength cdlKickerByLength = new CdlKickerByLength(symbol);
                cdlKickerByLength.Process();

                CdlLadderBottom cdlLadderBottom = new CdlLadderBottom(symbol);
                cdlLadderBottom.Process();

                CdlLongLeggedDoji cdlLongLeggedDoji = new CdlLongLeggedDoji(symbol);
                cdlLongLeggedDoji.Process();

                CdlLongLine cdlLongLine = new CdlLongLine(symbol);
                cdlLongLine.Process();

                CdlMarbozu cdlMarbozu = new CdlMarbozu(symbol);
                cdlMarbozu.Process();

                CdlMatchingLow cdlMatchingLow = new CdlMatchingLow(symbol);
                cdlMatchingLow.Process();

                CdlMatHold cdlMatHold = new CdlMatHold(symbol);
                cdlMatHold.Process();

                CdlMorningDojiStar cdlMorningDojiStar = new CdlMorningDojiStar(symbol);
                cdlMorningDojiStar.Process();

                CdlMorningStar cdlMorningStart = new CdlMorningStar(symbol);
                cdlMorningStart.Process();

                CdlEngulf cdlEngulf = new CdlEngulf(symbol);
                cdlEngulf.Process();

                CdlOnNeck cdlOnNeck = new CdlOnNeck(symbol);
                cdlOnNeck.Process();

                CdlPiercing cdlPiercing = new CdlPiercing(symbol);
                cdlPiercing.Process();

                CdlRickshawMan cdlRickshawMan = new CdlRickshawMan(symbol);
                cdlRickshawMan.Process();

                CdlRiseFall3Methods cdlRiseFall3Methods = new CdlRiseFall3Methods(symbol);
                cdlRiseFall3Methods.Process();

                CdlSeparatingLines cdlSeparatingLines = new CdlSeparatingLines(symbol);
                cdlSeparatingLines.Process();

                CdlShootingStar cdlShootingStar = new CdlShootingStar(symbol);
                cdlShootingStar.Process();

                CdlShortLine cdlShortLine = new CdlShortLine(symbol);
                cdlShortLine.Process();

                CdlStalled cdlStalled = new CdlStalled(symbol);
                cdlStalled.Process();

                CdlStickSandwich cdlStickSandwich = new CdlStickSandwich(symbol);
                cdlStickSandwich.Process();

                CdlTakuri cdlTakuri = new CdlTakuri(symbol);
                cdlTakuri.Process();

                CdlTasukiGap cdlTasukiGap = new CdlTasukiGap(symbol);
                cdlTasukiGap.Process();

                CdlThrusting cdlThrusting = new CdlThrusting(symbol);
                cdlThrusting.Process();

                CdlTriStar cdlTriStar = new CdlTriStar(symbol);
                cdlTriStar.Process();

                CdlUnique3River cdlUnique3River = new CdlUnique3River(symbol);
                cdlUnique3River.Process();

                CdlUpsideGapTwoCrows cdlUpsideGapTwoCrows = new CdlUpsideGapTwoCrows(symbol);
                cdlUpsideGapTwoCrows.Process();

                CdlXsideGapThreeMethods cdlXsideGapThreeMethods = new CdlXsideGapThreeMethods(symbol);
                cdlXsideGapThreeMethods.Process();
            }
        }
        public static void ExportToExcel()
        {
            string path = "Stats.csv";
            StreamWriter sw = File.CreateText(path);

            sw.WriteLine(
                "\"Symbol\"," +
                "\"Indicator\"," +
                "\"Bull occurances\"," +
                "\"Bull success rate\"," +
                "\"Bear occurances\"," +
                "\"Bear success rate\"," +
                "\"Last bull date\"," +
                "\"Last bear date\"," +
                "\"Proj Open Prie_1\"," +
                "\"Proj Open Price_2\"," +
                "\"Proj Low Price_1\"," +
                "\"Proj Low Price_2\"," +
                "\"Proj High Price_1\"," +
                "\"Proj High Price_2\"," +
                "\"Proj Close Price_1\"," +
                "\"Proj Close Price_2\","
                );
            foreach (var item in Stat.STATS)
            {
                //if( item.Value.summary.Indicator == IndicatorType.MISC)
                sw.WriteLine(
                    "\"{0}\"," +
                    "\"{1}\"," +
                    "\"{2}\"," +
                    "\"{3}\"," +
                    "\"{4}\"," +
                    "\"{5}\"," +
                    "\"{6}\"," +
                    "\"{7}\"," +
                    "\"{8}\"," +
                    "\"{9}\"," +
                    "\"{10}\"," +
                    "\"{11}\"," +
                    "\"{12}\"," +
                    "\"{13}\"," +
                    "\"{14}\"," +
                    "\"{15}\",",
                    item.Key.Item1,
                    item.Value.summary.Indicator.ToString(),
                    item.Value.summary.NumBullOccurances,
                    item.Value.summary.BullSuccessRatio,
                    item.Value.summary.NumBullOccurances,
                    item.Value.summary.BearSuccessRatio,
                    item.Value.summary.LastDateBullOccured,
                    item.Value.summary.LastDateBearOccured,
                    Stat.STATS[new Tuple<string, IndicatorType>(item.Key.Item1, IndicatorType.MISC)].summary.ProjOpenPrice[0],
                    Stat.STATS[new Tuple<string, IndicatorType>(item.Key.Item1, IndicatorType.MISC)].summary.ProjOpenPrice[1],
                    Stat.STATS[new Tuple<string, IndicatorType>(item.Key.Item1, IndicatorType.MISC)].summary.ProjLowPrice[0],
                    Stat.STATS[new Tuple<string, IndicatorType>(item.Key.Item1, IndicatorType.MISC)].summary.ProjLowPrice[1],
                    Stat.STATS[new Tuple<string, IndicatorType>(item.Key.Item1, IndicatorType.MISC)].summary.ProjHighPrice[0],
                    Stat.STATS[new Tuple<string, IndicatorType>(item.Key.Item1, IndicatorType.MISC)].summary.ProjHighPrice[1],
                    Stat.STATS[new Tuple<string, IndicatorType>(item.Key.Item1, IndicatorType.MISC)].summary.ProjClosePrice[0],
                    Stat.STATS[new Tuple<string, IndicatorType>(item.Key.Item1, IndicatorType.MISC)].summary.ProjClosePrice[1]
                    );
            }
            sw.Close();
        }

        public static void Summarize()
        {
            foreach (var item in Stat.STATS)
            {
                //if( item.Value.summary.Indicator == IndicatorType.MISC)
                Console.WriteLine(
                    "Symbol: {0} | " +
                    "Indicator: {1} | " + 
                    "Num Bull Occurances: {2} | " +
                    "Bull probability:{3} | " +
                    "Num Bear Occurances: {4} | " +
                    "Bear probability:{5} | " +
                    "Last date Bull:{6} | " +
                    "Last date Bear:{7} |" +
                    "Bull deviation:{8} |" +
                    "Bear devation:{9} |",
                    item.Key.Item1,
                    item.Value.summary.Indicator.ToString(),
                    item.Value.summary.NumBullOccurances,
                    item.Value.summary.BullSuccessRatio,
                    item.Value.summary.NumBullOccurances,
                    item.Value.summary.BearSuccessRatio,
                    item.Value.summary.LastDateBullOccured,
                    item.Value.summary.LastDateBearOccured ,
                    item.Value.summary.ProjOpenPrice[0],
                    item.Value.summary.ProjOpenPrice[1]
                    );
            }
        }
        public static void Strategy() 
        {
            DateOnly targetFromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));
            DateOnly targetToDate = DateOnly.FromDateTime(DateTime.Today);

            IndicatorType targtIndicatorType = IndicatorType.HHLL;

            double invested = 0;
            Dictionary<string, double> holdings = new Dictionary<string, double>();

            string companySymbol = "JSWSTEEL";
            HigherHighLowerLow hhll = (HigherHighLowerLow) Stat.STATS[new Tuple<string, IndicatorType>(companySymbol, IndicatorType.HHLL)];

            Console.WriteLine("HH");
            foreach(var item in hhll.HigherHighs)
            {
                Console.WriteLine("{0} {1}", item.Item1, History.HISTORY[new Tuple<string, DateOnly>(companySymbol, item.Item1)].CLOSEPRICE);
            }
            Console.WriteLine("HL");
            foreach (var item in hhll.HigherLows)
            {
                Console.WriteLine("{0} {1}", item.Item1, History.HISTORY[new Tuple<string, DateOnly>(companySymbol, item.Item1)].CLOSEPRICE);
            }
        }
        
        public static void QueryBuilder()
        {
            List<Query> list = new List<Query>();
            
            QueryCommand qc = new QueryCommand("[{\"Symbol\":null,\"Indicator\":2,\"Trend\":1,\"FromDate\":\"2024-02-01T00:00:00\",\"ToDate\":\"2024-02-03T00:00:00\"}]");
            qc.Print();
        }
    }
}