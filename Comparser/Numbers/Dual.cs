using System.Runtime.CompilerServices;
using static Comparser.Comparser.Numbers.Static;

namespace Comparser.Comparser.Numbers;
public readonly struct Dual<T,>(T r = default!, T e = default!) : INumber<Dual<T>> where T: unmanaged, INumber<T> {
	public readonly T R = r, E = e;

	#region Query
	public bool Is0() => T.Is0(R) && T.Is0(E);
	public bool IsNaN() => T.IsNaN(R) || T.IsNaN(E);

	public override string ToString() => ToString(-1);
	public static string[] epsUnit => [];
	public static double[] EpsValues(Dual<T> r, Dual<T> e) => [];
	public string ToString(int d) => ValueToString(T.epsUnit, T.EpsValues(R, E), d);
	#endregion

	#region Constants
	
	public static Dual<T> zero => default;
	public static Dual<T> nan => new(T.nan, T.nan);
	public static Dual<T> unit => new(T.unit);
	public static Dual<T> one => new(T.one, T.one);
	public static Dual<T> u => new(T.u, T.u);
	public static Dual<T> minusUnit => new(T.minusUnit);
	public static Dual<T> minusOne => new(T.minusOne, T.minusOne);
	public static Dual<T> minusU => new(T.minusU, T.minusU);
	#endregion

	#region Helpers
	//public static (double r, double i) ToPair(Dual<T> c) => (c.R, c.I);
	public static T Mix(Dual<T> c, Func<T, T, T> d) => d(c.R, c.E);
	public static Dual<T> D1(Dual<T> a, Func<T, T> d) => new(d(a.R), d(a.E));
	public static Dual<T> D2(Dual<T> a, Dual<T> b, Func<T, T, T> d) => new(d(a.R, b.R), d(a.E, b.E));
	public static Dual<T> D3(Dual<T> a, Dual<T> b, Dual<T> c, Func<T, T, T, T> d) => new(d(a.R, b.R, c.R), d(a.E, b.E, c.E));
	#endregion
	
	#region Basics
	public static bool AreEqual(Dual<T> a, Dual<T> b) => INumber<T>.Abs(a.R - b.R) < 1e-8 && INumber<T>.Abs(a.E - b.E) < 1e-8;
	public static double Re(Dual<T> c) => T.Re(c.R);
	public static double Im(Dual<T> c) => T.Im(c.R);
	public static double ImMag(Dual<T> c) => T.ImMag(c.R);
	public static Dual<T> MakeR(double r) => new(T.MakeR(r));
	//public static Dual<T> Swap(Dual<T> c) => new(c.I, c.R);
	// conjugate: a - bi
	public static Dual<T> operator ~(Dual<T> c) => new(INumber<T>.Conj(c.R), INumber<T>.Conj(c.E));
	// negative: - a - bi
	public static Dual<T> operator -(Dual<T> c) => new(-c.R, -c.E);
	
	public static Dual<T> operator !(Dual<T> c) => u * c;
	public static Dual<T> U(Dual<T> c) => new(T.U(c.R), T.U(c.E));
	// i * dual
	public static Dual<T> MulU(Dual<T> c) => U(one) * c;
	// -i * dual
	public static Dual<T> NegU(Dual<T> c) => U(minusOne) * c;
	// |dual|^2
	public static double operator +(Dual<T> c) => +c.R;
	// signed fractional part
	public static Dual<T> Frac(Dual<T> c) => new(T.Frac(c.R), T.Frac(c.E));
	// truncate
	public static Dual<T> Trunc(Dual<T> c) => new(T.Trunc(c.R), T.Trunc(c.E));
	// round down
	public static Dual<T> Floor(Dual<T> c) => new(T.Floor(c.R), T.Floor(c.E));
	// round
	public static Dual<T> Round(Dual<T> c) => new(T.Round(c.R), T.Round(c.E));
	// round up
	public static Dual<T> Ceil(Dual<T> c) => new(T.Ceil(c.R), T.Ceil(c.E));
	public static Dual<T> Cycle(Dual<T> c) => new(T.Cycle(c.R), T.Cycle(c.E));
	// 1 / dual
	public static Dual<T> Dconj(Dual<T> c) => new(c.R, -c.E);
	public static Dual<T> Inv(Dual<T> c) => new(T.Inv(c.R), -T.Inv(c.R) * c.E * T.Inv(c.R))
	// Argument of dual
	public static double Arg(Dual<T> c) => T.Arg(c.E);
	// from angle
	public static Dual<T> InvArg(double angle, Dual<T> axis) => new(T.InvArg(angle, axis.R), T.InvArg(angle, axis.E));
	public static Dual<T> Axis(Dual<T> q) =>  new(T.Axis(q.E), T.Axis(q.E));
	// square root
	public static Dual<T> Sqrt(Dual<T> c) => new(Sqrt(c.R), c.E / (2 * c.R));
	// dual^2
	public static Dual<T> Sqr(Dual<T> c) => new(T.Sqr(c.R), c.R * c.E + c.E * c.R); // maybe can't simplify into 2RI, because R and E could be quaternions...?
	// dual^3
	public static Dual<T> Cub(Dual<T> c) /* new(T.Sqr(c.R), c.R * c.E + c.E * c.R) *  new(c.R, c.E)*/ => new(T.Cub(c.R), T.Sqr(c.R) * c.E + (c.R * c.E * c.R + c.E * T.Sqr(c.R))); // a=T.Sqr(c.R), b = c.R * c.E + c.E * c.R, c= c.R, d = c.E
	// dual^ * 4
	public static Dual<T> Quart(Dual<T> c) => Sqr(c) * Sqr(c);
	// |a| + |b|i
	public static Dual<T> AbsComp(Dual<T> c) => new(T.AbsComp(c.R), T.AbsComp(c.E));
	public static double Dot(Dual<T> a, Dual<T> b) => T.Dot(a.R, b.R);
	public static Dual<T> Min(Dual<T> a, Dual<T> b) =>  D2(a,b,T.Min);
	public static Dual<T> Max(Dual<T> a, Dual<T> b) => D2(a,b,T.Max);
	public static Dual<T> Clamp(Dual<T> c, Dual<T> min, Dual<T> max) => D3(c, min, max, T.Clamp);
	#endregion

	#region Additions
	public static Dual<T> operator ++(Dual<T> c) => c + 1;
	public static Dual<T> operator +(Dual<T> a, Dual<T> b) => new(a.R + b.R, a.E + b.E);
	// dual + real
	public static Dual<T> operator +(Dual<T> c, double r) => new(c.R + r, c.E);
	// real + dual
	public static Dual<T> operator +(double r, Dual<T> c) => new(c.R + r, c.E);
	// dual + imaginary
	public static Dual<T> AddV(Dual<T> c, double v) => new(T.AddV(c.R, v), c.E);
	// imaginary + dual
	public static Dual<T> AddV(double v, Dual<T> c) => new(T.AddV(v, c.R), c.E);
	#endregion

	#region Subtractions
	public static Dual<T> operator --(Dual<T> c) => c - 1;
	public static Dual<T> operator -(Dual<T> a, Dual<T> b) => new(a.R - b.R, a.E - b.E);
	// dual - real
	public static Dual<T> operator -(Dual<T> c, double r) => new(c.R - r, c.E);
	// real - dual
	public static Dual<T> operator -(double r, Dual<T> c) => new(r - c.R, -c.E);
	// dual - imaginary
	public static Dual<T> SubV(Dual<T> c, double v) => new(T.SubV(c.R, v), c.E);
	// imaginary - dual
	public static Dual<T> SubV(double v, Dual<T> c) => new(T.SubV(v, c.R), c.E);
	#endregion

	#region Multiplications
	public static Dual<T> operator *(Dual<T> a, Dual<T> b) => new(a.R * b.R, a.R * b.E + a.E * b.R);
	// dual * real
	public static Dual<T> operator *(Dual<T> c, double r) => new(r * c.R, r * c.E);
	// real * dual
	public static Dual<T> operator *(double r, Dual<T> c) => new(r * c.R, r * c.E);
	// dot
	public static double operator |(Dual<T> a, Dual<T> b) => (a.R | b.R);// + (a.E | b.E);
	#endregion

	#region Divisions
	public static Dual<T> operator /(Dual<T> a, Dual<T> b) => a * Dconj(b) / (Inv(b) * Dconj(b));
	// dual / real
	public static Dual<T> operator /(Dual<T> c, double r) => new(c.R / r, c.E / r);
	// real / dual
	public static Dual<T> operator /(double r, Dual<T> c) => r * Inv(c);
	public static Dual<T> LDiv(Dual<T> a, Dual<T> b) => a / b;
	public static Dual<T> operator %(Dual<T> a, Dual<T> b) => INumber<Dual<T>>.NewMod(a,b);
	#endregion

	#region ExpLogs
	// Ln(dual)
	public static Dual<T> Log(Dual<T> c) => new(T.Log(c.R), c.E / c.R);
	// Ln(dual)/2
	public static Dual<T> LogH(Dual<T> c) => new(Math.Log(+c) * .25, Arg(c) * .5);
	// e ^ dual
	public static Dual<T> Exp(Dual<T> c) { var e = T.Exp(c.R); return new(e, e * c.E); }
	public static Dual<T> operator ^(Dual<T> a, Dual<T> b) => Exp(Log(a) * b);
	// dual ^ real
	public static Dual<T> operator ^(Dual<T> c, double r) => Exp(Log(c) * r);
	// real ^ dual
	public static Dual<T> operator ^(double r, Dual<T> c) => 0 <= r ? Exp(Math.Log(r) * c) : Exp(new Dual<T>(Math.Log(-r), Math.PI) * c);
	// (-1) ^ dual
	public static Dual<T> PowN1(Dual<T> c) => Exp(new(-c.I * Math.PI, c.R * Math.PI));
	// i ^ dual
	public static Dual<T> PowI(Dual<T> c) => Exp(new(-c.I * QTau, c.R * QTau));
	#endregion

	#region Hyperbolics
	// direct double math doesn't need dual Exp
	//public static Dual<T> Cosh(Dual<T> c) => (Exp(c) + Exp(-c)) / 2.0; 
	public static Dual<T> Cosh(Dual<T> c) => new(Math.Cos(c.I) * Math.Cosh(c.R), Math.Sin(c.I) * Math.Sinh(c.R));
	// direct double math doesn't need dual Exp
	//public static Dual<T> Sinh(Dual<T> c) => (Exp(c) - Exp(-c)) / 2.0; 
	public static Dual<T> Sinh(Dual<T> c) => new(Math.Cos(c.I) * Math.Sinh(c.R), Math.Sin(c.I) * Math.Cosh(c.R));
	// direct double math doesn't need dual Exp
	//public static Dual<T> Tanh(Dual<T> c) { var e2z = Exp(2 * c); return (e2z - 1) / (e2z + 1); }
	public static Dual<T> Tanh(Dual<T> c) {
		double t = Math.Tan(c.R), h = Math.Tanh(c.I), tt = t*t, hh = h*h;
		//return new Dual<T>(h, t) / new Dual<T>(1, t * h); // WIKI
		return new Dual<T>(h * (1 + tt), t * (1 - hh)) / (1 + tt * hh);
	}
	public static Dual<T> Coth(Dual<T> c) {
		double t = Math.Tan(c.R), h = Math.Tanh(c.I), tt = t*t, hh = h*h;
		//return new Dual<T>(1, t * h) / new Dual<T>(h, t); // WIKI
		return new Dual<T>(h * (tt + 1), t * (hh - 1)) / (hh + tt);
	}
	#endregion

	#region Trigonometrics
	// direct double math doesn't need ~
	//public static Dual<T> Cos(Dual<T> c) => Cosh(~c); 
	public static Dual<T> Cos(Dual<T> c) => new(Math.Cos(c.R) * Math.Cosh(c.I), Math.Sin(-c.R) * Math.Sinh(c.I));
	// direct double math doesn't need NI and ~
	//public static Dual<T> Sin(Dual<T> c) => NI(Sinh(~c)); 
	public static Dual<T> Sin(Dual<T> c) => new(Math.Sin(c.R) * Math.Cosh(c.I), Math.Cos(c.R) * Math.Sinh(c.I));
	// direct double math doesn't need NI and ~
	//public static Dual<T> Tan(Dual<T> c) => NI(Tanh(~c));
	public static Dual<T> Tan(Dual<T> c) {
		double t = Math.Tan(c.R), h = Math.Tanh(c.I), tt = Sqr(t), hh = Sqr(h);
		//return new Dual<T>(t, h) / new Dual<T>(1, -t * h); // WIKI
		return new Dual<T>(t * (1 - hh), h * (1 + tt)) / (1 + tt * hh); // simplified into double math
	}
	public static Dual<T> Cot(Dual<T> c) {
		double t = Math.Tan(c.R), h = Math.Tanh(c.I), tt = Sqr(t), hh = Sqr(h);
		//return new Dual<T>(1, -t * h) / new Dual<T>(t, h); // WIKI
		return new Dual<T>(t * (1 - hh), -h * (1 + tt)) / (tt + hh); // simplified into double math
	}
	#endregion

	#region ArcHyperbolics
	public static Dual<T> Acosh(Dual<T> c) => INumber<Dual<T>>.I_Acosh(c);
	public static Dual<T> Asinh(Dual<T> c) => INumber<Dual<T>>.I_Asinh(c);
	public static Dual<T> Atanh(Dual<T> c) => INumber<Dual<T>>.I_Atanh(c);
	public static Dual<T> Acoth(Dual<T> c) => INumber<Dual<T>>.I_Acoth(c);
	#endregion

	#region ArcTrigonometrics
	public static Dual<T> Acos(Dual<T> c) => INumber<Dual<T>>.I_Acos(c);
	public static Dual<T> Asin(Dual<T> c) => INumber<Dual<T>>.I_Asin(c);
	public static Dual<T> Atan(Dual<T> c) => INumber<Dual<T>>.I_Atan(c);
	public static Dual<T> Acot(Dual<T> c) => INumber<Dual<T>>.I_Acot(c);
	#endregion

	#region Exotic Trigonometrics
	// -i*((-1)^c - (-1)^(-c)) = 2sin(πc)
	public static Dual<T> Sin_P(Dual<T> c) {
		double ci = c.I * Math.PI, cr = c.R * Math.PI, cos = Math.Cos(cr), sin = Math.Sin(cr), e = Math.Exp(ci), ie = Math.Exp(-ci);
		return new Dual<T>((ie + e) * sin, (e - ie) * cos);
	}
	// -i * ((i)^c - (i)^(-c)) = 2sin(πc/2)
	public static Dual<T> Sin_2Q(Dual<T> c) {
		double ci = c.I * QTau, cr = c.R * QTau, cos = Math.Cos(cr), sin = Math.Sin(cr), e = Math.Exp(ci), ie = Math.Exp(-ci);
		return new Dual<T>((e + ie) * sin, (e - ie) * cos);
	}
	#endregion

	#region Special Functions
	public static Dual<T> Gauss(Dual<T> c) => INumber<Dual<T>>.I_Gauss(c);
	public static Dual<T> Gamma(Dual<T> c) => INumber<Dual<T>>.I_Gamma(c);
	public static Dual<T> Factorial(Dual<T> c) => INumber<Dual<T>>.I_Factorial(c);
	public static Dual<T> Zeta(Dual<T> c) => INumber<Dual<T>>.I_Zeta(c);
	#endregion
	public static void IndexAndAddToRgb(Color[] axis, Dual<T> indices, Dual<T> value) {
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