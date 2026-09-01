using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
namespace Comparser.Comparser.Numbers;
public static class Static {
	private const int Bernoullis = 20;
	static Static() {
		var b2 = MakeBernoullisR();
		B2F = new double[b2.Length];
		for (int i = 0; i < b2.Length; ++i)
			B2F[i] = b2[i] * Factorial(2*i + 1);
		B2G = new double[b2.Length];
		for (int i = 0; i < b2.Length; ++i)
			B2G[i] = b2[i] / (2 + (6 + 4 * i) * i);
	}
	static double Factorial(int n) {
		ulong f = 1; // calculate natural factorial exactly
		while (n > 0)
			f *= (ulong)n--;
		return f;
	}

	private static double[] MakeBernoullisR() {
		
		var B = new double[Bernoullis];
		var maxIterations = Bernoullis << 1;
		B[0] = 1.0 / 6;
		// b[i] = sum[m=0..n]: sum[k=0..m]: (-1)^k * Comb(m k) * k^n * (m+1)^(-1)
		for (var i = 2; i < Bernoullis; ++i) {
			int e, o, m, tri, n = i << 1;
			var powers = new BigRational[n+1];
			for (var k = 2; k <= n; ++k) { // prepare k^n
				BigRational power = powers[k] = new(k, 1);
				for(int j = n - 1; j > 0; j >>= 1, power *= power)
					if((j & 1) == 1)
						powers[k] *= power;
			}
			BigRational nk, ns =  (powers[2] - new BigRational(7,2)) * new BigRational(1, 3); // m=0 + m=1 + m=2 ...first 3 terms
			var p = new int[maxIterations + 2]; // pascal triangle rows, the first term is always zero, because it is unused
			for (p[m = 2] = 1; (tri = m) < n; ) { // m-loop
				while (tri > 2) p[tri] += p[--tri]; // add the non-edge pascal triangle terms
				p[2] += m; // increment the 3rd column on the left edge of the pascal triangle
				if (m % 2 == 0) (e, o) = (m, ++m); else (o, e) = (m, ++m); // odd and even loops
				p[m] = 1; // add a new 1 at the end column on the right edge of the pascal triangle
				var m1 = 1.0 + m; // 0th and 1st k-term
				nk = new BigRational(-m, 1);  // 1st term (and 0th is always zero for n>0)
				do nk += new BigRational(p[e], 1) * powers[e]; // k-loop: even k terms, p[e] = Combinations(n,e)
				while (2 <= (e -= 2)); // decrement even k iterators down to 1
				do nk -=  new BigRational(p[o], 1) * powers[o]; // k-loop: odd k terms, p[o] = Combinations(n,o)
				while (2 <= (o -= 2)); // decrement odd k iterators down to 1
				ns += nk  * new BigRational(1,1 + m);
			}
			B[i - 1] = ns.FromD();
		}
		return B;
	}//1.0 / (1 + 1.0 / m);
	
	// bernouli numbers
	// B2[i]/(2(n+1))! ...for zeta euler
	public static readonly double[] B2F;
	// B2[i]/(4ii+6i+2) ...for gamma stirling
	public static readonly double[] B2G;
	public const double QTau = Math.Tau / 4;
	
	public static readonly double Ln10 = Math.Log(10);
	public static readonly double Ln2 = Math.Log(2);
	public static readonly double LnTh = Math.Log(Math.Tau) / 2;
	public static readonly double LnTau = Math.Log(Math.Tau);
	// gamma_n / n!
	public const double G1 = -0.0728158454836767248605863758749013191377363383; // gamma1
	public const double G2 = -0.0096903631928723184845303860352125293590658061 / 2; // gamma2 / 2!
	public const double G3 = 0.0020538344203033458661600465427533842857158044 / 6; // gamma3 / 3!
	public const double G4 = 0.0023253700654673000574681701775260680009044694 / 24; // gamma4 / 4!
	public const double G5 = 0.0007933238173010627017533348774444448307315394 / 120; // gamma5 / 5!

