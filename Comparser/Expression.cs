namespace Comparser.Comparser;
public abstract partial class Comparser<T> {
	public class Expression {
		
		#region Content
		// Contains user-defined custom function
		protected readonly Comparser<T> Context;
		// Parsed and evaluated data
		protected readonly Value V;
		// Cache for remembering recently evaluated arguments
		private readonly CallFunction.EvalCache _cache;
		public readonly List<(int start, ParseDictionary.Type color)> Colors = [];
		#endregion
		
		#region Evaluations
		/// <summary>
		/// Evaluates the expression
		/// </summary>
		/// <param name="depth">depth of stack</param>
		/// <param name="args">arguments</param>
		/// <returns>Evaluated value of this expression</returns>
		public virtual Value Eval(ushort depth, Value args/*, string text = ""*/) {
			if (_cache.GetEval(args)) return _cache.Result?.Eval!;
			Value result = new(new Value[V.Values.Length]);
			int error = 0;
			if (V.Values.Length == 0)
				result = EvalValue(depth, V, args);
			else
				for (var e = 0; e < V.Values.Length; error |= result.Values[e].Error, ++e)
					result.Values[e] = EvalValue(depth, V.Values[e], args);
			//if (text != "") result.Text = text;
			var t = V.Text;
			if (error == 0) {
				result = CollapseScalar(result);
				result.Text = t;
			}else result = new(error, t);
			_cache.Insert(args, result);
			return result;
		}
		public Value EvalCopy(ushort depth, Value args/*, string text = ""*/) => new Expression(Context, V, _cache).Eval(depth, args/*, text*/);
		//private Value EvalSingle(ushort depth, int arrayIndex, Value args) => arrayIndex < V.Values.Length ? EvalValue(depth, V.Values[arrayIndex], args) : None;
		protected Value EvalValue(ushort depth, Value v, Value args/*, bool allowArg = false*/) {
			var a = args.Values;
			//if (allowArg) return v.Operand?.Eval(depth, args) ?? v;
			if (v.Values.Length == 0) {
				var eval =  Value.Operate2(
					v.Term?.Eval(depth, args) ?? new([v.Arg.Length == 0 ? v : GetArg(v.Arg, a)]),
					v.Operand?.Eval(depth, args) ?? None, v.Op.Op, v.Op.SOp);
				eval.Operand = v.Operand; // copy possible default argument
				if(eval.Text == "") eval.Text = v.Text;
				return eval;
			}
			Value result = new(new Value[v.Values.Length]);
			for (var e = 0; e < v.Values.Length; ++e) result.Values[e] = EvalValue(depth, v.Values[e], args);
			result.Text = v.Text;
			return result;
			Value GetArg(int[] arg, Value[] argVals) {
				for (var i = 0; arg[i] < argVals.Length; ++i) {
					var valI = argVals[arg[i]];
					if (i + 1 == arg.Length)
						return EvalArg(depth, valI, args);
					argVals = valI.Values;
				}
				return v;
			}
		}
		private Value EvalArg(ushort depth, Value arg, Value args) => depth > Context._stackOverflow ? StackOverflow 
			: arg.Leaf.IsNaN() && arg.Operand != null ? arg.Operand?.Eval((ushort)(1 + depth), args) ?? arg : arg;
		
		#endregion
		
		#region Expression GpuValue Translation
		public virtual GpuValue GpuParse(ushort depth) {
			GpuValue result = new(new GpuValue[V.Values.Length], OpCode.Nop);
			if (V.Values.Length == 0)
				result = GpuParseValue(depth, V);
			else for (var e = 0; e < V.Values.Length;++e) 
				result.Values[e] = GpuParseValue(depth, V.Values[e]);
			result.Operand = V.Operand;
			return GpuValue.CollapseScalar(result);
		}
		//public GpuValue GpuEvalCopy(ushort depth, Value args) 
		//	=> new Expression(Context, V, _cache).GpuParse(depth);
		//private GpuValue GpuParseSingle(ushort depth, int arrayIndex) 
		//	=> arrayIndex < V.Values.Length ? GpuParseValue(depth, V.Values[arrayIndex]) : new();
		public static GpuValue GpuParseValue(ushort depth, Value v) 
			=> v.Op.Gop(v.Term?.GpuParse(depth) ?? (v.Arg.Length == 0 ? new(v.Leaf) : new(v.Arg)), v.Operand?.GpuParse(depth) ?? new());
		#endregion
		
