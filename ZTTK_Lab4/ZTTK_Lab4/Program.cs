using System;
using System.Collections.Generic;
using System.Numerics;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(" Generowanie Udziałów ");

        
        BigInteger prime = 11;
        int n = 5;
        // Wielomian P(x) = 10 + 7x + 2x^2  (Sekret = 10)
        // Tablica współczynników [a0, a1, a2] -> [10, 7, 2]
        BigInteger[] testCoefficients = { 10, 7, 2 };

        Console.WriteLine($"Wielomian: 2x^2 + 7x + 10 (mod 11)");
        Console.WriteLine($"Sekret: {testCoefficients[0]}");

        // TEST
        List<Share> shares = Shamir.SplitSecretWithCoefficients(testCoefficients, n, prime);

        Console.WriteLine("\nWyliczone udziały:");
        foreach (var share in shares)
        {
            Console.WriteLine($"Użytkownik {share.ID}: Udział {share.Value}");
        }

        Console.WriteLine("\nOczekiwane wartości");
        Console.WriteLine("x=1 -> 8");
        Console.WriteLine("x=2 -> 10");
        Console.WriteLine("x=3 -> 5");
        Console.WriteLine("x=4 -> 4");
        Console.WriteLine("x=5 -> 7");

        Console.ReadLine();
    }
}