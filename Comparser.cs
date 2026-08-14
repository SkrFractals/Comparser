using A = (string[] k, Comparser.Complex[] v);
using I = (Comparser.Argument[] input, string def);

namespace Comparser;

public class Comparser {
	//public static readonly I[] scalar = Array.Empty<(Argument[], string)>();
	//public static readonly string[] x = ["x"];
	//public static readonly string[] xy = ["x", "y"];
	// Only parse teh text into an expression
	public Expression Parse(string text, A arg) => new(this, text.Replace(" ", ""), arg);
	// Re-evaluates already parsed expression with new arguments
	public A Eval(Expression exp, A arg) => exp.Eval([], arg);
	// Parses and evaluates the text with selected arguments (and returns the expression for possible re-evaluation)
	public A ParseEval(string text, out Expression expr, A arg) => (expr = Parse(text, arg)).Eval(x);
	// Parses and evaluates the text with selected arguments (without returning the parsed expression, only immediate one-time evaluation)
	public A ParseEval(string text, A arg) => Parse(text, arg).Eval([]); 
	public CallFunction[] CustomFunctions = []; // user-defined functions
	public A CustomConstants = ([], []); // user-defined constants
	// If any name A is contained within a name B, make sure name B comes first. For example acosh before cos.
	// Otherwise this would need to be sorted decrementally by length at runtime.
	public static CallDel factorial = new("factorial", Complex.Factorial);
	private static readonly CallFunction[] DefaultFunctions = [

		// double arguments:
		new CallExp("minimum", typeof(Min),0),	// component-wise minimum: min(1+4i,3+2i)=1+2i
		new CallExp("maximum", typeof(Max),0),	// component-wise maximum
		new CallExp("min", typeof(Min),0),		// component-wise minimum: min(1+4i,3+2i)=1+2i
		new CallExp("max", typeof(Max),0),		// component-wise maximum
		new CallExp("clamp", typeof(Clamp)),	// component-wise clamp: clamp(5+4i,3+6i,4+7i)=4+6i
		new CallExp("vectorclamp", typeof(VectorClamp), 0),	// component-wise vector clamp: clamp((5+4i,2+8i),3+6i,4+7i)=(4+6i,3+7i)

		// quadruple arguments
		new CallExp("product", typeof(Product), 0),	// iterative product: product(<index>,from,to,expression(k<index>)); Ex: product(0,1,4,k0) = 1*2*3*4 = 24
		new CallExp("prod", typeof(Product), 0),	// iterative product
		new CallExp("sum", typeof(Sum), 0),			// iterative sum: sum(<index>,from,to,expression(k<index>)); Ex: sum(0,1,4,k0) = 1+2+3+4 = 10
		new CallExp("vector", typeof(Vector), 0),	// iterative vector builder: vector(<index>,from,to,expression(k<index>)); Ex: vector(0,1,3,2^k0) = (2,4,8)
		new CallExp("vec", typeof(Vector), 0),		// iterative vector builder

		// exp/log
		new CallExp("exp10", typeof(Exp10)),	// Decimal exponential: 10^x
		new CallExp("exp2", typeof(Exp2)),		// Binary exponential: 2^x
		new CallDel("exp", Complex.Exp),		// Exponential: e^x = (cos(b)+isin(b))e^a
		new CallExp("log10", typeof(Log10)),	// Decimal logarithm: log_10(x) = ln(x)/ln(2)
		new CallExp("log2", typeof(Log2)),		// Binary logarithm: log_2(x) = ln(x)/ln(10)
		new CallDel("log", Complex.Log),		// Natural logarithm: ln(x)
		new CallDel("ln", Complex.Log),			// Natural logarithm: ln(x)

		// sincs
		new CallDel("nsinhc", Complex.Nsinhc),	// sinhc(x*pi)
		new CallDel("sinchpi", Complex.Nsinhc),	// sinhc(x*pi)
		new CallDel("sinhc", Complex.Sinhc),	// sinhc(x) = sinh(x)/x
		new CallDel("nsinc", Complex.Nsinc),	// sinc(x*pi)
		new CallDel("sincpi", Complex.Nsinc),	// sinc(x*pi)
		new CallDel("sinc", Complex.Sinc),		// sinc(x) = sin(x)/x
		new CallDel("sinc", Complex.Cosc),		// cosc(x) = (1-cos(x))/x

		// arc hyperbolics
		new CallDel("acosh", Complex.Acosh),	// acosh(x)
		new CallDel("asinh", Complex.Asinh),	// asinh(x)
		new CallDel("atanh", Complex.Atanh),	// atanh(x)
		new CallDel("asech", Complex.Asech),	// asech(x)
		new CallDel("acsch", Complex.Acsch),	// acsch(x)
		new CallDel("acoth", Complex.Acoth),	// acoth(x)

		// hyperbolics
		new CallDel("cosh", Complex.Cosh),		// cosh(x)
		new CallDel("sinh", Complex.Sinh),		// sinh(x)
		new CallDel("tanh", Complex.Tanh),		// tanh(x)
		new CallDel("sech", Complex.Sech),		// sech(x)
		new CallDel("csch", Complex.Csch),		// csch(x)
		new CallDel("coth", Complex.Coth),		// coth(x)

		// arc trigs
		new CallDel("acos", Complex.Acos),		// acos(x)
		new CallDel("asin", Complex.Asin),		// asin(x)
		new CallDel("atan", Complex.Atan),		// atan(x)
		new CallDel("asec", Complex.Asec),		// asec(x)
		new CallDel("acsc", Complex.Acsc),		// acsc(x)
		new CallDel("acot", Complex.Acot),		// acot(x)

		// trigs
		new CallDel("cos", Complex.Cos),		// cos(x)
		new CallDel("sin", Complex.Sin),		// sin(x)
		new CallDel("tan", Complex.Tan),		// tan(x)
		new CallDel("sec", Complex.Sec),		// sec(x)
		new CallDel("csc", Complex.Csc),		// csc(x)
		new CallDel("cot", Complex.Cot),		// cot(x)

		// basics
		new CallExp("real", typeof(Re)),		// Real part: Re(x) = a
		new CallExp("imag", typeof(Im)),		// Imaginary part: Im(x) = b
		new CallExp("re", typeof(Re)),			// Real part: Re(x) = a
		new CallExp("im", typeof(Im)),			// Imaginary part: Im(x) = b
		new CallExp("r", typeof(Re)),			// Real part: Re(x) = a
		new CallExp("i", typeof(Im)),			// Imaginary part: Im(x) = b
		new CallDel("frac", Complex.Frac),		// Signed fractional part: Frac(x) = x - Trunc(x)
		new CallDel("trunc", Complex.Trunc),	// Whole part: Truncate(x)
		new CallDel("floor", Complex.Floor),	// Round down: Floor(x)
		new CallDel("round", Complex.Round),	// Round near: Round(x)
		new CallDel("ceil", Complex.Ceil),		// Round up: Ceiling(x)
		new CallDel("sign", Complex.Sign),		// Sign(x) = x / |x|
		new CallDel("sgn", Complex.Sign),		// Sign(x) = x / |x|
		new CallExp("negative", typeof(Neg)),	// Negation: -x
		new CallExp("neg", typeof(Neg)),		// Negation: -x
		new CallDel("inverse", Complex.Inv),	// Inverse: 1/x
		new CallDel("inv", Complex.Inv),		// Inverse: 1/x
		new CallDel("absri", Complex.AbsRI),	// Positive components: |a|+|b|i
		new CallExp("sqrabs", typeof(Sqrabs)),	// Squared absolute value: |x|^2 = a*a + b*b
		new CallExp("absolute", typeof(Abs)),	// Absolute value: |x| = sqrt(a*a + b*b)
		new CallExp("abs", typeof(Abs)),		// Absolute value: |x| = sqrt(a*a + b*b)
		new CallExp("arg", typeof(Arg)),		// Argument: Arg(x)
		new CallExp("conjugate", typeof(Conj)),	// Conjugate: Conj(x) = a - bi
		new CallExp("conj", typeof(Conj)),		// Conjugate: Conj(x) = a - bi

		// powers
		new CallDel("sqrt", Complex.Sqrt),		// Square root: Sqrt(x)
		new CallExp("cbrt", typeof(Cbrt)),		// Cube root: Cbrt(x)
		new CallDel("sqr", Complex.Sqr),		// Square: x*x
		new CallDel("cube", Complex.Cub),		// Cube: x*x*x
		new CallDel("cub", Complex.Cub),		// Cube: x*x*x
		new CallDel("quart", Complex.Quart),	// Quart: x*x*x*x

		// specials
		new CallExp("gauss", typeof(Gauss)),			// e^(-x^2)
		new CallExp("softplus1", typeof(Sftadd1)),		// ln(1+e^x)
		new CallExp("softplus", typeof(Sftadd)),		// ln(1+e^x)
		factorial,	// x!
		new CallDel("gamma", Complex.Gamma_Stirling),	// gamma(x)
		new CallDel("zeta", Complex.Zeta)				// zeta(x)
		];
	private static readonly A DefaultConstants = (
		[	    "pi",		"tau",		 "e",		"i",	   "one"], 
		[Complex.pi, Complex.tau, Complex.e, Complex.i, Complex.One]);

