using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;

namespace Tagada.Benchmark
{
    public class Program
    {
        private static readonly int _seconds = 30;

        private static readonly string _netcoreBaseUrl = "http://localhost:57490";
        private static readonly string _tagadaBaseUrl = "http://localhost:54829";

        private static readonly List<string> _urls = new List<string>
        {
            "/api/Calculate/plus?number1=1&number2=2"
        };

        public static void Main(string[] args)
        {
            Console.WriteLine("Press enter to start the benchmark");
            Console.ReadLine();

            StartTagadaBenchmark();
            Console.WriteLine("--- ---");

            StartTraditionalBenchmark();
            Console.WriteLine("--- ---");

            Console.ReadLine();
        }

        private static void StartTagadaBenchmark()
        {
            Console.WriteLine("Start Tagada .NET Core API benchmark");

            var stopWatchTagada = new Stopwatch();
            stopWatchTagada.Start();

            int tagadaIterations = 0;

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_tagadaBaseUrl);

                while (stopWatchTagada.Elapsed.TotalSeconds < _seconds)
                {
                    httpClient.GetAsync(_urls[0]).Wait();
                    tagadaIterations++;
                }
            }

            stopWatchTagada.Stop();

            Console.WriteLine($"Number of iterations in {_seconds} seconds: {tagadaIterations}");
        }

        private static void StartTraditionalBenchmark()
        {
            Console.WriteLine("Start traditional .NET Core API benchmark");

            var stopWatchNetcore = new Stopwatch();
            stopWatchNetcore.Start();

            int netcoreIterations = 0;

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_netcoreBaseUrl);

                while (stopWatchNetcore.Elapsed.TotalSeconds < _seconds)
                {
                    httpClient.GetAsync(_urls[0]).Wait();
                    netcoreIterations++;
                }
            }

            stopWatchNetcore.Stop();

            Console.WriteLine($"Number of iterations in {_seconds} seconds: {netcoreIterations}");
        }
    }
}
