using System;
using System.Collections.Generic;
using System.Numerics;

public struct Share
{
    public int ID;          // x (numer udziału)
    public BigInteger Value; // y (wartość wielomianu w punkcie x)

    public Share(int id, BigInteger value)
    {
        ID = id;
        Value = value;
    }
}

public class Shamir
{
    //funkcja dzielenia sekretu z losowymi współczynnikami
    public static List<Share> SplitSecret(BigInteger secret, int n, int t, BigInteger prime)
    {
        // 1. Losowanie współczynników wielomianu a_1 ... a_{t-1}
        // Wyraz wolny a_0 to sekret.
        BigInteger[] coefficients = new BigInteger[t];
        coefficients[0] = secret;

        for (int i = 1; i < t; i++)
        {
            // współczynnik z zakresu [1, prime - 1]
            coefficients[i] = ZTTK_Lab4.Math.RandomBigInteger(prime - 1) + 1;
        }

        return GenerateShares(coefficients, n, prime);
    }

    // Funkcja do testów 
    public static List<Share> SplitSecretWithCoefficients(BigInteger[] coefficients, int n, BigInteger prime)
    {
        return GenerateShares(coefficients, n, prime);
    }

    // logika wyliczania punktów wielomianu 
    // s_i = P(i) mod q
    private static List<Share> GenerateShares(BigInteger[] coefficients, int n, BigInteger prime)
    {
        List<Share> shares = new List<Share>();

        for (int x = 1; x <= n; x++)
        {
            // Obliczanie wartości wielomianu metodą Hornera lub klasycznie
            // P(x) = a_0 + a_1*x + a_2*x^2 ...
            BigInteger y = 0;
            BigInteger x_pow = 1; // x^0, x^1, x^2...

            for (int i = 0; i < coefficients.Length; i++)
            {
                // term = a_i * x^i
                BigInteger term = (coefficients[i] * x_pow);
                y = (y + term);

                // Zwiększamy potęgę x do następnej iteracji
                x_pow = (x_pow * x);
            }

            // Wynik musi być modulo q
            y = ZTTK_Lab4.Math.Mod(y, prime);

            shares.Add(new Share(x, y));
        }

        return shares;
    }
}