	#region Call Functions
	// abstract parent
	public abstract class CallFunction(string name, int _cache = 1/*, I[]? def = null*/) {
		//protected readonly I[]? Def = def;
		public readonly string Name = name;
		public readonly EvalCache Cache = new(_cache);
		public abstract Expression Call(Comparser context, ref string text, A arg);
	}
	protected abstract class FunctionExpression : Expression {
		protected readonly CallFunction Parent;
		public FunctionExpression(Comparser context, CallFunction parent, ref string text, A arg) : base(context, ref text, out _, arg) => Parent = parent;
		public FunctionExpression(Comparser context, CallFunction parent, Input input) : base(context, input) => Parent = parent;
		public override A Eval(string[] o, A? i = null) {
			var (_, v) = base.Eval([], i);
			return (o, Parent.Cache.Insert(v, Parent.Cache.GetEval(v) ? Parent.Cache.result?.Eval! : Eval(v, i)));
		}
		protected abstract Complex[] EvalF(Complex[] v, A? i = null);
	}
	// how to use: e.Insert(args, e.GetEval(args) ? e.result.Eval : base.Eval([], args).v); 
	public struct EvalCache(int size = 1) {
		private readonly int Size = size;
		public int filled = 0;
		public Evaluated? cache = null;
		public Evaluated? result;
		public bool GetEval(Complex[] args) {
			result = cache;
			for (var c = cache; c != null; result = c, c = c.Next) {
				if (c.Args.Length != args.Length) {
					if (c.Next == null) break; continue; // argument count not matching
				}
				var m = true;
				for (var i = 0; i < args.Length; ++i)
					m &= args[i].R == c.Args[i].R && args[i].I == c.Args[i].I;
				if (m) {
					if (result != c) {
						result!.Next = c.Next;
						c.Next = cache;
						cache = c;
					}
					result = c;
					return true;
				}
				if (c.Next == null) break;
			}
			return false;
		}
		public Complex[] Insert(Complex[] args, Complex[] eval) {
			if (Size <= filled && result != null)
				result.Next = null;
			if (Size > 0)
				cache = new Evaluated(cache, args, eval);
			return eval;
		}
		public void Reset() { filled = 0; cache = null; }
	}
	public class Evaluated(Evaluated? next, Complex[] args, Complex[] eval) {
		public Complex[] Eval = eval;
		public Complex[] Args = args;
		public Evaluated? Next = next;

	}
	// Expressions.Functions:
	private class CallExp(string name, Type type, int _cache = 1) : CallFunction(name, _cache) {
		public override Expression Call(Comparser context, ref string text, A arg) {
			object[] a = [context, this, text, arg]; // activator arguments
			var n = (FunctionExpression)Activator.CreateInstance(type, a)!;
			text = (string)a[1]; // ref string text
			return n;
		}
	}
	// Single argument delegated functions
	public class CallDel(string name, Func<Complex, Complex> del) : CallFunction(name) {
		public override Expression Call(Comparser context, ref string text, A arg)
			=> new Del(context, this, del, ref text, arg);
	}
	private class Del(Comparser context, CallFunction parent, Func<Complex, Complex> del, ref string text, A arg) : FunctionExpression(context, parent, ref text, arg) {
		protected override Complex[] EvalF(Complex[] v, A? i = null) {
			Complex[] result = new Complex[v.Length];
			for (int j = 0; j < v.Length; ++j)
				result[j] = v[j].IsNaN ? v[j] : del(v[j]);
			return result;
		}
	}
	// Iterative single argument delegated functions (gamma, zeta, etc)
	/*public class CallDelI(string name, Func<Complex, int, Complex> del) : CallFunction(name, scalar) {
		public override Expression Call(Comparser context, ref string text, A arg)
			=> new DelI(context, del, ref text, arg);
	}
	private class DelI(Comparser context, Func<Complex, int, Complex> del, ref string text, A arg) : Expression(context, ref text, out _, arg) {
		public override A Eval(string[] o, A? i = null) {
			var args = base.Eval(scalar[0].input, i).v;
			return (o, [del(args[0], (int)args[1].R)]);
		}
	}*/
	// iterative sum/product: name(<index>,<from>,<to>,expression(k<index>))
	// "to" can be smaller than "from", works both ways (does not return additive/multiplicative identity when in the wrong order, just iterates backwards)
	private abstract class Iterator(Comparser context, CallFunction parent, ref string text, A arg) : FunctionExpression(context, parent, ref text, arg) {
		protected override Complex[] EvalF(Complex[] v, A? i = null) {
			int iterator = (int)v[0].R, from = (int)v[1].R, to = (int)v[2].R;
			A ni; int iteratorIndex;
			if (i is A a) {
				iteratorIndex = a.v.Length;
				ni = new(new string[iteratorIndex + 1], new Complex[iteratorIndex + 1]);
				Array.Copy(a.k, ni.k, iteratorIndex);
				Array.Copy(a.k, ni.k, iteratorIndex);
				ni.k[iteratorIndex] = "k" + iterator.ToString();
			} else { 
				iteratorIndex = 0;
				ni = new(["k"+iterator.ToString()], new Complex[1]);
			}
			Complex[] evalK(int f) {
				ni.v[iteratorIndex] = new(f);
				return EvalMulti(3, ni);
			}
			return Result(evalK, from, to);
		}
		protected virtual void Op(ref Complex result, Complex iteration) => result = iteration;
		protected abstract Complex[] Result(Func<int, Complex[]> eval, int from, int to);
		protected static void Iterate(Action<int> iter, int from, int to) {
			// add the other iterations all the way to "to"
			while (from < to)
				iter(++from);
			while (from > to)
				iter(--from);
		}
	}
	private abstract class CollapseIterator(Comparser context, CallFunction parent, ref string text, A arg) : Iterator(context, parent, ref text, arg) { 
		protected override Complex[] Result(Func<int, Complex[]> eval, int from, int to) {
			var sum = eval(from); // prepare first iteration as the initial vector
			void iterK(int f) {
				var v = eval(f);
				for (var j = v.Length; 0 <= --j; Op(ref sum[j], v[j])) { }
			}
			Iterate(iterK, from, to);
			return sum;
		}
	}
	// return a vector of sums of iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (x,2x); sum(0,1,3,exp(k0)) => (1+2+3,2+4+6) => (6,12); // 6 is the sum of x term, evaluated with k0=1..3, 12 is the sum of 2x term, evaluated with k0=1..3
	private class Sum(Comparser context, CallFunction parent, ref string text, A arg) : CollapseIterator(context, parent, ref text, arg) { 
		protected override void Op(ref Complex result, Complex iteration) => result += iteration;
	}
	// return a vector of products of iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (x,2x); prod(0,1,3,exp(k0)) => (1*2*3,2*4*6) => (6,48); // 6 is the product of x term, evaluated with k0=1..3, 48 is the product of 2x term, evaluated with k0=1..3
	private class Product(Comparser context, CallFunction parent, ref string text, A arg) : CollapseIterator(context, parent, ref text, arg) {
		protected override void Op(ref Complex result, Complex iteration) => result *= iteration;
	}
	// returns a vector of first elements of evaluated iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (3x,2x,4x); vector(0,1,3,exp(k0)) => (3*1,3*2,3*3) => (3,6,9); // only took the first 3x term, evaluated with k0=1..3
	private class Vector(Comparser context, CallFunction parent, ref string text, A arg) : Iterator(context, parent, ref text, arg) {
		protected override Complex[] Result(Func<int, Complex[]> eval, int from, int to) { 
			var size = Math.Abs(from - to);
			var sum = new Complex[size];
			sum[0] = eval(from)[0];
			var iteratorIndex = 0;
			void iterK(int f) => Op(ref sum[++iteratorIndex], eval(f).v[0]);
			Iterate(iterK, from, to);
			return sum;
		}
	}
	// User defined custom expression functions
	public class CallCustom : CallFunction {
		public CallCustom(Comparser context, string name, I[] def, int _cache = 1) : base(name, _cache) {
			Parsed = new Expression[(Def = def).Length];
			for (var p = 0; p < def.Length; ++p)
				Parsed[p] = context.Parse(def[p].def, Argument.ToArgs(def[p].input));
		}
		public readonly I[] Def;
		public readonly Expression[] Parsed;
		public override Expression Call(Comparser context, ref string text, A arg) 
			=> new CustomFunc(context, this, ref text, arg);
	}
	private class CustomFunc(Comparser context, CallCustom parent, ref string text, A arg) : FunctionExpression(context, parent, ref text, arg) {
		protected override Complex[] EvalF(Complex[] v, A? i = null) {
			var match = -1;
			for (var m = 0; m < parent.Def.Length; ++m) {
				var ok = true;
				for (var id = 0; id < parent.Def[m].input.Length; ++id)
					ok &= parent.Def[m].input[id].Match(v[id]);
				if (ok) { match = m; break; }
			}
			if (match == -1)
				return []; // failed to match any available argument list
			return Context.Eval(parent.Parsed[match], ([], v)).v;
		}
	}
	#endregion

