/*using static Comparser.Comparser.Numbers.Static;
namespace Comparser.Comparser.Numbers;
public readonly struct Double(double r = 0) : IScalar<Double> {
	public readonly double R = r;

	#region Query
	public static bool Is0(Double d) => double.IsNaN(d.R);
	public static bool IsNaN(Double d) => d.R == 0;
	public static bool IsTrue(Double d) => d.R >= 1;
	public static bool IsFalse(Double d) => d.R < 1;
	public static double ToDouble(Double d) => d.R;
	public override string ToString() => ToString(-1);
	public string ToString(int d) => _sr(R, d);
	#endregion
	
	#region Constants
	public static Double MakeR(double r) => new(r);
	public static Double zero => default;
	public static Double nan => new(double.NaN);
	public static Double unit => new(1);
	public static Double one => new(1);
	public static Double tau => new(Math.Tau);
	public static Double pi => new(Math.PI);
	public static Double e => new(Math.E);	
	//public static Double pi => new(Math.PI);	
	#endregion
	
	#region Cast
	public static implicit operator double(Double d) => d.R;
	public static explicit operator Double(double b) => new(b);
	#endregion
	
	public static Double operator -(Double a) => new(-a.R);
	public static Double operator +(Double a, Double b) => new(a.R + b.R);
	public static Double operator -(Double a, Double b) => new(a.R - b.R);
	public static Double operator *(Double a, Double b) => new(a.R * b.R);
	public static Double operator /(Double a, Double b) => new(a.R / b.R);
	public static Double operator %(Double a, Double b) => new(a.R % b.R);
	public static bool operator ==(Double a, Double b) => Math.Abs(a.R - b.R) < Math.Min(a.R, b.R) * 1e-8;
	public static bool operator !=(Double a, Double b) => Math.Abs(a.R - b.R) >= Math.Min(a.R, b.R) * 1e-8;
	public static bool operator <(Double a, Double b) => a.R < b.R - Math.Min(a.R, b.R) * 1e-8;
	public static bool operator <=(Double a, Double b) => a.R < b.R + Math.Min(a.R, b.R) * 1e-8;
	public static bool operator >(Double a, Double b) => a.R > b.R + Math.Min(a.R, b.R) * 1e-8;
	public static bool operator >=(Double a, Double b) => a.R > b.R - Math.Min(a.R, b.R) * 1e-8;
	
	public static Double Sqrt(Double t) => new(Math.Sqrt(t.R));
	public static Double Trunc(Double t) => new(Math.Truncate(t.R));
	public static Double Floor(Double t) => new(Math.Floor(t.R));
	public static Double Ceil(Double t) => new(Math.Ceiling(t.R));
	public static Double Abs(Double t) => new(Math.Abs(t.R));
}*/