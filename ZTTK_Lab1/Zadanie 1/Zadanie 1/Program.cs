using System.Net;
using System.Text;

string szyfr = "Every day i wake up then i start to brake up";
int LA = 5;
int LB  = 6;
int[] czestosci = new int[26];
var szyfrogramBuilder = new StringBuilder();

foreach (char c in szyfr.ToUpper())
{
    if (c >= 'A' && c <= 'Z')
    {
        int pozycjaWAlfabecie = c - 'A';
        int szyfrogram = (LA * pozycjaWAlfabecie + LB) % 26;
        char zaszyfrowanaLitera = (char)('A' + szyfrogram);
        czestosci[szyfrogram]++;
        szyfrogramBuilder.Append(zaszyfrowanaLitera);
    }
    else
    {
        Console.Write(c);
        szyfrogramBuilder.Append(c);
    }
}

string szyfrogramString = szyfrogramBuilder.ToString();

var ENGLISH_FREQ = new Dictionary<char, double>
{
    {'A', 0.082},
    {'B', 0.015},
    {'C', 0.028},
    {'D', 0.043},
    {'E', 0.127},
    {'F', 0.022},
    {'G', 0.020},
    {'H', 0.061},
    {'I', 0.070},
    {'J', 0.002},
    {'K', 0.008},
    {'L', 0.040},
    {'M', 0.024},
    {'N', 0.067},
    {'O', 0.075},
    {'P', 0.019},
    {'Q', 0.001},
    {'R', 0.060},
    {'S', 0.063},
    {'T', 0.091},
    {'U', 0.028},
    {'V', 0.010},
    {'W', 0.023},
    {'X', 0.001},
    {'Y', 0.020},
    {'Z', 0.001}
};

Console.WriteLine("\n\nCzęstość wystąpień liter szyfrogramu:");
for (int i = 0; i < 26; i++)
{
    Console.WriteLine($"{(char)('A' + i)}: {czestosci[i]}");
}

// Wyświetlenie całego szyfrogramu jako string
Console.WriteLine($"\nSzyfrogram jako string:\n{szyfrogramString}");


// (a * x) mod m = 1
//x = a_inv * (y - b) mod 26
int ModInverse(int a, int m)
{
    a = a % m;
    for (int x = 1; x < m; x++)
        if ((a * x) % m == 1)
            return x;
    return -1;
}

// Lista wartości a mających odwrotność modulo 26
int[] possibleA = { 1, 3, 5, 7, 9, 11, 15, 17, 19, 21, 23, 25 };

List<(int a, int b, string plain)> results = new();

foreach (int a in possibleA)
{
    int a_inv = ModInverse(a, 26);
    if (a_inv == -1) continue;
    for (int b = 0; b < 26; b++)
    {
        char[] plain = new char[szyfrogramString.Length];
        for (int i = 0; i < szyfrogramString.Length; i++)
        {
            char c = szyfrogramString[i];
            if (c >= 'A' && c <= 'Z')
            {
                int y = c - 'A';
                int x = (a_inv * (y - b + 26)) % 26;
                plain[i] = (char)('A' + x);
            }
            else
            {
                plain[i] = c;
            }
        }
        string plainText = new string(plain);
        results.Add((a, b, plainText));
    }
}

// Wyświetl kilka przykładowych odszyfrowań (możesz tu dodać własną heurystykę, np. szukanie słów "THE", "AND" itp.)
foreach (var (a, b, plain) in results)
{
    // Prosta heurystyka: sprawdź, czy tekst zawiera "THE"
    if (plain.Contains("THE"))
    {
        Console.WriteLine($"Możliwy klucz: a={a}, b={b}");
        Console.WriteLine(plain);
        Console.WriteLine();
    }
}