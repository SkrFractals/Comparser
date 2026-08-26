using Comparser.Comparser.Numbers;
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
		#endregion
		
		#region Evaluations
		/// <summary>
		/// Evaluates the expression
		/// </summary>
		/// <param name="depth">depth of stack</param>
		/// <param name="args">arguments</param>
		/// <param name="text">string argument to insert into the field</param>
		/// <returns>Evaluated value of this expression</returns>
		public virtual Value Eval(ushort depth, Value args, string text = "") {
			if (_cache.GetEval(args)) return _cache.Result?.Eval!;
			Value result = new(new Value[V.Values.Length]);
			if (V.Values.Length == 0)
				result = EvalValue(depth, V, args);
			else
				for (var e = 0; e < V.Values.Length; ++e)
					result.Values[e] = EvalValue(depth, V.Values[e], args);
			if (text != "") V.Text = text;
			_cache.Insert(args, result);
			return CollapseScalar(result);
		}
		public Value EvalCopy(ushort depth, Value args, string text = "") => new Expression(Context, V, _cache).Eval(depth, args, text);
		//private Value EvalSingle(ushort depth, int arrayIndex, Value args) => arrayIndex < V.Values.Length ? EvalValue(depth, V.Values[arrayIndex], args) : None;
		static protected Value EvalValue(ushort depth, Value v, Value args, bool allowArg = false) {
			var a = args.Values;
			if (allowArg) return v.Operand?.Eval(depth, args) ?? v;
			if (v.Values.Length == 0)
				return Value.Operate2(
					v.Term?.Eval(depth, args) ?? new([v.Arg == null ? v : GetArg(v.Arg, a)]),
					v.Operand?.Eval(depth, args) ?? None, v.Op.Op, v.Op.SOp);
			Value result = new(new Value[v.Values.Length]);
			for (var e = 0; e < v.Values.Length; ++e) result.Values[e] = EvalValue(depth, v.Values[e], args);
			return result;
			Value GetArg(Nest arg, Value[] aa) {
				for (; arg.V < 255 && arg.V < aa.Length; arg = arg.Next) {
					if (arg.Next == null)
						return EvalArg(depth, aa[arg.V], args);
					aa = aa[arg.V].Values;
				}
				return v;
			}
		}
		private static Value EvalArg(ushort depth, Value arg, Value args) 
			=> arg.Leaf.IsNaN() && arg.Operand != null ? EvalValue((ushort)(1 + depth), arg, args, true) : arg;
		
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
			=> v.Op.Gop(v.Term?.GpuParse(depth) ?? (v.Arg == null ? new(v.Leaf) : MakeArg(v.Arg)), v.Operand?.GpuParse(depth) ?? new());
		private static GpuValue MakeArg(Nest arg) {
			List<byte> args = [];
			for (;arg.Next != null ; arg = arg.Next)
				args.Add(arg.V);
			return new(args.ToArray()); // OpCode=Argument
		}
		#endregion
		
		#region Parse Constructors
		/// <summary>
		/// Reads and parses an expression string
		/// </summary>
		/// <param name="context">context that contains custom callable functions/constants</param>
		/// <param name="text">string to parse</param>
		/// <param name="nextOp">returns operand's operator if that operand should be left-associated with my term, will encapsulate previous operator into my term, and use nextOp on next operand</param>
		/// <param name="args">argument value, will substitute every x in the string</param>
		/// <param name="cache">cache size of this new Expression</param>
		/// <param name="left">what order of operations was my parent's operator? Used to test for associativity</param>
		public Expression(Comparser<T> context, ref string text, out Operator nextOp, Value args, int cache = 0, byte left = 0) {
			// DEBUG
			//var origText = text;
			_cache = new(cache);
			Context = context;
			Value t = new(), r;
			List<Value> expr = [];
			do {
				Read(ref text, out nextOp);
				t.Text += r.Text;
				r = t;
				text = text.TrimStart(' ');
			} while (left == 0 && Char(ref text, ',')); // only left == 0 (aka top layer expression) should accept ',' for a next value
			V = new([..expr], t.Text);
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

			void Read(ref string text, out Operator nextOp) {
				// Init read
				r = new();
				Nest a = new(255);
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
						|| Constant(ref text, args, out n, a, true)	// function arguments
						//|| Constant(ref text, context._customConstants, out n, a)  // _value = local user constant
						|| Constant(ref text, context._customConstants, out n, a)  // _value = global user constant
						|| Constant(ref text, context.GenericConstants(), out n, a) // _value = constant (i/j/k/x/y/z...)
						|| Constant(ref text, DefaultConstants, out n, a) // _value = generic constants (pi/tau/e/i/gamma/one)
						))
					r.Term = new(context, a.Next == null ? n : new(a.Next, r.Text), cache); //r.Value = n; // _value = argument (x/y/z/t...)
				else if (Fail(r) && F(ref text)) { // failed to read a term/value
					if (Next(text,':') && Char(ref text, ':'))
						r.Operand = new(context, ref text, out _, args, cache); // try to read argument default
					return; // false; //  unexpected end fail
				}
				text = text.TrimStart(' ');
				if (End(ref text)) // unexpected ')', or no op, and return back successful
					return; // true; 
				// Read operators/comments:
				Operator o;
				while ((o = text[0] switch {
					'+' => new Add(), '-' => new Sub(), '*' => new Mul(), '/' => new Div(), '\\' => new LDiv(), '^' => new Pow(), '$' => new Root(), '%' => new Mod(),
					'=' => new Equal(), '<' => new Less(Next(text)), '>' => new More(Next(text)),
					'[' => new Index(), '!' => new Exclamation(), _ => new Mul(false)
				}).GetType() switch {
					var x when x == typeof(Exclamation) => NotUnequal(text) && Eat(1, ref text) && Encapsulate(new FuncOperator(context, Fact, T.Factorial, OpCode.Factorial, expr[^1])), // factorial
					var x when x == typeof(Index) => ExtractTerms(ref text), // index
					var x when x == typeof(Div) => text.Length > 0 && text[1] == '*' && Comment(ref text), // comment
					_ => false
				}) {
					text = text.TrimStart(' ');
					if (o.Order == 0 || text.Length == 0) return; // true;
				}
				//if (o.GetType() == typeof(Equal) && !OrEqual(text))
				//	return; // reject single =
				while (o.EatOp-- > 0) // not an operator-less multiplication?
					_ = Eat(1, ref text); // eat operator
				o.Negative = r.Op.Negative; // move negative flag to the new operator
				if (LeftAssociate(o)) {
					nextOp = o; // perform left-associativity by returning back, and the parent will encapsulate
					return; // false;
				}
				// Read operand:
				while (true) {
					if (Fail((r.Operand = new(context, ref text, out o, args, cache, (r.Op = o).Order)).V)) {
						r.String = r.Text += r.Operand.V.Text;
						if (r.Op.EatOp >= 0 && F(ref text))
							return; // false; // failed to read operand
						r.Op = new();
						break; // if it was operator-less multiplication - assume it was an expression end instead
					}
					r.String = r.Text += r.Operand.V.Text;
					if (o.Order == 0) break;
					// operand's next op has lower or equal order priority:
					// encapsulate my term into another term (wrap my term into parentheses), take the next operator and find the next operand to use it on
					_ = Encapsulate(new(context, expr[^1], cache));
					if (!LeftAssociate(o))
						continue; // need to test associativity again, to let it recurse backwards. otherwise 2^2^2+1 would be 2^(2^2+1)
					nextOp = o; // perform left-associativity by returning back, and the parent will encapsulate
					return; // false;
				}
				return; // true;

				bool Next(string text, char c = '=') => text.Length > 1 && text[1] == '=';
				bool NotUnequal(string text) {
					if (!Next(text))
						return true; // must be a factorial, keep it
					o = new Exclamation(1); // must be !=, change into that
					return false;
				}
				bool Comment(ref string text) {
					_ = Eat(1, ref text); // eat initial /
					for (var go = true; go;) {
						var i = text.IndexOf('/');
						if (i < 0) {
							text = "";
							o = new();
							return true;
						}
						go = text[i - 1] != '*';
						Eat((byte)(i + 1), ref text);
					}
					return true;
				}
				bool ExtractTerms(ref string text) {
					_ = Eat(1, ref text);
					if (SubTerm(out var indices, ref text, ']'))
						o = new(); // failed to parse indices
					return Encapsulate(new FuncIndex(context, expr[^1], indices.V));
				}
				bool SubTerm(out Expression readTo, ref string text, char req) {
					var fail = Fail((readTo = new(context, ref text, out _, args)).V);
					r.String = r.Text += readTo.V.Text;
					return (FailRequiredSymbol(req, ref text) || fail || readTo.V.Values.Length == 0) && F(ref text);
				}
				bool End(ref string text) => (text = text.TrimStart(' ')).Length == 0 || text[0] switch {
					')' => true, // ends parentheses
					',' => true, // divides vector element esxpressions
					'{' => true, // after if or while
					'}' => true, // after block
					';' => true, // separator
					'\n' => true, // separator
					'?' => true, // ternary
					':' => true, // ternary, default arguments, definitions
					']' => true, // ends indexer 
					_ => false
				};
				bool Encapsulate(Expression p) {
					expr[^1] = r = new(T.NaN(), new(), null, p, null, false, r.Text);
					return true;
				}
				bool LeftAssociate(Operator testOp) => testOp.Right ? testOp.Order < left : testOp.Order <= left;
				bool Fail(Value test)
					=> test.Term == null && (test.Values.Length == 0 || test.Values is [{ Term: null }]) /*&& test.Values[0].Values.Length == 0*/; // && test.Value.IsNaN; // no longer needed as even values are now nested in terms, and I don't test their insides.
				bool F(ref string text) {
					r.Op = new();
					r.Leaf = T.NaN();
					r.Values = [];
					r.Term = r.Operand = null;
					char[] ends = [')', ',', '{', '}', ';', '\n', '?', ':', ']'];
					int e, end = text.Length;
					foreach (var et in ends)
						if ((e = text.IndexOf(et)) >= 0 && e < end)
							end = e;
					r.String = r.Text += text[..end].TrimStart(' ').TrimEnd(' ');
					text = end < text.Length ? text[end..] : "";
					return true;
				} // reading failed
				bool FailRequiredSymbol(char c, ref string text, byte offset = 0) => !Char(ref text, c, offset) && F(ref text);
				bool Func(Dictionary<string, CallFunction> funcs, ref string text) {
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
				}
				bool Negative(ref string text) => Char(ref text, '-');
				bool Number(ref string text, out Value number) {
					if (Char(ref text, '_', 0)) {
						number = new(T.NaN());
						return true;
					}
					if (RealNumber(ref text, out var real)) {
						number = new(T.MakeR(real), r.Text);
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
							_ = RealNumber(ref text, out number, l + i); // && 1 <= n ? 10 * i + n : i + n;
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
				bool Constant(ref string text, Value consts, out Value number, Nest arg, bool isArg = false) {
					// WARNING, if there is any function with the same name, then you can't operator-less multiply with parentheses from the right!
					// for example gamma is either eulerConstant or the gamma function:
					// gamma2 = eulerConstant*2, gamma(2+1) = evaluates gamma function at 2, (2+1)gamma = (2+1)*eulerConstant
					//var t = CollapseScalar(a);
					for (var f = 0; f < consts.Values.Length; ++f) {
						var v = consts.Values[f];
						if (v.Values.Length > 0) {
							if (Constant(ref text, v, out number, arg.Next = new((byte)f), isArg))
								return true;
							arg.Next = null;
							continue;
						}
						var k = v.Text;
						if (k == "" || text.Length < k.Length ||
							text[0..k.Length] != k) continue;
						_ = Eat((byte)k.Length, ref text);
						number = v.Copy();
						number.String = number.Text = r.Text;
						if (isArg)
							arg.Next = new((byte)f);
						return true;
					}
					number = None;
					return false;
				}
				bool Eat(byte c, ref string text) {
					r.String = r.Text += text[..c];
					R(c, ref text);
					return true;
				}
			}
			void R(byte c, ref string text) => text = text[c..];
			bool Char(ref string text, char c, byte offset = 0) {
				var test = text.Length > offset && text[offset] == c;
				if (!test)
					return test;
				r.String = r.Text += text[..++offset];
				R(offset, ref text);
				return test;
			}
		}
		// encapsulate a value
		protected Expression(Comparser<T> context, Value t, int cache = 0) {
			_cache = new(cache);
			Context = context;
			V = new([new(t.Leaf, t.Op, t.Arg, t.Term, t.Operand, t.Op.Negative)], t.Text);
		}
		// copy
		private Expression(Comparser<T> context, Value t, CallFunction.EvalCache cache) {
			_cache = cache;
			Context = context;
			V = t.Copy();
		}// Simple single expression parse (without returning the rest of the code)
		public Expression(Comparser<T> context, string text, Value args, int cache = 0) {
			var e = new Expression(Context = context, ref text, out _, args, cache);
			V = e.V;
			V.String = V.Text += text;
			//V = text == "" ? e.V : None;
			_cache = new(cache);
		}
		#endregion

		#region Default Functions and Constants
		private static readonly Dictionary<string, CallFunction> DefaultFunctions;
		private static readonly Value DefaultConstants = new([
			new(INumber<T>.C_Pi(), "pi"),
			new(INumber<T>.C_Tau(), "tau"),
			new(INumber<T>.C_E(), "e"),
			new(INumber<T>.C_Gamma(), "gamma"),
			new(T.One(), "one")
		]);
		static Expression() {
			CallFunction min, max, mul, prod, vec, ln, nsinhc, nsinc, re, im, sign, neg, inv, abs, conj, compMod, cub;
			DefaultFunctions = new() {
				// meta
				["eval"] = new Ce(typeof(FuncEval), 0), // attempts to parse and evaluate every Text in the input
				["count"] = new Ce(typeof(FuncCount), 0), // counts the number of elements in the vector
				["concat"] = new Ce(typeof(FuncCat), 0), // Un-nests the vectors: concat((1,2),3,((4,5),6)) = (1,2,3,4,5,6)
				["cat"] = new Ce(typeof(FuncCat), 0), // Un-nests the vectors: concat((1,2),3,((4,5),6)) = (1,2,3,4,5,6)
				["true"] = new Cf((x) => T.MakeR(T.Re(INumber<T>.SqrAbs(x)) >= 1 ? 1 : 0), OpCode.True), // = size >= 1
				["false"] = new Cf((x) => T.MakeR(T.Re(INumber<T>.SqrAbs(x)) < 1 ? 1 : 0), OpCode.False), // = size < 1

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
				["sinhc"] = new Cf(INumber<T>.Sinhc, OpCode.Sinhc),
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

				// basics/components
				["real"] = re = new Cf(INumber<T>.T_Re, OpCode.Re),			// real part: re(a+bi) = a
				["re"] = re,												// real part
				["imag"] = im = new Cf(INumber<T>.T_I, OpCode.Im),			// imaginary sum: im(r+ai+bj+ck) = a+b+c
				["im"] = im,												// imaginary sum
                ["immg"] = new Cf((x) => T.MakeR(T.ImMag(x)), OpCode.ImMag),	// imaginary magnitude immg(r+ai+bj+ck) = sqrt(a^2+b^2+c^2)
                ["icoef"] = new Cf2((x,y) => -x * y, OpCode.ImCoef), // = re(-a*b), imaginary coefficient icoef(a+bi,i)=b, icoef(r+ai+bj+ck,j)=b, icoef(a+bi,1)=-a
				["frac"] = new Cf(T.Frac, OpCode.Frac),						// = fractional part
				["trunc"] = new Cf(T.Trunc, OpCode.Trunc),					// = whole part
				["floor"] = new Cf(T.Floor, OpCode.Floor),					// = round down
				["round"] = new Cf(T.Round, OpCode.Round),					// = round
				["ceil"] = new Cf(T.Ceil, OpCode.Ceil),						// = round up
				["sign"] = sign = new Cf(INumber<T>.Sign, OpCode.Sgn),		// = z/|z|
				["sgn"] = sign,												// = z/|z|
				["negative"] = neg = new Cf(INumber<T>.Neg, OpCode.Neg),	// = -z
				["neg"] = neg,												// = -z
				["inverse"] = inv = new Cf(T.Inv, OpCode.Inv),				// = 1/z
				["inv"] = inv,												// = 1/z
				["absri"] = new Cf(T.AbsComp, OpCode.Absri),				// component-abs: absri(a+bi) = |a|+|b|i
				["sqrabs"] = new Cf(INumber<T>.SqrAbs, OpCode.SqrAbs),		// = |z|^2; sqrabs(a+bi) = a^2+b^2
				["absolute"] = abs = new Cf(INumber<T>.T_Abs, OpCode.Abs),	// = |z|
				["abs"] = abs,												// = |z|
				["norm"] = abs,												// = |z|
				["arg"] = new Cf(INumber<T>.T_Arg, OpCode.Arg),				// argument, the angle from (0,0). arg(-1)=pi
				["conjugate"] = conj = new Cf(INumber<T>.Conj, OpCode.Conj),
				["conj"] = conj, // conjugate: negates all imaginary units, conj(r+ai+bj+dk) = r-ai-bj-bk
				["compmod"] = compMod = new Cf2(INumber<T>.CompMod, OpCode.CompMod), 
				["cmod"] = compMod, // component-wise remainder, returns 0 when dividing by zero
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
		}
		#endregion
	}
}