	#region Function Implementations
	static readonly double ln10 = Math.Log(10);
	static readonly double ln2 = Math.Log(2);

	protected class Fact(Comparser context, Expression.Input input) : FunctionExpression(context, factorial, input) { // factorial called by !
		protected override Complex[] EvalF(Complex[] v, A? i = null) {
			Complex[] result = new Complex[v.Length];
			for (int j = 0; j < v.Length; ++j)
				result[j] = v[j].IsNaN ? v[j] : Complex.Factorial(v[j]);
			return result;
		}
	}
	private class Compare(Comparser context, CallFunction parent, Func<double, double, double> comp, ref string text, A arg) : FunctionExpression(context, parent, ref text, arg) {
		protected override Complex[] EvalF(Complex[] v, A? i = null) { 
			var m = v[0];
			for (var c = 1; c < v.Length; ++c)
				m = new(double.IsNaN(m.R) ? v[c].R : comp(m.R, v[c].R), double.IsNaN(m.I) ? v[c].I : comp(m.I, v[c].I));
			return [m];
		}
	}
	private class Max(Comparser context, CallFunction parent, ref string text, A arg) : Compare(context, parent, Math.Max, ref text, arg) { }
	private class Min(Comparser context, CallFunction parent, ref string text, A arg) : Compare(context, parent, Math.Min, ref text, arg) { }
	private class FuncUnOperator(Comparser context, CallFunction parent, Func<Complex, Complex> comp, Complex pre, Complex post, ref string text, A arg) : FunctionExpression(context, parent, ref text, arg) {
		protected override Complex[] EvalF(Complex[] v, A? i = null) {
			var m = new Complex[v.Length];
			for (var c = 0; c < v.Length; ++c)
				m[c] = v[c].IsNaN ? v[c] : comp(pre * v[c]) * post;
			return m;
		}
	}
	private class FuncCompOperator(Comparser context, CallFunction parent, Func<double, double> comp, ref string text, A arg) : FunctionExpression(context, parent, ref text, arg) {
		protected override Complex[] EvalF(Complex[] v, A? i = null) {
			var m = new Complex[v.Length];
			for (var c = 0; c < v.Length; ++c)
				m[c] = new(double.IsNaN(v[c].R) ? double.NaN : comp(v[c].R), double.IsNaN(v[c].I) ? double.NaN : comp(v[c].I));
			return m;
		}
	}
	
