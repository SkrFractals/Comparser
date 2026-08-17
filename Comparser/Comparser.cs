using Comparser.Comparser.Numbers;
using System.Text.RegularExpressions;
namespace Comparser.Comparser;
public interface IComparser {
	public object Parse(string text, object? args = null);
	// Re-evaluates already parsed expression with new arguments
	public object Eval(object exp, object? args = null);
	// Parses and evaluates the text with selected arguments (and returns the expression for possible re-evaluation)
	public object ParseEval(string text, out object expr, object? args = null);
	// Parses and evaluates the text with selected arguments (and returns the expression for possible re-evaluation, and eats the parsed part, allows it to be incomplete - leaving the remainder in the ref text)
	public object ParseEval(ref string text, object? args = null);
	// Parses and evaluates the text with selected arguments (without returning the parsed expression, only immediate one-time evaluation)
	public object ParseEval(string text, object? args = null);
	public string ToString(object value, int decimals = -1);
	public string ReadCode(string text);
}

public abstract partial class Comparser<T>(ushort stackOverflowLimit = 9999) : IComparser where T : INumber<T> {
	static Comparser() {
		CallFunction min, max, prod, vec, ln, nsinhc, nsinc, re, im, sign, neg, inv, abs, conj, cub;
		DefaultFunctions = new() {
			// meta
			["eval"] = new Ce(typeof(FuncEval), 0), // attempts to parse and evaluate every Text in the input
			["count"] = new Ce(typeof(FuncCount), 0), // counts the number of elements in the vector
			["concat"] = new Ce(typeof(FuncCat), 0), // Un-nests the vectors: concat((1,2),3,((4,5),6)) = (1,2,3,4,5,6)
			["cat"] = new Ce(typeof(FuncCat), 0), // Un-nests the vectors: concat((1,2),3,((4,5),6)) = (1,2,3,4,5,6)
			["true"] = new Cf((x) => T.MakeR(T.Re(INumber<T>.SqrAbs(x)) >= 1 ? 1 : 0)), // = size >= 1
			["false"] = new Cf((x) => T.MakeR(T.Re(INumber<T>.SqrAbs(x)) < 1 ? 1 : 0)), // = size < 1

			// double arguments:
			["minimum"] = min = new Cf2(T.Min), // component-wise minimum
			["maximum"] = max = new Cf2(T.Max), // component-wise maximum
			["min"] = min, // component-wise minimum
			["max"] = max, // component-wise maximum
			["softmax"] = new Cf2(INumber<T>.SoftMax),
			["softmin"] = new Cf2(INumber<T>.SoftMin),
			["clamp"] = new Cf3(T.Clamp), // component-wise clamp

			// quadruple arguments
			["product"] = prod = new Ce(typeof(Product), 0), // iterative product
			["prod"] = prod, // iterative product
			["sum"] = new Ce(typeof(Sum), 0), // iterative sum
			["vector"] = vec = new Ce(typeof(Vector), 0), // iterative vector builder
			["vec"] = vec, // iterative vector builder

			// exp/log
			["exp10"] = new Cf(INumber<T>.Exp10), // 10^x
			["exp2"] = new Cf(INumber<T>.Exp2), // 2^x
			["exp"] = new Cf(T.Exp), // e^x
			["log10"] = new Cf(INumber<T>.Log10), // log_10(x)
			["log2"] = new Cf(INumber<T>.Log2), // log_2(x)
			["log"] = ln = new Cf(T.Log), // ln(x)
			["ln"] = ln, // ln(x)

			// sincs
			["nsinhc"] = nsinhc = new Cf(INumber<T>.Nsinhc),
			["sinchpi"] = nsinhc,
			["sinhc"] = new Cf(INumber<T>.Sinhc),
			["nsinc"] = nsinc = new Cf(INumber<T>.Nsinc),
			["sincpi"] = nsinc,
			["sinc"] = new Cf(INumber<T>.Sinc),
			["cosc"] = new Cf(INumber<T>.Cosc),

			// arc hyperbolics
			["acosh"] = new Cf(T.Acosh),
			["asinh"] = new Cf(T.Asinh),
			["atanh"] = new Cf(T.Atanh),
			["asech"] = new Cf(INumber<T>.Asech),
			["acsch"] = new Cf(INumber<T>.Acsch),
			["acoth"] = new Cf(T.Acoth),

			// hyperbolics
			["cosh"] = new Cf(T.Cosh),
			["sinh"] = new Cf(T.Sinh),
			["tanh"] = new Cf(T.Tanh),
			["sech"] = new Cf(INumber<T>.Sech),
			["csch"] = new Cf(INumber<T>.Csch),
			["coth"] = new Cf(T.Coth),

			// arc trigs
			["acos"] = new Cf(T.Acos),
			["asin"] = new Cf(T.Asin),
			["atan"] = new Cf(T.Atan),
			["asec"] = new Cf(INumber<T>.Asec),
			["acsc"] = new Cf(INumber<T>.Acsc),
			["acot"] = new Cf(T.Acot),

			// trigs
			["cos"] = new Cf(T.Cos),
			["sin"] = new Cf(T.Sin),
			["tan"] = new Cf(T.Tan),
			["sec"] = new Cf(INumber<T>.Sec),
			["csc"] = new Cf(INumber<T>.Csc),
			["cot"] = new Cf(T.Cot),

			// basics/components
			["real"] = re = new Cf(INumber<T>.T_Re),
			["re"] = re,
			["imag"] = im = new Cf(INumber<T>.T_I),
			["im"] = im,
			["frac"] = new Cf(T.Frac),
			["trunc"] = new Cf(T.Trunc),
			["floor"] = new Cf(T.Floor),
			["round"] = new Cf(T.Round),
			["ceil"] = new Cf(T.Ceil),
			["sign"] = sign = new Cf(INumber<T>.Sign),
			["sgn"] = sign,
			["negative"] = neg = new Cf(INumber<T>.Neg),
			["neg"] = neg,
			["inverse"] = inv = new Cf(T.Inv),
			["inv"] = inv,
			["absri"] = new Cf(T.AbsComp),
			["sqrabs"] = new Cf(INumber<T>.SqrAbs),
			["absolute"] = abs = new Cf(INumber<T>.T_Abs),
			["abs"] = abs,
			["norm"] = abs,
			["arg"] = new Cf(INumber<T>.T_Arg),
			["conjugate"] = conj = new Cf(INumber<T>.Conj),
			["conj"] = conj,

			// powers
			["sqrt"] = new Cf(T.Sqrt),
			["cbrt"] = new Cf(INumber<T>.Cbrt),
			["sqr"] = new Cf(T.Sqr),
			["cube"] = cub = new Cf(T.Cub),
			["cub"] = cub,
			["quart"] = new Cf(T.Quart),

			// specials
			["fact"] = Fact,
			["factorial"] = Fact,
			["gauss"] = new Cf(T.Gauss),
			["gamma"] = new Cf(T.Gamma),
			["zeta"] = new Cf(T.Zeta),
			["softabs"] = new Cf(INumber<T>.SoftAbs),
			["softneg"] = new Cf(INumber<T>.SoftNeg)
		};
	}

