using System.Globalization;
using System.Runtime.CompilerServices;

namespace Comparser.Comparser.Numbers;
public interface INumber<T> where T : INumber<T> {
	protected const double QTau = Math.Tau / 4;
	public bool Is0();
	public bool IsNaN();

	#region Export
	public string ToString(int d);
	public static Color ToColorLog(T t,double repeatValue = 1) { var s = +t; return INumber<Complex>.Hsv((T.Arg(t) + Math.PI) * 360 / Math.Tau, 1 - Math.Exp(-s), Math.Log(s) * .5 % repeatValue); }
	public static Color ToColorLin(T t, double repeatValue = 1) { var s = +t; return INumber<Complex>.Hsv((T.Arg(t) + Math.PI) * 360 / Math.Tau, 1 - Math.Exp(-s), Math.Sqrt(+t) % repeatValue); }
	public static Color ToColorExp(T t) => INumber<Complex>.Hsv((T.Arg(t) + Math.PI) * 360 / Math.Tau, 1, 1 - Math.Exp(-+t));
	#endregion

	#region Constants
	public static T Unit() => T.MakeR(1);
	public static abstract T NaN();
	public static abstract T Zero();
	public static abstract T One();
	#endregion

	#region Basics
	public static bool IsTrue(T t) => +t >= 1;
	public static bool IsFalse(T t) => +t < 1;
	public static T True(bool t) => t ? Unit() : T.Zero();
	public static abstract bool Compare(T a, T b);
	public static abstract double Re(T t);
	public static abstract T MakeR(double r);
	public static abstract double Im(T t);
	public static T T_I(T t) => T.MakeR(T.Im(t));
	public static abstract T operator !(T t);
	public static abstract T operator -(T t);
	public static abstract T operator ~(T t);
	public static abstract T U(T t);
	public static abstract T MulU(T r);
	public static abstract T NegU(T t);
	public static abstract double operator +(T t);
	public static abstract T Frac(T t);
	public static abstract T Trunc(T t);
	public static abstract T Floor(T t);
	public static abstract T Round(T t);
	public static abstract T Ceil(T t);
	public static abstract T Inv(T t);
	public static abstract double Arg(T t);
	public static abstract T InvArg(double p, T axis);
	public static abstract T Axis(T t);
	public static abstract T Sqrt(T t);
	public static abstract T Sqr(T t);
	public static abstract T Cub(T t);
	public static abstract T Quart(T t);
	// |x|
	public static double Abs(T t) => Math.Sqrt(+t);
	// x / |x|
	public static T Sign(T t) => t / Abs(t);
	public static abstract T AbsComp(T t);
	public static abstract T Min(T a, T b);
	public static abstract T Max(T a, T b);
	public static abstract T Clamp(T t, T min, T max);
	#endregion

	#region Additions
	public static T Add(T a, T b) => a + b;
	public static abstract T operator +(T a, T b);
	public static abstract T operator +(T t, double r);
	public static abstract T operator +(double r, T t);
	public static abstract T AddV(T t, double u);
	public static abstract T AddV(double u, T t);
	#endregion

	#region Subtractions
	public static T Sub(T a, T b) => a - b;
	public static abstract T operator -(T a, T b);
	public static abstract T operator -(T t, double r);
	public static abstract T operator -(double r, T t);
	public static abstract T SubV(T t, double i);
	public static abstract T SubV(double i, T t);
	#endregion

	#region Multiplications
	public static T Mul(T a, T b) => a * b;
	public static abstract T operator *(T a, T b);
	public static abstract T operator *(T t, double r);
	public static abstract T operator *(double r, T t);
	//public abstract static T MulNV(T t, double i);
	//public abstract static T MulNV(double i, T t);
	#endregion

	#region Divisions
	//public static T Div(T a, T b) => a / b;
	public static abstract T operator /(T a, T b);
	public static abstract T operator /(T t, double r);
	public static abstract T operator /(double r, T t);
	public static abstract T LDiv(T a, T b);
	public static abstract T operator %(T a, T b);
	//public abstract static T DivNV(T t, double i);
	//public abstract static T DivNV(double i, T t);

