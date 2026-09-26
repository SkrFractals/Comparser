using System.Diagnostics.CodeAnalysis;
using static Comparser.Comparser.Numbers.Static;
namespace Comparser.Comparser.Numbers;
public readonly struct Quaternion(double r = 0, double i = 0, double j = 0, double k = 0) : ILeaf, INumber<Quaternion> {

	public readonly double R = r, I = i, J = j, K = k, Mi = 1, Mj, Mk;

	private Quaternion(Quaternion m, Quaternion n) : this(n.R,n.I,n.J,n.K) 
		//=> (Mi, Mj, Mk) = n is { I: 0, J: 0, K: 0 } ? (Nz(n.I) ? -m.Mi : m.Mi, Nz(n.J) ? -m.Mj : m.Mj, Nz(n.K) ? -m.Mk : m.Mk) : (n.I, n.J, n.K);
		=> (Mi, Mj, Mk) = n is { I: 0, J: 0, K: 0 } ? (m.Mi, m.Mj, m.Mk) : (n.I, n.J, n.K);
	private Quaternion(double mi, double mj, double mk, double r, double i, double j = 0, double k = 0) : this(r, i, j, k) 
		//=> (Mi, Mj, Mk) = i == 0 && j == 0 && k == 0 ? (Nz(i) ? -mi : mi, Nz(j) ? -mj : mj, Nz(k) ? -mk : mk) : (i, j, k);
			=> (Mi, Mj, Mk) = i == 0 && j == 0 && k == 0 ? (mi, mj, mk) : (i, j, k);
	private Quaternion(Quaternion m, double r = 0, double i = 0, double j = 0, double k = 0) : this(r, i, j, k) 
		//=> (Mi, Mj, Mk) = i == 0 && j == 0 && k == 0 ? (Nz(i) ? -m.Mi : m.Mi, Nz(j) ? -m.Mj : m.Mj, Nz(k) ? -m.Mk : m.Mk) : (i, j, k);
			=> (Mi, Mj, Mk) = i == 0 && j == 0 && k == 0 ? (m.Mi, m.Mj, m.Mk) : (i, j, k);
	private Quaternion(double mi, double mj, double mk, Quaternion n) : this(n.R,n.I,n.J,n.K) 
		//=> (Mi, Mj, Mk) = n is { I: 0, J: 0, K: 0 } ? (Nz(n.I) ? -mi : mi, Nz(n.J) ? -mj : mj, Nz(n.K) ? -mk : mk)  : (n.I, n.J, n.K);
		=> (Mi, Mj, Mk) = n is { I: 0, J: 0, K: 0 } ? (mi, mj, mk)  : (n.I, n.J, n.K);
	//private static bool Nz(double x) => x == 0.0 && double.IsNegativeInfinity(1.0 / x);
	
	
	#region ILeaf
	public NumericKind kind => NumericKind.Quaternion;
	public ILeaf/*<T>*/ Cast(NumericKind to) => to switch {
		NumericKind.Real => (Real)/*<T>*/R,
		NumericKind.Complex => new Complex(R,I),
		NumericKind.Quaternion => this,
		_ => nan
	};
	#endregion
	
	#region Quaternion Constants
	public static Quaternion i => new(0, 1);
	public static Quaternion j => new(0, 0, 1);
	public static Quaternion k => new(0, 0, 0, 1);
	public static Quaternion ni => new(0, -1);
	public static Quaternion nj => new(0, 0, -1);
	public static Quaternion nk => new(0, 0, 0, -1);
	#endregion
	
	#region Cast
	public static implicit operator double(Quaternion/*<T>*/ d) => d.R;
	//public static implicit operator double(Complex/*<T>*/ d) => IScalar<double>.ToDouble(d.R);
	public static explicit operator Quaternion/*<T>*/(double b) => new(b);
	public static explicit operator Quaternion/*<T>*/(Real b) => new(b.R);
	public static explicit operator Quaternion/*<T>*/(Complex b) => new(b.R);
	#endregion

	#region Query
	public double Re() => R;
	public static bool operator ==(Quaternion /*<T>*/ a, Quaternion /*<T>*/ b) 
		=> Math.Abs(a.R - b.R) <= Math.Max(Math.Abs(a.R), Math.Abs(b.R)) * 1e-6 
			&& Math.Abs(a.I - b.I) <= Math.Max(Math.Abs(a.I), Math.Abs(b.I)) * 1e-6
			&& Math.Abs(a.J - b.J) <= Math.Max(Math.Abs(a.J), Math.Abs(b.K)) * 1e-6
			&& Math.Abs(a.K - b.K) <= Math.Max(Math.Abs(a.K), Math.Abs(b.K)) * 1e-6;//a.R == b.R && a.I == b.I;
	public static bool operator !=(Quaternion/*<T>*/ a, Quaternion/*<T>*/ b) => Math.Abs(a.R - b.R) > Math.Max(Math.Abs(a.R), Math.Abs(b.R)) * 1e-6 
		|| Math.Abs(a.I - b.I) > Math.Max(Math.Abs(a.I), Math.Abs(b.I)) * 1e-6
		|| Math.Abs(a.J - b.J) > Math.Max(Math.Abs(a.J), Math.Abs(b.K)) * 1e-6
		|| Math.Abs(a.K - b.K) > Math.Max(Math.Abs(a.K), Math.Abs(b.K)) * 1e-6;//a.R != b.R || a.I != b.I;
	public bool IsFalse() => INumber<Quaternion>.IsFalse(this);
	public bool IsTrue() => INumber<Quaternion>.IsTrue(this);
	public static bool Is0(Quaternion q) => q is { R: 0, I: 0, J: 0, K: 0 };
	public bool Is0() => this is { R: 0, I: 0, J: 0, K: 0 };
	public bool IsNaN() => IsNaN(this);
	public static bool IsNaN(Quaternion q) => double.IsNaN(q.R) || double.IsNaN(q.I)|| double.IsNaN(q.J)|| double.IsNaN(q.K);
	public static string[] epsUnit => ["i", "j", "k", "ε", "εi", "εj", "εk"];
	public static double[] EpsValues(Quaternion r, Quaternion e) => [r.R, r.I, r.J, r.K, e.R, e.I, e.J, e.K];
	public override string ToString() => ToString(-1);
	private static readonly string[] Units = ["i", "j", "k"];
    public override bool Equals([NotNullWhen(true)] object? obj) => GetHashCode() == obj?.GetHashCode();//=> obj is ILeaf l && ILeaf.LeafEquals(this, l);
    public override int GetHashCode() => HashCode.Combine(R, I, J, K);
    public string ToString(int d) => ValueToString(Units, [R, I, J, K], d);
	#endregion

	#region Constants
	public static Quaternion zero => default;
	public static Quaternion nan => new(double.NaN, double.NaN, double.NaN, double.NaN);
	public static Quaternion unit => new(1);
	public static Quaternion one => new(1, 1, 1, 1);
	public static Quaternion u => new(0, 1, 1, 1);
	public static Quaternion minusUnit => new(-1);
	public static Quaternion minusOne => new(-1, -1, -1, -1);
	public static Quaternion minusU => new(0, -1, -1, -1);
	#endregion

	#region Helpers
	public static double Mix(Quaternion c, Func<double, double, double> d) => d(d(c.R, c.I),d(c.J,c.K));
	private static double I_Dot(Quaternion q) => q.I * q.I + q.J * q.J + q.K * q.K;
	public static Quaternion D1(Quaternion a, Func<double, double> d) => new(a.I, a.J, a.K, d(a.R), d(a.I), d(a.J), d(a.K));
	public static Quaternion D2(Quaternion a, Quaternion b, Func<double, double, double> d) 
		=> new(new(a,b), d(a.R, b.R), d(a.I, b.I), d(a.J, b.J), d(a.K, b.K));
	public static Quaternion D3(Quaternion a, Quaternion b, Quaternion c, Func<double, double, double, double> d) => new(a.I, a.J, a.K, d(a.R, b.R, c.R), d(a.I, b.I, c.I), d(a.J, b.J, c.J), d(a.K, b.K, c.K));
	#endregion

	#region Basics
	public static bool AreEqual(Quaternion a, Quaternion b)
		=> Math.Abs(a.R - b.R) + Math.Abs(a.I - b.I) + Math.Abs(a.J - b.J) + Math.Abs(a.K - b.K) < 1e-8;
	public static double Re(Quaternion q) => q.R;
	public static Quaternion MakeR(double r) => new(r);
	public static double Im(Quaternion q) => q.I + q.J + q.K;
	public double Im() => Im(this);
	public static double ImMag(Quaternion q) => Math.Sqrt(I_Dot(q));
	public static double ImMagM(Quaternion q) => Math.Sqrt(q.Mi * q.Mi + q.Mj * q.Mj + q.Mk * q.Mk);
	// conjugate: a - bi
	public static Quaternion operator ~(Quaternion q) => new(q.Mi, q.Mj, q.Mk, q.R, -q.I, -q.J, -q.K);
	// negative: - a - bi
	public static Quaternion operator -(Quaternion q) 
		=> new(-q.Mi, -q.Mj, -q.Mk, -q.R, -q.I, -q.J, -q.K);
	// u * quaternion: (0, q.I, q.J, q.K) * q;
	public static Quaternion operator !(Quaternion q) => new(q.Mi, q.Mj, q.Mk, -q.I - q.J - q.K, q.R + q.K - q.J, q.R + q.I - q.K, q.J - q.I + q.R);
	// u = i+j+k
	public static Quaternion U(Quaternion q) => new(q.Mi, q.Mj, q.Mk, 0, q.I, q.J, q.K);
	// u * quaternion
	public static Quaternion MulU(Quaternion q) => new(q.Mi, q.Mj, q.Mk, -q.I - q.J - q.K, q.R + q.K - q.J, q.R + q.I - q.K, q.J - q.I + q.R);
	// -u * quaternion
	public static Quaternion NegU(Quaternion q) => new(q.Mi, q.Mj, q.Mk, q.I + q.J + q.K, q.J - q.R - q.K, q.K - q.R - q.I, q.I - q.J - q.R);
	// i * quaternion
	public static Quaternion MulI(Quaternion q) => new(q.Mi, q.Mj, q.Mk, -q.I, q.R, -q.K, q.J);
	// j * quaternion
	public static Quaternion MulJ(Quaternion q) => new(q.Mi, q.Mj, q.Mk, -q.J, q.K, q.R, -q.I);
	// k * quaternion
	public static Quaternion MulK(Quaternion q) => new(q.Mi, q.Mj, q.Mk, -q.K, -q.J, q.I, q.R);
	// -i * quaternion
	public static Quaternion NegI(Quaternion q) => new(q.Mi, q.Mj, q.Mk, q.I, -q.R, q.K, -q.J);
	// -j * quaternion
	public static Quaternion NegJ(Quaternion q) => new(q.Mi, q.Mj, q.Mk, q.J, -q.K, -q.R, q.I);
	// -k * quaternion
	public static Quaternion NegK(Quaternion q) => new(q.Mi, q.Mj, q.Mk, q.K, q.J, -q.I, -q.R);
	// |quaternion|^2
	public static double operator +(Quaternion q) => q.R * q.R + I_Dot(q);
	// signed fractional part
	public static Quaternion Frac(Quaternion q) => D1(q, (r) => r - Math.Truncate(r));
	// truncate
	public static Quaternion Truncate(Quaternion q) => D1(q, Math.Truncate);
	// round down
	public static Quaternion Floor(Quaternion q) => D1(q, Math.Floor);
	// round
	public static Quaternion Round(Quaternion q) => D1(q, Math.Round);
	// round up
	public static Quaternion Ceiling(Quaternion q) => D1(q, Math.Ceiling);
	public static Quaternion Cycle(Quaternion q) => D1(q, Static.Cycle);
	// 1 / quaternion
	public static Quaternion Inv(Quaternion q) => INumber<Quaternion>.I_Inv(q);
	// = iτ/c // using this in my Gamma_Stirling_Negative, maybe won't work for quaternions as it's only using i
	//public static Quaternion InvITau(Quaternion c) => new Quaternion(c.I, c.R) * (Math.Tau / +c);
	// Argument of quaternion
	public static double Arg(Quaternion q) => Math.Atan2(ImMag(q), q.R);
	// from angle
	public static Quaternion InvArg(double angle, Quaternion axis) {
		var s = Math.Sin(angle); 
		return new(Math.Cos(angle), axis.I * s, axis.J * s, axis.K * s);
	}
	public static Quaternion Axis(Quaternion q) {
		var im = ImMag(q);
		return im == 0 ? i : new(0, q.I / im, q.J / im, q.K / im);
	}
	private static Quaternion AxisM(Quaternion q) {
		var im = ImMagM(q);
		return im == 0 ? i : new(0, q.Mi / im, q.Mj / im, q.Mk / im);
	}
	//private static Quaternion Memory(Quaternion q, )
	// square root
	public static Quaternion Sqrt(Quaternion q) {
		double qr = q.R, iDot = I_Dot(q), a = Math.Sqrt(.5 * (Math.Sqrt(qr * qr + iDot) + qr));
		//Quaternion m;
		return a == 0 ? //(m = AxisM(q)) == 0 ? new (q.Mi, q.Mj, q.Mk, 0, Math.Sqrt(-qr)) : // no memory - principal i-axis 
			new(q.Mi, q.Mj, q.Mk, Math.Sqrt(-qr) * AxisM(q)) // use memory axis
			: new(q.Mi, q.Mj, q.Mk,a, q.I / (a *= 2), q.J / a, q.K / a); // normal result
	}
	// quaternion^2
	public static Quaternion Sqr(Quaternion q) { 
		double qr = q.R, iDot = I_Dot(q), a = 2 * qr;
		return new(q.Mi, q.Mj, q.Mk, qr * qr - iDot, a * q.I, a * q.J, a * q.K);
	}
	// quaternion^3
	public static Quaternion Cub(Quaternion q) {
		double qr = q.R, iDot = I_Dot(q), r2 = qr * qr, v = 3 * r2 - iDot;
		return new(q.Mi, q.Mj, q.Mk, qr * (r2 - 3 * iDot), v * q.I, v * q.J, v * q.K);
	}
	// quaternion^4
	public static Quaternion Quart(Quaternion q) {
		double qr = q.R, iDot = I_Dot(q), r2 = qr * qr, v = 4 * qr * (r2 - iDot);
		return new(q.Mi, q.Mj, q.Mk, r2 * r2 - 6 * r2 * iDot + iDot * iDot, v * q.I, v * q.J, v * q.K);
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
	public static Quaternion operator +(Quaternion a, Quaternion b) => D2(a, b, Add);
	// quaternion + real
	public static Quaternion operator +(Quaternion q, double r) => new(q.Mi, q.Mj, q.Mk, q.R + r, q.I, q.J, q.K);
	// real + quaternion
	public static Quaternion operator +(double r, Quaternion q) => new(q.Mi, q.Mj, q.Mk, q.R + r, q.I, q.J, q.K);
	// quaternion + imaginary
	public static Quaternion AddV(Quaternion q, double v) {
		var nv = Math.Sqrt(3 * v * v); 
		return new(q.Mi, q.Mj, q.Mk, q.R, q.I + nv, q.J + nv, q.K + nv);
	}
	// quaternion + imaginary
	public static Quaternion AddV(double v, Quaternion q) {
		var nv = Math.Sqrt(3 * v * v); 
		return new(q.Mi, q.Mj, q.Mk, q.R, q.I + nv, q.J + nv, q.K + nv);
	}
	// quaternion + imaginary
	public static Quaternion AddI(Quaternion q, double v) => new(q.Mi, q.Mj, q.Mk,q.R, q.I + v, q.J, q.K);
	// quaternion + imaginary
	public static Quaternion AddJ(Quaternion q, double v) => new(q.Mi, q.Mj, q.Mk,q.R, q.I, q.J + v, q.K);
	// quaternion + imaginary
	public static Quaternion AddK(Quaternion q, double v) => new(q.Mi, q.Mj, q.Mk,q.R, q.I, q.J, q.K + v);
	// imaginary + quaternion
	#endregion

	#region Subtractions
	public static Quaternion operator --(Quaternion c) => c - 1;
	public static Quaternion operator -(Quaternion a, Quaternion b) => D2(a, b, Sub);
	// quaternion - real
	public static Quaternion operator -(Quaternion q, double r) => new(q.Mi, q.Mj, q.Mk,q.R - r, q.I, q.J, q.K);
	// real - quaternion
	public static Quaternion operator -(double r, Quaternion q) 
		//=> new(/*-q.Mi, -q.Mj, -q.Mk*/q.Mi, q.Mj, q.Mk,r - q.R, -q.I, -q.J, -q.K);
		=> new(-q.Mi, -q.Mj, -q.Mk/*q.Mi, q.Mj, q.Mk*/,r - q.R, -q.I, -q.J, -q.K);
	// quaternion + imaginary
	public static Quaternion SubV(Quaternion q, double v) {
		var nv = Math.Sqrt(3 * v * v); 
		return new(q.Mi, q.Mj, q.Mk,q.R, q.I - nv, q.J - nv, q.K - nv);
	}
	// quaternion + imaginary
	public static Quaternion SubV(double v, Quaternion q) {
		var nv = Math.Sqrt(3 * v * v);
		//return new(q.Mi, q.Mj, q.Mk, -q.R, nv - q.I, nv - q.J, nv - q.K);
		return new(-q.Mi, -q.Mj, -q.Mk, -q.R, nv - q.I, nv - q.J, nv - q.K);
	}
	// quaternion - imaginary
	public static Quaternion SubI(Quaternion q, double v) => new(q.Mi, q.Mj, q.Mk, q.R, q.I - v, q.K, q.K);
	// imaginary - quaternion
	public static Quaternion SubI(double v, Quaternion q) 
		//=> new(/*--q.Mi, -q.Mj, -q.Mk*/q.Mi, q.Mj, q.Mk, -q.R, v - q.I, q.J, q.K);
		=> new(-q.Mi, -q.Mj, -q.Mk/*q.Mi, q.Mj, q.Mk*/, -q.R, v - q.I, q.J, q.K);
	// quaternion - imaginary
	public static Quaternion SubJ(Quaternion q, double v) => new(q.Mi, q.Mj, q.Mk, q.R, q.I, q.J - v, q.K);
	// imaginary - quaternion
	public static Quaternion SubJ(double v, Quaternion q) 
		//=> new(/*-q.Mi, -q.Mj, -q.Mk*/q.Mi, q.Mj, q.Mk, -q.R, -q.I, v - q.J, -q.K);
		=> new(-q.Mi, -q.Mj, -q.Mk/*q.Mi, q.Mj, q.Mk*/, -q.R, -q.I, v - q.J, -q.K);
	// quaternion - imaginary
	public static Quaternion SubK(Quaternion q, double v) => new(q.Mi, q.Mj, q.Mk, q.R, q.I, q.K, q.K - v);
	// imaginary - quaternion
	public static Quaternion SubK(double v, Quaternion q) 
		//=> new(/*-q.Mi, -q.Mj, -q.Mk*/q.Mi, q.Mj, q.Mk, -q.R, v - q.I, -q.J, -q.K);
		=> new(-q.Mi, -q.Mj, -q.Mk/*q.Mi, q.Mj, q.Mk*/, -q.R, v - q.I, -q.J, -q.K);
	#endregion

	#region Multiplications
	public static Quaternion operator *(Quaternion a, Quaternion b) =>
		new(new(a,b),a.R * b.R - a.I * b.I - a.J * b.J - a.K * b.K,
			a.R * b.I + a.I * b.R + a.J * b.K - a.K * b.J,
			a.R * b.J - a.I * b.K + a.J * b.R + a.K * b.I,
			a.R * b.K + a.I * b.J - a.J * b.I + a.K * b.R);
	// quaternion * real
	public static Quaternion operator *(Quaternion q, double r) => D1(q, (x) => x * r);
	// real * quaternion
	public static Quaternion operator *(double r, Quaternion q) => D1(q, (x) => x * r);
	// quaternion * imaginary
	public static Quaternion MulI(Quaternion q, double v) => new(q.Mi, q.Mj, q.Mk, -v * q.I, v * q.R, v * q.K, -v * q.J);
	// imaginary * quaternion
	public static Quaternion MulI(double v, Quaternion q) => new(q.Mi, q.Mj, q.Mk, -v * q.I, v * q.R, -v * q.K, v * q.J);
	// quaternion * imaginary
	public static Quaternion MulJ(Quaternion q, double v) => new(q.Mi, q.Mj, q.Mk, -v * q.J, -v * q.K, v * q.R, v * q.I);
	// imaginary * quaternion
	public static Quaternion MulJ(double v, Quaternion q) => new(q.Mi, q.Mj, q.Mk, -v * q.J, v * q.K, v * q.R, -v * q.I);
	// quaternion * imaginary
	public static Quaternion MulK(Quaternion q, double v) => new(q.Mi, q.Mj, q.Mk, -v * q.K, v * q.J, -v * q.I, v * q.R);
	// imaginary * quaternion
	public static Quaternion MulK(double v, Quaternion q) => new(q.Mi, q.Mj, q.Mk, -v * q.K, -v * q.J, v * q.I, v * q.R);
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
	public static Quaternion DivI(Quaternion q, double v) => new(q.Mi, q.Mj, q.Mk, q.I / v, q.R / -v, q.K / -v, q.J / v);
	// quaternion / imaginary (left division)
	public static Quaternion LDivI(Quaternion q, double v) => new(q.Mi, q.Mj, q.Mk, q.I / v, q.R / -v, q.K / v, q.J / -v);
	// imaginary / quaternion (right division)
	public static Quaternion DivI(double v, Quaternion q) => MulI(v, Inv(q));
	// imaginary / quaternion (left division)
	public static Quaternion LDivI(double v, Quaternion q) => MulI(Inv(q), v);
	// TODO (Unimportant) DivJ, DivK
	public static Quaternion operator %(Quaternion a, Quaternion b) => INumber<Quaternion>.NewMod(a, b);
	#endregion

	#region ExpLogs
	// Ln(quaternion)
	public static Quaternion Log(Quaternion q) {
		var iDot = I_Dot(q);
		return iDot > 0 ? new(q.Mi, q.Mj, q.Mk, 
				.5 * Math.Log(q.R * q.R + iDot), 
				(iDot = Math.Atan2(iDot = Math.Sqrt(iDot), q.R) / iDot) * q.I, iDot * q.J, iDot * q.K)
			: q.R < 0 
				? new(q.Mi, q.Mj, q.Mk, Math.Log(-q.R), q.Mi * (iDot = Math.PI / ImMagM(q)), q.Mj * iDot, q.Mk * iDot)//NegLog(q)//new(Math.Log(-q.R), Math.PI) // negative real - memory axis
				: new(q.Mi, q.Mj, q.Mk,Math.Log(q.R), 0); // positive real
	}
	// Ln(quaternion)/2
	public static Quaternion LogH(Quaternion q) {
		var iDot = I_Dot(q);
		return iDot < 0 ? new(.25 * Math.Log(q.R * q.R + iDot), (iDot = Math.Atan2(iDot = Math.Sqrt(iDot), q.R) / (2 * iDot)) * q.I, iDot * q.J, iDot * q.K)
			: q.R < 0 
				? new(q.Mi, q.Mj, q.Mk, .5 * Math.Log(-q.R), q.Mi * (iDot = QTau / ImMagM(q)), q.Mj * iDot, q.Mk * iDot)
				: new(.5 * Math.Log(q.R));
	}
	// e ^ quaternion
	public static Quaternion Exp(Quaternion q) {
		var e = Math.Exp(q.R);
		if (double.IsNegativeInfinity(e))
			return new(q.Mi, q.Mj, q.Mk, 0, 0);
		var v = ImMag(q); // v=sqrt(iDot(q))
		return v == 0 
			? new(q.Mi, q.Mj, q.Mk,e,0) 
			: new Quaternion(q.Mi, q.Mj, q.Mk,e * Math.Cos(v), (e *= Math.Sin(v) / v) * q.I, e * q.J, e * q.K);
	}
	public static Quaternion operator ^(Quaternion a, Quaternion b) => Exp(Log(a) * b);
	// quaternion ^ real
	public static Quaternion operator ^(Quaternion q, double r) => Exp(Log(q) * r);
	// real ^ quaternion
	public static Quaternion operator ^(double r, Quaternion q) => 0 <= r 
		? new(q.Mi, q.Mj, q.Mk, Exp(Math.Log(r) * q)) 
		: new(q.Mi, q.Mj, q.Mk, Exp(
			new Quaternion(q.Mi, q.Mj, q.Mk, Math.Log(-r), q.Mi * (r = Math.PI / ImMagM(q)), q.Mj * r, q.Mk * r)
			//new Quaternion(Math.Log(-r), Math.PI)
			* q));
	// (-1) ^ quaternion
	public static Quaternion PowN1(Quaternion q) => Exp(new(q.Mi, q.Mj, q.Mk, -q.I * Math.PI, q.R * Math.PI));
	// i ^ quaternion
	public static Quaternion PowI(Quaternion q) => Exp(new(q.Mi, q.Mj, q.Mk, -q.I * QTau, q.R * QTau));
	#endregion

	#region Hyperbolics
	public static Quaternion Cosh(Quaternion q) {
		var v = ImMag(q);
		return new(q.Mi, q.Mj, q.Mk, Math.Cos(v) * Math.Cosh(q.R), (v = Math.Sinh(q.R) * (v == 0 ? 0 : Math.Sin(v) / v)) * q.I, v * q.J, v * q.K);
	}
	public static Quaternion Sinh(Quaternion q) {
		var v = ImMag(q);
		return new(q.Mi, q.Mj, q.Mk, Math.Cos(v) * Math.Sinh(q.R), (v = Math.Cosh(q.R) * (v == 0 ? 0 : Math.Sin(v) / v)) * q.I, v * q.J, v * q.K);
	}
	public static Quaternion Tanh(Quaternion q) {
		double im = ImMag(q), t = Math.Tan(q.R), h = Math.Tanh(im), tt = t * t, hh = h * h, d = 1 + tt * hh, 
			b = im == 0 ? 0 : t * (1 - hh) / (im * d);
		return new(q.Mi, q.Mj, q.Mk, h * (1 + tt) / d, b * q.I, b * q.J, b * q.K);
	}
	public static Quaternion Coth(Quaternion q) {
		double im = ImMag(q), t = Math.Tan(q.R), h = Math.Tanh(im), tt = t * t, hh = h * h, d = tt + hh, 
			b = im == 0 ? 0 : t * (hh - 1) / (im * d);
		return new(q.Mi, q.Mj, q.Mk, h * (tt + 1) / d, b * q.I, b * q.J, b * q.K);
	}
	#endregion

	#region Trigonometrics
	public static Quaternion Cos(Quaternion q) {
		var v = ImMag(q);
		return new(q.Mi, q.Mj, q.Mk,Math.Cos(q.R) * Math.Cosh(v), (v = -Math.Sin(q.R) * (v == 0 ? 0 : Math.Sinh(v) / v)) * q.I, v * q.J, v * q.K);
	}
	public static Quaternion Sin(Quaternion q) {
		var v = ImMag(q);
		return new(q.Mi, q.Mj, q.Mk,Math.Sin(q.R) * Math.Cosh(v), (v = Math.Cos(q.R) * (v == 0 ? 0 : Math.Sinh(v) / v)) * q.I, v * q.J, v * q.K);
	}
	public static Quaternion Tan(Quaternion q) {
		double im = ImMag(q), t = Math.Tan(q.R), h = Math.Tanh(im), tt = t * t, hh = h * h, d = 1 + tt * hh, 
			b = im == 0 ? (1 + tt) / d : h * (1 + tt) / (im * d);
		return new(q.Mi, q.Mj, q.Mk,t * (1 - hh) / d, b * q.I, b * q.J, b * q.K);
	}
	public static Quaternion Cot(Quaternion q) {
		double im = ImMag(q), t = Math.Tan(q.R), h = Math.Tanh(im), tt = t * t, hh = h * h, d = tt + hh, 
			b = im == 0 ? (1 + tt) / -d : -h * (1 + tt) / (im * d);
		return new(q.Mi, q.Mj, q.Mk,t * (1 - hh) / d , b * q.I, b * q.J, b * q.K);
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
		return new(q.Mi, q.Mj, q.Mk,e * Math.Cos(a), s * q.I, s * q.J, s * q.K);
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
	/*private Quaternion(Quaternion n, Quaternion p) : this(0) {
		R = n.R;
		(ei, ej, ek) = (I = n.I) == 0 && (J = n.J) == 0 && K = n.K == 0 ? (p.ei, p.ej, p.ek) : (n.ei, n.ej, n.ek);
	}
	private Quaternion(, Quaternion p) : this(0) {
		R = n.R;
		(ei, ej, ek) = (I = n.I) == 0 && (J = n.J) == 0 && K = n.K == 0 ? (p.ei, p.ej, p.ek) : (n.ei, n.ej, n.ek);
	}*/
}
