using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using static Comparser.Comparser.Numbers.Static;

namespace Comparser.Comparser.Numbers;
public interface INumber<T> where T : unmanaged, INumber<T> {
	public bool Is0();
	public bool IsNaN();

	#region Export
	public string ToString(int d);
	public static Color ToColorLog(T t, double repeatValue = 1) { var s = +t; return Hsv((T.Arg(t) + Math.PI) * 360 / Math.Tau, 1 - Math.Exp(-s), Math.Log(s) * .5 % repeatValue); }
	public static Color ToColorLin(T t, double repeatValue = 1) { var s = +t; return Hsv((T.Arg(t) + Math.PI) * 360 / Math.Tau, 1 - Math.Exp(-s), Math.Sqrt(s) % repeatValue); }
	public static Color ToColorExp(T t) => Hsv((T.Arg(t) + Math.PI) * 360 / Math.Tau, 1, 1 - Math.Exp(-+t));
	public static byte[] ToBytes(T str) {
		var size = Marshal.SizeOf(str);
		var arr = new byte[size];
		var ptr = IntPtr.Zero;
		try {
			ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(str, ptr, true);
			Marshal.Copy(ptr, arr, 0, size);
		} finally {
			Marshal.FreeHGlobal(ptr);
		}
		return arr;
	}
	public static T FromBytes(byte[] arr) {
		var str = new T();
		var size = Marshal.SizeOf(str);
		var ptr = IntPtr.Zero;
		try {
			ptr = Marshal.AllocHGlobal(size);
			Marshal.Copy(arr, 0, ptr, size);
			str = (T)Marshal.PtrToStructure(ptr, str.GetType())!;
		} finally {
			Marshal.FreeHGlobal(ptr);
		}
		return str;
	}
	#endregion

	#region Constants
	public static T Unit() => T.MakeR(1);
	public static abstract T NaN();
	public static abstract T Zero();
	public static abstract T One();
	#endregion

	#region Helpers
	public static abstract double Mix(T t, Func<double, double, double> del);
	public static abstract T D1(T a, Func<double, double> d);
	public static abstract T D2(T a, T b, Func<double, double, double> d);
	public static abstract T D3(T a, T b, T c, Func<double, double, double, double> d);

	public static T ComplexOp(T t, Func<Complex, Complex> func) {
		var r = func(Abs(t) * Complex.Complex_InvArg(T.Arg(t)));
		return INumber<Complex>.Abs(r) * T.InvArg(Complex.Arg(r), T.Axis(t));
	}
	#endregion
	
	#region Basics
	public static bool IsTrue(T t) => +t >= 1;
	public static bool IsFalse(T t) => +t < 1;
	public static T True(bool t) => t ? Unit() : T.Zero();
	public static abstract bool AreEqual(T a, T b);
	public static abstract T MakeR(double r);
	public static abstract double Re(T t);
	public static abstract double Im(T t);
	public static abstract double ImMag(T t);
	public static T T_I(T t) => T.MakeR(T.ImMag(t));
	public static abstract T operator !(T t);
	public static abstract T operator -(T t);
	public static abstract T operator ~(T t);
	public static abstract T U(T t);
	public static abstract T MulU(T r);
	public static abstract T NegU(T t);
	public static abstract double operator +(T t);
	public static abstract T Frac(T t);
	public static abstract T Trunc(T t);
	public static abstract T Floor(T t);
	public static abstract T Round(T t);
	public static abstract T Ceil(T t);
	public static abstract T Inv(T t);
	public static abstract double Arg(T t);
	public static abstract T InvArg(double p, T axis);
	public static abstract T Axis(T t);
	public static abstract T Sqrt(T t);
	public static abstract T Sqr(T t);
	public static abstract T Cub(T t);
	public static abstract T Quart(T t);
	// |x|
	public static double Abs(T t) => Math.Sqrt(+t);
	// x / |x|
	public static T Sign(T t) => t / Abs(t);
	public static abstract T AbsComp(T t);
	public static abstract double Dot(T a, T b);
	public static abstract T Min(T a, T b);
	public static abstract T Max(T a, T b);
	public static abstract T Clamp(T t, T min, T max);
	#endregion

