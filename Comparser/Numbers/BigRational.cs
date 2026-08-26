using System.Numerics;
namespace Comparser.Comparser.Numbers;
class BigRational(BigInteger  n, BigInteger  d) {
	public double FromD() => (double)N / (double)D;
	protected BigInteger  N = n, D = d;
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
		if (a.D % b.D == 0) {
			var n = a.N + (a.D / b.D) * b.N;
			var gcd = Gcd(a.D, n);
			return new(n/gcd, a.D / gcd);
		} if (b.D % a.D == 0) {
			var n = b.N + (b.D / a.D) * a.N;
			var gcd = Gcd(b.D, n);
			return new(n / gcd, b.D / gcd);
		}
		var g = Gcd(a.D, b.D);
		var na = a.N * (b.D / g) + b.N * (a.D / g);
		var nb = (a.D / g) * b.D;
		g = Gcd(na, nb);
		return new(na / g, nb / g);
	}
	public static BigRational operator -(BigRational a, BigRational b) {
		if (a.D % b.D == 0) {
			var n = a.N - (a.D / b.D) * b.N;
			var gcd = Gcd(a.D, n);
			return new(n/gcd, a.D / gcd);
		} if (b.D % a.D == 0) {
			var n = (b.D / a.D) * a.N - b.N;
			var gcd = Gcd(b.D, n);
			return new(n / gcd, b.D / gcd);
		}
		var g = Gcd(a.D, b.D);
		var na = a.N * (b.D / g) - b.N * (a.D / g);
		var nb = (a.D / g) * b.D;
		g = Gcd(na, nb);
		return new(na / g, nb / g);
	}
	public static BigRational operator *(BigRational a, BigRational b) {
		BigInteger  gcd = Gcd(a.N, b.D), g2 = Gcd(a.D, b.N);
		return new((a.N / gcd) * (b.N / g2), (a.D / g2) * (b.D / gcd));
	}
	public static BigRational operator /(BigRational a, BigRational b) {
		BigInteger  gcd = Gcd(a.N, b.N), g2 = Gcd(a.D, b.D);
		return new((a.N / gcd) * (b.D / g2), (a.D / g2) * (b.N / gcd));
	}
}