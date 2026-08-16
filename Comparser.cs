
//using I = (Comparser.Comparser<T>.Input input, Comparser.Comparser.Expression def);

using Comparser.Numbers;
using System.CodeDom;
using System.Text.RegularExpressions;
namespace Comparser;
public interface IComparser {
	public abstract object Parse(string text, object? args = null);
	// Re-evaluates already parsed expression with new arguments
	public abstract object Eval(object exp, object? args = null);
	// Parses and evaluates the text with selected arguments (and returns the expression for possible re-evaluation)
	public abstract object ParseEval(string text, out object expr, object? args = null);
	// Parses and evaluates the text with selected arguments (and returns the expression for possible re-evaluation, and eats the parsed part, allows it to be incomplete - leaving the remainder in the ref text)
	public abstract object ParseEval(ref string text, object? args = null);
	// Parses and evaluates the text with selected arguments (without returning the parsed expression, only immediate one-time evaluation)
	public abstract object ParseEval(string text, object? args = null);
	public abstract string ReadCode(string text);
}

public abstract class Comparser<T>(ushort stackOverflowLimit = 9999) : IComparser where T : INumber<T> {

	#region Interface
	private static Value AsInput(object? e) => e as Value ?? new Value();
	public object Parse(string text, object? args) => new Expression(this, text, AsInput(args), 0);
	public object Eval(object exp, object? args) => (exp as Expression ?? new Expression(this, "", new(), 0)).Eval(AsInput(args));
	public object ParseEval(string text, out object expr, object? args) { var e = (Expression)Parse(text, args); expr = e; return e.Eval(AsInput(args)); }
	public object ParseEval(ref string text, object? args) => new Expression(this, ref text, out _, AsInput(args), 0).Eval(AsInput(args));
	public object ParseEval(string text, object? args) => ParseEval(text, args);
	public string ReadCode(string text) {
		Dictionary<string, List<(Value, Expression, Expression?)>> parsedF = [];
		List<Value> parsedC = [];
		int returnIndex = -1, i, skip = 0;
		bool comment = false;
		string codeLine, log = "";
		ReadLines("", text.Split('\n'));
		return log;

		void ReadLines(string pref, string[] code) {
			//_context.CustomFunctions = new Comparser.CallFunction[CustomFunctions.Count];
			for (i = 0; i < code.Length; ++i) {
				codeLine = code[i];
				while (codeLine.Length > 0)
					ReadLine(pref, i.ToString());

				void ReadLine(string pref, string line) {
					codeLine = codeLine.TrimStart(' ').TrimEnd(' ');
					if (codeLine == "")
						return;
					// skip comment and braces:
					var plus = codeLine.IndexOf('}');
					int minus = codeLine.IndexOf('{');
					var comm = codeLine.IndexOf('/');
					bool eat = true;
					while (eat && (!comment && skip > 0 && (plus >= 0 || minus >= 0) || comment && comm >= 0)) {
						var commbest = comm < plus && comm < minus;
						if ((comment || commbest) && comm >= 0) {
							if (comment) {
								comment &= codeLine[comm - 1] != '*' || comm + 1 < codeLine.Length && codeLine[comm + 1] == '*';
								eatc();
							} else {
								if (commbest) {
									comment |= comm + 1 < codeLine.Length && codeLine[comm + 1] == '*';
									eatc();
								}
							}
							void eatc() { eat = Eat(comm); comm = codeLine.IndexOf('/'); }
						}
						if (!comment && skip > 0) {
							if (plus >= 0 && plus < minus && plus < comm) {
								eat = Eat(plus);
								++skip;
								plus = code[i].IndexOf('}');
							} else if (minus >= 0 && minus < comm) {
								eat = Eat(minus);
								--skip;
								minus = codeLine.IndexOf('{');
							}
						}
						bool Eat(int from) {
							minus -= from;
							plus -= from;
							comm -= from;
							return (codeLine = codeLine[(from + 1)..]).Length > 0;
						}
					}
					// read format: (<cacheIntExpression>)functionname(<argumentsExpression>)=<definitionExpression>

					int l, cache = 1; string e, name = "";
					Value eval;
					switch (codeLine[0]) {
						case '/':
							if (codeLine.Length > 1 && codeLine[1] == '*') {
								comment = true;
							} else
								CL();
							return;

						case '}': // this must be and out of a branch i'm in, jus ignore it
							if (returnIndex >= 0) {
								codeLine = code[i = returnIndex];
								return;
							}
							goto case ';';
						case ';': // separator
						case ' ': // space
						case '\t': // space
							codeLine = codeLine[1..];
							return;
						case '!': // WHILE
							returnIndex = i;
							goto case '?';
						case '?': // IF
							if ("" != (e = failEval(ref codeLine, out eval, '{') ? "Failed to parse condition." : "")) {
								lg(); CL(); return;
							}
							while (eval.Values.Length > 0)
								eval = eval.Values[0];
							if (INumber<T>.IsTrue(eval.Leaf))
								return;
							skip = 1;
							while ((minus = codeLine.IndexOf('{')) < 0)
								codeLine = code[++i];
							codeLine = codeLine[(minus + 1)..];
							break;
						case '(': // CACHE
							if ("" != (e = failEval(ref codeLine, out eval) ? "Failed to parse cache size."
							: eval.Values.Length != 1 ? "Multiple values in the cache size expression: " + eval.ToString()
							: eval.Values[0].Leaf.IsNaN() ? "Cache size evaluated as NaN." : "")) {
								lg();
								CL();
								return;
							}
							cache = (int)Math.Round(T.Re(eval.Values[0].Leaf));
							goto default;
						default:
							Value args = new();
							if ((minus = codeLine.IndexOf('=')) < 0) {
								CL();
								return;
							}
							(string s, string s1) = (codeLine[..minus], codeLine[(minus + 1)..]);
							if ("" != (e = s.Length < 2 ? "There must be an equal sign between the name and definition." : (s1 = Clean(s1).TrimStart(' ').TrimEnd(' ')).Length == 0 ? "No expression after the equal sign." : "")) {
								err();
								return;
							}
							l = (s = Clean(s)).IndexOf('(');
							if ("" != (e = l < 0
								? (isAlphaNumeric(name = s) ? "" : FN())
								: (l < 1 ? "No name."
									: !isAlphaNumeric(name = s[..l].TrimEnd(' ')) ? FN()
									: failArgs(s[(l + 1)..], out args) ? "Failed to parse arguments." : ""))) {
								err();
								return;
							}
							if (name == "") {
								e = "No name.";
								err();
								return;
							}
							if (args.Values.Length == 0) { // it is a constant or codecall
								if (cache != 1) {
									e = name + " is a constant/call and doesn't support caching.";
									lg();
								}
								eval = new Expression(this, s1, args, 0).Eval(new(), name);
								if (name == "print") {
									log += eval.ToString() + "\n";
									end();
									return;
								}
								if (name == "do") {
									var cl = codeLine;
									string[] expand = eval.ToLines().Split('\n'); // ToString(true) recursively exports only string values as lines
									ReadLines(pref + line + "/", expand);
									codeLine = cl;
									end();
									return;
								}
								end();
								foreach (var p in parsedC)
									if (p.Text == name) {
										// mutate variable
										p.Values = eval.Values;
										p.Leaf = eval.Leaf; // probably not needed?
										CustomConstants = new([.. parsedC]);
										return;
									}
								// new variable
								parsedC.Add(eval);
								CustomConstants = new([.. parsedC]);
								return;
							} // it is a function
							  //
							Expression cond = new(this, ref s1, out _, args, 0, cache);
							s1 = s1.TrimStart(' ');
							if (s1[0] == '?') {
								Expression falseExp, trueExp = new(this, ref s1, out _, args, 0, cache);
								s1 = s1.TrimStart(' ');
								if (s1[0] == ':') {
									falseExp = new(this, ref s1, out _, args, 0, cache);
								} else {
									e = "Failed to find : after the true expression."
									err();
									return;
								}
								
								parsedF[name].Add((args, trueExp, cond));
								parsedF[name].Add((args, falseExp, null));
								SaveFuncs();
								return;
							}

							parsedF[name].Add((args, cond, null));
							SaveFuncs();
							return;

							void err() { lg(); end(); }
							void end() => codeLine = (minus = s.IndexOf(';')) >= 0 ? s[(minus + 1)..] + s1 : (minus = s1.IndexOf(';')) >= 0 ? s1[(minus + 1)..] : "";
							void lg() => log += "Line " + line + ": " + e + "\n";
							void SaveFuncs() { // Save the parsed functions and constants:
								i = 0; end();
								CustomFunctions = new CallFunction[parsedF.Count];
								foreach (var p in parsedF)
									CustomFunctions[i++] = new CallCustom(p.Key, [.. p.Value]);
							}
					}
					bool failEval(ref string exp, out Value eval, char close = ')') {
						exp = exp[1..];
						eval = new Expression(this, ref exp, out _, new(), 0).Eval(new());
						if (exp[0] == close) { // closing parenthesis after the argument expression
							exp = exp[1..];
							return false;
						}
						return true;
					}

					void CL() => codeLine = (minus = codeLine.IndexOf(';')) < 0 ? "" : codeLine[(minus + 1)..];
					string FN() => "Failed to parse name: " + name;
					bool failArgs(string exp, out Value args) => failEval(ref exp, out args) || exp.TrimEnd(' ') != "";
					static bool isAlphaNumeric(string strToCheck) => new Regex(@"^[a-zA-Z0-9\s,]*$").IsMatch(strToCheck);
					static string Clean(string t) // forbidden symbols in expressions
						=> t.ToLower().Replace(":", "").Replace(";", "").Replace("|", "")
						.Replace("\t", "").Replace("\r", "").Replace("\n", "");

				}
			}
		}
	}
	#endregion

