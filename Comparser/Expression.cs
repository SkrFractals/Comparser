using Comparser.Comparser.Numbers;
using static Comparser.Comparser.Numbers.ILeaf;
namespace Comparser.Comparser;
public /*abstract*/  partial class Comparser/*<T> where T : unmanaged, IScalar<T>*/  {
	public class Expression {
		
		
		public static FailReason Err(ref FailReason error, Value b) => error = (FailReason)Math.Max((byte)error, (byte)b.Error);

		
		#region Content
		// Contains user-defined custom function
		protected readonly Comparser/*<T>*/ Context;
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
		/// <param name="allowCache">should be disabled when multitasking as it is not thread safe</param>
		/// <returns>Evaluated value of this expression</returns>
		public virtual Value Eval(ushort depth, Value args, bool allowCache = true) {
			if (allowCache && _cache.GetEval(args)) return _cache.Result?.Eval!;
			Value result = new(new Value[V.Values.Length]);
			var error = FailReason.Success;
			if (V.Values.Length == 0)
				result = EvalValue(depth, V, args, allowCache);
			else
				for (var e = 0; e < V.Values.Length; Err(ref error, result.Values[e]), ++e)
					result.Values[e] = EvalValue(depth, V.Values[e], args, allowCache);
			var t = V.Text;
			if (error == 0) {
				result = CollapseScalar(result);
				result.Text = t;
			}else result = new(error, t);
			if(allowCache)
				_cache.Insert(args, result);
			return result;
		}
		public Value EvalCopy(ushort depth, Value args, bool allowCache) => new Expression(Context, V, _cache).Eval(depth, args, allowCache);
		protected Value EvalValue(ushort depth, Value v, Value args/*, bool allowArg = false*/, bool allowCache) {
			var a = args.Values;
			if (v.Values.Length == 0) {
				var eval =  Value.Operate2(
					v.Term?.Eval(depth, args, allowCache) ?? new([v.Arg.Length == 0 ? v : GetArg(v.Arg, a)], v.Error, v.Text, v.String),
					v.Operand?.Eval(depth, args, allowCache) ?? None, v.Op.Op, v.Op.SOp, depth, Context, args, allowCache, v.Op is Mul);
				eval.Operand = v.Operand; // copy possible default argument
				if(eval.Text == "") eval.Text = v.Text;
				return eval;
			}
			Value result = new(new Value[v.Values.Length]);
			for (var e = 0; e < v.Values.Length; ++e) result.Values[e] = EvalValue(depth, v.Values[e], args, allowCache);
			result.Text = v.Text;
			return result;
			Value GetArg(int[] arg, Value[] argVals) {
				for (var i = 0; arg[i] < argVals.Length; ++i) {
					var valI = argVals[arg[i]];
					if (i + 1 == arg.Length)
						return EvalArg(depth, valI, args, allowCache);
					argVals = valI.Values;
				}
				return v;
			}
		}
		private Value EvalArg(ushort depth, Value arg, Value args, bool allowCache) => depth > Context._stackOverflow ? StackOverflow 
			: arg.Leaf.IsNaN() && arg.Operand != null ? arg.Operand?.Eval((ushort)(1 + depth), args, allowCache) ?? arg : arg;
		
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
			Operator o; nextOp = new();
			do { // Read vector loop:
				startR = read.From;
				// Init read
				expr.Add(r = new() {
					Op = { Negative = Char('-') },
					Leaf = nan
				});
				nextOp = new();
				_ = End(false) // unexpected ')', or no op, and return back successful
					|| ReadUnaryOperatorReturned() // read unary
					|| ReadTermReturned() // read term
					|| CollapseTerm(ref r.Term!) // pre-eval const term
					|| End(false) // test expression end
					|| ReadOperatorReturn() // read operator
					|| o.EatOp == 0 && !Context._operatorLess // operator-less multiply is blocked
					|| ProcessBinaryOperator(ref nextOp) // eat operator chars and try to perform left-association
					|| ReadOperand(ref nextOp); // Read operand
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

			#region Read Terms and Operators
			bool ReadTermReturned() {
				if (o.Order != 0) {
					// we had a unary operator (the only one I have to far is the unary inverse, so do that)
					r.Term = new(Context, new(unit)); // unary inverse (pretend we have just successfully read "1/")
					read.TrimStart(1); // trim white space
					return true; // then return and go read a second operand
				}
				// no unary - read teh first term like normal: Try parenthesis/function/number/constant/argument:
				if (ReadTermProperNeedsFailTest() && Fail(r) && F())
					return true;
				read.TrimStart();
				return false;

				bool ReadTermProperNeedsFailTest() {
					// definitions do not allow parenthesis unless they got one right at the beginning of the top level
					// if opening parenthesis, allow a newline,and read a subterm to encapsulate
					var startTerm = read.From;
					if (Char('"') && ReadString(out r.Term) && Delegate(r.Term.V.String) // try string, and possibly delegate it
						|| (parseAs != ParseAs.Definition || left == 0) && Char('(') && !read.TrimStart(1) && SubTerm(out r.Term, ')') // try parenthesis group
						|| TryFunc() // try read function
						|| !(ReadNumber(out var n) || ReadConst(out n) && Delegate(n.String)) // read number OR const OR argument, const/argument and possibly delegate it:
						) return true;
					// these calls only return value, need to encapsulate that into a term
					r.Term = new(Context, n, cache);
					return false;

					bool Delegate(string s) => !(Context.UserFunctions.TryGetValue(s, out var f) || Context.DefaultFunctions.TryGetValue(s, out f)) // does the string value match any function name?
						|| read.GotoFirstFailed(0, 2, ['('], 1, out _, out _) // if yes, try to eat the next opening parenthesis
						|| !CallFunction(f, startTerm, read.From - 1, ParseDictionary.Type.PointerF); // and then actually parse calling that function

				}
			}
			bool ReadUnaryOperatorReturned() {
				while ((o = read.nextChar switch {
					'/' => new Div(), '\\' => new LDiv(), _ => new()
				}).GetType() switch {
					var x when x == typeof(Div) => read.IsComment(), // comment
					_ => false
				}) {
					if (o.Order == 0)
						return true;
					read.TrimStart();
					if (read.From >= read.Text.Length) return true;
				}
				return false;
			}
			bool ReadOperatorReturn() {
				// Try to read a binary operator, or apply and encapsulate a post-fix unary operator if it happens to be one
				while ((o = read.nextChar switch {
					'+' => new Add(), '-' => new Sub(), '*' => new Mul(), '/' => new Div(), '\\' => new LDiv(), '^' => new Pow(), '$' => new Root(read.CharAtRel('$', 1)), '%' => new Mod(read.CharAtRel('%', 1)),
					'=' => new Equal(), '<' => new Less(read.CharAtRel('=', 1)), '>' => new More(read.CharAtRel('=', 1)),
					'[' => new Index(), '!' => new Exclamation(), '&' => new Sqr(), '~' => new Conj(), '#' => new Count(true), '@' => new Abs(true), '|' => new AbsRi(true), _ => new Mul(false)
				}).GetType() switch {
					var x when x == typeof(Sqr) => ++read.From > 0 && Encapsulate(new FuncOperator(Context, OpSqr, ILeaf.Sqr, OpCode.Sqr, expr[^1])), // sqr
					var x when x == typeof(Conj) => ++read.From > 0 && Encapsulate(new FuncOperator(Context, OpConj, ILeaf.Conj, OpCode.Conj, expr[^1])), // conjugate
					var x when x == typeof(Exclamation) => SecondOpCharMissing('=', new Exclamation(OpOrder.Compare)) && ++read.From > 0 && Encapsulate(new FuncOperator(Context, OpFact, Factorial, OpCode.Factorial, expr[^1])), // factorial
					var x when x == typeof(Index) => ExtractTerms(), // index
					var x when x == typeof(Div) => read.IsComment(), // comment
					var x when x == typeof(Count) => SecondOpCharMissing('#', new Count()) && ++read.From > 0 && Encapsulate(new FuncCount(Context, OpCount, expr[^1]))
						|| (read.From += 2) > 0 && Encapsulate(new FuncCatCount(Context, OpCatCount, expr[^1])), // count / catCount
					var x when x == typeof(Abs) => DuO('@', new Abs(), OpAbs, T_Abs, OpCode.Abs, OpSqrAbs, T_SqrAbs, OpCode.SqrAbs), // abs / sqrAbs
					var x when x == typeof(AbsRi) => DuO('|', new AbsRi(), OpCompAbs, AbsComp, OpCode.Absri, OpSign, Sign, OpCode.Sgn), // count / catCount
					_ => false
				}) {
					read.TrimStart();
					if (o.Order == 0 || read.From >= read.Text.Length) return true;
				}
				return false;
			}
			bool ReadOperand(ref Operator nextOp) {
				while (true) {
					var fail = Fail((r.Operand = new(read, out o, args, cache, (r.Op = o).Order, parseAs)).V);
					if (fail) {
						if (parseAs != ParseAs.Expression && F()) // operand failed when we're looking for an argument - go back and take the string and try the defArg there 
							break;
						if (r.Op.EatOp > 0 && F())
							return false; // false; // failed to read operand
						r.Op = new();
						break; // if it was trying to be an operator-less multiplication - assume it was an expression end instead, because we literally read nothing
					}
					CollapseTerm(ref r.Operand);
					if (o.Order == 0) break;
					// operand's next op has lower or equal order priority:
					// encapsulate my term into another term (wrap my term into parentheses), take the next operator and find the next operand to use it on
					_ = Encapsulate(new(Context, expr[^1], cache));
					if (NotLeftAssociate(o))
						continue; // need to test associativity again, to let it recurse backwards. otherwise 2^2^2+1 would be 2^(2^2+1)
					nextOp = o; // perform left-associativity by returning back, and the parent will encapsulate
					return true; // false;
				}
				return false;
			}
			bool DuO(char c, Operator newOp, CallFunction parent1, Func<ILeaf, ILeaf> del1, OpCode op1, CallFunction parent2, Func<ILeaf, ILeaf> del2, OpCode op2)
				=> SecondOpCharMissing(c, newOp) && ++read.From > 0 && Encapsulate(new FuncOperator(Context, parent1, del1, op1, expr[^1]))
					|| (read.From += 2) > 0 && Encapsulate(new FuncOperator(Context, parent2, del2, op2, expr[^1]));
			bool SecondOpCharMissing(char c, Operator newOp) {
				if (!read.CharAtRel(c, 1))
					return true; // must be the single-character operator, go to the single-character branch (true)
				o = newOp; // found the second possible character for this op -> replace teh operator and go to the double-char branch (false) 
				return false;
			}
			#endregion

			#region Orders Of Operations
			bool SubTerm(out Expression readTo, char req) {
				var fail = Fail((readTo = new(read, out _, args, 0, 0, parseAs >= ParseAs.Definition ? ParseAs.DefinitionExp : parseAs)).V);
				read.TrimStart();
				return (fail || readTo.V.Values.Length == 0 || FailRequiredSymbol(req)) && F();
			}
			bool Encapsulate(Expression p) {
				// TEST I just moved the unary minus out of encapsulation, test if that's ok every time
				var n = r.Op.Negative;
				r.Op.Negative = false;
				expr[^1] = r = new(nan, new(), null, p, null, false, read.Uncomment(startR, read.From));
				r.Op.Negative = n;
				CollapseTerm(ref p);
				return true;
			}
			bool NotLeftAssociate(Operator testOp) => testOp.Right ? testOp.Order >= left : testOp.Order > left;
			bool ProcessBinaryOperator(ref Operator nextOp) {
				read.From += o.EatOp; // eat operator
				read.TrimStart(o.EatOp > 0 ? 1 : 0);
				o.Negative = r.Op.Negative; // move negative flag to the new operator
				if (NotLeftAssociate(o))
					return false;
				nextOp = o; // perform left-associativity by returning back, and the parent will encapsulate
				return true;
			}
			#endregion

			#region Read Function/Delegate Calls
			bool TryFunc() {
				var startFrom = read.From;
				foreach (var (name, obj) in Context.Context.Get(read.Text, read.From, Functions))
					if (name.Length > 0 && !FailRequiredSymbol('(', (byte)name.Length, true))
						return CallFunction((CallFunction)obj.Obj, startFrom, startFrom + name.Length, obj.Type);
				return false;
			}
			bool CallFunction(CallFunction f, int startFrom, int endFrom, ParseDictionary.Type type) {
				read.TrimStart(1); // allow newline after opening parenthesis
				if ((Fail((r.Term = f.Call(read, args)).V) || FailRequiredSymbol(')')) && F()) {
					read.AddC(startFrom, read.From, ParseDictionary.Type.Error);
					return false; // must eat func closing parenthesis
				}
				read.AddC(startFrom, endFrom, type);
				return true;
			}
			#endregion

			#region Read Values
			bool ReadNumber(out Value number) {
				var startFrom = read.From;
				if (Char('_')) {
					read.AddC(startFrom, read.From, ParseDictionary.Type.Number);
					number = new(nan); // '_' is NaN
					return true;
				}
				if (RealNumber(out var real)) {
					read.AddC(startFrom, read.From, ParseDictionary.Type.Number);
					number = new((Real)(real), 0, read.Uncomment(startR, read.From));
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
			bool ReadConst(out Value number) {
				var found = ConstType(out var readArg, pArgs, (byte)ParseDictionary.Type.Arg); // function arguments
				if (ConstType(out var readConst, Context.Context, Constants) || found) { // constants
					if (readConst.name.Length <= readArg.name.Length) {
						number = new(read.Uncomment(startR, read.From), (int[])readArg.obj.Obj); // the longest match was an argument
						AddC(readArg);
						return true;
					}
					number = ((Value)readConst.obj.Obj).Copy(); // the longest match was a constant
					AddC(readConst);
					return true;
					void AddC((string name, ParseDictionary.S obj) c) => read.AddC(read.From, read.From += c.name.Length, c.obj.Type);
				}
				number = None;
				return false;
			}
			bool ConstType(out (string name, ParseDictionary.S obj) number, ParseDictionary dic, byte type /*, Func<(string name, ParseDictionary.S obj),Value> make*/) {
				foreach (var c in dic.Get(read.Text, read.From, type)) {
					if ((number = c).name.Length <= 0) continue;
					number = c; //number = make(c);

					return true;
				}
				number = ("", new());
				return false;
			}
			bool ReadString(out Expression readTo) {
				var before = read.From;
				if (read.GotoFirstFailed(1, 2, [], 0, out _, out _, false, 0, true)) {
					readTo = new(Context, None);
					return !F();
				}
				var s = read.Uncomment(before, read.From - 1);
				read.AddC(before, read.From - 1, ParseDictionary.Type.String);
				for (var i = 0; (i = s.IndexOf('\\', i)) >= 0;) {
					if (i + 1 < s.Length)
						switch (s[i + 1]) {
						case '\\': s = s.Remove(++i, 1); break; // intentional backslash in string
						case 'n': R("\n"); break; // intentional newline in string
						case 't': R("\t"); break; // intentional newline in string
						case 'r': R("\r"); break; // intentional newline in string
						default:
							++i;
							break;
							void R(string character) {
								s = s.Remove(i, 2).Insert(i, character);
								++i;
							}
						}
				}
				readTo = new(Context, new(0, s));
				return true;
			}
			bool ExtractTerms() {
				++read.From;
				if (!SubTerm(out var indices, ']'))
					return Encapsulate(new FuncIndex(Context, expr[^1], indices.V));
				o = new(); // failed to parse indices
				return true;
			}
			#endregion

			#region Fails/Endings
			bool End(bool allowNewLines) {
				read.TrimStart();
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
			bool FailRequiredSymbol(char c, int offset = 0, bool doNotFail = false) {
				if (read.GotoFirstFailed(offset, 0, [c], 1, out _, out var found))
					return doNotFail || F();
				read.From = found + 1; // goto behind the char we found
				return false;
			}
			bool Fail(Value test) => test.Term == null && (test.Values.Length == 0 || test.Values is [{ Term: null }]);
			bool F() {
				(r.Op, r.Leaf, r.Values, r.Term, r.Operand) = (new(), nan, [], null, null);
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
			#endregion

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
			bool CollapseTerm(ref Expression exp) {
				if (Context.PreEvaluate && !CollapseValue(exp.V))
					exp = new(Context, exp.Eval(0, None, false));
				return false;
			}
            bool CollapseValue(Value v) => !Context.PreEvaluate || (v.Values.Length > 0
                    ? (v.HasArgs |= CollapseValues(v.Values))
                    : (v.Arg.Length > 0 || v.Term != null && (v.Term.V.HasArgs || v.Operand is { V.HasArgs: true })) && (v.HasArgs = true));
            bool CollapseValues(Value[] vals) {
                var has = false;
                foreach (var val in vals)
                    has |= CollapseValue(val);
                return has;
            }
            bool Char(char c, byte offset = 0) {
				var off = read.From + offset;
				var test = read.Text.Length > off && read.Text[off] == c;
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
		public Expression(Comparser/*<T>*/ context, Value t, int cache = 0) {
			_cache = new(cache);
			Context = context;
			V = new(t.Values.Length > 0 ? t.Values : [new(t.Leaf, t.Op, t.Arg, t.Term, t.Operand, t.Op.Negative, t.String)], t.Error, t.Text);
		}
		// copy
		private Expression(Comparser/*<T>*/ context, Value t, CallFunction.EvalCache cache) {
			_cache = cache;
			Context = context;
			V = t.Copy();
		}
		#endregion
		
		private const byte Functions = (byte)ParseDictionary.Type.UserF | (byte)ParseDictionary.Type.DefaultF;
		private const byte Constants = (byte)ParseDictionary.Type.UserC | (byte)ParseDictionary.Type.DefaultC;
	}
}