	#region Additions
	public static T Add(T a, T b) => a + b;
	public static abstract T operator ++(T a);
	public static abstract T operator +(T a, T b);
	public static abstract T operator +(T t, double r);
	public static abstract T operator +(double r, T t);
	public static abstract T AddV(T t, double u);
	public static abstract T AddV(double u, T t);
	#endregion

	#region Subtractions
	public static T Sub(T a, T b) => a - b;
	public static abstract T operator --(T a);
	public static abstract T operator -(T a, T b);
	public static abstract T operator -(T t, double r);
	public static abstract T operator -(double r, T t);
	public static abstract T SubV(T t, double i);
	public static abstract T SubV(double i, T t);
	#endregion

	#region Multiplications
	public static T Mul(T a, T b) => a * b;
	public static abstract T operator *(T a, T b);
	public static abstract T operator *(T t, double r);
	public static abstract T operator *(double r, T t);
	public static abstract double operator |(T a, T b);
	//public abstract static T MulNV(T t, double i);
	//public abstract static T MulNV(double i, T t);
	#endregion

	#region Divisions
	//public static T Div(T a, T b) => a / b;
	public static abstract T operator /(T a, T b);
	public static abstract T operator /(T t, double r);
	public static abstract T operator /(double r, T t);
	public static abstract T LDiv(T a, T b);
	public static abstract T operator %(T a, T b);
	//public abstract static T DivNV(T t, double i);
	//public abstract static T DivNV(double i, T t);
	public static T NewMod(T a, T b) => a - T.D1(a / b, Math.Truncate) * b;
	public static T CompMod(T a, T b) => T.D2(a, b, Mod);

	#endregion

	#region ExpLogs
	public static abstract T Log(T t);
	public static abstract T LogH(T t);
	public static abstract T Exp(T t);
	public static T Pow(T a, T b) => a ^ b;
	public static abstract T operator ^(T a, T b);
	public static abstract T operator ^(T t, double r);
	public static abstract T operator ^(double r, T t);
	public static abstract T PowN1(T t);
	public static abstract T PowI(T t);
	public static T SoftMax(T a, T b) => T.Log(T.Exp(a) + T.Exp(b));
	public static T SoftMin(T a, T b) => -T.Log(T.Exp(-a) + T.Exp(-b));
	// SoftMax(x,0)
	public static T SoftAbs(T t) => T.Log(1 + T.Exp(t));
	// SoftMin(x,0)
	public static T SoftNeg(T t) => -T.Log(1 + T.Exp(-t));
	#endregion

	#region Hyperbolics
	public static abstract T Cosh(T t);
	public static T Sech(T t) => T.Inv(T.Cosh(t));
	public static abstract T Sinh(T t);
	public static T Csch(T t) => T.Inv(T.Sinh(t));
	public static abstract T Tanh(T t);
	public static abstract T Coth(T t);
	#endregion

	#region Trigonometrics
	public static abstract T Cos(T t);
	public static T Sec(T t) => T.Inv(T.Cos(t));
	public static abstract T Sin(T t);
	public static T Csc(T t) => T.Inv(T.Sin(t));
	public static abstract T Tan(T t);
	public static abstract T Cot(T t);
	#endregion

	#region ArcHyperbolics
	public static abstract T Acosh(T t);
	public static abstract T Asinh(T t);
	public static abstract T Atanh(T t);
	public static abstract T Acoth(T t);
	public static T I_Acosh(T t) => T.Log(t + T.Sqrt(T.Sqr(t) - 1));
	public static T Asech(T t) => T.Acosh(T.Inv(t));
	public static T I_Asinh(T t) => T.Log(t + T.Sqrt(T.Sqr(t) + 1));
	public static T Acsch(T t) => T.Asinh(T.Inv(t));
	public static T I_Atanh(T t) => T.LogH((1 + t) / (1 - t));
	public static T I_Acoth(T t) => T.LogH((t + 1) / (t - 1));
	#endregion

	#region ArcTrigonometrics
	public static abstract T Acos(T t);
	public static abstract T Asin(T t);
	public static abstract T Atan(T t);
	public static abstract T Acot(T t);
	public static T I_Acos(T t) => T.Acosh(~t);
	public static T Asec(T t) => T.Acos(T.Inv(t));
	public static T I_Asin(T t) => T.NegU(T.Asinh(~t));
	public static T Acsc(T t) => T.Asin(T.Inv(t));
	public static T I_Atan(T t) => T.NegU(T.LogH(T.SubV(1, t) / T.AddV(1, t)));
	public static T I_Acot(T t) => T.NegU(T.LogH(T.AddV(t, 1) / T.SubV(t, 1)));
	#endregion

