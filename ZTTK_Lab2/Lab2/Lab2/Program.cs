// Na początek dodajmy potrzebne usingi na górze pliku Program.cs
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        RunKeyGenerationBenchmarks();

         RunEncryptionBenchmarks(); 
    }

    public static void RunKeyGenerationBenchmarks()
    {
        Console.WriteLine("============= Rozpoczynam pomiar generowania kluczy =============");

        // Lista scenariuszy do przetestowania
        var counts = new List<int> { 1, 10, 100, 1000 };

        foreach (int count in counts)
        {
            Console.WriteLine($"\n--- Testowanie dla {count} kluczy ---");

            // === RSA ===
            MeasureRsaKeyGen(2048, count);
            MeasureRsaKeyGen(3072, count);

            // === AES ===
            MeasureAesKeyGen(128, count);
            MeasureAesKeyGen(256, count);

            // === 3DES ===
            Measure3DesKeyGen(192, count);
        }

        Console.WriteLine("============= Zakończono pomiar generowania kluczy =============");
    }
    public static void RunEncryptionBenchmarks()
    {
        Console.WriteLine("\n============= Rozpoczynam pomiar szyfrowania/deszyfrowania =============");

        var dataSizes = new List<int>
    {
        128,                   // 128 B
        512,                   // 512 B
        2 * 1024,              // 2 KB
        8 * 1024,              // 8 KB
        32 * 1024,             // 32 KB
        1 * 1024 * 1024,       // 1 MB
        4 * 1024 * 1024,       // 4 MB
        16 * 1024 * 1024       // 16 MB
    };

        foreach (int size in dataSizes)
        {
            Console.WriteLine($"\n--- Testowanie dla rozmiaru danych: {size / (1024.0 * 1024.0):F2} MB ({size} B) ---");

            byte[] data = GenerateRandomBytes(size);

            // --- Testy AES ---
            MeasureAesGcm(data, 128);
            MeasureAesGcm(data, 256);

            // --- Testy 3DES ---
            Measure3DesCbc(data);

            // --- Testy RSA ---
            // Będą one uruchamiane tylko dla rozmiarów, które się zmieszczą (czyli 128 B)
            MeasureRsaOaep(data, 2048);
            MeasureRsaOaep(data, 3072);
        }

        Console.WriteLine("============= Zakończono pomiar szyfrowania/deszyfrowania =============");
    }
    private static void MeasureRsaKeyGen(int keySizeInBits, int count)
    {
        var singleKeyTimings = new List<double>();
        var totalStopwatch = Stopwatch.StartNew();

        for (int i = 0; i < count; i++)
        {
            var singleStopwatch = Stopwatch.StartNew();

            // Generowanie klucza RSA
            using (RSA rsa = RSA.Create(keySizeInBits))
            {
                var parameters = rsa.ExportParameters(false);
            }

            singleStopwatch.Stop();
            singleKeyTimings.Add(singleStopwatch.Elapsed.TotalMilliseconds);
        }

        totalStopwatch.Stop();

        // Analiza statystyczna
        string title = $"RSA-{keySizeInBits} (Liczba: {count})";
        StatisticsCalculator.CalculateAndPrint(title, singleKeyTimings);
        Console.WriteLine($"  Łączny czas {count} kluczy: {totalStopwatch.Elapsed.TotalMilliseconds:F4} ms\n");
    }
    private static void MeasureAesKeyGen(int keySizeInBits, int count)
    {
        var singleKeyTimings = new List<double>();
        var totalStopwatch = Stopwatch.StartNew();

        for (int i = 0; i < count; i++)
        {
            var singleStopwatch = Stopwatch.StartNew();

            // Generowanie klucza AES
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = keySizeInBits;
                var key = aes.Key;
            }

            singleStopwatch.Stop();
            singleKeyTimings.Add(singleStopwatch.Elapsed.TotalMilliseconds);
        }

        totalStopwatch.Stop();

        // Analiza statystyczna
        string title = $"AES-{keySizeInBits} (Liczba: {count})";
        StatisticsCalculator.CalculateAndPrint(title, singleKeyTimings);
        Console.WriteLine($"  Łączny czas {count} kluczy: {totalStopwatch.Elapsed.TotalMilliseconds:F4} ms\n");
    }
    private static void Measure3DesKeyGen(int keySizeInBits, int count)
    {
        var singleKeyTimings = new List<double>();
        var totalStopwatch = Stopwatch.StartNew();

        for (int i = 0; i < count; i++)
        {
            var singleStopwatch = Stopwatch.StartNew();

            // Generowanie klucza 3DES
            using (TripleDES des = TripleDES.Create())
            {
                des.KeySize = keySizeInBits; 
                des.GenerateKey(); 

            }

            singleStopwatch.Stop();
            singleKeyTimings.Add(singleStopwatch.Elapsed.TotalMilliseconds);
        }

        totalStopwatch.Stop();

        // Analiza statystyczna
        string title = $"3DES-{keySizeInBits} (efektywnie 168) (Liczba: {count})";
        StatisticsCalculator.CalculateAndPrint(title, singleKeyTimings);
        Console.WriteLine($"  Łączny czas {count} kluczy: {totalStopwatch.Elapsed.TotalMilliseconds:F4} ms\n");
    }
    private static byte[] GenerateRandomBytes(int size)
    {
        return RandomNumberGenerator.GetBytes(size);
    }
    private static void MeasurePerformance(string title, int dataSizeInBytes, int iterations, int warmupIterations, Action operationToMeasure)
    {
        for (int i = 0; i < warmupIterations; i++)
        {
            operationToMeasure();
        }

        // 2. Właściwe pomiary
        var timings = new List<double>(iterations);
        var stopwatch = new Stopwatch();
        for (int i = 0; i < iterations; i++)
        {
            stopwatch.Restart();
            operationToMeasure();
            stopwatch.Stop();
            timings.Add(stopwatch.Elapsed.TotalMilliseconds);
        }

        // 3. Obliczenia i wyświetlenie statystyk
        StatisticsCalculator.CalculateAndPrint(title, timings);

        double averageTimeMs = timings.Average();
        double averageTimeSec = averageTimeMs / 1000.0;
        double dataSizeInMB = dataSizeInBytes / (1024.0 * 1024.0);
        double throughputMBps = (averageTimeSec > 0) ? (dataSizeInMB / averageTimeSec) : 0;

        Console.WriteLine($"  Przepustowość (śr.): {throughputMBps:F2} MB/s");
        Console.WriteLine("-------------------------------------\n");
    }
    private static void MeasureAesGcm(byte[] data, int keySizeInBits)
    {
        // Przygotowanie kluczy i buforów
        int keySizeInBytes = keySizeInBits / 8;
        byte[] key = GenerateRandomBytes(keySizeInBytes);

        // "nonce" (numer użyty raz)
        byte[] nonce = GenerateRandomBytes(12);

        //  "tag" uwierzytelniający - 16 bajtów 
        byte[] tag = new byte[16];

        byte[] encryptedData = new byte[data.Length];
        byte[] decryptedData = new byte[data.Length];

        using (var aesGcm = new AesGcm(key))
        {
            string title = $"AES-{keySizeInBits}-GCM (Rozmiar: {data.Length} B)";

            // Pomiar szyfrowania
            MeasurePerformance(
                title: $"SZYFROWANIE {title}",
                dataSizeInBytes: data.Length,
                iterations: 30,
                warmupIterations: 2,
                operationToMeasure: () =>
                {
                    aesGcm.Encrypt(nonce, data, encryptedData, tag);
                }
            );

            // Pomiar deszyfrowania
            MeasurePerformance(
                title: $"DESZYFRACJA {title}",
                dataSizeInBytes: data.Length,
                iterations: 30,
                warmupIterations: 2,
                operationToMeasure: () =>
                {
                    // odszyfrowane trafiają do decryptedData
                    aesGcm.Decrypt(nonce, encryptedData, tag, decryptedData);
                }
            );
        }
    }
    private static void Measure3DesCbc(byte[] data)
    {
        // Przygotowanie kluczy i buforów
        // Klucz 192 bity (24 bajty) dla Keying Option
        using (var tdes = TripleDES.Create())
        {
            tdes.KeySize = 192;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.PKCS7;

            // Generujemy klucz i IV (Initialization Vector)
            byte[] key = tdes.Key; // wygenerowany klucz
            byte[] iv = tdes.IV;   // wygenerowany IV

            string title = $"3DES-168-CBC (Rozmiar: {data.Length} B)";
            byte[] encryptedData = null;

            // Pomiar szyfrowania
            MeasurePerformance(
                title: $"SZYFROWANIE {title}",
                dataSizeInBytes: data.Length,
                iterations: 30,
                warmupIterations: 2,
                operationToMeasure: () =>
                {
                    // Używamy MemoryStream, aby trzymać dane w RAM-ie
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, tdes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                        {
                            cs.Write(data, 0, data.Length);
                            cs.FlushFinalBlock(); 
                        }
                        encryptedData = ms.ToArray(); // Zapisujemy wynik do późniejszej deszyfracji
                    }
                }
            );

           

            // Pomiar deszyfrowania
            MeasurePerformance(
                title: $"DESZYFRACJA {title}",
                dataSizeInBytes: data.Length,
                iterations: 30,
                warmupIterations: 2,
                operationToMeasure: () =>
                {
                    using (var ms = new MemoryStream(encryptedData)) // Używamy zaszyfrowanych danych
                    {
                        using (var cs = new CryptoStream(ms, tdes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                        {
                            // Czytamy dane do bufora, aby wymusić deszyfrację
                            byte[] buffer = new byte[data.Length];
                            cs.Read(buffer, 0, buffer.Length);
                        }
                    }
                }
            );
        }
    }
    private static void MeasureRsaOaep(byte[] data, int keySizeInBits)
    {
        // Sprawdzenie, czy dane nie są za duże
        int maxDataSize = (keySizeInBits / 8) - 2 * (256 / 8) - 2; // (SHA-256 = 256 bitów = 32 bajty)
        if (data.Length > maxDataSize)
        {
            Console.WriteLine($"! POMIJAM RSA-{keySizeInBits} dla danych {data.Length} B (za duże, max to {maxDataSize} B)\n");
            return;
        }

        using (var rsa = RSA.Create(keySizeInBits))
        {
            string title = $"RSA-{keySizeInBits}-OAEP-SHA256 (Rozmiar: {data.Length} B)";
            byte[] encryptedData = null;

            // Pomiar szyfrowania
            MeasurePerformance(
                title: $"SZYFROWANIE {title}",
                dataSizeInBytes: data.Length,
                iterations: 30,
                warmupIterations: 2,
                operationToMeasure: () =>
                {
                    encryptedData = rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
                }
            );

            // Pomiar deszyfrowania
            MeasurePerformance(
                title: $"DESZYFRACJA {title}",
                dataSizeInBytes: data.Length,
                iterations: 30,
                warmupIterations: 2,
                operationToMeasure: () =>
                {
                    byte[] decryptedData = rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);
                }
            );
        }
    }
}

