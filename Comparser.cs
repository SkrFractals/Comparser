using A = (string[] k, Expressions.Complex[] v);

namespace Expressions;

public class Comparser {
	public static readonly string[] x = ["x"];
	public static readonly string[] xy = ["x", "y"];
	public A Eval(Expression exp, A arg) => exp.Eval(x);
	public A Eval(string text, A arg) => new Expression(this, text.Replace(" ", ""), arg).Eval(x); // = (["x"], eval)
	public CallFunction[] CustomFunctions = [];
	// If any name A is contained within a name B, make sure name B comes first. For example acosh before cos.
	// Otherwise this would need to be sorted decrementally by length at runtime.
	private static readonly CallFunction[] DefaultFunctions = [
		// exp/log
		new Call("exp10", typeof(Exp10)),	// Decimal exponential: 10^x
		new Call("exp2", typeof(Exp2)),		// Binary exponential: 2^x
		new CallDel("exp", Complex.Exp),	// Exponential: e^x = (cos(b)+isin(b))e^a
		new Call("log10", typeof(Log10)),	// Decimal logarithm: log_10(x) = ln(x)/ln(2)
		new Call("log2", typeof(Log2)),		// Binary logarithm: log_2(x) = ln(x)/ln(10)
		new CallDel("log", Complex.Log),	// Natural logarithm: ln(x)
		new CallDel("ln", Complex.Log),		// Natural logarithm: ln(x)

		// sincs
		new CallDel("nsinhc", Complex.Nsinhc),	// sinhc(x*pi)
		new CallDel("sinchpi", Complex.Nsinhc),	// sinhc(x*pi)
		new CallDel("sinhc", Complex.Sinhc),	// sinhc(x) = sinh(x)/x
		new CallDel("nsinc", Complex.Nsinc),	// sinc(x*pi)
		new CallDel("sincpi", Complex.Nsinc),	// sinc(x*pi)
		new CallDel("sinc", Complex.Sinc),		// sinc(x) = sin(x)/x
		new CallDel("sinc", Complex.Cosc),		// cosc(x) = (1-cos(x))/x

		// arc hyperbolics
		new CallDel("acosh", Complex.Acosh),// acosh(x)
		new CallDel("asinh", Complex.Asinh),// asinh(x)
		new CallDel("atanh", Complex.Atanh),// atanh(x)
		new CallDel("asech", Complex.Asech),// asech(x)
		new CallDel("acsch", Complex.Acsch),// acsch(x)
		new CallDel("acoth", Complex.Acoth),// acoth(x)

		// hyperbolics
		new CallDel("cosh", Complex.Cosh),	// cosh(x)
		new CallDel("sinh", Complex.Sinh),	// sinh(x)
		new CallDel("tanh", Complex.Tanh),	// tanh(x)
		new CallDel("sech", Complex.Sech),	// sech(x)
		new CallDel("csch", Complex.Csch),	// csch(x)
		new CallDel("coth", Complex.Coth),	// coth(x)

		// arc trigs
		new CallDel("acos", Complex.Acos),	// acos(x)
		new CallDel("asin", Complex.Asin),	// asin(x)
		new CallDel("atan", Complex.Atan),	// atan(x)
		new CallDel("asec", Complex.Asec),	// asec(x)
		new CallDel("acsc", Complex.Acsc),	// acsc(x)
		new CallDel("acot", Complex.Acot),	// acot(x)

		// trigs
		new CallDel("cos", Complex.Cos),	// cos(x)
		new CallDel("sin", Complex.Sin),	// sin(x)
		new CallDel("tan", Complex.Tan),	// tan(x)
		new CallDel("sec", Complex.Sec),	// sec(x)
		new CallDel("csc", Complex.Csc),	// csc(x)
		new CallDel("cot", Complex.Cot),	// cot(x)

		// basics
		new Call("real", typeof(Re)),		// Real part: Re(x) = a
		new Call("imag", typeof(Im)),		// Imaginary part: Im(x) = b
		new Call("re", typeof(Re)),			// Real part: Re(x) = a
		new Call("im", typeof(Im)),			// Imaginary part: Im(x) = b
		new CallDel("frac", Complex.Frac),	// Signed fractional part: Frac(x) = x - Trunc(x)
		new CallDel("trunc", Complex.Trunc),// Whole part: Truncate(x)
		new CallDel("floor", Complex.Floor),// Round down: Floor(x)
		new CallDel("round", Complex.Round),// Round near: Round(x)
		new CallDel("ceil", Complex.Ceil),	// Round up: Ceiling(x)
		new Call("neg", typeof(Neg)),		// Negation: -x
		new CallDel("inv", Complex.Inv),	// Inverse: 1/x
		new Call("sqrabs", typeof(Sqrabs)),	// Squared absolute value: |x|^2 = a*a + b*b
		new Call("abs", typeof(Abs)),		// Absolute value: |x| = sqrt(a*a + b*b)
		new Call("arg", typeof(Arg)),		// Argument: Arg(x)
		new Call("conj", typeof(Conj)),		// Conjugate: Conj(x) = a - bi

		// powers
		new CallDel("sqrt", Complex.Sqrt),	// Square root: Sqrt(x)
		new Call("cbrt", typeof(Cbrt)),		// Cube root: Cbrt(x)
		new CallDel("sqr", Complex.Sqr),	// Square: x*x
		new Call("cub", typeof(Cub)),		// Cube: x*x*x

		// specials
		new Call("gauss", typeof(Gauss)),		// e^(-x^2)
		new Call("softplus", typeof(Sftadd))	// ln(1+e^x)
		];
	private static readonly A DefaultConstants = (
		[	    "pi",		"tau",		 "e",		"i",	   "one"], 
		[Complex.pi, Complex.tau, Complex.e, Complex.i, Complex.One]);