	#region Exotic Trigonometrics
	public static T Sinc(T t) => t.Is0() ? T.MakeR(1) : T.Sin(t) / t;
	public static T Nsinc(T t) => Sinc(Math.PI * t);
	public static T Sinhc(T t) => T.Sinh(t) / t;
	public static T Nsinhc(T t) => Sinhc(Math.PI * t);
	public static T Cosc(T t) => (1 - T.Cos(t)) / t;
	public static T Ncosc(T t) => Cosc(Math.PI * t);
	public static T Coshc(T t) => (1 - T.Cosh(t)) / t;
	public static T Ncoshc(T t) => Coshc(Math.PI * t);
	public static abstract T Sin_P(T t);
	public static abstract T Sin_2Q(T t);
	#endregion

	#region Simple Functions and Constants
	public static T C_E() => T.MakeR(Math.E);
	public static T C_Pi() => T.MakeR(Math.PI);
	public static T C_Tau() => T.MakeR(Math.Tau);
	public static T C_Gamma() => T.MakeR(0.57721566490153286060651209008240243104215933593992); // Euler's constant
	public static T Log10(T t) => T.Log(t) / Static.Ln10;
	public static T Log2(T t) => T.Log(t) / Ln2;
	public static T Exp10(T t) => T.Exp(Ln10 * t);
	public static T Exp2(T t) => T.Exp(Ln2 * t);
	public static T T_Re(T t) => T.MakeR(T.Re(t));
	public static T Neg(T t) => -t;
	public static T SqrAbs(T t) => T.MakeR(+t);
	public static T T_Abs(T t) => T.MakeR(Abs(t));
	public static T T_Arg(T t) => T.MakeR(T.Arg(t));
	public static T Conj(T t) => !t;
	public static T Cbrt(T t) => t ^ 1.0 / 3;
	public static T Lerp(T a, T b, T alpha) => b * alpha + a * (1 - alpha);
	public static T Lerp(T a, T b, double alpha) => b * alpha + a * (1 - alpha);
	#endregion

	#region Special Functions
	public static abstract T Gauss(T t);
	public static abstract T Gamma(T t);
	public static abstract T Factorial(T t);
	public static abstract T Zeta(T t);

	public static T I_Gauss(T t) => T.Exp(-T.Sqr(t));

