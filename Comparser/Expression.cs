using Comparser.Comparser.Numbers;
namespace Comparser.Comparser;
public abstract partial class Comparser<T> {
	public class Expression {
		
		#region Content
		// Contains user-defined custom function
		protected readonly Comparser<T> Context;
		// Parsed and evaluated data
		public readonly Value V;
		// Cache for remembering recently evaluated arguments
		private readonly CallFunction.EvalCache _cache;
		//public readonly List<(int start, ParseDictionary.Type color)> Colors = [];
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
		/// <param name="read">parser class providing the Comparser context and read stream</param>
		/// <param name="nextOp">returns operand's operator if that operand should be left-associated with my term, will encapsulate previous operator into my term, and use nextOp on next operand</param>
		/// <param name="args">argument value, will substitute every x in the string</param>
		/// <param name="cache">cache size of this new Expression</param>
		/// <param name="left">what order of operations was my parent's operator? Used to test for associativity</param>
		/// <param name="isArgument">is this an argument parse?</param>
		public Expression(Reader read, out Operator nextOp, Value args, int cache = 0, OpOrder left = 0, bool isArgument = false) {
			Context = read.Context;
			
			// Init variables
			int startR, start = read.From, error = 0;
			_cache = new(cache);
			Value /*t = new(),*/ r;
			List<Value> expr = [];
			
			// Parse Arguments:
			ParseDictionary pArgs = new(); //args = UnCollapseScaler(args);
			Nest();
			read.TrimStart();
			nextOp = new();
			do { // Read vector loop:
				startR = read.From;
				Read(out nextOp);
				if (r.String == "") r.String = read.Uncomment(startR, read.From); // if it didn't remember pre-defaultArg string, it will take it here
				error |= r.Error;
				read.TrimStart();
				//read.CharAtRel('\n', 0)
			}
			// only left == 0 (aka top layer expression) should accept ',' for a next value
			while(left == 0 && !read.GotoFirstFailed(0,[','], 1, out _, out _));
			
			// Save vector to my values:
			V = new([..expr], error, read.Uncomment(start, read.From));
			return;

			void Read(out Operator nextOp) {
				// Init read
				r = new();
				expr.Add(r);
				nextOp = r.Op = new();
				r.Op.Negative = Char('-');
				r.Leaf = T.NaN();
				read.TrimStart();
				// Try parenthesis/function/number/constant/argument:
				int[] argNest = [];

				// unary operators:
				Operator o;
				if (End(false)) // unexpected ')', or no op, and return back successful
					return; // true; 
				while ((o = read.nextChar switch {
					'/' => new Div(), '\\' => new LDiv(), _ => new()
				}).GetType() switch {
					var x when x == typeof(Div) => read.IsComment(), // comment
					_ => false
				}) {
					if (o.Order == 0)
						return;
					read.TrimStart();
					if (read.From >= read.Text.Length) return; // true;
				}
				// no unary:
				if (o.Order == 0) {
					if ((!Char('(') || read.TrimStart(1) && SubTerm(out r.Term, ')'))
						&& TryFuncFailed() // _term = default function
						&& (Number(out var n) // _value = number
							|| Argument(ref argNest) // function arguments
							|| Constant(out n) // _value = constant
						))
						r.Term = new(Context, argNest.Length == 0 ? n : new(read.Uncomment(startR, read.From), argNest), cache); //r.Value = n; // _value = argument (x/y/z/t...)
					else if (Fail(r) && F()) { // failed to read a term/value
						if (isArgument) {
							read.AddC(startR, read.From, ParseDictionary.Type.Arg); // color as argument
							read.TrimStart(1);
						}
						r.String = TrimEnd(read.Text[startR..read.From], 1); // remember string before ':'
						//Trim(ref from);
						if (read.Text.Length <= read.From || read.nextChar != ':' || !isArgument)
							return;
						++read.From; // eat default argument colon
						// Try to read argument default new([..expr]) is to let it reference already read arguments:
						read.TrimStart(1);
						r.Operand = new(read, out _, new([..expr]), 1, OpOrder.SubExpression); // cache=1 for recalling evaluated defArgs
						// CollectColors
						return; // false; //  unexpected end fail
					}
					read.TrimStart();
				} else {
					r.Term = new(Context, new(T.MakeR(1))); // unary inverse
					read.TrimStart(1);
				}

				if (End(false)) // unexpected ')', or no op, and return back successful
					return; // true; 
				// Read operators/comments:
				while ((o = read.nextChar switch {
					'+' => new Add(), '-' => new Sub(), '*' => new Mul(), '/' => new Div(), '\\' => new LDiv(), '^' => new Pow(), '$' => new Root(read.CharAtRel('$', 1)), '%' => new Mod(read.CharAtRel('%', 1)),
					'=' => new Equal(), '<' => new Less(read.CharAtRel('=', 1)), '>' => new More(read.CharAtRel('=', 1)),
					'[' => new Index(), '!' => new Exclamation(), '&' => new Sqr(), '~' => new Conj(), '#' => new Count(true), '@' => new Abs(true), '|' => new AbsRi(true), _ => new Mul(false)
				}).GetType() switch {
					var x when x == typeof(Sqr) => ++read.From > 0 && Encapsulate(new FuncOperator(Context, OpSqr, T.Sqr, OpCode.Sqr, expr[^1])), // sqr
					var x when x == typeof(Conj) => ++read.From > 0 && Encapsulate(new FuncOperator(Context, OpConj, INumber<T>.Conj, OpCode.Conj, expr[^1])), // conjugate
					var x when x == typeof(Exclamation) => DoubleOp('=', new Exclamation(OpOrder.Compare)) && ++read.From > 0 && Encapsulate(new FuncOperator(Context, OpFact, T.Factorial, OpCode.Factorial, expr[^1])), // factorial
					var x when x == typeof(Index) => ExtractTerms(), // index
					var x when x == typeof(Div) => read.IsComment(), // comment
					var x when x == typeof(Count) => DoubleOp('#', new Count()) && ++read.From > 0 && Encapsulate(new FuncCount(Context, OpCount, expr[^1]))
						|| (read.From += 2) > 0 && Encapsulate(new FuncCatCount(Context, OpCatCount, expr[^1])), // count / catCount
					var x when x == typeof(Abs) => DuO('@', new Abs(), OpAbs, (z) => T.MakeR(INumber<T>.Abs(z)), OpCode.Abs, OpSqrAbs, INumber<T>.SqrAbs, OpCode.SqrAbs), // abs / sqrAbs
					var x when x == typeof(AbsRi) => DuO('|', new AbsRi(), OpAbsRi, T.AbsComp, OpCode.Absri, OpSign, INumber<T>.Sign, OpCode.Sgn), // count / catCount
					_ => false
				}) {
					read.TrimStart();
					if (o.Order == 0 || read.From >= read.Text.Length) return; // true;
				}
				read.From += o.EatOp; // eat operator
				read.TrimStart(o.EatOp > 0 ? 1 : 0);
				o.Negative = r.Op.Negative; // move negative flag to the new operator
				if (LeftAssociate(o)) {
					nextOp = o; // perform left-associativity by returning back, and the parent will encapsulate
					return; // false;
				}
				// Read operand:
				while (true) {
					var fail = Fail((r.Operand = new(read, out o, args, cache, (r.Op = o).Order)).V);
					//CollectColors(r.Operand);
					if (fail) {
						if (r.Op.EatOp > 0 && F()) {
							return; // false; // failed to read operand
						}
						r.Op = new();
						break; // if it was operator-less multiplication - assume it was an expression end instead
					}
					if (o.Order == 0) break;
					// operand's next op has lower or equal order priority:
					// encapsulate my term into another term (wrap my term into parentheses), take the next operator and find the next operand to use it on
					_ = Encapsulate(new(Context, expr[^1], cache));
					if (!LeftAssociate(o))
						continue; // need to test associativity again, to let it recurse backwards. otherwise 2^2^2+1 would be 2^(2^2+1)
					nextOp = o; // perform left-associativity by returning back, and the parent will encapsulate
					return; // false;
				}
				return; // true;

				//bool IsChar(int from, char c = '=') => from < text.Length && text[from] == c;
				bool DuO(char c, Operator newOp, CallFunction parent1, Func<T, T> del1, OpCode op1, CallFunction parent2, Func<T, T> del2, OpCode op2)
					=> DoubleOp(c, newOp) && ++read.From > 0 && Encapsulate(new FuncOperator(Context, parent1, del1, op1, expr[^1]))
						|| ++read.From > 0 && Encapsulate(new FuncOperator(Context, parent2, del2, op2, expr[^1]));
				bool DoubleOp(char c, Operator newOp) {
					if (!read.CharAtRel(c, 1))
						return true; // must be a factorial, keep it
					o = newOp; // must be !=, change into that
					return false;
				}
				//bool IsComment(ref int from, ref int line) => read.CharAtRel('*', 1) && !Context.GotoFirstFailed(cancel, text, ref from, ref line, Colors, [],1,out _, out _, true);
				/*bool Comment(ref int from) {
					var before = from;
					++from; // eat initial /
					for (var go = true; go;) {
						var i = text.IndexOf('/', from);
						if (i < 0) {
							from = text.Length;
							o = new();
							AddColors(before, from, ParseDictionary.Type.Comment);
							return true;
						}
						go = text[i - 1] != '*';
						from += i - from + 1; //Eat((byte)(i + 1), ref text);
					}
					AddColors(before, from, ParseDictionary.Type.Comment);
					return true;
				}*/
				bool ExtractTerms() {
					++read.From;
					if (!SubTerm(out var indices, ']'))
						return Encapsulate(new FuncIndex(Context, expr[^1], indices.V));
					o = new(); // failed to parse indices
					return true;
				}
				bool SubTerm(out Expression readTo, char req) {
					var fail = Fail((readTo = new(read, out _, args)).V);
					//CollectColors(readTo);
					//r.String = r.Text += readTo.V.Text;
					read.TrimStart();
					return  (fail || readTo.V.Values.Length == 0 || FailRequiredSymbol(req)) && F();
				}
				bool FailRequiredSymbol(char c, int offset = 0) {
					if (read.GotoFirstFailed(offset, 0, [c], 1, out _, out var found)) 
						return F();
					read.From = found + 1; // goto behind the char we found
					return false;
				}
				/*void CollectColors(Expression e) {
					foreach (var c in e.Colors)
						Colors.Add(c); // collect sub expression parsing colors
				}*/
				bool End(bool allowNewLines) {
					bool endDefault = false;
					char next;
					if (!allowNewLines) {
						int beforeFrom = read.From, beforeLine = read.Line;
						while (!read.GotoFirstFailed(0,1, ['\n'], 0, out _, out _/*, false, 0, false, true, [false]*/)) 
							endDefault = true;
							//++read.From;
						var nf = read.From;
						read.From = beforeFrom;
						read.Line = beforeLine;
						if (!endDefault) 
							nf = beforeFrom;
						if (nf >= read.Text.Length) 
							return true;
						next = read.Text[nf];
					} else {
						if (read.TrimStart(1)) return true;
						next = read.nextChar;
					}

					bool result = read.TrimStart(allowNewLines ? 1 : 0) || next switch {
						// what counds as an expression end:
						')' => true, // ends parentheses
						',' => true, // divides vector element expressions
						'{' => true, // after if or while
						'}' => true, // after block
						';' => true, // separator
						'\n' => !allowNewLines, // separator
						'?' => true, // ternary
						':' => true, // ternary, default arguments, definitions
						']' => true, // ends indexer 
						// operators strictly allowing continuation:
						'+' => false, // add
						'-' => false, // subtract
						'*' => false, // multiply
						'/' => false, // div
						'%' => false, // mod
						'^' => false, // pow
						'$' => false, // root/log
						'&' => false, // sqr
						'|' => false, // absri
						'@' => false, // abs
						'#' => false, // count
						'~' => false, // conj
						'!' => false, // !read.CharAtRel('=',1) && endDefault, // unequal, but not factorial, as that could be while
						'<' => false, // less
						'>' => false, // more
						'=' => false, // equal
						'[' => false, // begin indexer, TODO allow newline after
						'(' => false, // TODO move cache between args
						//'(' // don't, could be a cache of a following command, make newlines after (// TODO newline after (parentheses and functions )
						_ => endDefault
					};
					return endDefault && !result ? read.TrimStart(1) : result; // f we found an op on the next line, then trim the newlines
				}
				bool Encapsulate(Expression p) {
					expr[^1] = r = new(T.NaN(), new(), null, p, null, false, read.Uncomment(startR,read.From));
					return true;
				}
				bool LeftAssociate(Operator testOp) => testOp.Right ? testOp.Order < left : testOp.Order <= left;
				bool Fail(Value test) => test.Term == null && (test.Values.Length == 0 || test.Values is [{ Term: null }]) /*&& test.Values[0].Values.Length == 0*/; // && test.Value.IsNaN; // no longer needed as even values are now nested in terms, and I don't test their insides.
				bool F() {
					r.Op = new();
					r.Leaf = T.NaN();
					r.Values = [];
					r.Term = r.Operand = null;
					char[] ends = [')', ',', '{', '}', ';', '\n', '?', ':', ']'];
					int e, end = read.Text.Length;
					var prevF = read.From;
				
					if(read.From < read.Text.Length)foreach (var et in ends)
						if ((e = read.Text.IndexOf(et, read.From)) >= 0 && e < end)
							end = e;
					//if(from > prevF) 
					//Colors.Add((prevF, ParseDictionary.Type.Error));
					if(prevF < end)read.AddC(prevF, end, ParseDictionary.Type.Error);
					
					//r.String = r.Text += text[..end].TrimStart(' ').TrimEnd(' ');
					//text = end < text.Length ? text[end..] : "";
					read.From = end;
					return true;
				} // reading failed
				//bool FailRequiredSymbol(char c, byte offset = 0) => !Char(c, offset) && F();
				bool TryFuncFailed() {
					int startFrom = read.From;
					foreach (var f in Context.Context.Get(read.Text, read.From, Functions)) {
						if (f.name.Length <= 0 || FailRequiredSymbol('(', (byte)f.name.Length)) continue;
						if ((Fail((r.Term = ((CallFunction)f.obj.Obj).Call(read, args)).V) || FailRequiredSymbol(')')) && F()) {
							read.AddC(startFrom, read.From, ParseDictionary.Type.Error);
							return true; // must eat func closing parenthesis
						}
						read.AddC(startFrom,startFrom+f.name.Length, f.obj.Type);
						return false;
					}
					return true;
				}
				bool Number(out Value number) {
					var startFrom = read.From;
					if (Char('_')) {
						read.AddC(startFrom, read.From, ParseDictionary.Type.Number);
						number = new(T.NaN()); // '_' is NaN
						return true;
					}
					if (RealNumber(out var real)) {
						read.AddC(startFrom, read.From, ParseDictionary.Type.Number);
						number = new(T.MakeR(real), 0, read.Uncomment(startR, read.From));
						return true;
					}
					number = None;
					return false;
				}
				bool RealNumber(out double number, double l = 0) {
					if (read.From < read.Text.Length) {
						if (read.nextChar == '.') {
							// eat decimal point
							++read.From; //_ = Eat(1, ref text);
							// get fractional part
							number = l + DecimalNumber();
							return true;
						}
						if (int.TryParse(read.nextChar.ToString(), out var i)) {
							l *= 10;
							// eat another digit
							++read.From; //_ = Eat(1, ref text);
							// add another whole digit, or finish
							_ = RealNumber(out number, l + i); // && 1 <= n ? 10 * i + n : i + n;
							return true;
						}
					}
					number = l; // no more digits
					return false;
				}
				double DecimalNumber(double d = 1) {
					if (read.From >= read.Text.Length) return 0; // no more digits
					d /= 10; // prepare another position
					if (!int.TryParse(read.nextChar.ToString(), out var i))
						return 0;
					++read.From; //Eat(1, ref text);
					return i * d + DecimalNumber(d);
				}
				bool Constant(out Value number) {
					// WARNING, if there is any function with the same name, then you can't operator-less multiply with parentheses from the right!
					// for example gamma is either eulerConstant or the gamma function:
					// gamma2 = eulerConstant*2, gamma(2+1) = evaluates gamma function at 2, (2+1)gamma = (2+1)*eulerConstant
					foreach (var c in Context.Context.Get(read.Text, read.From, Constants)) {
						if (c.name.Length <= 0) continue;
						number = ((Value)c.obj.Obj).Copy();
						read.AddC(read.From, read.From += c.name.Length, c.obj.Type);
						return true;
					}
					number = None;
					return false;
				}
				bool Argument(ref int[] resultNest) {
					foreach (var a in pArgs.Get(read.Text, read.From, (byte)ParseDictionary.Type.Arg)) {
						if (a.name.Length <= 0) continue;
						resultNest = (int[])a.obj.Obj;
						read.AddC(read.From, read.From += a.name.Length, a.obj.Type);
						return true;
					}
					return false;
				}
			}
			bool Char(char c, byte offset = 0) {
				var o = read.From + offset;
				var test = read.Text.Length > o && read.Text[o] == c;
				if (!test)
					return test;
				//r.String = r.Text += text[..++offset];
				read.From += offset + 1; //R(offset, ref from);
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
		/*public Expression(Reader read, Value args, int from = 0, int cache = 0) {
			Context = read.Context;
			var e = new Expression(read, out _, args, cache);
			V = e.V;
			V.String = V.Text += read.Text[from..];
			//Colors = e.Colors;
			//V = text == "" ? e.V : None;
			_cache = new(cache);
		}*/
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