	#region Call Functions
	public abstract class CallFunction(string name, string[] i, string def = "") { // "name(i0,i1,...) = def"
		protected readonly string Def = def;
		public readonly string Name = name;
		public readonly string[] Input = i;
		public abstract Expression New(Comparser context, ref string text, A arg);
	}
	public class CallDel(string name, Func<Complex, Complex> del) : CallFunction(name, x) {
		public override Expression New(Comparser context, ref string text, A arg)
			=> new Del(context, del, ref text, arg);
	}
	private class Del(Comparser context, Func<Complex, Complex> del, ref string text, A arg) : Expression(context, ref text, out _, arg) {
		public override A Eval(string[] o, A? i = null) => (o, [del(base.Eval(x, i).v[0])]);
	}
	private class Call(string name, Type type) : CallFunction(name, x) {
		public override Expression New(Comparser context, ref string text, A arg) {
			object[] a = [context, text, arg]; // activator arguments
			var n = (Expression)Activator.CreateInstance(type, a)!;
			text = (string)a[1]; // ref string text
			return n;
		}
	}
	public class CallCustom(string name, string[] i, string def) : CallFunction(name, i, def) {
		public override Expression New(Comparser context, ref string text, A arg) 
			=> new CustomFunc(context, Def, Input, ref text, arg);
	}
	private class CustomFunc(Comparser context, string def, string[] input, ref string text, A arg) 
		: Expression(context, ref text, out _, arg) {
		public override A Eval(string[] o, A? i = null) {
			return Context.Eval(def, base.Eval(input, i));
		}
	}
	#endregion

	#region Function Implementations
	static readonly double ln10 = Math.Log(10);
	static readonly double ln2 = Math.Log(2);

	private class Min(Comparser context, ref string text, A arg) : Expression(context, ref text, out _, arg) {
		public override A Eval(string[] o, A? i = null) { var (_, v) = base.Eval(xy, i); return (o, [v.Length == 2 ? new(Math.Min(v[0].R, v[1].R), Math.Min(v[1].I, v[1].I)) : Complex.NaN]); }
	}
	private class Max(Comparser context, ref string text, A arg) : Expression(context, ref text, out _, arg) {
		public override A Eval(string[] o, A? i = null) { var (_, v) = base.Eval(xy, i); return (o, [v.Length == 2 ? new(Math.Max(v[0].R, v[1].R), Math.Max(v[1].I, v[1].I)) : Complex.NaN]); }
	}

