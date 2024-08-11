using EODDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsManager
{
    public class Utilities
    {
        public static Dictionary<DateTime, int> TradedDates = new Dictionary<DateTime, int>();

        public static bool isGreenCandle(double openPrice, double highPrice, double lowPrice, double closePrice)
        {
            //Console.WriteLine("{0} {1}", openPrice, closePrice);
            return openPrice < closePrice ? true : false;
        }
        public static bool isRedCandle(double openPrice, double highPrice, double lowPrice, double closePrice)
        {
            return(closePrice < openPrice ? true : false);
        }
        public static bool PriceInRange(int left, int right, int pivot)
        {
            bool status = false;
            if (left < right)
            {
                if (pivot > left && pivot < right) status = true;
            }
            else if (right < left)
            {
                if (pivot > right && pivot < left) status = true;
            }
            return status;
        }
        public static bool PriceInRange(double left, double right, double pivot)
        {
            bool status = false;
            if (left < right)
            {
                if (pivot > left && pivot < right) status = true;
            }
            else if (right < left)
            {
                if (pivot > right && pivot < left) status = true;
            }
            return status;
        }
        public static bool ArePricesInSameRange(double leftPrice, double rightPrice, float atmostDifferencePercentage)
        {
            bool inSameRange = false;
            double differenceInPercentage;

            if (rightPrice < leftPrice)
            {
                differenceInPercentage = ((leftPrice - rightPrice) / ((leftPrice + rightPrice) / 2)) * 100;
            }
            else 
            {
                differenceInPercentage = ((rightPrice - leftPrice) / ((leftPrice + rightPrice) / 2)) * 100;
            }

            if ( (int) differenceInPercentage <= atmostDifferencePercentage)
                inSameRange = true;
            
            return inSameRange;
        }
        public static double StandardDeviation(List<double> values)
        {
            double result = 0;

            //1. Step1 Mean
            double mean = 0;
            foreach (double value in values)
            {
                mean += value;
            }
            if (values.Count > 0) mean /= values.Count;

            double distance = 0;
            double sumDistance = 0;
            foreach (double value in values)
            {
                distance = value > mean ? value - mean : mean - value;
                distance = Math.Pow(distance, 2);
                sumDistance += distance;
            }
            if (values.Count > 0) sumDistance /= values.Count;
            result = Math.Sqrt(sumDistance);
            return result;
        }
        public static DateOnly GetNthTradedDate(string symbol, DateOnly currentDate, int distance)
        {
            int maxAttempts = 5;

            DateOnly nThTradedDate = currentDate;

            nThTradedDate = nThTradedDate.AddDays(distance);

            Tuple<string, DateOnly> itr = new Tuple<string, DateOnly>(symbol, nThTradedDate);

            while ( !History.HISTORY.ContainsKey(itr) && maxAttempts > 0)
            {
                nThTradedDate = nThTradedDate.AddDays(-1);
                itr = new Tuple<string, DateOnly>(symbol, nThTradedDate);
                maxAttempts--;
            }

            return History.HISTORY.ContainsKey(new Tuple<string, DateOnly>(symbol, nThTradedDate)) ? nThTradedDate : currentDate;
        }

        public static DateOnly GetLastDayOfWeek(String symbol, DateOnly currentDate)
        {
            DateOnly lastDayOfWeek = currentDate;
            int difference = DayOfWeek.Friday - lastDayOfWeek.DayOfWeek;

            //if (currentDate == DateOnly.FromDateTime(DateTime.Parse("03APR2023")))
                //Console.WriteLine("*********************");

            while ( 
                !History.HISTORY.ContainsKey(new Tuple<string, DateOnly>(symbol, currentDate.AddDays(difference))) && 
                difference  >= 0
                )
            {
                difference--;
            }
            lastDayOfWeek = currentDate.AddDays((int)difference);

            //Console.WriteLine("DEBUG: GetLastDayOfWeek: Given {0} Output {1}", currentDate, lastDayOfWeek);

            return lastDayOfWeek;
        }
    }
}
