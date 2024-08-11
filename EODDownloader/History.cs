using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EODDownloader
{
    public static class History
    {
        /* File where the history is saved to*/
        private static string HDataJson = new string("history.json");

        /* List of equities where the file is loaded to */
        public static List<Equity> HData = new List<Equity>();

        /* List of equities where the last year's data is loaded to */
        public static List<Equity> HDataOld = new List<Equity>();

        /* Latest date of the history available */
        public static DateTime LastUpdatedDate = new DateTime();

        /* Dictionary of historical data by symbol+date for day charts */
        public static Dictionary<Tuple<string, DateOnly>, Equity> HISTORY = new Dictionary<Tuple<string, DateOnly>, Equity>();

        /* Reset all the static data */
        private static void Reset()
        {
            HData.Clear();
            LastUpdatedDate = DateTime.MinValue;
            HISTORY.Clear();
        }

        /* Load the history of two years atmost */
        private static void LoadHistoryAtmostTwoYears()
        {
            int pastYear = DateTime.Today.Year - 1;
            string pastYearHistoryFile = "history_" + pastYear.ToString() + ".json";

            LoadHistoryFromJsonFile(pastYearHistoryFile);
            LoadHistoryFromJsonFile();
        }

        /* Load the historical EOD data from the file */
        private static void LoadHistoryFromJsonFile(string hDataJson="history.json")
        {
            HDataJson = hDataJson;
            System.Console.WriteLine(Environment.CurrentDirectory);
            if (File.Exists(HDataJson))
            {
                string HISTORYSTRING = File.ReadAllText(HDataJson);

                HData.AddRange(JsonSerializer.Deserialize<List<Equity>>(HISTORYSTRING));
                
                LastUpdatedDate = HData.Last().DATE;
            }
            else
            {
                LastUpdatedDate = DateTime.Parse("31Dec2023");
            }
        }

        /* Save the list of EOD historical data to a file */
        private static void SaveHistoryToJsonFile()
        {
            string HISTORYSTRING = JsonSerializer.Serialize<List<Equity>>(HData);
            File.WriteAllText(HDataJson, HISTORYSTRING);
        }

        /* Append to the HData, the recently downloaded Historical EOD data */
        private static void AddToHistory()
        {
            HData.AddRange(EODDataManager.EODList);
        }

        
        /* Refreshes the historical data to the latest available */
        public static void RefreshHistory()
        {
            Reset();

            //1. Load the history
            History.LoadHistoryFromJsonFile();

            //2. Download the EOD data
            DateTime fromdate = History.LastUpdatedDate.Date;

            //EODDataManager.DownloadFromNSE(fromdate.AddDays(1).Date, DateTime.Now.Date);
            EODDataManager.DownloadFromNSE(fromdate.AddDays(1).Date, DateTime.Now);

            //3. Update the History with the downloaded EOD list
            History.AddToHistory();

            //4. Save the latest history
            History.SaveHistoryToJsonFile();

            //5. Reset again
            Reset();
            History.LoadHistoryAtmostTwoYears();

            foreach (var eq in HData)
            {
                HISTORY.Add(new Tuple<string, DateOnly>(eq.SYMBOL, DateOnly.FromDateTime(eq.DATE)), eq);
            }
        }
    }
}