	private class Exp10(Comparser context, ref string text, A arg)	: Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null) 
			=> (o, [Complex.Exp(ln10 * base.Eval(x, i).v[0])]); }
	private class Exp2(Comparser context, ref string text, A arg)	: Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null) 
			=> (o, [Complex.Exp(ln2 * base.Eval(x, i).v[0])]); }
	private class Log10(Comparser context, ref string text, A arg)	: Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null) 
			=> (o, [Complex.Log(base.Eval(x, i).v[0]) / ln10]); }
	private class Log2(Comparser context, ref string text, A arg)	: Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null)
			=> (o, [Complex.Log(base.Eval(x, i).v[0]) / ln2]); }
	private class Re(Comparser context, ref string text, A arg)		: Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null) 
			=> (o, [new(base.Eval(x, i).v[0].R)]); }
	private class Im(Comparser context, ref string text, A arg)		: Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null) 
			=> (o, [new(base.Eval(x, i).v[0].I)]); }
	private class Neg(Comparser context, ref string text, A arg)	: Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null) 
			=> (o, [-base.Eval(x, i).v[0]]); }
	private class Sqrabs(Comparser context, ref string text, A arg) : Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null)
			=> (o, [new(+base.Eval(x, i).v[0])]); }
	private class Abs(Comparser context, ref string text, A arg)	: Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null)
			=> (o, [new(Complex.Abs(base.Eval(x, i).v[0]))]); }
	private class Arg(Comparser context, ref string text, A arg)	: Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null) 
			=> (o, [new(Complex.Arg(base.Eval(x, i).v[0]))]); }
	private class Conj(Comparser context, ref string text, A arg)	: Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null) 
			=> (o, [!base.Eval(x).v[0]]); }
	private class Cbrt(Comparser context, ref string text, A arg)	: Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null)
			=> (o, [base.Eval(x).v[0] ^ (1.0/3)]); }
	private class Cub(Comparser context, ref string text, A arg)	: Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null) 
			{ var e = base.Eval(x).v[0]; return (o, [e * Complex.Sqr(e)]); } }
	private class Gauss(Comparser context, ref string text, A arg)	: Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null) 
			=> (o, [Complex.Exp(-Complex.Sqr(base.Eval(x, i).v[0]))]); }
	private class Sftadd(Comparser context, ref string text, A arg) : Expression(context, ref text, out _, arg) { public override A Eval(string[] o, A? i = null)
			=> (o, [Complex.Log(1 + Complex.Exp(base.Eval(x, i).v[0]))]); }
	#endregion

	#region Operators
	public class Operator(byte order = 0, bool right = false, bool eatOp = true) {
		public readonly byte Order = order; // order of operations
		public readonly bool Right = right; // right-associativity
		public readonly bool EatOp = eatOp; // using an operator symbol? (if false, it is an operator-less multiplication)
		public virtual Complex Op(bool neg, Complex value, Complex operand) => neg ? -value : value;
	}
	private class Add() : Operator(1) {
		public override Complex Op(bool neg, Complex value, Complex operand) => (neg ? -value : value) + operand;
	}
	private class Sub() : Operator(1) {
		public override Complex Op(bool neg, Complex value, Complex operand) => (neg ? -value : value) - operand;
	}
	/*private class Mod() : Operator(2) {
		public override Complex Op(bool neg, Complex value, Complex operand) => operand.Is0 ? Complex.NaN : (neg ? -value : value) % operand;
	}*/
	private class Div() : Operator(2) {
		public override Complex Op(bool neg, Complex value, Complex operand) => operand.Is0 ? Complex.NaN : (neg ? -value : value) / operand;
	}
	// if ": Operator(3, ...)", then 1/2*3 could be 1/6, but left associativity for "*/" matches computer evals
	private class Mul(bool eatOp = true) : Operator(2, false, eatOp) { 
		public override Complex Op(bool neg, Complex value, Complex operand) => (neg ? -value : value) * operand;
	}
	private class Pow() : Operator(4, true) {
		public override Complex Op(bool neg, Complex value, Complex operand) => (neg ? -1 : 1) * (operand.Is0 ? new(1) : value ^ operand);
	}
	#endregion

	public class Expression {
		protected class Input(Complex value, Operator op, int arg, Expression? term = null, Expression? operand = null, bool negative = false) {
			// unary negative flag
			public bool Negative = negative;
			// simple value if it is only a number not containing an expression term (can remain present even if it is replaced with a term)
			public Complex Value = value;
			// main term and operand (second term)
			public Expression? Term = term, Operand = operand;
			// operator (eval = term <operator> operand), if it is a pure parent Operator, it only evaluates the term
			public Operator Op = op;
			public int Arg = arg;
		}
		// contains user-defined custom function
		protected readonly Comparser Context;
		private readonly List<Input> Expr;
		/// <summary>
		/// Evaluates the expression
		/// </summary>
		/// <returns>Evaluated value of this expression</returns>
		public virtual A Eval(string[] o, A? i = null) {
			var result = new Complex[o.Length];
			for (int e = 0; e < o.Length; ++e) {
				if (e < Expr.Count) {
					var ee = Expr[e];
					Complex getarg() => ee.Arg < 0 || ee.Arg >= i?.v.Length ? ee.Value : i?.v[ee.Arg] ?? ee.Value;
					result[e] = ee.Op.Op(ee.Negative, ee.Term?.Eval(x).v[0] ?? getarg(), ee.Operand?.Eval(x).v[0] ?? Complex.Zero);
				} else result[e] = Complex.NaN;
			}
			return (o, result);
		}
		public Expression(Comparser context, string text, A arg) {
			var e = new Expression(Context = context, ref text, out _, arg);
			Expr = text == "" ? e.Expr : [new(Complex.NaN, new(), -1)];
		}
		/// <summary>
		/// reads an expression string
		/// </summary>
		/// <param name="context">context that contains custom callable functions</param>
		/// <param name="text">string to parse</param>
		/// <param name="nextOp">returns operand's operator if that operand should be left-associated with my term, will encapsulate previous operator into my term, and use nextOp on next operand</param>
		/// <param name="arg">argument value, will substitute every x in the string</param>
		/// <param name="left">what oreder of operations was my parent's operator? Used to test for associativity</param>
		public Expression(Comparser context, ref string text, out Operator nextOp, A arg, byte left = 0) {
			Context = context;
			Expr = [];
			while(Read(out var t, ref text, out nextOp) && Char(text, ','))
				R(1, ref text);
			return;

			void R(byte c, ref string text) => text = text[c..];
			bool Char(string text, char c) => text.Length > 0 && text[0] == c;
			bool Read(out Input r, ref string text, out Operator nextOp) {
				r = new(Complex.NaN, new(), -1);
				int a = -1;
				Expr.Add(r);
				nextOp = r.Op = new();
				r.Negative = Negative(ref text);
				r.Value = Complex.NaN;
				if (Char(text, '(')) {
					// eat opening parentheses
					R(1, ref text);
					// read parenthesis term, and eat ')'
					if ((Fail((r.Term = new Expression(context, ref text, out _, arg)).Expr[0]) || r.Term.Expr.Count != 1 || Must(')', ref r, ref text)) && F(ref r))
						return false;
				} else if (Func(DefaultFunctions, ref r, ref text) // _term = default function
					&& Func(context.CustomFunctions, ref r, ref text) // _term = custom function
					&& (Number(ref text, out var n) // _value = number
						|| Constant(ref text, DefaultConstants, out n, out a) // _value = constant (pi/tau/e/i)
						|| Constant(ref text, arg, out n, out a, true))) 
							r.Term = new(context, n, a);//r.Value = n; // _value = argument (x/y/z/t...)
				else if (Fail(r) && F(ref r)) // failed to read a term/value
					return false; //  unexpected end fail
				if (text.Length == 0 || text[0] == ')')
					return true; // unexpected ')', or no op, and return back successful
				Operator o = text[0] switch {
					'+' => new Add(),
					'-' => new Sub(),
					'*' => new Mul(),
					'/' => new Div(),
					'^' => new Pow(),
					_ => new Mul(false) // operator-less multiplication
				};
				if (o.EatOp)
					R(1, ref text); // eat operator
				if (o.Right ? o.Order < left : o.Order <= left) {
					nextOp = o; // perform left-associativity by returning back, and the parent will encapsualte
					return false;
				}
				while (true) {
					// read operand
					if (Fail((r.Operand = new Expression(context, ref text, out o, arg, (r.Op = o).Order)).Expr[0]) || r.Operand.Expr.Count != 1) {
						if (r.Op.EatOp && F(ref r))
							return false; // failed to read operand
						// if it was operator-less multiplication - assume it was an expression end instead
						r.Op = new();
						break;
					}
					if (o.Order == 0)
						break;
					// operand's next op has lower or equal order priority:
					// copy myself into my term (wrap my term into parentheses)
					Expr[^1] = r = new(Complex.NaN, new(), -1, new(context, Expr[^1]));
					//exp = [r = new(Complex.NaN, new(), new Expression(this))];
				}
				return true;

				bool Fail(Input test) =>  test.Term == null && test.Value.IsNaN;
				bool F(ref Input r) { // reading failed
					r.Op = new();
					r.Value = Complex.NaN;
					r.Term = r.Operand = null;
					r.Negative = false;
					return true;
				}
				bool Must(char c, ref Input r, ref string text) {
					if (!Char(text, c) && F(ref r)) return true;
					R(1, ref text);
					return false;
				}
				bool Func(CallFunction[] f, ref Input r, ref string text) {
					foreach (var t in f) {
						if (t.Name.Length <= 0 || text.Length <= t.Name.Length || text[0..t.Name.Length] != t.Name) continue;
						R((byte)t.Name.Length, ref text);
						// func opening parenthesis
						if (Must('(', ref r, ref text)) return F(ref r);
						// must eat func closing parenthesis
						return (Fail((r.Term = t.New(context, ref text, arg)).Expr[0]) || Must(')', ref r, ref text)) && F(ref r);
					}
					return true;
				}
				bool Negative(ref string text) {
					if (!Char(text, '-'))
						return false;
					R(1, ref text); // eat minus sign
					return true;
				}
				bool Number(ref string text, out Complex n) {
					if (RealNumber(ref text, out var r)) {
						n = new(r);
						return true;
					}
					n = Complex.NaN;
					return false;
				}
				bool RealNumber(ref string text, out double n, double l = 0) {
					if (text.Length > 0) {
						if (text[0] == '.') {
							// eat decimal point
							R(1, ref text);
							// get fractional part
							n = l + DecimalNumber(ref text);
							return true;
						}
						if (int.TryParse(text[0].ToString(), out int i)) {
							l *= 10;
							// eat another digit
							R(1, ref text);
							// add another whole digit, or finish
							RealNumber(ref text, out n, l + i);// && 1 <= n ? 10 * i + n : i + n;
							return true;
						}
					}
					n = l; // no more digits
					return false;
				}
				double DecimalNumber(ref string text, double d = 1) {
					if (text.Length == 0) return 0; // no more digits
					d /= 10; // prepare another position
					if (!int.TryParse(text[0].ToString(), out int i))
						return 0;
					R(1, ref text);
					return i * d + DecimalNumber(ref text, d);
				}
				bool Constant(ref string text, A a, out Complex n, out int foundArg, bool isArg = false) {
					foundArg = -1;
					for (var f = 0; f < a.k.Length; ++f) {
						var k = a.k[f];
						if (text.Length < k.Length ||
							text[0..k.Length] != k) continue;
						R((byte)k.Length, ref text);
						n = a.v[f];
						if (isArg)
							foundArg = f;
						return true;
					}
					n = Complex.NaN;
					return false;
				}
			}
		}
		/// <summary>
		/// Copies the expression (for encapsulating into parentheses)
		/// </summary>
		/// <param name="copy">Which expression to copy</param>
		private Expression(Expression copy) {
			Context = copy.Context;
			Expr = copy.Expr;
		}
		private Expression(Comparser context, Input t) {
			Context = context;
			Expr = [new Input(t.Value,t.Op, t.Arg, t.Term,t.Operand, t.Negative)];
		}
		private Expression(Comparser context, Complex value, int outArg = -1) {
			Context = context;
			Expr = [new Input(value, new(), outArg)];
		}
	}
}
