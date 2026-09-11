using Comparser.Comparser.Numbers;
namespace Comparser.Comparser;
public abstract partial class Comparser<T> {
	public class Expression {
		
		
		public static FailReason Err(ref FailReason error, Value b) => error = (FailReason)Math.Max((byte)error, (byte)b.Error);

		
		#region Content
		// Contains user-defined custom function
		protected readonly Comparser<T> Context;
		// Parsed and evaluated data
		public Value V;
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
		public virtual Value Eval(ushort depth, Value args) {
			if (_cache.GetEval(args)) return _cache.Result?.Eval!;
			Value result = new(new Value[V.Values.Length]);
			FailReason error = FailReason.Success;
			if (V.Values.Length == 0)
				result = EvalValue(depth, V, args);
			else
				for (var e = 0; e < V.Values.Length; Err(ref error, result.Values[e]), ++e)
					result.Values[e] = EvalValue(depth, V.Values[e], args);
			var t = V.Text;
			if (error == 0) {
				result = CollapseScalar(result);
				result.Text = t;
			}else result = new(error, t);
			_cache.Insert(args, result);
			return result;
		}
		public Value EvalCopy(ushort depth, Value args) => new Expression(Context, V, _cache).Eval(depth, args);
		protected Value EvalValue(ushort depth, Value v, Value args/*, bool allowArg = false*/) {
			var a = args.Values;
			if (v.Values.Length == 0) {
				var eval =  Value.Operate2(
					v.Term?.Eval(depth, args) ?? new([v.Arg.Length == 0 ? v : GetArg(v.Arg, a)], v.Error, v.Text, v.String),
					v.Operand?.Eval(depth, args) ?? None, v.Op.Op, v.Op.SOp, depth, Context, args, v.Op is Mul);
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
			: T.IsNaN(arg.Leaf) && arg.Operand != null ? arg.Operand?.Eval((ushort)(1 + depth), args) ?? arg : arg;
		
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
		public static GpuValue GpuParseValue(ushort depth, Value v) 
			=> v.Op.Gop(v.Term?.GpuParse(depth) ?? (v.Arg.Length == 0 ? new(v.Leaf) : new(v.Arg)), v.Operand?.GpuParse(depth) ?? new());
		#endregion

		public enum ParseAs : byte {
			Expression = 0,		// tries to parse an evaluable expression
			Argument = 1,		// always successfully returns a string if parsing fails at any point, finally will try to read defArg into its Operand
			Definition = 2,		// always successfully returns a string if parsing fails at any point, first level parenthesis is only allowed at the very beginning
			DefinitionExp = 3	// if we got parenthesis at beginning, this mode will allow further deeper parentheses
		}

		#region Parse Constructors
		/// <summary>
		/// Reads and parses an expression string
		/// </summary>
		/// <param name="read">parser class providing the Comparser context and read stream</param>
		/// <param name="nextOp">returns operand's operator if that operand should be left-associated with my term, will encapsulate previous operator into my term, and use nextOp on next operand</param>
		/// <param name="args">argument value, will substitute every x in the string</param>
		/// <param name="cache">cache size of this new Expression</param>
		/// <param name="left">what order of operations was my parent's operator? Used to test for associativity</param>
		/// <param name="parseAs">what are e expecting this to be? Modifies what it is allowed to parse and return.</param>
		public Expression(Reader read, out Operator nextOp, Value args, int cache = 0, OpOrder left = 0, ParseAs parseAs = ParseAs.Expression) {
			Context = read.Context;
			
			// Init variables
			int startR, start = read.From;
			FailReason error = 0;
			_cache = new(cache);
			Value /*t = new(),*/ r;
			List<Value> expr = [];
			
			// Parse Arguments:
			ParseDictionary pArgs = new();
			Nest();
			read.TrimStart();
			nextOp = new();
			do { // Read vector loop:
				startR = read.From;
				Read(out nextOp);
				if (r.Text == "")
					r.Text = read.Uncomment(startR, read.From); // if it didn't remember pre-defaultArg string, it will take it here
				if (r.String == "")
					r.String = (r.Term?.V.String ?? "") != "" ? r.Term?.V.String! : r.Text; // if it didn't remember pre-defaultArg string, it will take it here
				CollapseValue(r);
				Err(ref error, r);
				read.TrimStart();
			} while (ParseContinue());

			// Save vector to my values:
			V = new([..expr], error, read.Uncomment(start, read.From));
			return;

			void Read(out Operator nextOp) {
				// Init read
				r = new();
				expr.Add(r);
				nextOp = r.Op = new();
				r.Op.Negative = Char('-');
				r.Leaf = T.nan;
				read.TrimStart();
				// Try parenthesis/function/number/constant/argument:
				//int[] argNest = [];

				// unary operators:
				Operator o;
				if (End(false)) // unexpected ')', or no op, and return back successful
					return;
				while ((o = read.nextChar switch {
					'/' => new Div(), '\\' => new LDiv(), _ => new()
				}).GetType() switch {
					var x when x == typeof(Div) => read.IsComment(), // comment
					_ => false
				}) {
					if (o.Order == 0)
						return;
					read.TrimStart();
					if (read.From >= read.Text.Length) return;
				}
				// no unary:
				if (o.Order == 0) {
					var startTerm = read.From;
					if ((
							parseAs == ParseAs.Definition && left != 0 // definitions do not allow parenthesis unless they got one right at the beginning of the top level
							|| !Char('(') 
							|| !read.TrimStart(1) && SubTerm(out r.Term, ')')
							) // if opening parenthesis, allow a newline,and read a subterm to encapsulate
						&& (!Char('"') || ReadString(out r.Term))
						&& TryFuncFailed() // _term = default function
						&& (Number(out var n) // _value = number
							|| Const(out n, pArgs, (byte)ParseDictionary.Type.Arg, c => new(read.Uncomment(startR, read.From), (int[])c.obj.Obj)) // function arguments
							|| Const(out n, Context.Context, Constants, c => ((Value)c.obj.Obj).Copy()) // _value = constant
							&& (!(Context.UserFunctions.TryGetValue(n.String, out var f) || Context.DefaultFunctions.TryGetValue(n.String, out f))
								|| read.GotoFirstFailed(0, 2, ['('], 1, out _, out _)
								|| CallF(f, startTerm, read.From - 1, ParseDictionary.Type.PointerF))
						)) r.Term = new(Context, n, cache); // just a value
					else if (Fail(r) && F()) return;//if(Fail(r) && F()) TryComment();
					read.TrimStart();
				} else {
					r.Term = new(Context, new(T.unit)); // unary inverse
					read.TrimStart(1);
				}
				// collapse constant evaluations:
				CollapseTerm(ref r.Term!);
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
					var x when x == typeof(Abs) => DuO('@', new Abs(), OpAbs, INumber<T>.T_Abs, OpCode.Abs, OpSqrAbs, INumber<T>.SqrAbs, OpCode.SqrAbs), // abs / sqrAbs
					var x when x == typeof(AbsRi) => DuO('|', new AbsRi(), OpCompAbs, T.AbsComp, OpCode.Absri, OpSign, INumber<T>.Sign, OpCode.Sgn), // count / catCount
					_ => false
				}) {
					read.TrimStart();
					if (o.Order == 0 || read.From >= read.Text.Length) return; // true;
				}
				if (o.EatOp == 0 && !Context._operatorLess)
					return; // operator-less multiply is blocked
				read.From += o.EatOp; // eat operator
				read.TrimStart(o.EatOp > 0 ? 1 : 0);
				o.Negative = r.Op.Negative; // move negative flag to the new operator
				if (LeftAssociate(o)) {
					nextOp = o; // perform left-associativity by returning back, and the parent will encapsulate
					return; // false;
				}
				// Read operand:
				while (true) {
					var fail = Fail((r.Operand = new(read, out o, args, cache, (r.Op = o).Order, parseAs)).V);
					if (fail) {
						if (parseAs != ParseAs.Expression && F()) // operand failed when we're looking for an argument - go back and take the string and try the defArg there 
							break;
						if (r.Op.EatOp > 0 && F())
							return; // false; // failed to read operand
						r.Op = new();
						break; // if it was trying to be an operator-less multiplication - assume it was an expression end instead, because we literally read nothing
					}
					CollapseTerm(ref r.Operand);
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

				bool DuO(char c, Operator newOp, CallFunction parent1, Func<T, T> del1, OpCode op1, CallFunction parent2, Func<T, T> del2, OpCode op2)
					=> DoubleOp(c, newOp) && ++read.From > 0 && Encapsulate(new FuncOperator(Context, parent1, del1, op1, expr[^1]))
						|| (read.From += 2) > 0 && Encapsulate(new FuncOperator(Context, parent2, del2, op2, expr[^1]));
				bool DoubleOp(char c, Operator newOp) {
					if (!read.CharAtRel(c, 1))
						return true; // must be a factorial, keep it
					o = newOp; // must be !=, change into that
					return false;
				}
				bool ExtractTerms() {
					++read.From;
					if (!SubTerm(out var indices, ']'))
						return Encapsulate(new FuncIndex(Context, expr[^1], indices.V));
					o = new(); // failed to parse indices
					return true;
				}
				bool SubTerm(out Expression readTo, char req) {
					var fail = Fail((readTo = new(read, out _, args, 0, 0, parseAs >= ParseAs.Definition ? ParseAs.DefinitionExp : parseAs)).V);
					read.TrimStart();
					return (fail || readTo.V.Values.Length == 0 || FailRequiredSymbol(req)) && F();
				}
				bool ReadString(out Expression readTo) {
					var before = read.From;
					if (read.GotoFirstFailed(1, 2, [], 0, out _, out _, false, 0, true)) {
						readTo = new(Context, None);
						return F();
					}
					var s = read.Uncomment(before, read.From - 1);
					for (var i = 0; (i = s.IndexOf('\\', i)) >= 0;) {
						if (i + 1 < s.Length)
							switch (s[i + 1]) {
							case '\\': s = s.Remove(++i, 1); break; // intentional backslash in string
							case 'n': R("\n"); break; // intentional newline in string
							case 't': R("\t"); break; // intentional newline in string
							case 'r': R("\r"); break; // intentional newline in string
							default: ++i; break;
								void R(string character) {
									s = s.Remove(i, 2).Insert(i, character);
									++i;
								}
							}
					}
					readTo = new(Context, new(0, s));
					return false;
				}
				bool FailRequiredSymbol(char c, int offset = 0) {
					if (read.GotoFirstFailed(offset, 0, [c], 1, out _, out var found))
						return F();
					read.From = found + 1; // goto behind the char we found
					return false;
				}
				bool End(bool allowNewLines) {
					bool endDefault = false;
					char next;
					if (!allowNewLines) {
						int beforeFrom = read.From, beforeLine = read.Line;
						while (!read.GotoFirstFailed(0, 1, ['\n'], 0, out _, out _))
							endDefault = true;
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
					var result = read.TrimStart(allowNewLines ? 1 : 0) || next switch {
						// what counts as an expression end:
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
						'|' => false, // absRi
						'@' => false, // abs
						'#' => false, // count
						'~' => false, // conj
						'!' => false, // unequal, but not factorial, as that could be while
						'<' => false, // less
						'>' => false, // more
						'=' => false, // equal
						'[' => false, // begin indexer
						//'(' => false, // a definition can begin with a parenthesis, so those are not allowed on a new line
						_ => endDefault
					};
					return endDefault && !result ? read.TrimStart(1) : result; // if we found an op on the next line, then trim the newlines
				}
				bool Encapsulate(Expression p) {
					// TEST I just moved the unary minus out of encapsulation, test if that's ok every time
					var n = r.Op.Negative;
					r.Op.Negative = false;
					expr[^1] = r = new(T.nan, new(), null, p, null, false, read.Uncomment(startR, read.From));
					r.Op.Negative = n;
					CollapseTerm(ref p);
					return true;
				}
				bool LeftAssociate(Operator testOp) => testOp.Right ? testOp.Order < left : testOp.Order <= left;
				bool Fail(Value test) => test.Term == null && (test.Values.Length == 0 || test.Values is [{ Term: null }]);
				bool F() {
					(r.Op, r.Leaf, r.Values, r.Term, r.Operand) = (new(), T.nan, [], null, null);
					int e, end = read.Text.Length, prevF = read.From;
					char[] ends = [')', ',', '{', '}', ';', '\n', '?', ':', ']', '/'];
					if (read.From < read.Text.Length)
						foreach (var et in ends)
							if ((e = read.Text.IndexOf(et, read.From)) >= 0 && e < end)
								end = e;
					if (prevF < end)
						read.AddC(prevF, end, ParseDictionary.Type.Error);
					read.From = end;
					if (parseAs == ParseAs.Expression)
						r.Error = (FailReason)Math.Max((byte)FailReason.BadExpression, (byte)r.Error);
					return true;
				} // reading failed
				bool TryFuncFailed() {
					var startFrom = read.From;
					foreach (var f in Context.Context.Get(read.Text, read.From, Functions))
						if (f.name.Length > 0 && !FailRequiredSymbol('(', (byte)f.name.Length))
							return CallF((CallFunction)f.obj.Obj, startFrom, startFrom + f.name.Length, f.obj.Type);
					return true;
				}
				bool CallF(CallFunction f, int startFrom, int endFrom, ParseDictionary.Type type) {
					read.TrimStart(1); // allow newline after opening parenthesis
					if ((Fail((r.Term = f.Call(read, args)).V) || FailRequiredSymbol(')')) && F()) {
						read.AddC(startFrom, read.From, ParseDictionary.Type.Error);
						return true; // must eat func closing parenthesis
					}
					read.AddC(startFrom, endFrom, type);
					return false;
				}
				bool Number(out Value number) {
					var startFrom = read.From;
					if (Char('_')) {
						read.AddC(startFrom, read.From, ParseDictionary.Type.Number);
						number = new(T.nan); // '_' is NaN
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
							++read.From;
							// get fractional part
							number = l + DecimalNumber();
							return true;
						}
						if (int.TryParse(read.nextChar.ToString(), out var i)) {
							l *= 10;
							// eat another digit
							++read.From; 
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
					++read.From; 
					return i * d + DecimalNumber(d);
				}
				bool Const(out Value number, ParseDictionary dic, byte type, Func<(string name, ParseDictionary.S obj),Value> make) {
					foreach (var c in dic.Get(read.Text, read.From, type)) {
						if (c.name.Length <= 0) continue;
						number = make(c);
						read.AddC(read.From, read.From += c.name.Length, c.obj.Type);
						return true;
					}
					number = None;
					return false;
				}
			}
			bool ParseContinue() {
				if (left != 0) // only the top-level layer is allowed to follow up with ',' or ':'
					return false;
				int s;
				switch (parseAs) {
				case ParseAs.Definition: // definitions are not allowed to follow with ',' or ':' inside their expressions
					TrimString();
					return false;
				case ParseAs.Argument:
					TrimString();
					read.AddC(startR, read.From, ParseDictionary.Type.Arg); // color as argument
					read.TrimStart(1);
					if (read.GotoFirstFailed(0, 2, [',', ':'], 1, out s, out _))
						return false;
					if (s == 0)
						return true; // found ',', so return false to try read another argument
					// found ':', so try to read argument default new([..expr]) is to let it reference already read arguments:
					read.TrimStart(1);
					r.Operand = new(read, out _, new([..expr]), 1, OpOrder.SubExpression); // cache=1 for recalling evaluated defArgs
					CollapseTerm(ref r.Operand);
					goto default; // after reading the defArd, go try read ',' again, but with ':' not allowed again
				default:
					return !read.GotoFirstFailed(0, 2, [','], 1, out s, out _);
					void TrimString() => r.String = TrimEnd(read.Uncomment(startR, read.From), 1); // remember string before ':'
				}
			}
			// experimental - pre-evaluate parts of expressions that are not dependent on any arguments:
			void CollapseTerm(ref Expression exp) {
				if (Context.PreEvaluate && !CollapseValue(exp.V))
					exp = new(Context, exp.Eval(0, None));
			}
			bool CollapseValue(Value v) {
				if (!Context.PreEvaluate)
					return true;
				if (v.Values.Length > 0)
					return v.HasArgs |= CollapseValues(v.Values);
				if (v.Arg.Length > 0 || v.Term != null && (v.Term.V.HasArgs || v.Operand != null && v.Operand.V.HasArgs))
					return v.HasArgs = true;
				return false;
				bool CollapseValues(Value[] vals) {
					var has = false;
					foreach (var val in vals) 
						has |= CollapseValue(val);
					return has;
				}
			}
			bool Char(char c, byte offset = 0) {
				var o = read.From + offset;
				var test = read.Text.Length > o && read.Text[o] == c;
				if (!test)
					return test;
				read.From += offset + 1;
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
		public Expression(Comparser<T> context, Value t, int cache = 0) {
			_cache = new(cache);
			Context = context;
			V = new([new(t.Leaf, t.Op, t.Arg, t.Term, t.Operand, t.Op.Negative, t.String)], t.Error, t.Text);
		}
		// copy
		private Expression(Comparser<T> context, Value t, CallFunction.EvalCache cache) {
			_cache = cache;
			Context = context;
			V = t.Copy();
		}
		#endregion
		
		private const byte Functions = (byte)ParseDictionary.Type.UserF | (byte)ParseDictionary.Type.DefaultF;
		private const byte Constants = (byte)ParseDictionary.Type.UserC | (byte)ParseDictionary.Type.DefaultC;
	}
}