		#region Parse Constructors
		/// <summary>
		/// Reads and parses an expression string
		/// </summary>
		/// <param name="context">context that contains custom callable functions/constants</param>
		/// <param name="text">string to parse</param>
		/// <param name="from">starting index in the text to parse, will return how much of the text was parsed</param>
		/// <param name="nextOp">returns operand's operator if that operand should be left-associated with my term, will encapsulate previous operator into my term, and use nextOp on next operand</param>
		/// <param name="args">argument value, will substitute every x in the string</param>
		/// <param name="cache">cache size of this new Expression</param>
		/// <param name="left">what order of operations was my parent's operator? Used to test for associativity</param>
		public Expression(Comparser<T> context, string text, ref int from, out Operator nextOp, Value args, int cache = 0, byte left = 0, bool isArgument = false) {
			
			// Get and set context
			if ((Context = context)._caseInsensitive)
				text = text.ToLower();
			
			// Init variables
			int startR, start = from, error = 0;
			_cache = new(cache);
			Value /*t = new(),*/ r;
			List<Value> expr = [];
			
			// Parse Arguments:
			ParseDictionary pArgs = new(); //args = UnCollapseScaler(args);
			Nest();
			
			do { // Read vector loop:
				startR = from;
				Read(ref from, out nextOp);
				if (r.String == "") r.String = text[startR..from]; // if it didn't remember pre-defaultArg string, it will take it here
				error |= r.Error;
				Trim(ref from);
			} while (left == 0 && Char(ref from, ',')); // only left == 0 (aka top layer expression) should accept ',' for a next value
			
			// Save vector to my values:
			V = new([..expr], error, text[start..from]);
			return;

			void Read(ref int from, out Operator nextOp) {
				// Init read
				r = new();
				//Nest a = new(255);
				expr.Add(r);
				nextOp = r.Op = new();
				Trim(ref from);
				r.Op.Negative = Char(ref from, '-');
				r.Leaf = T.NaN();
				Trim(ref from);
				// Try parenthesis/function/number/constant/argument:
				int[] argNest = [];
				if ((!Char(ref from, '(') || SubTerm(out r.Term, ref from, ')'))
					&& TryFuncFailed(ref from) // _term = default function
					&& (Number(ref from, out var n) // _value = number
						|| Argument(ref from, ref argNest) // function arguments
						|| Constant(ref from, out n) // _value = constant
					))
					r.Term = new(context, argNest.Length == 0 ? n : new(text[startR..from], argNest), cache); //r.Value = n; // _value = argument (x/y/z/t...)
				else if (Fail(r) && F(ref from)) { // failed to read a term/value
					if(isArgument)
						AddColors(startR, from, ParseDictionary.Type.Arg); // color as argument
					if (text.Length <= from || text[from] != ':' || !isArgument)
						return;
					r.String = text[startR..(from++)]; // remember string before ':'
					r.Operand = new(context, text, ref from, out _, new([..expr]), cache); // try to read argument default new([..expr]) is to let it reference already read arguments
					return; // false; //  unexpected end fail
				}
				Trim(ref from);
				if (End(ref from)) // unexpected ')', or no op, and return back successful
					return; // true; 
				// Read operators/comments:
				Operator o;
				while ((o = text[from] switch {
					'+' => new Add(), '-' => new Sub(), '*' => new Mul(), '/' => new Div(), '\\' => new LDiv(), '^' => new Pow(), '$' => new Root(), '%' => new Mod(),
					'=' => new Equal(), '<' => new Less(CharAt(text,'=',from + 1)), '>' => new More(CharAt(text,'=',from + 1)),
					'[' => new Index(), '!' => new Exclamation(), _ => new Mul(false)
				}).GetType() switch {
					var x when x == typeof(Exclamation) => NotUnequal(from) && ++from > 0 && Encapsulate(new FuncOperator(context, Fact, T.Factorial, OpCode.Factorial, expr[^1]), from), // factorial
					var x when x == typeof(Index) => ExtractTerms(ref from), // index
					var x when x == typeof(Div) => CharAt(text, '*', from + 1) && Comment(ref from), // comment
					_ => false
				}) {
					Trim(ref from);
					if (o.Order == 0 || from >= text.Length) return; // true;
				}
				from += o.EatOp; // eat operator
				o.Negative = r.Op.Negative; // move negative flag to the new operator
				if (LeftAssociate(o)) {
					nextOp = o; // perform left-associativity by returning back, and the parent will encapsulate
					return; // false;
				}
				// Read operand:
				while (true) {
					if (Fail((r.Operand = new(context, text, ref from, out o, args, cache, (r.Op = o).Order)).V)) {
						if (r.Op.EatOp >= 0 && F(ref from))
							return; // false; // failed to read operand
						r.Op = new();
						break; // if it was operator-less multiplication - assume it was an expression end instead
					}
					CollectColors(r.Operand);
					if (o.Order == 0) break;
					// operand's next op has lower or equal order priority:
					// encapsulate my term into another term (wrap my term into parentheses), take the next operator and find the next operand to use it on
					_ = Encapsulate(new(context, expr[^1], cache), from);
					if (!LeftAssociate(o))
						continue; // need to test associativity again, to let it recurse backwards. otherwise 2^2^2+1 would be 2^(2^2+1)
					nextOp = o; // perform left-associativity by returning back, and the parent will encapsulate
					return; // false;
				}
				return; // true;

				//bool IsChar(int from, char c = '=') => from < text.Length && text[from] == c;
				bool NotUnequal(int from) {
					if (!CharAt(text, '=', from + 1))
						return true; // must be a factorial, keep it
					o = new Exclamation(1); // must be !=, change into that
					return false;
				}
				bool Comment(ref int from) {
					--from; // eat initial /
					for (var go = true; go;) {
						var i = text.IndexOf('/', from);
						if (i < 0) {
							from = text.Length;
							o = new();
							return true;
						}
						go = text[i - 1] != '*';
						from += i - from + 1; //Eat((byte)(i + 1), ref text);
					}
					return true;
				}
				bool ExtractTerms(ref int from) {
					++from;
					if (SubTerm(out var indices, ref from, ']'))
						o = new(); // failed to parse indices
					return Encapsulate(new FuncIndex(context, expr[^1], indices.V), from);
				}
				bool SubTerm(out Expression readTo, ref int from, char req) {
					var fail = Fail((readTo = new(context, text, ref from, out _, args)).V);
					CollectColors(readTo);
					//r.String = r.Text += readTo.V.Text;
					return (FailRequiredSymbol(req, ref from) || fail || readTo.V.Values.Length == 0) && F(ref from);
				}
				void CollectColors(Expression e) {
					foreach (var c in e.Colors)
						Colors.Add(c); // collect sub expression parsing colors
				}
				bool End(ref int from) => Trim(ref from) || text[from] switch {
					')' => true, // ends parentheses
					',' => true, // divides vector element expressions
					'{' => true, // after if or while
					'}' => true, // after block
					';' => true, // separator
					'\n' => true, // separator
					'?' => true, // ternary
					':' => true, // ternary, default arguments, definitions
					']' => true, // ends indexer 
					_ => false
				};
				bool Encapsulate(Expression p, int from) {
					expr[^1] = r = new(T.NaN(), new(), null, p, null, false, text[startR..from]);
					return true;
				}
				bool LeftAssociate(Operator testOp) => testOp.Right ? testOp.Order < left : testOp.Order <= left;
				bool Fail(Value test) => test.Term == null && (test.Values.Length == 0 || test.Values is [{ Term: null }]) /*&& test.Values[0].Values.Length == 0*/; // && test.Value.IsNaN; // no longer needed as even values are now nested in terms, and I don't test their insides.
				bool F(ref int from) {
					r.Op = new();
					r.Leaf = T.NaN();
					r.Values = [];
					r.Term = r.Operand = null;
					char[] ends = [')', ',', '{', '}', ';', '\n', '?', ':', ']'];
					int e, end = text.Length;
					var prevF = from;
				
					foreach (var et in ends)
						if ((e = text.IndexOf(et, from)) >= 0 && e < end)
							end = e;
					if(from > prevF)
						Colors.Add((prevF, ParseDictionary.Type.Error));
					//r.String = r.Text += text[..end].TrimStart(' ').TrimEnd(' ');
					//text = end < text.Length ? text[end..] : "";
					from = end;
					return true;
				} // reading failed
				bool FailRequiredSymbol(char c, ref int from, byte offset = 0) => !Char(ref from, c, offset) && F(ref from);
				bool TryFuncFailed(ref int from) {
					int startFrom = from;
					foreach (var f in Context.Context.Get(text, Functions)) {
						if (f.name.Length <= 0 || FailRequiredSymbol('(', ref from, (byte)f.name.Length)) continue;
						var endFrom = startFrom + f.name.Length;
						var fail = Fail((r.Term = ((CallFunction)f.obj.Obj).Call(context, text, ref from, args)).V); // try to read the arguments
						//r.String = r.Text += r.Term.V.Text;
						fail = (FailRequiredSymbol(')', ref from) || fail) && F(ref from);
						if (fail) {
							Colors.Add((from, ParseDictionary.Type.Error));
							return fail; // must eat func closing parenthesis
						}
						AddColors(startFrom, endFrom, f.obj.Type);
						return fail; // must eat func closing parenthesis
					}
					return true;
				}
				bool Number(ref int from, out Value number) {
					var startFrom = from;
					if (Char(ref from, '_')) {
						AddColors(startFrom, from, ParseDictionary.Type.Number);
						number = new(T.NaN()); // '_' is NaN
						return true;
					}
					if (RealNumber(ref from, out var real)) {
						number = new(T.MakeR(real), 0, text[startR..from]);
						return true;
					}
					number = None;
					return false;
				}
				bool RealNumber(ref int from, out double number, double l = 0) {
					if (from < text.Length) {
						if (text[from] == '.') {
							// eat decimal point
							++from; //_ = Eat(1, ref text);
							// get fractional part
							number = l + DecimalNumber(ref from);
							return true;
						}
						if (int.TryParse(text[from].ToString(), out var i)) {
							l *= 10;
							// eat another digit
							++from; //_ = Eat(1, ref text);
							// add another whole digit, or finish
							_ = RealNumber(ref from, out number, l + i); // && 1 <= n ? 10 * i + n : i + n;
							return true;
						}
					}
					number = l; // no more digits
					return false;
				}
				double DecimalNumber(ref int from, double d = 1) {
					if (from >= text.Length) return 0; // no more digits
					d /= 10; // prepare another position
					if (!int.TryParse(text[from].ToString(), out var i))
						return 0;
					++from; //Eat(1, ref text);
					return i * d + DecimalNumber(ref from, d);
				}
				bool Constant(ref int from, out Value number) {
					// WARNING, if there is any function with the same name, then you can't operator-less multiply with parentheses from the right!
					// for example gamma is either eulerConstant or the gamma function:
					// gamma2 = eulerConstant*2, gamma(2+1) = evaluates gamma function at 2, (2+1)gamma = (2+1)*eulerConstant
					foreach (var c in Context.Context.Get(text, Constants)) {
						if (c.name.Length <= 0) continue;
						number = ((Value)c.obj.Obj).Copy();
						AddColors(from, from += c.name.Length, c.obj.Type);
						return true;
					}
					number = None;
					return false;
				}
				bool Argument(ref int from, ref int[] resultNest) {
					foreach (var a in pArgs.Get(text, (byte)ParseDictionary.Type.Arg)) {
						if (a.name.Length <= 0) continue;
						resultNest = (int[])a.obj.Obj;
						AddColors(from, from += a.name.Length, a.obj.Type);
						return true;
					}
					return false;
				}
				void AddColors(int startFrom, int endFrom, ParseDictionary.Type type) {
					Colors.Add((startFrom, type));
					Colors.Add((endFrom, ParseDictionary.Type.Text));
				}
			}
			bool Char(ref int from, char c, byte offset = 0) {
				var o = from + offset;
				var test = text.Length > o && text[o] == c;
				if (!test)
					return test;
				//r.String = r.Text += text[..++offset];
				from += offset; //R(offset, ref from);
				return test;
			}
			void Nest() {
				List<int> nest = [];
				N(args, 0);
				return;
				void N(Value aa, int depth) {
					if (aa.Values.Length == 0) {
						pArgs.Insert(new(nest.ToArray(), ParseDictionary.Type.Arg), aa.String);
						return;
					}
					for (var a = 0; a < aa.Values.Length; ++a) {
						nest.Add(a);
						N(aa.Values[a], 1 + depth);
						nest.RemoveAt(depth);
					}
				}
			}
			// Trims whitespace
			bool Trim(ref int from) => TrimStart(text, ref from);
		}
		// encapsulate a value
		protected Expression(Comparser<T> context, Value t, int cache = 0) {
			_cache = new(cache);
			Context = context;
			V = new([new(t.Leaf, t.Op, t.Arg, t.Term, t.Operand, t.Op.Negative)], t.Error, t.Text);
		}
		// copy
		private Expression(Comparser<T> context, Value t, CallFunction.EvalCache cache) {
			_cache = cache;
			Context = context;
			V = t.Copy();
		}// Simple single expression parse (without returning the rest of the code)
		public Expression(Comparser<T> context, string text, Value args, int from = 0, int cache = 0) {
			if (context._caseInsensitive) text = text.ToLower();
			var e = new Expression(Context = context, text, ref from, out _, args, cache);
			V = e.V;
			V.String = V.Text += text;
			//V = text == "" ? e.V : None;
			_cache = new(cache);
		}
		#endregion
		
