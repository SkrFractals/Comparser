using System.Runtime.CompilerServices;

namespace Comparser.Numbers;
public interface INumber<T> where T : INumber<T> {
	protected const double qTau = Math.Tau / 4;
	public abstract bool Is0();
	public abstract bool IsNaN();

	#region Export
	public abstract string ToString(int d);
	public static Color ToColorLog(T t,double repeatValue = 1) { var s = +t; return INumber<Complex>.Hsv((T.Arg(t) + Math.PI) * 360 / Math.Tau, 1 - Math.Exp(-s), Math.Log(s) * .5 % repeatValue); }
	public static Color ToColorLin(T t, double repeatValue = 1) { var s = +t; return INumber<Complex>.Hsv((T.Arg(t) + Math.PI) * 360 / Math.Tau, 1 - Math.Exp(-s), Math.Sqrt(+t) % repeatValue); }
	public static Color ToColorExp(T t) => INumber<Complex>.Hsv((T.Arg(t) + Math.PI) * 360 / Math.Tau, 1, 1 - Math.Exp(-+t));
	#endregion

	#region Constants
	public static T Unit() => T.MakeR(1);
	public abstract static T NaN();
	public abstract static T Zero();
	public abstract static T One();
	#endregion

	#region Basics
	public static bool IsTrue(T t) => +t >= 1;
	public static bool IsFalse(T t) => +t < 1;
	public static T True(bool t) => t ? Unit() : T.Zero();
	public abstract static bool Compare(T a, T b);
	public abstract static double Re(T t);
	public abstract static T MakeR(double r);
	public abstract static double Im(T t);
	public static T TI(T t) => T.MakeR(T.Im(t));
	public abstract static T operator !(T t);
	public abstract static T operator -(T t);
	public abstract static T operator ~(T t);
	public abstract static T U(T t);
	public abstract static T NU(T t);
	public abstract static double operator +(T t);
	public abstract static T Frac(T t);
	public abstract static T Trunc(T t);
	public abstract static T Floor(T t);
	public abstract static T Round(T t);
	public abstract static T Ceil(T t);
	public abstract static T Inv(T t);
	//public abstract static T InvITau(T c);
	public abstract static double Arg(T t);
	public abstract static T IArg(double p, T axis);
	public abstract static T Sqrt(T t);
	public abstract static T Sqr(T t);
	public abstract static T Cub(T t);
	public abstract static T Quart(T t);
	// |x|
	public static double Abs(T t) => Math.Sqrt(+t);
	// x / |x|
	public static T Sign(T t) => t / Abs(t);
	public abstract static T AbsComp(T t);
	public abstract static T Min(T a, T b);
	public abstract static T Max(T a, T b);
	public abstract static T Clamp(T t, T min, T max);
	#endregion

	#region Additions
	public static T Add(T a, T b) => a + b;
	public abstract static T operator +(T a, T b);
	public abstract static T operator +(T t, double r);
	public abstract static T operator +(double r, T t);
	public abstract static T AddNV(T t, double u);
	public abstract static T AddNV(double u, T t);
	#endregion

	#region Subtractions
	public static T Sub(T a, T b) => a - b;
	public abstract static T operator -(T a, T b);
	public abstract static T operator -(T t, double r);
	public abstract static T operator -(double r, T t);
	public abstract static T SubNV(T t, double i);
	public abstract static T SubNV(double i, T t);
	#endregion

	#region Multiplications
	public static T Mul(T a, T b) => a * b;
	public abstract static T operator *(T a, T b);
	public abstract static T operator *(T t, double r);
	public abstract static T operator *(double r, T t);
	//public abstract static T MulNV(T t, double i);
	//public abstract static T MulNV(double i, T t);
	#endregion

	#region Divisions
	public static T Div(T a, T b) => a / b;
	public abstract static T operator /(T a, T b);
	public abstract static T operator /(T t, double r);
	public abstract static T operator /(double r, T t);
	public abstract static T LDiv(T a, T b);
	public abstract static T operator %(T a, T b);
	//public abstract static T DivNV(T t, double i);
	//public abstract static T DivNV(double i, T t);

	#endregion