	#endregion

	#region ExpLogs
	public static abstract T Log(T t);
	public static abstract T LogH(T t);
	public static abstract T Exp(T t);
	public static T Pow(T a, T b) => a ^ b;
	public static abstract T operator ^(T a, T b);
	public static abstract T operator ^(T t, double r);
	public static abstract T operator ^(double r, T t);
	public static abstract T PowN1(T t);
	public static abstract T PowI(T t);
	public static T SoftMax(T a, T b) => T.Log(T.Exp(a) + T.Exp(b));
	public static T SoftMin(T a, T b) => -T.Log(T.Exp(-a) + T.Exp(-b));
	// SoftMax(x,0)
	public static T SoftAbs(T t) => T.Log(1 + T.Exp(t));
	// SoftMin(x,0)
	public static T SoftNeg(T t) => -T.Log(1 + T.Exp(-t));
	#endregion

	#region Hyperbolics
	public static abstract T Cosh(T t);
	public static T Sech(T t) => T.Inv(T.Cosh(t));
	public static abstract T Sinh(T t);
	public static T Csch(T t) => T.Inv(T.Sinh(t));
	public static abstract T Tanh(T t);
	public static abstract T Coth(T t);
	#endregion

	#region Trigonometrics
	public static abstract T Cos(T t);
	public static T Sec(T t) => T.Inv(T.Cos(t));
	public static abstract T Sin(T t);
	public static T Csc(T t) => T.Inv(T.Sin(t));
	public static abstract T Tan(T t);
	public static abstract T Cot(T t);
	#endregion

	#region ArcHyperbolics
	public static abstract T Acosh(T t);
	public static abstract T Asinh(T t);
	public static abstract T Atanh(T t);
	public static abstract T Acoth(T t);
	public static T I_Acosh(T t) => T.Log(t + T.Sqrt(T.Sqr(t) - 1));
	public static T Asech(T t) => T.Acosh(T.Inv(t));
	public static T I_Asinh(T t) => T.Log(t + T.Sqrt(T.Sqr(t) + 1));
	public static T Acsch(T t) => T.Asinh(T.Inv(t));
	public static T I_Atanh(T t) => T.LogH((1 + t) / (1 - t));
	public static T I_Acoth(T t) => T.LogH((t + 1) / (t - 1));
	#endregion

	#region ArcTrigonometrics
	public static abstract T Acos(T t);
	public static abstract T Asin(T t);
	public static abstract T Atan(T t);
	public static abstract T Acot(T t);
	public static T I_Acos(T t) => T.Acosh(~t);
	public static T Asec(T t) => T.Acos(T.Inv(t));
	public static T I_Asin(T t) => T.NegU(T.Asinh(~t));
	public static T Acsc(T t) => T.Asin(T.Inv(t));
	public static T I_Atan(T t) => T.NegU(T.LogH(T.SubV(1, t) / T.AddV(1, t)));
	public static T I_Acot(T t) => T.NegU(T.LogH(T.AddV(t, 1) / T.SubV(t, 1)));
	#endregion

	#region Exotic Trigonometrics
	public static T Sinc(T t) => T.Sin(t) / t;
	public static T Nsinc(T t) => Sinc(Math.PI * t);
	public static T Sinhc(T t) => T.Sinh(t) / t;
	public static T Nsinhc(T t) => Sinhc(Math.PI * t);
	public static T Cosc(T t) => (1 - T.Cos(t)) / t;
	public static abstract T Sin_P(T t);
	public static abstract T Sin_2Q(T t);
	#endregion

