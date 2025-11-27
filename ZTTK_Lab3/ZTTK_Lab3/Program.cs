using Org.BouncyCastle.Crypto.Digests;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using ZTTK_Lab3;

namespace HashAnalysis
{
    // --- INTERFEJSY I WRAPPERY (Te same co wcześniej) ---
    public interface IHashFunction
    {
        string Name { get; }
        byte[] ComputeHash(byte[] input);
        int HashSizeInBits { get; }
    }

    public class Sha2Wrapper : IHashFunction
    {
        public string Name => "SHA-256";
        public int HashSizeInBits => 256;
        public byte[] ComputeHash(byte[] input) { using (var sha = SHA256.Create()) return sha.ComputeHash(input); }
    }

    public class Sha3Wrapper : IHashFunction
    {
        public string Name => "SHA3-256";
        public int HashSizeInBits => 256;
        public byte[] ComputeHash(byte[] input)
        {
            var digest = new Sha3Digest(256);
            digest.BlockUpdate(input, 0, input.Length);
            byte[] result = new byte[digest.GetDigestSize()];
            digest.DoFinal(result, 0);
            return result;
        }
    }

    public class AsconWrapper : IHashFunction
    {
        public string Name => "ASCON-HASH-256";
        public int HashSizeInBits => 256;
        public byte[] ComputeHash(byte[] input) { return AsconHash.ComputeHash(input); } // Korzysta z Twojego pliku AsconHash.cs
    }

    //TEST SERII
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ROZPOCZYNAM TEST SERII (WALD-WOLFOWITZ) ===");

            var functions = new List<IHashFunction>
            {
                new Sha2Wrapper(),
                new Sha3Wrapper(),
                new AsconWrapper()
            };

            int sampleSize = 10000; // Zgodnie z wymaganiami [cite: 256]
            Console.WriteLine($"Liczba próbek: {sampleSize}");

            // Tabela wyników (Wzorowana na tabeli 4 z PDF)
            Console.WriteLine("\n{0,-15} | {1,-8} | {2,-8} | {3,-8} | {4,-10} | {5,-10}",
                "Funkcja", "Max Z", "Min Z", "AVG Z", "SD Z", "Fail Rate");
            Console.WriteLine(new string('-', 85));

            foreach (var func in functions)
            {
                RunSeriesTest(func, sampleSize);
            }

            Console.WriteLine("\nGotowe! Wykresy (Series_*.png) zostały zapisane.");
        }

        static void RunSeriesTest(IHashFunction algo, int sampleSize)
        {
            double[] distances = new double[sampleSize];
            double[] xAxis = DataGen.Consecutive(sampleSize); // Oś X: 1, 2, 3...
            Random rand = new Random();
            int fails = 0;

            int inputSize = 32; // 256 bitów wejścia (tak jak długość wyjścia, dobra praktyka)

            for (int i = 0; i < sampleSize; i++)
            {
                // 1. Generuj losowy skrót
                byte[] input = new byte[32];
                rand.NextBytes(input);
                byte[] hash = algo.ComputeHash(input);

                // 2. Skopiuj i zmień DOKŁADNIE 1 losowy bit 
                byte[] input2 = (byte[])input1.Clone();
                int byteIndex = rand.Next(inputSize);
                int bitIndex = rand.Next(8);
                input2[byteIndex] ^= (byte)(1 << bitIndex);

                // Wzór (3.4): Z statistic
                double z = Math.Abs((R - expectedR) / SD);
                zStatistics[i] = z;

                if (z > 1.96) fails++;
            }

            // --- Analiza Statystyczna ---
            double avg = distances.Average();
            double min = distances.Min();
            double max = distances.Max();
            double sumSquares = distances.Sum(d => Math.Pow(d - avg, 2));
            double stdDev = Math.Sqrt(sumSquares / (sampleSize - 1)); // [cite: 266, 311]

            // Z-score: (Avg - Exp) / (SD / sqrt(N))
          // Oczekiwana wartość: 50% długości skrótu = 128 bitów dla 256-bitowego hasha 
            double expected = 128.0;
            double zScore = Math.Abs((avg - expected) / (stdDev / Math.Sqrt(sampleSize))); // Wzór (3.1) z obrazka [cite: 266]

            Console.WriteLine("{0,-15} | {1,-8:F2} | {2,-8:F2} | {3,-8:F2} | {4,-10:F2} | {5,-9:F2}%",
                algo.Name, maxZ, minZ, avgZ, sdZ, failRate);

            // --- Wykres ---
            var plt = new ScottPlot.Plot(800, 400);
            plt.Title($"Test Serii: {algo.Name}");
            plt.XLabel("Numer próbki");
            plt.YLabel("Wartość statystyki Z (|Z|)");

           
            plt.SetAxisLimitsY(0, 5.0);

            // Dodaj punkty (Scatter plot)
            var scatter = plt.AddScatter(xAxis, distances);
            scatter.LineWidth = 0; // Brak linii łączącej
            scatter.MarkerSize = 2; // Małe kropki
            scatter.Color = Color.Gray; // Kolor jak w artykule [cite: 272]

            // Linia krytyczna 1.96
            var hLine = plt.AddHorizontalLine(1.96);
            hLine.Color = Color.Black;
            hLine.LineWidth = 1;
            hLine.LineStyle = LineStyle.Solid;

            string fileName = $"Series_{algo.Name}.png";
            plt.SaveFig(fileName);
        }

        // Metoda pomocnicza do liczenia różnic bitów
        static int CalculateHammingDistance(byte[] h1, byte[] h2)
        {
            int distance = 0;
            for (int i = 0; i < h1.Length; i++)
            {
                byte val = (byte)(h1[i] ^ h2[i]); // XOR pokazuje różnice
                while (val != 0)
                {
                    distance++;
                    val &= (byte)(val - 1); // Algorytm Kernighana do liczenia bitów
                }
            }
            return distance;
        }
    }
}