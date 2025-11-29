using System;
using System.Numerics;


class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(" Arytmetyka Modulo");

        BigInteger prime = 11;

        // Test 1: Modulo dla liczb ujemnych
        // Slajd 13 pokazuje wynik -1 mod 11 = 10 
        BigInteger negativeVal = -1;
        BigInteger modResult = ZTTK_Lab4.Math.Mod(negativeVal, prime);
        Console.WriteLine($"1. Test Modulo: -1 mod 11 = {modResult} (Oczekiwane: 10)");

        // Test 2: Odwrotność modulo (Dzielenie)
        // 2 * x = 1 mod 11.  Na przykład =6, bo 2*6 = 12 = 1 mod 11.
        BigInteger a = 2;
        BigInteger inverse = ZTTK_Lab4.Math.ModInverse(a, prime);
        Console.WriteLine($"2. Test Odwrotności: 1/{a} mod 11 = {inverse} (Oczekiwane: 6)");
        Console.WriteLine($"   Sprawdzenie: {a} * {inverse} = {a * inverse} = {(a * inverse) % prime} mod 11");

        Console.WriteLine("\nJeśli wyniki są zgodne z oczekiwanymi, matematyka działa.");
        Console.ReadLine();
    }
}