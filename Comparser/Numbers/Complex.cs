using System.Runtime.CompilerServices;
using static Comparser.Comparser.Numbers.Static;

namespace Comparser.Comparser.Numbers;
public readonly struct Complex(double r = 0, double i = 0) : INumber<Complex> {
	public readonly double R = r, I = i;

	#region Complex Constants
	public static Complex i => new(0, 1);
	public static Complex ni => new(0, -1);
	#endregion

	#region Query
	public bool Is0() => R == 0 && I == 0;
	public bool IsNaN() => double.IsNaN(R) || double.IsNaN(I);

	public override string ToString() => ToString(-1);
	public string ToString(int d) => Static.ValueToString("i", [R, I], d);
	#endregion

	#region Constants
	public static Complex Zero() => new(0);
	public static Complex One() => new(1, 1);
	public static Complex NaN() => new(double.NaN, double.NaN);
	#endregion

	#region Helpers
	//public static (double r, double i) ToPair(Complex c) => (c.R, c.I);
	public static double Mix(Complex c, Func<double, double, double> d) => d(c.R, c.I);
	public static Complex D1(Complex a, Func<double, double> d) => new(d(a.R), d(a.I));
	public static Complex D2(Complex a, Complex b, Func<double, double, double> d) => new(d(a.R, b.R), d(a.I, b.I));
	public static Complex D3(Complex a, Complex b, Complex c, Func<double, double, double, double> d) => new(d(a.R, b.R, c.R), d(a.I, b.I, c.I));
	#endregion
	
	#region Basics
	public static bool AreEqual(Complex a, Complex b) => Math.Abs(a.R - b.R) < 1e-8 && Math.Abs(a.I - b.I) < 1e-8;
	public static double Re(Complex c) => c.R;
	public static double Im(Complex c) => c.I;
	public static double ImMag(Complex c) => Math.Abs(c.I);
	public static Complex MakeR(double r) => new(r);
	public static Complex Swap(Complex c) => new(c.I, c.R);
	// conjugate: a - bi
	public static Complex operator !(Complex c) => new(c.R, -c.I);
	// negative: - a - bi
	public static Complex operator -(Complex c) => new(-c.R, -c.I);
	public static Complex operator ~(Complex c) => new(-c.I, c.R);
	public static Complex U(Complex c) => new(0, c.I);
	// i * complex
	public static Complex MulU(Complex c) => new(-c.I, c.R);
	// -i * complex
	public static Complex NegU(Complex c) => new(c.I, -c.R);
	// -i * complex
	public static Complex NegI(Complex c) => new(c.I, -c.R);
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
	public static Complex InvArg(double p, Complex _) => Complex_InvArg(p);
	public static Complex Axis(Complex q) => i;
	public static Complex Complex_InvArg(double p) => new(Math.Cos(p), Math.Sin(p));
	// square root
	public static Complex Sqrt(Complex c) { 
		var a = INumber<Complex>.Abs(c); 
		return new(Math.Sqrt(.5 * (a + c.R)), Math.CopySign(Math.Sqrt(.5 * (a - c.R)), c.I));
	}
	// complex^2
	public static Complex Sqr(Complex c) => new(Sqr(c.R) - Sqr(c.I), 2 * c.R * c.I);
	// real^2
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static double Sqr(double r) => r * r;
	// complex^3
	public static Complex Cub(Complex c) { double cr = c.R, ci = c.I, rr = cr*cr, ii = ci*cr; return new(cr * (rr - 3 * ii), ci * (3 * rr - ii)); }
	// complex^4
	public static Complex Quart(Complex c) { double cr = c.R, ci = c.I, a = cr * cr + ci * ci, ri = cr * ci; return new(a * a - 6 * ri * ri, 4 * a * ri); }
	// |a| + |b|i
	public static Complex AbsComp(Complex c) => new(Math.Abs(c.R), Math.Abs(c.I));
	public static double Dot(Complex a, Complex b) => a.R * b.R + a.I * b.I;
	public static Complex Min(Complex a, Complex b) =>  D2(a,b,Static.Min);
	public static Complex Max(Complex a, Complex b) => D2(a,b,Static.Max);
	public static Complex Clamp(Complex c, Complex min, Complex max) => D3(c, min, max, Static.Clamp);
	#endregion

	#region Additions
	public static Complex operator ++(Complex c) => c + 1;
	public static Complex operator +(Complex a, Complex b) => new(a.R + b.R, a.I + b.I);
	// complex + real
	public static Complex operator +(Complex c, double r) => new(c.R + r, c.I);
	// real + complex
	public static Complex operator +(double r, Complex c) => new(c.R + r, c.I);
	// complex + imaginary
	public static Complex AddV(Complex c, double v) => new(c.R, c.I + v);
	// imaginary + complex
	public static Complex AddV(double v, Complex c) => new(c.R, c.I + v);
	#endregion

	#region Subtractions
	public static Complex operator --(Complex c) => c - 1;
	public static Complex operator -(Complex a, Complex b) => new(a.R - b.R, a.I - b.I);
	// complex - real
	public static Complex operator -(Complex c, double r) => new(c.R - r, c.I);
	// real - complex
	public static Complex operator -(double r, Complex c) => new(r - c.R, -c.I);
	// complex - imaginary
	public static Complex SubV(Complex c, double v) => new(c.R, c.I - v);
	// imaginary - complex
	public static Complex SubV(double v, Complex c) => new(-c.R, v - c.I);
	#endregion

	#region Multiplications
	public static Complex operator *(Complex a, Complex b) => new(a.R * b.R - a.I * b.I, a.R * b.I + a.I * b.R);
	// complex * real
	public static Complex operator *(Complex c, double r) => new(r * c.R, r * c.I);
	// real * complex
	public static Complex operator *(double r, Complex c) => new(r * c.R, r * c.I);
	// complex * imaginary
	public static Complex MulI(Complex c, double v) => new(-v * c.I, v * c.R);
	// imaginary * complex
	public static Complex MulI(double v, Complex c) => new(-v * c.I, v * c.R);
	public static double operator |(Complex a, Complex b) => a.R * b.R + a.I * b.I;
	#endregion

	#region Divisions
	public static Complex operator /(Complex a, Complex b) => a * Inv(b);
	// complex / real
	public static Complex operator /(Complex c, double r) => new(c.R / r, c.I / r);
	// real / complex
	public static Complex operator /(double r, Complex c) => r * Inv(c);
	// complex / imaginary
	public static Complex DivI(Complex c, double v) => new(c.I / v, c.R / -v);
	// imaginary / complex
	public static Complex DivI(double v, Complex c) => MulI(v, Inv(c));
	public static Complex LDiv(Complex a, Complex b) => a / b;
	public static Complex operator %(Complex a, Complex b) => INumber<Complex>.NewMod(a,b);
	#endregion

	#region ExpLogs
	// Ln(complex)
	public static Complex Log(Complex c) => new(Math.Log(+c) * .5, Arg(c));
	// Ln(complex)/2
	public static Complex LogH(Complex c) => new(Math.Log(+c) * .25, Arg(c) * .5);
	// e ^ complex
	public static Complex Exp(Complex c) => Math.Exp(c.R) * Complex_InvArg(c.I);
	public static Complex operator ^(Complex a, Complex b) => Exp(Log(a) * b);
	// complex ^ real
	public static Complex operator ^(Complex c, double r) => Exp(Log(c) * r);
	// real ^ complex
	public static Complex operator ^(double r, Complex c) => 0 <= r ? Exp(Math.Log(r) * c) : Exp(new Complex(Math.Log(-r), Math.PI) * c);
	// (-1) ^ complex
	public static Complex PowN1(Complex c) => Exp(new(-c.I * Math.PI, c.R * Math.PI));
	// i ^ complex
	public static Complex PowI(Complex c) => Exp(new(-c.I * QTau, c.R * QTau));
	#endregion

	#region Hyperbolics
	// direct double math doesn't need complex Exp
	//public static Complex Cosh(Complex c) => (Exp(c) + Exp(-c)) / 2.0; 
	public static Complex Cosh(Complex c) => new(Math.Cos(c.I) * Math.Cosh(c.R), Math.Sin(c.I) * Math.Sinh(c.R));
	// direct double math doesn't need complex Exp
	//public static Complex Sinh(Complex c) => (Exp(c) - Exp(-c)) / 2.0; 
	public static Complex Sinh(Complex c) => new(Math.Cos(c.I) * Math.Sinh(c.R), Math.Sin(c.I) * Math.Cosh(c.R));
	// direct double math doesn't need complex Exp
	//public static Complex Tanh(Complex c) { var e2z = Exp(2 * c); return (e2z - 1) / (e2z + 1); }
	public static Complex Tanh(Complex c) {
		double t = Math.Tan(c.R), h = Math.Tanh(c.I), tt = t*t, hh = h*h;
		//return new Complex(h, t) / new Complex(1, t * h); // WIKI
		return new Complex(h * (1 + tt), t * (1 - hh)) / (1 + tt * hh);
	}
	public static Complex Coth(Complex c) {
		double t = Math.Tan(c.R), h = Math.Tanh(c.I), tt = t*t, hh = h*h;
		//return new Complex(1, t * h) / new Complex(h, t); // WIKI
		return new Complex(h * (tt + 1), t * (hh - 1)) / (hh + tt);
	}
	#endregion

	#region Trigonometrics
	// direct double math doesn't need ~
	//public static Complex Cos(Complex c) => Cosh(~c); 
	public static Complex Cos(Complex c) => new(Math.Cos(c.R) * Math.Cosh(c.I), Math.Sin(-c.R) * Math.Sinh(c.I));
	// direct double math doesn't need NI and ~
	//public static Complex Sin(Complex c) => NI(Sinh(~c)); 
	public static Complex Sin(Complex c) => new(Math.Sin(c.R) * Math.Cosh(c.I), Math.Cos(c.R) * Math.Sinh(c.I));
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
	public static Complex Acosh(Complex c) => INumber<Complex>.I_Acosh(c);
	public static Complex Asinh(Complex c) => INumber<Complex>.I_Asinh(c);
	public static Complex Atanh(Complex c) => INumber<Complex>.I_Atanh(c);
	public static Complex Acoth(Complex c) => INumber<Complex>.I_Acoth(c);
	#endregion

	#region ArcTrigonometrics
	public static Complex Acos(Complex c) => INumber<Complex>.I_Acos(c);
	public static Complex Asin(Complex c) => INumber<Complex>.I_Asin(c);
	public static Complex Atan(Complex c) => INumber<Complex>.I_Atan(c);
	public static Complex Acot(Complex c) => INumber<Complex>.I_Acot(c);
	#endregion

	#region Exotic Trigonometrics
	// -i*((-1)^c - (-1)^(-c)) = 2sin(πc)
	public static Complex Sin_P(Complex c) {
		double ci = c.I * Math.PI, cr = c.R * Math.PI, cos = Math.Cos(cr), sin = Math.Sin(cr), e = Math.Exp(ci), ie = Math.Exp(-ci);
		return new Complex((ie + e) * sin, (e - ie) * cos);
	}
	// -i * ((i)^c - (i)^(-c)) = 2sin(πc/2)
	public static Complex Sin_2Q(Complex c) {
		double ci = c.I * QTau, cr = c.R * QTau, cos = Math.Cos(cr), sin = Math.Sin(cr), e = Math.Exp(ci), ie = Math.Exp(-ci);
		return new Complex((e + ie) * sin, (e - ie) * cos);
	}
	#endregion

	#region Special Functions
	public static Complex Gauss(Complex c) => INumber<Complex>.I_Gauss(c);
	public static Complex Gamma(Complex c) => INumber<Complex>.I_Gamma(c);
	public static Complex Factorial(Complex c) => INumber<Complex>.I_Factorial(c);
	public static Complex Zeta(Complex c) => INumber<Complex>.I_Zeta(c);
	#endregion
	public static void IndexAndAddToRgb(Color[] axis, Complex indices, Complex value) {
		var a = axis[(int)indices.R];
		if (indices.R >= 0 && indices.R < axis.Length)
			axis[(int)indices.R] = Color.FromArgb(a.R + (int)value.R, a.G, a.B);
		if (indices.I >= 0 && indices.I < axis.Length)
			axis[(int)indices.I] = Color.FromArgb(a.R, a.G, a.B + (int)value.I);
	}
}
/* this one was originally used for zeta reflection, but it combined itself with SinN1 into NISinI
// i^c + i^(-c) = 2cos(πc/2) // is this faster than 2*T.Cos(qTau * c)? T.Cos(c) = new(Math.Cos(c.R) * Math.Cosh(c.I), Math.Sin(-c.R) * Math.Sinh(c.I));
private static T CosI(T c) {
	double i = c.I * qTau, r = c.R * qTau, cos = Math.Cos(r), sin = Math.Sin(r), e = Math.Exp(i), ie = 1 / e;
	return new T((ie - e) * cos, (ie + e) * sin);
}*/