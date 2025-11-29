using System;
using System.Collections.Generic;
using System.Numerics;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Rekonstrukcja Sekretu (Lagrange)");

        // Dane testowe (te same co wcześniej)
        BigInteger prime = 11;
        int t = 3; // Próg

        // Zestaw udziałów wyliczony w poprzednim kroku:
        // (1, 8), (2, 10), (3, 5), (4, 4), (5, 7)
        List<Share> allShares = new List<Share>
        {
            new Share(1, 8),
            new Share(2, 10),
            new Share(3, 5),
            new Share(4, 4),
            new Share(5, 7)
        };

        Console.WriteLine("Oryginalny sekret: 10");
        Console.WriteLine($"Próg t: {t}");

        //Rekonstrukcja z 3 udziałów 
        //udziały 1, 3, 5
        // P(1), P(2), P(3) -> program używa  1, 3, 5
        List<Share> subsetT = new List<Share> { allShares[0], allShares[2], allShares[4] };

        Console.WriteLine("\nPróba odzyskania z 3 udziałów (ID: 1, 3, 5)");
        BigInteger recoveredA = Shamir.RecoverSecret(subsetT, prime);
        Console.WriteLine($"Odzyskany sekret: {recoveredA}");

        if (recoveredA == 10) Console.WriteLine("-> SUKCES: Sekret poprawny.");
        else Console.WriteLine("-> BŁĄD: Sekret niepoprawny.");


        // Rekonstrukcja z 2 udziałów
        List<Share> subsetLess = new List<Share> { allShares[0], allShares[4] };

        Console.WriteLine("\nPróba odzyskania z 2 udziałów (ID: 1, 5)");
        BigInteger recoveredB = Shamir.RecoverSecret(subsetLess, prime);
        Console.WriteLine($"Odzyskany sekret: {recoveredB}");

        if (recoveredB != 10) Console.WriteLine("-> SUKCES: sekret jest błędny (bezpieczny).");
        else Console.WriteLine("-> BŁĄD: Udało się odgadnąć sekret mimo braku udziałów");

        Console.ReadLine();
    }
}