	#region Interface
	private static Value AsInput(object? e) => e as Value ?? new Value();
	public object Parse(string text, object? args) => new Expression(this, text, AsInput(args), 0);
	public object Eval(object exp, object? args) => (exp as Expression ?? new Expression(this, "", None, 0)).Eval(AsInput(args));
	public object ParseEval(string text, out object expr, object? args) { var e = (Expression)Parse(text, args); expr = e; return e.Eval(AsInput(args)); }
	public object ParseEval(ref string text, object? args) => new Expression(this, ref text, out _, AsInput(args), 0).Eval(AsInput(args));
	public object ParseEval(string text, object? args) => ParseEval(ref text, args);
	public string ToString(object value, int decimals = -1) => AsInput(value).ToString(decimals);
	public string ReadCode(string text) {
		Dictionary<string, List<(Value, Expression, Expression?)>> parsedF = [];
		List<Value> parsedC = [];
		int returnIndex = -1, i, skip = 0;
		var comment = false;
		string codeLine, log = "";
		_customFunctions = [];
		ReadLines("", text.Split('\n'));
		return log;
		
		void ReadLines(string pref, string[] code) {
			//_context.CustomFunctions = new Comparser.CallFunction[CustomFunctions.Count];
			for (i = 0; i < code.Length; ++i) {
				codeLine = code[i];
				while (codeLine.Length > 0)
					ReadLine(i.ToString());
				continue;

				void ReadLine(string line) {
					codeLine = codeLine.TrimStart(' ').TrimEnd(' ');
					if (codeLine == "")
						return;
					// skip comment and braces:
					var plus = codeLine.IndexOf('}');
					var minus = codeLine.IndexOf('{');
					var comm = codeLine.IndexOf('/');
					var eat = true;
					while (eat && (!comment && skip > 0 && (plus >= 0 || minus >= 0) || comment && comm >= 0)) {
						var best = comm < plus && comm < minus;
						if ((comment || best) && comm >= 0) {
							if (comment) {
								comment &= codeLine[comm - 1] != '*' || comm + 1 < codeLine.Length && codeLine[comm + 1] == '*';
								EatComm();
							} else if (best) {
								comment |= comm + 1 < codeLine.Length && codeLine[comm + 1] == '*';
								EatComm();
							}
							void EatComm() { eat = Eat(comm); comm = codeLine.IndexOf('/'); }
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
						continue;
						
						bool Eat(int from) {
							minus -= from;
							plus -= from;
							comm -= from;
							return (codeLine = codeLine[(from + 1)..]).Length > 0;
						}
					}
					// read format: (<cacheIntExpression>)functionname(<argumentsExpression>)=<definitionExpression>

					int cache = 1;
					string e, name = "";
					Value eval;
					switch (codeLine[0]) {
						case '/':
							if (codeLine.Length > 1 && codeLine[1] == '*') 
								comment = true;
							else
								Cl();
							return;
						case '}': // this must be and out of a branch I'm in, jus ignore it
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
							if ("" != (e = FailEval(ref codeLine, out eval, '{') ? "Failed to parse condition." : "")) {
								Lg(); Cl(); return;
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
							if ("" != (e = FailEval(ref codeLine, out eval) ? "Failed to parse cache size."
							: eval.Values.Length != 1 ? "Multiple values in the cache size expression: " + eval
							: eval.Values[0].Leaf.IsNaN() ? "Cache size evaluated as NaN." : "")) {
								Lg();
								Cl();
								return;
							}
							cache = (int)Math.Round(T.Re(eval.Values[0].Leaf));
							goto default;
						default:
							Value args = None;
							if ((minus = codeLine.IndexOf('=')) < 0) {
								Cl();
								return;
							}
							var (s, s1) = (codeLine[..minus], codeLine[(minus + 1)..]);
							if ("" != (e = s.Length < 2 ? "There must be an equal sign between the name and definition." : (s1 = Clean(s1).TrimStart(' ').TrimEnd(' ')).Length == 0 ? "No expression after the equal sign." : "")) {
								Err();
								return;
							}
							int l = (s = Clean(s)).IndexOf('(');
							if ("" != (e = l < 0
								? IsAlphaNumeric(name = s) ? "" : Fn()
								: l < 1 ? "No name."
									: !IsAlphaNumeric(name = s[..l].TrimEnd(' ')) ? Fn()
									: FailArgs(s[(l + 1)..], out args) ? "Failed to parse arguments." : "")) {
								Err();
								return;
							}
							if (name == "") {
								e = "No name.";
								Err();
								return;
							}
							if (args.Values.Length == 0) { // it is a constant or code call:
								if (cache != 1) {
									e = name + " is a constant/call and doesn't support caching.";
									Lg();
								}
								eval = new Expression(this, s1, args, 0).Eval(None, name);
								switch (name) {
								case "print":
									log += eval + "\n";
									End();
									return;
								case "do":
									var cl = codeLine;
									var expand = eval.ToLines().Split('\n'); // ToString(true) recursively exports only string values as lines
									ReadLines(pref + line + "/", expand);
									codeLine = cl;
									End();
									return;
								}
								End();
								foreach (var p in parsedC)
									if (p.Text == name) {
										// mutate variable
										p.Values = eval.Values;
										p.Leaf = eval.Leaf; // probably not needed?
										_customConstants = new([.. parsedC]);
										return;
									}
								// new variable
								parsedC.Add(eval);
								_customConstants = new([.. parsedC]);
								return;
							} // it is a function:
							Expression cond = new(this, ref s1, out _, args, 0, cache);
							s1 = s1.TrimStart(' ');

							if (s1[0] == '?') {
								Expression falseExp, trueExp = new(this, ref s1, out _, args, 0, cache);
								s1 = s1.TrimStart(' ');
								if (s1[0] == ':')
									falseExp = new(this, ref s1, out _, args, 0, cache);
								else {
									e = "Failed to find : after the true expression.";
									Err();
									return;
								}
								AddF((args, trueExp, cond));
								parsedF[name].Add((args, falseExp, null));
								_customFunctions[name] =  new CallCustom([.. parsedF[name]]);
								return;
							}
							AddF((args, cond, null));
							_customFunctions[name] =  new CallCustom([.. parsedF[name]]);
							return;

							void AddF((Value, Expression, Expression?) f) {
								if (!parsedF.ContainsKey(name))
									parsedF[name].Add(f);
								else parsedF[name] = [f];
							}
							void Err() { Lg(); End(); }
							void End() => codeLine = (minus = s.IndexOf(';')) >= 0 ? s[(minus + 1)..] + s1 : (minus = s1.IndexOf(';')) >= 0 ? s1[(minus + 1)..] : "";
							void Lg() => log += "Line " + pref + line + ": " + e + "\n";
					}
					return;
					
					bool FailEval(ref string exp, out Value evaluated, char close = ')') {
						exp = exp[1..];
						evaluated = new Expression(this, ref exp, out _, None, 0).Eval(None);
						if (exp[0] != close)
							return true; // closing parenthesis after the argument expression
						exp = exp[1..];
						return false;
					}
					void Cl() => codeLine = (minus = codeLine.IndexOf(';')) < 0 ? "" : codeLine[(minus + 1)..];
					string Fn() => "Failed to parse name: " + name;
					bool FailArgs(string exp, out Value args) => FailEval(ref exp, out args) || exp.TrimEnd(' ') != "";
					static bool IsAlphaNumeric(string strToCheck) => MyRegex().IsMatch(strToCheck);
					static string Clean(string t) // forbidden symbols in expressions
						=> t.ToLower().Replace(":", "").Replace(";", "").Replace("|", "")
						.Replace("\t", "").Replace("\r", "").Replace("\n", "");
				}
			}
		}
	}
	#endregion

	#region Content
	private readonly ushort _stackOverflow = stackOverflowLimit;
	static protected readonly Value None = new();
	private Dictionary<string, CallFunction> _customFunctions = []; // user-defined functions
	private Value _customConstants = None; // user-defined constants
	private static readonly Cf Fact = new(T.Factorial);
	//protected abstract CallFunction[] DefaultFunctions();
	private static readonly Dictionary<string, CallFunction> DefaultFunctions;
	protected abstract Value GenericConstants();
	private static readonly Value DefaultConstants = new([
		new(INumber<T>.C_Pi(), "pi"), 
		new(INumber<T>.C_Tau(), "tau"), 
		new(INumber<T>.C_E(), "e"), 
		new(INumber<T>.C_Gamma(), "gamma"), 
		new(T.One(), "one")]);
	#endregion

	#region Call Functions
	// abstract parent
	public abstract class CallFunction(int cacheSize = 1) {
		//protected readonly I[]? Def = def;
		//public readonly string Name = name;
		public readonly EvalCache Cache = new(cacheSize);
		public abstract Expression Call(Comparser<T> context, ref string text, Value args, ushort depth);
		// how to use: e.Insert(args, e.GetEval(args) ? e.result.Eval : base.Eval([], args).v); 
		public class EvalCache(int size = 1) {
			private int _filled;
			private Evaluated? _cache;
			public Evaluated? Result;
			public bool GetEval(Value args) {
				for (var c = Result = _cache; c != null; Result = c, c = c.Next) {
					if (c.Args.Match(args)) {
						if (Result != c) {
							Result!.Next = c.Next;
							c.Next = _cache;
							_cache = c;
						}
						Result = c;
						return true;
					}
					if (c.Next == null) break;
				}
				return false;
			}
			public Value Insert(Value args, Value eval) {
				if (size <= _filled) Result?.Next = null; else ++_filled;
				if (size > 0) _cache = new Evaluated(_cache, args, eval);
				return eval;
			}
			//public void Reset() { _filled = 0; cache = null; }
		}
		public class Evaluated(Evaluated? next, Value args, Value eval) {
			public readonly Value Eval = eval.Copy();
			public readonly Value Args = args.Copy();
			public Evaluated? Next = next;
		}
	}
	// Expressions.Functions:
	private class Ce(Type type, int cache = 1) : CallFunction(cache) {
		public override Expression Call(Comparser<T> context, ref string text, Value args, ushort depth) {
			object[] a = [context, this, text, args, depth]; // activator arguments
			var n = (FunctionExpression)Activator.CreateInstance(type, a)!;
			text = (string)a[1]; // ref string text
			return n;
		}
	}
	// Single/Double/Triple argument delegated functions
	public class Cf(Func<T, T> del, int cache = 1) : CallFunction(cache) {
		public override Expression Call(Comparser<T> context, ref string text, Value args, ushort depth) => new FuncOperator(context, this, del, ref text, args, depth);
	}
	public class Cf2(Func<T, T, T> del, int cache = 1) : CallFunction(cache) {
		public override Expression Call(Comparser<T> context, ref string text, Value args, ushort depth) => new FuncOperator2(context, this, del, ref text, args, depth);
	}
	public class Cf3(Func<T, T, T, T> del, int cache = 1) : CallFunction(cache) {
		public override Expression Call(Comparser<T> context, ref string text, Value args, ushort depth) => new FuncOperator3(context, this, del, ref text, args, depth);
	}
	#endregion

	#region Function Expressions - Operators
	protected abstract class FunctionExpression : Expression {
		private readonly CallFunction _parent;
		protected FunctionExpression(Comparser<T> context, CallFunction parent, ref string text, Value args, ushort depth) : base(context, ref text, out _, args, depth) => _parent = parent;
		protected FunctionExpression(Comparser<T> context, CallFunction parent, Value input, ushort depth) : base(context, input, depth) => _parent = parent;
		public override Value Eval(Value args, string text = "") {
			var v = base.Eval(args, text);
			return _parent.Cache.Insert(v, _parent.Cache.GetEval(v) ? _parent.Cache.Result?.Eval! : EvalF(v, args));
		}
		protected abstract Value EvalF(Value v, Value args);
	}
	private class FuncTextOperator(Comparser<T> context, CallFunction parent, Func<string, Value> del, ref string text, Value args, ushort depth)  : FunctionExpression(context, parent, ref text, args, depth) {
		//private readonly Func<string, Value> Del;
		//public FuncTextOperator: base => Del = del;
		//public FuncTextOperator(Comparser<T> context, CallFunction parent, Func<string, Value> del, Value args, ushort depth) : base(context, parent, args, depth) => Del = del;
		override protected Value EvalF(Value v, Value args) => Value.OperateText(v, V, del);
	}
	private class FuncEval(Comparser<T> context, CallFunction parent, ref string text, Value args, ushort depth, int cache = 0) 
		: FuncTextOperator(context, parent, (x) => new Expression(context, x, args, depth, cache).Eval(args), ref text, args, depth) { }
	private class FuncOperator : FunctionExpression {
		private readonly Func<T, T> _del;
		public FuncOperator(Comparser<T> context, CallFunction parent, Func<T, T> del, ref string text, Value args, ushort depth) : base(context, parent, ref text, args, depth) => _del = del;
		public FuncOperator(Comparser<T> context, CallFunction parent, Func<T, T> del, Value input, ushort depth) : base(context, parent, input, depth) => _del = del;
		override protected Value EvalF(Value v, Value args) => Value.Operate(v, V, _del);
	}
	private class FuncOperator2(Comparser<T> context, CallFunction parent, Func<T, T, T> comp, ref string text, Value args, ushort depth) : FunctionExpression(context, parent, ref text, args, depth) {
		override protected Value EvalF(Value v, Value args) {
			Value m = v.Values[0], s = new();
			for (var c = 1; c < v.Values.Length; m = Value.Operate2(m, v.Values[c++], s, comp, (x, _) => x).Copy()) { }
			return s;
		}
	}
	private class FuncOperator3(Comparser<T> context, CallFunction parent, Func<T, T, T, T> comp, ref string text, Value args, ushort depth) : FunctionExpression(context, parent, ref text, args, depth) {
		override protected Value EvalF(Value v, Value args) => v.Values.Length == 3 ? Value.Operate3(v.Values[0], v.Values[1], v.Values[2], V, comp) : new();
	}
	#endregion

	#region Function Expressions - Vectors
	// extracts terms from a vector using indices in: [expression]. Example: (0a,1b,2c,(30d,31e),5f)[3,2,(5,1,3)] = (30d,31e),2c,(5,1,(30d,31e))
	private class FuncIndex(Comparser<T> context, Value indices, Value input, ushort depth) : Expression(context, input, depth) {
		public override Value Eval(Value args, string text = "") {
			List<Value> r = [];
			Operate(ref r, indices, base.Eval(args, text));
			return new([.. r]);
		} 
		private static void Operate(ref List<Value> r, Value indices, Value v) {
			v = CollapseScalar(v);
			var iV = CollapseScalar(indices).Values;
			int s = iV.Length;
			for (var i = 0; i < s; ++i)
				if (0 == (iV[i] = CollapseScalar(iV[i])).Values.Length) {
					var index = T.Re(iV[i].Leaf);
					int integer;
					if (double.IsNaN(index) || (integer = (int)Math.Round(index)) < 0 || integer >= v.Values.Length)
						r.Add(None);
					else
						r.Add(CollapseScalar(v.Values[integer]));
				} else {
					List<Value> nr = [];
					Operate(ref nr, iV[i], v);
					r.Add(new([.. nr]));
				}
		}
	}
	private class FuncCat(Comparser<T> context, Value input, ushort depth) : Expression(context, input, depth) {
		public override Value Eval(Value args, string text = "") {
			List<Value> r = [];
			Operate(ref r, base.Eval(args, text));
			return new([.. r]);
		}
		private static void Operate(ref List<Value> r, Value v) {
			var vV = v.Values;
			var s = vV.Length;
			for (var i = 0; i < s; ++i)
				if (0 == vV[i].Values.Length) r.Add(new(vV[i].Leaf));
				else Operate(ref r, vV[i]);
		}
	}
	// counts the elements in a vector
	protected abstract class FuncCount(Comparser<T> context, ref string text, Value args, ushort depth) : Expression(context, ref text, out _, args, depth) {
		public override Value Eval(Value args, string text = "") => new(CollapseScalar(base.Eval(args, text)).Values.Length);
	}
	// iterative sum/product: name(<index>,<from>,<to>,expression(k<index>))
	// "to" can be smaller than "from", works both ways (does not return additive/multiplicative identity when in the wrong order, just iterates backwards)
	private abstract class Iterator(Comparser<T> context, CallFunction parent, ref string text, Value args, ushort depth) : FunctionExpression(context, parent, ref text, args, depth) {
		override protected Value EvalF(Value v, Value args) {
			int iterator = (int)Math.Round(T.Re(v.Values[0].Leaf)), 
				from = (int)Math.Round(T.Re(v.Values[1].Leaf)), 
				to = (int)Math.Round(T.Re(v.Values[2].Leaf));
			var iteratorIndex = args.Values.Length;
			Value ni = new(new Value[iteratorIndex + 1]);
			Array.Copy(args.Values, ni.Values, iteratorIndex);
			ni.Values[iteratorIndex].Text = "k" + iterator;
			return Result(EvalK, from, to);
			Value EvalK(int f) {
				ni.Values[iteratorIndex].Leaf = T.MakeR(f);
				return EvalSingle(3, ni);
			}
		}
		virtual protected void Op(ref Value result, Value iteration) => result = iteration;
		protected abstract Value Result(Func<int, Value> eval, int from, int to);
		static protected void Iterate(Action<int> iter, int from, int to) {
			// add the other iterations all the way to "to"
			while (from < to) iter(++from);
			while (from > to) iter(--from);
		}
	}
	private abstract class CollapseIterator(Comparser<T> context, CallFunction parent, ref string text, Value args, ushort depth) : Iterator(context, parent, ref text, args, depth) { 
		override protected Value Result(Func<int, Value> eval, int from, int to) {
			var sum = eval(from); // prepare first iteration as the initial vector
			Iterate(IterK, from, to);
			return sum;
			void IterK(int f) {
				var v = eval(f);
				Op(ref sum, v);
				//for (var j = v.Values.Length; 0 <= --j; Op(ref sum[j], v[j])) { }
			}
		}
	}
	// return a vector of sums of iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (x,2x); sum(0,1,3,exp(k0)) => (1+2+3,2+4+6) => (6,12); // 6 is the sum of x term, evaluated with k0=1..3, 12 is the sum of 2x term, evaluated with k0=1..3
	private class Sum(Comparser<T> context, CallFunction parent, ref string text, Value args, ushort depth) : CollapseIterator(context, parent, ref text, args, depth) { 
		override protected void Op(ref Value result, Value iteration) => result = Value.Operate2(result, iteration, new(), INumber<T>.Add, (x, y) => x + y);
	}
	// return a vector of products of iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (x,2x); prod(0,1,3,exp(k0)) => (1*2*3,2*4*6) => (6,48); // 6 is the product of x term, evaluated with k0=1..3, 48 is the product of 2x term, evaluated with k0=1..3
	private class Product(Comparser<T> context, CallFunction parent, ref string text, Value args, ushort depth) : CollapseIterator(context, parent, ref text, args, depth) {
		override protected void Op(ref Value result, Value iteration) => result = Value.Operate2(result, iteration, new(), INumber<T>.Mul, (x, _) => x);
	}
	// returns a vector of first elements of evaluated iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (3x,2x,4x); vector(0,1,3,exp(k0)) => (3*1,3*2,3*3) => (3,6,9); // only took the first 3x term, evaluated with k0=1..3
	private class Vector(Comparser<T> context, CallFunction parent, ref string text, Value args, ushort depth) : Iterator(context, parent, ref text, args, depth) {
		override protected Value Result(Func<int, Value> eval, int from, int to) { 
			var size = Math.Abs(from - to);
			Value sum = new(new Value[size]) { Values = { [0] = eval(from).Values[0] } };
			var iteratorIndex = 0;
			Iterate(IterK, from, to);
			return sum;
			void IterK(int f) => Op(ref sum.Values[++iteratorIndex], eval(f).Values[0]);
		}
	}
	#endregion

	#region Function Expressions - Custom
	// User defined custom expression functions
	public class CallCustom(/*string name, */(Value input, Expression def, Expression? condition)[] def, int cache = 1) : CallFunction(cache) {
		public readonly (Value input, Expression def, Expression? condition)[] Def = def;
		public override Expression Call(Comparser<T> context, ref string text, Value args, ushort depth) => new CustomFunc(context, this, ref text, args, depth);
	}
	private class CustomFunc(Comparser<T> context, CallCustom parent, ref string text, Value args, ushort depth) : FunctionExpression(context, parent, ref text, args, depth) {
		override protected Value EvalF(Value v, Value i) {
			var match = -1; for (var m = 0; m < parent.Def.Length; ++m)
				if (parent.Def[m].input.Match(v)) {
					match = m; break;
				}
				//var ok = true; for (var id = 0; id < parent.Def[m].input.Values.Length; ++id) ok &= parent.Def[m].input[id].Match(v[id]);if (ok) { match = m; break; }
			return match == -1 || Cond(parent.Def[match].condition, v) ? None : parent.Def[match].def.Eval(v); // failed to match any available argument list ? else eval.
		}
		private static bool Cond(Expression? e, Value v) {
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
		public bool Negative;
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
	//private static int SafeCollapse(Value[] v, int i) => v.Length == 0 ? 0 : (v[i] = CollapseScalar(v[i])).Values.Length;
	//private static T SafeLeaf(Value[] v, int i) => v.Length == 0 ? T.NaN() : v[i].Leaf;
	//private static string SafeText(Value[] v, int i) => v.Length == 0 ? "" : v[i].Text;
	[GeneratedRegex(@"^[a-zA-Z0-9\s,]*$")]
	private static partial Regex MyRegex();
	#endregion

	public class Expression {
		// Protection against infinite loops, causing a stack coverflow NaN result
		protected readonly ushort Depth;
		// Contains user-defined custom function
		protected readonly Comparser<T> Context;
		// Parsed and evaluated data
		protected readonly Value V;
		// Cache for remembering recently evaluated arguments
		private readonly CallFunction.EvalCache _cache;
		/// <summary>
		/// Evaluates the expression
		/// </summary>
		/// <param name="args">arguments</param>
		/// <param name="text">string argument to insert into the field</param>
		/// <returns>Evaluated value of this expression</returns>
		public virtual Value Eval(Value args, string text = "") {
			if (_cache.GetEval(args)) return _cache.Insert(args, _cache.Result?.Eval!);
			for (var e = 0; e < V.Values.Length; _ = EvalSingle(e++, args)) { }
			if (text != "") V.Text = text;
			_cache.Insert(args, V);
			return V;
		}
		protected Value EvalSingle(int arrayIndex, Value args) {
			Value ee; var a = args.Values; var v = V.Values;

			var term = (ee = v[arrayIndex]).Term?.Eval(args) ?? new([ee.Arg < 0 || ee.Arg >= a.Length ? ee : a[ee.Arg]]);
			var operand = ee.Operand?.Eval(args) ?? None;
			return arrayIndex < v.Length ? Value.Operate2(term, operand, ee, ee.Op.Op, ee.Op.SOp) : None;
			
			
			
			/*return arrayIndex < v.Length ? Value.Operate2(
				(ee = v[arrayIndex]).Term?.Eval(args) ?? new([ee.Arg < 0 || ee.Arg >= a.Length ? ee : a[ee.Arg]]),
				ee.Operand?.Eval(args) ?? None, ee, ee.Op.Op, ee.Op.SOp) : None;*/
		}
		public Expression(Comparser<T> context, string text, Value args, ushort depth, int cache = 0) {
			var e = new Expression(Context = context, ref text, out _, args, Depth = depth, cache);
			V = text == "" ? e.V : None;
			_cache = new(cache);
		}
		/// <summary>
		/// Reads adn parses an expression string
		/// </summary>
		/// <param name="context">context that contains custom callable functions/constants</param>
		/// <param name="text">string to parse</param>
		/// <param name="nextOp">returns operand's operator if that operand should be left-associated with my term, will encapsulate previous operator into my term, and use nextOp on next operand</param>
		/// <param name="args">argument value, will substitute every x in the string</param>
		/// <param name="depth">function calling depth, for triggering stack overflow</param>
		/// <param name="cache">cache size of this new Expression</param>
		/// <param name="left">what oreder of operations was my parent's operator? Used to test for associativity</param>
		public Expression(Comparser<T> context, ref string text, out Operator nextOp, Value args, ushort depth, int cache = 0, byte left = 0) {
			// DEBUG
			//var origtext = text;
			Depth = depth;
			_cache = new(cache);
			Context = context;
			List<Value> expr = [];
			while (Read(ref text, out nextOp) && left == 0 && Char(ref text, ',')) ; // only left == 0 (aka top layer expression) should accept ',' for a next value
			V = new([..expr]);
			// DEBUG
			/*var vO = V.Op;
			var vl = V.Leaf;
			var arg = V.Arg;
			var v0 = V.Values.Length == 0 ? null : V.Values[0];
			var v0L = v0 == null ? T.NaN() : v0.Leaf;
			var v0arg = v0?.Arg ?? -1;
			var v0O = v0?.Op.Order;
			var v0A = v0?.Term;
			var v0B = v0?.Operand;
			var v0AL = v0A == null ? T.NaN() : v0A.V.Leaf;
			var v0AO = v0A?.V.Op.Order;
			var v0Av0 = (v0A?.V.Values.Length ?? 0) > 0 ? v0A!.V.Values[0] : null;
			var v0Av0arg = v0Av0?.Arg ?? -1;*/
			return;

			bool Read(ref string text, out Operator nextOp) {
				// Init read
				Value r = new();
				var a = -1;
				expr.Add(r);
				nextOp = r.Op = new();
				text = text.TrimStart(' ');
				r.Op.Negative = Negative(ref text);
				r.Leaf = T.NaN();
				text = text.TrimStart(' ');
				//if (End(text))
				//	return false; // unexpected end
				// Try parenthesis/function/number/constant/argument:
				if ((!Char(ref text, '(') || SubTerm(out r.Term, ref text, ')'))
					&& Func(DefaultFunctions, ref text) // _term = default function //&& Func(context.DefaultFunctions(), ref r, ref text) // _term = default function
					&& Func(context._customFunctions, ref text) // _term = custom function
					&& (Number(ref text, out var n) // _value = number
						|| Constant(ref text, DefaultConstants, out n, out a) // _value = generic constants (pi/tau/e/i/gamma/one)
						|| Constant(ref text, context.GenericConstants(), out n, out a) // _value = constant (i/j/k/x/y/z...)
						|| Constant(ref text, context._customConstants, out n, out a) // _value = user constant
						|| Constant(ref text, args, out n, out a, true)))
					r.Term = new(context, a < 0 ? n : new Value(a), depth, cache);//r.Value = n; // _value = argument (x/y/z/t...)
				else if (Fail(r) && F()) // failed to read a term/value
					return false; //  unexpected end fail
				text = text.TrimStart(' ');
				if (End(text)) // unexpected ')', or no op, and return back successful
					return true; 
				// Read operators/comments:
				Operator o;
				while ((o = text[0] switch { 
					'+' => new Add(), '-' => new Sub(), '*' => new Mul(), '/' => new Div(), '\\' => new LDiv(), '^' => new Pow(), '%' => new Mod(),
					'=' => new Equal(),	'<' => new Less(text.Length > 1 && text[1] == '='),	'>' => new More(text.Length > 1 && text[1] == '='),
					'[' => new Index(), '!' => new Exclamation(), _ => new Mul(false) }).GetType() switch {
					var x when x == typeof(Exclamation) => NotUnequal(text) && Eat(1, ref text) && Encapsulate(new FuncOperator(context, Fact, T.Factorial, expr[^1], depth)), // factorial
					var x when x == typeof(Index) => ExtractTerms(ref text), // index
					var x when x == typeof(Div) => text[1] == '*' && Comment(ref text), // comment
					_ => false
				}) {
					text = text.TrimStart(' ');
					if (o.Order == 0) return true;
				}
				while (o.EatOp-- > 0) // not an operator-less multiplication?
					_ = Eat(1, ref text); // eat operator
				o.Negative = r.Op.Negative; // move negative flag to the new operator
				if (LeftAssociate(o)) {
					nextOp = o; // perform left-associativity by returning back, and the parent will encapsulate
					return false;
				}
				// Read operand:
				while (true) {
					if (Fail((r.Operand = new Expression(context, ref text, out o, args, depth, cache, (r.Op = o).Order)).V)) {
						if (r.Op.EatOp >= 0 && F())
							return false; // failed to read operand
										  // if it was operator-less multiplication - assume it was an expression end instead
						r.Op = new();
						break;
					}
					if (o.Order == 0)
						break;
					// operand's next op has lower or equal order priority:
					// encapsulate my term into another term (wrap my term into parentheses), take the next operator and find the next operand to use it on
					_ = Encapsulate(new(context, expr[^1], depth, cache));
					if (LeftAssociate(o)) { // need to test associativity again, to let it recurse backwards. otherwise 2^2^2+1 would be 2^(2^2+1)
						nextOp = o; // perform left-associativity by returning back, and the parent will encapsulate
						return false;
					}
				}
				return true;

				bool NotUnequal(string text) {
					if (text.Length <= 1 || text[1] != '=')
						return true; // must be a factorial, keep it
					o = new Exclamation(1); // must be !=, change into that
					return false;
				}
				bool Comment(ref string text) {
					_ = Eat(1, ref text); // eat initial /
					for (var go = true; go;) {
						var i = text.IndexOf('/');
						if (i < 0) { text = ""; o = new(); return true; }
						go = text[i - 1] != '*';
						Eat((byte)(i + 1), ref text);
					}
					return true;
				}
				bool ExtractTerms(ref string text) {
					_ = Eat(1, ref text);
					if (SubTerm(out var indices, ref text, ']'))
						o = new(); // failed to parse indices
					return Encapsulate(new FuncIndex(context, indices.V, expr[^1], depth));
				}
				bool SubTerm(out Expression readTo, ref string text, char req) => (Fail((readTo = new Expression(context, ref text, out _, args, depth)).V) || readTo.V.Values.Length == 0 || FailRequiredSymbol(req, ref text)) && F();
				bool End(string text) => text.Length == 0 || text[0] switch { ')' => true, ',' => true, '{' => true, '}' => true, ';' => true, '?' => true, ':' => true, _ => false };
				bool Encapsulate(Expression p) { expr[^1] = r = new(T.NaN(), new(), -1, p); return true; }
				bool LeftAssociate(Operator testOp) => testOp.Right ? testOp.Order < left : testOp.Order <= left;
				bool Fail(Value test) => test.Term == null && (test.Values.Length == 0 || test.Values[0].Term == null) /*&& test.Values[0].Values.Length == 0*/;// && test.Value.IsNaN; // no longer needed as even values are now nested in terms, and I don't test their insides.
				bool F() { r.Op = new(); r.Leaf = T.NaN(); r.Values = []; r.Term = r.Operand = null; return true; } // reading failed
				bool FailRequiredSymbol(char c, ref string text, int offset = 0) => !Char(ref text, c, offset) && F();
				bool Func(Dictionary<string, CallFunction> f, ref string text) {
					foreach (var t in f) {
						if (t.Key.Length <= 0 // func must have a name
							|| text.Length <= t.Key.Length // text must have enough characters for the func name
							|| text[..t.Key.Length] != t.Key // must match the func name
							|| FailRequiredSymbol('(', ref text, t.Key.Length))  // no argument parentheses found, maybe it's a constant with the same name...?
							continue;
						return (depth > context._stackOverflow // check for stack overflow
							|| Fail((r.Term = t.Value.Call(context, ref text, args, (ushort)(1 + depth))).V) // try to read the arguments
							|| FailRequiredSymbol(')', ref text)) && F(); // must eat func closing parenthesis
					}
					return true;
				}
				bool Negative(ref string text) => Char(ref text, '-');
				bool Number(ref string text, out Value number) {
					if (RealNumber(ref text, out var real)) {
						number = new(T.MakeR(real));
						return true;
					}
					number = None;
					return false;
				}
				bool RealNumber(ref string text, out double number, double l = 0) {
					if (text.Length > 0) {
						if (text[0] == '.') {
							// eat decimal point
							_ = Eat(1, ref text);
							// get fractional part
							number = l + DecimalNumber(ref text);
							return true;
						}
						if (int.TryParse(text[0].ToString(), out var i)) {
							l *= 10;
							// eat another digit
							_ = Eat(1, ref text);
							// add another whole digit, or finish
							_ = RealNumber(ref text, out number, l + i);// && 1 <= n ? 10 * i + n : i + n;
							return true;
						}
					}
					number = l; // no more digits
					return false;
				}
				double DecimalNumber(ref string text, double d = 1) {
					if (text.Length == 0) return 0; // no more digits
					d /= 10; // prepare another position
					if (!int.TryParse(text[0].ToString(), out var i))
						return 0;
					Eat(1, ref text);
					return i * d + DecimalNumber(ref text, d);
				}
				bool Constant(ref string text, Value consts, out Value number, out int foundArg, bool isArg = false) {
					// WARNING, if there is any function with the same name, then you can't operator-less multiply with parentheses from the right!
					// for example gamma is either eulerConstant or the gamma function:
					// gamma2 = eulerConstant*2, gamma(2+1) = evaluates gamma function at 2, (2+1)gamma = (2+1)*eulerConstant
					foundArg = -1;
					//var t = CollapseScalar(a);
					for (var f = 0; f < consts.Values.Length; ++f) {
						var v = consts.Values[f];
						var k = v.Text;
						if (text.Length < k.Length ||
							text[0..k.Length] != k) continue;
						_ = Eat((byte)k.Length, ref text);
						number = v;
						if (isArg)
							foundArg = f;
						return true;
					}
					number = None;
					return false;
				}
				bool Eat(byte c, ref string text) {
					r.Text += text[..c];
					R(c, ref text);
					return true;
				}
			}
			void R(byte c, ref string text) => text = text[c..];
			bool Char(ref string text, char c, int offset = 0) { var r = text.Length > offset && text[offset] == c; if (r) R((byte)(1 + offset), ref text); return r; }
		}
		protected Expression(Comparser<T> context, Value t, ushort depth, int cache = 0) {
			_cache = new(cache);
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
		public Expression? Term, Operand;
		// Operator (eval = term <operator> operand), if it is a pure parent Operator, it only evaluates the term
		public Operator Op = new();
		// Argument index binding (if non-negative, it will get replaced by the argument value with this Arg index)
		public readonly int Arg = -1;
		// Multiple functions:
		// In constant/variable/argument bindings, this is the alias name that will get replaced with the Leaf value whenever it is detected
		// In a string data type, it contains the string value
		// After parsing will contain the original parsed text, even if error occurs (this also naturally works with argument parsing and matching)
		public string Text; // The original text that this input has been parsed from, even if it fails parsing
		public override string ToString() => CollapseScalar(this).Pv(-1);
		public string ToString(int decimals) => CollapseScalar(this).Pv(decimals);
		private string Pv(int decimals, string a = "", string b = "") {
			if (Values.Length <= 0)
				return Leaf.ToString(decimals) ?? "";
			var s = a;
			for (var i = 0; i < Values.Length; ++i) {
				Values[i] = CollapseScalar(Values[i]);
				if (i > 0) s += ", ";
				s += Values[i].Pv(decimals, "(", ")");
			}
			return s + b;
		}
		private string Tl() {
			if (Values.Length <= 0)
				return Text;
			var s = "";
			for (var i = 0; i < Values.Length; ++i)
				s += (Values[i] = CollapseScalar(Values[i])).Tl() + "\n";
			return s;
		}
		public string ToLines() => CollapseScalar(this).Tl();
		public Value(T value, Operator op, int arg = -1, Expression? term = null, Expression? operand = null, bool negative = false, string text = "") {
			Leaf = value; Term = term; Operand = operand; Op = op; Op.Negative = negative; Arg = arg; Text = text;
		}
		public Value(T value, string text = "") { Leaf = value; Text = text;	}
		public Value(string text = "") => Text = text;
		public Value(Value[] values, string text = "") { Values = values; Text = text; }
		public Value(int arg, string text = "") { Arg = arg; Text = text; }
		public bool Match(Value a) { // defArguments.Match(callArguments)
			if (Leaf.IsNaN()) {
				if (Values.Length == 0) return true;
				if (Values.Length != a.Values.Length) return false;
				var m = true;
				for (var i = 0; i < Values.Length; ++i)
					m &= Values[i].Match(a.Values[i]);
				return m;

			} else return T.Compare(Leaf, a.Leaf); // callArguments always starts with Values
		}
		public static Value OperateText(Value av, Value vals, Func<string, Value> o) {
			var vA = (av = CollapseScalar(av)).Values;
			var s = vA.Length;
			vals.Values = new Value[s];
			if (vA.Length == 0) vA = [new(av.Leaf, av.Text)];
			for (int an, a = 0; a < s; ++a)
				vals.Values[a] = (an = (vA[a] = CollapseScalar(vA[a])).Values.Length) == 0 
					? o(vA[a].Text) 
					: OperateText(an == 0 ? new([new(vA[a].Leaf)]) : vA[a], new(new Value[an]), o);
			if (s != 0)
				return vals;
			vals.Leaf = CollapseScalar(o(av.Text)).Leaf;
			vals.Text = av.Text;
			return vals;
		}
		public static Value Operate(Value av, Value vals, Func<T, T> o) {
			int s;
			var vA = (av = CollapseScalar(av)).Values;
			vals.Values = new Value[s = vA.Length];
			if (vA.Length == 0) vA = [new(av.Leaf, av.Text)];
			for (int an, a = 0; a < s; ++a)
				vals.Values[a] = (an = (vA[a] = CollapseScalar(vA[a])).Values.Length) == 0 ? new(o(vA[a].Leaf)) : Operate(vA[a], new(new Value[an]), o);
			if (s != 0)
				return vals;
			vals.Leaf = o(av.Leaf);
			vals.Text = av.Text;
			return vals;
		}
		public static Value Operate2(Value av, Value bv, Value vals, Func<T, T, T> o, Func<string, string, string> so) {
			// DEBUG
			/*var oe = o(T.MakeR(3), T.MakeR(3));
			var avO = av.Op;
			var bvO = bv.Op;
			var avl = av.Leaf;
			var bvl = bv.Leaf;
			var av0 = av.Values.Length == 0 ? null : av.Values[0];
			var bv0 = bv.Values.Length == 0 ? null : bv.Values[0];
			var a0L = av0 == null ? T.NaN() : av0.Leaf;
			var b0L = bv0 == null ? T.NaN() : bv0.Leaf;
			var a0O = av0?.Op ?? null;
			var b0O = bv0?.Op ?? null;*/
			Value[] vA = (av = CollapseScalar(av)).Values, vB = (bv = CollapseScalar(bv)).Values;
			int a = 0, b = 0, s = Math.Max(vA.Length, vB.Length);
			vals.Values = new Value[s];
			if (vA.Length == 0) vA = [new(av.Leaf, av.Text)];
			if (vB.Length == 0) vB = [new(bv.Leaf, bv.Text)];
			for (var i = 0; i < s; ++i) {
				int an, bn;
				vals.Values[i] = (an = (vA[a] = CollapseScalar(vA[a])).Values.Length) 
					+ (bn = (vB[b] = CollapseScalar(vB[b])).Values.Length) == 0
					? new(o(vA[a].Leaf, vB[b].Leaf), so(vA[a].Text, vB[b].Text)) 
					: Operate2(
						an == 0 ? new([new(vA[a].Leaf)]) : vA[a],
						bn == 0 ? new([new(vB[b].Leaf)]) : vB[b],
						new(new Value[Math.Max(an, bn)]), o, so);
				a = (a + 1) % vA.Length;
				b = (b + 1) % vB.Length;
			}
			if (s != 0)
				return vals;
			vals.Leaf = o(av.Leaf, bv.Leaf);
			vals.Text = so(av.Text, bv.Text);
			return vals;
		}
		public static Value Operate3(Value av, Value bv, Value cv, Value vals, Func<T, T, T, T> o) {
			Value[] vA = (av = CollapseScalar(av)).Values, vB = (bv = CollapseScalar(bv)).Values, vC = (cv = CollapseScalar(cv)).Values;
			int a = 0, b = 0, c = 0, s = Math.Max(vC.Length, Math.Max(vA.Length, vB.Length));
			vals.Values = new Value[s];
			if (vA.Length == 0) vA = [new(av.Leaf, av.Text)];
			if (vB.Length == 0) vB = [new(bv.Leaf, bv.Text)];
			if (vC.Length == 0) vC = [new(cv.Leaf, cv.Text)];
			for (int i = 0; i < s; ++i) {
				int an, bn, cn;
				vals.Values[i] = (an = (vA[a] = CollapseScalar(vA[a])).Values.Length) 
					+ (bn = (vB[b] = CollapseScalar(vB[b])).Values.Length) 
					+ (cn = (vC[c] = CollapseScalar(vC[c])).Values.Length) == 0 
					? new(o(vA[a].Leaf, vB[b].Leaf, vC[c].Leaf)) 
					: Operate3(
						an == 0 ? new([new(vA[a].Leaf)]) : vA[a],
						bn == 0 ? new([new(vB[b].Leaf)]) : vB[b],
						cn == 0 ? new([new(vC[c].Leaf)]) : vC[c],
						new(new Value[Math.Max(cn, Math.Max(an, bn))]), o);
				a = (a + 1) % vA.Length;
				b = (b + 1) % vB.Length;
				c = (c + 1) % vC.Length;
			}
			if (s != 0)
				return vals;
			vals.Leaf = o(av.Leaf, bv.Leaf, cv.Leaf);
			vals.Text = av.Text;
			return vals;
		}
		public Value Copy() => new(Leaf, Op, Arg, Term, Operand, Op.Negative, Text) { Values = CopyValues(Values) };
		private static Value[] CopyValues(Value[] c) {
			var v = new Value[c.Length];
			for (var i = 0; i < c.Length; ++i)
				v[i] = c[i].Copy();
			return v;
		}
	}
}
public class ComparserR : Comparser<Real> { override protected Value GenericConstants() => None; }
public class ComparserC : Comparser<Complex> { override protected Value GenericConstants() => new([new(Complex.i, "i")]); }
public class ComparserQ : Comparser<Quaternion> { override protected Value GenericConstants() => new([new(Quaternion.i, "i"), new(Quaternion.j, "j"), new(Quaternion.k, "k")]); }

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
		/* rest of the initializer
		new Ce("count", typeof(FuncCount), 0), // counts the number of elements in the vector
		new Ce("concat", typeof(FuncCat), 0), // Un-nests the vectors: concat((1,2),3,((4,5),6)) = (1,2,3,4,5,6)
		new Ce("cat", typeof(FuncCat), 0), // Un-nests the vectors: concat((1,2),3,((4,5),6)) = (1,2,3,4,5,6)
		new Cf("true", (x) => T.MakeR(T.Re(INumber<T>.Sqrabs(x)) >= 1 ? 1 : 0)), // = size >= 1
		new Cf("false",  (x) => T.MakeR(T.Re(INumber<T>.Sqrabs(x)) < 1 ? 1 : 0)), // = size < 1

		// double arguments:
		new Cf2("minimum", T.Min),	// component-wise minimum: min(1+4i,3+2i)=1+2i
		new Cf2("maximum", T.Max),	// component-wise maximum
		new Cf2("min", T.Min),		// component-wise minimum: min(1+4i,3+2i)=1+2i
		new Cf2("max", T.Max),		// component-wise maximum
		new Cf3("clamp", T.Clamp),	// component-wise clamp: clamp(5+4i,3+6i,4+7i)=4+6i

		// quadruple arguments
		new Ce("product", typeof(Product), 0),	// iterative product: product(<index>,from,to,expression(k<index>)); Ex: product(0,1,4,k0) = 1*2*3*4 = 24
		new Ce("prod", typeof(Product), 0),		// iterative product
		new Ce("sum", typeof(Sum), 0),			// iterative sum: sum(<index>,from,to,expression(k<index>)); Ex: sum(0,1,4,k0) = 1+2+3+4 = 10
		new Ce("vector", typeof(Vector), 0),	// iterative vector builder: vector(<index>,from,to,expression(k<index>)); Ex: vector(0,1,3,2^k0) = (2,4,8)
		new Ce("vec", typeof(Vector), 0),		// iterative vector builder

		// exp/log
		new Cf("exp10", INumber<T>.Exp10),	// Decimal exponential: 10^x
		new Cf("exp2", INumber<T>.Exp2),	// Binary exponential: 2^x
		new Cf("exp", T.Exp),				// Exponential: e^x = (cos(b)+isin(b))e^a
		new Cf("log10", INumber<T>.Log10),	// Decimal logarithm: log_10(x) = ln(x)/ln(2)
		new Cf("log2", INumber<T>.Log2),	// Binary logarithm: log_2(x) = ln(x)/ln(10)
		new Cf("log", T.Log),				// Natural logarithm: ln(x)
		new Cf("ln", T.Log),				// Natural logarithm: ln(x)

		// sincs
		new Cf("nsinhc", INumber<T>.Nsinhc),// sinhc(x*pi)
		new Cf("sinchpi", INumber<T>.Nsinhc),// sinhc(x*pi)
		new Cf("sinhc", INumber<T>.Sinhc),	// sinhc(x) = sinh(x)/x
		new Cf("nsinc", INumber<T>.Nsinc),	// sinc(x*pi)
		new Cf("sincpi", INumber<T>.Nsinc),	// sinc(x*pi)
		new Cf("sinc", INumber<T>.Sinc),	// sinc(x) = sin(x)/x
		new Cf("sinc", INumber<T>.Cosc),	// cosc(x) = (1-cos(x))/x

		// arc hyperbolics
		new Cf("acosh", T.Acosh),			// acosh(x)
		new Cf("asinh", T.Asinh),			// asinh(x)
		new Cf("atanh", T.Atanh),			// atanh(x)
		new Cf("asech", INumber<T>.Asech),	// asech(x)
		new Cf("acsch", INumber<T>.Acsch),	// acsch(x)
		new Cf("acoth", T.Acoth),			// acoth(x)

		// hyperbolics
		new Cf("cosh", T.Cosh),			// cosh(x)
		new Cf("sinh", T.Sinh),			// sinh(x)
		new Cf("tanh", T.Tanh),			// tanh(x)
		new Cf("sech", INumber<T>.Sech),// sech(x)
		new Cf("csch", INumber<T>.Csch),// csch(x)
		new Cf("coth", T.Coth),			// coth(x)

		// arc trigs
		new Cf("acos", T.Acos),			// acos(x)
		new Cf("asin", T.Asin),			// asin(x)
		new Cf("atan", T.Atan),			// atan(x)
		new Cf("asec", INumber<T>.Asec),// asec(x)
		new Cf("acsc", INumber<T>.Acsc),// acsc(x)
		new Cf("acot", T.Acot),			// acot(x)

		// trigs
		new Cf("cos", T.Cos),			// cos(x)
		new Cf("sin", T.Sin),			// sin(x)
		new Cf("tan", T.Tan),			// tan(x)
		new Cf("sec", INumber<T>.Sec),	// sec(x)
		new Cf("csc", INumber<T>.Csc),	// csc(x)
		new Cf("cot", T.Cot),			// cot(x)

		// basics/components
		new Cf("real", INumber<T>.TRe),			// Real part: Re(x) = a
		new Cf("re", INumber<T>.TRe),			// Real part: Re(x) = a
		new Cf("imag", INumber<T>.TI),			// Imaginary part: Im(x) = b (or sqrt(bb+cc+dd) for quats)
		new Cf("im", INumber<T>.TI),			// Imaginary part
		new Cf("frac", T.Frac),					// Signed fractional part: Frac(x) = x - Trunc(x)
		new Cf("trunc", T.Trunc),				// Whole part: Truncate(x)
		new Cf("floor", T.Floor),				// Round down: Floor(x)
		new Cf("round", T.Round),				// Round near: Round(x)
		new Cf("ceil", T.Ceil),					// Round up: Ceiling(x)
		new Cf("sign", INumber<T>.Sign),		// Sign(x) = x / |x|
		new Cf("sgn", INumber<T>.Sign),			// Sign(x) = x / |x|
		new Cf("negative", INumber<T>.Neg),		// Negation: -x
		new Cf("neg", INumber<T>.Neg),			// Negation: -x
		new Cf("inverse", T.Inv),				// Inverse: 1/x
		new Cf("inv", T.Inv),					// Inverse: 1/x
		new Cf("absri", T.AbsComp),				// Positive components: |a|+|b|i
		new Cf("sqrabs", INumber<T>.Sqrabs),	// Squared absolute value: |x|^2 = a*a + b*b
		new Cf("absolute", INumber<T>.TAbs),	// Absolute value: |x| = sqrt(a*a + b*b)
		new Cf("abs", INumber<T>.TAbs),			// Absolute value: |x| = sqrt(a*a + b*b)
		new Cf("arg", INumber<T>.TArg),			// Argument: Arg(x)
		new Cf("conjugate", INumber<T>.Conj),	// Conjugate: Conj(x) = a - bi
		new Cf("conj", INumber<T>.Conj),		// Conjugate: Conj(x) = a - bi

		// powers
		new Cf("sqrt", T.Sqrt),			// Square root: Sqrt(x)
		new Cf("cbrt", INumber<T>.Cbrt),// Cube root: Cbrt(x)
		new Cf("sqr", T.Sqr),			// Square: x*x
		new Cf("cube", T.Cub),			// Cube: x*x*x
		new Cf("cub", T.Cub),			// Cube: x*x*x
		new Cf("quart", T.Quart),		// Quart: x*x*x*x

		// specials
		Fact,								// x!
		new Cf("gauss", T.Gauss),				// e^(-x^2)
		new Cf("softabs", INumber<T>.SoftAbs),	// ln(1+e^x)
		new Cf("softneg", INumber<T>.SoftNeg),	// ln(1+e^x)
		new Cf2("softmax", INumber<T>.SoftMax),	// ln(e^a+e^b+...)
		new Cf2("softmin", INumber<T>.SoftMin),	// ln(a^a+e^b+...)
		new Cf("gamma", T.Gamma),				// gamma(x)
		new Cf("zeta", T.Zeta)					// zeta(x)*/