	#region ExpLogs
	public abstract static T Log(T t);
	public abstract static T LogH(T t);
	public abstract static T Exp(T t);
	public static T Pow(T a, T b) => a ^ b;
	public abstract static T operator ^(T a, T b);
	public abstract static T operator ^(T t, double r);
	public abstract static T operator ^(double r, T t);
	public abstract static T PowN1(T t);
	public abstract static T PowI(T t);
	public static T SoftMax(T a, T b) => T.Log(T.Exp(a) + T.Exp(b));
	public static T SoftMin(T a, T b) => -T.Log(T.Exp(-a) + T.Exp(-b));
	// SoftMax(x,0)
	public static T SoftAbs(T t) => T.Log(1 + T.Exp(t));
	// SoftMin(x,0)
	public static T SoftNeg(T t) => -T.Log(1 + T.Exp(-t));
	#endregion

	#region Hyperbolics
	public abstract static T Cosh(T t);
	public static T Sech(T t) => T.Inv(T.Cosh(t));
	public abstract static T Sinh(T t);
	public static T Csch(T t) => T.Inv(T.Sinh(t));
	public abstract static T Tanh(T t);
	public abstract static T Coth(T t);
	#endregion

	#region Trigonometrics
	public abstract static T Cos(T t);
	public static T Sec(T t) => T.Inv(T.Cos(t));
	public abstract static T Sin(T t);
	public static T Csc(T t) => T.Inv(T.Sin(t));
	public abstract static T Tan(T t);
	public abstract static T Cot(T t);
	#endregion

	#region ArcHyperbolics
	public abstract static T Acosh(T t);
	public abstract static T Asinh(T t);
	public abstract static T Atanh(T t);
	public abstract static T Acoth(T t);
	public static T IAcosh(T t) => T.Log(t + T.Sqrt(T.Sqr(t) - 1));
	public static T Asech(T t) => T.Acosh(T.Inv(t));
	public static T IAsinh(T t) => T.Log(t + T.Sqrt(T.Sqr(t) + 1));
	public static T Acsch(T t) => T.Asinh(T.Inv(t));
	public static T IAtanh(T t) => T.LogH((1 + t) / (1 - t));
	public static T IAcoth(T t) => T.LogH((t + 1) / (t - 1));
	#endregion

	#region ArcTrigonometrics
	public abstract static T Acos(T t);
	public abstract static T Asin(T t);
	public abstract static T Atan(T t);
	public abstract static T Acot(T t);
	public static T IAcos(T t) => T.Acosh(~t);
	public static T Asec(T t) => T.Acos(T.Inv(t));
	public static T IAsin(T t) => T.NU(T.Asinh(~t));
	public static T Acsc(T t) => T.Asin(T.Inv(t));
	public static T IAtan(T t) => T.NU(T.LogH(T.SubNV(1, t) / T.AddNV(1, t)));
	public static T IAcot(T t) => T.NU(T.LogH(T.AddNV(t, 1) / T.SubNV(t, 1)));
	#endregion

	#region Exotic Trigonometrics
	public static T Sinc(T t) => T.Sin(t) / t;
	public static T Nsinc(T t) => Sinc(Math.PI * t);
	public static T Sinhc(T t) => T.Sinh(t) / t;
	public static T Nsinhc(T t) => Sinhc(Math.PI * t);
	public static T Cosc(T t) => (1 - T.Cos(t)) / t;
	public abstract static T SinN1(T t);
	public abstract static T NISinI(T t);
	#endregion

	#region Simple Functions and Constants
	private static readonly double ln10 = Math.Log(10);
	private static readonly double ln2 = Math.Log(2);
	public static T e() => T.MakeR(Math.E);
	public static T pi() => T.MakeR(Math.PI);
	public static T tau() => T.MakeR(Math.Tau);
	public static T gamma() => T.MakeR(0.57721566490153286060651209008240243104215933593992); // Euler's constant
	public static T Log10(T t) => T.Log(t) / ln10;
	public static T Log2(T t) => T.Log(t) / ln2;
	public static T Exp10(T t) => T.Exp(ln10 * t);
	public static T Exp2(T t) => T.Exp(ln2 * t);
	public static T TRe(T t) => T.MakeR(T.Re(t));
	public static T Neg(T t) => -t;
	public static T Sqrabs(T t) => T.MakeR(+t);
	public static T TAbs(T t) => T.MakeR(Abs(t));
	public static T TArg(T t) => T.MakeR(T.Arg(t));
	public static T Conj(T t) => !t;
	public static T Cbrt(T t) => t ^ 1.0 / 3;
	public static T Gauss(T t) => T.Exp(-T.Sqr(t));
	#endregion

