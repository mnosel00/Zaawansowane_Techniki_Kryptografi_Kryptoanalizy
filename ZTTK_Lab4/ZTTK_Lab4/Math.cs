using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ZTTK_Lab4
{
    public class Math
    {
        public static BigInteger Mod(BigInteger x, BigInteger m)
        {
            BigInteger r = x % m;
            return r < 0 ? r + m : r;
        }

        // Algorytm Euklidesa do znalezienia odwrotności modulo
        public static BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m;
            BigInteger y = 0, x = 1;

            if (m == 1) return 0;

            while (a > 1)
            {
                if (m == 0) throw new DivideByZeroException("Moduł nie może być zerem (liczba pierwsza wymagana).");

                // q iloraz
                BigInteger q = a / m;
                BigInteger t = m;

                // m reszta
                m = a % m;
                a = t;
                t = y;

                // Aktualizacja x i y
                y = x - q * y;
                x = t;
            }

            // Upewnij się, że x jest dodatnie
            if (x < 0) x += m0;

            return x;
        }

        // Generowanie losowej dużej liczby (BigInteger)
        public static BigInteger RandomBigInteger(BigInteger limit)
        {
            Random rng = new Random();
            byte[] bytes = limit.ToByteArray();
            BigInteger R;

            do
            {
                rng.NextBytes(bytes);
                bytes[bytes.Length - 1] &= (byte)0x7F; // Upewnij się, że liczba jest dodatnia
                R = new BigInteger(bytes);
            } while (R >= limit || R < 0);

            return R;
        }
    }
}
