using static Comparser.Comparser.Numbers.Static;
namespace Comparser.Comparser.Numbers;
public readonly struct Quaternion(double r = 0, double i = 0, double j = 0, double k = 0) : INumber<Quaternion> {

	public readonly double R = r, I = i, J = j, K = k;

	#region Quaternion Constants
	public static Quaternion i => new(0, 1);
	public static Quaternion j => new(0, 0, 1);
	public static Quaternion k => new(0, 0, 0, 1);
	public static Quaternion ni => new(0, -1);
	public static Quaternion nj => new(0, 0, -1);
	public static Quaternion nk => new(0, 0, 0, -1);
	#endregion

	#region Query
	public bool Is0() => R == 0 && I == 0 && J == 0 && K == 0;
	public bool IsNaN() => double.IsNaN(R) || double.IsNaN(I);

	public override string ToString() => ToString(-1);
	public string ToString(int d) => ValueToString("ijk", [R, I, J, K], d);
	#endregion

	#region Constants
	public static Quaternion Zero() => new(0);
	public static Quaternion One() => new(1, 1, 1, 1);
	public static Quaternion NaN() => new(double.NaN, double.NaN, double.NaN, double.NaN);
	#endregion

	#region Helpers
	public static double Mix(Quaternion c, Func<double, double, double> d) => d(d(c.R, c.I),d(c.J,c.K));
	private static double I_Dot(Quaternion q) => q.I * q.I + q.J * q.J + q.K * q.K;
	public static Quaternion D1(Quaternion a, Func<double, double> d) => new(d(a.R), d(a.I), d(a.J), d(a.K));
	public static Quaternion D2(Quaternion a, Quaternion b, Func<double, double, double> d) => new(d(a.R, b.R), d(a.I, b.I), d(a.J, b.J), d(a.K, b.K));
	public static Quaternion D3(Quaternion a, Quaternion b, Quaternion c, Func<double, double, double, double> d) => new(d(a.R, b.R, c.R), d(a.I, b.I, c.I), d(a.J, b.J, c.J), d(a.K, b.K, c.K));
	#endregion

	#region Basics
	public static bool AreEqual(Quaternion a, Quaternion b)
		=> Math.Abs(a.R - b.R) + Math.Abs(a.I - b.I) + Math.Abs(a.J - b.J) + Math.Abs(a.K - b.K) < 1e-8;
	public static double Re(Quaternion q) => q.R;
	public static Quaternion MakeR(double r) => new(r);
	public static double Im(Quaternion q) => q.I + q.J + q.K;
	public static double ImMag(Quaternion q) => Math.Sqrt(I_Dot(q));
	// conjugate: a - bi
	public static Quaternion operator !(Quaternion q) => new(q.R, -q.I, -q.J, -q.K);
	// negative: - a - bi
	public static Quaternion operator -(Quaternion q) => new(-q.R, -q.I, -q.J, -q.K);
	// u * quaternion: (0, q.I, q.J, q.K) * q;
	public static Quaternion operator ~(Quaternion q) => new(-q.I - q.J - q.K, q.R + q.K - q.J, q.R + q.I - q.K, q.J - q.I + q.R);
	// u = i+j+k
	public static Quaternion U(Quaternion q) => new(0, q.I, q.J, q.K);
	// u * quaternion
	public static Quaternion MulU(Quaternion q) => new(-q.I - q.J - q.K, q.R + q.K - q.J, q.R + q.I - q.K, q.J - q.I + q.R);
	// -u * quaternion
	public static Quaternion NegU(Quaternion q) => new(q.I + q.J + q.K, q.J - q.R - q.K, q.K - q.R - q.I, q.I - q.J - q.R);
	// i * quaternion
	public static Quaternion MulI(Quaternion q) => new(-q.I, q.R, -q.K, q.J);
	// j * quaternion
	public static Quaternion MulJ(Quaternion q) => new(-q.J, q.K, q.R, -q.I);
	// k * quaternion
	public static Quaternion MulK(Quaternion q) => new(-q.K, -q.J, q.I, q.R);
	// -i * quaternion
	public static Quaternion NegI(Quaternion q) => new(q.I, -q.R, q.K, -q.J);
	// -j * quaternion
	public static Quaternion NegJ(Quaternion q) => new(q.J, -q.K, -q.R, q.I);
	// -k * quaternion
	public static Quaternion NegK(Quaternion q) => new(q.K, q.J, -q.I, -q.R);
	// |quaternion|^2
	public static double operator +(Quaternion q) => q.R * q.R + I_Dot(q);
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
	public static double Arg(Quaternion q) => Math.Atan2(ImMag(q), q.R);
	// from angle
	public static Quaternion InvArg(double angle, Quaternion axis) { var s = Math.Sin(angle); return new(Math.Cos(angle), axis.I * s, axis.J * s, axis.K * s); }
	public static Quaternion Axis(Quaternion q) {
		var im = ImMag(q);
		return im == 0 ? i : new(0, q.I / im, q.J / im, q.K / im);
	}
	// square root
	public static Quaternion Sqrt(Quaternion q) {
		double qr = q.R, idot = I_Dot(q), a = Math.Sqrt(.5 * (Math.Sqrt(qr * qr + idot) + qr));
		return a == 0 ? new(0, Math.Sqrt(-qr)) : new(a, q.I / (a *= 2), q.J / a, q.K / a);
		//return q ^ .5; // is there some similar for like below?
		//var a = INumber<Quaternion>.Abs(c);
		//return new(Math.Sqrt(.5 * (a + c.R)), Math.CopySign(Math.Sqrt(.5 * (a - c.R)), c.I));
	}
	// quaternion^2
	public static Quaternion Sqr(Quaternion q) { 
		double qr = q.R, idot = I_Dot(q), a = 2 * qr;
		return new(qr * qr - idot, a * q.I, a * q.J, a * q.K);
	}
	// quaternion^3
	public static Quaternion Cub(Quaternion q) {
		double qr = q.R, idot = I_Dot(q), r2 = qr * qr, v = 3 * r2 - idot;
		return new(qr * (r2 - 3 * idot), v * q.I, v * q.J, v * q.K);
		//double I = IDot(q), a = 2 * q.R, aa = q.R * a, AI = q.R * q.R - I;
		//return new(a * AI - a * I, q.I * (AI += aa), q.J * AI, q.K * AI);
	}
	// quaternion^4
	public static Quaternion Quart(Quaternion q) {
		double qr = q.R, idot = I_Dot(q), r2 = qr * qr, v = 4 * qr * (r2 - idot);
		return new(r2 * r2 - 6 * r2 * idot + idot * idot, v * q.I, v * q.J, v * q.K);
	}
	// |a| + |b|i
	public static Quaternion AbsComp(Quaternion q) => D1(q, Math.Abs);
	public static double Dot(Quaternion a, Quaternion b) => a.R * b.R + a.I * b.I + a.J * b.J + a.K * b.K;
	public static Quaternion Min(Quaternion a, Quaternion b) => D2(a, b, Static.Min);
	public static Quaternion Max(Quaternion a, Quaternion b) => D2(a, b, Static.Max);
	public static Quaternion Clamp(Quaternion q, Quaternion min, Quaternion max) => D3(q, min, max, Static.Clamp);
	#endregion

	#region Additions
	public static Quaternion operator ++(Quaternion c) => c + 1;
	public static Quaternion operator +(Quaternion a, Quaternion b) => D2(a, b, Static.Add);
	// quaternion + real
	public static Quaternion operator +(Quaternion q, double r) => new(q.R + r, q.I, q.J, q.K);
	// real + quaternion
	public static Quaternion operator +(double r, Quaternion q) => new(q.R + r, q.I, q.J, q.K);
	// quaternion + imaginary
	public static Quaternion AddV(Quaternion q, double v) { var nv = Math.Sqrt(3 * v * v); return new(q.R, q.I + nv, q.J + nv, q.K + nv); }
	// quaternion + imaginary
	public static Quaternion AddV(double v, Quaternion q) { var nv = Math.Sqrt(3 * v * v); return new(q.R, q.I + nv, q.J + nv, q.K + nv); }
	// quaternion + imaginary
	public static Quaternion AddI(Quaternion q, double v) => new(q.R, q.I + v, q.J, q.K);
	// quaternion + imaginary
	public static Quaternion AddJ(Quaternion q, double v) => new(q.R, q.I, q.J + v, q.K);
	// quaternion + imaginary
	public static Quaternion AddK(Quaternion q, double v) => new(q.R, q.I, q.J, q.K + v);
	// imaginary + quaternion
	#endregion

	#region Subtractions
	public static Quaternion operator --(Quaternion c) => c - 1;
	public static Quaternion operator -(Quaternion a, Quaternion b) => D2(a, b, Static.Sub);
	// quaternion - real
	public static Quaternion operator -(Quaternion q, double r) => new(q.R - r, q.I, q.J, q.K);
	// real - quaternion
	public static Quaternion operator -(double r, Quaternion q) => new(r - q.R, -q.I, -q.J, -q.K);
	// quaternion + imaginary
	public static Quaternion SubV(Quaternion q, double v) { var nv = Math.Sqrt(3 * v * v); return new(q.R, q.I - nv, q.J - nv, q.K - nv); }
	// quaternion + imaginary
	public static Quaternion SubV(double v, Quaternion q) { var nv = Math.Sqrt(3 * v * v); return new(-q.R, nv - q.I, nv - q.J, nv - q.K); }
	// quaternion - imaginary
	public static Quaternion SubI(Quaternion q, double v) => new(q.R, q.I - v, q.K, q.K);
	// imaginary - quaternion
	public static Quaternion SubI(double v, Quaternion q) => new(-q.R, v - q.I, q.J, q.K);
	// quaternion - imaginary
	public static Quaternion SubJ(Quaternion q, double v) => new(q.R, q.I, q.J - v, q.K);
	// imaginary - quaternion
	public static Quaternion SubJ(double v, Quaternion q) => new(-q.R, -q.I, v - q.J, -q.K);
	// quaternion - imaginary
	public static Quaternion SubK(Quaternion q, double v) => new(q.R, q.I, q.K, q.K - v);
	// imaginary - quaternion
	public static Quaternion SubK(double v, Quaternion q) => new(-q.R, v - q.I, -q.J, -q.K);
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
	public static Quaternion MulI(Quaternion q, double v) => new(-v * q.I, v * q.R, v * q.K, -v * q.J);
	// imaginary * quaternion
	public static Quaternion MulI(double v, Quaternion q) => new(-v * q.I, v * q.R, -v * q.K, v * q.J);
	// quaternion * imaginary
	public static Quaternion MulJ(Quaternion q, double v) => new(-v * q.J, -v * q.K, v * q.R, v * q.I);
	// imaginary * quaternion
	public static Quaternion MulJ(double v, Quaternion q) => new(-v * q.J, v * q.K, v * q.R, -v * q.I);
	// quaternion * imaginary
	public static Quaternion MulK(Quaternion q, double v) => new(-v * q.K, v * q.J, -v * q.I, v * q.R);
	// imaginary * quaternion
	public static Quaternion MulK(double v, Quaternion q) => new(-v * q.K, -v * q.J, v * q.I, v * q.R);
	public static double operator |(Quaternion a, Quaternion b) => a.R * b.R + a.I * b.I + a.J * b.J + a.K * b.K;
	#endregion

	#region Divisions
	public static Quaternion operator /(Quaternion a, Quaternion b) => a * Inv(b);
	// quaternion / real
	public static Quaternion operator /(Quaternion q, double r) => D1(q, (x) => x / r);
	// real / quaternion
	public static Quaternion operator /(double r, Quaternion q) => r * Inv(q);
	public static Quaternion LDiv(Quaternion a, Quaternion b) => Inv(b) * a;

	// quaternion / imaginary (right division)
	public static Quaternion DivI(Quaternion q, double v) => new(q.I / v, q.R / -v, q.K / -v, q.J / v);
	// quaternion / imaginary (left division)
	public static Quaternion LDivI(Quaternion q, double v) => new(q.I / v, q.R / -v, q.K / v, q.J / -v);
	// imaginary / quaternion (right division)
	public static Quaternion DivI(double v, Quaternion q) => MulI(v, Inv(q));
	// imaginary / quaternion (left division)
	public static Quaternion LDivI(double v, Quaternion q) => MulI(Inv(q), v);
	// TODO DivJ, DivK
	public static Quaternion operator %(Quaternion a, Quaternion b) => INumber<Quaternion>.NewMod(a, b);
	#endregion

	#region ExpLogs
	// Ln(quaternion)
	public static Quaternion Log(Quaternion q) {
		var idot = I_Dot(q);
		return idot > 0 ? new(.5 * Math.Log(q.R * q.R + idot), (idot = Math.Atan2(idot = Math.Sqrt(idot), q.R) / idot) * q.I, idot * q.J, idot * q.K)
			: q.R < 0 ? new(Math.Log(-q.R), Math.PI) : new(Math.Log(q.R));
	}
	// Ln(quaternion)/2
	public static Quaternion LogH(Quaternion q) {
		var idot = I_Dot(q);
		return idot < 0 ? new(.25 * Math.Log(q.R * q.R + idot), (idot = Math.Atan2(idot = Math.Sqrt(idot), q.R) / (2 * idot)) * q.I, idot * q.J, idot * q.K)
			: q.R < 0 ? new(.5 * Math.Log(-q.R), QTau) : new(.5 * Math.Log(q.R));
	}
	// e ^ quaternion
	public static Quaternion Exp(Quaternion q) {
		double e = Math.Exp(q.R), v = ImMag(q); // v=sqrt(idot(q))
		return v == 0 ? new Quaternion(e) : new Quaternion(e * Math.Cos(v), (e *= Math.Sin(v) / v) * q.I, e * q.J, e * q.K);
	}
	public static Quaternion operator ^(Quaternion a, Quaternion b) => Exp(Log(a) * b);
	// quaternion ^ real
	public static Quaternion operator ^(Quaternion c, double r) => Exp(Log(c) * r);
	// real ^ quaternion
	public static Quaternion operator ^(double r, Quaternion c) => 0 <= r ? Exp(Math.Log(r) * c) : Exp(new Quaternion(Math.Log(-r), Math.PI) * c);
	// (-1) ^ quaternion
	public static Quaternion PowN1(Quaternion c) => Exp(new(-c.I * Math.PI, c.R * Math.PI));
	// i ^ quaternion
	public static Quaternion PowI(Quaternion c) => Exp(new(-c.I * QTau, c.R * QTau));
	#endregion

	#region Hyperbolics
	public static Quaternion Cosh(Quaternion q) {
		var v = ImMag(q);
		return new Quaternion(Math.Cos(v) * Math.Cosh(q.R), (v = Math.Sinh(q.R) * (v == 0 ? 0 : Math.Sin(v) / v)) * q.I, v * q.J, v * q.K);
	}
	public static Quaternion Sinh(Quaternion q) {
		var v = ImMag(q);
		return new Quaternion(Math.Cos(v) * Math.Sinh(q.R), (v = Math.Cosh(q.R) * (v == 0 ? 0 : Math.Sin(v) / v)) * q.I, v * q.J, v * q.K);
	}
	public static Quaternion Tanh(Quaternion c) {
		double im = ImMag(c), t = Math.Tan(c.R), h = Math.Tanh(im), tt = t * t, hh = h * h, d = 1 + tt * hh, 
			b = im == 0 ? 0 : t * (1 - hh) / (im * d);
		return new Quaternion(h * (1 + tt) / d, b * c.I, b * c.J, b * c.K);
	}
	public static Quaternion Coth(Quaternion c) {
		double im = ImMag(c), t = Math.Tan(c.R), h = Math.Tanh(im), tt = t * t, hh = h * h, d = tt + hh, 
			b = im == 0 ? 0 : t * (hh - 1) / (im * d);
		return new Quaternion(h * (tt + 1) / d, b * c.I, b * c.J, b * c.K);
	}
	#endregion

	#region Trigonometrics
	public static Quaternion Cos(Quaternion q) {
		var v = ImMag(q);
		return new Quaternion(Math.Cos(q.R) * Math.Cosh(v), (v = -Math.Sin(q.R) * (v == 0 ? 0 : Math.Sinh(v) / v)) * q.I, v * q.J, v * q.K);
	}
	public static Quaternion Sin(Quaternion q) {
		var v = ImMag(q);
		return new Quaternion(Math.Sin(q.R) * Math.Cosh(v), (v = Math.Cos(q.R) * (v == 0 ? 0 : Math.Sinh(v) / v)) * q.I, v * q.J, v * q.K);
	}
	public static Quaternion Tan(Quaternion c) {
		double im = ImMag(c), t = Math.Tan(c.R), h = Math.Tanh(im), tt = t * t, hh = h * h, d = 1 + tt * hh, 
			b = im == 0 ? (1 + tt) / d : h * (1 + tt) / (im * d);
		return new Quaternion(t * (1 - hh) / d, b * c.I, b * c.J, b * c.K);
	}
	public static Quaternion Cot(Quaternion c) {
		double im = ImMag(c), t = Math.Tan(c.R), h = Math.Tanh(im), tt = t * t, hh = h * h, d = tt + hh, 
			b = im == 0 ? (1 + tt) / -d : -h * (1 + tt) / (im * d);
		return new Quaternion(t * (1 - hh) / d , b * c.I, b * c.J, b * c.K);
	}
	#endregion

	#region ArcHyperbolics
	public static Quaternion Acosh(Quaternion q) => INumber<Quaternion>.I_Acosh(q);
	public static Quaternion Asinh(Quaternion q) => INumber<Quaternion>.I_Asinh(q);
	public static Quaternion Atanh(Quaternion q) => INumber<Quaternion>.I_Atanh(q);
	public static Quaternion Acoth(Quaternion q) => INumber<Quaternion>.I_Acoth(q);
	#endregion

	#region ArcTrigonometrics
	public static Quaternion Acos(Quaternion q) => INumber<Quaternion>.I_Acos(q);
	public static Quaternion Asin(Quaternion q) => INumber<Quaternion>.I_Asin(q);
	public static Quaternion Atan(Quaternion q) => INumber<Quaternion>.I_Atan(q);
	public static Quaternion Acot(Quaternion q) => INumber<Quaternion>.I_Acot(q);
	#endregion

	#region Exotic Trigonometrics
	// -i*((-1)^c - (-1)^(-c)) = 2sin(πc)
	public static Quaternion Sin_P(Quaternion q) => 2 * Sin(Math.PI * q);
	// -i * ((i)^c - (i)^(-c)) = 2sin(πc/2)
	public static Quaternion Sin_2Q(Quaternion q) => 2 * Sin(QTau * q);
	#endregion

	#region Special Functions
	public static Quaternion Gauss(Quaternion q) { // optimized
		double r = q.R, v = ImMag(q), e = Math.Exp(v * v - r * r), a = -2 * r * v, s = v == 0 ? 0 : e * Math.Sin(a) / v;
		return new(e * Math.Cos(a), s * q.I, s * q.J, s * q.K);
	}
	public static Quaternion Gamma(Quaternion q) => INumber<Quaternion>.ComplexOp(q, INumber<Complex>.I_Gamma); // = INumber<Quaternion>.IGamma(q);
	public static Quaternion Factorial(Quaternion q) => INumber<Quaternion>.ComplexOp(q, INumber<Complex>.I_Factorial); // = INumber<Quaternion>.IFactorial(q);
	public static Quaternion Zeta(Quaternion q) => INumber<Quaternion>.ComplexOp(q, INumber<Complex>.I_Zeta); // = INumber<Quaternion>.IZeta(q);
	#endregion
	public static void IndexAndAddToRgb(Color[] axis, Quaternion indices, Quaternion value) {
		var a = axis[(int)indices.R];
		if (indices.R >= 0 && indices.R < axis.Length)
			axis[(int)indices.R] = Color.FromArgb(a.R + (int)value.R, a.G+ (int)value.R, a.B+ (int)value.R);
		if (indices.I >= 0 && indices.I < axis.Length)
			axis[(int)indices.I] = Color.FromArgb(a.R+(int)value.I, a.G, a.B);
		if (indices.J >= 0 && indices.I < axis.Length)
			axis[(int)indices.I] = Color.FromArgb(a.R, a.G+(int)value.J, a.B);
		if (indices.K >= 0 && indices.I < axis.Length)
			axis[(int)indices.I] = Color.FromArgb(a.R, a.G, a.B + (int)value.K);
	}
}