	private class Exp10(Comparser context, CallFunction parent, ref string text, A arg)	: FuncUnOperator(context, parent, Complex.Exp, new(ln10), new(1), ref text, arg) { }
	private class Exp2(Comparser context, CallFunction parent, ref string text, A arg)	: FuncUnOperator(context, parent, Complex.Exp, new(ln2), new (1), ref text, arg) { }
	private class Log10(Comparser context, CallFunction parent, ref string text, A arg)	: FuncUnOperator(context, parent, Complex.Log, new(1), new(1/ln10), ref text, arg) { }
	private class Log2(Comparser context, CallFunction parent, ref string text, A arg)	: FuncUnOperator(context, parent, Complex.Log, new(1), new(1 / ln2), ref text, arg) { }
	private class Re(Comparser context, CallFunction parent, ref string text, A arg)	: FuncUnOperator(context, parent, (x) => new(x.R), new(1), new(1 / ln2), ref text, arg) { }
	private class Im(Comparser context, CallFunction parent, ref string text, A arg)	: FuncUnOperator(context, parent, (x) => new(x.I), new(1), new(1 / ln2), ref text, arg) { }
	private class Neg(Comparser context, CallFunction parent, ref string text, A arg)	: FuncUnOperator(context, parent, (x) => -x, new(1), new(1), ref text, arg) { }
	private class Sqrabs(Comparser context, CallFunction parent, ref string text, A arg) : FuncUnOperator(context, parent, (x) => new(+x), new(1), new(1), ref text, arg) { }
	private class Abs(Comparser context, CallFunction parent, ref string text, A arg)	: FuncUnOperator(context, parent, (x) => new(Complex.Abs(x)), new(1), new(1), ref text, arg) { }
	private class Arg(Comparser context, CallFunction parent, ref string text, A arg)	: FuncUnOperator(context, parent, (x) => new(Complex.Arg(x)), new(1), new(1), ref text, arg) { }
	private class Conj(Comparser context, CallFunction parent, ref string text, A arg)	: FuncUnOperator(context, parent, (x) => !x, new(1), new(1), ref text, arg) { }
	private class Cbrt(Comparser context, CallFunction parent, ref string text, A arg)	: FuncUnOperator(context, parent, (x) => x ^ 1.0/3, new(1), new(1), ref text, arg) { }
	//private class Cub(Comparser context, CallFunction parent, ref string text, A arg)	: FuncUnOperator(context, parent, Complex.Cub, new(1), new(1), ref text, arg) { }
	private class Gauss(Comparser context, CallFunction parent, ref string text, A arg)	: FuncUnOperator(context, parent, (x) => Complex.Exp(-Complex.Sqr(x)), new(1), new(1), ref text, arg) { }
	private class Sftadd1(Comparser context, CallFunction parent, ref string text, A arg) : FuncUnOperator(context, parent, (x) => Complex.Log(1 + Complex.Exp(x)), new(1), new(1), ref text, arg) { }
	private class Sftadd(Comparser context, CallFunction parent, ref string text, A arg) : FunctionExpression(context, parent, ref text, arg) {
		protected override Complex[] EvalF(Complex[] v, A? i = null) { var m = v[0]; for (var c = 1; c < v.Length; ++c) m += Math.Exp(v[c].R); return [Complex.Log(m)]; } 
	}
	private class Clamp(Comparser context, CallFunction parent, ref string text, A arg) : FunctionExpression(context, parent, ref text, arg) {
		protected override Complex[] EvalF(Complex[] v, A? i = null) => v.Length == 3 ? [new Complex(Math.Clamp(v[0].R, v[1].R, v[2].R), Math.Clamp(v[0].I, v[1].I, v[2].I))] : [Complex.NaN];
	}
	private class VectorClamp(Comparser context, CallFunction parent, ref string text, A arg) : FunctionExpression(context, parent, ref text, arg) {
		protected override Complex[] EvalF(Complex[] v, A? i = null) {
			if (v.Length != 3) return [Complex.NaN];
			var m = new Complex[v.Length];
			var multi = EvalMulti(0, i);
			for (var c = 0; c < v.Length; ++c)
				m[c] = new Complex(Math.Clamp(multi[c].R, v[1].R, v[2].R), Math.Clamp(multi[c].I, v[1].I, v[2].I));
			return m;
		}
	}
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
	private class Mod() : Operator(2) {
		public override Complex Op(bool neg, Complex value, Complex operand) 
			=> new(operand.R == 0 ? double.NaN : (neg ? -value.R : value.R) % operand.R, operand.I == 0 ? double.NaN : (neg ? -value.I : value.I) % operand.I);
	}
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
	private class Factorial() : Operator(5) {
		public override Complex Op(bool neg, Complex value, Complex operand) => neg ? -Complex.Factorial(value) : Complex.Factorial(value);
	}
	#endregion

