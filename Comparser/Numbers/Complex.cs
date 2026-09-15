using System.Runtime.CompilerServices;
using static Comparser.Comparser.Numbers.ILeaf;
using static Comparser.Comparser.Numbers.Static;

namespace Comparser.Comparser.Numbers;
public readonly struct Complex/*<double>*/(double r = default, double i = default) 
	: ILeaf/*<double>*/, INumber<Complex/*<double>,double*/> /*where double : unmanaged, IScalar<double>*/ {
	public readonly double R = r, I = i;

	#region ILeaf
	public NumericKind kind => NumericKind.Complex;
	public ILeaf/*<T>*/ Cast(NumericKind to) => to switch {
		NumericKind.Real => (Real)/*<T>*/R,
		NumericKind.Complex => this,
		NumericKind.Quaternion => new Quaternion/*<T>*/(R, I),
		_ => nan
	};
	#endregion
	
	#region Complex/*<T>*/ Constants
	public static Complex/*<T>*/ i => new(IScalar<double>.zero, IScalar<double>.unit);
	//public static Complex/*<T>*/ ni => new(double.zero, -IScalar<double>.unit);
	#endregion

	#region Query
	public bool IsFalse() => INumber<Complex>.IsFalse(this);
	public bool IsTrue() => INumber<Complex>.IsTrue(this);
	public static bool Is0(Complex/*<T>*/ t) => t is { R: 0, I: 0 };
	public bool Is0() => this is { R: 0, I: 0 };
	public static bool IsNaN(Complex/*<T>*/ c) => double.IsNaN(c.R) || double.IsNaN(c.I);
	public bool IsNaN() => IsNaN(this);

	public override string ToString() => ToString(-1);
	//private static readonly string[] Units = ["i"];
	//public static string[] epsUnit => ["i", "ε", "εi"];
	public static double[] EpsValues(Complex/*<T>*/ r, Complex/*<T>*/ e) => [r.R, r.I, e.R, e.I];
	public string ToString(int d) => ValueToString(AllUnits, /*GetValues()*/[R,I], d);

	private static readonly string[] AllUnits = ["i"];//GetUnits();
	/*private static string[] GetUnits() {
		var units = double.units;
		var r = new string[2 * units.Length - 1];
		r[0] = "i";
		var c = 0;
		foreach (var u in units) {
			r[++c] = u;
			r[++c] = u + "i";
		}
		return r;
	}
	private List<double> GetValues() {
		List<double> r = [];
		double[] rv = R.GetValues(), iv = I.GetValues();
		for (var u = 0; u < rv.Length; r.Add(iv[u++]))
			r.Add(rv[u]);
		return r;
	}*/
	#endregion

	#region Constants
	public static Complex/*<T>*/ zero => default;
	public static Complex/*<T>*/ nan => new(IScalar<double>.nan, IScalar<double>.nan);
	public static Complex/*<T>*/ one => new(IScalar<double>.one, IScalar<double>.one);
	#endregion
	
	#region Cast
	public static implicit operator double(Complex/*<T>*/ d) => d.R;
	//public static implicit operator double(Complex/*<T>*/ d) => IScalar<double>.ToDouble(d.R);
	public static explicit operator Complex/*<T>*/(double b) => new(b);
	public static explicit operator Complex/*<T>*/(Real b) => new(b.R);
	public static explicit operator Complex/*<T>*/(Quaternion b) => new(b.R, b.I);
	#endregion

	#region Helpers
	//public static (double r, double i) ToPair(Complex/*<T>*/ c) => (c.R, c.I);
	public static double Mix(Complex/*<T>*/ c, Func<double, double, double> d) => d(c.R, c.I);
	public static Complex/*<T>*/ D1(Complex/*<T>*/ a, Func<double, double> d) => new(d(a.R), d(a.I));
	public static Complex/*<T>*/ D2(Complex/*<T>*/ a, Complex/*<T>*/ b, Func<double, double, double> d) => new(d(a.R, b.R), d(a.I, b.I));
	public static Complex/*<T>*/ D3(Complex/*<T>*/ a, Complex/*<T>*/ b, Complex/*<T>*/ c, Func<double, double, double, double> d) => new(d(a.R, b.R, c.R), d(a.I, b.I, c.I));
	#endregion
	
	#region Poly Operators
	public static bool operator ==(Complex /*<T>*/ a, Complex /*<T>*/ b) 
		=> Math.Abs(a.R - b.R) <= Math.Max(Math.Abs(a.R), Math.Abs(b.R)) * 1e-6 
			&& Math.Abs(a.I - b.I) <= Math.Max(Math.Abs(a.I), Math.Abs(b.I)) * 1e-6;//a.R == b.R && a.I == b.I;
	public static bool operator !=(Complex/*<T>*/ a, Complex/*<T>*/ b) => Math.Abs(a.R - b.R) > Math.Max(Math.Abs(a.R), Math.Abs(b.R)) * 1e-6 
		|| Math.Abs(a.I - b.I) > Math.Max(Math.Abs(a.I), Math.Abs(b.I)) * 1e-6;//a.R != b.R || a.I != b.I;


	public static ILeaf operator +(Complex a, ILeaf b) => b.kind switch {
		NumericKind.Real => a + ((Real)b).R, // only adds other's real part
		NumericKind.Complex => a + (Complex)b,
		NumericKind.Quaternion => AddCq(a, (Quaternion)b), // only adds my complex parts
		_ => nan
	};
	public static ILeaf operator +(ILeaf b, Complex a) => a + b;
	public static ILeaf operator -(Complex a, ILeaf b) => b.kind switch {
		NumericKind.Real => a - ((Real)b).R,
		NumericKind.Complex => a - (Complex)b,
		NumericKind.Quaternion => SubCq(a, (Quaternion)b),
		_ => nan
	};
	public static ILeaf operator -(ILeaf b, Complex a) => b.kind switch {
		NumericKind.Real => ((Real)b).R - a, 
		NumericKind.Complex => (Complex)b - a,
		NumericKind.Quaternion => SubQc((Quaternion)b, a),
		_ => nan
	};
	public static ILeaf operator *(Complex a, ILeaf b) => b.kind switch {
		NumericKind.Real => a * ((Real)b).R, 
		NumericKind.Complex => a * (Complex)b,
		NumericKind.Quaternion => MulCq(a, (Quaternion)b),
		_ => nan
	};
	public static ILeaf operator *(ILeaf b, Complex a) => a * b;
	public static ILeaf operator /(Complex a, ILeaf b) => b.kind switch {
		NumericKind.Real => a / ((Real)b).R, 
		NumericKind.Complex => a / (Complex)b,
		NumericKind.Quaternion => DivCq(a, (Quaternion)b), 
		_ => nan
	};
	public static ILeaf operator /(ILeaf b, Complex a) => b.kind switch {
		NumericKind.Real => ((Real)b).R / a,
		NumericKind.Complex => (Complex)b / a,
		NumericKind.Quaternion => DivQc((Quaternion)b, a), 
		_ => nan
	};
	public static ILeaf LDiv(Complex a, ILeaf b) => b.kind switch {
		NumericKind.Real => a / ((Real)b).R, 
		NumericKind.Complex => a / (Complex)b,
		NumericKind.Quaternion => LDivCq(a, (Quaternion)b), 
		_ => nan
	};
	public static ILeaf LDiv(ILeaf b, Complex a) => b.kind switch {
		NumericKind.Real => ((Real)b).R / a,
		NumericKind.Complex => (Complex)b / a,
		NumericKind.Quaternion => LDivQc((Quaternion)b, a), 
		_ => nan
	};
	public static ILeaf operator %(Complex a, ILeaf b) => b.kind switch {
		NumericKind.Real => INumber<Complex>.NewMod(a, ((Real)b).R), 
		NumericKind.Complex => a % (Complex)b,
		NumericKind.Quaternion => ModCq(a, (Quaternion)b), 
		_ => nan
	};
	public static ILeaf operator %(ILeaf b, Complex a) => b.kind switch {
		NumericKind.Real => INumber<Complex>.NewMod(((Real)b).R, a),
		NumericKind.Complex => (Complex)b % a,
		NumericKind.Quaternion => ModQc((Quaternion)b, a), 
		_ => nan
	};
	#endregion
	
	#region Basics
	public static double Re(Complex/*<T>*/ c) => c.R;
	public double Re() => IScalar<double>.ToDouble(R);
	public double Im() => Im(this);
	public static double Im(Complex/*<T>*/ c) => c.I;
	public static double ImMag(Complex/*<T>*/ c) => double.Abs(c.I);
	public static Complex/*<T>*/ MakeR(double r) => new(r);
	public static Complex/*<T>*/ Swap(Complex/*<T>*/ c) => new(c.I, c.R);
	// conjugate: a - bi
	public static Complex/*<T>*/ operator ~(Complex/*<T>*/ c) => new(c.R, -c.I);
	// negative: - a - bi
	public static Complex/*<T>*/ operator -(Complex/*<T>*/ c) => new(-c.R, -c.I);
	public static Complex/*<T>*/ operator !(Complex/*<T>*/ c) => new(-c.I, c.R);
	public static Complex/*<T>*/ U(Complex/*<T>*/ c) => new(IScalar<double>.zero, c.I);
	// i * complex
	public static Complex/*<T>*/ MulU(Complex/*<T>*/ c) => new(-c.I, c.R);
	// -i * complex
	public static Complex/*<T>*/ NegU(Complex/*<T>*/ c) => new(c.I, -c.R);
	// -i * complex
	public static Complex/*<T>*/ NegI(Complex/*<T>*/ c) => new(c.I, -c.R);
	// |complex|^2
	public static double operator +(Complex/*<T>*/ c) => Sqr(c.R) + Sqr(c.I);
	// signed fractional part
	public static Complex/*<T>*/ Frac(Complex/*<T>*/ c) => new(c.R - double.Truncate(c.R), c.I - double.Truncate(c.I));
	// truncate
	public static Complex/*<T>*/ Truncate(Complex/*<T>*/ c) => new(double.Truncate(c.R), double.Truncate(c.I));
	// round down
	public static Complex/*<T>*/ Floor(Complex/*<T>*/ c) => new(double.Floor(c.R), double.Floor(c.I));
	// round
	public static Complex/*<T>*/ Round(Complex/*<T>*/ c) => new(double.Round(c.R), double.Round(c.I));
	// round up
	public static Complex/*<T>*/ Ceiling(Complex/*<T>*/ c) => new(double.Ceiling(c.R), double.Ceiling(c.I));
	public static Complex/*<T>*/ Cycle(Complex/*<T>*/ c) => new(IScalar<double>.Cycle(c.R), IScalar<double>.Cycle(c.I));
	// 1 / complex
	public static Complex/*<T>*/ Inv(Complex/*<T>*/ c) => INumber<Complex/*<T>,T*/>.I_Inv(c); //~c / +c;
	// Argument of complex
	public static double Arg(Complex/*<T>*/ c) => double.Atan2(c.I, c.R);
	// from angle
	public static Complex/*<T>*/ InvArg(double p, Complex/*<T>*/ _) => Complex_InvArg(p);
	public static Complex/*<T>*/ Axis(Complex/*<T>*/ q) => i;
	public static Complex/*<T>*/ Complex_InvArg(double p) => new(double.Cos(p), double.Sin(p));
	// square root
	public static Complex/*<T>*/ Sqrt(Complex/*<T>*/ c) { 
		var a = INumber<Complex/*<T>,T*/>.Abs(c);
		var half = IScalar<double>.half;

		return new(
			double.Sqrt(half * (a + c.R)),
			double.CopySign(double.Sqrt(half * (a - c.R)), c.I)
		);
		//var a = INumber<Complex/*<T>*/,double>.Abs(c); 
		//return new(Math.Sqrt(.5 * (a + c.R)), Math.CopySign(Math.Sqrt(.5 * (a - c.R)), c.I));
	}
	// complex^2
	public static Complex/*<T>*/ Sqr(Complex/*<T>*/ c) => new(Sqr(c.R) - Sqr(c.I), 2 * c.R * c.I);
	// real^2
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static double Sqr(double r) => r * r;
	// complex^3
	public static Complex/*<T>*/ Cub(Complex/*<T>*/ c) { double cr = c.R, ci = c.I, rr = cr*cr, ii = ci*cr; return new(cr * (rr - 3 * ii), ci * (3 * rr - ii)); }
	// complex^4
	public static Complex/*<T>*/ Quart(Complex/*<T>*/ c) { double cr = c.R, ci = c.I, a = cr * cr + ci * ci, ri = cr * ci; return new(a * a - 6 * ri * ri, 4 * a * ri); }
	// |a| + |b|i
	public static Complex/*<T>*/ AbsComp(Complex/*<T>*/ c) => new(Math.Abs(c.R), Math.Abs(c.I));
	public static double Dot(Complex/*<T>*/ a, Complex/*<T>*/ b) => a.R * b.R + a.I * b.I;
	public static Complex/*<T>*/ Min(Complex/*<T>*/ a, Complex/*<T>*/ b) =>  D2(a,b,Static.Min);
	public static Complex/*<T>*/ Max(Complex/*<T>*/ a, Complex/*<T>*/ b) => D2(a,b,Static.Max);
	public static Complex/*<T>*/ Clamp(Complex/*<T>*/ c, Complex/*<T>*/ min, Complex/*<T>*/ max) => D3(c, min, max, Static.Clamp);
	#endregion

	#region Additions
	public static Complex/*<T>*/ operator ++(Complex/*<T>*/ c) => c + 1;
	public static Complex/*<T>*/ operator +(Complex/*<T>*/ a, Complex/*<T>*/ b) => new(a.R + b.R, a.I + b.I);
	// complex + real
	public static Complex/*<T>*/ operator +(Complex/*<T>*/ c, double r) => new(c.R + r, c.I);
	// real + complex
	public static Complex/*<T>*/ operator +(double r, Complex/*<T>*/ c) => new(c.R + r, c.I);
	// complex + imaginary
	public static Complex/*<T>*/ AddV(Complex/*<T>*/ c, double v) => new(c.R, c.I + v);
	// imaginary + complex
	public static Complex/*<T>*/ AddV(double v, Complex/*<T>*/ c) => new(c.R, c.I + v);
	#endregion

	#region Subtractions
	public static Complex/*<T>*/ operator --(Complex/*<T>*/ c) => c - 1;
	public static Complex/*<T>*/ operator -(Complex/*<T>*/ a, Complex/*<T>*/ b) => new(a.R - b.R, a.I - b.I);
	// complex - real
	public static Complex/*<T>*/ operator -(Complex/*<T>*/ c, double r) => new(c.R - r, c.I);
	// real - complex
	public static Complex/*<T>*/ operator -(double r, Complex/*<T>*/ c) => new(r - c.R, -c.I);
	// complex - imaginary
	public static Complex/*<T>*/ SubV(Complex/*<T>*/ c, double v) => new(c.R, c.I - v);
	// imaginary - complex
	public static Complex/*<T>*/ SubV(double v, Complex/*<T>*/ c) => new(-c.R, v - c.I);
	#endregion

	#region Multiplications
	public static Complex/*<T>*/ operator *(Complex/*<T>*/ a, Complex/*<T>*/ b) => new(a.R * b.R - a.I * b.I, a.R * b.I + a.I * b.R);
	// complex * real
	public static Complex/*<T>*/ operator *(Complex/*<T>*/ c, double r) => new(r * c.R, r * c.I);
	// real * complex
	public static Complex/*<T>*/ operator *(double r, Complex/*<T>*/ c) => new(r * c.R, r * c.I);
	// complex * imaginary
	public static Complex/*<T>*/ MulI(Complex/*<T>*/ c, double v) => new(-v * c.I, v * c.R);
	// imaginary * complex
	public static Complex/*<T>*/ MulI(double v, Complex/*<T>*/ c) => new(-v * c.I, v * c.R);
	public static double operator |(Complex/*<T>*/ a, Complex/*<T>*/ b) => a.R * b.R + a.I * b.I;
	#endregion

	#region Divisions
	public static Complex/*<T>*/ operator /(Complex/*<T>*/ a, Complex/*<T>*/ b) => a * Inv(b);
	// complex / real
	public static Complex/*<T>*/ operator /(Complex/*<T>*/ c, double r) => new(c.R / r, c.I / r);
	// real / complex
	public static Complex/*<T>*/ operator /(double r, Complex/*<T>*/ c) => r * Inv(c);
	// complex / imaginary
	public static Complex/*<T>*/ DivI(Complex/*<T>*/ c, double v) => new(c.I / v, c.R / -v);
	// imaginary / complex
	public static Complex/*<T>*/ DivI(double v, Complex/*<T>*/ c) => MulI(v, Inv(c));
	public static Complex/*<T>*/ LDiv(Complex/*<T>*/ a, Complex/*<T>*/ b) => a / b;
	public static Complex/*<T>*/ operator %(Complex/*<T>*/ a, Complex/*<T>*/ b) => INumber<Complex/*<T>*/>.NewMod(a,b);
	#endregion

	#region ExpLogs
	// Ln(complex)
	public static Complex/*<T>*/ Log(Complex/*<T>*/ c) => new(Math.Log(+c) * .5, Arg(c));
	// Ln(complex)/2
	public static Complex/*<T>*/ LogH(Complex/*<T>*/ c) => new(Math.Log(+c) * .25, Arg(c) * .5);
	// e ^ complex
	public static Complex/*<T>*/ Exp(Complex/*<T>*/ c) => double.IsNegativeInfinity(c.R) ? zero : Math.Exp(c.R) * Complex_InvArg(c.I);
	public static Complex/*<T>*/ operator ^(Complex/*<T>*/ a, Complex/*<T>*/ b) => Exp(Log(a) * b);
	// complex ^ real
	public static Complex/*<T>*/ operator ^(Complex/*<T>*/ c, double r) => Exp(Log(c) * r);
	// real ^ complex
	public static Complex/*<T>*/ operator ^(double r, Complex/*<T>*/ c) => 0 <= r ? Exp(Math.Log(r) * c) : Exp(new Complex/*<T>*/(Math.Log(-r), Math.PI) * c);
	// (-1) ^ complex
	public static Complex/*<T>*/ PowN1(Complex/*<T>*/ c) => Exp(new(-c.I * Math.PI, c.R * Math.PI));
	// i ^ complex
	public static Complex/*<T>*/ PowI(Complex/*<T>*/ c) => Exp(new(-c.I * QTau, c.R * QTau));
	#endregion

	#region Hyperbolics
	// direct double math doesn't need complex Exp
	//public static Complex/*<T>*/ Cosh(Complex/*<T>*/ c) => (Exp(c) + Exp(-c)) / 2.0; 
	public static Complex/*<T>*/ Cosh(Complex/*<T>*/ c) => new(Math.Cos(c.I) * Math.Cosh(c.R), Math.Sin(c.I) * Math.Sinh(c.R));
	// direct double math doesn't need complex Exp
	//public static Complex/*<T>*/ Sinh(Complex/*<T>*/ c) => (Exp(c) - Exp(-c)) / 2.0; 
	public static Complex/*<T>*/ Sinh(Complex/*<T>*/ c) => new(Math.Cos(c.I) * Math.Sinh(c.R), Math.Sin(c.I) * Math.Cosh(c.R));
	// direct double math doesn't need complex Exp
	//public static Complex/*<T>*/ Tanh(Complex/*<T>*/ c) { var e2z = Exp(2 * c); return (e2z - 1) / (e2z + 1); }
	public static Complex/*<T>*/ Tanh(Complex/*<T>*/ c) {
		double t = double.Tan(c.R), h = double.Tanh(c.I), tt = t*t, hh = h*h;
		//return new Complex/*<T>*/(h, t) / new Complex/*<T>*/(1, t * h); // WIKI
		return new Complex/*<T>*/(h * (1 + tt), t * (1 - hh)) / (1 + tt * hh);
	}
	public static Complex/*<T>*/ Coth(Complex/*<T>*/ c) {
		double t = double.Tan(c.R), h = double.Tanh(c.I), tt = t*t, hh = h*h;
		//return new Complex/*<T>*/(1, t * h) / new Complex/*<T>*/(h, t); // WIKI
		return new Complex/*<T>*/(h * (tt + 1), t * (hh - 1)) / (hh + tt);
	}
	#endregion

	#region doublerigonometrics
	// direct double math doesn't need ~
	//public static Complex/*<T>*/ Cos(Complex/*<T>*/ c) => Cosh(~c); 
	public static Complex/*<T>*/ Cos(Complex/*<T>*/ c) => new(Math.Cos(c.R) * Math.Cosh(c.I), Math.Sin(-c.R) * Math.Sinh(c.I));
	// direct double math doesn't need NI and ~
	//public static Complex/*<T>*/ Sin(Complex/*<T>*/ c) => NI(Sinh(~c)); 
	public static Complex/*<T>*/ Sin(Complex/*<T>*/ c) => new(Math.Sin(c.R) * Math.Cosh(c.I), Math.Cos(c.R) * Math.Sinh(c.I));
	// direct double math doesn't need NI and ~
	//public static Complex/*<T>*/ Tan(Complex/*<T>*/ c) => NI(Tanh(~c));
	public static Complex/*<T>*/ Tan(Complex/*<T>*/ c) {
		double t = double.Tan(c.R), h = double.Tanh(c.I), tt = Sqr(t), hh = Sqr(h);
		//return new Complex/*<T>*/(t, h) / new Complex/*<T>*/(1, -t * h); // WIKI
		return new Complex/*<T>*/(t * (1 - hh), h * (1 + tt)) / (1 + tt * hh); // simplified into double math
	}
	public static Complex/*<T>*/ Cot(Complex/*<T>*/ c) {
		double t = double.Tan(c.R), h = double.Tanh(c.I), tt = Sqr(t), hh = Sqr(h);
		//return new Complex/*<T>*/(1, -t * h) / new Complex/*<T>*/(t, h); // WIKI
		return new Complex/*<T>*/(t * (1 - hh), -h * (1 + tt)) / (tt + hh); // simplified into double math
	}
	#endregion

	#region ArcHyperbolics
	public static Complex/*<T>*/ Acosh(Complex/*<T>*/ c) => INumber<Complex/*<T>*/>.I_Acosh(c);
	public static Complex/*<T>*/ Asinh(Complex/*<T>*/ c) => INumber<Complex/*<T>*/>.I_Asinh(c);
	public static Complex/*<T>*/ Atanh(Complex/*<T>*/ c) => INumber<Complex/*<T>*/>.I_Atanh(c);
	public static Complex/*<T>*/ Acoth(Complex/*<T>*/ c) => INumber<Complex/*<T>*/>.I_Acoth(c);
	#endregion

	#region Arcdoublerigonometrics
	public static Complex/*<T>*/ Acos(Complex/*<T>*/ c) => INumber<Complex/*<T>*/>.I_Acos(c);
	public static Complex/*<T>*/ Asin(Complex/*<T>*/ c) => INumber<Complex/*<T>*/>.I_Asin(c);
	public static Complex/*<T>*/ Atan(Complex/*<T>*/ c) => INumber<Complex/*<T>*/>.I_Atan(c);
	public static Complex/*<T>*/ Acot(Complex/*<T>*/ c) => INumber<Complex/*<T>*/>.I_Acot(c);
	#endregion

	#region Exotic doublerigonometrics
	// -i*((-1)^c - (-1)^(-c)) = 2sin(πc)
	public static Complex/*<T>*/ Sin_P(Complex/*<T>*/ c) {
		double ci = c.I * Math.PI, cr = c.R * Math.PI, cos = Math.Cos(cr), sin = Math.Sin(cr), e = Math.Exp(ci), ie = Math.Exp(-ci);
		return new Complex/*<T>*/((ie + e) * sin, (e - ie) * cos);
	}
	// -i * ((i)^c - (i)^(-c)) = 2sin(πc/2)
	public static Complex/*<T>*/ Sin_2Q(Complex/*<T>*/ c) {
		double ci = c.I * QTau, cr = c.R * QTau, cos = Math.Cos(cr), sin = Math.Sin(cr), e = Math.Exp(ci), ie = Math.Exp(-ci);
		return new Complex/*<T>*/((e + ie) * sin, (e - ie) * cos);
	}
	#endregion

	#region Special Functions
	public static Complex/*<T>*/ Gauss(Complex/*<T>*/ c) => INumber<Complex/*<T>*/>.I_Gauss(c);
	public static Complex/*<T>*/ Gamma(Complex/*<T>*/ c) => INumber<Complex/*<T>*/>.I_Gamma(c);
	public static Complex/*<T>*/ Factorial(Complex/*<T>*/ c) => INumber<Complex/*<T>*/>.I_Factorial(c);
	public static Complex/*<T>*/ Zeta(Complex/*<T>*/ c) => INumber<Complex/*<T>*/>.I_Zeta(c);
	#endregion
	public static void IndexAndAddToRgb(Color[] axis, Complex/*<T>*/ indices, Complex/*<T>*/ value) {
		var a = axis[(int)indices.R];
		if (indices.R >= 0 && indices.R < axis.Length)
			axis[(int)indices.R] = Color.FromArgb(a.R + (int)value.R, a.G, a.B);
		if (indices.I >= 0 && indices.I < axis.Length)
			axis[(int)indices.I] = Color.FromArgb(a.R, a.G, a.B + (int)value.I);
	}
}
/* this one was originally used for zeta reflection, but it combined itself with SinN1 into NISinI
// i^c + i^(-c) = 2cos(πc/2) // is this faster than 2*double.Cos(qTau * c)? double.Cos(c) = new(Math.Cos(c.R) * Math.Cosh(c.I), Math.Sin(-c.R) * Math.Sinh(c.I));
private static double CosI(double c) {
	double i = c.I * qTau, r = c.R * qTau, cos = Math.Cos(r), sin = Math.Sin(r), e = Math.Exp(i), ie = 1 / e;
	return new double((ie - e) * cos, (ie + e) * sin);
}*/