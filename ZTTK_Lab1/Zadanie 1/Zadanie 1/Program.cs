using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    // Pomocnicza funkcja do obliczania odwrotności modularnej
    static int ModInverse(int a, int m)
    {
        a %= m;
        for (int x = 1; x < m; x++)
            if ((a * x) % m == 1)
                return x;
        throw new Exception($"Brak odwrotności modularnej dla a={a}, m={m}");
    }

    // Funkcja deszyfrująca szyfrogram
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

   
    static int ScoreByCommonWords(string text)
    {
        int score = 0;
        string paddedText = " " + text.ToUpper() + " ";

        // Lista popularnych słów (można rozszerzyć)
        string[] commonWords = {
            " THE ", " AND ", " FOR ", " YOU ", " ARE ", " WAS ", " IN ", " IS ",
            " IT ", " OF ", " TO ", " I ", " A ", " IF ", " U ", " WE ", " GO ",
            " ME ", " MY ", " KNOW ", " HOW ", " THEN ", " LIVE "
        };

        foreach (string word in commonWords)
        {
            if (paddedText.Contains(word))
            {
                score++;
            }
        }
        return score;
    }


    static void Main()
    {
        string ciphertext = "U IAZPQD UR G WZAI TAI FTQK XUHQ UZ FAWKA UR G EQQZ UF FTQZ G YQMZ UF FTQZ G WZAI G TMHQ FA SA";

        Console.WriteLine("=== ANALIZA CZĘSTOŚCI ===");
        var grouped = GroupLetterOccurrences(ciphertext);
        foreach (var group in grouped)
            Console.WriteLine($"Wystąpień: {group.Key} -> Litery: {string.Join(", ", group.Value)}");

        int m = 26;
        int[] commonPlain = { 4, 19, 0, 14, 8, 13 }; // E, T, A, O, I, N
        var commonCipher = grouped.SelectMany(g => g.Value).Take(6).ToList();

        Console.WriteLine($"\nTestowane litery szyfrogramu: {string.Join(", ", commonCipher)}");
        Console.WriteLine("Testowane litery tekstu jawnego: E, T, A, O, I, N");
        Console.WriteLine("\n... Rozpoczynanie ataku kryptoanalitycznego ...\n");

        int bestScore = -1;
        string bestPlaintext = "";
        int bestA = 0;
        int bestB = 0;

        // === NOWE ZMIENNE DO PRZECHOWYWANIA HIPOTEZY ===
        char best_y1 = ' ', best_y2 = ' ', best_x1 = ' ', best_x2 = ' ';

        foreach (char y1_char in commonCipher)
        {
            foreach (char y2_char in commonCipher)
            {
                if (y1_char == y2_char) continue;

                foreach (int x1 in commonPlain)
                {
                    foreach (int x2 in commonPlain)
                    {
                        if (x1 == x2) continue;

                        int y1 = y1_char - 'A';
                        int y2 = y2_char - 'A';

                        int diffX = (x2 - x1 + m) % m;
                        int diffY = (y2 - y1 + m) % m;

                        try
                        {
                            int inv = ModInverse(diffX, m);
                            int a = (diffY * inv) % m;
                            int b = (y1 - a * x1 + m * a) % m;

                            string plaintext = DecryptAffine(ciphertext, a, b);
                            int currentScore = ScoreByCommonWords(plaintext);

                            if (currentScore > bestScore)
                            {
                                bestScore = currentScore;
                                bestA = a;
                                bestB = b;
                                bestPlaintext = plaintext;

                                best_y1 = y1_char;
                                best_y2 = y2_char;
                                best_x1 = (char)('A' + x1);
                                best_x2 = (char)('A' + x2);
                            }
                        }
                        catch (Exception ex)
                        {
                            char x1_char = (char)('A' + x1);
                            char x2_char = (char)('A' + x2);
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine($"  [INFO] Hipoteza ({y1_char} -> {x1_char}), ({y2_char} -> {x2_char}) odrzucona. Powód: {ex.Message}");
                            Console.ResetColor();
                        }
                    }
                }
            }
        }

        // Wyświetl tylko jeden, najlepszy wynik
        Console.WriteLine("\n=== ATAK ZAKOŃCZONY ===");
        if (bestScore > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nNajlepszy znaleziony wynik (Ocena: {bestScore}):");

            // === DODANA LINIA WYŚWIETLAJĄCA HIPOTEZĘ ===
            Console.WriteLine($"Zwycięska hipoteza: ({best_y1} -> {best_x1}), ({best_y2} -> {best_x2})");

            Console.WriteLine($"Klucz: a = {bestA}, b = {bestB}");
            Console.WriteLine($"Tekst: {bestPlaintext}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Nie znaleziono prawdopodobnego tekstu jawnego. Spróbuj rozszerzyć listę słów w 'ScoreByCommonWords'.");
            Console.ResetColor();
        }
    }
}