	public class Expression {
		public class Input(Complex value, Operator op, int arg, Expression? term = null, Expression? operand = null, bool negative = false) {
			// unary negative flag
			public bool Negative = negative;
			// simple value if it is only a number not containing an expression term (can remain present even if it is replaced with a term)
			public Complex Value = value;
			// main term and operand (second term)
			public Expression? Term = term, Operand = operand;
			// operator (eval = term <operator> operand), if it is a pure parent Operator, it only evaluates the term
			public Operator Op = op;
			public int Arg = arg;
			public string Text = ""; // The original text that this input has been parsed from, even if it fails parsing
		}
		// contains user-defined custom function
		protected readonly Comparser Context;
		private readonly List<Input> Expr;
		private readonly EvalCache cache;
		/// <summary>
		/// Evaluates the expression
		/// </summary>
		/// <returns>Evaluated value of this expression</returns>
		public virtual A Eval(string[] returnKeys, A? i = null) {
			var v = i?.v ?? [];
			if (cache.GetEval(v))
				return (returnKeys, cache.Insert(v, cache.result?.Eval!));
			if (Expr.Count > returnKeys.Length) {
				var _o = new string[Expr.Count];
				for (var e = 0; e < returnKeys.Length; ++e)
					_o[e] = returnKeys[e];
				for (var e = returnKeys.Length; e < Expr.Count; ++e)
					_o[e] = "_"; // has more expressions that it was asked to associate with return arguments.
				returnKeys = _o;
			}
			var result = new Complex[returnKeys.Length];
			for (var e = 0; e < returnKeys.Length; ++e) 
				result[e] = EvalSingle(e, i);
			return (returnKeys, cache.Insert(v, result));
		}
		protected Complex EvalSingle(int arrayIndex = 0, A? i = null) {
			if (arrayIndex < Expr.Count) {
				var ee = Expr[arrayIndex];
				Complex getarg() => ee.Arg < 0 || ee.Arg >= i?.v.Length ? ee.Value : i?.v[ee.Arg] ?? ee.Value;
				// operators only expect scalar values, they will ignore values with index > 0
				return ee.Op.Op(ee.Negative, ee.Term?.Eval([],i).v[0] ?? getarg(), ee.Operand?.Eval([],i).v[0] ?? Complex.NaN);
			} else return  Complex.NaN;
		}
		protected Complex[] EvalMulti(int arrayIndex = 0, A? i = null) {
			if (arrayIndex < Expr.Count) {
				var ee = Expr[arrayIndex];
				Complex getarg() => ee.Arg < 0 || ee.Arg >= i?.v.Length ? ee.Value : i?.v[ee.Arg] ?? ee.Value;
				Complex? getJ(Complex[]? v, int j) => v?.Length < j ? v[j] : null;
				// operators only expect scalar values, they will ignore values with index > 0
				A? t = ee.Term?.Eval([], i), o = ee.Operand?.Eval([], i);
				Complex[] result = new Complex[t?.v.Length ?? 1];
				for(int j = 0; j < result.Length; ++j)
					result[j] = ee.Op.Op(ee.Negative, getJ(ee.Term?.Eval([], i).v, j) ?? getarg(), getJ(ee.Operand?.Eval([], i).v,j) ?? Complex.NaN);
				return result;
			} else return [];
		}
		public Expression(Comparser context, string text, A arg, int _cache = 0) {
			var e = new Expression(Context = context, ref text, out _, arg, _cache);
			Expr = text == "" ? e.Expr : [new(Complex.NaN, new(), -1)];
			cache = new(_cache);
		}
		/// <summary>
		/// reads an expression string
		/// </summary>
		/// <param name="context">context that contains custom callable functions</param>
		/// <param name="text">string to parse</param>
		/// <param name="nextOp">returns operand's operator if that operand should be left-associated with my term, will encapsulate previous operator into my term, and use nextOp on next operand</param>
		/// <param name="arg">argument value, will substitute every x in the string</param>
		/// <param name="left">what oreder of operations was my parent's operator? Used to test for associativity</param>
		public Expression(Comparser context, ref string text, out Operator nextOp, A arg, int _cache = 0, byte left = 0) {
			cache = new(_cache);
			Context = context;
			Expr = [];
			while(Read(out var t, ref text, out nextOp) && left == 0 && Char(text, ',')) // only left == 0 (aka top layer expression) should accept ',' for a next value
				R(1, ref text);
			return;

			bool Read(out Input r, ref string text, out Operator nextOp) {
				// Init read
				r = new(Complex.NaN, new(), -1);
				var a = -1;
				Expr.Add(r);
				nextOp = r.Op = new();
				r.Negative = Negative(ref text, ref r);
				r.Value = Complex.NaN;
				// Try parenthesis/function/number/constant/argument:
				if ((!Char(text, '(', true) || (Fail((r.Term = new Expression(context, ref text, out _, arg)).Expr[0]) || r.Term.Expr.Count != 1 || FailRequiredSymbol(')', ref r, ref text)) && F(ref r))
					&& Func(DefaultFunctions, ref r, ref text) // _term = default function
					&& Func(context.CustomFunctions, ref r, ref text) // _term = custom function
					&& (Number(ref text, ref r, out var n) // _value = number
						|| Constant(ref text, ref r, DefaultConstants, out n, out a) // _value = constant (pi/tau/e/i)
						|| Constant(ref text, ref r, context.CustomConstants, out n, out a) // _value = user constant
						|| Constant(ref text, ref r, arg, out n, out a, true)))
					r.Term = new(context, n, _cache, a);//r.Value = n; // _value = argument (x/y/z/t...)
				else if (Fail(r) && F(ref r)) // failed to read a term/value
					return false; //  unexpected end fail
				if (End(text))
					return true; // unexpected ')', or no op, and return back successful
				// Read operators/comments:
				Operator o;
				while ((o = text[0] switch { '+' => new Add(), '-' => new Sub(), '*' => new Mul(), '/' => new Div(), '^' => new Pow(), '%' => new Mod(), '!' => new Factorial(), _ => new Mul(false) }).GetType() switch {
					var x when x == typeof(Factorial) => Encapsulate(new Fact(context, Expr[^1]), out r), // factorial
					var x when x == typeof(Div) => Comment(ref text, ref r, ref o), // comment
					_ => false
				}) if (o.Order == 0) return true;
				if (o.EatOp) // not an operator-less multiplication?
					RP(1, ref r, ref text); // eat operator
				if (LeftAssociate(o)) {
					nextOp = o; // perform left-associativity by returning back, and the parent will encapsualte
					return false;
				}
				// Read operand:
				while (true) {
					if (Fail((r.Operand = new Expression(context, ref text, out o, arg, _cache, (r.Op = o).Order)).Expr[0]) || r.Operand.Expr.Count != 1) {
						if (r.Op.EatOp && F(ref r))
							return false; // failed to read operand
						// if it was operator-less multiplication - assume it was an expression end instead
						r.Op = new();
						break;
					}
					if (o.Order == 0)
						break;
					// operand's next op has lower or equal order priority:
					// encapsulate my term into another term (wrap my term into parentheses), take the next operator and find the next operand to use it on
					_ = Encapsulate(new(context, Expr[^1], _cache), out r);
					if (LeftAssociate(o)) { // need to test associativity again, to let it recurse backwards. othewise 2^2^2+1 woudl be 2^(2^2+1)
						nextOp = o; // perform left-associativity by returning back, and the parent will encapsualte
						return false;
					}
				}
				return true;

				bool Comment(ref string text, ref Input r, ref Operator o) {
					var go = true;
					while (go) {
						var i = text.IndexOf('/');
						if (i < 0) {
							text = "";
							o = new();
							return true;
						}
						go = text[i - 1] != '*';
						RP((byte)(i + 1), ref r, ref text);
					}
					return true;
				}
				bool End(string text) => text.Length == 0 || text[0] == ')' || text[0] == ',';
				bool Encapsulate(Expression p, out Input r) { Expr[^1] = r = new(Complex.NaN, new(), -1, p); return true; }
				bool LeftAssociate(Operator testop) => testop.Right ? testop.Order < left : testop.Order <= left;
				bool Fail(Input test) => test.Term == null;// && test.Value.IsNaN; // no longer needed as even values are now nested in terms, and i don't test their insides.
				bool F(ref Input r) { // reading failed
					r.Op = new();
					r.Value = Complex.NaN;
					r.Term = r.Operand = null;
					r.Negative = false;
					return true;
				}
				bool FailRequiredSymbol(char c, ref Input r, ref string text) {
					if (!Char(text, c) && F(ref r)) return true;
					RP(1, ref r, ref text);
					return false;
				}
				bool Func(CallFunction[] f, ref Input r, ref string text) {
					foreach (var t in f) {
						if (t.Name.Length <= 0 || text.Length <= t.Name.Length || text[0..t.Name.Length] != t.Name) continue;
						// func opening parenthesis
						if (FailRequiredSymbol('(', ref r, ref text)) continue; // no argument parentheses found, maybe it's a constant with the same name...?
						RP((byte)t.Name.Length, ref r, ref text);
						// must eat func closing parenthesis
						return (Fail((r.Term = t.Call(context, ref text, arg)).Expr[0]) || FailRequiredSymbol(')', ref r, ref text)) && F(ref r);
					}
					return true;
				}
				bool Negative(ref string text, ref Input r) {
					if (!Char(text, '-'))
						return false;
					RP(1, ref r, ref text); // eat minus sign
					return true;
				}
				bool Number(ref string text, ref Input r, out Complex n) {
					if (RealNumber(ref text, ref r, out var real)) {
						n = new(real);
						return true;
					}
					n = Complex.NaN;
					return false;
				}
				bool RealNumber(ref string text, ref Input r, out double n, double l = 0) {
					if (text.Length > 0) {
						if (text[0] == '.') {
							// eat decimal point
							RP(1, ref r, ref text);
							// get fractional part
							n = l + DecimalNumber(ref text, ref r);
							return true;
						}
						if (int.TryParse(text[0].ToString(), out var i)) {
							l *= 10;
							// eat another digit
							RP(1, ref r, ref text);
							// add another whole digit, or finish
							_ = RealNumber(ref text, ref r, out n, l + i);// && 1 <= n ? 10 * i + n : i + n;
							return true;
						}
					}
					n = l; // no more digits
					return false;
				}
				double DecimalNumber(ref string text, ref Input r, double d = 1) {
					if (text.Length == 0) return 0; // no more digits
					d /= 10; // prepare another position
					if (!int.TryParse(text[0].ToString(), out var i))
						return 0;
					RP(1, ref r, ref text);
					return i * d + DecimalNumber(ref text, ref r, d);
				}
				bool Constant(ref string text, ref Input r, A a, out Complex n, out int foundArg, bool isArg = false) {
					// WARNING, if there is any function with the same name, then you can't operator-less multiply with parentheses from the right!
					// for example gamma is either eulerContant or the gamma function:
					// gamma2 = eulerConstant*2, gamma(2+1) = evaluates gamma function at 2, (2+1)gamma = (2+1)*eulerConstant
					foundArg = -1;
					for (var f = 0; f < a.k.Length; ++f) {
						var k = a.k[f];
						if (text.Length < k.Length ||
							text[0..k.Length] != k) continue;
						RP((byte)k.Length, ref r, ref text);
						n = a.v[f];
						if (isArg)
							foundArg = f;
						return true;
					}
					n = Complex.NaN;
					return false;
				}
				void RP(byte c, ref Input r, ref string text) {
					r.Text += text[..c];
					R(c, ref text);
				}
			}
			void R(byte c, ref string text) => text = text[c..];
			bool Char(string text, char c, bool eat = false) { var r = text.Length > 0 && text[0] == c; if (eat) R(1, ref text); return r; }
			}
		protected Expression(Comparser context, Input t, int _cache = 0) {
			cache = new(_cache);
			Context = context;
			Expr = [new Input(t.Value,t.Op, t.Arg, t.Term,t.Operand, t.Negative)];
		}
		private Expression(Comparser context, Complex value, int _cache = 0, int outArg = -1) {
			Context = context;
			cache = new(_cache);
			Expr = [new Input(value, new(), outArg)];
		}
	}
}
public struct Argument(string imput, Complex value) {
	public string Input = imput;
	public Complex Value = value;
	public bool Match(Complex v) => Value.IsNaN || Value.R == v.R && Value.I == v.I;
	public static A ToArgs(Argument[] args) {
		A a = (new string[args.Length], new Complex[args.Length]);
		for (var i = 0; i < args.Length; ++i) {
			a.k[i] = args[i].Input;
			a.v[i] = args[i].Value;
		}
		return a;
	}
}