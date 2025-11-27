using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZTTK_Lab3
{
    public static class AsconHash
    {
        public const int Rate = 8; // 64 bity = 8 bajtów
        public const int HashSize = 32; // 256 bitów = 32 bajty

        public static byte[] ComputeHash(byte[] input)
        {
            // Stan ASCON składa się z 5 słów 64-bitowych (x0...x4)
            ulong x0 = 0, x1 = 0, x2 = 0, x3 = 0, x4 = 0;

            // 1. Inicjalizacja (IV dla ASCON-HASH)
            // IV: 0x00400c0000000100 (dla Ascon-Hash)
            x0 = 0x00400c0000000100UL;

            // 2. Absorpcja (Wchłanianie wiadomości)
            int len = input.Length;
            int offset = 0;

            while (len >= Rate)
            {
                x0 ^= BytesToUlong(input, offset);
                Permutation(ref x0, ref x1, ref x2, ref x3, ref x4, 12); // P12
                offset += Rate;
                len -= Rate;
            }

            // Padding (Dopełnienie)
            // Dodajemy bit '1' i zera
            ulong pad = 0x8000000000000000UL >> (0); // Upuść bajty, jeśli trzeba (uproszczone dla pełnych bloków, ale tutaj robimy ogólnie)

            // Obsługa końcówki (partial block)
            ulong lastBlock = 0;
            for (int i = 0; i < len; i++)
            {
                lastBlock |= (ulong)input[offset + i] << (56 - 8 * i);
            }
            lastBlock |= 0x8000000000000000UL >> (8 * len);

            x0 ^= lastBlock;

            // Finalna permutacja P12 po paddingu
            Permutation(ref x0, ref x1, ref x2, ref x3, ref x4, 12);

            // 3. Wyciskanie (Squeezing) - generowanie skrótu
            byte[] hash = new byte[HashSize];
            for (int i = 0; i < HashSize; i += 8)
            {
                UlongToBytes(x0, hash, i);
                Permutation(ref x0, ref x1, ref x2, ref x3, ref x4, 12);
            }

            return hash;
        }

        // Pomocnicza funkcja permutacji (rdzeń ASCON)
        private static void Permutation(ref ulong x0, ref ulong x1, ref ulong x2, ref ulong x3, ref ulong x4, int rounds)
        {
            for (int i = 12 - rounds; i < 12; i++)
            {
                // Dodanie stałych rundy
                x2 ^= ((ulong)(0xf0 - i * 0x10 + i * 0x1) << 56) | (ulong)(0xf0 - i * 0x10 + i * 0x1);

                // Warstwa substytucji (S-box)
                x0 ^= x4; x4 ^= x3; x2 ^= x1;
                ulong t0 = x0, t1 = x1, t2 = x2, t3 = x3, t4 = x4;
                t0 = ~t0; t1 = ~t1; t2 = ~t2; t3 = ~t3; t4 = ~t4;
                t0 &= x1; t1 &= x2; t2 &= x3; t3 &= x4; t4 &= x0;
                x0 ^= t1; x1 ^= t2; x2 ^= t3; x3 ^= t4; x4 ^= t0;
                x1 ^= x0; x0 ^= x4; x3 ^= x2; x2 = ~x2;

                // Warstwa dyfuzji (Linear diffusion)
                x0 ^= RotateRight(x0, 19) ^ RotateRight(x0, 28);
                x1 ^= RotateRight(x1, 61) ^ RotateRight(x1, 39);
                x2 ^= RotateRight(x2, 1) ^ RotateRight(x2, 6);
                x3 ^= RotateRight(x3, 10) ^ RotateRight(x3, 17);
                x4 ^= RotateRight(x4, 7) ^ RotateRight(x4, 41);
            }
        }

        private static ulong RotateRight(ulong value, int bits)
        {
            return (value >> bits) | (value << (64 - bits));
        }

        private static ulong BytesToUlong(byte[] b, int offset)
        {
            return ((ulong)b[offset] << 56) | ((ulong)b[offset + 1] << 48) |
                   ((ulong)b[offset + 2] << 40) | ((ulong)b[offset + 3] << 32) |
                   ((ulong)b[offset + 4] << 24) | ((ulong)b[offset + 5] << 16) |
                   ((ulong)b[offset + 6] << 8) | (ulong)b[offset + 7];
        }

        private static void UlongToBytes(ulong value, byte[] b, int offset)
        {
            b[offset] = (byte)(value >> 56);
            b[offset + 1] = (byte)(value >> 48);
            b[offset + 2] = (byte)(value >> 40);
            b[offset + 3] = (byte)(value >> 32);
            b[offset + 4] = (byte)(value >> 24);
            b[offset + 5] = (byte)(value >> 16);
            b[offset + 6] = (byte)(value >> 8);
            b[offset + 7] = (byte)(value);
        }
    }
}
