using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StatsManager
{
    public class Query
    {
        public string Symbol { get; set; }
        public IndicatorType Indicator { get; set; }
        public TrendType Trend { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        //Constructor
        public Query() { }
    }
    public class QueryCommand
    {
        public List<Query> QueryCmd;
        public QueryCommand() { }
        public QueryCommand(string qry) 
        {
            QueryCmd = JsonSerializer.Deserialize<List<Query>>(qry);
        }
        public void Process()
        {
            List< DateOnly > list = new List<DateOnly>();
            
            list = Stat.STATS[new Tuple<string, IndicatorType>(QueryCmd[0].Symbol, QueryCmd[0].Indicator)].
                        summary.BearOccurances.Where
                        (
                            x => x.Key <= DateOnly.FromDateTime(QueryCmd[0].ToDate) &&
                            x.Key >= DateOnly.FromDateTime(QueryCmd[0].FromDate)
                        ).Select(x => x.Key).ToList();

            list.Sort();
            if (list.Count > 0)
                Console.WriteLine(QueryCmd[0].Symbol);
        }
        public void Print() {
            List<string> allSymbols = Stat.STATS.Select(x => x.Key.Item1).Distinct().ToList();
            Dictionary<string, byte> symbols = new Dictionary<string, byte>();
            
            foreach (var cmd in QueryCmd)
            {
                foreach (var sym in allSymbols)
                {
                    if (Stat.STATS.Where(x => 
                                FilterDelegator(new Tuple<string, IndicatorType>(sym, cmd.Indicator), cmd) ).ToList().Count() != 0)
                    {
                        if (!symbols.ContainsKey(sym))
                            symbols.Add(sym, 1);
                        else
                            symbols[sym]++;
                    }
                }
            }

            foreach(var k in symbols.Where( x => x.Value == QueryCmd.Count))
            {
                Console.WriteLine("Result:" + k.Key);
            }
        }
        public bool FilterDelegator(Tuple<string, IndicatorType> k, Query cmd)
        {
            if (cmd.Trend == TrendType.BULL)
            {
                if (Stat.STATS[k].summary.BullOccurances.Where(x => x.Key >= DateOnly.FromDateTime(cmd.FromDate) && x.Key <= DateOnly.FromDateTime(cmd.ToDate)).ToList().Count > 0)
                    return true;
                else
                    return false;
            }
            else
            {
                if (Stat.STATS[k].summary.BearOccurances.Where(x => x.Key >= DateOnly.FromDateTime(cmd.FromDate) && x.Key <= DateOnly.FromDateTime(cmd.ToDate)).ToList().Count > 0)
                    return true;
                else
                    return false;
            }
        }
    }
}