	#region Content
	private readonly ushort StackOverflow = stackOverflowLimit;
	public static readonly Value noArgs = new();
	public CallFunction[] CustomFunctions = []; // user-defined functions
	public Value CustomConstants = new(); // user-defined constants
	public static CF factorial = new("factorial", INumber<T>.Factorial);
	//protected abstract CallFunction[] DefaultFunctions();
	private static readonly CallFunction[] GenericFunctions = [

		// meta
		new CE("eval", typeof(FuncEval), 0), // attempts to parse and evaluate every Text in the input
		new CE("count", typeof(FuncCount), 0), // counts the number of elements in the vector
		new CE("concat", typeof(FuncCat), 0), // Un-nests the vectors: concat((1,2),3,((4,5),6)) = (1,2,3,4,5,6)
		new CE("cat", typeof(FuncCat), 0), // Un-nests the vectors: concat((1,2),3,((4,5),6)) = (1,2,3,4,5,6)
		new CF("true", (x) => T.MakeR(T.Re(INumber<T>.Sqrabs(x)) >= 1 ? 1 : 0)), // = size >= 1
		new CF("false",  (x) => T.MakeR(T.Re(INumber<T>.Sqrabs(x)) < 1 ? 1 : 0)), // = size < 1

		// double arguments:
		new CF2("minimum", T.Min),	// component-wise minimum: min(1+4i,3+2i)=1+2i
		new CF2("maximum", T.Max),	// component-wise maximum
		new CF2("min", T.Min),		// component-wise minimum: min(1+4i,3+2i)=1+2i
		new CF2("max", T.Max),		// component-wise maximum
		new CF3("clamp", T.Clamp),	// component-wise clamp: clamp(5+4i,3+6i,4+7i)=4+6i

		// quadruple arguments
		new CE("product", typeof(Product), 0),	// iterative product: product(<index>,from,to,expression(k<index>)); Ex: product(0,1,4,k0) = 1*2*3*4 = 24
		new CE("prod", typeof(Product), 0),		// iterative product
		new CE("sum", typeof(Sum), 0),			// iterative sum: sum(<index>,from,to,expression(k<index>)); Ex: sum(0,1,4,k0) = 1+2+3+4 = 10
		new CE("vector", typeof(Vector), 0),	// iterative vector builder: vector(<index>,from,to,expression(k<index>)); Ex: vector(0,1,3,2^k0) = (2,4,8)
		new CE("vec", typeof(Vector), 0),		// iterative vector builder

		// exp/log
		new CF("exp10", INumber<T>.Exp10),	// Decimal exponential: 10^x
		new CF("exp2", INumber<T>.Exp2),	// Binary exponential: 2^x
		new CF("exp", T.Exp),				// Exponential: e^x = (cos(b)+isin(b))e^a
		new CF("log10", INumber<T>.Log10),	// Decimal logarithm: log_10(x) = ln(x)/ln(2)
		new CF("log2", INumber<T>.Log2),	// Binary logarithm: log_2(x) = ln(x)/ln(10)
		new CF("log", T.Log),				// Natural logarithm: ln(x)
		new CF("ln", T.Log),				// Natural logarithm: ln(x)

		// sincs
		new CF("nsinhc", INumber<T>.Nsinhc),// sinhc(x*pi)
		new CF("sinchpi", INumber<T>.Nsinhc),// sinhc(x*pi)
		new CF("sinhc", INumber<T>.Sinhc),	// sinhc(x) = sinh(x)/x
		new CF("nsinc", INumber<T>.Nsinc),	// sinc(x*pi)
		new CF("sincpi", INumber<T>.Nsinc),	// sinc(x*pi)
		new CF("sinc", INumber<T>.Sinc),	// sinc(x) = sin(x)/x
		new CF("sinc", INumber<T>.Cosc),	// cosc(x) = (1-cos(x))/x

		// arc hyperbolics
		new CF("acosh", T.Acosh),			// acosh(x)
		new CF("asinh", T.Asinh),			// asinh(x)
		new CF("atanh", T.Atanh),			// atanh(x)
		new CF("asech", INumber<T>.Asech),	// asech(x)
		new CF("acsch", INumber<T>.Acsch),	// acsch(x)
		new CF("acoth", T.Acoth),			// acoth(x)

		// hyperbolics
		new CF("cosh", T.Cosh),			// cosh(x)
		new CF("sinh", T.Sinh),			// sinh(x)
		new CF("tanh", T.Tanh),			// tanh(x)
		new CF("sech", INumber<T>.Sech),// sech(x)
		new CF("csch", INumber<T>.Csch),// csch(x)
		new CF("coth", T.Coth),			// coth(x)

		// arc trigs
		new CF("acos", T.Acos),			// acos(x)
		new CF("asin", T.Asin),			// asin(x)
		new CF("atan", T.Atan),			// atan(x)
		new CF("asec", INumber<T>.Asec),// asec(x)
		new CF("acsc", INumber<T>.Acsc),// acsc(x)
		new CF("acot", T.Acot),			// acot(x)

		// trigs
		new CF("cos", T.Cos),			// cos(x)
		new CF("sin", T.Sin),			// sin(x)
		new CF("tan", T.Tan),			// tan(x)
		new CF("sec", INumber<T>.Sec),	// sec(x)
		new CF("csc", INumber<T>.Csc),	// csc(x)
		new CF("cot", T.Cot),			// cot(x)

		// basics/components
		new CF("real", INumber<T>.TRe),			// Real part: Re(x) = a
		new CF("re", INumber<T>.TRe),			// Real part: Re(x) = a
		new CF("imag", INumber<T>.TI),			// Imaginary part: Im(x) = b (or sqrt(bb+cc+dd) for quats)
		new CF("im", INumber<T>.TI),			// Imaginary part
		new CF("frac", T.Frac),					// Signed fractional part: Frac(x) = x - Trunc(x)
		new CF("trunc", T.Trunc),				// Whole part: Truncate(x)
		new CF("floor", T.Floor),				// Round down: Floor(x)
		new CF("round", T.Round),				// Round near: Round(x)
		new CF("ceil", T.Ceil),					// Round up: Ceiling(x)
		new CF("sign", INumber<T>.Sign),		// Sign(x) = x / |x|
		new CF("sgn", INumber<T>.Sign),			// Sign(x) = x / |x|
		new CF("negative", INumber<T>.Neg),		// Negation: -x
		new CF("neg", INumber<T>.Neg),			// Negation: -x
		new CF("inverse", T.Inv),				// Inverse: 1/x
		new CF("inv", T.Inv),					// Inverse: 1/x
		new CF("absri", T.AbsComp),				// Positive components: |a|+|b|i
		new CF("sqrabs", INumber<T>.Sqrabs),	// Squared absolute value: |x|^2 = a*a + b*b
		new CF("absolute", INumber<T>.TAbs),	// Absolute value: |x| = sqrt(a*a + b*b)
		new CF("abs", INumber<T>.TAbs),			// Absolute value: |x| = sqrt(a*a + b*b)
		new CF("arg", INumber<T>.TArg),			// Argument: Arg(x)
		new CF("conjugate", INumber<T>.Conj),	// Conjugate: Conj(x) = a - bi
		new CF("conj", INumber<T>.Conj),		// Conjugate: Conj(x) = a - bi

		// powers
		new CF("sqrt", T.Sqrt),			// Square root: Sqrt(x)
		new CF("cbrt", INumber<T>.Cbrt),// Cube root: Cbrt(x)
		new CF("sqr", T.Sqr),			// Square: x*x
		new CF("cube", T.Cub),			// Cube: x*x*x
		new CF("cub", T.Cub),			// Cube: x*x*x
		new CF("quart", T.Quart),		// Quart: x*x*x*x

		// specials
		factorial,									// x!
		new CF("gauss", INumber<T>.Gauss),			// e^(-x^2)
		new CF("softabs", INumber<T>.SoftAbs),		// ln(1+e^x)
		new CF("softneg", INumber<T>.SoftNeg),		// ln(1+e^x)
		new CF2("softmax", INumber<T>.SoftMax),		// ln(e^a+e^b+...)
		new CF2("softmin", INumber<T>.SoftMin),		// ln(a^a+e^b+...)
		new CF("gamma", INumber<T>.Gamma_Stirling),	// gamma(x)
		new CF("zeta", INumber<T>.Zeta)				// zeta(x)
		];
	protected abstract Value DefaultConstants();
	protected static Value GenericConstants = new([
		new(INumber<T>.pi(), "pi"), 
		new(INumber<T>.tau(), "tau"), 
		new(INumber<T>.e(), "e"), 
		new(INumber<T>.gamma(), "gamma"), 
		new(T.One(), "one")]);
	#endregion

