using System.Runtime.CompilerServices;
namespace Comparser.Comparser.Numbers;
public enum NumericKind : byte {
	Real = 1,
	Complex = 2,
	Quaternion = 3
}
public interface ILeaf/*<T> where T : unmanaged, IScalar<T>*/ {
	public NumericKind kind { get; }
	public ILeaf/*<T>*/ Cast(NumericKind to);

	public bool IsNaN();
	public bool Is0();

	/*public static NumericKind PromoteR(Real a, out ILeaf oa, ref ILeaf b) {
		NumericKind k;
		if (b.kind > a.kind) oa = a.Cast(k = b.kind);
		else { oa = a; k = NumericKind.Real; }
		return k;
	}
	public static NumericKind PromoteC(Complex a, out ILeaf oa, ref ILeaf b) {
		NumericKind k;
		if (b.kind > a.kind) oa = a.Cast(k = b.kind);
		else if (b.kind < (k = a.kind)) b = b.Cast(k = (oa = a).kind);
		else oa = a;
		return k;
	}
	public static NumericKind PromoteQ(Quaternion a, out ILeaf oa, ref ILeaf b) {
		NumericKind k;
		if (b.kind < a.kind) b = b.Cast(k = (oa = a).kind);
		else { oa = a; k = NumericKind.Quaternion; }
		return k;
	}*/
	public static NumericKind Promote(ref ILeaf/*<T>*/ a, ref ILeaf/*<T>*/ b) {
		NumericKind k;
		if (b.kind > a.kind) a = a.Cast(k = b.kind);
		else if(b.kind < (k = a.kind)) b = b.Cast(k);
		return k;
	}
	public static NumericKind Demote(ref ILeaf/*<T>*/ a, ref ILeaf/*<T>*/ b) {
		NumericKind k;
		if (b.kind < a.kind) a = a.Cast(k = b.kind);
		else if(b.kind > (k = a.kind)) b = b.Cast(k);
		return k;
	}
	private static NumericKind Promote3(ref ILeaf/*<T>*/ a, ref ILeaf/*<T>*/ b,ref ILeaf/*<T>*/ c) {
		NumericKind k;
		if (a.kind > b.kind) {
			if (c.kind > a.kind) 
				a = a.Cast(k = b.kind);
			else 
				c = c.Cast(k = a.kind);
			b = b.Cast(k);
		} else {
			if (c.kind > b.kind) {
				c = c.Cast(k = c.kind);
			} else {
				b = b.Cast(k = b.kind);
			}
			a = a.Cast(k);
		}
		return k;
	}
	private static NumericKind Promote4(ref ILeaf/*<T>*/ a, ref ILeaf/*<T>*/ b, ref ILeaf/*<T>*/ c, ref ILeaf/*<T>*/ d) {
		NumericKind k;
		if (a.kind > b.kind) {
			if (c.kind > d.kind) {
				if (a.kind > c.kind) c = c.Cast(k = a.kind);
				else a = a.Cast(k = c.kind);
				(b, d) = (b.Cast(k), d.Cast(k));
			} else {
				if (a.kind > d.kind) d = d.Cast(k = a.kind);
				else a = a.Cast(k = d.kind);
				(b, c) = (b.Cast(k), c.Cast(k));
			}
		} else {
			if (c.kind > d.kind) {
				if (b.kind > c.kind) c = c.Cast(k = b.kind); 
				else b = b.Cast(k = c.kind);
				(a, d) = (a.Cast(k), d.Cast(k));
			} else {
				if (b.kind > d.kind) d = d.Cast(k = b.kind);
				else b = b.Cast(k = d.kind);
				(a, c) = (a.Cast(k), b.Cast(k));
			}
		}
		return k;
	}
	#region Polymorphic Arithmetics
	static protected Quaternion AddCq(Complex a, Quaternion b) => new(a.R + b.R, a.I + b.I, b.J, b.K);
	static protected Quaternion SubCq(Complex a, Quaternion b) => new(a.R - b.R, a.I - b.I, -b.J, -b.K);
	static protected Quaternion SubQc(Quaternion a, Complex b) => new(a.R - b.R, a.I - b.I, a.J, a.K);
	static protected Quaternion MulCq(Complex a, Quaternion b) => new(a.R * b.R - a.I * b.I, a.R * b.I + a.I * b.R, a.R * b.J - a.I * b.K, a.R * b.K + a.I * b.J);
	static protected Quaternion MulQc(Quaternion a, Complex b) => new(a.R * b.R - a.I * b.I, a.R * b.I + a.I * b.R, b.R * a.J + b.I * a.K, b.R * a.K - b.I * a.J);
	static protected Quaternion DivCq(Complex a, Quaternion b) => MulCq(a, Quaternion.Inv(b));
	static protected Quaternion DivQc(Quaternion a, Complex b) => MulQc(a, Complex.Inv(b));
	static protected Quaternion LDivCq(Complex a, Quaternion b) => MulQc(Quaternion.Inv(b), a);
	static protected Quaternion LDivQc(Quaternion a, Complex b) => MulCq(Complex.Inv(b), a);
	static protected Quaternion ModCq(Complex a, Quaternion b) => SubCq(a, Quaternion.D1(DivCq(a, b), double.Truncate) * b);
	static protected Quaternion ModQc(Quaternion a, Complex b) => a - MulQc(Quaternion.D1(DivQc(a, b), double.Truncate), b);
	static protected Quaternion PowCq(Complex a, Quaternion b) => Quaternion.Exp(MulCq(Complex.Log(a), b));
	static protected Quaternion PowQc(Quaternion a, Complex b) => Quaternion.Exp(MulQc(Quaternion.Log(a), b));
	public static ILeaf/*<T>*/ Add(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => (a.kind, b.kind) switch {
		(NumericKind.Real, NumericKind.Real) => (Real/*<T>*/)a + (Real/*<T>*/)b,
		(NumericKind.Real, NumericKind.Complex) => ((Real/*<T>*/)a).R + (Complex/*<T>*/)b,
		(NumericKind.Real, NumericKind.Quaternion) => ((Real/*<T>*/)a).R + (Quaternion/*<T>*/)b,
		(NumericKind.Complex, NumericKind.Real)  => (Complex/*<T>*/)a + ((Real/*<T>*/)b).R,
		(NumericKind.Complex, NumericKind.Complex) => (Complex/*<T>*/)a + (Complex/*<T>*/)b,
		(NumericKind.Complex, NumericKind.Quaternion) => AddCq((Complex/*<T>*/)a, (Quaternion/*<T>*/)b),
		(NumericKind.Quaternion, NumericKind.Real) => (Quaternion/*<T>*/)a + ((Real/*<T>*/)b).R,
		(NumericKind.Quaternion, NumericKind.Complex) => AddCq((Complex/*<T>*/)b, (Quaternion/*<T>*/)a),
		(NumericKind.Quaternion, NumericKind.Quaternion) => (Quaternion/*<T>*/)a + (Quaternion/*<T>*/)b,
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Sub(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => (a.kind, b.kind) switch {
		(NumericKind.Real, NumericKind.Real) => (Real/*<T>*/)a - (Real/*<T>*/)b,
		(NumericKind.Real, NumericKind.Complex) => ((Real/*<T>*/)a).R - (Complex/*<T>*/)b,
		(NumericKind.Real, NumericKind.Quaternion) => ((Real/*<T>*/)a).R - (Quaternion/*<T>*/)b,
		(NumericKind.Complex, NumericKind.Real)  => (Complex/*<T>*/)a - ((Real/*<T>*/)b).R,
		(NumericKind.Complex, NumericKind.Complex) => (Complex/*<T>*/)a - (Complex/*<T>*/)b,
		(NumericKind.Complex, NumericKind.Quaternion) => SubCq((Complex/*<T>*/)a, (Quaternion/*<T>*/)b),
		(NumericKind.Quaternion, NumericKind.Real) => (Quaternion/*<T>*/)a - ((Real/*<T>*/)b).R,
		(NumericKind.Quaternion, NumericKind.Complex) => SubQc((Quaternion/*<T>*/)a, (Complex/*<T>*/)b),
		(NumericKind.Quaternion, NumericKind.Quaternion) => (Quaternion/*<T>*/)a - (Quaternion/*<T>*/)b,
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Mul(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => (a.kind, b.kind) switch {
		(NumericKind.Real, NumericKind.Real) => (Real/*<T>*/)a * (Real/*<T>*/)b,
		(NumericKind.Real, NumericKind.Complex) => ((Real/*<T>*/)a).R * (Complex/*<T>*/)b,
		(NumericKind.Real, NumericKind.Quaternion) => ((Real/*<T>*/)a).R * (Quaternion/*<T>*/)b,
		(NumericKind.Complex, NumericKind.Real)  => (Complex/*<T>*/)a * ((Real/*<T>*/)b).R,
		(NumericKind.Complex, NumericKind.Complex) => (Complex/*<T>*/)a * (Complex/*<T>*/)b,
		(NumericKind.Complex, NumericKind.Quaternion) => MulCq((Complex/*<T>*/)a, (Quaternion/*<T>*/)b),
		(NumericKind.Quaternion, NumericKind.Real) => (Quaternion/*<T>*/)a * ((Real/*<T>*/)b).R,
		(NumericKind.Quaternion, NumericKind.Complex) => MulCq((Complex/*<T>*/)b, (Quaternion/*<T>*/)a),
		(NumericKind.Quaternion, NumericKind.Quaternion) => (Quaternion/*<T>*/)a * (Quaternion/*<T>*/)b,
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Div(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => (a.kind, b.kind) switch {
		(NumericKind.Real, NumericKind.Real) => (Real/*<T>*/)a / (Real/*<T>*/)b,
		(NumericKind.Real, NumericKind.Complex) => ((Real/*<T>*/)a).R / (Complex/*<T>*/)b,
		(NumericKind.Real, NumericKind.Quaternion) => ((Real/*<T>*/)a).R / (Quaternion/*<T>*/)b,
		(NumericKind.Complex, NumericKind.Real)  => (Complex/*<T>*/)a / ((Real/*<T>*/)b).R,
		(NumericKind.Complex, NumericKind.Complex) => (Complex/*<T>*/)a / (Complex/*<T>*/)b,
		(NumericKind.Complex, NumericKind.Quaternion) => DivCq((Complex/*<T>*/)a, (Quaternion/*<T>*/)b),
		(NumericKind.Quaternion, NumericKind.Real) => (Quaternion/*<T>*/)a / ((Real/*<T>*/)b).R,
		(NumericKind.Quaternion, NumericKind.Complex) => DivQc((Quaternion/*<T>*/)a, (Complex/*<T>*/)b),
		(NumericKind.Quaternion, NumericKind.Quaternion) => (Quaternion/*<T>*/)a / (Quaternion/*<T>*/)b,
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ LDiv(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => (a.kind, b.kind) switch {
		(NumericKind.Real, NumericKind.Real) => (Real/*<T>*/)a / (Real/*<T>*/)b,
		(NumericKind.Real, NumericKind.Complex) => ((Real/*<T>*/)a).R / (Complex/*<T>*/)b,
		(NumericKind.Real, NumericKind.Quaternion) => ((Real/*<T>*/)a).R / (Quaternion/*<T>*/)b,
		(NumericKind.Complex, NumericKind.Real) => (Complex/*<T>*/)a / ((Real/*<T>*/)b).R,
		(NumericKind.Complex, NumericKind.Complex) => (Complex/*<T>*/)a / (Complex/*<T>*/)b,
		(NumericKind.Complex, NumericKind.Quaternion) => LDivCq((Complex/*<T>*/)a, (Quaternion/*<T>*/)b),
		(NumericKind.Quaternion, NumericKind.Real) => (Quaternion/*<T>*/)a / ((Real/*<T>*/)b).R,
		(NumericKind.Quaternion, NumericKind.Complex) => LDivQc((Quaternion/*<T>*/)a, (Complex/*<T>*/)b),
		(NumericKind.Quaternion, NumericKind.Quaternion) => LDiv((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Mod(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => (a.kind, b.kind) switch {
		(NumericKind.Real, NumericKind.Real) => (Real/*<T>*/)a % (Real/*<T>*/)b,
		(NumericKind.Real, NumericKind.Complex) => INumber<Complex>.NewMod(((Real/*<T>*/)a).R, (Complex/*<T>*/)b),
		(NumericKind.Real, NumericKind.Quaternion) => INumber<Quaternion>.NewMod(((Real/*<T>*/)a).R, (Quaternion/*<T>*/)b),
		(NumericKind.Complex, NumericKind.Real) => INumber<Complex>.NewMod((Complex/*<T>*/)a, ((Real/*<T>*/)b).R),
		(NumericKind.Complex, NumericKind.Complex) => (Complex/*<T>*/)a % (Complex/*<T>*/)b,
		(NumericKind.Complex, NumericKind.Quaternion) => ModCq((Complex/*<T>*/)a, (Quaternion/*<T>*/)b),
		(NumericKind.Quaternion, NumericKind.Real) => INumber<Quaternion>.NewMod((Quaternion/*<T>*/)a, ((Real/*<T>*/)b).R),
		(NumericKind.Quaternion, NumericKind.Complex) => ModQc((Quaternion/*<T>*/)a, (Complex/*<T>*/)b),
		(NumericKind.Quaternion, NumericKind.Quaternion) => (Quaternion/*<T>*/)a % (Quaternion/*<T>*/)b,
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Pow(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => (a.kind, b.kind) switch {
		(NumericKind.Real, NumericKind.Real) => (Real/*<T>*/)a ^ (Real/*<T>*/)b,
		(NumericKind.Real, NumericKind.Complex) => ((Real/*<T>*/)a).R ^ (Complex/*<T>*/)b,
		(NumericKind.Real, NumericKind.Quaternion) => ((Real/*<T>*/)a).R ^ (Quaternion/*<T>*/)b,
		(NumericKind.Complex, NumericKind.Real)  => (Complex/*<T>*/)a ^ ((Real/*<T>*/)b).R,
		(NumericKind.Complex, NumericKind.Complex) => (Complex/*<T>*/)a ^ (Complex/*<T>*/)b,
		(NumericKind.Complex, NumericKind.Quaternion) => PowCq((Complex/*<T>*/)a, (Quaternion/*<T>*/)b),
		(NumericKind.Quaternion, NumericKind.Real) => (Quaternion/*<T>*/)a ^ ((Real/*<T>*/)b).R,
		(NumericKind.Quaternion, NumericKind.Complex) => PowQc((Quaternion/*<T>*/)a, (Complex/*<T>*/)b),
		(NumericKind.Quaternion, NumericKind.Quaternion) => (Quaternion/*<T>*/)a ^ (Quaternion/*<T>*/)b,
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ D1(ILeaf/*<T>*/ a, Func<double, double> d) => a.kind switch {
		NumericKind.Real => Real.D1((Real)a, d),
		NumericKind.Complex => Complex.D1((Complex)a, d),
		NumericKind.Quaternion => Quaternion.D1((Quaternion)a, d),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ D2(ILeaf/*<T>*/ a, ILeaf b, Func<double, double, double> d) => a.kind switch {
		NumericKind.Real => Real.D2((Real)a,(Real)b, d),
		NumericKind.Complex => Complex.D2((Complex)a, (Complex)b, d),
		NumericKind.Quaternion => Quaternion.D2((Quaternion)a, (Quaternion)b, d),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ D3(ILeaf/*<T>*/ a, ILeaf b, ILeaf c, Func<double, double, double, double> d) => a.kind switch {
		NumericKind.Real => Real.D3((Real)a, (Real)b, (Real)c, d),
		NumericKind.Complex => Complex.D3((Complex)a, (Complex)b, (Complex)c, d),
		NumericKind.Quaternion => Quaternion.D3((Quaternion)a, (Quaternion)b, (Quaternion)c, d),
		_ => Real/*<T>*/.nan
	};
	public static double Mix(ILeaf a, Func<double, double, double> d) => a.kind switch {
		NumericKind.Real => Real.Mix((Real)a, d),
		NumericKind.Complex => Complex.Mix((Complex)a, d),
		NumericKind.Quaternion => Quaternion.Mix((Quaternion)a, d),
		_ => Real/*<T>*/.nan
	};
	public static double Dot(ILeaf a, ILeaf b) => Demote(ref a, ref b)switch {
		NumericKind.Real => Real.Dot((Real)a, (Real)b),
		NumericKind.Complex => Complex.Dot((Complex)a, (Complex)b),
		NumericKind.Quaternion => Quaternion.Dot((Quaternion)a, (Quaternion)b),
		_ => Real/*<T>*/.nan
	};
	public static void IndexAndAddToRgb(Color[] axis, ILeaf indices, ILeaf value) {
		switch (indices.kind) {
			case NumericKind.Real: Real.IndexAndAddToRgb(axis, (Real)indices, (Real)value); return;
			case NumericKind.Complex: Complex.IndexAndAddToRgb(axis, (Complex)indices, (Complex)value); return;
			case NumericKind.Quaternion: Quaternion.IndexAndAddToRgb(axis, (Quaternion)indices, (Quaternion)value); return;
		}
	}
	public static ILeaf Lerp(ILeaf a, ILeaf b, double alpha) => (a.kind, b.kind) switch {
		(NumericKind.Real, NumericKind.Real) => (Real)b * alpha + (Real)a * (1 - alpha),
		(NumericKind.Real, NumericKind.Complex) =>  (Complex)b * alpha + ((Real)a).R * (1 - alpha),
		(NumericKind.Real, NumericKind.Quaternion) =>  (Quaternion)b * alpha + ((Real)a).R * (1 - alpha),
		(NumericKind.Complex, NumericKind.Real)  =>  ((Real)b).R * alpha + (Complex)a * (1 - alpha),
		(NumericKind.Complex, NumericKind.Complex) =>  (Complex)b * alpha + (Complex)a * (1 - alpha),
		(NumericKind.Complex, NumericKind.Quaternion) =>  AddCq((Complex)a * (1 - alpha), (Quaternion)b * alpha),
		(NumericKind.Quaternion, NumericKind.Real) =>  ((Real)b).R * alpha + (Quaternion)a * (1 - alpha),
		(NumericKind.Quaternion, NumericKind.Complex) =>  AddCq((Complex)b * alpha, (Quaternion)a * (1 - alpha)),
		(NumericKind.Quaternion, NumericKind.Quaternion) => (Quaternion)b * alpha + (Quaternion)a * (1 - alpha),
		_ => Real/*<T>*/.nan
	};
	#endregion
	
	#region Query
	public double Re();
	public double Im();
	public static Real True(bool t) => t ? Real.unit : Real.zero;
	public bool IsTrue();
	public bool IsFalse();
	public string ToString(int decimals);
	public static byte[] ToBytes(ILeaf t) => t.kind switch {
		NumericKind.Real => INumber<Real>.ToBytes((Real)t),
		NumericKind.Complex => INumber<Complex>.ToBytes((Complex)t),
		NumericKind.Quaternion => INumber<Quaternion>.ToBytes((Quaternion)t),
		_ => []
	};
	public static ILeaf FromBytes(byte[] t) => (NumericKind)t[0] switch {
		NumericKind.Real => INumber<Real>.FromBytes(t),
		NumericKind.Complex => INumber<Complex>.FromBytes(t),
		NumericKind.Quaternion => INumber<Quaternion>.FromBytes(t),
		_ => Real.nan
	};
	public static (double h, double s, double v) Log2Hsv(ILeaf t) => t.kind switch {
		NumericKind.Real => INumber<Real>.Log2Hsv((Real)t),
		NumericKind.Complex => INumber<Complex>.Log2Hsv((Complex)t),
		NumericKind.Quaternion => INumber<Quaternion>.Log2Hsv((Quaternion)t),
		_ => (0,0,0)
	};
	public static (double h, double s, double v) Lin2Hsv(ILeaf t) => t.kind switch {
		NumericKind.Real => INumber<Real>.Lin2Hsv((Real)t),
		NumericKind.Complex => INumber<Complex>.Lin2Hsv((Complex)t),
		NumericKind.Quaternion => INumber<Quaternion>.Lin2Hsv((Quaternion)t),
		_ => (0,0,0)
	};
	public static (double h, double s, double v) Log2HsvC(ILeaf t) => t.kind switch {
		NumericKind.Real => INumber<Real>.Log2HsvC((Real)t),
		NumericKind.Complex => INumber<Complex>.Log2HsvC((Complex)t),
		NumericKind.Quaternion => INumber<Quaternion>.Log2HsvC((Quaternion)t),
		_ => (0,0,0)
	};
	public static (double h, double s, double v) Lin2HsvC(ILeaf t) => t.kind switch {
		NumericKind.Real => INumber<Real>.Lin2HsvC((Real)t),
		NumericKind.Complex => INumber<Complex>.Lin2HsvC((Complex)t),
		NumericKind.Quaternion => INumber<Quaternion>.Lin2HsvC((Quaternion)t),
		_ => (0,0,0)
	};
	#endregion
	
	#region Unary
	public static ILeaf/*<T>*/ T_Re(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.T_Re((Real/*<T>*/)t),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.T_Re((Complex/*<T>*/)t),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.T_Re((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ T_I(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.T_I((Real/*<T>*/)t),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.T_I((Complex/*<T>*/)t),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.T_I((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ ImMag(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => (Real)Real/*<T>*/.ImMag((Real/*<T>*/)t),
		NumericKind.Complex => (Real)Complex/*<T>*/.ImMag((Complex/*<T>*/)t),
		NumericKind.Quaternion => (Real)Quaternion/*<T>*/.ImMag((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Frac(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Frac((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Frac((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Frac((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Truncate(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Truncate((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Truncate((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Truncate((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Floor(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Floor((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Floor((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Floor((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Round(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Round((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Round((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Round((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Ceiling(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Ceiling((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Ceiling((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Ceiling((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Cycle(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Cycle((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Cycle((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Cycle((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Clamp01(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Clamp01((Real/*<T>*/)t),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.Clamp01((Complex/*<T>*/)t),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.Clamp01((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ SoftClamp01(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.SoftClamp01((Real/*<T>*/)t),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.SoftClamp01((Complex/*<T>*/)t),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.SoftClamp01((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Neg(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Neg((Real/*<T>*/)t),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.Neg((Complex/*<T>*/)t),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.Neg((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Inv(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Inv((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Inv((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Inv((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ T_Arg(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.T_Arg((Real/*<T>*/)t),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.T_Arg((Complex/*<T>*/)t),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.T_Arg((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ SoftAbs(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.SoftAbs((Real/*<T>*/)t),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.SoftAbs((Complex/*<T>*/)t),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.SoftAbs((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ SoftNeg(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.SoftNeg((Real/*<T>*/)t),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.SoftNeg((Complex/*<T>*/)t),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.SoftNeg((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Cbrt(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Cbrt((Real/*<T>*/)t),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.Cbrt((Complex/*<T>*/)t),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.Cbrt((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Cub(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Cub((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Cub((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Cub((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Quart(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Quart((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Quart((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Quart((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Gauss(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Gauss((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Gauss((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Gauss((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Gamma(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Gamma((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Gamma((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Gamma((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Zeta(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Zeta((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Zeta((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Zeta((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	
	public static ILeaf/*<T>*/ Conj(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Conj((Real/*<T>*/)t),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.Conj((Complex/*<T>*/)t),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.Conj((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Sqr(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Sqr((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Sqr((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Sqr((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Sqrt(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => N(t, out var r) ? Complex/*<T>*/.Sqrt((Complex)r) : Real/*<T>*/.Sqrt((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Sqrt((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Sqrt((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ T_Abs(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.T_Abs((Real/*<T>*/)t),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.T_Abs((Complex/*<T>*/)t),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.T_Abs((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ T_SqrAbs(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.SqrAbs((Real/*<T>*/)t),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.SqrAbs((Complex/*<T>*/)t),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.SqrAbs((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static double SqrAbs(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => +(Real/*<T>*/)t,
		NumericKind.Complex => +(Complex/*<T>*/)t,
		NumericKind.Quaternion => +(Quaternion/*<T>*/)t,
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ AbsComp(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.AbsComp((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.AbsComp((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.AbsComp((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Sign(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Sign((Real/*<T>*/)t),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.Sign((Complex/*<T>*/)t),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.Sign((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Factorial(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Factorial((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Factorial((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Factorial((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	
	public static ILeaf/*<T>*/ Log(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => N(t, out var r) ? Complex/*<T>*/.Log((Complex/*<T>*/)r) : Real/*<T>*/.Log((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Log((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Log((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Exp(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Exp((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Exp((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Exp((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Log2(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => N(t, out var r) ? INumber<Complex/*<T>,T*/>.Log2((Complex/*<T>*/)r) : INumber<Real/*<T>,T*/>.Log2((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Log2((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Log2((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Log10(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => N(t, out var r) ? INumber<Complex/*<T>,T*/>.Log10((Complex/*<T>*/)r) : INumber<Real/*<T>,T*/>.Log2((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Log10((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Log10((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Exp2(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Exp2((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Exp2((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Exp2((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Exp10(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Exp10((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Exp10((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Exp10((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	// trigs/hyperbolics
	public static ILeaf/*<T>*/ Sinhc(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Sinhc((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Sinhc((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Sinhc((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Nsinhc(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Nsinhc((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Nsinhc((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Nsinhc((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Sinc(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Sinc((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Sinc((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Sinc((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Nsinc(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Nsinc((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Nsinc((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Nsinc((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Coshc(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Coshc((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Coshc((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Coshc((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Ncoshc(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Ncoshc((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Ncoshc((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Ncoshc((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Cosc(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Cosc((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Cosc((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Cosc((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Ncosc(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Ncosc((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Ncosc((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Ncosc((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	
	public static ILeaf/*<T>*/ Acosh(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Acosh((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Acosh((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Acosh((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Asinh(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Asinh((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Asinh((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Asinh((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Atanh(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Atanh((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Atanh((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Atanh((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Asech(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Asech((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Asech((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Asech((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Acsch(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Acsch((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Acsch((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Acsch((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Acoth(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Acoth((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Acoth((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Acoth((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	
	public static ILeaf/*<T>*/ Cosh(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Cosh((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Cosh((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Cosh((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Sinh(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Sinh((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Sinh((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Sinh((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Tanh(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Tanh((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Tanh((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Tanh((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Sech(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Sech((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Sech((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Sech((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Csch(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Csch((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Csch((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Csch((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Coth(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Coth((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Coth((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Coth((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	
	public static ILeaf/*<T>*/ Acos(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Acos((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Acos((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Acos((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Asin(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Asin((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Asin((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Asin((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Atan(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Atan((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Atan((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Atan((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Asec(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Asec((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Asec((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Asec((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Acsc(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Acsc((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Acsc((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Acsc((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Acot(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Acot((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Acot((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Acot((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	
	public static ILeaf/*<T>*/ Cos(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Cos((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Cos((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Cos((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Sin(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Sin((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Sin((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Sin((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Tan(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Tan((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Tan((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Tan((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Sec(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Sec((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Sec((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Sec((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Csc(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.Csc((Real/*<T>*/)t),
		NumericKind.Complex =>  INumber<Complex/*<T>,T*/>.Csc((Complex/*<T>*/)t),
		NumericKind.Quaternion =>  INumber<Quaternion/*<T>,T*/>.Csc((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Cot(ILeaf/*<T>*/ t) => t.kind switch {
		NumericKind.Real => Real/*<T>*/.Cot((Real/*<T>*/)t),
		NumericKind.Complex => Complex/*<T>*/.Cot((Complex/*<T>*/)t),
		NumericKind.Quaternion => Quaternion/*<T>*/.Cot((Quaternion/*<T>*/)t),
		_ => Real/*<T>*/.nan
	};
	#endregion
	
	#region Binary
	public static ILeaf/*<T>*/ Min(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => Promote(ref a, ref b) switch {
		NumericKind.Real => Real/*<T>*/.Min((Real/*<T>*/)a, (Real/*<T>*/)b),
		NumericKind.Complex => Complex/*<T>*/.Min((Complex/*<T>*/)a, (Complex/*<T>*/)b),
		NumericKind.Quaternion => Quaternion/*<T>*/.Min((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ Max(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => Promote(ref a, ref b) switch {
		NumericKind.Real => Real/*<T>*/.Max((Real/*<T>*/)a, (Real/*<T>*/)b),
		NumericKind.Complex => Complex/*<T>*/.Max((Complex/*<T>*/)a, (Complex/*<T>*/)b),
		NumericKind.Quaternion => Quaternion/*<T>*/.Max((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ SoftMin(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => Promote(ref a, ref b) switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.SoftMin((Real/*<T>*/)a, (Real/*<T>*/)b),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.SoftMin((Complex/*<T>*/)a, (Complex/*<T>*/)b),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.SoftMin((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ SoftMax(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => Promote(ref a, ref b) switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.SoftMax((Real/*<T>*/)a, (Real/*<T>*/)b),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.SoftMax((Complex/*<T>*/)a, (Complex/*<T>*/)b),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.SoftMax((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ CompMod(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => Promote(ref a, ref b) switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.CompMod((Real/*<T>*/)a, (Real/*<T>*/)b),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.CompMod((Complex/*<T>*/)a, (Complex/*<T>*/)b),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.CompMod((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ ExpB(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => Promote(ref a, ref b) switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.ExpB((Real/*<T>*/)a, (Real/*<T>*/)b),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.ExpB((Complex/*<T>*/)a, (Complex/*<T>*/)b),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.ExpB((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ LogB(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => Promote(ref a, ref b) switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.LogB((Real/*<T>*/)a, (Real/*<T>*/)b),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.LogB((Complex/*<T>*/)a, (Complex/*<T>*/)b),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.LogB((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ SoftAbsB(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => Promote(ref a, ref b) switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.SoftAbsB((Real/*<T>*/)a, (Real/*<T>*/)b),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.SoftAbsB((Complex/*<T>*/)a, (Complex/*<T>*/)b),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.SoftAbsB((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ SoftNegB(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => Promote(ref a, ref b) switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.SoftNegB((Real/*<T>*/)a, (Real/*<T>*/)b),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.SoftNegB((Complex/*<T>*/)a, (Complex/*<T>*/)b),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.SoftNegB((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ SoftClamp01B(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b) => Promote(ref a, ref b) switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.SoftClamp01B((Real/*<T>*/)a, (Real/*<T>*/)b),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.SoftClamp01B((Complex/*<T>*/)a, (Complex/*<T>*/)b),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.SoftClamp01B((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b),
		_ => Real/*<T>*/.nan
	};
	#endregion
	
	#region Ternary	+ 4
	public static ILeaf/*<T>*/ Clamp(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b, ILeaf/*<T>*/ c) => Promote3(ref a, ref b, ref c) switch {
		NumericKind.Real => Real/*<T>*/.Clamp((Real/*<T>*/)a, (Real/*<T>*/)b, (Real/*<T>*/)c),
		NumericKind.Complex => Complex/*<T>*/.Clamp((Complex/*<T>*/)a, (Complex/*<T>*/)b, (Complex/*<T>*/)c),
		NumericKind.Quaternion => Quaternion/*<T>*/.Clamp((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b, (Quaternion/*<T>*/)c),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ SoftClamp(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b,ILeaf/*<T>*/ c) => Promote3(ref a, ref b, ref c) switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.SoftClamp((Real/*<T>*/)a, (Real/*<T>*/)b, (Real/*<T>*/)c),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.SoftClamp((Complex/*<T>*/)a, (Complex/*<T>*/)b, (Complex/*<T>*/)c),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.SoftClamp((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b, (Quaternion/*<T>*/)c),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ SoftMaxB(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b,ILeaf/*<T>*/ c) => Promote3(ref a, ref b, ref c) switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.SoftMaxB((Real/*<T>*/)a, (Real/*<T>*/)b, (Real/*<T>*/)c),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.SoftMaxB((Complex/*<T>*/)a, (Complex/*<T>*/)b, (Complex/*<T>*/)c),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.SoftMaxB((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b, (Quaternion/*<T>*/)c),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ SoftMinB(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b,ILeaf/*<T>*/ c) => Promote3(ref a, ref b, ref c) switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.SoftMinB((Real/*<T>*/)a, (Real/*<T>*/)b, (Real/*<T>*/)c),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.SoftMinB((Complex/*<T>*/)a, (Complex/*<T>*/)b, (Complex/*<T>*/)c),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.SoftMinB((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b, (Quaternion/*<T>*/)c),
		_ => Real/*<T>*/.nan
	};
	public static ILeaf/*<T>*/ SoftClampB(ILeaf/*<T>*/ a, ILeaf/*<T>*/ b,ILeaf/*<T>*/ c, ILeaf/*<T>*/d) => Promote4(ref a, ref b, ref c, ref d) switch {
		NumericKind.Real => INumber<Real/*<T>,T*/>.SoftClampB((Real/*<T>*/)a, (Real/*<T>*/)b, (Real/*<T>*/)c, (Real/*<T>*/)d),
		NumericKind.Complex => INumber<Complex/*<T>,T*/>.SoftClampB((Complex/*<T>*/)a, (Complex/*<T>*/)b, (Complex/*<T>*/)c, (Complex/*<T>*/)d),
		NumericKind.Quaternion => INumber<Quaternion/*<T>,T*/>.SoftClampB((Quaternion/*<T>*/)a, (Quaternion/*<T>*/)b, (Quaternion/*<T>*/)c, (Quaternion/*<T>*/)d),
		_ => Real/*<T>*/.nan
	};
	
	#endregion
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool N(ILeaf t, out double r) => (r = t.Re()) < 0;
}
