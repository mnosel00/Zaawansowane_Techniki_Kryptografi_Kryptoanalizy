using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    // Pomocnicza funkcja do obliczania odwrotności modularnej (rozszerzony algorytm Euklidesa)
    static int ModInverse(int a, int m)
    {
        a %= m;
        for (int x = 1; x < m; x++)
            if ((a * x) % m == 1)
                return x;
        throw new Exception($"Brak odwrotności modularnej dla a={a}, m={m}");
    }

    // Funkcja deszyfrująca szyfrogram przy użyciu kluczy a, b
    static string DecryptAffine(string ciphertext, int a, int b)
    {
        int m = 26;
        int aInv = ModInverse(a, m);
        char[] result = new char[ciphertext.Length];

        for (int i = 0; i < ciphertext.Length; i++)
        {
            char c = ciphertext[i];
            if (char.IsLetter(c))
            {
                int y = char.ToUpper(c) - 'A';
                int x = (aInv * (y - b + m)) % m; // D(y) = a^-1 * (y - b)
                result[i] = (char)('A' + x);
            }
            else
            {
                result[i] = c; // pozostaw znaki niealfabetyczne
            }
        }
        return new string(result);
    }

    // Funkcja do zliczania częstości liter
    static Dictionary<int, List<char>> GroupLetterOccurrences(string text)
    {
        var freq = new Dictionary<char, int>();
        foreach (char c in text.ToUpper())
        {
            if (char.IsLetter(c))
            {
                if (!freq.ContainsKey(c))
                    freq[c] = 0;
                freq[c]++;
            }
        }

        // Grupowanie liter według liczby wystąpień
        var grouped = freq
            .GroupBy(kv => kv.Value)
            .OrderByDescending(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Select(kv => kv.Key).OrderBy(ch => ch).ToList()
            );

        return grouped;
    }

    static void Main()
    {
        string ciphertext = "BIBOX WHX V NHFB DE YQBU V TYHOY YZ MOHFB DE";

        Console.WriteLine("=== ANALIZA CZĘSTOŚCI ===");
        var grouped = GroupLetterOccurrences(ciphertext);
        foreach (var group in grouped)
            Console.WriteLine($"Wystąpień: {group.Key} -> Litery: {string.Join(", ", group.Value)}");

        // Hipoteza: E -> B (y=1), T -> Y (y=24)
        int m = 26;
        int x1 = 4, y1 = 1;   // E -> B
        int x2 = 19, y2 = 24; // T -> Y

        int a, b;
        int diffX = (x2 - x1) % m;
        int diffY = (y2 - y1 + m) % m;

        int inv = ModInverse(diffX, m);
        a = (diffY * inv) % m;
        b = (y1 - a * x1) % m;
        if (b < 0) b += m;

        Console.WriteLine($"\nKlucz a = {a}, b = {b}");

        Console.WriteLine("\n=== ODSZYFROWANIE ===");
        string plaintext = DecryptAffine(ciphertext, a, b);
        Console.WriteLine(plaintext);
    }
}