	#region Simple Functions and Constants
	private static readonly double Ln10 = Math.Log(10);
	private static readonly double Ln2 = Math.Log(2);
	private static readonly double LnTh = Math.Log(Math.Tau) / 2;
	public static T C_E() => T.MakeR(Math.E);
	public static T C_Pi() => T.MakeR(Math.PI);
	public static T C_Tau() => T.MakeR(Math.Tau);
	public static T C_Gamma() => T.MakeR(0.57721566490153286060651209008240243104215933593992); // Euler's constant
	public static T Log10(T t) => T.Log(t) / Ln10;
	public static T Log2(T t) => T.Log(t) / Ln2;
	public static T Exp10(T t) => T.Exp(Ln10 * t);
	public static T Exp2(T t) => T.Exp(Ln2 * t);
	public static T T_Re(T t) => T.MakeR(T.Re(t));
	public static T Neg(T t) => -t;
	public static T SqrAbs(T t) => T.MakeR(+t);
	public static T T_Abs(T t) => T.MakeR(Abs(t));
	public static T T_Arg(T t) => T.MakeR(T.Arg(t));
	public static T Conj(T t) => !t;
	public static T Cbrt(T t) => t ^ 1.0 / 3;
	#endregion

	#region Helpers
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static protected double Add(double a, double b) => a + b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static protected double Sub(double a, double b) => a - b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static protected double Mul(double a, double b) => a * b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static protected double Div(double a, double b) => a / b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static protected double Mod(double a, double b) => b == 0 ? double.NaN : a % b;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static protected double Max(double a, double b) => double.IsNaN(a) ? a : Math.Max(a, b);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static protected double Min(double a, double b) => double.IsNaN(a) ? a : Math.Min(a, b);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static protected double Clamp(double r, double min, double max) => double.IsNaN(r) ? r : Math.Clamp(r, min, max);
	public static T ComplexOp(T t, Func<Complex, Complex> func) {
		var r = func(INumber<T>.Abs(t) * Complex.Complex_InvArg(T.Arg(t)));
		return INumber<Complex>.Abs(r) * T.InvArg(Complex.Arg(r), T.Axis(t));
	}
	#endregion

	#region Special Functions
	public static abstract T Gauss(T t);
	public static abstract T Gamma(T t);
	public static abstract T Factorial(T t);
	public static abstract T Zeta(T t);

	public static T I_Gauss(T t) => T.Exp(-T.Sqr(t));
	/* not very precise, use Stirling instead:
	public static T Gamma_Weierstrass(T c, int n) {
		var s = c - Log(1 + c);
		do {
			var r = c / n;
			s += r - Log(1 + r);
		} while (1 < --n);
		return Exp(s - Log(c) - gamma * c);
	}*/

	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static T Log_Gamma_Stirling_Positive(T t, double off = -.5) {
		T tt = t * t, t4 = tt * tt, t8 = t4 * t4;
		// var correction = Inv(c *= 12) - Inv(30 * (c * cc)) + Inv(105 * (c * c4)) - Inv(140 * (c8/c)) + Inv(99 * (c8*c))
		// TODO determine if these are correction enough terms
		var correction = (140.0 / 99 - tt + (t4 - 3.5 * (t4 * tt - 30 * t8)) * 4 / 3) / (1680 * t8 * t); // this takes quite fewer operations   
		return (t + off) * T.Log(t) - t + LnTh + correction;
	}
	private static T Log_Factorial_Stirling_Positive(T t) => Log_Gamma_Stirling_Positive(t, .5);
	private static T Gamma_Stirling_Positive(T t) => T.Exp(Log_Gamma_Stirling_Positive(t));
	private static T Factorial_Stirling_Positive(T t) => T.Exp(Log_Factorial_Stirling_Positive(t));

	// Sin version: Γ(1-z)Γ(z)=π/sin(πz) => Γ(1.5+z)Γ(.5-z) = π/sin(π(.5+z)) => Γ(.5-z) = π/(Γ(1.5+z)sin(π(.5+z))); c = .5-z => z = .5-c
	//public static T Gamma_Stirling_Sin(T c) => c.R > .5 ? Gamma_Stirling_Positive(c) : Math.PI * Inv(Gamma_Stirling_Positive(2 - c) * Sin(new(Math.PI * c));