		private const byte Functions = (byte)ParseDictionary.Type.UserF | (byte)ParseDictionary.Type.DefaultF;
		private const byte Constants = (byte)ParseDictionary.Type.UserC | (byte)ParseDictionary.Type.DefaultC;
	}
}

/*bool Constant(ref int from, Value consts, out Value number, Nest arg, bool isArg = false) {
// WARNING, if there is any function with the same name, then you can't operator-less multiply with parentheses from the right!
// for example gamma is either eulerConstant or the gamma function:
// gamma2 = eulerConstant*2, gamma(2+1) = evaluates gamma function at 2, (2+1)gamma = (2+1)*eulerConstant
//var t = CollapseScalar(a);

// TODO rewrite

for (var f = 0; f < consts.Values.Length; ++f) {
var v = consts.Values[f];
if (v.Values.Length > 0) {
	if (Constant(ref text, v, out number, arg.Next = new((byte)f), isArg))
		return true;
	arg.Next = null;
	continue;
}
var k = v.String;
if (k == "" || text.Length < k.Length ||
	text[0..k.Length] != k) continue;
from += k.Length;//_ = Eat((byte)k.Length, ref text);
number = v.Copy();
number.String = number.Text = r.Text;
if (isArg)
	arg.Next = new((byte)f);
return true;
}
number = None;
return false;
}*/
/*bool Eat(byte c, ref string from) {
	//r.String = r.Text += text[..c];
	from += c;//R(c, ref text);
	return true;
}*/
//void R(byte c, ref int from) => from += c;//text = text[c..];