	#region Helpers
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static double Add(double a, double b) => a + b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static double Sub(double a, double b) => a - b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static double Mul(double a, double b) => a * b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static double Div(double a, double b) => a / b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static double Mod(double a, double b) => b == 0 ? double.NaN : a % b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static double Max(double a, double b) => double.IsNaN(a) ? a : Math.Max(a, b);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static double Min(double a, double b) => double.IsNaN(a) ? a : Math.Min(a, b);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static double Clamp(double r, double min, double max) => double.IsNaN(r) ? r : Math.Clamp(r, min, max);
	#endregion

	#region Special Functions
	/* not very precise, use Stirling instead:
	public static T Gamma_Weierstrass(T c, int n) {
		var s = c - Log(1 + c);
		do {
			var r = c / n;
			s += r - Log(1 + r);
		} while (1 < --n);
		return Exp(s - Log(c) - gamma * c);
	}*/

	private static readonly double lnTH = Math.Log(Math.Tau) / 2;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static T Log_Gamma_Stirling_Positive(T c, double off = -.5) {
		T cc = c * c, c4 = cc * cc, c8 = c4 * c4;
		// var correction = Inv(c *= 12) - Inv(30 * (c * cc)) + Inv(105 * (c * c4)) - Inv(140 * (c8/c)) + Inv(99 * (c8*c))
		// TODO determine if these are correction enough terms
		var correction = (140 / 99 - cc + (c4 - 3.5 * (c4 * cc - 30 * c8)) * 4 / 3) / (1680 * c8 * c); // this takes quite fewer operations   
		return (c + off) * T.Log(c) - c + lnTH + correction;
	}
	private static T Log_Factorial_Stirling_Positive(T c) => Log_Gamma_Stirling_Positive(c, .5);
	private static T Gamma_Stirling_Positive(T c) => T.Exp(Log_Gamma_Stirling_Positive(c));
	private static T Factorial_Stirling_Positive(T c) => T.Exp(Log_Factorial_Stirling_Positive(c));

	// Sin version: Γ(1-z)Γ(z)=π/sin(πz) => Γ(1.5+z)Γ(.5-z) = π/sin(π(.5+z)) => Γ(.5-z) = π/(Γ(1.5+z)sin(π(.5+z))); c = .5-z => z = .5-c
	//public static T Gamma_Stirling_Sin(T c) => c.R > .5 ? Gamma_Stirling_Positive(c) : Math.PI * Inv(Gamma_Stirling_Positive(2 - c) * Sin(new(Math.PI * c));

