using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using ZTTK_Lab4;

class Program
{
    static BigInteger currentPrime = 0;
    static BigInteger[] currentCoefficients = null;
    static List<Share> currentShares = new List<Share>();
    static int currentN = 0;
    static int currentT = 0;

    static void Main(string[] args)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("1. Podziel nowy sekret (Tekst lub Liczba)");
            Console.WriteLine("2. Pokaż aktualne udziały");
            Console.WriteLine("3. Odzyskaj sekret (Wybierz udziały)");
            Console.WriteLine("4. Dodaj nowego użytkownika");
            Console.WriteLine("5. TEST WYDAJNOŚCI");
            Console.WriteLine("0. Wyjście");
            Console.Write("\nWybór: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": SplitNewSecret(); break;
                case "2": ShowShares(); break;
                case "3": RecoverSecretUI(); break;
                case "4": AddNewUser(); break;
                case "5": RunPerformanceTest(); break;
                case "0": return;
                default: Console.WriteLine("Nieznana opcja."); break;
            }
            Console.WriteLine("\nNaciśnij ENTER, aby kontynuować...");
            Console.ReadLine();
        }
    }

    static void SplitNewSecret()
    {
        Console.WriteLine("\nDZIELENIE SEKRETU");
        Console.Write("Podaj sekret (tekst): ");
        string secretText = Console.ReadLine();
        BigInteger secretVal = ZTTK_Lab4.Math.TextToBigInteger(secretText);

        Console.Write("Podaj liczbę udziałów (n): ");
        currentN = int.Parse(Console.ReadLine());

        Console.Write("Podaj próg rekonstrukcji (t): ");
        currentT = int.Parse(Console.ReadLine());

        if (currentT > currentN)
        {
            Console.WriteLine("Błąd: t nie może być większe od n!");
            return;
        }

        // Automatyczny dobór liczby pierwszej q > Secret oraz q > n
        Console.WriteLine("Szukanie liczby pierwszej q...");
        BigInteger minVal = secretVal > currentN ? secretVal : currentN;
        currentPrime = ZTTK_Lab4.Math.GetNextPrime(minVal);
        Console.WriteLine($"Znaleziono q: {currentPrime}");

        // Dzielenie
        // Zapisywanie współczynników, żeby móc potem dodawać użytkowników
        currentCoefficients = new BigInteger[currentT];
        currentCoefficients[0] = secretVal;
        for (int i = 1; i < currentT; i++)
            currentCoefficients[i] = ZTTK_Lab4.Math.RandomBigInteger(currentPrime - 1) + 1;

        // Generowanie udziałów
        // Użyta metoda z klasy Shamir 
        // Użycie pętli z CreateNewShare
        currentShares.Clear();
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 1; i <= currentN; i++)
        {
            currentShares.Add(Shamir.CreateNewShare(i, currentCoefficients, currentPrime));
        }
        sw.Stop();

        Console.WriteLine($"\nSekret podzielony pomyślnie w {sw.ElapsedMilliseconds} ms.");
    }

    static void ShowShares()
    {
        if (currentShares.Count == 0)
        {
            Console.WriteLine("Brak aktywnych udziałów.");
            return;
        }
        Console.WriteLine("\nAKTUALNE UDZIAŁY");
        foreach (var s in currentShares)
        {
            Console.WriteLine($"ID: {s.ID} => {s.Value}");
        }
    }

    static void RecoverSecretUI()
    {
        if (currentShares.Count == 0) { Console.WriteLine("Najpierw podziel sekret."); return; }

        Console.WriteLine($"\nREKONSTRUKCJA (Wymagane t={currentT})");
        Console.WriteLine("Podaj ID użytkowników oddzielone spacją (np. '1 3 5'):");
        string input = Console.ReadLine();

        try
        {
            var ids = Array.ConvertAll(input.Split(' '), int.Parse);
            List<Share> sharesToUse = new List<Share>();

            foreach (int id in ids)
            {
                //  udział o tym ID w pamięci
                var share = currentShares.Find(s => s.ID == id);
                if (share.Equals(default(Share))) // nie znaleziono
                {
                    Console.WriteLine($"Błąd: Nie znaleziono udziału o ID {id}");
                    Console.Write($"Podaj wartość dla ID {id} ręcznie: ");
                    BigInteger val = BigInteger.Parse(Console.ReadLine());
                    sharesToUse.Add(new Share(id, val));
                }
                else
                {
                    sharesToUse.Add(share);
                }
            }

            BigInteger recoveredVal = Shamir.RecoverSecret(sharesToUse, currentPrime);
            string recoveredText = ZTTK_Lab4.Math.BigIntegerToText(recoveredVal);

            Console.WriteLine($"\nOdzyskana liczba: {recoveredVal}");
            Console.WriteLine($"Odzyskany tekst:  {recoveredText}");

            // Weryfikacja
            if (recoveredVal == currentCoefficients[0])
                Console.WriteLine("-> SUKCES: Sekret zgodny z oryginałem.");
            else
                Console.WriteLine("-> UWAGA: Wynik inny niż oryginał");

        }
        catch (Exception ex)
        {
            Console.WriteLine("Błąd danych wejściowych: " + ex.Message);
        }
    }

    static void AddNewUser()
    {
        if (currentCoefficients == null) { Console.WriteLine("Najpierw podziel sekret."); return; }

        Console.WriteLine("\n DODAWANIE UŻYTKOWNIKA");
        int newId = currentShares.Count + 1; // Nowe ID to n+1

        // Możemy też pozwolić wpisać dowolne ID
        Console.Write($"Podaj ID nowego użytkownika (sugerowane {newId}): ");
        string input = Console.ReadLine();
        if (!string.IsNullOrEmpty(input)) newId = int.Parse(input);

        Share newShare = Shamir.CreateNewShare(newId, currentCoefficients, currentPrime);
        currentShares.Add(newShare);
        currentN++;

        Console.WriteLine($"Wygenerowano udział: ID={newShare.ID}, Value={newShare.Value}");
        Console.WriteLine("Dodano do listy aktywnych udziałów.");
    }

    static void RunPerformanceTest()
    {
        Console.WriteLine("\nTEST WYDAJNOŚCI");

        BigInteger secret = 123456789;
        int t = 5;
        BigInteger prime = ZTTK_Lab4.Math.GetNextPrime(secret * 100000);

        int[] nValues = { 10, 100, 500, 1000, 2000, 5000 };

        Console.WriteLine($"{"n",-10} | {"Czas (ms)",-10} | {"Średni czas/udział (ms)",-20}");
        Console.WriteLine(new string('-', 45));

        foreach (int n in nValues)
        {
            // Przygotowanie współczynników
            BigInteger[] coeffs = new BigInteger[t];
            coeffs[0] = secret;
            for (int i = 1; i<t; i++) coeffs[i] = ZTTK_Lab4.Math.RandomBigInteger(prime);

            // Start pomiaru
            Stopwatch sw = Stopwatch.StartNew();

            for (int x = 1; x <= n; x++)
            {
                // Symulacja generowania pojedynczego udziału
                Shamir.CreateNewShare(x, coeffs, prime);
            }

            sw.Stop();
            double avg = (double)sw.ElapsedMilliseconds / n;
            Console.WriteLine($"{n,-10} | {sw.ElapsedMilliseconds,-10} | {avg,-20:F4}");
        }
        Console.WriteLine("\nTest zakończony.");
    }
}