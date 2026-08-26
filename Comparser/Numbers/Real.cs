using static Comparser.Comparser.Numbers.Static;
namespace Comparser.Comparser.Numbers;
public readonly struct Real(double r = 0) : INumber<Real> {
	public readonly double R = r;

	#region Query
	public bool Is0() => R == 0;
	public bool IsNaN() => double.IsNaN(R);
	//public static bool operator ==(Real a, Real b) => a.R == b.R && a.I == b.I;
	//public static bool operator !=(Real a, Real b) => a.R != b.R || a.I != b.I;

	public override string ToString() => ToString(-1);
	public string ToString(int d) => _sr(R, d);
	#endregion

	#region Constants
	public static Real Zero() => new(0);
	public static Real One() => new(1);
	public static Real NaN() => new(double.NaN);
	#endregion

	#region Helpers
	public static double Mix(Real c, Func<double, double, double> d) => c.R;
	public static Real D1(Real a, Func<double, double> d) => new(d(a.R));
	public static Real D2(Real a, Real b, Func<double, double, double> d) => new(d(a.R, b.R));
	public static Real D3(Real a, Real b, Real c, Func<double, double, double, double> d) => new(d(a.R, b.R, c.R));
	#endregion
	
	#region Basics
	public static bool AreEqual(Real a, Real b) => Math.Abs(a.R - b.R) < 1e-8;
	public static double Re(Real r) => r.R;
	public static double Im(Real r) => 0;
	public static double ImMag(Real r) => 0;
	public static Real MakeR(double r) => new(r);
	
	// conjugate: a - bi
	public static Real operator !(Real r) => r;
	// negative: - a - bi
	public static Real operator -(Real r) => new(-r.R);
	// i * real
	public static Real operator ~(Real r) => r; // ?
	public static Real U(Real r) => Zero();
	public static Real MulU(Real r) => r; // ?
	public static Real NegU(Real r) => -r; // ??
	// |real|^2
	public static double operator +(Real r) => r.R * r.R;
	// signed fractional part
	public static Real Frac(Real r) => new(r.R - Math.Truncate(r.R));
	// truncate
	public static Real Trunc(Real r) => new(Math.Truncate(r.R));
	// round down
	public static Real Floor(Real r) => new(Math.Floor(r.R));
	// round
	public static Real Round(Real r) => new(Math.Round(r.R));
	// round up
	public static Real Ceil(Real r) => new(Math.Ceiling(r.R));
	// 1 / real
	public static Real Inv(Real r) => new(1 / r.R);
	// Argument of real
	public static double Arg(Real r) => r.R < 0 ? Math.PI : 0;
	// from angle
	public static Real InvArg(double p, Real _) { var a = p % Math.Tau; return new(a == 0 ? 1 : Math.Abs(a - Math.PI) < 1e-8 ? -1 : double.NaN); }
	public static Real Axis(Real q) => NaN();
	// square root
	public static Real Sqrt(Real r) => new(Math.Sqrt(r.R));
	// real^2
	public static Real Sqr(Real r) => new(r.R * r.R);
	// real^3
	public static Real Cub(Real r) => new(r.R * r.R * r.R);
	// real^4
	public static Real Quart(Real r) { var s = r.R * r.R; return new Real(s * s); }
	// |a| + |b|i
	public static Real AbsComp(Real r) => new(Math.Abs(r.R));
	public static double Dot(Real a, Real b) => a.R * b.R;
	public static Real Min(Real a, Real b) => new(Static.Min(a.R, b.R));
	public static Real Max(Real a, Real b) => new(Static.Max(a.R, b.R));
	public static Real Clamp(Real r, Real min, Real max) => new(Static.Clamp(r.R, min.R, max.R));
	#endregion

	#region Additions
	public static Real operator ++(Real c) => c + 1;
	public static Real operator +(Real a, Real b) => new(a.R + b.R);
	// real + real
	public static Real operator +(Real r, double x) => new(r.R + x);
	// real + real
	public static Real operator +(double x, Real r) => new(r.R + x);
	public static Real AddV(Real r, double x) => NaN();
	public static Real AddV(double x, Real r) => NaN();
	#endregion

	#region Subtractions
	public static Real operator --(Real c) => c - 1;
	public static Real operator -(Real a, Real b) => new(a.R - b.R);
	// real - real
	public static Real operator -(Real r, double x) => new(r.R - x);
	// real - real
	public static Real operator -(double x, Real r) => new(x - r.R);
	public static Real SubV(Real r, double x) => NaN();
	public static Real SubV(double x, Real r) => NaN();
	#endregion

	#region Multiplications
	public static Real operator *(Real a, Real b) => new(a.R * b.R);
	// real * real
	public static Real operator *(Real r, double x) => new(x * r.R);
	// real * real
	public static Real operator *(double x, Real r) => new(x * r.R);
	public static double operator |(Real a, Real b) => a.R + b.R;
	#endregion

	#region Divisions
	public static Real operator /(Real a, Real b) => new(a.R / b.R);
	// real / real
	public static Real operator /(Real r, double x) => new(r.R / x);
	// real / real
	public static Real operator /(double x, Real r) => new(x / r.R);
	public static Real operator %(Real a, Real b) => INumber<Real>.NewMod(a, b);
	public static Real LDiv(Real a, Real b) => a / b;
	#endregion

	#region ExpLogs
	// Ln(real)
	public static Real Log(Real r) => new(Math.Log(r.R));
	// Ln(real)/2
	public static Real LogH(Real r) => new(Math.Log(r.R) * .5);
	// e ^ real
	public static Real Exp(Real r) => new(Math.Exp(r.R));
	public static Real operator ^(Real a, Real b) => new(Math.Exp(Math.Log(a.R) * b.R));
	// real ^ real
	public static Real operator ^(Real r, double x) => new(Math.Exp(Math.Log(r.R) * x));
	// real ^ real
	public static Real operator ^(double x, Real r) => new(0 <= x ? Math.Exp(Math.Log(x) * r.R) : Math.Exp(Math.Abs(x)) * (Math.Abs((x = -r.R % 2) - 1) < 1e-8 ? -1 : x == 0 ? 1 : double.NaN));
	// (-1) ^ real
	public static Real PowN1(Real r) { var x = -r.R % 2; return new(Math.Abs(x % 2 - 1) < 1e-8 ? -1 : x == 0 ? 1 : double.NaN); }
	// i ^ real
	public static Real PowI(Real r) { var x = -r.R % 4; return new(Math.Abs(x % 2 - 2) < 1e-8 ? -1 : x == 0 ? 1 : double.NaN); }
	#endregion

	#region Hyperbolics
	public static Real Cosh(Real r) => new(Math.Cosh(r.R));
	public static Real Sinh(Real r) => new(Math.Sinh(r.R));
	public static Real Tanh(Real r) => new(Math.Tanh(r.R));
	public static Real Coth(Real r) => new(1 / Math.Tanh(r.R));
	#endregion

	#region Trigonometrics
	public static Real Cos(Real r) => new(Math.Cos(r.R));
	public static Real Sin(Real r) => new(Math.Sin(r.R));
	public static Real Tan(Real r) => new(Math.Tan(r.R));
	public static Real Cot(Real r) => new(1 / Math.Tan(r.R));
	#endregion

	#region ArcHyperbolics
	public static Real Acosh(Real r) => new(Math.Acosh(r.R));
	public static Real Asinh(Real r) => new(Math.Asinh(r.R));
	public static Real Atanh(Real r) => new(Math.Atanh(r.R));
	public static Real Acoth(Real r) => new(Math.Atanh(1 / r.R));
	#endregion

	#region ArcTrigonometrics
	public static Real Acos(Real r) => new(Math.Acos(r.R));
	public static Real Asin(Real r) => new(Math.Asin(r.R));
	public static Real Atan(Real r) => new(Math.Atan(r.R));
	public static Real Acot(Real r) => new(Math.Atan(1 / r.R));
	#endregion

	#region Exotic Trigonometrics
	// 2sin(πc)
	public static Real Sin_P(Real r) => new(2 * Math.Sin(Math.PI * r.R));
	// 2sin(πc/2)
	public static Real Sin_2Q(Real r) => new(2 * Math.Sin(QTau * r.R));
	#endregion

	#region Special Functions
	public static Real Gauss(Real r) => INumber<Real>.I_Gauss(r);
	public static Real Gamma(Real r) => INumber<Real>.I_Gamma(r); // = INumber<Real>.ComplexOp(r, INumber<Complex>.IGamma);
	public static Real Factorial(Real r) => INumber<Real>.I_Factorial(r); // = INumber<Real>.ComplexOp(r, INumber<Complex>.IFactorial);
	public static Real Zeta(Real r) => INumber<Real>.I_Zeta(r);// = INumber<Real>.ComplexOp(r, INumber<Complex>.IZeta); 
	#endregion
	public static void IndexAndAddToRgb(Color[] axis, Real indices, Real value) {
		var a = axis[(int)indices.R];
		if (indices.R >= 0 && indices.R < axis.Length)
			axis[(int)indices.R] = Color.FromArgb(a.R + (int)value.R, a.G + (int)value.R, a.B + (int)value.R);
	}
}