	// SinN1 version: Γ(1-z)Γ(z) = iτ/((-1)^z-(-1)^(-z)) = iτ/SinN1(z) =>
	// z -> z+.5: Γ(.5-z)Γ(.5+z) = iτ/SinN1(z+.5) => Γ(c) = iτ/(Γ(.5+z)SinN1(z+.5))
	// z = .5-c => iτ/(Γ(1-c))SinN1(c)) 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static T Gamma_Stirling_Negative(T c) => Math.Tau * T.Inv(Gamma_Stirling_Positive(1 - c) * T.SinN1(c));
	public static T Gamma_Stirling(T c) => T.Re(c) > .5 ? Gamma_Stirling_Positive(c) : Gamma_Stirling_Negative(c);
	// z! reflection: z!(-z)!nsinc(z) = 1 => c! = Inv((-c)!nsinc(-c)) = Inv((-c)!sinc(c*pi)) 
	// SinN1(1-c) = SinN1(c)
	public static T Factorial_Stirling(T c) => 0 <= T.Re(c) ? Factorial_Stirling_Positive(c) : T.Inv(Factorial_Stirling_Positive(-c) * Sinc(Math.PI * c));
	private static ulong IntFactorial(int n) {
		ulong f = 1; // calculate natural factorial exactly
		while (n > 0)
			f *= (ulong)n--;
		return f;
	}
	public static T Factorial(T c) {
		var re = T.Re(c);
		var r = (int)Math.Floor(re);
		// Is it a natural number?
		if (r == re && r >= 0 && (c - T.MakeR(re)).Is0()) {
			var f = 1.0; // calculate natural factorial exactly (as far as double's mantissa allows)
			while (r > 0)
				f *= r--;
			return T.MakeR(f);
		}
		return Factorial_Stirling(c); // iterate complex factorial for n iterations
	}
	/// <summary>
	/// Optimized Hasse/Sondow approximation of zeta function (without the last division by "c + 1")
	/// </summary>
	/// <param name="c">T input (often written as "s")</param>
	/// <param name="i">Iteration ceiling. n=0..max(1,i). Terms 0 and 1 are precomputed, you'll get those even if you input 0 or negative.</param>
	/// <returns></returns>
	private static T Zeta_Hasse(T c) {
		// zeta(c) = (c-1)^(-1) * sum[n=0..i]: (n+1)^(-1) * sum[k=0..n]: (-1)^k * Combination(n,k) * (k + 1)^(1 - c)
		T nk, ns = 1.5 - (2 ^ -c), c1 = 1 - c; // 0th n term is 1, 1st term is (1-2^(1-c))/2 = 0.5-2^(-c), precount that, and loop n = n->2
		const int maxIterations = 42;
		T one, term = one = T.MakeR(1);
		var p = new int[maxIterations + 2]; // pascal triangle rows, the first term is always zero, because it is unused
		for (int t, n = p[1] = 1; (t = n) < maxIterations && +term / Math.Max(1, +ns) > 1e-20; ns += term = nk / (n + 1)) {
			while (t > 1)
				p[t] += p[--t];             // add the non-edge pascal triangle terms
			(var e, var o) = n % 2 == 0 ? (n, ++n) : (++n, n); // even and odd k iterators (so i don't have to (-1)^k)
			++p[p[n] = 1];                  // increment 2nd term in the pascal row, last term (one) in the pascal row is a new one
			nk = one;                       // zeroth k term is 1, precount that and loop k = n->1
			do nk += p[e] * (e + 1 ^ c1); // even k terms, p[e] = Combinations(n,e)
			while (1 <= (e -= 2));          // decrement even k iterators down to 1
			do nk -= p[o] * (o + 1 ^ c1); // odd k terms, p[o] = Combinations(n,o)
			while (1 <= (o -= 2));          // decrement odd k iterators down to 1
		}
		;
		return ns; // multiplied by Inc(c - 1) outside this function, as c - 1 is precomputed there
	}
	// Precomputed even Bernoulli numbers B2, B4, / factorial
	static readonly double[] B2k = {
		1.0/(6        *IntFactorial(2)),  // B2
        -1.0/(30      *IntFactorial(4)),  // B4
        1.0/(42       *IntFactorial(6)),  // B6
        -1.0/(30      *IntFactorial(8)),  // B8
        5.0/(66       *IntFactorial(10)), // ...
		-691.0/(2730  *IntFactorial(12)),
		7.0/(6        *IntFactorial(14)),
		-3617.0/(510  *IntFactorial(16)),
		43867.0/(498  *IntFactorial(18)),
		-174611.0/(330*IntFactorial(20)),
        // ... add more
    };
	private static T Zeta_Euler(T s) { // using B2k bernouli numbers/factorials
		int N = 32, M = B2k.Length;
		var S = T.MakeR(1);
		for (byte n = 2; n < N; ++n)
			S += n ^ -s;
		var sum = S + (S = N ^ -s) * (.5 + N / (s - 1)) + B2k[0] * (S *= s / N);
		N *= N;
		for (var k = 1; k < M; ++k)
			sum += B2k[k] * (S *= (s + 2 * k + 1) * (s + 2 * k + 2) / N);
		return sum;
	}
	// Elegant Zeta reflection: ζ(1-s)τ^s = CosI(s)Γ(s)ζ(s)
	// ζ(s) = (ζ(1-s)τ^s) / (CosI(s)Γ(s))
	// Zeta_Euler_Reflected(c) = Zeta_Euler(c) * R
	// R = (τ ^ c) * Inv(CosI(c) * Gamma_Stirling_Negative(c)) // substitute Inv and Gamma_Stirling_Negative
	// = (τ ^ c) / (CosI(c) * InvITau(Gamma_Stirling_Positive(1 - c) * SinN1(c))) // substitute InvITau
	// = (τ ^ c) / (iτCosI(c) / (Gamma_Stirling_Positive(1 - c) * SinN1(c))) // simplify division
	// = (τ ^ c) * Gamma_Stirling_Positive(1 - c) * SinN1(c) / (CosI(c) * i * tau) // move iτ
	// = -i(τ ^ (c - 1)) * Gamma_Stirling_Positive(1 - c) * SinN1(c) / CosI(c) // substitute NISinI, and c - 1 with the difference from fole that was precomputed for the condition
	// = (τ ^ c1) * Gamma_Stirling_Positive(1 - c) * NISinI(c)
	private const double g1 = -0.0728158454836767248605863758749013191377363383; // gamma1
	private const double g2 = -0.0096903631928723184845303860352125293590658061 / 2; // gamma2 / 2!
	private const double g3 = 0.0020538344203033458661600465427533842857158044 / 6; // gamma3 / 3!
	private const double g4 = 0.0023253700654673000574681701775260680009044694 / 24; // gamma4 / 4!
	private const double g5 = 0.0007933238173010627017533348774444448307315394 / 120; // gamma5 / 5!
																					  // TODO determine if the 5 terms in laurent series are appropriate for the e-4 distance, and that the Hasse-Euler boundary is appropriate for 2 distance, and that Hasse has appropriate number of terms
	public static T Zeta(T c) {
		// relative to the pole (useful in many places in this algo, including the reflection)
		var c1 = c - 1; 
		// squared distance from the pole
		var pole = +c1; 
		// exactly the pole - return infinity
		return pole == 0 ? T.MakeR(double.PositiveInfinity)
			// very near the pole - use Laurent that is excellent when this near, hopefully 5 terms are enough for the Laurent series with the distance from to pole up to e-4
			: pole < 1e-8 ? T.Inv(c1) + gamma() - c1 * (g1 - c1 * (g2 - c1 * (g3 - c1 * (g4 - c1 * g5))))
			// near the pole - use general Hasse that is decent everywhere
			: pole < 4 ? Zeta_Hasse(c) * T.Inv(c1)
			// far from pole - use Euler that is excellent when far from it
			: T.Re(c) >= .5 ? Zeta_Euler(c)
			// negative and far from the pole - reflect Euler (using the formula derived above)
			: Zeta_Euler(c) * (tau() ^ c1) * Gamma_Stirling_Positive(1 - c) * T.NISinI(c);
	}
	#endregion

