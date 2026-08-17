using System.Runtime.CompilerServices;

namespace Comparser.Comparser.Numbers;
public readonly struct Complex(double r = 0, double i = 0) : INumber<Complex> {
	private readonly double _r = r, _i = i;

	#region Complex Constants
	public static Complex i => new(0, 1);
	public static Complex ni => new(0, -1);
	#endregion

	#region Query
	public bool Is0() => _r == 0 && _i == 0;
	public bool IsNaN() => double.IsNaN(_r) || double.IsNaN(_i);

	public override string ToString() => ToString(-1);
	public string ToString(int d) {
		string si = INumber<Complex>._sr(_i, d), r = INumber<Complex>._sr(_r, d);
		return r == "0" ? si == "0" ? "0" : INumber<Complex>._i(si, "i") : si == "0" ? r : r + " + " + INumber<Complex>._i(si, "i");
	}
	#endregion

	#region Constants
	public static Complex Zero() => new(0);
	public static Complex One() => new(1, 1);
	public static Complex NaN() => new(double.NaN, double.NaN);
	#endregion

	#region Basics
	public static bool Compare(Complex a, Complex b) => Math.Abs(a._r - b._r) < 1e-8 && Math.Abs(a._i - b._i) < 1e-8;
	// conjugate: a - bi
	public static double Re(Complex c) => c._r;
	public static double Im(Complex c) => c._i;
	public static Complex MakeR(double r) => new(r);
	// conjugate: a - bi
	public static Complex operator !(Complex c) => new(c._r, -c._i);
	// negative: - a - bi
	public static Complex operator -(Complex c) => new(-c._r, -c._i);
	public static Complex operator ~(Complex c) => new(-c._i, c._r);
	public static Complex U(Complex c) => new(0, c._i);
	// i * complex
	public static Complex MulU(Complex c) => new(-c._i, c._r);
	// -i * complex
	public static Complex NegU(Complex c) => new(c._i, -c._r);
	// -i * complex
	public static Complex NegI(Complex c) => new(c._i, -c._r);
	// |complex|^2
	public static double operator +(Complex c) => Sqr(c._r) + Sqr(c._i);
	// signed fractional part
	public static Complex Frac(Complex c) => new(c._r - Math.Truncate(c._r), c._i - Math.Truncate(c._i));
	// truncate
	public static Complex Trunc(Complex c) => new(Math.Truncate(c._r), Math.Truncate(c._i));
	// round down
	public static Complex Floor(Complex c) => new(Math.Floor(c._r), Math.Floor(c._i));
	// round
	public static Complex Round(Complex c) => new(Math.Round(c._r), Math.Round(c._i));
	// round up
	public static Complex Ceil(Complex c) => new(Math.Ceiling(c._r), Math.Ceiling(c._i));
	// 1 / complex
	public static Complex Inv(Complex c) => !c / +c;
	// Argument of complex
	public static double Arg(Complex c) => Math.Atan2(c._i, c._r);
	// from angle
	public static Complex InvArg(double p, Complex _) => Complex_InvArg(p);
	public static Complex Axis(Complex q) => i;
	public static Complex Complex_InvArg(double p) => new(Math.Cos(p), Math.Sin(p));
	// square root
	public static Complex Sqrt(Complex c) { 
		var a = INumber<Complex>.Abs(c); 
		return new(Math.Sqrt(.5 * (a + c._r)), Math.CopySign(Math.Sqrt(.5 * (a - c._r)), c._i));
	}
	// complex^2
	public static Complex Sqr(Complex c) => new(Sqr(c._r) - Sqr(c._i), 2 * c._r * c._i);
	// real^2
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static double Sqr(double r) => r * r;
	// complex^3
	public static Complex Cub(Complex c) { double cr = c._r, ci = c._i, rr = cr*cr, ii = ci*cr; return new(cr * (rr - 3 * ii), ci * (3 * rr - ii)); }
	// complex^4
	public static Complex Quart(Complex c) { double cr = c._r, ci = c._i, a = cr * cr + ci * ci, ri = cr * ci; return new(a * a - 6 * ri * ri, 4 * a * ri); }
	// |a| + |b|i
	public static Complex AbsComp(Complex c) => new(Math.Abs(c._r), Math.Abs(c._i));
	public static Complex Min(Complex a, Complex b) => new(double.IsNaN(a._r) ? a._r : Math.Min(a._r, b._r), double.IsNaN(a._i) ? a._i : Math.Min(a._i, b._i));
	public static Complex Max(Complex a, Complex b) => new(double.IsNaN(a._r) ? a._r : Math.Min(a._r, b._r), double.IsNaN(a._i) ? a._i : Math.Min(a._i, b._i));
	public static Complex Clamp(Complex c, Complex min, Complex max) => new(double.IsNaN(c._r) ? c._r : Math.Clamp(c._r, min._r, max._r), double.IsNaN(min._i) ? min._i : Math.Clamp(c._i, min._i, max._i));
	#endregion

	#region Additions
	public static Complex operator +(Complex a, Complex b) => new(a._r + b._r, a._i + b._i);
	// complex + real
	public static Complex operator +(Complex c, double r) => new(c._r + r, c._i);
	// real + complex
	public static Complex operator +(double r, Complex c) => new(c._r + r, c._i);
	// complex + imaginary
	public static Complex AddV(Complex c, double v) => new(c._r, c._i + v);
	// imaginary + complex
	public static Complex AddV(double v, Complex c) => new(c._r, c._i + v);
	#endregion

	#region Subtractions
	public static Complex operator -(Complex a, Complex b) => new(a._r - b._r, a._i - b._i);
	// complex - real
	public static Complex operator -(Complex c, double r) => new(c._r - r, c._i);
	// real - complex
	public static Complex operator -(double r, Complex c) => new(r - c._r, -c._i);
	// complex - imaginary
	public static Complex SubV(Complex c, double v) => new(c._r, c._i - v);
	// imaginary - complex
	public static Complex SubV(double v, Complex c) => new(-c._r, v - c._i);
	#endregion

	#region Multiplications
	public static Complex operator *(Complex a, Complex b) => new(a._r * b._r - a._i * b._i, a._r * b._i + a._i * b._r);
	// complex * real
	public static Complex operator *(Complex c, double r) => new(r * c._r, r * c._i);
	// real * complex
	public static Complex operator *(double r, Complex c) => new(r * c._r, r * c._i);
	// complex * imaginary
	public static Complex MulI(Complex c, double v) => new(-v * c._i, v * c._r);
	// imaginary * complex
	public static Complex MulI(double v, Complex c) => new(-v * c._i, v * c._r);
	#endregion

	#region Divisions
	public static Complex operator /(Complex a, Complex b) => a * Inv(b);
	// complex / real
	public static Complex operator /(Complex c, double r) => new(c._r / r, c._i / r);
	// real / complex
	public static Complex operator /(double r, Complex c) => r * Inv(c);
	// complex / imaginary
	public static Complex DivI(Complex c, double v) => new(c._i / v, c._r / -v);
	// imaginary / complex
	public static Complex DivI(double v, Complex c) => MulI(v, Inv(c));
	public static Complex operator %(Complex a, Complex b) => new(b._r == 0 ? double.NaN : a._r % b._r, b._i == 0 ? double.NaN : a._i % b._i);
	public static Complex LDiv(Complex a, Complex b) => a / b;
	#endregion

	#region ExpLogs
	// Ln(complex)
	public static Complex Log(Complex c) => new(Math.Log(+c) * .5, Arg(c));
	// Ln(complex)/2
	public static Complex LogH(Complex c) => new(Math.Log(+c) * .25, Arg(c) * .5);
	// e ^ complex
	public static Complex Exp(Complex c) => Math.Exp(c._r) * Complex_InvArg(c._i);
	public static Complex operator ^(Complex a, Complex b) => Exp(Log(a) * b);
	// complex ^ real
	public static Complex operator ^(Complex c, double r) => Exp(Log(c) * r);
	// real ^ complex
	public static Complex operator ^(double r, Complex c) => 0 <= r ? Exp(Math.Log(r) * c) : Exp(new Complex(Math.Log(-r), Math.PI) * c);
	// (-1) ^ complex
	public static Complex PowN1(Complex c) => Exp(new(-c._i * Math.PI, c._r * Math.PI));
	// i ^ complex
	public static Complex PowI(Complex c) => Exp(new(-c._i * INumber<Complex>.QTau, c._r * INumber<Complex>.QTau));
	#endregion

	#region Hyperbolics
	// direct double math doesn't need complex Exp
	//public static Complex Cosh(Complex c) => (Exp(c) + Exp(-c)) / 2.0; 
	public static Complex Cosh(Complex c) => new(Math.Cos(c._i) * Math.Cosh(c._r), Math.Sin(c._i) * Math.Sinh(c._r));
	// direct double math doesn't need complex Exp
	//public static Complex Sinh(Complex c) => (Exp(c) - Exp(-c)) / 2.0; 
	public static Complex Sinh(Complex c) => new(Math.Cos(c._i) * Math.Sinh(c._r), Math.Sin(c._i) * Math.Cosh(c._r));
	// direct double math doesn't need complex Exp
	//public static Complex Tanh(Complex c) { var e2z = Exp(2 * c); return (e2z - 1) / (e2z + 1); }
	public static Complex Tanh(Complex c) {
		double t = Math.Tan(c._r), h = Math.Tanh(c._i), tt = t*t, hh = h*h;
		//return new Complex(h, t) / new Complex(1, t * h); // WIKI
		return new Complex(h * (1 + tt), t * (1 - hh)) / (1 + tt * hh);
	}
	public static Complex Coth(Complex c) {
		double t = Math.Tan(c._r), h = Math.Tanh(c._i), tt = t*t, hh = h*h;
		//return new Complex(1, t * h) / new Complex(h, t); // WIKI
		return new Complex(h * (tt + 1), t * (hh - 1)) / (hh + tt);
	}
	#endregion

	#region Trigonometrics
	// direct double math doesn't need ~
	//public static Complex Cos(Complex c) => Cosh(~c); 
	public static Complex Cos(Complex c) => new(Math.Cos(c._r) * Math.Cosh(c._i), Math.Sin(-c._r) * Math.Sinh(c._i));
	// direct double math doesn't need NI and ~
	//public static Complex Sin(Complex c) => NI(Sinh(~c)); 
	public static Complex Sin(Complex c) => new(Math.Sin(c._r) * Math.Cosh(c._i), Math.Cos(c._r) * Math.Sinh(c._i));
	// direct double math doesn't need NI and ~
	//public static Complex Tan(Complex c) => NI(Tanh(~c));
	public static Complex Tan(Complex c) {
		double t = Math.Tan(c._r), h = Math.Tanh(c._i), tt = Sqr(t), hh = Sqr(h);
		//return new Complex(t, h) / new Complex(1, -t * h); // WIKI
		return new Complex(t * (1 - hh), h * (1 + tt)) / (1 + tt * hh); // simplified into double math
	}
	public static Complex Cot(Complex c) {
		double t = Math.Tan(c._r), h = Math.Tanh(c._i), tt = Sqr(t), hh = Sqr(h);
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
		double ci = c._i * Math.PI, cr = c._r * Math.PI, cos = Math.Cos(cr), sin = Math.Sin(cr), e = Math.Exp(ci), ie = Math.Exp(-ci);
		return new Complex((ie + e) * sin, (e - ie) * cos);
	}
	// -i * ((i)^c - (i)^(-c)) = 2sin(πc/2)
	public static Complex Sin_2Q(Complex c) {
		double ci = c._i * INumber<Complex>.QTau, cr = c._r * INumber<Complex>.QTau, cos = Math.Cos(cr), sin = Math.Sin(cr), e = Math.Exp(ci), ie = Math.Exp(-ci);
		return new Complex((e + ie) * sin, (e - ie) * cos);
	}
	#endregion

	#region Special Functions
	public static Complex Gauss(Complex c) => INumber<Complex>.I_Gauss(c);
	public static Complex Gamma(Complex c) => INumber<Complex>.I_Gamma(c);
	public static Complex Factorial(Complex c) => INumber<Complex>.I_Factorial(c);
	public static Complex Zeta(Complex c) => INumber<Complex>.I_Zeta(c);
	#endregion
}
/* this one was originally used for zeta reflection, but it combined itself with SinN1 into NISinI
// i^c + i^(-c) = 2cos(πc/2) // is this faster than 2*T.Cos(qTau * c)? T.Cos(c) = new(Math.Cos(c.R) * Math.Cosh(c.I), Math.Sin(-c.R) * Math.Sinh(c.I));
private static T CosI(T c) {
	double i = c.I * qTau, r = c.R * qTau, cos = Math.Cos(r), sin = Math.Sin(r), e = Math.Exp(i), ie = 1 / e;
	return new T((ie - e) * cos, (ie + e) * sin);
}*/