using Org.BouncyCastle.Crypto.Digests;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ZTTK_Lab3;

namespace HashAnalysis
{
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
        public byte[] ComputeHash(byte[] input) { return AsconHash.ComputeHash(input); } 
    }

    // --- KLASA DO TESTÓW ---
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ROZPOCZYNAM TEST ODLEGŁOŚCI HAMMINGA ===");

            var functions = new List<IHashFunction>
            {
                new Sha2Wrapper(),
                new Sha3Wrapper(),
                new AsconWrapper()
            };

            int sampleSize = 10000; 
            Console.WriteLine($"Liczba próbek: {sampleSize}");

            // Tabela wyników
            Console.WriteLine("\n{0,-15} | {1,-5} | {2,-5} | {3,-10} | {4,-10} | {5,-10}",
                "Funkcja", "Min", "Max", "Średnia", "Odchylenie", "Z-score");
            Console.WriteLine(new string('-', 80));

            foreach (var func in functions)
            {
                RunHammingTest(func, sampleSize);
            }

            Console.WriteLine("\nGotowe! Wykresy zostały zapisane w folderze z plikiem wykonywalnym (bin/Debug/...).");
        }

        static void RunHammingTest(IHashFunction algo, int sampleSize)
        {
            double[] distances = new double[sampleSize];
            double[] xAxis = DataGen.Consecutive(sampleSize); 
            Random rand = new Random();

            int inputSize = 32; // 256 bitów wejścia 

            for (int i = 0; i < sampleSize; i++)
            {
                // 1. Generuj losowy input
                byte[] input1 = new byte[inputSize];
                rand.NextBytes(input1);

                // 2. Skopiuj i zmień  1 losowy bit 
                byte[] input2 = (byte[])input1.Clone();
                int byteIndex = rand.Next(inputSize);
                int bitIndex = rand.Next(8);
                input2[byteIndex] ^= (byte)(1 << bitIndex);

                // 3. Oblicz skróty
                byte[] hash1 = algo.ComputeHash(input1);
                byte[] hash2 = algo.ComputeHash(input2);

               // 4. Oblicz dystans Hamminga 
                distances[i] = CalculateHammingDistance(hash1, hash2);
            }

            // --- Analiza Statystyczna ---
            double avg = distances.Average();
            double min = distances.Min();
            double max = distances.Max();
            double sumSquares = distances.Sum(d => Math.Pow(d - avg, 2));
            double stdDev = Math.Sqrt(sumSquares / (sampleSize - 1));

            // Z-score: (Avg - Exp) / (SD / sqrt(N))
          // 50% długości skrótu = 128 bitów dla 256-bitowego hasha 
            double expected = 128.0;
            double zScore = Math.Abs((avg - expected) / (stdDev / Math.Sqrt(sampleSize))); 

            // Wyświetlenie w tabeli
            Console.WriteLine("{0,-15} | {1,-5} | {2,-5} | {3,-10:F4} | {4,-10:F4} | {5,-10:F4}",
                algo.Name, min, max, avg, stdDev, zScore);

            // --- Generowanie Wykresu (ScottPlot) ---
            var plt = new ScottPlot.Plot(800, 400);
            plt.Title($"Test Odległości Hamminga: {algo.Name}");
            plt.XLabel("Numer próbki");
            plt.YLabel("Odległość Hamminga");

            // Dodaj punkty (Scatter plot)
            var scatter = plt.AddScatter(xAxis, distances);
            scatter.LineWidth = 0; 
            scatter.MarkerSize = 2; 
            scatter.Color = Color.Gray; 

            // Dodaj linię oczekiwaną (128)
            var hLine = plt.AddHorizontalLine(expected);
            hLine.Color = Color.Black;
            hLine.LineWidth = 1;
            hLine.LineStyle = LineStyle.Solid;

            // Zapisz do pliku
            string fileName = $"Hamming_{algo.Name}.png";
            plt.SaveFig(fileName);
        }

        // Metoda do liczenia różnic bitów
        static int CalculateHammingDistance(byte[] h1, byte[] h2)
        {
            int distance = 0;
            for (int i = 0; i < h1.Length; i++)
            {
                byte val = (byte)(h1[i] ^ h2[i]); 
                while (val != 0)
                {
                    distance++;
                    val &= (byte)(val - 1); 
                }
            }
            return distance;
        }
    }
}