	/// <summary>
	/// turn hsv into rgb color
	/// </summary>
	/// <param name="h">0-360 hue</param>
	/// <param name="s">0-1 saturation</param>
	/// <param name="v">0-1 value</param>
	/// <returns></returns>
	public static Color Hsv(double h, double s, double v) {
		if (s > 0) {
			double r, g, b, f, p, q, t;
			h = h == 360 ? 0 : h / 60;
			var i = (int)Math.Truncate(h);
			f = h - i;
			p = v * (1.0 - s);
			q = v * (1.0 - s * f);
			t = v * (1.0 - s * (1.0 - f));
			(r, g, b) = i switch {
				0 => (v, t, p),
				1 => (q, v, p),
				2 => (p, v, t),
				3 => (p, q, v),
				4 => (t, p, v),
				_ => (v, p, q)
			};
			return Color.FromArgb((byte)(255 * r), (byte)(255 * g), (byte)(255 * b));
		} else {
			var l = (byte)(255 * v);
			return Color.FromArgb(l, l, l);
		}
	}
	protected static string _i(string i, string c) => i == "1" ? c : i + c; // redundant part of I.ToString
																	 //private static string _s(double v, int d) => d < 0 ? v.ToString() : v.ToString("F" + d.ToString()); 
	protected static string _sr(double value, int d) { // decimals ToString
		if (d < 0)
			return value.ToString();
		if (d == 0)
			return Math.Round(value).ToString();
		var s = value.ToString($"F{d}");
		s = s.TrimEnd('0');
		if (s.EndsWith(".")) s = s.TrimEnd('.');
		return s == "-0" ? "0" : s;
	}
}
