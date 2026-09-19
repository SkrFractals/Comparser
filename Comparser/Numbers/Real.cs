using static Comparser.Comparser.Numbers.Static;
namespace Comparser.Comparser.Numbers;
public readonly struct Real/*<double>*/(double r = default) : ILeaf/*<double>*/, INumber<Real/*<double>,double*/> /*where double : unmanaged, IScalar<double>*/ {
	public readonly double R = r;

	#region ILeaf
	public NumericKind kind => NumericKind.Real;
	public ILeaf/*<double>*/ Cast(NumericKind to) => to switch {
		NumericKind.Real => this,
		NumericKind.Complex => (Complex)R,
		NumericKind.Quaternion => (Quaternion)R,
		_ => nan
	};
	#endregion
	
	#region Query
	public double Re() => R;
	public bool IsFalse() => INumber<Real>.IsFalse(this); 
	public bool IsTrue() => INumber<Real>.IsTrue(this); 
	public static bool Is0(Real /*<double>*/ r) => r.R == 0;//double.Is0(r.R);
	public bool Is0() => this is { R: 0 };
	public bool IsNaN() => double.IsNaN(R);
	public static bool IsNaN(Real/*<double>*/ r) => double.IsNaN(r.R);
	public static bool operator ==(Real /*<double>*/ a, Real /*<double>*/ b) => Math.Abs(a.R - b.R) <= Math.Max(Math.Abs(a.R), Math.Abs(b.R)) * 1e-6;//(a.R == b.R);
	public static bool operator !=(Real/*<double>*/ a, Real/*<double>*/ b) => Math.Abs(a.R - b.R) > Math.Max(Math.Abs(a.R), Math.Abs(b.R)) * 1e-6;//(a.R != b.R);
	//public static string[] epsUnit => ["ε"];
	//public static double[] EpsValues(Real/*<double>*/ r, Real/*<double>*/ e) => [r.R, e.R];
	public override string ToString() => ToString(-1);
	public string ToString(int d) => _sr(R, d);
	#endregion
	
	#region Cast
	public static implicit operator double(Real/*<T>*/ d) => d.R;
	//public static implicit operator double(Complex/*<T>*/ d) => IScalar<double>.ToDouble(d.R);
	public static explicit operator Real/*<T>*/(double b) => new(b);
	public static explicit operator Real/*<T>*/(Complex b) => new(b.R);
	public static explicit operator Real/*<T>*/(Quaternion b) => new(b.R);
	#endregion

	#region Constants
	public static Real/*<double>*/ zero => default;
	public static Real/*<double>*/ nan => new(IScalar<double>.nan);
	public static Real/*<double>*/ unit => new(IScalar<double>.unit);
	public static Real/*<double>*/ one => new(IScalar<double>.nan);
	public static Real/*<double>*/ u => default;
	public static Real/*<double>*/ minusUnit => new(-IScalar<double>.unit);
	public static Real/*<double>*/ minusOne => new(-IScalar<double>.one);
	public static Real/*<double>*/ minusU => default;
	
	#endregion
	
	#region Helpers
	public static double Mix(Real/*<double>*/ c, Func<double, double, double> d) => c.R;
	public static Real/*<double>*/ D1(Real/*<double>*/ a, Func<double, double> d) => new(d(a.R));
	public static Real/*<double>*/ D2(Real/*<double>*/ a, Real/*<double>*/ b, Func<double, double, double> d) => new(d(a.R, b.R));
	public static Real/*<double>*/ D3(Real/*<double>*/ a, Real/*<double>*/ b, Real/*<double>*/ c, Func<double, double, double, double> d) => new(d(a.R, b.R, c.R));
	#endregion
	
	#region Basics
	public static bool AreEqual(Real/*<double>*/ a, Real/*<double>*/ b) => Math.Abs(a.R - b.R) < 1e-8;
	public static double Re(Real/*<double>*/ r) => r.R;
	public static double Im(Real/*<double>*/ r) => 0;
	public double Im() => 0;
	public static double ImMag(Real/*<double>*/ r) => 0;
	public static Real/*<double>*/ MakeR(double r) => new(r);
	// conjugate: a - bi
	public static Real/*<double>*/ operator ~(Real/*<double>*/ r) => r;
	// negative: - a - bi
	public static Real/*<double>*/ operator -(Real/*<double>*/ r) => new(-r.R);
	// i * real
	public static Real/*<double>*/ operator !(Real/*<double>*/ r) => nan; // ?
	public static Real/*<double>*/ U(Real/*<double>*/ r) => zero;
	public static Real/*<double>*/ MulU(Real/*<double>*/ r) => r; // ?
	public static Real/*<double>*/ NegU(Real/*<double>*/ r) => -r; // ??
	// |real|^2
	public static double operator +(Real/*<double>*/ r) => r.R * r.R;
	// signed fractional part
	public static Real/*<double>*/ Frac(Real/*<double>*/ r) => new(r.R - double.Truncate(r.R));
	// truncate
	public static Real/*<double>*/ Truncate(Real/*<double>*/ r) => new(double.Truncate(r.R));
	// round down
	public static Real/*<double>*/ Floor(Real/*<double>*/ r) => new(double.Floor(r.R));
	// round
	public static Real/*<double>*/ Round(Real/*<double>*/ r) => new(double.Round(r.R));
	// round up
	public static Real/*<double>*/ Ceiling(Real/*<double>*/ r) => new(double.Ceiling(r.R));
	public static Real/*<double>*/ Cycle(Real/*<double>*/ r) => new(IScalar<double>.Cycle(r.R));
	// 1 / real
	public static Real/*<double>*/ Inv(Real/*<double>*/ r) => new(1 / r.R);
	// Argument of real
	public static double Arg(Real/*<double>*/ r) => r.R < 0 ? Math.PI : 0;
	// from angle
	public static Real/*<double>*/ InvArg(double p, Real/*<double>*/ _) { var a = p % Math.Tau; return new(a == 0 ? 1 : Math.Abs(a - Math.PI) < 1e-8 ? -1 : double.NaN); }
	public static Real/*<double>*/ Axis(Real/*<double>*/ q) => nan;
	// square root
	public static Real/*<double>*/ Sqrt(Real/*<double>*/ r) => new(Math.Sqrt(r.R));
	// real^2
	public static Real/*<double>*/ Sqr(Real/*<double>*/ r) => new(r.R * r.R);
	// real^3
	public static Real/*<double>*/ Cub(Real/*<double>*/ r) => new(r.R * r.R * r.R);
	// real^4
	public static Real/*<double>*/ Quart(Real/*<double>*/ r) { var s = r.R * r.R; return new Real/*<double>*/(s * s); }
	// |a| + |b|i
	public static Real/*<double>*/ AbsComp(Real/*<double>*/ r) => new(Math.Abs(r.R));
	public static double Dot(Real/*<double>*/ a, Real/*<double>*/ b) => a.R * b.R;
	public static Real/*<double>*/ Min(Real/*<double>*/ a, Real/*<double>*/ b) => new(Static.Min(a.R, b.R));
	public static Real/*<double>*/ Max(Real/*<double>*/ a, Real/*<double>*/ b) => new(Static.Max(a.R, b.R));
	public static Real/*<double>*/ Clamp(Real/*<double>*/ r, Real/*<double>*/ min, Real/*<double>*/ max) => new(Static.Clamp(r.R, min.R, max.R));
	#endregion

	#region Additions
	public static Real/*<double>*/ operator ++(Real/*<double>*/ c) => c + 1;
	public static Real/*<double>*/ operator +(Real/*<double>*/ a, Real/*<double>*/ b) => new(a.R + b.R);
	// real + real
	public static Real/*<double>*/ operator +(Real/*<double>*/ r, double x) => new(r.R + x);
	// real + real
	public static Real/*<double>*/ operator +(double x, Real/*<double>*/ r) => new(r.R + x);
	public static Real/*<double>*/ AddV(Real/*<double>*/ r, double x) => nan;
	public static Real/*<double>*/ AddV(double x, Real/*<double>*/ r) => nan;
	#endregion

	#region Subtractions
	public static Real/*<double>*/ operator --(Real/*<double>*/ c) => c - 1;
	public static Real/*<double>*/ operator -(Real/*<double>*/ a, Real/*<double>*/ b) => new(a.R - b.R);
	// real - real
	public static Real/*<double>*/ operator -(Real/*<double>*/ r, double x) => new(r.R - x);
	// real - real
	public static Real/*<double>*/ operator -(double x, Real/*<double>*/ r) => new(x - r.R);
	public static Real/*<double>*/ SubV(Real/*<double>*/ r, double x) => nan;
	public static Real/*<double>*/ SubV(double x, Real/*<double>*/ r) => nan;
	#endregion

	#region Multiplications
	public static Real/*<double>*/ operator *(Real/*<double>*/ a, Real/*<double>*/ b) => new(a.R * b.R);
	// real * real
	public static Real/*<double>*/ operator *(Real/*<double>*/ r, double x) => new(x * r.R);
	// real * real
	public static Real/*<double>*/ operator *(double x, Real/*<double>*/ r) => new(x * r.R);
	public static double operator |(Real/*<double>*/ a, Real/*<double>*/ b) => a.R + b.R;
	#endregion

	#region Divisions
	public static Real/*<double>*/ operator /(Real/*<double>*/ a, Real/*<double>*/ b) => new(a.R / b.R);
	// real / real
	public static Real/*<double>*/ operator /(Real/*<double>*/ r, double x) => new(r.R / x);
	// real / real
	public static Real/*<double>*/ operator /(double x, Real/*<double>*/ r) => new(x / r.R);
	public static Real/*<double>*/ operator %(Real/*<double>*/ a, Real/*<double>*/ b) => INumber<Real/*<double>*/>.NewMod(a, b);
	public static Real/*<double>*/ LDiv(Real/*<double>*/ a, Real/*<double>*/ b) => a / b;
	#endregion

	#region ExpLogs
	// Ln(real)
	public static Real/*<double>*/ Log(Real/*<double>*/ r) => new(Math.Log(r.R));
	// Ln(real)/2
	public static Real/*<double>*/ LogH(Real/*<double>*/ r) => new(Math.Log(r.R) * .5);
	// e ^ real
	public static Real/*<double>*/ Exp(Real/*<double>*/ r) => new(Math.Exp(r.R));
	public static Real/*<double>*/ operator ^(Real/*<double>*/ a, Real/*<double>*/ b) => new(Math.Exp(Math.Log(a.R) * b.R));
	// real ^ real
	public static Real/*<double>*/ operator ^(Real/*<double>*/ r, double x) => new(Math.Exp(Math.Log(r.R) * x));
	// real ^ real
	public static Real/*<double>*/ operator ^(double x, Real/*<double>*/ r) => new(0 <= x ? Math.Exp(Math.Log(x) * r.R) : Math.Exp(Math.Abs(x)) * (Math.Abs((x = -r.R % 2) - 1) < 1e-8 ? -1 : x == 0 ? 1 : double.NaN));
	// (-1) ^ real
	public static Real/*<double>*/ PowN1(Real/*<double>*/ r) { var x = -r.R % 2; return new(Math.Abs(x % 2 - 1) < 1e-8 ? -1 : x == 0 ? 1 : double.NaN); }
	// i ^ real
	public static Real/*<double>*/ PowI(Real/*<double>*/ r) { var x = -r.R % 4; return new(Math.Abs(x % 2 - 2) < 1e-8 ? -1 : x == 0 ? 1 : double.NaN); }
	#endregion

	#region Hyperbolics
	public static Real/*<double>*/ Cosh(Real/*<double>*/ r) => new(Math.Cosh(r.R));
	public static Real/*<double>*/ Sinh(Real/*<double>*/ r) => new(Math.Sinh(r.R));
	public static Real/*<double>*/ Tanh(Real/*<double>*/ r) => new(Math.Tanh(r.R));
	public static Real/*<double>*/ Coth(Real/*<double>*/ r) => new(1 / Math.Tanh(r.R));
	#endregion

	#region doublerigonometrics
	public static Real/*<double>*/ Cos(Real/*<double>*/ r) => new(Math.Cos(r.R));
	public static Real/*<double>*/ Sin(Real/*<double>*/ r) => new(Math.Sin(r.R));
	public static Real/*<double>*/ Tan(Real/*<double>*/ r) => new(Math.Tan(r.R));
	public static Real/*<double>*/ Cot(Real/*<double>*/ r) => new(1 / Math.Tan(r.R));
	#endregion

	#region ArcHyperbolics
	public static Real/*<double>*/ Acosh(Real/*<double>*/ r) => new(Math.Acosh(r.R));
	public static Real/*<double>*/ Asinh(Real/*<double>*/ r) => new(Math.Asinh(r.R));
	public static Real/*<double>*/ Atanh(Real/*<double>*/ r) => new(Math.Atanh(r.R));
	public static Real/*<double>*/ Acoth(Real/*<double>*/ r) => new(Math.Atanh(1 / r.R));
	#endregion

	#region Arcdoublerigonometrics
	public static Real/*<double>*/ Acos(Real/*<double>*/ r) => new(Math.Acos(r.R));
	public static Real/*<double>*/ Asin(Real/*<double>*/ r) => new(Math.Asin(r.R));
	public static Real/*<double>*/ Atan(Real/*<double>*/ r) => new(Math.Atan(r.R));
	public static Real/*<double>*/ Acot(Real/*<double>*/ r) => new(Math.Atan(1 / r.R));
	#endregion

	#region Exotic doublerigonometrics
	// 2sin(πc)
	public static Real/*<double>*/ Sin_P(Real/*<double>*/ r) => new(2 * Math.Sin(Math.PI * r.R));
	// 2sin(πc/2)
	public static Real/*<double>*/ Sin_2Q(Real/*<double>*/ r) => new(2 * Math.Sin(QTau * r.R));
	#endregion

	#region Special Functions
	public static Real/*<double>*/ Gauss(Real/*<double>*/ r) => INumber<Real/*<double>*/>.I_Gauss(r);
	public static Real/*<double>*/ Gamma(Real/*<double>*/ r) => INumber<Real/*<double>*/>.I_Gamma(r); // = INumber<Real/*<double>*/>.ComplexOp(r, INumber<Complex>.IGamma);
	public static Real/*<double>*/ Factorial(Real/*<double>*/ r) => INumber<Real/*<double>*/>.I_Factorial(r); // = INumber<Real/*<double>*/>.ComplexOp(r, INumber<Complex>.IFactorial);
	public static Real/*<double>*/ Zeta(Real/*<double>*/ r) => INumber<Real/*<double>*/>.I_Zeta(r);// = INumber<Real/*<double>*/>.ComplexOp(r, INumber<Complex>.IZeta); 
	#endregion
	public static void IndexAndAddToRgb(Color[] axis, Real/*<double>*/ indices, Real/*<double>*/ value) {
		var a = axis[(int)indices.R];
		if (indices.R >= 0 && indices.R < axis.Length)
			axis[(int)indices.R] = Color.FromArgb(a.R + (int)value.R, a.G + (int)value.R, a.B + (int)value.R);
	}
}