	#region Call Functions
	// abstract parent
	public abstract class CallFunction(string name, int _cache = 1) {
		//protected readonly I[]? Def = def;
		public readonly string Name = name;
		public readonly EvalCache Cache = new(_cache);
		public abstract Expression Call(Comparser<T> context, ref string text, Value args, ushort depth);
		// how to use: e.Insert(args, e.GetEval(args) ? e.result.Eval : base.Eval([], args).v); 
		public struct EvalCache(int size = 1) {
			private readonly int Size = size;
			public int filled = 0;
			public Evaluated? cache = null;
			public Evaluated? result;
			public bool GetEval(Value args) {
				for (var c = result = cache; c != null; result = c, c = c.Next) {
					if (c.Args.Match(args)) {
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
			public Value Insert(Value args, Value eval) {
				if (Size <= filled && result != null)
					result.Next = null;
				if (Size > 0)
					cache = new Evaluated(cache, args, eval);
				return eval;
			}
			public void Reset() { filled = 0; cache = null; }
		}
		public class Evaluated(Evaluated? next, Value args, Value eval) {
			public Value Eval = eval.Copy();
			public Value Args = args.Copy();
			public Evaluated? Next = next;
		}
	}
	// Expressions.Functions:
	private class CE(string name, Type type, int _cache = 1) : CallFunction(name, _cache) {
		public override Expression Call(Comparser<T> context, ref string text, Value args, ushort depth) {
			object[] a = [context, this, text, args, depth]; // activator arguments
			var n = (FunctionExpression)Activator.CreateInstance(type, a)!;
			text = (string)a[1]; // ref string text
			return n;
		}
	}
	// Single/Double/Triple argument delegated functions
	public class CF(string name, Func<T, T> del, int _cache = 1) : CallFunction(name, _cache) {
		public override Expression Call(Comparser<T> context, ref string text, Value args, ushort depth) => new FuncOperator(context, this, del, ref text, args, depth);
	}
	public class CF2(string name, Func<T, T, T> del, int _cache = 1) : CallFunction(name, _cache) {
		public override Expression Call(Comparser<T> context, ref string text, Value args, ushort depth) => new FuncOperator2(context, this, del, ref text, args, depth);
	}
	public class CF3(string name, Func<T, T, T, T> del, int _cache = 1) : CallFunction(name, _cache) {
		public override Expression Call(Comparser<T> context, ref string text, Value args, ushort depth) => new FuncOperator3(context, this, del, ref text, args, depth);
	}
	#endregion

	#region Function Expressions - Operators
	protected abstract class FunctionExpression : Expression {
		protected readonly CallFunction Parent;
		public FunctionExpression(Comparser<T> context, CallFunction parent, ref string text, Value args, ushort depth) : base(context, ref text, out _, args, depth) => Parent = parent;
		public FunctionExpression(Comparser<T> context, CallFunction parent, Value input, ushort depth) : base(context, input, depth) => Parent = parent;
		public override Value Eval(Value args, string text = "") {
			var v = base.Eval(args);
			return Parent.Cache.Insert(v, Parent.Cache.GetEval(v) ? Parent.Cache.result?.Eval! : EvalF(v, args));
		}
		protected abstract Value EvalF(Value v, Value args);
	}
	private class FuncTextOperator : FunctionExpression {
		private readonly Func<string, Value> Del;
		public FuncTextOperator(Comparser<T> context, CallFunction parent, Func<string, Value> del, ref string text, Value args, ushort depth) : base(context, parent, ref text, args, depth) => Del = del;
		public FuncTextOperator(Comparser<T> context, CallFunction parent, Func<string, Value> del, Value args, ushort depth) : base(context, parent, args, depth) => Del = del;
		protected override Value EvalF(Value v, Value args) => Value.OperateText(v, V, Del);
	}
	private class FuncEval(Comparser<T> context, CallFunction parent, ref string text, Value args, ushort depth, int _cache = 0) 
		: FuncTextOperator(context, parent, (x) => new Expression(context, x, args, depth, _cache).Eval(args), ref text, args, depth) { }
	private class FuncOperator : FunctionExpression {
		private readonly Func<T, T> Del;
		public FuncOperator(Comparser<T> context, CallFunction parent, Func<T, T> del, ref string text, Value args, ushort depth) : base(context, parent, ref text, args, depth) => Del = del;
		public FuncOperator(Comparser<T> context, CallFunction parent, Func<T, T> del, Value input, ushort depth) : base(context, parent, input, depth) => Del = del;
		protected override Value EvalF(Value v, Value args) => Value.Operate(v, V, Del);
	}
	private class FuncOperator2(Comparser<T> context, CallFunction parent, Func<T, T, T> comp, ref string text, Value args, ushort depth) : FunctionExpression(context, parent, ref text, args, depth) {
		protected override Value EvalF(Value v, Value args) {
			Value m = v.Values[0], s = new();
			for (var c = 1; c < v.Values.Length; m = Value.Operate2(m, v.Values[c++], s, comp, (x,y) => x).Copy()) { }
			return s;
		}
	}
	private class FuncOperator3(Comparser<T> context, CallFunction parent, Func<T, T, T, T> comp, ref string text, Value args, ushort depth) : FunctionExpression(context, parent, ref text, args, depth) {
		protected override Value EvalF(Value v, Value args) => v.Values.Length == 3 ? Value.Operate3(v.Values[0], v.Values[1], v.Values[2], V, comp) : new();
	}
	#endregion

	#region Function Expressions - Vectors
	// extracts terms from a vector using indices in: [expression]. Example: (0a,1b,2c,(30d,31e),5f)[3,2,(5,1,3)] = (30d,31e),2c,(5,1,(30d,31e))
	private class FuncIndex(Comparser<T> context, Value indices, Value input, ushort depth) : Expression(context, input, depth) {
		public override Value Eval(Value args, string text = "") {
			List<Value> r = [];
			Operate(ref r, indices, base.Eval(args));
			return new([.. r]);
		} 
		private static void Operate(ref List<Value> r, Value indices, Value v) {
			v = CollapseScalar(v);
			Value[] I; I = CollapseScalar(indices).Values;
			int integer, s = I.Length;
			for (int i = 0; i < s; ++i)
				if (0 == (I[i] = CollapseScalar(I[i])).Values.Length) {
					var index = T.Re(I[i].Leaf);
					if (double.IsNaN(index) || (integer = (int)Math.Round(index)) < 0 || integer >= v.Values.Length)
						r.Add(new());
					else
						r.Add(CollapseScalar(v.Values[integer]));
				} else {
					List<Value> nr = [];
					Operate(ref nr, I[i], v);
					r.Add(new([.. nr]));
				}
		}
	}
	private class FuncCat(Comparser<T> context, Value input, ushort depth) : Expression(context, input, depth) {
		public override Value Eval(Value args, string text = "") {
			List<Value> r = [];
			Operate(ref r, base.Eval(args));
			return new([.. r]);
		}
		private static void Operate(ref List<Value> r, Value v) {
			Value[] V; V = v.Values;
			int s = V.Length;
			for (int i = 0; i < s; ++i)
				if (0 == V[i].Values.Length) r.Add(new(V[i].Leaf));
				else Operate(ref r, V[i]);
		}
	}
	// counts the elements in a vector
	protected abstract class FuncCount(Comparser<T> context, ref string text, Value args, ushort depth) : Expression(context, ref text, out _, args, depth) {
		public override Value Eval(Value args, string text = "") => new(CollapseScalar(base.Eval(args)).Values.Length);
	}
	// iterative sum/product: name(<index>,<from>,<to>,expression(k<index>))
	// "to" can be smaller than "from", works both ways (does not return additive/multiplicative identity when in the wrong order, just iterates backwards)
	private abstract class Iterator(Comparser<T> context, CallFunction parent, ref string text, Value args, ushort depth) : FunctionExpression(context, parent, ref text, args, depth) {
		protected override Value EvalF(Value v, Value args) {
			int iterator = (int)Math.Round(T.Re(v.Values[0].Leaf)), 
				from = (int)Math.Round(T.Re(v.Values[1].Leaf)), 
				to = (int)Math.Round(T.Re(v.Values[2].Leaf));
			int iteratorIndex = args.Values.Length;
			Value ni = new(new Value[iteratorIndex + 1]);
			Array.Copy(args.Values, ni.Values, iteratorIndex);
			ni.Values[iteratorIndex].Text = "k" + iterator.ToString();
			Value evalK(int f) {
				ni.Values[iteratorIndex].Leaf = T.MakeR(f);
				return EvalSingle(3, ni);
			}
			return Result(evalK, from, to);
		}
		protected virtual void Op(ref Value result, Value iteration) => result = iteration;
		protected abstract Value Result(Func<int, Value> eval, int from, int to);
		protected static void Iterate(Action<int> iter, int from, int to) {
			// add the other iterations all the way to "to"
			while (from < to) iter(++from);
			while (from > to) iter(--from);
		}
	}
	private abstract class CollapseIterator(Comparser<T> context, CallFunction parent, ref string text, Value args, ushort depth) : Iterator(context, parent, ref text, args, depth) { 
		protected override Value Result(Func<int, Value> eval, int from, int to) {
			var sum = eval(from); // prepare first iteration as the initial vector
			void iterK(int f) {
				var v = eval(f);
				Op(ref sum, v);
				//for (var j = v.Values.Length; 0 <= --j; Op(ref sum[j], v[j])) { }
			}
			Iterate(iterK, from, to);
			return sum;
		}
	}
	// return a vector of sums of iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (x,2x); sum(0,1,3,exp(k0)) => (1+2+3,2+4+6) => (6,12); // 6 is the sum of x term, evaluated with k0=1..3, 12 is the sum of 2x term, evaluated with k0=1..3
	private class Sum(Comparser<T> context, CallFunction parent, ref string text, Value args, ushort depth) : CollapseIterator(context, parent, ref text, args, depth) { 
		protected override void Op(ref Value result, Value iteration) => result = Value.Operate2(result, iteration, new(), INumber<T>.Add, (x, y) => x + y);
	}
	// return a vector of products of iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (x,2x); prod(0,1,3,exp(k0)) => (1*2*3,2*4*6) => (6,48); // 6 is the product of x term, evaluated with k0=1..3, 48 is the product of 2x term, evaluated with k0=1..3
	private class Product(Comparser<T> context, CallFunction parent, ref string text, Value args, ushort depth) : CollapseIterator(context, parent, ref text, args, depth) {
		protected override void Op(ref Value result, Value iteration) => result = Value.Operate2(result, iteration, new(), INumber<T>.Mul, (x, y) => x);
	}
	// returns a vector of first elements of evaluated iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (3x,2x,4x); vector(0,1,3,exp(k0)) => (3*1,3*2,3*3) => (3,6,9); // only took the first 3x term, evaluated with k0=1..3
	private class Vector(Comparser<T> context, CallFunction parent, ref string text, Value args, ushort depth) : Iterator(context, parent, ref text, args, depth) {
		protected override Value Result(Func<int, Value> eval, int from, int to) { 
			var size = Math.Abs(from - to);
			Value sum = new(new Value[size]);
			sum.Values[0] = eval(from).Values[0];
			var iteratorIndex = 0;
			void iterK(int f) => Op(ref sum.Values[++iteratorIndex], eval(f).Values[0]);
			Iterate(iterK, from, to);
			return sum;
		}
	}
	#endregion

	#region Function Expressions - Custom
	// User defined custom expression functions
	public class CallCustom(string name, (Value input, Expression def, Expression? condition)[] def, int _cache = 1) : CallFunction(name, _cache) {
		public readonly (Value input, Expression def, Expression? condition)[] Def = def;
		public override Expression Call(Comparser<T> context, ref string text, Value args, ushort depth) 
			=> new CustomFunc(context, this, ref text, args, depth);
	}
	private class CustomFunc(Comparser<T> context, CallCustom parent, ref string text, Value args, ushort depth) : FunctionExpression(context, parent, ref text, args, depth) {
		protected override Value EvalF(Value v, Value i) {
			var match = -1; for (var m = 0; m < parent.Def.Length; ++m)
				if (parent.Def[m].input.Match(v)) {
					match = m; break;
				}
				//var ok = true; for (var id = 0; id < parent.Def[m].input.Values.Length; ++id) ok &= parent.Def[m].input[id].Match(v[id]);if (ok) { match = m; break; }
			return match == -1 || Cond(parent.Def[match].condition, v) ? new() : parent.Def[match].def.Eval(v); // failed to match any available argument list ? else eval.
		}
		protected bool Cond(Expression? e, Value v) {
			if (e == null)
				return true;
			var l = e.Eval(v);
			while (l.Values.Length > 0)
				l = l.Values[0];
			return INumber<T>.IsFalse(l.Leaf);
		}
	}
	#endregion

	#region Operand Operators
	public class Operator(byte order = 0, bool right = false, int eatOp = 1) {
		public bool Negative = false;
		public readonly byte Order = order; // order of operations
		public readonly bool Right = right; // right-associativity
		public int EatOp = eatOp; // using an operator symbol? (if false, it is an operator-less multiplication)
		public virtual T Op(T value, T operand) => Negative ? -value : value;
		public virtual string SOp(string value, string operand) => value;
	}
	private class Less(bool orEqual) : Operator(1, false, orEqual ? 2 : 1) { public override T Op(T value, T operand) => INumber<T>.True(orEqual
		? (Negative ? -T.Re(value) : T.Re(value)) <= T.Re(operand) 
		: (Negative ? -T.Re(value) : T.Re(value)) < T.Re(operand)); }
	private class More(bool orEqual) : Operator(1, false, orEqual ? 2 : 1) { public override T Op(T value, T operand) => INumber<T>.True(orEqual
		? (Negative ? -T.Re(value) : T.Re(value)) >= T.Re(operand) 
		: (Negative ? -T.Re(value) : T.Re(value)) > T.Re(operand)); }
	private class Equal() : Operator(1) { public override T Op(T value, T operand) => INumber<T>.True(T.Compare(Negative ? -value : value, operand)); }
	private class Exclamation(byte order = 6) : Operator(order) { public override T Op(T value, T operand) => INumber<T>.True(!T.Compare(Negative ? -value : value, operand)); } // change order to 1 if followed by '='
	private class Add() : Operator(2) { 
		public override T Op(T value, T operand) => (Negative ? -value : value) + operand;
		public override string SOp(string value, string operand) => value + operand;
	}
	private class Sub() : Operator(2) {	
		public override T Op(T value, T operand) => (Negative ? -value : value) - operand;
		public override string SOp(string value, string operand) => value.Replace(operand, "");
	}
	private class Mod() : Operator(3) {	public override T Op(T value, T operand) => (Negative ? -value : value) % operand; }
	private class Div() : Operator(3) {	public override T Op(T value, T operand) => operand.Is0() ? T.NaN() : (Negative ? -value : value) / operand; }
	private class LDiv() : Operator(3) { public override T Op(T value, T operand) => operand.Is0() ? T.NaN() : T.LDiv(Negative ? -value : value, operand); }
	// if ": Operator(4, ...)", then 1/2*3 could be 1/6, but left associativity for "*/" matches computer evals
	private class Mul(bool eatOp = true) : Operator(3, false, eatOp ? 1 : 0) { public override T Op(T value, T operand) => (Negative ? -value : value) * operand; }
	private class Pow() : Operator(5, true) { public override T Op(T value, T operand) => (Negative ? -1 : 1) * (operand.Is0() ? T.MakeR(1) : value ^ operand); }
	// Implemented as encapsulated function, these Ops are just for parsing:
	private class Index() : Operator(6) { public override T Op(T value, T operand) => T.NaN(); }
	#endregion

	#region Helpers
	private static Value CollapseScalar(Value i) {
		while (i.Values.Length == 1)
			i = i.Values[0];
		return i;
	}
	#endregion

	public class Expression {
		// Protection against infinite loops, causing a stack coverflow NaN result
		protected readonly ushort Depth;
		// Contains user-defined custom function
		protected readonly Comparser<T> Context;
		// Parsed and evaluated data
		protected readonly Value V;
		// Cache for remembering recently evaluated arguments
		private readonly CallFunction.EvalCache cache;
		/// <summary>
		/// Evaluates the expression
		/// </summary>
		/// <param name="args">arguments</param>
		/// <returns>Evaluated value of this expression</returns>
		public virtual Value Eval(Value args, string text = "") {
			if (cache.GetEval(args)) return cache.Insert(args, cache.result?.Eval!);
			for (int e = 0; e < V.Values.Length; _ = EvalSingle(e++, args)) { }
			if (text != "") V.Text = text;
			return V;
		}
		protected Value EvalSingle(int arrayIndex, Value args) {
			Value ee; Value[] v, a; v = V.Values; a = args.Values;
			return arrayIndex < v.Length ? Value.Operate2(
				(ee = v[arrayIndex]).Term?.Eval(args) ?? new([ee.Arg < 0 || ee.Arg >= a.Length ? ee : a[ee.Arg] ?? ee]),
				ee.Operand?.Eval(args) ?? new(), ee, ee.Op.Op, ee.Op.SOp) : new();
		}
		public Expression(Comparser<T> context, string text, Value args, ushort depth, int _cache = 0) {
			var e = new Expression(Context = context, ref text, out _, args, Depth = depth, _cache);
			V = text == "" ? e.V : new();
			cache = new(_cache);
		}
		/// <summary>
		/// Reads adn parses an expression string
		/// </summary>
		/// <param name="context">context that contains custom callable functions/constants</param>
		/// <param name="text">string to parse</param>
		/// <param name="nextOp">returns operand's operator if that operand should be left-associated with my term, will encapsulate previous operator into my term, and use nextOp on next operand</param>
		/// <param name="args">argument value, will substitute every x in the string</param>
		/// <param name="_cache">cache size of this new Expression</param>
		/// <param name="left">what oreder of operations was my parent's operator? Used to test for associativity</param>
		public Expression(Comparser<T> context, ref string text, out Operator nextOp, Value args, ushort depth, int _cache = 0, byte left = 0) {
			Depth = depth;
			cache = new(_cache);
			Context = context;
			List<Value> Expr = [];
			while (Read(out var t, ref text, out nextOp) && left == 0 && Char(text, ',')) // only left == 0 (aka top layer expression) should accept ',' for a next value
				R(1, ref text);
			V = new([.. Expr]);
			return;

			bool Read(out Value r, ref string text, out Operator nextOp) {
				// Init read
				r = new(T.NaN(), new(), -1);
				var a = -1;
				Expr.Add(r);
				nextOp = r.Op = new();
				text = text.TrimStart(' ');
				r.Op.Negative = Negative(ref text, ref r);
				r.Leaf = T.NaN();
				text = text.TrimStart(' ');
				// Try parenthesis/function/number/constant/argument:
				if ((!Char(text, '(', true) || SubTerm(out r.Term, ref r, ref text, ')'))
					&& Func(GenericFunctions, ref r, ref text) // _term = default function
															   //&& Func(context.DefaultFunctions(), ref r, ref text) // _term = default function
					&& Func(context.CustomFunctions, ref r, ref text) // _term = custom function
					&& (Number(ref text, ref r, out var n) // _value = number
						|| Constant(ref text, ref r, GenericConstants, out n, out a) // _value = generic constants (pi/tau/e/i/gamma/one)
						|| Constant(ref text, ref r, context.DefaultConstants(), out n, out a) // _value = constant (i/j/k/x/y/z...)
						|| Constant(ref text, ref r, context.CustomConstants, out n, out a) // _value = user constant
						|| Constant(ref text, ref r, args, out n, out a, true)))
					r.Term = new(context, a < 0 ? n : new Value(a), depth, _cache);//r.Value = n; // _value = argument (x/y/z/t...)
				else if (Fail(r) && F(ref r)) // failed to read a term/value
					return false; //  unexpected end fail
				if (End(text)) // unexpected ')', or no op, and return back successful
					return true; 
				// Read operators/comments:
				Operator o;
				text = text.TrimStart(' ');
				while ((o = text[0] switch { 
					'+' => new Add(), '-' => new Sub(), '*' => new Mul(), '/' => new Div(), '\\' => new LDiv(), '^' => new Pow(), '%' => new Mod(),
					'=' => new Equal(),	'<' => new Less(text.Length > 1 && text[1] == '='),	'>' => new More(text.Length > 1 && text[1] == '='),
					'[' => new Index(), '!' => new Exclamation(), _ => new Mul(false) }).GetType() switch {
					var x when x == typeof(Exclamation) => NotUnequal(text, ref o) && RP(1, ref r, ref text) && Encapsulate(new FuncOperator(context, factorial, INumber<T>.Factorial, Expr[^1], depth), out r), // factorial
					var x when x == typeof(Index) => ExtractTerms(Expr[^1], ref r, ref text, ref o), // index
					var x when x == typeof(Div) => text[1] == '*' && Comment(ref text, ref r, ref o), // comment
					_ => false
				}) {
					text = text.TrimStart(' ');
					if (o.Order == 0) return true;
				}
				while (o.EatOp-- > 0) // not an operator-less multiplication?
					RP(1, ref r, ref text); // eat operator
				o.Negative = r.Op.Negative; // move negative flag to the new operator
				if (LeftAssociate(o)) {
					nextOp = o; // perform left-associativity by returning back, and the parent will encapsualte
					return false;
				}
				// Read operand:
				while (true) {
					if (Fail((r.Operand = new Expression(context, ref text, out o, args, depth, _cache, (r.Op = o).Order)).V)) {
						if (r.Op.EatOp >= 0 && F(ref r))
							return false; // failed to read operand
										  // if it was operator-less multiplication - assume it was an expression end instead
						r.Op = new();
						break;
					}
					if (o.Order == 0)
						break;
					// operand's next op has lower or equal order priority:
					// encapsulate my term into another term (wrap my term into parentheses), take the next operator and find the next operand to use it on
					_ = Encapsulate(new(context, Expr[^1], depth, _cache), out r);
					if (LeftAssociate(o)) { // need to test associativity again, to let it recurse backwards. othewise 2^2^2+1 woudl be 2^(2^2+1)
						nextOp = o; // perform left-associativity by returning back, and the parent will encapsualte
						return false;
					}
				}
				return true;

				bool NotUnequal(string text, ref Operator o) {
					if (text.Length <= 1 || text[1] != '=')
						return true; // must be a factorial, keep it
					o = new Exclamation(1); // must be !=, change into that
					return false;
				}
				bool Comment(ref string text, ref Value r, ref Operator o) {
					RP(1, ref r, ref text); // eat initial /
					for (var go = true; go;) {
						var i = text.IndexOf('/');
						if (i < 0) { text = ""; o = new(); return true; }
						go = text[i - 1] != '*';
						RP((byte)(i + 1), ref r, ref text);
					}
					return true;
				}
				bool ExtractTerms(Value p, ref Value r, ref string text, ref Operator o) {
					RP(1, ref r, ref text);
					if (SubTerm(out var indices, ref r, ref text, ']'))
						o = new(); // failed to parse indices
					return Encapsulate(new FuncIndex(context, indices.V, Expr[^1], depth), out r);
				}
				bool SubTerm(out Expression readto, ref Value r, ref string text, char req) => (Fail((readto = new Expression(context, ref text, out _, args, depth)).V) || readto.V.Values.Length == 0 || FailRequiredSymbol(req, ref r, ref text)) && F(ref r);
				bool End(string text) => text.Length == 0 || text[0] == ')' || text[0] == ',';
				bool Encapsulate(Expression p, out Value r) { Expr[^1] = r = new(T.NaN(), new(), -1, p); return true; }
				bool LeftAssociate(Operator testop) => testop.Right ? testop.Order < left : testop.Order <= left;
				bool Fail(Value test) => test.Term == null || test.Values.Length == 0;// && test.Value.IsNaN; // no longer needed as even values are now nested in terms, and i don't test their insides.
				bool F(ref Value r) { r.Op = new(); r.Leaf = T.NaN(); r.Values = []; r.Term = r.Operand = null; return true; } // reading failed
				bool FailRequiredSymbol(char c, ref Value r, ref string text) {
					if (!Char(text, c) && F(ref r)) return true;
					RP(1, ref r, ref text);
					return false;
				}
				bool Func(CallFunction[] f, ref Value r, ref string text) {
					foreach (var t in f) {
						if (t.Name.Length <= 0 || text.Length <= t.Name.Length || text[0..t.Name.Length] != t.Name) continue;
						// func opening parenthesis
						if (FailRequiredSymbol('(', ref r, ref text)) continue; // no argument parentheses found, maybe it's a constant with the same name...?
						RP((byte)t.Name.Length, ref r, ref text);
						// must eat func closing parenthesis
						return (depth > context.StackOverflow || Fail((r.Term = t.Call(context, ref text, args, (ushort)(1 + depth))).V) || FailRequiredSymbol(')', ref r, ref text)) && F(ref r);
					}
					return true;
				}
				bool Negative(ref string text, ref Value r) {
					if (!Char(text, '-'))
						return false;
					RP(1, ref r, ref text); // eat minus sign
					return true;
				}
				bool Number(ref string text, ref Value r, out Value n) {
					if (RealNumber(ref text, ref r, out var real)) {
						n = new(T.MakeR(real));
						return true;
					}
					n = new();
					return false;
				}
				bool RealNumber(ref string text, ref Value r, out double n, double l = 0) {
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
				double DecimalNumber(ref string text, ref Value r, double d = 1) {
					if (text.Length == 0) return 0; // no more digits
					d /= 10; // prepare another position
					if (!int.TryParse(text[0].ToString(), out var i))
						return 0;
					RP(1, ref r, ref text);
					return i * d + DecimalNumber(ref text, ref r, d);
				}
				bool Constant(ref string text, ref Value r, Value a, out Value n, out int foundArg, bool isArg = false) {
					// WARNING, if there is any function with the same name, then you can't operator-less multiply with parentheses from the right!
					// for example gamma is either eulerContant or the gamma function:
					// gamma2 = eulerConstant*2, gamma(2+1) = evaluates gamma function at 2, (2+1)gamma = (2+1)*eulerConstant
					foundArg = -1;
					for (var f = 0; f < a.Values.Length; ++f) {
						var v = a.Values[f];
						var k = v.Text;
						if (text.Length < k.Length ||
							text[0..k.Length] != k) continue;
						RP((byte)k.Length, ref r, ref text);
						n = v;
						if (isArg)
							foundArg = f;
						return true;
					}
					n = new();
					return false;
				}
				bool RP(byte c, ref Value r, ref string text) {
					r.Text += text[..c];
					R(c, ref text);
					return true;
				}
			}
			void R(byte c, ref string text) => text = text[c..];
			bool Char(string text, char c, bool eat = false) { var r = text.Length > 0 && text[0] == c; if (eat) R(1, ref text); return r; }
		}
		protected Expression(Comparser<T> context, Value t, ushort depth, int _cache = 0) {
			cache = new(_cache);
			Context = context;
			Depth = depth;
			V = new([new Value(t.Leaf, t.Op, t.Arg, t.Term, t.Operand, t.Op.Negative)]);
		}
	}
	public class Value {
		// simple value if it is only a number not containing an expression term (can remain present even if it is replaced with a term)
		public T Leaf = T.NaN();
		// Vector elements (nestable)
		public Value[] Values = [];
		// Main term and operand (second term)
		public Expression? Term = null, Operand = null;
		// Operator (eval = term <operator> operand), if it is a pure parent Operator, it only evaluates the term
		public Operator Op = new();
		// Argument index binding (if nonegative, it will get replaced by the argument value with this Arg index)
		public int Arg = -1;
		// Multiple functions:
		// In constant/variable/argument bindings, this is the alias name that will get replaced with the Leaf value whenever it is detected
		// In a string data type, it contains the string value
		// After parsing will contain the original parsed text, even if error occurs (this also naturally works with argument parsing and matching)
		public string Text = ""; // The original text that this input has been parsed from, even if it fails parsing
		public override string ToString() => CollapseScalar(this).PV();
		private string PV(string a = "", string b = "") {
			if (Values.Length <= 0)
				return Leaf.ToString() ?? "";
			string s = a;
			for (int i = 0; i < Values.Length; ++i) {
				Values[i] = CollapseScalar(Values[i]);
				if (i > 0) s += ", ";
				s += Values[i].PV("(", ")");
			}
			return s + b;
		}
		private string TL() {
			if (Values.Length <= 0)
				return Text;
			string s = "";
			for (int i = 0; i < Values.Length; ++i)
				s += (Values[i] = CollapseScalar(Values[i])).TL() + "\n";
			return s;
		}
		public string ToLines() => CollapseScalar(this).TL();
		public Value(T value, Operator op, int arg = -1, Expression? term = null, Expression? operand = null, bool negative = false, string text = "") {
			Leaf = value; Term = term; Operand = operand; Op = op; Op.Negative = negative; Arg = arg; Text = text;
		}
		public Value(T value, string text = "") { Leaf = value; Text = text;	}
		public Value(string text = "") => Text = text;
		public Value(Value[] values, string text = "") { Values = values; Text = text; }
		public Value(int arg, string text = "") { Arg = arg; Text = text; }
		/*public void SetValue(T value) {
			Values = [];
			Value = value;
		}
		public void SetValue() {
			Values = [];
			Value = T.NaN();
		}
		public void SetValues(Value[] values) {
			Values = values;
			Value = T.NaN();
		}
		public static Value[] CopyValues(Value[] v) {
			var a = new Value[v.Length];
			for (int i = 0; i < v.Length; ++i)
				a[i] = v[i].Copy();
			return a;
		}*/
		public bool Match(Value a) { // defArguments.Match(callArguments)
			if (Leaf.IsNaN()) {
				if (Values.Length == 0) return true;
				if (Values.Length != a.Values.Length) return false;
				bool m = true;
				for (int i = 0; i < Values.Length; ++i)
					m &= Values[i].Match(a.Values[i]);
				return m;

			} else return T.Compare(Leaf, a.Leaf); // callArguments always starts with Values
		}
		public static Value OperateText(Value AV, Value vals, Func<string, Value> o) {
			Value[] A; A = (AV = CollapseScalar(AV)).Values;
			int s = A.Length;
			vals.Values = new Value[s];
			for (int an, a = 0; a < s; ++a)
				vals.Values[a] = (an = (A[a] = CollapseScalar(A[a])).Values.Length) == 0 
					? o(A[a].Text) 
					: OperateText(an == 0 ? new([new(A[a].Leaf)]) : A[a], new(new Value[an]), o);
			return vals;
		}
		public static Value Operate(Value AV, Value vals, Func<T, T> o) {
			Value[] A; int s; A = (AV = CollapseScalar(AV)).Values;
			vals.Values = new Value[s = A.Length];
			for (int an, a = 0; a < s; ++a)
				vals.Values[a] = (an = (A[a] = CollapseScalar(A[a])).Values.Length) == 0 ? new(o(A[a].Leaf)) : Operate(A[a], new(new Value[an]), o);
			return vals;
		}
		public static Value Operate2(Value AV, Value BV, Value vals, Func<T, T, T> o, Func<string, string, string> so) {
			Value[] A = (AV = CollapseScalar(AV)).Values, B = (BV = CollapseScalar(BV)).Values;
			int a = 0, b = 0, s = Math.Max(A.Length, B.Length);
			vals.Values = new Value[s];
			for (int an, bn, i = 0; i < s; ++i) {
				vals.Values[i] = (an = (A[i] = CollapseScalar(A[i])).Values.Length) 
					+ (bn = (B[i] = CollapseScalar(B[i])).Values.Length) == 0
					? new(o(A[i].Leaf, B[i].Leaf), so(A[i].Text, B[i].Text)) 
					: Operate2(
						an == 0 ? new([new(A[i].Leaf)]) : A[i],
						bn == 0 ? new([new(B[i].Leaf)]) : B[i],
						new(new Value[Math.Max(an, bn)]), o, so);
				a = (a + 1) % A.Length;
				b = (b + 1) % B.Length;
			}
			return vals;
		}
		public static Value Operate3(Value AV, Value BV, Value CV, Value vals, Func<T, T, T, T> o) {
			Value[] A = (AV = CollapseScalar(AV)).Values, B = (BV = CollapseScalar(BV)).Values, C = (CV = CollapseScalar(CV)).Values;
			int a = 0, b = 0, c = 0, s = Math.Max(C.Length, Math.Max(A.Length, B.Length));
			vals.Values = new Value[s];
			for (int an, bn, cn, i = 0; i < s; ++i) {
				vals.Values[i] = (an = (A[i] = CollapseScalar(A[i])).Values.Length) 
					+ (bn = (B[i] = CollapseScalar(B[i])).Values.Length) 
					+ (cn = (C[i] = CollapseScalar(C[i])).Values.Length) == 0 
					? new(o(A[i].Leaf, B[i].Leaf, C[i].Leaf)) 
					: Operate3(
						an == 0 ? new([new(A[i].Leaf)]) : A[i],
						bn == 0 ? new([new(B[i].Leaf)]) : B[i],
						cn == 0 ? new([new(C[i].Leaf)]) : C[i],
						new(new Value[Math.Max(cn, Math.Max(an, bn))]), o);
				a = (a + 1) % A.Length;
				b = (b + 1) % B.Length;
				c = (c + 1) % C.Length;
			}
			return vals;
		}
		public Value Copy() => new(Leaf, Op, Arg, Term, Operand, Op.Negative, Text) { Values = CopyValues(Values) };
		public Value[] CopyValues(Value[] c) {
			var v = new Value[c.Length];
			for (int i = 0; i < c.Length; ++i)
				v[i] = c[i].Copy();
			return v;
		}
	}
	
}
public class ComparserR : Comparser<Real> { protected override Value DefaultConstants() => new(); }
public class ComparserC : Comparser<Complex> { protected override Value DefaultConstants() => new([new(Complex.i, "i")]); }
public class ComparserQ : Comparser<Quaternion> { protected override Value DefaultConstants() => new([new(Quaternion.i, "i"), new(Quaternion.i, "j"), new(Quaternion.i, "k")]); }