	// SinN1 version: Γ(1-z)Γ(z) = iτ/((-1)^z-(-1)^(-z)) = iτ/SinN1(z) =>
	// z -> z+.5: Γ(.5-z)Γ(.5+z) = iτ/SinN1(z+.5) => Γ(c) = iτ/(Γ(.5+z)SinN1(z+.5))
	// z = .5-c => iτ/(Γ(1-c))SinN1(c)) 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static T Gamma_Stirling_Negative(T t) => Math.Tau * T.Inv(Gamma_Stirling_Positive(1 - t) * T.Sin_P(t));
	public static T I_Gamma(T t) => T.Re(t) > .5 ? Gamma_Stirling_Positive(t) : Gamma_Stirling_Negative(t);
	// z! reflection: z!(-z)!nsinc(z) = 1 => c! = Inv((-c)!nsinc(-c)) = Inv((-c)!sinc(c*pi)) 
	// SinN1(1-c) = SinN1(c)
	private static T Factorial_Stirling(T t) => 0 <= T.Re(t) ? Factorial_Stirling_Positive(t) : T.Inv(Factorial_Stirling_Positive(-t) * Sinc(Math.PI * t));
	private static ulong IntFactorial(int n) {
		ulong f = 1; // calculate natural factorial exactly
		while (n > 0)
			f *= (ulong)n--;
		return f;
	}
	public static T I_Factorial(T t) {
		var re = T.Re(t);
		var r = (int)Math.Floor(re);
		// Is it a natural number?
		if (Math.Abs(r - re) > 1e-8 || r < 0 || !(t - T.MakeR(re)).Is0())
			return Factorial_Stirling(t); // iterate complex factorial for n iterations
		var f = 1.0; // calculate natural factorial exactly (as far as double's mantissa allows)
		while (r > 0)
			f *= r--;
		return T.MakeR(f);
	}
	/// <summary>
	/// Optimized Hasse/Sondow approximation of zeta function (without the last division by "c + 1")
	/// </summary>
	/// <param name="t">T input (often written as "s")</param>
	/// <returns>zeta evaluated</returns>
	private static T Zeta_Hasse(T t) {
		// zeta(c) = (c-1)^(-1) * sum[n=0..i]: (n+1)^(-1) * sum[k=0..n]: (-1)^k * Combination(n,k) * (k + 1)^(1 - c)
		T nk, ns = 1.5 - (2 ^ -t), c1 = 1 - t; // 0th n term is 1, 1st term is (1-2^(1-c))/2 = 0.5-2^(-c), precount that, and loop n = n->2
		const int maxIterations = 42;
		T one, term = one = T.MakeR(1);
		var p = new int[maxIterations + 2]; // pascal triangle rows, the first term is always zero, because it is unused
		for (int tri, n = p[1] = 1; (tri = n) < maxIterations && +term / Math.Max(1, +ns) > 1e-20; ns += term = nk / (n + 1)) {
			while (tri > 1)
				p[tri] += p[--tri]; // add the non-edge pascal triangle terms
			var (e, o) = n % 2 == 0
				? (n, ++n) : (++n, n); // even and odd k iterators (so I don't have to (-1)^k)
			++p[p[n] = 1]; // increment 2nd term in the pascal row, last term (one) in the pascal row is a new one
			nk = one; // zeroth k term is 1, precount that and loop k = n->1
			do nk += p[e] * (e + 1 ^ c1); // even k terms, p[e] = Combinations(n,e)
			while (1 <= (e -= 2)); // decrement even k iterators down to 1
			do nk -= p[o] * (o + 1 ^ c1); // odd k terms, p[o] = Combinations(n,o)
			while (1 <= (o -= 2)); // decrement odd k iterators down to 1
		}
		return ns; // multiplied by Inc(c - 1) outside this function, as c - 1 is precomputed there
	}
	// Precomputed even Bernoulli numbers B2, B4, / factorial
	private static readonly double[] B2K = [
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
    ];
	private static T Zeta_Euler(T t) { // using B2k Bernoulli numbers/factorials
		int terms = 32, berns = B2K.Length;
		var s = T.MakeR(1);
		for (byte n = 2; n < terms; ++n)
			s += n ^ -t;
		var sum = s + (s = terms ^ -t) * (.5 + terms / (t - 1)) + B2K[0] * (s *= t / terms);
		terms *= terms;
		for (var k = 1; k < berns; ++k)
			sum += B2K[k] * (s *= (t + 2 * k + 1) * (t + 2 * k + 2) / terms);
		return sum;
	}
	// Elegant Zeta reflection: ζ(1-s)τ^s = CosI(s)Γ(s)ζ(s)
	// ζ(s) = (ζ(1-s)τ^s) / (CosI(s)Γ(s))
	// Zeta_Euler_Reflected(c) = Zeta_Euler(c) * R
	// R = (τ ^ c) * Inv(CosI(c) * Gamma_Stirling_Negative(c)) // substitute Inv and Gamma_Stirling_Negative
	// = (τ ^ c) / (CosI(c) * InvITau(Gamma_Stirling_Positive(1 - c) * SinN1(c))) // substitute InvITau
	// = (τ ^ c) / (iτCosI(c) / (Gamma_Stirling_Positive(1 - c) * SinN1(c))) // simplify division
	// = (τ ^ c) * Gamma_Stirling_Positive(1 - c) * SinN1(c) / (CosI(c) * i * tau) // move iτ
	// = -i(τ ^ (c - 1)) * Gamma_Stirling_Positive(1 - c) * SinN1(c) / CosI(c) // substitute NISinI, and c - 1 with the difference from pole that was precomputed for the condition
	// = (τ ^ c1) * Gamma_Stirling_Positive(1 - c) * NISinI(c)
	private const double G1 = -0.0728158454836767248605863758749013191377363383; // gamma1
	private const double G2 = -0.0096903631928723184845303860352125293590658061 / 2; // gamma2 / 2!
	private const double G3 = 0.0020538344203033458661600465427533842857158044 / 6; // gamma3 / 3!
	private const double G4 = 0.0023253700654673000574681701775260680009044694 / 24; // gamma4 / 4!
	private const double G5 = 0.0007933238173010627017533348774444448307315394 / 120; // gamma5 / 5!
																					  // TODO determine if the 5 terms in laurent series are appropriate for the e-4 distance, and that the Hasse-Euler boundary is appropriate for 2 distance, and that Hasse has appropriate number of terms
	public static T I_Zeta(T t) {
		// relative to the pole (useful in many places in this algo, including the reflection)
		var t1 = t - 1;
		// squared distance from the pole
		var pole = +t1;
		// exactly the pole - return infinity
		return pole == 0 ? T.MakeR(double.PositiveInfinity)
			// very near the pole - use Laurent that is excellent when this near, hopefully 5 terms are enough for the Laurent series with the distance from to pole up to e-4
			: pole < 1e-8 ? T.Inv(t1) + C_Gamma() - t1 * (G1 - t1 * (G2 - t1 * (G3 - t1 * (G4 - t1 * G5))))
			// near the pole - use general Hasse that is decent everywhere
			: pole < 4 ? Zeta_Hasse(t) * T.Inv(t1)
			// far from pole - use Euler that is excellent when far from it
			: T.Re(t) >= .5 ? Zeta_Euler(t)
			// negative and far from the pole - reflect Euler (using the formula derived above)
			: Zeta_Euler(t) * (C_Tau() ^ t1) * Gamma_Stirling_Positive(1 - t) * T.Sin_2Q(t);
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
			h = h >= 360 ? 0 : h / 60;
			var i = (int)Math.Truncate(h);
			double f = h - i, p = v * (1.0 - s), q = v * (1.0 - s * f), t = v * (1.0 - s * (1.0 - f));
			(double r, double g, double b) = i switch {
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
	static protected string _i(string i, string c) => i == "1" ? c : i + c; // redundant part of I.ToString
																	 //private static string _s(double v, int d) => d < 0 ? v.ToString() : v.ToString("F" + d.ToString()); 
	static protected string _sr(double value, int d) {
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
}
