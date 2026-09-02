using System.Numerics;
namespace Comparser.Comparser.Numbers;
class BigRational(BigInteger  n, BigInteger  d) {
	public double FromD() => (double)_n / (double)_d;
	private readonly BigInteger _n = n;
	private readonly BigInteger _d = d;
	private static BigInteger Gcd(BigInteger  a, BigInteger  b) {
		if (a < 0) a = -a;
		if (b < 0) b = -b;
		while (a != 0 && b != 0) {
			if (a > b) a %= b;
			else b %= a;
		}
		return a | b;
	}
	public static BigRational operator +(BigRational a, BigRational b) {
		if (a._d % b._d == 0) {
			var n = a._n + (a._d / b._d) * b._n;
			var gcd = Gcd(a._d, n);
			return new(n/gcd, a._d / gcd);
		} if (b._d % a._d == 0) {
			var n = b._n + (b._d / a._d) * a._n;
			var gcd = Gcd(b._d, n);
			return new(n / gcd, b._d / gcd);
		}
		var g = Gcd(a._d, b._d);
		var na = a._n * (b._d / g) + b._n * (a._d / g);
		var nb = (a._d / g) * b._d;
		g = Gcd(na, nb);
		return new(na / g, nb / g);
	}
	public static BigRational operator -(BigRational a, BigRational b) {
		if (a._d % b._d == 0) {
			var n = a._n - (a._d / b._d) * b._n;
			var gcd = Gcd(a._d, n);
			return new(n/gcd, a._d / gcd);
		} if (b._d % a._d == 0) {
			var n = (b._d / a._d) * a._n - b._n;
			var gcd = Gcd(b._d, n);
			return new(n / gcd, b._d / gcd);
		}
		var g = Gcd(a._d, b._d);
		var na = a._n * (b._d / g) - b._n * (a._d / g);
		var nb = (a._d / g) * b._d;
		g = Gcd(na, nb);
		return new(na / g, nb / g);
	}
	public static BigRational operator *(BigRational a, BigRational b) {
		BigInteger  gcd = Gcd(a._n, b._d), g2 = Gcd(a._d, b._n);
		return new((a._n / gcd) * (b._n / g2), (a._d / g2) * (b._d / gcd));
	}
	public static BigRational operator /(BigRational a, BigRational b) {
		BigInteger  gcd = Gcd(a._n, b._n), g2 = Gcd(a._d, b._d);
		return new((a._n / gcd) * (b._d / g2), (a._d / g2) * (b._n / gcd));
	}
}