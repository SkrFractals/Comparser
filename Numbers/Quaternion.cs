namespace Comparser.Numbers;
public readonly struct Quaternion(double r = 0, double i = 0, double j = 0, double k = 0) : INumber<Quaternion> {

	public readonly double R = r, I = i, J = j, K = k;

	#region Quaternion Constants
	public static Quaternion i => new(0, 1, 0, 0);
	public static Quaternion j => new(0, 0, 1, 0);
	public static Quaternion k => new(0, 0, 0, 1);
	public static Quaternion ni => new(0, -1, 0, 0);
	public static Quaternion nj => new(0, 0, -1, 0);
	public static Quaternion nk => new(0, 0, 0, -1);
	#endregion

	#region Query
	public bool Is0() => R == 0 && I == 0 && J == 0 && K == 0;
	public bool IsNaN() => double.IsNaN(R) || double.IsNaN(I);

	public override string ToString() => ToString(-1);
	public string ToString(int d) {
		var ijk = " ijkx";
		var s = "";
		var r = "";
		var i = -1;
		string[] v = [INumber<Complex>._sr(R, d), INumber<Complex>._sr(I, d), INumber<Complex>._sr(J, d), INumber<Complex>._sr(K, d)];
		while (4 > ++i && ijk[0] != 'x') {
			string l;
			for (r += s, l = ijk[..1], ijk = ijk[1..]; i < 4 && v[i] == "0"; ++i) {
				l = ijk[..1];
				ijk = ijk[1..];
			}
			if (4 <= i) {
				if (r == "")
					r = "0";
				break;
			}
			r += i == 0 ? INumber<Complex>._i(v[i], "") : INumber<Complex>._i(v[i], l);
			s = " ";
		}
		return r;
	}
	#endregion

	#region Constants
	public static Quaternion Zero() => new(0);
	public static Quaternion One() => new(1, 1, 1, 1);
	public static Quaternion NaN() => new(double.NaN, double.NaN, double.NaN, double.NaN);
	#endregion

	#region Helpers
	private static double IDot(Quaternion q) => q.I * q.I + q.J * q.J + q.K * q.K;
	private static Quaternion D1(Quaternion a, Func<double, double> D) => new(D(a.R), D(a.I), D(a.J), D(a.K));
	private static Quaternion D2(Quaternion a, Quaternion b, Func<double, double, double> D) => new(D(a.R, b.R), D(a.I, b.I), D(a.J, b.J), D(a.K, b.K));
	private static Quaternion D3(Quaternion a, Quaternion b, Quaternion c, Func<double, double, double, double> D) => new(D(a.R, b.R, c.R), D(a.I, b.I, c.I), D(a.J, b.J, c.J), D(a.K, b.K, c.K));
	#endregion

	#region Basics
	public static bool Compare(Quaternion a, Quaternion b) => a.R == b.R && a.I == b.I && a.J == b.J && a.K == b.K;
	// conjugate: a - bi
	public static double Re(Quaternion q) => q.R;
	public static Quaternion MakeR(double r) => new(r);
	public static double Im(Quaternion q) => Math.Sqrt(IDot(q));
	// conjugate: a - bi
	public static Quaternion operator !(Quaternion q) => new(q.R, -q.I, -q.K, -q.K);
	// negative: - a - bi
	public static Quaternion operator -(Quaternion q) => new(-q.R, -q.I, -q.J, -q.K);
	// i * quaternion
	public static Quaternion MU(Quaternion q) => new(-q.I - q.J - q.K, q.R + q.K - q.J, q.R + q.I - q.K, q.J - q.I + q.R);
	public static Quaternion operator ~(Quaternion q) => new(-q.I - q.J - q.K, q.R + q.K - q.J, q.R + q.I - q.K, q.J - q.I + q.R);
	//=> new Quaternion(0, q.I, q.J, q.K) * q;
	public static Quaternion U(Quaternion q) => new(0, q.I, q.J, q.K);
	// -u * quaternion
	public static Quaternion NU(Quaternion q) => new(q.I + q.J + q.K, q.J - q.R - q.K, q.K - q.R - q.I, q.I - q.J - q.R);
	// -i * quaternion
	public static Quaternion NI(Quaternion q) => new(q.I, -q.R, q.K, -q.J);
	// -j * quaternion
	public static Quaternion NJ(Quaternion q) => new(q.J, -q.K, -q.R, q.I);
	// -k * quaternion
	public static Quaternion NK(Quaternion q) => new(q.K, q.J, -q.I, -q.R);
	// |quaternion|^2
	public static double operator +(Quaternion q) => q.R * q.R + IDot(q);
	// signed fractional part
	public static Quaternion Frac(Quaternion q) => D1(q, (r) => r - Math.Truncate(r));
	// truncate
	public static Quaternion Trunc(Quaternion q) => D1(q, Math.Truncate);
	// round down
	public static Quaternion Floor(Quaternion q) => D1(q, Math.Floor);
	// round
	public static Quaternion Round(Quaternion q) => D1(q, Math.Round);
	// round up
	public static Quaternion Ceil(Quaternion q) => D1(q, Math.Ceiling);
	// 1 / quaternion
	public static Quaternion Inv(Quaternion q) => !q / +q;
	// = iτ/c // using this in my Gamma_Stirling_Negative, maybe won't work for quaternions as it's only using i
	//public static Quaternion InvITau(Quaternion c) => new Quaternion(c.I, c.R) * (Math.Tau / +c);
	// Argument of quaternion
	public static double Arg(Quaternion q) => Math.Atan2(IDot(q), q.R);
	// from angle
	public static Quaternion IArg(double angle, Quaternion axis) { var s = Math.Sin(angle); return new(Math.Cos(angle), axis.I * s, axis.J * s, axis.K * s); }
	// square root
	public static Quaternion Sqrt(Quaternion q) {
		double r = q.R, i = IDot(q), a = Math.Sqrt(.5 * (Math.Sqrt(r * r + i) + r));
		return a == 0 ? new(0, Math.Sqrt(-r)) : new(a, q.I / (a *= 2), q.J / a, q.K / a);
		//return q ^ .5; // is there some similar for like below?
		//var a = INumber<Quaternion>.Abs(c);
		//return new(Math.Sqrt(.5 * (a + c.R)), Math.CopySign(Math.Sqrt(.5 * (a - c.R)), c.I));
	}
	// quaternion^2
	public static Quaternion Sqr(Quaternion q) { 
		double r = q.R, i = IDot(q), a = 2 * r;
		return new(r * r - i, a * q.I, a * q.J, a * q.K);
	}
	// quaternion^3
	public static Quaternion Cub(Quaternion q) {
		double r = q.R, i = IDot(q), r2 = r * r, v = 3 * r2 - i;
		return new(r * (r2 - 3 * i), v * q.I, v * q.J, v * q.K);
		//double I = IDot(q), a = 2 * q.R, aa = q.R * a, AI = q.R * q.R - I;
		//return new(a * AI - a * I, q.I * (AI += aa), q.J * AI, q.K * AI);
	}
	// quaternion^4
	public static Quaternion Quart(Quaternion q) {
		double r = q.R, i = IDot(q), r2 = r * r, v = 4 * r * (r2 - i);
		return new(r2 * r2 - 6 * r2 * i + i * i, v * q.I, v * q.J, v * q.K);
	}
	// |a| + |b|i
	public static Quaternion AbsComp(Quaternion q) => D1(q, Math.Abs);
	public static Quaternion Min(Quaternion a, Quaternion b) => D2(a, b, INumber<Quaternion>.Min);
	public static Quaternion Max(Quaternion a, Quaternion b) => D2(a, b, INumber<Quaternion>.Max);
	public static Quaternion Clamp(Quaternion q, Quaternion min, Quaternion max) => D3(q, min, max, INumber<Quaternion>.Clamp);
	#endregion

	#region Additions
	public static Quaternion operator +(Quaternion a, Quaternion b) => D2(a, b, INumber<Quaternion>.Add);
	// quaternion + real
	public static Quaternion operator +(Quaternion q, double r) => new(q.R + r, q.I, q.J, q.K);
	// real + quaternion
	public static Quaternion operator +(double r, Quaternion q) => new(q.R + r, q.I, q.J, q.K);
	// quaternion + imaginary
	public static Quaternion AddNV(Quaternion q, double i) { var nv = Math.Sqrt(3 * i * i); return new(q.R, q.I + nv, q.J + nv, q.K + nv); }
	// quaternion + imaginary
	public static Quaternion AddNV(double i, Quaternion q) { var nv = Math.Sqrt(3 * i * i); return new(q.R, q.I + nv, q.J + nv, q.K + nv); }
	// quaternion + imaginary
	public static Quaternion AddI(Quaternion q, double i) => new(q.R, q.I + i, q.J, q.K);
	// quaternion + imaginary
	public static Quaternion AddJ(Quaternion q, double j) => new(q.R, q.I, q.J + j, q.K);
	// quaternion + imaginary
	public static Quaternion AddK(Quaternion q, double k) => new(q.R, q.I, q.J, q.K + k);
	// imaginary + quaternion
	#endregion

	#region Subtractions
	public static Quaternion operator -(Quaternion a, Quaternion b) => D2(a, b, INumber<Quaternion>.Sub);
	// quaternion - real
	public static Quaternion operator -(Quaternion q, double r) => new(q.R - r, q.I, q.J, q.K);
	// real - quaternion
	public static Quaternion operator -(double r, Quaternion q) => new(r - q.R, -q.I, -q.J, -q.K);
	// quaternion + imaginary
	public static Quaternion SubNV(Quaternion q, double i) { var nv = Math.Sqrt(3 * i * i); return new(q.R, q.I - nv, q.J - nv, q.K - nv); }
	// quaternion + imaginary
	public static Quaternion SubNV(double i, Quaternion q) { var nv = Math.Sqrt(3 * i * i); return new(-q.R, nv - q.I, nv - q.J, nv - q.K); }
	// quaternion - imaginary
	public static Quaternion SubI(Quaternion q, double i) => new(q.R, q.I - i, q.K, q.K);
	// imaginary - quaternion
	public static Quaternion SubI(double i, Quaternion q) => new(-q.R, i - q.I, q.J, q.K);
	// quaternion - imaginary
	public static Quaternion SubJ(Quaternion q, double j) => new(q.R, q.I, q.J - j, q.K);
	// imaginary - quaternion
	public static Quaternion SubJ(double j, Quaternion q) => new(-q.R, -q.I, j - q.J, -q.K);
	// quaternion - imaginary
	public static Quaternion SubK(Quaternion q, double k) => new(q.R, q.I, q.K, q.K - k);
	// imaginary - quaternion
	public static Quaternion SubK(double k, Quaternion q) => new(-q.R, k - q.I, -q.J, -q.K);
	#endregion

	#region Multiplications
	public static Quaternion operator *(Quaternion a, Quaternion b) =>
		new(a.R * b.R - a.I * b.I - a.J * b.J - a.K * b.K,
			a.R * b.I + a.I * b.R + a.J * b.K - a.K * b.J,
			a.R * b.J - a.I * b.K + a.J * b.R + a.K * b.I,
			a.R * b.K + a.I * b.J - a.J * b.I + a.K * b.R);
	// quaternion * real
	public static Quaternion operator *(Quaternion q, double r) => D1(q, (x) => x * r);
	// real * quaternion
	public static Quaternion operator *(double r, Quaternion q) => D1(q, (x) => x * r);
	// quaternion * imaginary
	public static Quaternion MulI(Quaternion q, double i) => new(-i * q.I, i * q.R, i * q.K, -i * q.J);
	// imaginary * quaternion
	public static Quaternion MulI(double i, Quaternion q) => new(-i * q.I, i * q.R, -i * q.K, i * q.J);
	// quaternion * imaginary
	public static Quaternion MulJ(Quaternion q, double j) => new(-j * q.J, -j * q.K, j * q.R, j * q.I);
	// imaginary * quaternion
	public static Quaternion MulJ(double j, Quaternion q) => new(-j * q.J, j * q.K, j * q.R, -j * q.I);
	// quaternion * imaginary
	public static Quaternion MulK(Quaternion q, double k) => new(-k * q.K, k * q.J, -k * q.I, k * q.R);
	// imaginary * quaternion
	public static Quaternion MulK(double k, Quaternion q) => new(-k * q.K, -k * q.J, k * q.I, k * q.R);

	#endregion

	#region Divisions
	public static Quaternion operator /(Quaternion a, Quaternion b) => a * Inv(b);
	// quaternion / real
	public static Quaternion operator /(Quaternion q, double r) => D1(q, (x) => x / r);
	// real / quaternion
	public static Quaternion operator /(double r, Quaternion q) => r * Inv(q);
	public static Quaternion LDiv(Quaternion a, Quaternion b) => Inv(b) * a;
	public static Quaternion operator %(Quaternion a, Quaternion b) => D2(a, b, INumber<Quaternion>.Mod);

	// quaternion / imaginary (right division)
	public static Quaternion DivI(Quaternion q, double i) => new(q.I / i, q.R / -i, q.K / -i, q.J / i);
	// quaternion / imaginary (left division)
	public static Quaternion LDivI(Quaternion q, double i) => new(q.I / i, q.R / -i, q.K / i, q.J / -i);
	// imaginary / quaternion (right division)
	public static Quaternion DivI(double i, Quaternion q) => MulI(i, Inv(q));
	// imaginary / quaternion (left division)
	public static Quaternion LDivI(double i, Quaternion q) => MulI(Inv(q), i);
	// TODO DivJ, DivK
	#endregion

	#region ExpLogs
	// Ln(quaternion)
	public static Quaternion Log(Quaternion q) {
		var i = IDot(q);
		return i > 0 ? new(.5 * Math.Log(q.R * q.R + i), (i = Math.Atan2(i = Math.Sqrt(i), q.R) / i) * q.I, i * q.J, i * q.K)
			: q.R < 0 ? new(Math.Log(-q.R), Math.PI) : new(Math.Log(q.R));
	}
	// Ln(quaternion)/2
	public static Quaternion LogH(Quaternion q) {
		var i = IDot(q);
		return i < 0 ? new(.25 * Math.Log(q.R * q.R + i), (i = Math.Atan2(i = Math.Sqrt(i), q.R) / (2 * i)) * q.I, i * q.J, i * q.K)
			: q.R < 0 ? new(.5 * Math.Log(-q.R), INumber<Quaternion>.qTau) : new(.5 * Math.Log(q.R));
	}
	// e ^ quaternion
	public static Quaternion Exp(Quaternion q) {
		double e = Math.Exp(q.R), v = Im(q); // v=sqrt(idot(q))
		return v == 0 ? new Quaternion(e, 0, 0, 0) : new Quaternion(e * Math.Cos(v), (e *= Math.Sin(v) / v) * q.I, e * q.J, e * q.K);
	}
	public static Quaternion operator ^(Quaternion a, Quaternion b) => Exp(Log(a) * b);
	// quaternion ^ real
	public static Quaternion operator ^(Quaternion c, double r) => Exp(Log(c) * r);
	// real ^ quaternion
	public static Quaternion operator ^(double r, Quaternion c) => 0 <= r ? Exp(Math.Log(r) * c) : Exp(new Quaternion(Math.Log(-r), Math.PI) * c);
	// (-1) ^ quaternion
	public static Quaternion PowN1(Quaternion c) => Exp(new(-c.I * Math.PI, c.R * Math.PI));
	// i ^ quaternion
	public static Quaternion PowI(Quaternion c) => Exp(new(-c.I * INumber<Quaternion>.qTau, c.R * INumber<Quaternion>.qTau));
	#endregion

	#region Hyperbolics
	public static Quaternion Cosh(Quaternion q) {
		var v = Im(q);
		return new Quaternion(Math.Cos(v) * Math.Cosh(q.R), (v = Math.Sinh(q.R) * (v == 0 ? 0 : Math.Sin(v) / v)) * q.I, v * q.J, v * q.K);
	}
	public static Quaternion Sinh(Quaternion q) {
		var v = Im(q);
		return new Quaternion(Math.Cos(v) * Math.Sinh(q.R), (v = Math.Cosh(q.R) * (v == 0 ? 0 : Math.Sin(v) / v)) * q.I, v * q.J, v * q.K);
	}
	public static Quaternion Tanh(Quaternion c) {
		double i = Im(c), t = Math.Tan(c.R), h = Math.Tanh(i), tt = t * t, hh = h * h, d = 1 + tt * hh, 
			b = i == 0 ? 0 : t * (1 - hh) / (i * d);
		return new Quaternion(h * (1 + tt) / d, b * c.I, b * c.J, b * c.K);
	}
	public static Quaternion Coth(Quaternion c) {
		double i = Im(c), t = Math.Tan(c.R), h = Math.Tanh(i), tt = t * t, hh = h * h, d = tt + hh, 
			b = i == 0 ? 0 : t * (hh - 1) / (i * d);
		return new Quaternion(h * (tt + 1) / d, b * c.I, b * c.J, b * c.K);
	}
	#endregion

	#region Trigonometrics
	public static Quaternion Cos(Quaternion q) {
		var v = Im(q);
		return new Quaternion(Math.Cos(q.R) * Math.Cosh(v), (v = -Math.Sin(q.R) * (v == 0 ? 0 : Math.Sinh(v) / v)) * q.I, v * q.J, v * q.K);
	}
	public static Quaternion Sin(Quaternion q) {
		var v = Im(q);
		return new Quaternion(Math.Sin(q.R) * Math.Cosh(v), (v = Math.Cos(q.R) * (v == 0 ? 0 : Math.Sinh(v) / v)) * q.I, v * q.J, v * q.K);
	}
	public static Quaternion Tan(Quaternion c) {
		double i = Im(c), t = Math.Tan(c.R), h = Math.Tanh(i), tt = t * t, hh = h * h, d = 1 + tt * hh, 
			b = i == 0 ? (1 + tt) / d : h * (1 + tt) / (i * d);
		return new Quaternion(t * (1 - hh) / d, b * c.I, b * c.J, b * c.K);
	}
	public static Quaternion Cot(Quaternion c) {
		double i = Im(c), t = Math.Tan(c.R), h = Math.Tanh(i), tt = t * t, hh = h * h, d = tt + hh, 
			b = i == 0 ? (1 + tt) / -d : -h * (1 + tt) / (i * d);
		return new Quaternion(t * (1 - hh) / d , b * c.I, b * c.J, b * c.K);
	}
	#endregion

	#region ArcHyperbolics
	public static Quaternion Acosh(Quaternion q) => INumber<Quaternion>.IAcosh(q);
	public static Quaternion Asinh(Quaternion q) => INumber<Quaternion>.IAsinh(q);
	public static Quaternion Atanh(Quaternion q) => INumber<Quaternion>.IAtanh(q);
	public static Quaternion Acoth(Quaternion q) => INumber<Quaternion>.IAcoth(q);
	#endregion

	#region ArcTrigonometrics
	public static Quaternion Acos(Quaternion q) => INumber<Quaternion>.IAcos(q);
	public static Quaternion Asin(Quaternion q) => INumber<Quaternion>.IAsin(q);
	public static Quaternion Atan(Quaternion q) => INumber<Quaternion>.IAtan(q);
	public static Quaternion Acot(Quaternion q) => INumber<Quaternion>.IAcot(q);
	#endregion

	#region Exotic Trigonometrics
	// -i*((-1)^c - (-1)^(-c)) = 2sin(πc)
	public static Quaternion SinN1(Quaternion q) => 2 * Sin(Math.PI * q);
	// -i * ((i)^c - (i)^(-c)) = 2sin(πc/2)
	public static Quaternion NISinI(Quaternion q) => 2 * Sin(INumber<Quaternion>.qTau * q);
	#endregion
}
/* this one was originally used for zeta reflection, but it combined intself with SinN1 into NISinI
// i^c + i^(-c) = 2cos(πc/2) // is this faster than 2*T.Cos(qTau * c)? T.Cos(c) = new(Math.Cos(c.R) * Math.Cosh(c.I), Math.Sin(-c.R) * Math.Sinh(c.I));
private static T CosI(T c) {
	double i = c.I * qTau, r = c.R * qTau, cos = Math.Cos(r), sin = Math.Sin(r), e = Math.Exp(i), ie = 1 / e;
	return new T((ie - e) * cos, (ie + e) * sin);
}*/