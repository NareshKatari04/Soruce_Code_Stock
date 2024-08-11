using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace EODDownloader
{
    public class EODDataManager
    {
        public static List<DateTime> HolidayList = new List<DateTime> { DateTime.Parse("22-JAN-2024"), DateTime.Parse("26-JAN-2024"), DateTime.Parse("08-MAR-2024"), DateTime.Parse("25-MAR-2024"), DateTime.Parse("29-MAR-2024"), DateTime.Parse("11-APR-2024"), DateTime.Parse("17-APR-2024"), DateTime.Parse("01-MAY-2024"), DateTime.Parse("20-MAY-2024"), DateTime.Parse("17-JUN-2024"), DateTime.Parse("17-JUL-2024"), DateTime.Parse("15-AUG-2024"), DateTime.Parse("02-OCT-2024"), DateTime.Parse("01-NOV-2024"), DateTime.Parse("15-NOV-2024"), DateTime.Parse("25-DEC-2024") };

        public static List<Equity> EODList = new List<Equity>();
        public EODDataManager() { }
        private static void GetDateYYYYMMDD(DateTime dateTime, out int yyyy, out string mm, out string dd)
        {
            yyyy = dateTime.Year;
            mm = dateTime.ToString("MM");
            dd = dateTime.ToString("dd");
        }
        private static void GetDateYYYYMMMDD(DateTime dateTime, out int yyyy, out string mmm, out string dd)
        {
            yyyy = dateTime.Year;
            mmm = dateTime.ToString("MMM").ToUpper();
            dd = dateTime.ToString("dd");
        }
        public static string ComposeExtractFile(DateTime dateTime)
        {
            if (dateTime < DateTime.Parse("08JUL2024"))
            {
                int yyyy; string MMM, dd;
                GetDateYYYYMMMDD(dateTime, out yyyy, out MMM, out dd);
                return $"cm{dd}{MMM}{yyyy}bhav.csv";
            }
            else
            {
                int yyyy; string mm, dd;
                GetDateYYYYMMDD(dateTime, out yyyy, out mm, out dd);
                return $"BhavCopy_NSE_CM_0_0_0_{yyyy}{mm}{dd}_F_0000.csv";
            }
            
        }

        public static string ComposeDownloadURL(DateTime dateTime)
        {
            string downloadURL;
            if (dateTime < DateTime.Parse("08JUL2024")) //A dirty fix for the new format in NSE website
            {
                int yyyy;
                string mmm, dd;
                GetDateYYYYMMMDD(dateTime, out yyyy, out mmm, out dd);
                downloadURL = $"https://archives.nseindia.com/content/historical/EQUITIES/{yyyy}/{mmm}/cm{dd}{mmm}{yyyy}bhav.csv.zip";
            }
            else
            {
                int yyyy;
                string mm, dd;
                GetDateYYYYMMDD(dateTime, out yyyy, out mm, out dd);
                downloadURL = $"https://nsearchives.nseindia.com/content/cm/BhavCopy_NSE_CM_0_0_0_{yyyy}{mm}{dd}_F_0000.csv.zip";
            }
            Console.WriteLine(downloadURL );

            return downloadURL;
        }

        public static string ComposeDownloadFile(DateTime dateTime)
        {
            int yyyy;
            string mmm, dd;
            GetDateYYYYMMMDD(dateTime, out yyyy, out mmm, out dd);

            return $"{yyyy}_{mmm}_{dd}.zip";
        }

        public static void DownloadFromNSE(DateTime fDate, DateTime tDate )
        {
            WebClient myWebClient = new WebClient();
            DateTime FromDate = fDate;
            DateTime ToDate = tDate;

            for (DateTime dateTime = FromDate; dateTime.Date <= ToDate.Date ; dateTime = dateTime.AddDays(1))
            {
                if (!(dateTime.Date.DayOfWeek != DayOfWeek.Sunday && dateTime.Date.DayOfWeek != DayOfWeek.Saturday && !HolidayList.Contains(dateTime.Date)))
                    continue;

                if (dateTime.Date == DateTime.Now.Date)
                {
                    if (DateTime.Now.Hour < 18)
                        break;
                }

                //1. Download request
                try
                {
                    myWebClient.DownloadFile(new Uri(ComposeDownloadURL(dateTime)), ComposeDownloadFile(dateTime));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in downloading the data from NSE either the file is not ready from NSE / you are offline");
                    Console.WriteLine(ex.ToString());
                    continue;
                }

                //2. Wait
                Task.Delay(1000 * 5).Wait();

                //3. Print status
                Console.WriteLine("Downloaded {0} to {1} ", ComposeDownloadURL(dateTime), ComposeDownloadFile(dateTime));

                //4. Extract the zip file
                FileInfo fi = new FileInfo(ComposeDownloadFile(dateTime));
                if( fi.Length > 0 ) { 
                    ZipFile.ExtractToDirectory(ComposeDownloadFile(dateTime), Directory.GetCurrentDirectory());

                    //5. Archive the file
                    if( dateTime < DateTime.Parse("08JUL2024"))
                        SyncCSV(ComposeExtractFile(dateTime));
                    else
                        BhavEODDataManager.SyncCSV(ComposeExtractFile(dateTime));

                    File.Delete(ComposeExtractFile(dateTime));
                }
                File.Delete(ComposeDownloadFile(dateTime));
            }
        }

        public static void SyncCSV(string FilePath)
        {
            if ( File.Exists(FilePath) ) 
            {
                TextFieldParser parser = new TextFieldParser(FilePath);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                string[]? headers = parser.ReadFields();

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields[1] == "EQ")
                    {
                        int i = EODList.Count;

                        EODList.Add(new Equity(fields));
                    }
                }
                parser.Close();
            }
        }
    }
}
