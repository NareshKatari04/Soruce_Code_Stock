using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EODDownloader
{
    public class BhavEODDataManager
    {
        public static void SyncCSV(string FilePath)
        {
            if (File.Exists(FilePath))
            {
                TextFieldParser parser = new TextFieldParser(FilePath);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                string[]? headers = parser.ReadFields();

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields[8] == "EQ")
                    {
                        int i = EODDataManager.EODList.Count;

                        EODDataManager.EODList.Add(new Equity( new BhavEquity(fields) ));
                    }
                }
                parser.Close();
            }
        }
    }
}
