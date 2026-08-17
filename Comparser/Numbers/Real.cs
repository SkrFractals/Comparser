namespace Comparser.Comparser.Numbers;
public readonly struct Real(double r = 0) : INumber<Real> {
	private readonly double _r = r;

	#region Query
	public bool Is0() => _r == 0;
	public bool IsNaN() => double.IsNaN(_r);
	//public static bool operator ==(Real a, Real b) => a.R == b.R && a.I == b.I;
	//public static bool operator !=(Real a, Real b) => a.R != b.R || a.I != b.I;

	public override string ToString() => ToString(-1);
	public string ToString(int d) => INumber<Real>._sr(_r, d);
	#endregion

	#region Constants
	public static Real Zero() => new(0);
	public static Real One() => new(1);
	public static Real NaN() => new(double.NaN);
	#endregion

	#region Basics
	public static bool Compare(Real a, Real b) => Math.Abs(a._r - b._r) < 1e-8;
	// conjugate: a - bi
	public static double Re(Real r) => r._r;
	public static double Im(Real r) => 0;
	public static Real MakeR(double r) => new(r);
	
	// conjugate: a - bi
	public static Real operator !(Real r) => r;
	// negative: - a - bi
	public static Real operator -(Real r) => new(-r._r);
	// i * real
	public static Real operator ~(Real r) => r; // ?
	public static Real U(Real r) => Zero();
	public static Real MulU(Real r) => r; // ?
	public static Real NegU(Real r) => -r; // ??
	// |real|^2
	public static double operator +(Real r) => r._r * r._r;
	// signed fractional part
	public static Real Frac(Real r) => new(r._r - Math.Truncate(r._r));
	// truncate
	public static Real Trunc(Real r) => new(Math.Truncate(r._r));
	// round down
	public static Real Floor(Real r) => new(Math.Floor(r._r));
	// round
	public static Real Round(Real r) => new(Math.Round(r._r));
	// round up
	public static Real Ceil(Real r) => new(Math.Ceiling(r._r));
	// 1 / real
	public static Real Inv(Real r) => new(1 / r._r);
	// Argument of real
	public static double Arg(Real r) => r._r < 0 ? Math.PI : 0;
	// from angle
	public static Real InvArg(double p, Real _) { var a = p % Math.Tau; return new(a == 0 ? 1 : Math.Abs(a - Math.PI) < 1e-8 ? -1 : double.NaN); }
	public static Real Axis(Real q) => NaN();
	// square root
	public static Real Sqrt(Real r) => new(Math.Sqrt(r._r));
	// real^2
	public static Real Sqr(Real r) => new(r._r * r._r);
	// real^3
	public static Real Cub(Real r) => new(r._r * r._r * r._r);
	// real^4
	public static Real Quart(Real r) { var s = r._r * r._r; return new Real(s * s); }
	// |a| + |b|i
	public static Real AbsComp(Real r) => new(Math.Abs(r._r));
	public static Real Min(Real a, Real b) => new(INumber<Real>.Min(a._r, b._r));
	public static Real Max(Real a, Real b) => new(INumber<Real>.Max(a._r, b._r));
	public static Real Clamp(Real r, Real min, Real max) => new(INumber<Real>.Clamp(r._r, min._r, max._r));
	#endregion

	#region Additions
	public static Real operator +(Real a, Real b) => new(a._r + b._r);
	// real + real
	public static Real operator +(Real r, double x) => new(r._r + x);
	// real + real
	public static Real operator +(double x, Real r) => new(r._r + x);
	public static Real AddV(Real r, double x) => NaN();
	public static Real AddV(double x, Real r) => NaN();
	#endregion

	#region Subtractions
	public static Real operator -(Real a, Real b) => new(a._r - b._r);
	// real - real
	public static Real operator -(Real r, double x) => new(r._r - x);
	// real - real
	public static Real operator -(double x, Real r) => new(x - r._r);
	public static Real SubV(Real r, double x) => NaN();
	public static Real SubV(double x, Real r) => NaN();
	#endregion

	#region Multiplications
	public static Real operator *(Real a, Real b) => new(a._r * b._r);
	// real * real
	public static Real operator *(Real r, double x) => new(x * r._r);
	// real * real
	public static Real operator *(double x, Real r) => new(x * r._r);
	#endregion

	#region Divisions
	public static Real operator /(Real a, Real b) => new(a._r / b._r);
	// real / real
	public static Real operator /(Real r, double x) => new(r._r / x);
	// real / real
	public static Real operator /(double x, Real r) => new(x / r._r);
	public static Real operator %(Real a, Real b) => new(INumber<Real>.Mod(a._r, b._r));
	public static Real LDiv(Real a, Real b) => a / b;
	#endregion

	#region ExpLogs
	// Ln(real)
	public static Real Log(Real r) => new(Math.Log(r._r));
	// Ln(real)/2
	public static Real LogH(Real r) => new(Math.Log(r._r) * .5);
	// e ^ real
	public static Real Exp(Real r) => new(Math.Exp(r._r));
	public static Real operator ^(Real a, Real b) => new(Math.Exp(Math.Log(a._r) * b._r));
	// real ^ real
	public static Real operator ^(Real r, double x) => new(Math.Exp(Math.Log(r._r) * x));
	// real ^ real
	public static Real operator ^(double x, Real r) => new(0 <= x ? Math.Exp(Math.Log(x) * r._r) : Math.Exp(Math.Abs(x)) * (Math.Abs((x = -r._r % 2) - 1) < 1e-8 ? -1 : x == 0 ? 1 : double.NaN));
	// (-1) ^ real
	public static Real PowN1(Real r) { var x = -r._r % 2; return new(Math.Abs(x % 2 - 1) < 1e-8 ? -1 : x == 0 ? 1 : double.NaN); }
	// i ^ real
	public static Real PowI(Real r) { var x = -r._r % 4; return new(Math.Abs(x % 2 - 2) < 1e-8 ? -1 : x == 0 ? 1 : double.NaN); }
	#endregion

	#region Hyperbolics
	public static Real Cosh(Real r) => new(Math.Cosh(r._r));
	public static Real Sinh(Real r) => new(Math.Sinh(r._r));
	public static Real Tanh(Real r) => new(Math.Tanh(r._r));
	public static Real Coth(Real r) => new(1 / Math.Tanh(r._r));
	#endregion

	#region Trigonometrics
	public static Real Cos(Real r) => new(Math.Cos(r._r));
	public static Real Sin(Real r) => new(Math.Sin(r._r));
	public static Real Tan(Real r) => new(Math.Tan(r._r));
	public static Real Cot(Real r) => new(1 / Math.Tan(r._r));
	#endregion

	#region ArcHyperbolics
	public static Real Acosh(Real r) => new(Math.Acosh(r._r));
	public static Real Asinh(Real r) => new(Math.Asinh(r._r));
	public static Real Atanh(Real r) => new(Math.Atanh(r._r));
	public static Real Acoth(Real r) => new(Math.Atanh(1 / r._r));
	#endregion

	#region ArcTrigonometrics
	public static Real Acos(Real r) => new(Math.Acos(r._r));
	public static Real Asin(Real r) => new(Math.Asin(r._r));
	public static Real Atan(Real r) => new(Math.Atan(r._r));
	public static Real Acot(Real r) => new(Math.Atan(1 / r._r));
	#endregion

	#region Exotic Trigonometrics
	// 2sin(πc)
	public static Real Sin_P(Real r) => new(2 * Math.Sin(Math.PI * r._r));
	// 2sin(πc/2)
	public static Real Sin_2Q(Real r) => new(2 * Math.Sin(INumber<Real>.QTau * r._r));
	#endregion

	#region Special Functions
	public static Real Gauss(Real r) => INumber<Real>.I_Gauss(r);
	public static Real Gamma(Real r) => INumber<Real>.I_Gamma(r); // = INumber<Real>.ComplexOp(r, INumber<Complex>.IGamma);
	public static Real Factorial(Real r) => INumber<Real>.I_Factorial(r); // = INumber<Real>.ComplexOp(r, INumber<Complex>.IFactorial);
	public static Real Zeta(Real r) => INumber<Real>.I_Zeta(r);// = INumber<Real>.ComplexOp(r, INumber<Complex>.IZeta); 
	#endregion
}