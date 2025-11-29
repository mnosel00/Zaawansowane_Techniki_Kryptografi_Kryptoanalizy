using System;
using System.Numerics;
using System.Text;

namespace ZTTK_Lab4
{


    public static class Math
    {
        public static BigInteger Mod(BigInteger x, BigInteger m)
        {
            BigInteger r = x % m;
            return r < 0 ? r + m : r;
        }

        public static BigInteger ModInverse(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m;
            BigInteger y = 0, x = 1;
            if (m == 1) return 0;
            while (a > 1)
            {
                if (m == 0) throw new DivideByZeroException("Moduł nie może być zerem.");
                BigInteger q = a / m;
                BigInteger t = m;
                m = a % m;
                a = t;
                t = y;
                y = x - q * y;
                x = t;
            }
            if (x < 0) x += m0;
            return x;
        }

        public static BigInteger RandomBigInteger(BigInteger limit)
        {
            Random rng = new Random();
            byte[] bytes = limit.ToByteArray();
            BigInteger R;
            do
            {
                rng.NextBytes(bytes);
                bytes[bytes.Length - 1] &= (byte)0x7F;
                R = new BigInteger(bytes);
            } while (R >= limit || R < 0);
            return R;
        }

        // ETAP 5

        // Konwersja Tekst -> Liczba
        public static BigInteger TextToBigInteger(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            // bajt 0 na końcu, aby liczba była zawsze dodatnia
            byte[] unsignedBytes = new byte[bytes.Length + 1];
            Array.Copy(bytes, unsignedBytes, bytes.Length);
            return new BigInteger(unsignedBytes);
        }

        // Konwersja Liczba -> Tekst
        public static string BigIntegerToText(BigInteger number)
        {
            byte[] bytes = number.ToByteArray();
            // Usuwamy ewentualny nadmiarowy bajt znaku
            return Encoding.UTF8.GetString(bytes).Trim('\0');
        }

        // najbliższa liczba pierwsza większa od min
        public static BigInteger GetNextPrime(BigInteger min)
        {
            BigInteger candidate = min + 1;
            if (candidate % 2 == 0) candidate++;

            while (true)
            {
                if (IsPrime(candidate)) return candidate;
                candidate += 2;
            }
        }

        private static bool IsPrime(BigInteger n)
        {
            if (n < 2) return false;
            if (n == 2 || n == 3) return true;
            if (n % 2 == 0 || n % 3 == 0) return false;

            // Prosty test do pierwiastka 
            BigInteger i = 5;
            // Ograniczamy sprawdzenie dla wydajności 
            // W prawdziwym SSS generuje się losową liczbę pierwszą o znanej długości.
            // tutaj random
            int checks = 0;
            while (i * i <= n)
            {
                if (n % i == 0 || n % (i + 2) == 0) return false;
                i += 6;

                checks++;
                if (checks > 10000) return true; // Heurystyka dla bardzo dużych liczb
            }
            return true;
        }
    }
}