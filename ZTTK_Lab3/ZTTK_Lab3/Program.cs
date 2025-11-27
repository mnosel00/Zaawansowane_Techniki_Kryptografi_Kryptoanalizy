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
    // --- WRAPPERY (Bez zmian) ---
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

    // --- TEST SERII ---
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

            int sampleSize = 10000;
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
            double[] zStatistics = new double[sampleSize];
            double[] xAxis = DataGen.Consecutive(sampleSize);
            Random rand = new Random();
            int fails = 0;

            for (int i = 0; i < sampleSize; i++)
            {
                // 1. Generuj losowy skrót
                byte[] input = new byte[32];
                rand.NextBytes(input);
                byte[] hash = algo.ComputeHash(input);

                // 2. Analiza bitów (Liczenie n0, n1 i R)
                int n0 = 0;
                int n1 = 0;
                int R = 1; // Zaczynamy od pierwszej serii
                int? lastBit = null;

                // Konwersja byte[] na strumień bitów
                for (int b = 0; b < hash.Length; b++)
                {
                    for (int bitIdx = 0; bitIdx < 8; bitIdx++)
                    {
                        // Wyciągamy bit (0 lub 1)
                        int currentBit = (hash[b] >> bitIdx) & 1;

                        if (currentBit == 0) n0++;
                        else n1++;

                        if (lastBit.HasValue)
                        {
                            if (currentBit != lastBit.Value)
                            {
                                R++; // Zmiana wartości -> nowa seria
                            }
                        }
                        lastBit = currentBit;
                    }
                }

                // 3. Obliczenie statystyki Z (Wzory z PDF)
                // Wzór (3.5): Expected R
                double n = n0 + n1;
                double expectedR = ((2.0 * n0 * n1) / n) + 1.0;

                // Wzór (3.6): Standard Deviation
                double numerator = 2.0 * n0 * n1 * (2.0 * n0 * n1 - n);
                double denominator = Math.Pow(n, 2) * (n - 1);
                double SD = Math.Sqrt(numerator / denominator);

                // Wzór (3.4): Z statistic
                // Używamy wartości bezwzględnej do wykresu i oceny, tak jak w analizie PDF
                double z = Math.Abs((R - expectedR) / SD);
                zStatistics[i] = z;

                if (z > 1.96) fails++;
            }

            // --- Statystyki ---
            double avgZ = zStatistics.Average();
            double maxZ = zStatistics.Max();
            double minZ = zStatistics.Min();

            // Odchylenie standardowe samej statystyki Z
            double sumSquares = zStatistics.Sum(d => Math.Pow(d - avgZ, 2));
            double sdZ = Math.Sqrt(sumSquares / (sampleSize - 1));

            double failRate = (double)fails / sampleSize * 100.0;

            Console.WriteLine("{0,-15} | {1,-8:F2} | {2,-8:F2} | {3,-8:F2} | {4,-10:F2} | {5,-9:F2}%",
                algo.Name, maxZ, minZ, avgZ, sdZ, failRate);

            // --- Wykres ---
            var plt = new ScottPlot.Plot(800, 400);
            plt.Title($"Test Serii: {algo.Name}");
            plt.XLabel("Numer próbki");
            plt.YLabel("Wartość statystyki Z (|Z|)");

            // Ograniczenie osi Y, żeby wykres był czytelny (0 do 4.5 jak w PDF)
            plt.SetAxisLimitsY(0, 5.0);

            var scatter = plt.AddScatter(xAxis, zStatistics);
            scatter.LineWidth = 0;
            scatter.MarkerSize = 2;
            scatter.Color = Color.Gray;

            // Linia krytyczna 1.96
            var hLine = plt.AddHorizontalLine(1.96);
            hLine.Color = Color.Black;
            hLine.LineWidth = 1;
            hLine.LineStyle = LineStyle.Solid;

            string fileName = $"Series_{algo.Name}.png";
            plt.SaveFig(fileName);
        }
    }
}