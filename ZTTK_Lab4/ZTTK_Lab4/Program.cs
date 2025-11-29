using System;
using System.Collections.Generic;
using System.Numerics;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Dodawanie nowego użytkownika");

        BigInteger prime = 11;
        BigInteger[] coefficients = { 10, 7, 2 }; // Ten sam wielomian: 10 + 7x + 2x^2

        // Stare udziały 
        Share s1 = new Share(1, 8);
        Share s2 = new Share(2, 10);

        Console.WriteLine(" Old user 1 i 2.");

        // 1. Generujemy udział dla użytkownika nr 6
        Console.WriteLine("Generowanie udziału dla nowego Użytkownika 6...");
        Share sNew = Shamir.CreateNewShare(6, coefficients, prime);
        Console.WriteLine($"Nowy udział: ID={sNew.ID}, Value={sNew.Value}");

        // 2. Próba rekonstrukcji (Stary 1 + Stary 2 + Nowy 6) = 3 udziały (wymagane t=3)
        List<Share> mixedGroup = new List<Share> { s1, s2, sNew };

        Console.WriteLine("\nPróba rekonstrukcji z zestawu {1, 2, 6}:");
        BigInteger recovered = Shamir.RecoverSecret(mixedGroup, prime);

        Console.WriteLine($"Odzyskany sekret: {recovered}");

        if (recovered == 10)
            Console.WriteLine("-> SUKCES: Nowy udział działa poprawnie ze starymi.");
        else
            Console.WriteLine("-> BŁĄD: Coś poszło nie tak.");

        Console.ReadLine();
    }
}