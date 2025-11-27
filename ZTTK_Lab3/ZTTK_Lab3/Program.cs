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
    // --- INTERFEJSY I WRAPPERY (Bez zmian) ---
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

    // --- KLASA GŁÓWNA - TEST PREDYKCJI BITÓW ---
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ROZPOCZYNAM TEST PREDYKCJI BITÓW ===");

            var functions = new List<IHashFunction>
            {
                new Sha2Wrapper(),
                new Sha3Wrapper(),
                new AsconWrapper()
            };

            int sampleSize = 10000;
            Console.WriteLine($"Liczba próbek: {sampleSize}");

            // Nagłówek tabeli
            Console.WriteLine("\n{0,-15} | {1,-10} | {2,-10} | {3,-10} | {4,-10}",
                "Funkcja", "Max P(1)", "Min P(1)", "Średnie P", "Max Z-score");
            Console.WriteLine(new string('-', 75));

            foreach (var func in functions)
            {
                RunBitPredictionTest(func, sampleSize);
            }

            Console.WriteLine("\nGotowe! Wykresy predykcji (Prediction_*.png) zostały zapisane.");
        }

        static void RunBitPredictionTest(IHashFunction algo, int sampleSize)
        {
            int hashBits = algo.HashSizeInBits; // 256
            int[] onesCounts = new int[hashBits]; // Licznik jedynek dla każdej pozycji (0..255)
            Random rand = new Random();

            // 1. Pętla generująca próbki
            for (int i = 0; i < sampleSize; i++)
            {
                byte[] input = new byte[32]; // Losowe wejście
                rand.NextBytes(input);
                byte[] hash = algo.ComputeHash(input);

                // 2. Analiza bitów w uzyskanym skrócie
                for (int byteIdx = 0; byteIdx < hash.Length; byteIdx++)
                {
                    for (int bitIdx = 0; bitIdx < 8; bitIdx++)
                    {
                        // Sprawdzamy czy bit jest ustawiony na 1
                        // bitIdx 0 to najmniej znaczący bit w bajcie (lub najbardziej, zależy od konwencji, tutaj iterujemy wszystkie)
                        byte mask = (byte)(1 << bitIdx);
                        if ((hash[byteIdx] & mask) != 0)
                        {
                            int globalBitIndex = byteIdx * 8 + bitIdx;
                            onesCounts[globalBitIndex]++;
                        }
                    }
                }
            }

            // 3. Obliczenia statystyczne
            double[] probabilities = new double[hashBits];
            double[] xAxis = DataGen.Consecutive(hashBits);

            double maxProb = 0;
            double minProb = 100.0;
            double sumProb = 0;
            double maxAbsZ = 0;

            for (int j = 0; j < hashBits; j++)
            {
                double p = (double)onesCounts[j] / sampleSize * 100.0; // Procentowo
                probabilities[j] = p;

                if (p > maxProb) maxProb = p;
                if (p < minProb) minProb = p;
                sumProb += p;

                // Obliczanie statystyki Z dla pojedynczego bitu
                // Z = (X - n*p0) / sqrt(n*p0*(1-p0))
                // Gdzie X = liczba jedynek, n = 10000, p0 = 0.5
                double expectedCount = sampleSize * 0.5;
                double standardError = Math.Sqrt(sampleSize * 0.5 * 0.5);
                double z = (onesCounts[j] - expectedCount) / standardError;

                if (Math.Abs(z) > maxAbsZ) maxAbsZ = Math.Abs(z);
            }

            double avgProb = sumProb / hashBits;

            // Wyświetlenie w konsoli
            Console.WriteLine("{0,-15} | {1,-9:F2}% | {2,-9:F2}% | {3,-9:F2}% | {4,-10:F4}",
                algo.Name, maxProb, minProb, avgProb, maxAbsZ);

            // 4. Generowanie wykresu (Bar Plot / Scatter)
            var plt = new ScottPlot.Plot(800, 400);
            plt.Title($"Test Predykcji Bitów: {algo.Name}");
            plt.XLabel("Numer bitu (0-255)");
            plt.YLabel("Prawdopodobieństwo '1' [%]");

            // Ustawienie zakresu Y (np. 48% - 52%) żeby było widać fluktuacje, jak w artykule
            plt.SetAxisLimitsY(48, 52);
            plt.SetAxisLimitsX(0, 256);

            // Rysujemy jako "lizaki" (Lollipop plot) lub gęste słupki
            var bar = plt.AddBar(probabilities);
            bar.BarWidth = 0.8;
            bar.Color = Color.Black; // Styl jak w artykule (czarne słupki)

            // Linia idealna (50%)
            var hLine = plt.AddHorizontalLine(50.0);
            hLine.Color = Color.Red;
            hLine.LineStyle = LineStyle.Dash;
            hLine.LineWidth = 1;

            string fileName = $"Prediction_{algo.Name}.png";
            plt.SaveFig(fileName);
        }
    }
}