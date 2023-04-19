using MathNet.Numerics.Statistics;
using Newtonsoft.Json;

namespace ConsoleApp1
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static readonly string apiUrl = $"https://api.kucoin.com/api/v1/market/stats?symbol=BTC-USDT";//fixed
        static readonly int predictionDelaySeconds = 60;//fixed (زمان هر پیش بینی به ثانیه)
        static readonly int intervalSeconds = 5;//fixed (زمان بین هر دیتا به ثانیه)
        static readonly int datas = predictionDelaySeconds / intervalSeconds;//fixed (تعداد کل دیتاهای دریافتی تا قبل از پیش بینی)

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome!");
            Console.WriteLine("The Programm consists of 3 parts:");
            Console.WriteLine();
            Console.WriteLine("1- Get the price of bitcoin at specified intervals, print and save them.");
            Console.WriteLine("2- price prediction for the next time slice with the Median method.");
            Console.WriteLine("2- Calculate the percentage of error.");
            Console.WriteLine();
            Operation operation = new Operation();
            List<decimal> prices = new List<decimal>();
            decimal? nextPricePrediction = null;

            for (int i = 0; ; i++)
            {
                if (i == datas)
                {
                    nextPricePrediction = operation.GetNextPricePrediction(prices);
                    Console.WriteLine("-------------------------------------------------------------------");
                    Console.WriteLine($"Price forecast for the next time slice: {nextPricePrediction}");
                    Console.WriteLine("-------------------------------------------------------------------");
                    decimal actualPrice = operation.GetLastPrice();
                    decimal errorPercentage = 100 * Math.Abs(nextPricePrediction.Value - actualPrice) / actualPrice;
                    Console.WriteLine($"Prediction error percentage: {errorPercentage}%");
                    Console.WriteLine("-------------------------------------------------------------------");
                    Console.WriteLine();
                    Console.WriteLine("Start Next Time Slice:");
                    Console.WriteLine();
                    prices.Clear();
                    nextPricePrediction = null;
                    i = -1;
                }
                decimal price = operation.GetLastPrice();
                prices.Add(price);
                Console.WriteLine($" => Price: {price}");
                Thread.Sleep(intervalSeconds * 1000);
            }
        }

    class Operation 
        {
            public decimal GetLastPrice()
            {
                Task<string> task = client.GetStringAsync(apiUrl);
                task.Wait();
                dynamic response = JsonConvert.DeserializeObject<dynamic>(task.Result);
                return response.data.last;
            }

            public decimal GetNextPricePrediction(List<decimal> prices)
            {
                decimal medianPrice = (decimal)prices.Select(p => (double)p).Median();
                return medianPrice;
            }
        }
    }
}