	public static int Gcd(int a, int b) {
		if (a < 0) a = -a;
		if (b < 0) b = -b;
		while (a != 0 && b != 0) {
			if (a > b) a %= b;
			else b %= a;
		}
		return a | b;
	}

	/// <summary>
	/// turn hsv into rgb color
	/// </summary>
	/// <param name="h">0-360 hue</param>
	/// <param name="s">0-1 saturation</param>
	/// <param name="v">0-1 value</param>
	/// <returns></returns>
	public static Color Hsv(double h, double s, double v) {
		if (s > 0) {
			var i = (int)Math.Truncate(h = h >= 360 ? 0 : h / 60);
			double f = h - i, p = v * (1.0 - s), q = v * (1.0 - s * f), t = v * (1.0 - s * (1.0 - f));
			var (r, g, b) = i switch {
				0 => (v, t, p),
				1 => (q, v, p),
				2 => (p, v, t),
				3 => (p, q, v),
				4 => (t, p, v),
				_ => (v, p, q)
			};
			return Color.FromArgb((byte)(255 * r), (byte)(255 * g), (byte)(255 * b));
		}
		var l = (byte)(255 * v);
		return Color.FromArgb(l, l, l);
	}
	public static string _i(string i, string c) => i is "1" or "-1" ? c : i + c; // redundant part of I.ToString
	//private static string _s(double v, int d) => d < 0 ? v.ToString() : v.ToString("F" + d.ToString()); 
	public static string _sr(double value, int d) {
		string r;
		switch (d) {
		case < 0:
			r = value.ToString(CultureInfo.InvariantCulture);
			return r == "-0" ? "0" : r;
		case 0:
			r = Math.Round(value).ToString(CultureInfo.InvariantCulture);
			return r == "-0" ? "0" : r;
		}
		var s = value.ToString($"F{d}");
		s = s.TrimEnd('0');
		if (s.EndsWith(".")) s = s.TrimEnd('.');
		return s == "-0" ? "0" : s;
	}
	public static string ValueToString(string units, double[] values, int d) {
		var v = new string[values.Length];
		for (var i = 0; i < values.Length; ++i)
			v[i] = _sr(values[i], d);
		var (first, result) = v[0] == "0" ? (true, "") : (false, v[0]);
		for (var ci = 0; values.Length > ++ci; first &= v[ci] == "0") {
			result += v[ci] switch { "0" => "",
			"-1" => (first ? "-" : " - ") + units[ci - 1],
			"1" =>  (first ?  "" : " + ") + units[ci - 1],
			_ => first ? v[ci] + units[ci - 1] : 
				(v[ci][0] == '-' ? 
					" - " + v[ci][1..] : 
					" + " + v[ci])
				+ units[ci - 1]
			};
		}
		return result == "" ? "0" : result;
		
		
		/*while (4 > ++ci && ijk[0] != 'x') {
			string l;
			for (r += s, l = ijk[..1], ijk = ijk[1..]; ci < 4 && v[ci] == "0"; ++ci) {
				l = ijk[..1];
				ijk = ijk[1..];
			}
			if (4 <= ci) {
				if (r == "")
					r = "0";
				break;
			}
			r += ci == 0 ? _i(v[ci], "") : _i(v[ci], l);
			s = " ";
		}
		return r;*/
	}

	#region Math
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Add(double a, double b) => a + b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Sub(double a, double b) => a - b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Mul(double a, double b) => a * b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Div(double a, double b) => a / b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Mod(double a, double b) => b == 0 ? 0 : a % b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Max(double a, double b) => double.IsNaN(a) ? a : Math.Max(a, b);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Min(double a, double b) => double.IsNaN(a) ? a : Math.Min(a, b);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Clamp(double r, double min, double max) => double.IsNaN(r) || double.IsNaN(min) || double.IsNaN(max) || min > max ? double.NaN : Math.Clamp(r, min, max);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Floor(double x) => (int)x;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Round(double x) => (int)Math.Round(x);
	public static double Sqr(double x) => x * x;
	public static double Lerp(double a, double b, double t) => a * (1 - t) + b * t;
	#endregion
}


