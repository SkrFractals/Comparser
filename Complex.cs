using System.Globalization;
using System.Runtime.Intrinsics;

namespace Expressions;
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

	#region Constants
	public static Complex Zero => new(0);
	public static Complex One => new(1, 1);
	public static Complex i => new(0, 1);
	public static Complex ni => new(0, -1);
	public static Complex e => new(Math.E);
	public static Complex pi => new(Math.PI);
	public static Complex tau => new(Math.Tau);
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
	public static double Sqr(double r) => r * r;
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
	public static Complex operator ^(double r, Complex c) => Exp(Math.Log(r) * c);
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

	public static Complex Sinc(Complex c) => Sin(c) / c;
	public static Complex Nsinc(Complex c) => Sinc(pi * c);
	public static Complex Sinhc(Complex c) => Sinh(c) / c;
	public static Complex Nsinhc(Complex c) => Sinhc(pi * c);
	public static Complex Cosc(Complex c) => (1 - Cos(c)) / c;

}