	// ReSharper disable AccessToModifiedClosure
	/// <summary>
	/// evaluates a factorial over an array of plot points, for an arbitrary interval of real components of the input
	/// uses Stirling approximation and reflection formula optimized for such a batch
	/// </summary>
	/// <param name="result">the pointer to the beginning of the array that will be filled with evaluated factorial points</param>
	/// <param name="width">the length of the result array we want to fill, aka the plot bitmap width</param>
	/// <param name="imag">the imaginary component of our scan line over the real plot interval, make sure its real part is 0!</param>
	/// <param name="uN"> Moving uN/uD pixels to the right on the plot increments the input by 1</param>
	/// <param name="uD"> Moving uN/uD pixels to the right on the plot increments the input by 1, must be positive</param>
	/// <param name="center"> where on the result array is the corresponding zero input (y-axis crossing)</param>
	/// <param name="corrections">bernoulli correction terms</param>
	/// <param name="shifts">stirling pole shift terms</param>
	public static unsafe void I_Factorial_Batched(T* result, int width, T imag, int uN, int uD, int center, int corrections = 4, int shifts = 16) {
		var gcd = Gcd(uN, uD); uN /= gcd; uD /= gcd; // simplify uN/uD ratio
		imag -= T.Re(imag); // remove real component, the real part is what we are iterating instead
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
		ArgumentOutOfRangeException.ThrowIfZero(uN); // if uN is zero, then the plot would have the entire range collapsed to zero
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(uD); // if zero, then the plot would be infinitely zoomed in, for negatives (reverse plot) put than sign to the nominator uN
		ArgumentOutOfRangeException.ThrowIfNegative(corrections);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(shifts);
		var s = new T[shifts]; // terms
		T t = T.NaN(), x = T.NaN(), b = T.NaN();
		int u = 0, i = 0, pi = 0, ni = 0, m, aun = Math.Abs(uN), rp = (width - center) / aun, rn = center / aun, rm = Math.Min(rn, rp);
		Action side, branch, bp, bn, iShiftCenter, iShiftNoCenter, iUpdateShift;
		if (uN > 0) { bp = BtcP; bn = BtcN; } else { bp = BtcN; bn = BtcP; }
		(Action a, int end) run = (bp, rp);
		if (uD < shifts) {
			// use the sliding window optimization (enhancing the performance of shifts, to be almost like just one shift)
			iShiftCenter = InitialShifts;
			iShiftNoCenter = InitialShifts;
			iUpdateShift = UpdateShiftsWindow;
		} else {
			// sliding window won't be worth it, so only batch the reflections (enhacing theperformance up to 2x):
			iShiftCenter = InitialShiftsTotal;
			iShiftNoCenter = Nop;
			iUpdateShift = UpdateShiftsTotal;
		}
		if (center < 0 || center >= width) {
			if (center < 0) side = OnlyP;
			else (side, run.a) = (OnlyN, bn);
			branch = NoCenter;
		} else branch = Center; // the plot contains the center
		for (u = 0; u < aun; ++u) {
			i = 0; pi = ni = u + center; t = T.MakeR(LnTh); // train (sliding sum window)
			branch();
			Run(); // final run (every branch had it at the end, so i took it out)
		}
		return;

		// Branch cases:
		void OnlyP() { // will be calculating only positives, without the initial point (as plot starts from x+1)
			while ((m = ni + aun) < 0)
				ni = m; // shift the initial point x so that x+1 lands on the plot 
			run.end = (width - ni) / aun;  // how many unitLength shift iterations will still land on the plot?
		}
		void OnlyN() { // will be calculating only negatives, without the initial point (as plot starts from x-1)
			while ((m = ni - aun) >= width)
				ni = m; // shift the initial point x so that x-1 lands on the plot
			run = (bn, ni / aun); // how many unitLength shift iterations will still land on the plot?
		}
		void NoCenter() {
			side(); // prepare branch if we are doing only positives or negatives
			b = (x = (double)((pi = ni) - center) / aun + imag) + shifts; // initialize point x, and b = x + shifts
			iShiftNoCenter();
			// interval where both positive and negative reflections are on the plot: Run() outside the branch
		}
		void Center() {
			x = (double)u / aun + imag;
			b = x + shifts;
			iShiftCenter();
			result[center] = T.Exp(Btc()); // plot the center point 
			// interval where both positive and negative reflections are on the plot:
			run = (BtcB, rm);
			Run();
			// remaining interval where only one half of the reflection is on the plot: Run() outside the branch
			run = rn < rp ? (bp, rp) : (bn, rn);
			run.a(); // plot the initial point
		}
		void InitialShifts() {
			for(var o = 1; o < shifts; ++o) // o = 0 would be gamma, but that would reflect so easily, so just decrement center instead 
				t -= s[o] = T.Log(T.Sqr(x + o));
		}
		void InitialShiftsTotal() {
			t += s[0];
			for(var o = 1; o < shifts; ++o) // o = 0 would be gamma, but that would reflect so easily, so just decrement center instead 
				s[0] += T.Log(T.Sqr(x + o));
			t -= s[0];
		}
		void UpdateShiftsWindow() {
			for(var k = 0; k < uD; ++k) // if there is a denominator in unit pixels, we have to move the window that many times to land on another pixel
				t += -s[i % shifts] + (s[i++ % shifts] = T.Log(T.Sqr(b++)));
			run.a(); // plot the point(s)
		}
		void UpdateShiftsTotal() {
			x += uD;
			InitialShiftsTotal();
			run.a(); // plot the point(s)
		}
		void Nop() => s[0] = T.Zero(); // do not initialize shifts, just reset their memory
		// Actions:
		void Run() { while(i < run.end) iUpdateShift(); }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		T Btc() => T.Exp(B() + t + C()); // base + train + corrections
		void BtcP() => result[pi += uN] = Btc(); // positive to the right (left if uN < 0)
		void BtcN() => result[ni -= uN] = T.Inv(Btc() * Nsinc(t)); // reflected to the left (right if uN < 0)
		void BtcB() => result[ni -= uN] = T.Inv((result[pi += uN] = Btc()) * Nsinc(t)); // positive to the right (left if uN < 0) + reflected to the left (right if uN < 0)
		// Stirling base:
		T B() => (b - .5) * T.Log(b) - b; 
		// Stirling corrections:
		T C() {
			T c = T.Zero(), xp = b; 
			for (var ic = 0; ic < corrections; ++ic) 
				c += B2G[ic] * (xp /= T.Sqr(b)); // B2G[i] = B2[i]/(6ii+4i+2), B2[i] = B_2(i+1)
			return c;
		}
	}
	// ReSharper restore AccessToModifiedClosure
	public static unsafe void I_Gamma_Batched(T* result, int width, T imag, int uN, int uD, int center, int corrections = 4, int shifts = 16)
		=> I_Factorial_Batched(result, width, imag, uN, uD, center - (int)Math.Round((float)uN / uD), corrections, shifts); // if uD > 0, the center shift will be rounded, but that will offset the plot only by at most 0.5px
	
	
	private static T Factorial_Stirling_Positive(T t, int corrections = 4, int shifts = 16) {
		// core equation with shifts:
		T c = t + shifts, s = (c - .5) * T.Log(c) - c + LnTh; // LnTh = Math.Log(Math.Tau)/2
		while(1 <= --shifts) // it would calculate gamma if we replaced 1 with 0, but decrementing t is simpler and faster
			s -= T.LogH(T.Sqr(t + shifts)); // LogH = Ln/2
		// corrections:
		t = c;
		for (var i = 0; i < corrections; ++i) 
			s += B2G[i] * (c /= T.Sqr(t)); // B2G[i] = B2[i]/(6ii+4i+2), B2[i] = B_2(i+1)
		return T.Exp(s);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static T Gamma_Stirling_Positive(T t, int corrections = 4, int shifts = 16) => Factorial_Stirling_Positive(--t, corrections, shifts);
	// z! reflection: z!(-z)!nsinc(z) = 1 => c! = Inv((-c)!nsinc(-c)) = Inv((-c)!nsinc(c)) 
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static T Factorial_Stirling(T t) => 0 <= T.Re(t) ? Factorial_Stirling_Positive(t) : T.Inv(Factorial_Stirling_Positive(-t) * Nsinc(t));
	// Sin_P reflection: Γ(1-z)Γ(z) = iτ/((-1)^z-(-1)^(-z)) = iτ/Sin_P(z) =>
	// z -> z+.5: Γ(.5-z)Γ(.5+z) = iτ/Sin_P(z+.5) => Γ(c) = iτ/(Γ(.5+z)Sin_P(z+.5))
	// z = .5-c => iτ/(Γ(1-c))Sin_P(c)) 
	public static T I_Gamma(T t) => T.Re(t) > .5 ? Gamma_Stirling_Positive(t) : Math.Tau * T.Inv(Gamma_Stirling_Positive(1 - t) * T.Sin_P(t));
	public static T I_Factorial(T t) {
		var re = T.Re(t);
		var r = (int)Math.Floor(re);
		// Is it a natural number?
		if (Math.Abs(r - re) > 1e-8 || r < 0 || !(t - T.MakeR(re)).Is0())
			return Factorial_Stirling(t); // iterate complex factorial for n iterations
		var f = 1.0; // calculate natural factorial exactly (as far as double's mantissa allows)
		while (r > 0)
			f *= r--;
		return T.MakeR(f);
	}
	/// <summary>
	/// Optimized Hasse/Sondow approximation of zeta function (without the last division by "c + 1")
	/// </summary>
	/// <param name="t">T input (often written as "s")</param>
	/// <returns>zeta evaluated</returns>
	private static T Zeta_Hasse(T t) {
		// zeta(c) = (c-1)^(-1) * sum[n=0..i]: (n+1)^(-1) * sum[k=0..n]: (-1)^k * Combination(n,k) * (k + 1)^(1 - c)
		T nk, ns = 1.5 - (2 ^ -t), c1 = 1 - t; // 0th n term is 1, 1st term is (1-2^(1-c))/2 = 0.5-2^(-c), precount that, and loop n = n->2
		const int maxIterations = 42;
		T one, term = one = T.MakeR(1);
		var p = new int[maxIterations + 2]; // pascal triangle rows, the first term is always zero, because it is unused
		for (int tri, n = p[1] = 1; (tri = n) < maxIterations && +term / Math.Max(1, +ns) > 1e-20; ns += term = nk / (n + 1)) {
			while (tri > 1)
				p[tri] += p[--tri]; // add the non-edge pascal triangle terms
			int e, o;
			if (n % 2 == 0)
				(e, o) = (n, ++n);
			else (o, e) = (n, ++n); // even and odd k iterators (so I don't have to (-1)^k)
			++p[p[n] = 1]; // increment 2nd term in the pascal row, last term (one) in the pascal row is a new one
			nk = one; // zeroth k term is 1, precount that and loop k = n->1
			do nk += p[e] * (e + 1 ^ c1); // even k terms, p[e] = Combinations(n,e)
			while (1 <= (e -= 2)); // decrement even k iterators down to 1
			do nk -= p[o] * (o + 1 ^ c1); // odd k terms, p[o] = Combinations(n,o)
			while (1 <= (o -= 2)); // decrement odd k iterators down to 1
		}
		return ns; // multiplied by Inc(c - 1) outside this function, as c - 1 is precomputed there
	}
	private static T Zeta_Euler(T t) { // using B2k Bernoulli numbers/factorials
		int terms = 32, berns = B2F.Length;
		var s = T.MakeR(1);
		for (byte n = 2; n < terms; ++n)
			s += n ^ -t;
		var sum = s + (s = terms ^ -t) * (.5 + terms / (t - 1)) + B2F[0] * (s *= t / terms);
		terms *= terms;
		for (var k = 1; k < berns; ++k)
			sum += B2F[k] * (s *= (t + 2 * k + 1) * (t + 2 * k + 2) / terms);
		return sum;
	}
	// Elegant Zeta reflection: ζ(1-s)τ^s = CosI(s)Γ(s)ζ(s)
	// ζ(s) = (ζ(1-s)τ^s) / (CosI(s)Γ(s))
	// Zeta_Euler_Reflected(c) = Zeta_Euler(c) * R
	// R = (τ ^ c) * Inv(CosI(c) * Gamma_Stirling_Negative(c)) // substitute Inv and Gamma_Stirling_Negative
	// = (τ ^ c) / (CosI(c) * InvITau(Gamma_Stirling_Positive(1 - c) * SinN1(c))) // substitute InvITau
	// = (τ ^ c) / (iτCosI(c) / (Gamma_Stirling_Positive(1 - c) * SinN1(c))) // simplify division
	// = (τ ^ c) * Gamma_Stirling_Positive(1 - c) * SinN1(c) / (CosI(c) * i * tau) // move iτ
	// = -i(τ ^ (c - 1)) * Gamma_Stirling_Positive(1 - c) * SinN1(c) / CosI(c) // substitute NISinI, and c - 1 with the difference from pole that was precomputed for the condition
	// = (τ ^ c1) * Gamma_Stirling_Positive(1 - c) * NISinI(c)
	public static T I_Zeta(T t) {
		// relative to the pole (useful in many places in this algo, including the reflection)
		var t1 = t - 1;
		// squared distance from the pole
		var pole = +t1;
		// exactly the pole - return infinity
		return pole == 0 ? T.MakeR(double.PositiveInfinity)
			// very near the pole - use Laurent that is excellent when this near, hopefully 5 terms are enough for the Laurent series with the distance from to pole up to e-4
			: pole < 1e-8 ? T.Inv(t1) + C_Gamma() - t1 * (G1 - t1 * (G2 - t1 * (G3 - t1 * (G4 - t1 * G5))))
			// near the pole - use general Hasse that is decent everywhere
			: pole < 4 ? Zeta_Hasse(t) * T.Inv(t1)
			// far from pole - use Euler that is excellent when far from it
			: T.Re(t) >= .5 ? Zeta_Euler(t)
			// negative and far from the pole - reflect Euler (using the formula derived above)
			: Zeta_Euler(t) * (C_Tau() ^ t1) * Gamma_Stirling_Positive(1 - t) * T.Sin_2Q(t);
	}
	#endregion

	public static abstract void IndexAndAddToRgb(Color[] axis, T indices, T value);
	//T.Floor( ValueToScreenLin(length, i * h, S, E))
	/*void DrawLine(T s, T a) {

				if (s.r >= 0 && s.r < length)
					axis.r[(int)s.r] += a.r;
				if (s.i >= 0 && s.i < length)
					axis.i[(int)s.i] += a.i;
			}*/
}
