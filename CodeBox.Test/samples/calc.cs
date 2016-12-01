using System;using System.Threading;static class Program{    static void Main()    {        Console.WriteLine("Welcome to console calculator!");        Console.WriteLine("Enter mathematic expressions in reverse notation, e.g. '+ 2 3'.");        Console.WriteLine("To exit enter 'bye'.");        Console.WriteLine();        Cycle();    }    static void Cycle()    {        while (true) {            Console.Write(">");            var str = Console.ReadLine();            if (string.Equals(str, "bye", StringComparison.OrdinalIgnoreCase))                return;            var exp = ParseInput(str);            if (exp != null)                Console.WriteLine(exp.Eval());            Console.WriteLine();        }    }    static Expression ParseInput(string input)    {        var arr = input.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);        var exp = new Expression();        for (var i = 0; i < arr.Length; i++) {            var str = arr[i];            if (i == 0) {                if (str == "+")                    exp.Op = Operation.Add;                else if (str == "-")                    exp.Op = Operation.Sub;                else if (str == "*")                    exp.Op = Operation.Mul;                else if (str == "/")                    exp.Op = Operation.Div;                else {                    Error($"Unknown operation: {str}.");                    return null;                }            }            else {                double num;                if (!double.TryParse(str, NumberFormats.Number, CultureInfo.InvariantCulture, out num)) {                    Error($"Invalid number literal: {str}.");                    return null;                }                exp.Arguments.Add(num);            }        }        return exp;    }    static void Error(string message)    {        Console.WriteLine("Error: {0}", message);    }}enum Operation{    Add,    Sub,    Mul,    Div}class Expression{    public Operation Op { get; set; }    public List<double> Arguments { get; } = new List<double>();    public double Eval()    {        double? res = null;        foreach (var n in Arguments) {            if (res != null)                res = Bin(res.Value, n);            else                res = n;        }        return res;    }    private double Bin(double x, double y)    {        switch (Op) {            case Operation.Add: return x + y;            case Operation.Sub: return x - y;            case Operation.Mul: return x * y;            case Operation.Div: return x / y;            default: return 0d;        }    }}/*  using System;

/// <summary>
/// Contains approximate string matching
/// </summary>
static class LevenshteinDistance
{
    /// <summary>
    /// Compute the distance between two strings.
    /// </summary>
    public static int Compute(string s, string t)
    {
	int n = s.Length;
	int m = t.Length;
	int[,] d = new int[n + 1, m + 1];

	// Step 1
	if (n == 0)
	{
	    return m;
	}

	if (m == 0)
	{
	    return n;
	}

	// Step 2
	for (int i = 0; i <= n; d[i, 0] = i++)
	{
	}

	for (int j = 0; j <= m; d[0, j] = j++)
	{
	}

	// Step 3
	for (int i = 1; i <= n; i++)
	{
	    //Step 4
	    for (int j = 1; j <= m; j++)
	    {
		// Step 5
		int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

		// Step 6
		d[i, j] = Math.Min(
		    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
		    d[i - 1, j - 1] + cost);
	    }
	}
	// Step 7
	return d[n, m];
    }
}

class Program
{
    static void Main()
    {
	Console.WriteLine(LevenshteinDistance.Compute("aunt", "ant"));
	Console.WriteLine(LevenshteinDistance.Compute("Sam", "Samantha"));
	Console.WriteLine(LevenshteinDistance.Compute("flomax", "volmax"));
    }
}
*/
 * 