using System.Globalization;
using System.Runtime.CompilerServices;

namespace Comparser;
public readonly struct Complex(double r = 0, double i = 0) {

	public readonly double R = r, I = i;

	public bool Is0 => R == 0 && I == 0;
	public bool IsNaN => double.IsNaN(R) || double.IsNaN(I);
	//public static bool operator ==(Complex a, Complex b) => a.R == b.R && a.I == b.I;
	//public static bool operator !=(Complex a, Complex b) => a.R != b.R || a.I != b.I;

	public override string ToString() => ToString(-1);
	public string ToString(int d) {
		string i = _sr(I, d), r = _sr(R, d);
		return r == "0" ? i == "0" ? "0" : _i(i) : i == "0" ? r : r + " + " + _i(i);
	}
	private string _i(string i) => i == "1" ? "i" : i + "i"; // redundant part of I.ToString
	//private static string _s(double v, int d) => d < 0 ? v.ToString() : v.ToString("F" + d.ToString()); 
	private static string _sr(double value, int d) { // decimals ToString
		if(d < 0)
			return value.ToString();
		if (d == 0)
			return Math.Round(value).ToString();
		string s = value.ToString($"F{d}", CultureInfo.InvariantCulture);
		s = s.TrimEnd('0');
		if (s.EndsWith(".")) s = s.TrimEnd('.');
		return s == "-0" ? "0" : s;
	}
	public Color ToColorLog(double repeatValue = 1) { double s = +this; return Hsv((Arg(this) + Math.PI) * 360 / Math.Tau, 1 - Math.Exp(-s), (Math.Log(s) * .5) % repeatValue); }
	public Color ToColorLin(double repeatValue = 1) { double s = +this; return Hsv((Arg(this) + Math.PI) * 360 / Math.Tau, 1 - Math.Exp(-s), Math.Sqrt(+this) % repeatValue); }
	public Color ToColorExp() => Hsv((Arg(this) + Math.PI) * 360 / Math.Tau, 1, 1 - Math.Exp(-+this));

	//private double dFrac(double r) => r - Math.Truncate(r);

	#region Constants
	public static Complex Zero => new(0);
	public static Complex One => new(1, 1);
	public static Complex i => new(0, 1);
	public static Complex ni => new(0, -1);
	public static Complex e => new(Math.E);
	public static Complex pi => new(Math.PI);
	public static Complex tau => new(Math.Tau);
	public static Complex gamma => new(0.57721566490153286060651209008240243104215933593992); // Euler's constant
	public static Complex NaN => new(double.NaN, double.NaN);
	#endregion
	
	#region Basics
	// conjugate: a - bi
	public static Complex operator !(Complex c) => new(c.R, -c.I);
	// negative: - a - bi
	public static Complex operator -(Complex c) => new(-c.R, -c.I);
	// i * complex
	public static Complex operator ~(Complex c) => new(-c.I, c.R);
	// -i * complex
	public static Complex NI(Complex c) => new(c.I, -c.R);
	// |complex|^2
	public static double operator +(Complex c) => Sqr(c.R) + Sqr(c.I);
	// signed fractional part
	public static Complex Frac(Complex c) => new(c.R - Math.Truncate(c.R), c.I - Math.Truncate(c.I));
	// truncate
	public static Complex Trunc(Complex c) => new(Math.Truncate(c.R), Math.Truncate(c.I));
	// round down
	public static Complex Floor(Complex c) => new(Math.Floor(c.R), Math.Floor(c.I));
	// round
	public static Complex Round(Complex c) => new(Math.Round(c.R), Math.Round(c.I));
	// round up
	public static Complex Ceil(Complex c) => new(Math.Ceiling(c.R), Math.Ceiling(c.I));
	// 1 / complex
	public static Complex Inv(Complex c) => !c / +c;

	// Argument of complex
	public static double Arg(Complex c) => Math.Atan2(c.I, c.R);
	// from angle
	public static Complex IArg(double p) => new(Math.Cos(p), Math.Sin(p));
	// square root
	//public static Complex Sqrt(Complex c) => Math.Sqrt(Abs(c)) * IArg(.5 * Arg(c));
	public static Complex Sqrt(Complex c) { 
		var a = Abs(c); 
		return new(Math.Sqrt(.5 * (a + c.R)), Math.CopySign(Math.Sqrt(.5 * (a - c.R)), c.I));
	}
	// complex^2
	public static Complex Sqr(Complex c) => new(Sqr(c.R) - Sqr(c.I), 2 * c.R * c.I);
	// real^2
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static double Sqr(double r) => r * r;
	// complex^3
	public static Complex Cub(Complex c) { double r = c.R, i = c.I, rr = r*r, ii = i*r; return new(r * (rr - 3 * ii), i * (3 * rr - ii)); }
	// complex^4
	public static Complex Quart(Complex c) { double r = c.R, i = c.I, RI = r * r + i * i, ri = r * i; return new(RI * RI - 6 * ri * ri, 4 * RI * ri); }
	// |complex|
	public static double Abs(Complex c) => Math.Sqrt(+c);
	// complex / |complex|
	public static Complex Sign(Complex c) => c / Abs(c);
	// |a| + |b|i
	public static Complex AbsRI(Complex c) => new(Math.Abs(c.R), Math.Abs(c.I));
	#endregion

	#region Additions
	// complex + complex
	public static Complex operator +(Complex a, Complex b) => new(a.R + b.R, a.I + b.I);
	// complex + real
	public static Complex operator +(Complex c, double r) => new(c.R + r, c.I);
	// real + complex
	public static Complex operator +(double r, Complex c) => new(c.R + r, c.I);
	// complex + imaginary
	public static Complex AddI(Complex c, double i) => new(c.R, c.I + i);
	// imaginary + complex
	public static Complex AddI(double i, Complex c) => new(c.R, c.I + i);
	#endregion

	#region Subtractions
	// complex - complex
	public static Complex operator -(Complex a, Complex b) => new(a.R - b.R, a.I - b.I);
	// complex - real
	public static Complex operator -(Complex c, double r) => new(c.R - r, c.I);
	// real - complex
	public static Complex operator -(double r, Complex c) => new(r - c.R, -c.I);
	// complex - imaginary
	public static Complex SubI(Complex c, double i) => new(c.R, c.I - i);
	// imaginary - complex
	public static Complex SubI(double i, Complex c) => new(-c.R, i - c.I);
	#endregion

	#region Multiplications
	// complex * complex
	public static Complex operator *(Complex a, Complex b) => new(a.R * b.R - a.I * b.I, a.R * b.I + a.I * b.R);
	// complex * real
	public static Complex operator *(Complex c, double r) => new(r * c.R, r * c.I);
	// real * complex
	public static Complex operator *(double r, Complex c) => new(r * c.R, r * c.I);
	// complex * imaginary
	public static Complex MulI(Complex c, double i) => new(-i * c.I, i * c.R);
	// imaginary * complex
	public static Complex MulI(double i, Complex c) => new(-i * c.I, i * c.R);
	#endregion

	#region Divisions
	// complex / complex
	public static Complex operator /(Complex a, Complex b) => a * Inv(b);
	// complex / real
	public static Complex operator /(Complex c, double r) => new(c.R / r, c.I / r);
	// real / complex
	public static Complex operator /(double r, Complex c) => r * Inv(c);
	// complex / imaginary
	public static Complex DivI(Complex c, double i) => new(c.I / i, c.R / -i);
	// imaginary / complex
	public static Complex DivI(double i, Complex c) => MulI(i, Inv(c));
	#endregion

	#region ExpLogs
	// Ln(complex)
	public static Complex Log(Complex c) => new(Math.Log(+c) * .5, Arg(c));
	// Ln(complex)/2
	public static Complex LogH(Complex c) => new(Math.Log(+c) * .25, Arg(c) * .5);
	// e ^ complex
	public static Complex Exp(Complex c) => Math.Exp(c.R) * IArg(c.I);
	// complex ^ complex
	public static Complex operator ^(Complex a, Complex b) => Exp(Log(a) * b);
	// complex ^ real
	public static Complex operator ^(Complex c, double r) => Exp(Log(c) * r);
	// real ^ complex
	public static Complex operator ^(double r, Complex c) => 0 <= r ? Exp(Math.Log(r) * c) : Exp(new Complex(Math.Log(-r), Math.PI) * c);
	// (-1) ^ complex
	public static Complex PowN1(Complex c) => Exp(new(-c.I * Math.PI, c.R * Math.PI));
	// i ^ complex
	public static Complex PowI(Complex c) => Exp(new(-c.I * qTau, c.R * qTau));
	#endregion

	#region Hyperbolics
	// direct double math doesn't need complex Exp
	//public static Complex Cosh(Complex c) => (Exp(c) + Exp(-c)) / 2.0; 
	public static Complex Cosh(Complex c) => new(Math.Cos(c.I) * Math.Cosh(c.R), Math.Sin(c.I) * Math.Sinh(c.R));
	public static Complex Sech(Complex c) => Inv(Cosh(c));
	// direct double math doesn't need complex Exp
	//public static Complex Sinh(Complex c) => (Exp(c) - Exp(-c)) / 2.0; 
	public static Complex Sinh(Complex c) => new(Math.Cos(c.I) * Math.Sinh(c.R), Math.Sin(c.I) * Math.Cosh(c.R));
	public static Complex Csch(Complex c) => Inv(Sinh(c));
	// direct double math doesn't need complex Exp
	//public static Complex Tanh(Complex c) { var e2z = Exp(2 * c); return (e2z - 1) / (e2z + 1); }
	public static Complex Tanh(Complex c) {
		double t = Math.Tan(c.R), h = Math.Tanh(c.I), tt = Sqr(t), hh = Sqr(h);
		//return new Complex(h, t) / new Complex(1, t * h); // WIKI
		return new Complex(h * (1 + tt), t * (1 - hh)) / (1 + tt * hh);
	}
	public static Complex Coth(Complex c) {
		double t = Math.Tan(c.R), h = Math.Tanh(c.I), tt = Sqr(t), hh = Sqr(h);
		//return new Complex(1, t * h) / new Complex(h, t); // WIKI
		return new Complex(h * (tt + 1), t * (hh - 1)) / (hh + tt);
	}
	#endregion

	#region Trigonometrics
	// direct double math doesn't need ~
	//public static Complex Cos(Complex c) => Cosh(~c); 
	public static Complex Cos(Complex c) => new(Math.Cos(c.R) * Math.Cosh(c.I), Math.Sin(-c.R) * Math.Sinh(c.I));
	public static Complex Sec(Complex c) => Inv(Cos(c));
	// direct double math doesn't need NI and ~
	//public static Complex Sin(Complex c) => NI(Sinh(~c)); 
	public static Complex Sin(Complex c) => new(Math.Sin(c.R) * Math.Cosh(c.I), Math.Cos(c.R) * Math.Sinh(c.I));
	public static Complex Csc(Complex c) => Inv(Sin(c));
	// direct double math doesn't need NI and ~
	//public static Complex Tan(Complex c) => NI(Tanh(~c));
	public static Complex Tan(Complex c) {
		double t = Math.Tan(c.R), h = Math.Tanh(c.I), tt = Sqr(t), hh = Sqr(h);
		//return new Complex(t, h) / new Complex(1, -t * h); // WIKI
		return new Complex(t * (1 - hh), h * (1 + tt)) / (1 + tt * hh); // simplified into double math
	}
	public static Complex Cot(Complex c) {
		double t = Math.Tan(c.R), h = Math.Tanh(c.I), tt = Sqr(t), hh = Sqr(h);
		//return new Complex(1, -t * h) / new Complex(t, h); // WIKI
		return new Complex(t * (1 - hh), -h * (1 + tt)) / (tt + hh); // simplified into double math
	}
	#endregion

	#region ArcHyperbolics
	public static Complex Acosh(Complex c) => Log(c + Sqrt(Sqr(c) - 1));
	public static Complex Asech(Complex c) => Acosh(Inv(c));
	public static Complex Asinh(Complex c) => Log(c + Sqrt(Sqr(c) + 1));
	public static Complex Acsch(Complex c) => Asinh(Inv(c));
	public static Complex Atanh(Complex c) => LogH((1 + c) / (1 - c));
	public static Complex Acoth(Complex c) => LogH((c + 1) / (c - 1));
	#endregion

	#region ArcTrigonometrics
	public static Complex Acos(Complex c) => Acosh(~c);
	public static Complex Asec(Complex c) => Acos(Inv(c));
	public static Complex Asin(Complex c) => -~Asinh(~c);
	public static Complex Acsc(Complex c) => Asin(Inv(c));
	public static Complex Atan(Complex c) => NI(LogH(SubI(1, c) / AddI(1, c)));
	public static Complex Acot(Complex c) => NI(LogH(AddI(c, 1) / SubI(c, 1)));
	#endregion

	#region Exotic Trigonometrics
	public static Complex Sinc(Complex c) => Sin(c) / c;
	public static Complex Nsinc(Complex c) => Sinc(pi * c);
	public static Complex Sinhc(Complex c) => Sinh(c) / c;
	public static Complex Nsinhc(Complex c) => Sinhc(pi * c);
	public static Complex Cosc(Complex c) => (1 - Cos(c)) / c;
	#endregion
	
	#region Special
	/* not very precise, use Stirling instead:
	public static Complex Gamma_Weierstrass(Complex c, int n) {
		var s = c - Log(1 + c);
		do {
			var r = c / n;
			s += r - Log(1 + r);
		} while (1 < --n);
		return Exp(s - Log(c) - gamma * c);
	}*/
	private const double qTau = Math.Tau / 4;
	// (-1)^c - (-1)^(-c) = 2isin(πc) // is this faster than 2*i*Complex.Sin(Math.PI * c)? Complex.Sin(c) = new(Math.Sin(c.R) * Math.Cosh(c.I), Math.Cos(c.R) * Math.Sinh(c.I));
	private static Complex SinN1(Complex c) {
		double i = c.I * Math.PI, r = c.R * Math.PI, cos = Math.Cos(r), sin = Math.Sin(r), e = Math.Exp(i), ie = Math.Exp(-i);
		return new Complex((ie - e) * cos, (ie + e) * sin);
	}
	/* this one was originally used for zeta reflection, but it combined intself with SinN1 into NISinI
	// i^c + i^(-c) = 2cos(πc/2) // is this faster than 2*Complex.Cos(qTau * c)? Complex.Cos(c) = new(Math.Cos(c.R) * Math.Cosh(c.I), Math.Sin(-c.R) * Math.Sinh(c.I));
	private static Complex CosI(Complex c) {
		double i = c.I * qTau, r = c.R * qTau, cos = Math.Cos(r), sin = Math.Sin(r), e = Math.Exp(i), ie = 1 / e;
		return new Complex((ie - e) * cos, (ie + e) * sin);
	}*/
	// -i * ((i)^c - (i)^(-c)) = 2sin(πc/2) // is this faster than 2*Complex.Sin(qTau * c)? Complex.Sin(c) = new(Math.Sin(c.R) * Math.Cosh(c.I), Math.Cos(c.R) * Math.Sinh(c.I));
	private static Complex NISinI(Complex c) {
		double i = c.I * qTau, r = c.R * qTau, cos = Math.Cos(r), sin = Math.Sin(r), e = Math.Exp(i), ie = Math.Exp(-i);
		return new Complex((e + ie) * sin, (e - ie) * cos);
	}
	private static readonly double lnTH = Math.Log(Math.Tau) / 2;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Complex Log_Gamma_Stirling_Positive(Complex c, double off = -.5) {
		Complex cc = c * c, c4 = cc * cc, c8 = c4 * c4;
		// var correction = Inv(c *= 12) - Inv(30 * (c * cc)) + Inv(105 * (c * c4)) - Inv(140 * (c8/c)) + Inv(99 * (c8*c))
		// TODO determine if these are correction enough terms
		var correction = (140 / 99 - cc + (c4 - 3.5 * (c4 * cc - 30 * c8)) * 4 / 3) / (1680 * c8 * c); // this takes quite fewer operations   
		return (c + off) * Log(c) - c + lnTH + correction;
	}
	private static Complex Log_Factorial_Stirling_Positive(Complex c) => Log_Gamma_Stirling_Positive(c, .5);
	private static Complex Gamma_Stirling_Positive(Complex c) => Exp(Log_Gamma_Stirling_Positive(c));
	private static Complex Factorial_Stirling_Positive(Complex c) => Exp(Log_Factorial_Stirling_Positive(c));

	// Sin version: Γ(1-z)Γ(z)=π/sin(πz) => Γ(1.5+z)Γ(.5-z) = π/sin(π(.5+z)) => Γ(.5-z) = π/(Γ(1.5+z)sin(π(.5+z))); c = .5-z => z = .5-c
	//public static Complex Gamma_Stirling_Sin(Complex c) => c.R > .5 ? Gamma_Stirling_Positive(c) : Math.PI * Inv(Gamma_Stirling_Positive(2 - c) * Sin(new(Math.PI * c));

	// SinN1 version: Γ(1-z)Γ(z) = iτ/((-1)^z-(-1)^(-z)) = iτ/SinN1(z) =>
	// z -> z+.5: Γ(.5-z)Γ(.5+z) = iτ/SinN1(z+.5) => Γ(c) = iτ/(Γ(.5+z)SinN1(z+.5))
	// z = .5-c => iτ/(Γ(1-c))SinN1(c)) 
	private static Complex InvITau(Complex c) => new Complex(c.I, c.R) * (Math.Tau / +c); // = iτ/c
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Complex Gamma_Stirling_Negative(Complex c) => InvITau(Gamma_Stirling_Positive(1 - c) * SinN1(c));

	public static Complex Gamma_Stirling(Complex c) => c.R > .5 ? Gamma_Stirling_Positive(c) : Gamma_Stirling_Negative(c); // SinN1(1-c) = SinN1(c)
	// z! reflection: z!(-z)!nsinc(z) = 1 => c! = Inv((-c)!nsinc(-c)) = Inv((-c)!sinc(c*pi)) 
	public static Complex Factorial_Stirling(Complex c) => 0 <= c.R ? Factorial_Stirling_Positive(c) : Inv(Factorial_Stirling_Positive(-c) * Sinc(Math.PI * c));
	public static Complex Factorial(Complex c) {
		var r = (int)Math.Floor(c.R);
		if (c.R == r && c.R >= 0 && c.I == 0) {
			var f = 1.0; // calculate natural factorial exactly (as far as double's mantissa allows)
			while (r > 0)
				f *= r--;
			return new(f);
		}
		return Factorial_Stirling(c); // iterate complex factorial for n iterations
	}
	/// <summary>
	/// Optimized Hasse/Sondow approximation of zeta function (without the last division by "c + 1")
	/// </summary>
	/// <param name="c">Complex input (often written as "s")</param>
	/// <param name="i">Iteration ceiling. n=0..max(1,i). Terms 0 and 1 are precomputed, you'll get those even if you input 0 or negative.</param>
	/// <returns></returns>
	private static Complex Zeta_Hasse(Complex c) {
		// zeta(c) = (c-1)^(-1) * sum[n=0..i]: (n+1)^(-1) * sum[k=0..n]: (-1)^k * Combination(n,k) * (k + 1)^(1 - c)
		Complex nk, ns = 1.5 - (2^(-c)), c1 = 1 - c; // 0th n term is 1, 1st term is (1-2^(1-c))/2 = 0.5-2^(-c), precount that, and loop n = n->2
		const int maxIterations = 42;
		Complex term = new(1);
		var p = new int[maxIterations + 2]; // pascal triangle rows, the first term is always zero, because it is unused
		for(int t, n = p[1] = 1; (t = n) < maxIterations && +term / Math.Max(1, +ns) > 1e-20; ns += term = nk / (n + 1)) {
			while (t > 1)
				p[t] += p[--t]; // add the non-edge pascal triangle terms
			(int e, int o) = n % 2 == 0 ? (n, ++n) : (++n, n); // even and odd k iterators (so i don't have to (-1)^k)
			++p[p[n] = 1]; // increment 2nd term in the pascal row, last term (one) in the pascal row is a new one
			nk = new(1); // zeroth k term is 1, precount that and loop k = n->1
			do nk += p[e] * ((e + 1) ^ c1); // even k terms, p[e] = Combinations(n,e)
			while (1 <= (e -= 2)); // decrement even k iterators down to 1
			do nk -= p[o] * ((o + 1) ^ c1); // odd k terms, p[o] = Combinations(n,o)
			while (1 <= (o -= 2)); // decrement odd k iterators down to 1
		};
		return ns; // multiplied by Inc(c - 1) outside this function, as c - 1 is precomputed there
	}

	// Precomputed even Bernoulli numbers B2, B4, / factorial
	static readonly double[] B2k = {
		1/(6		*Factorial(new(2)).R),  // B2
        -1/(30		*Factorial(new(4)).R),  // B4
        1/(42		*Factorial(new(6)).R),  // B6
        -1/(30		*Factorial(new(8)).R),  // B8
        5/(66		*Factorial(new(10)).R), // ...
		-691/(2730	*Factorial(new(12)).R),
		7/(6		*Factorial(new(14)).R),
		-3617/(510	*Factorial(new(16)).R),
		43867/(498	*Factorial(new(18)).R),
		-174611/(330*Factorial(new(20)).R),
        // ... add more
    };
	private static Complex Zeta_Euler(Complex s) {
		int N = 32, M = B2k.Length;
		Complex S = new(1);
		for (byte n = 2; n < N; ++n)
			S += n ^ -s;
		var sum = S + ((S = N ^ -s) * (.5 + N / (s - 1))) + B2k[0] * (S *= s / N);
		N *= N;
		for (int k = 1; k < M; ++k)
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
	public static Complex Zeta(Complex c) {
		var c1 = c - 1; // relative to the pole (useful in many places in this alog, including the reflection)
		var pole = +c1; // squared distance from the pole
		// exactly the pole - return infinity
		return pole == 0 ? new(double.PositiveInfinity)
			// very near the pole - use Laurent that is excellent when this near, hopefully 5 terms are enough for the Laurent series with the distance from to pole up to e-4
			: pole < 1e-8 ? Inv(c1) + gamma - c1 * (g1 - c1 * (g2 - c1 * (g3 - c1 * (g4 - c1 * g5))))
			// near the pole - use general Hasse that is decent everywhere
			: pole < 4 ? Zeta_Hasse(c) * Inv(c1)
			// far from pole - use Euler that is excellent when far from it
			: c.R >= .5 ? Zeta_Euler(c)
			// negative and far from the pole - reflect Euler (using the formula derived above)
			: Zeta_Euler(c) * (tau ^ c1) * Gamma_Stirling_Positive(1 - c) * NISinI(c);
	}
	#endregion

	/// <summary>
	/// turn hsv into rgb color
	/// </summary>
	/// <param name="h">0-360 hue</param>
	/// <param name="s">0-1 saturation</param>
	/// <param name="v">0-1 value</param>
	/// <returns></returns>
	Color Hsv(double h, double s, double v) {
		if (s > 0) {
			double r, g, b, f, p, q, t;
			h = h == 360 ? 0 : h / 60;
			int i = (int)Math.Truncate(h);
			f = h - i;
			p = v * (1.0 - s);
			q = v * (1.0 - (s * f));
			t = v * (1.0 - (s * (1.0 - f)));
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
			byte l = (byte)(255 * v);
			return Color.FromArgb(l, l, l);
		}
	}
}