// Klasa pomocnicza do obliczeń statystycznych
public static class StatisticsCalculator
{
    public static void CalculateAndPrint(string title, List<double> measurementsMs)
    {
        if (measurementsMs == null || measurementsMs.Count == 0)
        {
            Console.WriteLine($"{title}: Brak danych do obliczeń.");
            return;
        }

        measurementsMs.Sort();

        double sum = measurementsMs.Sum();
        double average = sum / measurementsMs.Count;

        // Mediana (środkowa wartość)
        double median;
        if (measurementsMs.Count % 2 == 0)
        {
            // Parzysta liczba - średnia z dwóch środkowych
            int midIndex = measurementsMs.Count / 2;
            median = (measurementsMs[midIndex - 1] + measurementsMs[midIndex]) / 2.0;
        }
        else
        {
            // Nieparzysta - dokładnie środkowa wartość
            median = measurementsMs[measurementsMs.Count / 2];
        }

        // 95. percentyl (wartość, poniżej której mieści się 95% wyników)
        int p95Index = (int)Math.Ceiling(0.95 * measurementsMs.Count) - 1;
        if (p95Index < 0) p95Index = 0; // Zabezpieczenie dla małych list
        double p95 = measurementsMs[p95Index];

        // Wyświetlanie
        Console.WriteLine($"--- Statystyki dla: {title} ---");
        Console.WriteLine($"  Liczba pomiarów: {measurementsMs.Count}");
        Console.WriteLine($"  Średnia:         {average:F4} ms");
        Console.WriteLine($"  Mediana:         {median:F4} ms");
        Console.WriteLine($"  Percentyl 95 (p95): {p95:F4} ms");
        Console.WriteLine($"  Suma (łączny czas): {sum:F4} ms");
        Console.WriteLine("-------------------------------------");
    }
}