/*static Expression() {
			CallFunction min, max, mul, prod, vec, ln, nsinhc, nsinc, re, im, sign, neg, inv, abs, conj, compMod, cub, cabs, trunc, sinhc, ceil;
			DefaultFunctions = new() {
				// meta
				["eval"] = new Ce(typeof(FuncEval), 0), // attempts to parse and evaluate every Text in the input
				["count"] = new Ce(typeof(FuncCount), 0), // counts the number of elements in the vector
				["concat"] = new Ce(typeof(FuncCat), 0), // Un-nests the vectors: concat((1,2),3,((4,5),6)) = (1,2,3,4,5,6)
				["cat"] = new Ce(typeof(FuncCat), 0), // Un-nests the vectors: concat((1,2),3,((4,5),6)) = (1,2,3,4,5,6)

				// double arguments:
				["minimum"] = min = new Cf2(T.Min, OpCode.Min), // component-wise minimum
				["maximum"] = max = new Cf2(T.Max, OpCode.Max), // component-wise maximum
				["min"] = min, // component-wise minimum
				["max"] = max, // component-wise maximum
				["softmax"] = new Cf2(INumber<T>.SoftMax, OpCode.SoftMax),
				["softmin"] = new Cf2(INumber<T>.SoftMin, OpCode.SoftMin),
				["add"] = new Cf2(INumber<T>.Add, OpCode.Add), // adds all the top layer elements of the input vector
				["mul"] = mul = new Cf2(INumber<T>.Mul, OpCode.Mul), // multiplies all the top layer elements of the input vector
				["multiply"] = mul,
				["icoef"] = new Cf2((x,y) => -x * y, OpCode.ImCoef), // = re(-a*b), imaginary coefficient icoef(a+bi,i)=b, icoef(r+ai+bj+ck,j)=b, icoef(a+bi,1)=-a
				["compmod"] = compMod = new Cf2(INumber<T>.CompMod, OpCode.CompMod), 
				["cmod"] = compMod, // component-wise remainder, returns 0 when dividing by zero
				
				// triple arguments
				["clamp"] = new Cf3(T.Clamp, OpCode.Clamp), // component-wise clamp

				// quadruple arguments
				["product"] = prod = new Ce(typeof(Product), 0), // iterative product
				["prod"] = prod, // iterative product
				["sum"] = new Ce(typeof(Sum), 0), // iterative sum
				["vector"] = vec = new Ce(typeof(Vector), 0), // iterative vector builder
				["vec"] = vec, // iterative vector builder

				// exp/log
				["exp10"] = new Cf(INumber<T>.Exp10, OpCode.Exp10), // 10^x
				["exp2"] = new Cf(INumber<T>.Exp2, OpCode.Exp2), // 2^x
				["exp"] = new Cf(T.Exp, OpCode.Exp), // e^x
				["log10"] = new Cf(INumber<T>.Log10, OpCode.Log10), // log_10(x)
				["log2"] = new Cf(INumber<T>.Log2, OpCode.Log2), // log_2(x)
				["log"] = ln = new Cf(T.Log, OpCode.Log), // ln(x)
				["ln"] = ln, // ln(x)

				// sincs
				["nsinhc"] = nsinhc = new Cf(INumber<T>.Nsinhc, OpCode.Nsinhc),
				["sinchpi"] = nsinhc,
				["sinhc"] = sinhc = new Cf(INumber<T>.Sinhc, OpCode.Sinhc),
				["sinch"] = sinhc,
				["nsinc"] = nsinc = new Cf(INumber<T>.Nsinc, OpCode.Nsinc),
				["sincpi"] = nsinc,
				["sinc"] = new Cf(INumber<T>.Sinc, OpCode.Sinc),
				
				// coscs
				["coshc"] = new Cf(INumber<T>.Coshc, OpCode.Coshc),
				["coshcpi"] = new Cf(INumber<T>.Ncoshc, OpCode.Ncoshc),
				["ncoshc"] = new Cf(INumber<T>.Ncoshc, OpCode.Ncoshc),
				["cosc"] = new Cf(INumber<T>.Cosc, OpCode.Cosc),
				["coscpi"] = new Cf(INumber<T>.Ncosc, OpCode.Ncosc),
				["ncosc"] = new Cf(INumber<T>.Ncosc, OpCode.Ncosc),
				
				// arc hyperbolics
				["acosh"] = new Cf(T.Acosh, OpCode.Acosh),
				["asinh"] = new Cf(T.Asinh, OpCode.Asinh),
				["atanh"] = new Cf(T.Atanh, OpCode.Atanh),
				["asech"] = new Cf(INumber<T>.Asech, OpCode.Asech),
				["acsch"] = new Cf(INumber<T>.Acsch, OpCode.Acsch),
				["acoth"] = new Cf(T.Acoth, OpCode.Acoth),

				// hyperbolics
				["cosh"] = new Cf(T.Cosh, OpCode.Cosh),
				["sinh"] = new Cf(T.Sinh, OpCode.Sinh),
				["tanh"] = new Cf(T.Tanh, OpCode.Tanh),
				["sech"] = new Cf(INumber<T>.Sech, OpCode.Sech),
				["csch"] = new Cf(INumber<T>.Csch, OpCode.Csch),
				["coth"] = new Cf(T.Coth, OpCode.Coth),

				// arc trigs
				["acos"] = new Cf(T.Acos, OpCode.Acos),
				["asin"] = new Cf(T.Asin, OpCode.Asin),
				["atan"] = new Cf(T.Atan, OpCode.Atan),
				["asec"] = new Cf(INumber<T>.Asec, OpCode.Asec),
				["acsc"] = new Cf(INumber<T>.Acsc, OpCode.Acsc),
				["acot"] = new Cf(T.Acot, OpCode.Acot),

				// trigs
				["cos"] = new Cf(T.Cos, OpCode.Cos),
				["sin"] = new Cf(T.Sin, OpCode.Sin),
				["tan"] = new Cf(T.Tan, OpCode.Tan),
				["sec"] = new Cf(INumber<T>.Sec, OpCode.Sec),
				["csc"] = new Cf(INumber<T>.Csc, OpCode.Csc),
				["cot"] = new Cf(T.Cot, OpCode.Cot),

				// unary
				["true"] = new Cf((x) => T.MakeR(T.Re(INumber<T>.SqrAbs(x)) >= 1 ? 1 : 0), OpCode.True), // = size >= 1
				["false"] = new Cf((x) => T.MakeR(T.Re(INumber<T>.SqrAbs(x)) < 1 ? 1 : 0), OpCode.False), // = size < 1
				["real"] = re = new Cf(INumber<T>.T_Re, OpCode.Re),			// real part: re(a+bi) = a
				["re"] = re,												// real part
				["imag"] = im = new Cf(INumber<T>.T_I, OpCode.Im),			// imaginary sum: im(r+ai+bj+ck) = a+b+c
				["im"] = im,												// imaginary sum
                ["immg"] = new Cf((x) => T.MakeR(T.ImMag(x)), OpCode.ImMag),	// imaginary magnitude immg(r+ai+bj+ck) = sqrt(a^2+b^2+c^2)
				["frac"] = new Cf(T.Frac, OpCode.Frac),						// = fractional part
				["trunc"] = trunc = new Cf(T.Trunc, OpCode.Trunc),			// = whole part
				["truncate"] = trunc,										// = whole part
				["floor"] = new Cf(T.Floor, OpCode.Floor),					// = round down
				["round"] = new Cf(T.Round, OpCode.Round),					// = round
				["ceiling"] = ceil = new Cf(T.Ceil, OpCode.Ceil),			// = round up
				["ceil"] = ceil,											// = round up
				["sign"] = sign = new Cf(INumber<T>.Sign, OpCode.Sgn),		// = z/|z|
				["sgn"] = sign,												// = z/|z|
				["negative"] = neg = new Cf(INumber<T>.Neg, OpCode.Neg),	// = -z
				["neg"] = neg,												// = -z
				["inverse"] = inv = new Cf(T.Inv, OpCode.Inv),				// = 1/z
				["inv"] = inv,												// = 1/z
				["absri"] = cabs = new Cf(T.AbsComp, OpCode.Absri),			// component-abs: absri(a+bi) = |a|+|b|i
				["compabs"] = cabs,											// component-abs
				["cabs"] = cabs,											// component-abs
				["sqrabs"] = new Cf(INumber<T>.SqrAbs, OpCode.SqrAbs),		// = |z|^2; sqrabs(a+bi) = a^2+b^2
				["absolute"] = abs = new Cf(INumber<T>.T_Abs, OpCode.Abs),	// = |z|
				["abs"] = abs,												// = |z|
				["norm"] = abs,												// = |z|
				["arg"] = new Cf(INumber<T>.T_Arg, OpCode.Arg),				// argument, the angle from (0,0). arg(-1)=pi
				["conjugate"] = conj = new Cf(INumber<T>.Conj, OpCode.Conj),
				["conj"] = conj, // conjugate: negates all imaginary units, conj(r+ai+bj+dk) = r-ai-bj-bk
				// powers
				["sqrt"] = new Cf(T.Sqrt, OpCode.Sqrt),			// square root = z^(1/2)
				["sqr"] = new Cf(T.Sqr, OpCode.Sqr),			// square = z^2
				["cbrt"] = new Cf(INumber<T>.Cbrt, OpCode.Cbrt),// cube root = z^(1/3)
				["cube"] = cub = new Cf(T.Cub, OpCode.Cub),		// cube = z^3
				["cub"] = cub,									// cube
				["quart"] = new Cf(T.Quart, OpCode.Quart),		// z^4

				// specials
				["fact"] = Fact,							// factorial
				["factorial"] = Fact,						// factorial
				["gauss"] = new Cf(T.Gauss, OpCode.Gauss),	// gauss e^(-z^2)
				["gamma"] = new Cf(T.Gamma, OpCode.Gamma),	// gamma function = (xz1)!

				//["fac"] = new Cf(INumber<T>.Gamma_1), // testing
				//["gamneg"] = new Cf(INumber<T>.Gamma_05), // testing
				//["gamzero"] = new Cf(INumber<T>.Gamma0), // testing
				//["gamhalf"] = new Cf(INumber<T>.Gamma05), // testing
				//["gamone"] = new Cf(INumber<T>.Gamma1), // testing

				["zeta"] = new Cf(T.Zeta, OpCode.Zeta),						// riemann zeta function
				["softabs"] = new Cf(INumber<T>.SoftAbs, OpCode.SoftAbs),	// = e^(1+ln(z))
				["softneg"] = new Cf(INumber<T>.SoftNeg, OpCode.SoftNeg)	// = e^(1+ln(z))
			};
		}*/
/*bool Func(Dictionary<string, CallFunction> funcs, ref string text) {
foreach (var f in funcs) {
if (f.Key.Length <= 0 // func must have a name
	|| text.Length <= f.Key.Length // text must have enough characters for the func name
	|| text[..f.Key.Length] != f.Key // must match the func name
	|| FailRequiredSymbol('(', ref text, (byte)f.Key.Length)) // no argument parentheses found, maybe it's a constant with the same name...?
	continue;
var fail = Fail((r.Term = f.Value.Call(context, ref text, args)).V); // try to read the arguments
r.String = r.Text += r.Term.V.Text;
return (FailRequiredSymbol(')', ref text) || fail) && F(ref text); // must eat func closing parenthesis
}
return true;
}*/