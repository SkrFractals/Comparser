using Comparser.Comparser.Numbers;
using System.Text.RegularExpressions;
using static Comparser.Comparser.Numbers.ILeaf;
namespace Comparser.Comparser;
public interface IComparser {
	public object MakeArgs(string[] args);
	public object MakeArgs((string alias, object value)[] args);
	public object Parse(CancellationToken cancel, string text, int from, out (int position, Color color)[] colors, object? args = null);
	// Re-evaluates already parsed expression with new arguments
	public object Eval(object exp, object? args = null, bool allowCache = true);
	// Parses and evaluates the text with selected arguments (and returns the expression for possible re-evaluation)
	//public object ParseEval(string text, int from, out object expr, object? args = null);
	public object ParseEval(CancellationToken cancel, string text, int from, out object expr, out (int position, Color color)[] colors, object? args = null, bool allowCache = true);
	// Parses and evaluates the text with selected arguments (and returns the expression for possible re-evaluation, and eats the parsed part, allows it to be incomplete - leaving the remainder in the ref text)
	//public object ParseEval(string text, ref int from, object? args = null);
	// Parses and evaluates the text with selected arguments (without returning the parsed expression, only immediate one-time evaluation)
	//public object ParseEval(string text, int from, object? args = null);
	public string ToString(object value, int decimals = -1, bool pure = false, int type = 0);
	public void SetDarkMode(bool dark);
	public void SetPreEvaluate(bool preEval);
	public void SetDecimals(int decimals);
	public string ParsePeek();
	public (Color b, Color f) GetColor();
	public (Color e, Color s) GetErrorSuccessColor();
	public IPlot GetPlot();
	public double AsDouble(object? e);
	public List<(Color color, string log)> ReadCode(string text, CancellationToken cancel, out (int position, Color color)[] colors);
}

public /*abstract*/ partial class Comparser/*<T>*/ : IComparser /*where T : unmanaged, IScalar<T>*/ {


	public Comparser(
		bool caseInsensitive = true, 
		bool operatorLess = true,
		ushort stackOverflowLimit = 499, 
		ushort doOverflowLimit = 499, 
		ushort loopOverflowLimit = 499, 
		ushort iteratorOverflowLimit = 499,
		bool allowParsePeek = true
	) {
		_stackOverflowDef = stackOverflowLimit;
		_doOverflowDef = doOverflowLimit;
		_loopOverflowDef = loopOverflowLimit;
		_iterOverflowDef = iteratorOverflowLimit;
		_stackOverflow = stackOverflowLimit;
		_doOverflow = doOverflowLimit;
		_loopOverflow = loopOverflowLimit; 
		_iterOverflow = iteratorOverflowLimit;
		_caseInsensitiveDef = caseInsensitive;
		_allowParsePeek = allowParsePeek;
		_operatorLessDef = operatorLess;
		_operatorLess = operatorLess;
		Plotter = new(this);
	}

	private Reader? _currentReader;

	#region Interface
	public object MakeArgs((string alias, object value)[] pairs) => new Value([.. pairs.Select(p => new Value((ILeaf)p.value, 0, p.alias))]);
	public object MakeArgs(string[] names) => new Value(names.Select(p => new Value(Real.nan, 0, p)).ToArray());
	private static Value AsValue(object? e) => e as Value ?? new();
	public double AsDouble(object? e) =>  e switch { Value v => v.Re(), double d => d, int i => i, _ => 0 };
	public object Parse(CancellationToken cancel, string text, int from, out (int position, Color color)[] colors, object? args = null) {
		var read = new Reader(this, text, cancel, from);
		var e = new Expression(read, out _, AsValue(args), from);
		colors = read.GetColors(); // export colors
		return e;
	}
	public object Eval(object exp, object? args, bool allowCache) => exp is Expression e ? e.Eval(0, AsValue(args), allowCache) : None;
	//public object ParseEval(string text, int from, out object expr, object? args) { var e = (Expression)Parse(text, from, args); expr = e; return e.Eval(0, AsInput(args)); }
	public object ParseEval(CancellationToken cancel, string text, int from, out object expr, out (int position, Color color)[] colors, object? args, bool allowCache) {
		var read = new Reader(this, text, cancel, from);
		var e = new Expression(read, out _, AsValue(args), from);
		expr = e; // export expression
		colors = read.GetColors(); // export colors
		return e.Eval(0, AsValue(args), allowCache);
	}
	//public object ParseEval(string text, ref int from, object? args) => new Expression(this, text, ref from, out _, AsInput(args)).Eval(0, AsInput(args));
	//public object ParseEval(string text, int from, object? args) => ParseEval(text, ref from, args);
	public string ToString(object value, int decimals = int.MinValue, bool pure = false, int type = 0) 
		=> AsValue(value).ToString(decimals == int.MinValue ? Decimals : decimals, pure, type);
	public void SetDarkMode(bool dark) => _darkMode = dark;
	public void SetPreEvaluate(bool preEval) => PreEvaluate = preEval;
	public void SetDecimals(int decimals) => Decimals = decimals;
	public IPlot GetPlot() => Plotter;
	public string ParsePeek() => _allowParsePeek ? _currentReader == null ? "No current reader." : _currentReader.Text[_currentReader.From..] : "Parse Peek disabled.";
	public List<(Color, string)> ReadCode(string text, CancellationToken cancel, out (int, Color)[] colors) {
		// reset settings to default
		_operatorLess = _operatorLessDef;
		_doOverflow = _doOverflowDef;
		_iterOverflow = _iterOverflowDef;
		_loopOverflow = _loopOverflowDef;
		_stackOverflow = _stackOverflowDef;
		_caseInsensitive = _caseInsensitiveDef;
		// clear the previous build
		Context.Clear();
		// put defaults back in
		FillDefault(); // collects the default functions and constants into the Dictionary
		Dictionary<string, List<(Value input, Expression def, Expression? cond)>> parsedF = [];
		// temporary dictionary for parsed constants (will be put into a better dictionary that can parse characters as it eats)
		Dictionary<string, Value> parsedC = [];
		var brackets = 0; // start as not being inside any block yet
		var reader = _currentReader = new(this, text, cancel); // initiate reader
		List<(int i, int c)> back = [];
		string e;
		List<(Color, string)> log = [];
		UserFunctions.Clear();
		ReadLines("", reader);
		if (brackets > 0)
			log.Add((Color.Red, "Missing " + brackets + "x END BRACKET and eof. Not fatal but probably wrong."));
		colors = reader.GetColors();
		return log;

		void ReadLines(string pref, Reader read) {
			while (read.From < read.Text.Length) {
				if (cancel.IsCancellationRequested)
					return;
				// (<cacheIntExpression>)functionName(<argumentsExpression>:<defaultArgumentExpression>)=<definitionExpression>
				// functionName(<argument1Expression>,<argument2Expression>):<conditionExpression>?<ifExpression>:<elseExpression>
				// constantName:<constantExpression>
				// actionName:<actionArgumentExpression>

				int beforeI = 0, cache = 1;// skipStart = read.From;
				Value eval;
				if (read.TrimStart(1))
					return;
				switch (read.nextChar) {
				case '/': // Comment
					if (!read.IsComment())
						Cl(FailReason.Unexpected);
					break;
				case '}': // this must be and out of a branch I'm in, just ignore it
					EndBracket();
					break;
				case ';': // separator
				case '\t': // space
					++read.From; // eat it
					break;
				case ':':
				case '{': // these are unexpected when finding them out of the blue, they are always searched for and eaten with GotoFirstFailed instead, when legit
					Cl(FailReason.Unexpected);
					break;
				default:
					var args = None;
					read.TrimStart();
					beforeI = read.From;
					var name = "";
					int f, s;
					bool action = true, doContinue = false;
					if ("" != (e = LoadDef())) {
						if (s > 1) --read.From; // if we got trigger by a separator, go back to keep it for the error forward
						Lg();
						Cl(FailReason.Unexpected);
						break;
					}
					if (args.Values.Length == 0) { // it is a constant or code call:

						if (cache != 1) {
							e = name + " is a constant/call and doesn't support caching. Not fatal but doesn't do anything.";
							Lg();
						}
						var actionCode = name switch { // f(x) : "returned"+"string", 1+1 
							"print" => Actions.Print, // prints the expression, and it's value with its type.								f(x) = "returnedString", 2
							"Print" => Actions.Print,
							"printvalue" => Actions.PrintValue, // prints just the value, without type (number > string > expression).		returnedString, 2
							"PrintValue" => Actions.PrintValue,
							"printnumber" => Actions.PrintNumber, // prints just the numerical value, without fallback to string values.	NaN + NaNi, 2
							"PrintNumber" => Actions.PrintNumber, 
							"printstring" => Actions.PrintString, // prints only the string value, even if there's a numeric value			returnedString, '1+1'
							"PrintString" => Actions.PrintString, 
							"do" => Actions.Do,
							"Do" => Actions.Do,
							"include" => Actions.Include,
							"Include" => Actions.Include,
							"if" => Actions.If,
							"If" => Actions.If,
							"while" => Actions.While,
							"While" => Actions.While,
							"return" => Actions.Return,
							"Return" => Actions.Return,
							"break" => Actions.Break,
							"Break" => Actions.Break,
							"continue" => Actions.Continue,
							"Continue" => Actions.Continue,
							"stackoverflow" => Actions.StackOverflow,
							"StackOverflow" => Actions.StackOverflow,
							"iteratoroverflow" => Actions.IterOverflow,
							"IteratorOverflow" => Actions.IterOverflow,
							"whileoverflow" => Actions.WhileOverflow,
							"WhileOverflow" => Actions.WhileOverflow,
							"dooverflow" => Actions.DoOverflow,
							"DoOverflow" => Actions.DoOverflow,
							"operatorless" => Actions.Operator,
							"OperatorLess" => Actions.Operator,
							"casesensitive" => Actions.CaseSensitive,
							"CaseSensitive" => Actions.CaseSensitive,
							_ => Actions.None
						};
						read.AddC(beforeI, beforeI + name.Length, actionCode == Actions.None ? ParseDictionary.Type.UserC : ParseDictionary.Type.Action);
						ReadExpression();
						switch (actionCode) {
						case Actions.CaseSensitive:
							_caseInsensitive = eval.GetLeaf().IsFalse();
							break;
						case Actions.Print:
							log.Add((GetColor(ParseDictionary.Type.Text), ToString(eval, Decimals)));
							Cl(FailReason.Success);
							break;
						case Actions.PrintValue:
							log.Add((GetColor(ParseDictionary.Type.Text), ToString(eval, Decimals, true)));
							Cl(eval.Error); //Cl((eval.Error & 2) > 0 ? FailReason.StackOverflow : (eval.Error & 4) > 0 ? FailReason.BadExpression : FailReason.Success);
							break;
						case Actions.PrintNumber:
							log.Add((GetColor(ParseDictionary.Type.Text), ToString(eval, Decimals, true, 1)));
							Cl(eval.Error);
							break;
						case Actions.PrintString:
							log.Add((GetColor(ParseDictionary.Type.Text), ToString(eval, Decimals, true, 2)));
							Cl(FailReason.Success);
							break;
						case Actions.Do:
							if (pref.Length > _doOverflow) {
								e = "DO overflow limit exceeded.";
								Lg();
							} else ExpandCode(eval.ToLines());
							Cl(FailReason.Success);
							break;
						case Actions.Include:
							if (pref.Length > _doOverflow) {
								e = "INCLUDE overflow limit exceeded.";
								Lg();
							} else {
								var file = eval.GetString();
								if (!File.Exists(file)) {
									e = "Failed to include a file: " + file;
									Lg();
								} else ExpandCode(File.ReadAllText(file));
							}
							Cl(FailReason.Success);
							break;
						case Actions.If:
							Conditional((-1, 0));
							break;
						case Actions.While:
							var b = GetBack();
							Conditional(b.i < 0 ? (beforeI, 0) : (beforeI, b.c));
							break;
						case Actions.Return:
							EndLoop(false);
							break;
						case Actions.Break:
							EndLoop();
							break;
						case Actions.Continue:
							EndLoop(true, false);
							break;
						case Actions.StackOverflow:
							_stackOverflow = (ushort)(eval.Re());
							break;
						case Actions.IterOverflow:
							_iterOverflow = (ushort)(eval.Re());
							break;
						case Actions.WhileOverflow:
							_loopOverflow = (ushort)(eval.Re());
							break;
						case Actions.DoOverflow:
							_doOverflow = (ushort)(eval.Re());
							break;
						case Actions.Operator:
							_operatorLess = eval.GetLeaf().IsTrue();
							break;
						default:
							action = false;
							break;

							void EndLoop(bool onlyLoops = true, bool dontContinue = true) {
								for (var loops = (int)(eval.Re()); loops > 0;) {
									if (cancel.IsCancellationRequested)
										return;
									int startSkip = read.From;
									if (0 > brackets-- || read.GotoFirstFailed([], 1, out _, out _, false, 1)) {
										e = "Couldn't find an END bracket to escape from.";
										FailEnd();
									}
									read.AddC(startSkip, read.From, ParseDictionary.Type.Skip);
									if (onlyLoops && back[brackets].i < 0)
										continue; // not a loop, don't count that
									if (loops == 1) {
										--read.From; // go back to the end bracket
										if (dontContinue)
											back[brackets] = (-1, 0);
										++brackets;
										doContinue = true;
										return;
									}
									back[brackets] = (-1, 0);
									--loops;
								}
								Cl(FailReason.Unexpected);
							}
							void ExpandCode(string expand) {
								var newRead = _currentReader = new(this, expand, cancel);
								ReadLines(pref + (read.Line + 1) + "/", newRead);
								_currentReader = read;
								read.AppendC(newRead.Colors);
							}
						}
						if (doContinue)
							goto case '}';
						if (action)
							break;
						eval.Text = name;
						Cl(eval.Error); //Cl((eval.Error & 1) > 0 ? FailReason.StackOverflow : (eval.Error & 2) > 0? FailReason.BadExpression : FailReason.Success);
						if (parsedC.TryGetValue(name, out var exists)) {
							// mutate existing
							exists.Values = eval.Values;
							exists.Leaf = eval.Leaf;
							exists.String = eval.String;
							exists.Text = eval.Text;
							exists.Data = eval.Data;
						} else Context.Insert(new(parsedC[name] = eval, ParseDictionary.Type.UserC), name); // define new
						break;
					} // it is a function:
					var failFunc = FailReason.Success;
					read.AddC(beforeI, beforeI + name.Length, ParseDictionary.Type.UserF);
					LoadTernary();
					break;
					void LoadTernary() {
						List<(Expression, Expression)> conditionals = [];
						LoadFunc(out var defaultBranch);
						while (!read.GotoFirstFailed(0, 2, ['?'], 1, out _, out _)) {
							if (cancel.IsCancellationRequested)
								return;
							LoadFunc(out var trueExp);
							if (read.GotoFirstFailed(0, 2, [':'], 1, out _, out _)) {
								AddF(args, conditionals);
								return;
							}
							conditionals.Add((trueExp, defaultBranch));
							LoadFunc(out defaultBranch);
						}
						AddF(args, conditionals, defaultBranch); // no if
					}
					void ReadExpression() {
						//skipStart = read.From;
						read.TrimStart(1);
						var expression = new Expression(read, out _, args);
						eval = expression.Eval(0, None, false);
					}
					string LoadDef() {
						var stage = 0;
						var foundCantBeNext = Static.FindName;
						while (true) {
							int found;
							while (!read.GotoFirstFailed(0, 2, ['(', ':', '\n', ';'], 1, out s, out found, false, 0, false, stage > 0, foundCantBeNext)
								&& s == 2) ;
							switch (s) {
							case 0: // '('
								switch (++stage) {
								case 1: // name and args
									foundCantBeNext = Static.FindArgs;
									e = GetName(); // TODO parse as definition
									if (e != "") return e;
									if (!UserFunctions.ContainsKey(name)) // create the custom function if this is its first definition
										Context.Insert(new(UserFunctions[name] = new CallCustom([]), ParseDictionary.Type.UserF), name);
									if (FailArgs(out args)) return "Failed to parse ARGUMENTS.";
									break;
								case 2: // cache
									if ("" != (e = FailEvalClose(out eval) ? "Failed to parse CACHE size."
										: eval.Values.Length != 1 ? "Multiple values in the CACHE size expression: " + eval
										: eval.Values[0].Leaf.IsNaN() ? "CACHE size evaluated as NaN." : "")) { return e; }
									cache = (int)Math.Round(eval.Values[0].Leaf.Re());
									break;
								default:
									return "unexpected third parenthesis after cache size.";
								}
								break;
							case 1: //':'
								return (++stage == 1) ? GetName() : ""; // TODO parse as definition
							default: return Fc();
							}
							continue;
							string GetName() => TrimEnd(read.Uncomment(beforeI + (name = TrimEnd(read.Uncomment(beforeI, f = Math.Min(read.From - 1, found)), 1)).Length, f), 1).Length > 0
								? "Unexpected text after definition NAME." : name.Length == 0 ? "No definition name." : "";
						}
					}
					void IsFailed(Expression expression) {
						var v = CollapseScalar(expression.V);
						if (v.Values.Length != 0 || v.Term != null || !v.Leaf.IsNaN())
							return;
						read.AddC(beforeI, read.From, ParseDictionary.Type.Error);
						failFunc = FailReason.BadExpression;

					}
					void LoadFunc(out Expression expression) {
						read.TrimStart(1);
						beforeI = read.From;
						IsFailed(expression = new(read, out _, args, cache));
					}
					void AddF(Value input, List<(Expression ifTrue, Expression question)> conditional, Expression? dBranch = null) {
						if (parsedF.TryGetValue(name, out var pfn)) {
							for (var m = pfn.Count; 0 <= --m;)
								if (pfn[m].input.SameArg(input))
									pfn.RemoveAt(m); // already have the definition with the same arguments - mutate it
						} else pfn = parsedF[name] = [];
						foreach (var (ifTrue, question) in conditional)
							pfn.Add((args, ifTrue, question));
						if (dBranch != null)
							pfn.Add((args, dBranch, null));
						// create/update the compiled function:
						((CallCustom)UserFunctions[name]).Def = [.. parsedF[name]];
						Cl(failFunc);
					}
					(int i, int c) GetBack() => back.Count > brackets ? back[brackets] : (-1, -1);
					void Conditional((int i, int c) rb) {
						if (brackets < back.Count) back[brackets] = rb;
						else back.Add(rb);
						while (eval.Values.Length > 0)
							eval = eval.Values[0];
						if ((e = If())[0] == '_') { // "loop" during WHILE = stack overflow limit, "loop" during IF = skip :{} after ending a block
							switch (e[1]) {
							case 'T':
								++brackets;
								if (rb.i < 0) rb.c = 0;
								break;
							case 'W': rb.i = -1; break;
							case 'E': // enter else
								var il = rb.i >= 0;
								if (!il || rb.c == 0) ++brackets;
								if (il) rb.i = -1;
								goto case 'N';
							case 'N': rb.c = 1; break;
							}
						} else FailEnd();
						return;
						string If() {
							while (true) {
								var failCond = eval.Leaf.IsFalse();
								if (read.GotoFirstFailed(['{'], 1, out _, out _))
									return "Failed to find a START BRACKET.";
								if (!failCond)
									return "_T"; // enter true block
								//back[brackets]
								//read.AddC(skipStart, read.From-1, ParseDictionary.Type.Skip);
								if (rb.c > 0)
									return SkipElses(1);
								back[brackets] = (-1, -1); // failed condition should disable while goback, if it's set up
								if ("" != (e = SkipBlock()))
									return e;
								if (read.GotoFirstFailed(0,2,[':'], 1, out _, out _))
									return "_N"; // No else block
								//skipStart = read.From;
								ReadExpression(); // read else condition
							}
						}
					}
				}
				continue;
				void FailEnd() {
					Lg();
					Cl(FailReason.Success);
				}
				string SkipBlock(bool color = true) {
					int skipStart = read.From;
					if (read.GotoFirstFailed([], 1, out _, out _, false, 1)) // skip this block
						return "Failed to find an END BRACKET after skipping a failed condition";
					if(color)
						read.AddC(skipStart - 1, read.From, ParseDictionary.Type.Skip);
					return "";
				}
				string SkipElses(int dontSkipCol = 0) {
					int skipStart = read.From;
					do {
						bool dont = --dontSkipCol < 0;
						if ("" != (e = cancel.IsCancellationRequested ? "Cancelled" : SkipBlock(dont))) return e;
						if(!dont)
							skipStart = read.From;
					} while ("" == (e = read.GotoFirstFailed(0,2,[':'], 1, out _, out _) ? "_N" 
						: read.GotoFirstFailed(['{'], 1, out _, out _, false, 0, false, false, [false]) 
							? "Failed to find a START BRACKET." : "" ));
					if(--dontSkipCol < 0)
						read.AddC(skipStart, read.From, ParseDictionary.Type.Skip);
					return e;
				}
				bool FailEval(out Value evaluated, Expression.ParseAs parseAs = Expression.ParseAs.Expression) {
					var expression = new Expression(read, out _, None, 0, 0, parseAs);
					evaluated = UnCollapseScalar(expression.Eval(0, None, false));
					return false;
				}
				bool FailEvalClose(out Value evaluated, Expression.ParseAs parseAs = Expression.ParseAs.Expression) 
					=> FailEval(out evaluated, parseAs) || read.GotoFirstFailed(0, 2,[')'], 1, out _, out _);
				void EndBracket() {
					if (--brackets < 0) {
						Cl(FailReason.Unexpected); // closing a bracket that wasn't started
						return;
					}
					var (i, c) = back[brackets];
					var (bi, bc) = (i, c + 1);
					if (bi >= 0) {
						// at the end of while
						if (bc <= _loopOverflow) {
							// repeat
							read.From = bi; // goto */
							back[brackets] = (bi, bc); // increment stack overflow counter
							return;
						}
						//fail exit
						back[brackets] = (-1, 1);
						e = "WHILE loop limit exceeded.";
						Lg();
					}
					++read.From; // eat end bracket
					if (back[brackets].c > 0) {
						// no else expected
						back[brackets] = (bi, 0);
						return;
					}
					if (read.GotoFirstFailed(0,2, [':'], 1, out _, out _))
						return;
					beforeI = read.From;
					if ("_N" != (e = read.GotoFirstFailed(0, 2, ['{'], 1, out _, out _, false, 0, false, false, [false])
						? "Failed to find a START BRACKET after ELSE." : SkipElses()))
						FailEnd();
					else read.AddC(beforeI - 1, read.From, ParseDictionary.Type.Skip);
				}
				void Cl(FailReason reason, bool fromError = false) {
					read.TrimStart();
					var before = read.From;
					var fail = read.GotoFirstFailed(['}', ';', '\n'], 0, out var s, out var found) /* && i < codeL.Length*/;
					if (s == 0) { // move back to the bracket and process it:
						--read.From;
						EndBracket(); 
					}
					var initialBefore = beforeI;
					if (reason != FailReason.Success || fail && /*i*/before < read.Text.Length) {
						e = Static.Errors[(byte)reason]
							/*reason switch { FailReason.Unexpected => "Unexpected text: ", FailReason.BadExpression => "Bad expression: ",
								FailReason.StackOverflow => "Stack overflow: ", _ => "?" }*/
							+ read.Text[Math.Max(read.From - 12, 0)..read.From].Replace("\n", "") + "|" + read.Text[read.From..Math.Min(read.From + 12, read.Text.Length)].Replace("\n", ""); 
						GotoNext();
						read.AddC(reason != FailReason.Success ? initialBefore : beforeI, read.From, ParseDictionary.Type.Error);
						Lg();
					} else {
						GotoNext();
						if(fromError)
							read.AddC(beforeI, read.From, ParseDictionary.Type.Error);
					}
					read.TrimStart(2);

					return;
					void GotoNext() {
						if (!fail)
							return;
						if (reason <= FailReason.Unexpected)
							beforeI = before;
						if (found != int.MaxValue) {
							read.From = found;
							return;
						}
						read.From = read.Text.Length; // not found separator, put it at the oef. 
					}
				}
				void Lg() {
					read.From = Math.Min(read.From, read.Text.Length);
					// TODO prepare tooltip pointers
					log.Add((Color.Red, "Line " + pref + (read.From > 0 && read.Text[read.From - 1] == '\n' ? read.Line - 1 : read.Line) + ": " + e));
					e = "";
				}
				string Fc() => "Missing definition colon.";
				bool FailArgs(out Value args) => FailEvalClose(out args, Expression.ParseAs.Argument) || FailArgValues(args.Values); // || FailArgTerms(args.Values);
				static bool IsAlphaNumeric(string strToCheck) => MyRegex().IsMatch(strToCheck);
				bool FailArgValues(Value[] v) {
					if (v.Length == 0) return true;
					var fail = false;
					foreach (var iv in v)
						fail |= iv.Values.Length > 0
							? FailArgValues(iv.Values)
							: iv.Leaf.IsNaN() && (iv.String == "" || !IsAlphaNumeric(iv.String));
					return fail;
				}
			}
		}
	}
	#endregion
	
	#region Generic Interface
	//public Expression T_Parse(string text, int from, Value? args = null) => new(this, text, args ?? None, from);
	//public Value T_Eval(Expression exp, Value? args = null) => exp.Eval(0, args ?? None);
	//public Value T_ParseEval(string text, int from, out Expression expr, Value? args = null) { var e = T_Parse(text, from, args); expr = e; return e.Eval(0, args ?? None); }
	//public Value T_ParseEval(string text, ref int from, Value? args = null) => new Expression(this, text, ref from, out _, args ?? None).Eval(0, args ?? None);
	//public Value T_ParseEval(string text, int from, Value? args = null) => T_ParseEval(text, ref from, args);

	#endregion

	#region Enums
	public enum Actions : byte {
		None = 0,
		Print = 1,
		PrintValue = 2,
		PrintNumber = 3,
		PrintString = 4,
		Do = 5,
		Include = 6,
		If = 7,
		While = 8,
		Return = 9,
		Break = 10,
		Continue = 11,
		StackOverflow = 12, 
		IterOverflow = 13,
		WhileOverflow = 14,
		DoOverflow = 15,
		Operator = 16,
		CaseSensitive = 17
	}
	[Flags] public enum FailReason : byte {
		Success = 0,
		NaN = 1,
		StackOverflow = 2,
		BadExpression = 3,
		Unexpected = 4
	}
	#endregion
	
	#region Content
	private bool _darkMode = true;
	public int Decimals = 3;
	public bool PreEvaluate = true;
	public Plot Plotter;
	
	private Color GetColor(ParseDictionary.Type type) => _darkMode ? _darkColors[type] : _lightColors[type];

	public (Color b, Color f) GetColor() =>_darkMode ?  (_darkColors[ParseDictionary.Type.Back], _darkColors[ParseDictionary.Type.Fore]) 
		: (_lightColors[ParseDictionary.Type.Back], _lightColors[ParseDictionary.Type.Fore]);
	public (Color e, Color s) GetErrorSuccessColor() => _darkMode ?  (_darkColors[ParseDictionary.Type.Error], _darkColors[ParseDictionary.Type.Success]) 
		: (_lightColors[ParseDictionary.Type.Error], _lightColors[ParseDictionary.Type.Success])  ;
	
	private readonly Dictionary<ParseDictionary.Type, Color> _darkColors = new() {
		[ParseDictionary.Type.Action] = Color.FromArgb(255, 255, 0),
		[ParseDictionary.Type.UserF] = Color.FromArgb(128, 128, 255),
		[ParseDictionary.Type.DefaultF] = Color.FromArgb(64,64,255),
		[ParseDictionary.Type.PointerF] = Color.FromArgb(128,0,255),
		[ParseDictionary.Type.Arg] = Color.FromArgb(192, 96, 0),
		[ParseDictionary.Type.UserC] = Color.FromArgb(0,255,48),
		[ParseDictionary.Type.DefaultC] = Color.FromArgb(0,160,0),
		[ParseDictionary.Type.Number] = Color.FromArgb(192, 0, 192),
		[ParseDictionary.Type.Text] = Color.White,
		[ParseDictionary.Type.Comment] =  Color.FromArgb(64,64,64),
		[ParseDictionary.Type.String] =  Color.FromArgb(255, 0, 255),
		[ParseDictionary.Type.Skip] =  Color.FromArgb(64, 0, 0),
		[ParseDictionary.Type.Error] = Color.Red,
		[ParseDictionary.Type.Success] = Color.Green,
		[ParseDictionary.Type.Back] = Color.Black,
		[ParseDictionary.Type.Fore] = Color.White
	};
	private readonly Dictionary<ParseDictionary.Type, Color> _lightColors = new() {
		[ParseDictionary.Type.Action] =  Color.FromArgb(192, 192, 0),
		[ParseDictionary.Type.UserF] = Color.Blue,
		[ParseDictionary.Type.DefaultF] = Color.FromArgb(0,0,192),
		[ParseDictionary.Type.PointerF] = Color.FromArgb(64,0,192),
		[ParseDictionary.Type.Arg] = Color.FromArgb(160, 80, 0),
		[ParseDictionary.Type.UserC] = Color.FromArgb(0,192,32),
		[ParseDictionary.Type.DefaultC] =Color.FromArgb(0,128,0),
		[ParseDictionary.Type.Number] = Color.FromArgb(128, 0, 128),
		[ParseDictionary.Type.Text] = Color.Black,
		[ParseDictionary.Type.Comment] = Color.FromArgb(192,192,192),
		[ParseDictionary.Type.String] = Color.Purple,
		[ParseDictionary.Type.Skip] =  Color.FromArgb(96, 0, 0),
		[ParseDictionary.Type.Error] = Color.FromArgb(192,0, 0),
		[ParseDictionary.Type.Success] = Color.FromArgb(0,128,0),
		[ParseDictionary.Type.Back] = Color.White,
		[ParseDictionary.Type.Fore] = Color.Black
	};
	
	public class ParseDictionary(ParseDictionary? parent = null, int depth = 0) {
		public struct S(object obj, Type type) {
			public readonly object Obj = obj;
			public readonly Type Type = type;
		}
		public enum Type : byte {
			//NameSpace = 1 << 0,	// obj = Package pkg, with constants and functions | bitmap 
			Action = 1 << 0,		// obj = (byte)CodeCall callIndex | print, do...
			UserF = 1 << 1,			// obj = (Dictionary<CallFunction> )
			DefaultF = 1 << 2,		// obj = (Dictionary<CallFunction> )
			Arg = 1 << 3,			// obj = (Value container, int index) index | arguments
			UserC = 1 << 4,			// obj = (Value value) | arguments, user constants, default constants and generic constants
			DefaultC = 1 << 5,		// obj = (Value value) | arguments, user constants, default constants and generic constants
			Number = (1 << 5) + 1,	// not dictionary, just for parsing colors - direct numbers
			Text = (1 << 5) + 2,	// not dictionary, just for parsing colors - generic code text
			Comment = (1 << 5) + 3,	// not dictionary, just for parsing colors - comment
			String = (1 << 5) + 4,	// not dictionary, just for parsing colors - "string"
			PointerF = (1 << 5) + 5,// not dictionary, just for parsing colors - function pointer
			Skip = (1 << 5) + 6,	// not dictionary, just for parsing colors - skipped blocks/coditions
			Error = (1 << 5) + 7,	// not dictionary, just for parsing colors - error
			Success = (1 << 5) + 8,	// not dictionary, just for parsing colors - success
			Back = (1 << 5) + 9,	// not dictionary, just for parsing colors - background
			Fore = (1 << 5) + 10	// not dictionary, just for parsing colors - non-code text
		}
		private readonly List<S> _d = [];
		private readonly Dictionary<char, ParseDictionary> _next = [];
		private byte _deeper; // what TypeS are present deeper into the _next
		public void Clear() { _next.Clear(); _deeper = 0; }
		/// <summary>
		/// adds an entry keyword to the dictionary
		/// </summary>
		/// <param name="d">data()</param>
		/// <param name="name">keyword</param>
		/// <returns></returns>
		public S Insert(S d, string name) => I(d, name);
		private S I(S d, string name, int c = 0) {
			if (c >= name.Length) { 
				_d.Add(d);
				return d;
			}
			_deeper |= (byte)d.Type; // remember this type is deeper down
			(_next.TryGetValue(name[c], out var n) ? n : _next[name[c]] = new(this, 1 + depth)).I(d, name,1 + c);
			return d;
		}
		/// <summary>
		/// search for matching keywords at the beginning of this text
		/// </summary>
		/// <param name="text">string to search through</param>
		/// <param name="from">index location to search from</param>
		/// <param name="types">types I'm interested in (will stop going deeper, if _children don't contain anymore)</param>
		/// <returns>all found entries sorted from largest</returns>
		public List<(string name, S obj)> Get(string text, int from, byte types) {
			var nest = this;
			var c = 0;
			List<(string name, S obj)> r = [];
			while ((nest = nest.GetNext(text, from, types, out var name, out var addList, ref c)) != null) 
				foreach (var a in addList)
					if (((byte)a.Type & types) > 0)
						r.Add((name, a));
			return r;
		}
		// gets the longest matching list of names first, then if none of them fit.
		// If asked again with the same nest and c, it will return it's smaller parent, and until all possible matches have been depleted
		private ParseDictionary? GetNext(string text, int from, byte types, out string name, out List<S> d, ref int c) {
			if (c < 0) {
				(name,d) = parent == null ? ("",[]) : (text[from..(from + depth - 1)], parent._d);
				return parent;
			}
			if ((types & _deeper) > 0 && text.Length > c + from && _next.TryGetValue(text[from + c], out var n)) {
				++c;
				return n.GetNext(text, from,  types, out name, out d, ref c);
			}
			(c,name, d) = (-1, text[..depth], _d);
			return this;
		}
	}

	private readonly ushort _stackOverflowDef, _doOverflowDef, _loopOverflowDef, _iterOverflowDef;
	private ushort _stackOverflow, _doOverflow, _loopOverflow, _iterOverflow;
	private readonly bool _caseInsensitiveDef, _allowParsePeek, _operatorLessDef;
	private bool _operatorLess, _caseInsensitive;
	public static readonly Value None = new();
	private static readonly Value StackOverflow = new(FailReason.StackOverflow);
	private static readonly Cf OpFact = new(Factorial, OpCode.Factorial);
	private static readonly Cf OpSqr = new(ILeaf.Sqr, OpCode.Sqr);
	private static readonly Cf OpConj = new(ILeaf.Conj, OpCode.Conj);
	private static readonly Ce OpCount = new(typeof(FuncCount), 0);
	private static readonly Ce OpCatCount = new(typeof(FuncCatCount), 0);
	private static readonly Cf OpAbs = new(T_Abs, OpCode.Abs);
	private static readonly Cf OpSqrAbs = new(T_SqrAbs, OpCode.SqrAbs);
	private static readonly Cf OpCompAbs = new(AbsComp, OpCode.Absri);
	private static readonly Cf OpSign = new(Sign, OpCode.Sgn);
	//protected Value GenericConstants();
	protected readonly ParseDictionary Context = new();
	public readonly Dictionary<string, CallFunction> UserFunctions = [];
	public readonly Dictionary<string, CallFunction> DefaultFunctions = [];
	#endregion
	
	#region Helpers
	private static Real/*<T>*/ nan => Real/*<T>*/.nan;
	private static Real/*<T>*/ unit => Real/*<T>*/.unit;
	private static Real/*<T>*/ zero => Real/*<T>*/.zero;
	private static Real/*<T>*/ one => Real/*<T>*/.one;
	public class Reader(Comparser/*<T>*/ context, string text, CancellationToken cancel, int from = 0) {
		public char nextChar => From < Text.Length ? Text[From] : ';';
		public string remainingString => From < Text.Length ? Text[From..] : "";
		public readonly Comparser/*<T>*/ Context = context;
		public int From = from, Line = 1;
		public readonly CancellationToken Cancel = cancel;
		public readonly string Text = context._caseInsensitive ? text.ToLower() : text;
		public readonly List<(int position, ParseDictionary.Type color)> Colors = [(0, ParseDictionary.Type.Text)];
		private void GetChar(char c, out int location, int? i = null, string? dtext = null) {
			int f = i ?? From;
			string d = dtext ?? Text;
			if (f >= d.Length || (location = d.IndexOf(c, f)) < 0) location = int.MaxValue;
		}
		public bool CharAtRel(char c, int offset) => (offset += From) < Text.Length && Text[offset] == c;
		public bool CharAtAbs(char c, int offset, string? text = null) => offset < (text ??= Text).Length && text[offset] == c;
		public (int position, Color color)[] GetColors() => Colors.Select(p => (p.position, this.Context.GetColor(p.color))).ToArray();
		public void AddC(int start, int end, ParseDictionary.Type colorStart) {
			if (end <= start)
				return; // no range, don't do anything
			int ip = Colors.Count;
			var returnColor = ParseDictionary.Type.Text; // default return color
			// find the first key that is before our start and should not be overriden
			while (--ip >= 0) {
				returnColor = Colors[ip].color;
				if (Colors[ip].position < start) break;
			}
			// insert out start key after that.
			Colors.Insert(++ip, (start, colorStart));
			// remove following colors that are before the end of out selection, collecting the last color change to replace the end position with
			for (++ip; ip < Colors.Count && Colors[ip].position < end; Colors.RemoveAt(ip))
				returnColor = Colors[ip].color;
			// and place out new ending key there
			Colors.Insert(ip,(end, returnColor));
		}
		public void AppendC(List<(int, ParseDictionary.Type)> append) {
			foreach (var c in append)
				Colors.Add(c);
		}
		public bool IsComment(int separators = 0) => CharAtRel('*', 1) && !GotoFirstFailed([], separators, out _, out _, true);
		public bool TrimStart(int separators = 0/*, bool real = true*/) {
			while (Text.Length > From)
				switch (Text[From]) {
				case '/':
					if (IsComment(separators))//(CharAtRel('*', 1) && !GotoFirstFailed([], separators, out _, out _, true, 0,false,true,null, real)) 
						break;
					goto default;
				case ' ':
				case '\t':
				case '\r': 
					++From; 
					break;
				case '\n':
					if (separators <= 0) 
						goto default; 
					++From;
					++Line;
					break;
				case ';':
					if (separators <= 1)
						goto default;
					++From;
					break;
				default: return From >= Text.Length;
				}
			return From >= Text.Length;
		}
		// real = 0: do not advance from, line and colors
		// real = 1: only advance from and line, not colors
		// real = 2: advance everything
		public bool GotoFirstFailed(int offset, int real, char[] c, int separators, out int s, out int found, bool comment = false, int skip = 0, bool str = false, bool otherMustBeNext = true, bool[]? foundCantBeNext = null) {
			int bi = From, bl = Line;
			TrimStart(separators/*, real < 2*/);
			found = int.MaxValue;
			s = -1;
			
			var failed= Text.Length <= (From += offset) || GotoFirstFailed(c, separators, out s, out found,  comment,  skip ,  str, otherMustBeNext, foundCantBeNext/*, real > 0*/);
			if (real >= 1 && !failed)
				return failed;
			From = bi;
			Line = bl;
			return failed;
		}
		/// <summary>
		/// advances one character beyond any of the target characters, counting comments and lines in the process
		/// </summary>
		/// <param name="c">target characters</param>
		/// <param name="separators">0-newlines and semicolons not allowed, 1-semicolons not allowed, 2-newlines not allowed</param>
		/// <param name="s">if we have successfully found one of the target characters, this will say which one</param>
		/// <param name="found">location where we found (even unsuccessfully) the target character</param>
		/// <param name="comment">set to true if you have just detected /* and want to advance to the end of that comment (assuming you have NOT YET advanced past the start of that comment)</param>
		/// <param name="skip">set to 1 if you have just detected a start bracket that you want to skip the whole block (assuming you are already starting past that bracket)</param>
		/// <param name="str">set to true if you have just detected a string and wat to advance to its end. Assuming you are already past that starting quote mark</param>
		/// <param name="otherMustBeNext">will any non-ignored characters (whitespace, comments, and optionally separators) be allowed before our target character?</param>
		/// <param name="foundCantBeNext">which of the target characters are not allowed to have non-ignored character before them?</param>
		/// <returns></returns>
		public bool GotoFirstFailed(char[] c, int separators, out int s, out int found, bool comment = false, int skip = 0, bool str = false, bool otherMustBeNext = true, bool[]? foundCantBeNext = null/*, bool real = true*/) {
			bool wasComment = comment, wasSkip = skip > 0, wasString = str;
			found = int.MaxValue;
			s = -1;
			int startComment;
			if (comment) {
				startComment = From;
				From += 2; // found dash and star, starting a comment, eat that first dash and star
			} else  startComment = -1;
			var r = Perform(ref s, ref found);
			if (startComment >= 0)
				AddC(startComment, From = Text.Length, ParseDictionary.Type.Comment);
			
			return r;
			bool Perform(ref int s, ref int found) {
				var searches = new int[c.Length];
				if (TrimStart(separators))
					return true;
				int search = int.MaxValue;
				//do {
				GetChar('{', out var braStart);
				GetChar('}', out var braEnd);
				GetChar('/', out var commDash);
				GetChar('"', out var strMark);
				GetChar('\n', out var ln);
				for (s = 0; s < c.Length; ++s) {
					GetChar(c[s], out searches[s]);
					search = Math.Min(search, searches[s]);
				}
				found = search;
				int next;
				s = -1;
				while (int.MaxValue != (next = Math.Min(strMark, Math.Min(Math.Min(braStart, braEnd), Math.Min(commDash, search))))) {
					if (Cancel.IsCancellationRequested)
						return true;
					search = int.MaxValue;
					for (s = 0; s < c.Length; search = Math.Min(search, searches[s++])) {
						found = searches[s]; 
						if (Text[next] != c[s])
							continue;
						if (Text[next] == '\n')
							++Line;
						var isNotNext = next != From && Next(s);
						Eat();
						GetChar(c[s], out searches[s]);
						if (!comment && skip <= 0) {
							if (!isNotNext)// found it as the next non-white space character or not?
								return false;
							s = -1;
							return true;
						}
						s = int.MaxValue;
						break;
						bool Next(int s) => foundCantBeNext == null || s >= foundCantBeNext.Length || foundCantBeNext[s];
					}
					if (s == int.MaxValue)
						continue;
					s = -1;
					// skip comment strings and braces:
					switch (Text[next]) {
					case '"': // string
						if (otherMustBeNext && Text[From] != '"' && skip == 0 && !comment && !str)
							return true;
						Eat();
						GetChar('"', out strMark);
						if (comment)
							break; // inside a comment, doesn't count
						if (!str && skip == 0)
							return true;
						if (str) {
							if (Text[next - 1] != '\\') {
								str = false;
								if (wasString)
									return false;// found end of string
							}
						} else str = !str;
						break;
					case '/': // comment
						if (otherMustBeNext && Text[From] != '/' && skip == 0 && !comment && !str)
							return true;
						bool endComment = next > 0 && Text[next - 1] == '*', newComment = next < Text.Length - 1 && Text[next + 1] == '*';
						Eat();
						GetChar('/', out commDash);
						if (str)
							break; // inside a string, doesn't count
						var prev = comment;
						if (!(comment = newComment || comment && !endComment)) {
							if (prev) AddC(startComment, From, ParseDictionary.Type.Comment);
								startComment = -1;
							if (wasComment)
								return false; // found the end of the comment
						} else if (prev != comment) {
							startComment = next;
						} else if (prev == comment && !comment && skip == 0 && (!newComment || endComment))
							return true; // not skipping anything and it's not a beginning of a comment

						break;
					case '{':
						if (otherMustBeNext && Text[From] != '{' && skip == 0 && !comment && !str)
							return true;
						Eat();
						GetChar('{', out braStart);
						if (comment || str)
							break; // inside comment or string, doesn't count
						if (skip == 0)
							return true; // not skipping, what is this doing there?
						++skip;
						break;
					case '}':
						if (otherMustBeNext && Text[From] != '}' && skip == 0 && !comment && !str)
							return true;
						Eat();
						GetChar('}', out braEnd);
						if (comment || str)
							break; // inside comment or string, doesn't count
						if (skip == 0)
							return true; // not skipping, what is this doing there?
						if (--skip == 0 && wasSkip)
							return false; // finish skip block
						break;
					default: // can this even happen?
						return true; // found something wrong
					}
				}
				return true; // didn't find it

				void Eat() {
					if (ln < next) 
						++Line;
					From = next + 1;
					TrimStart(separators/*, real*/);
				}
			}
		}
		public string Uncomment(int from, int to) {
			int lo = 0, hi = Colors.Count;
			//var mSy = _mSy2 - yC;
			while (lo < hi) {
				var mid = lo + hi >> 1;
				int d = Colors[mid].position;
				if (d == from || hi == 1 + lo) {
					lo = mid; break; }
				if (d < from) lo = mid; // target is closer to d stepped towards hi
				else hi = mid; // target is closer to d stepped away from hi
			}
			var comment = Colors[Math.Min(lo, Colors.Count-1)].color == ParseDictionary.Type.Comment;
			var end = Math.Min(Text.Length, to);
			var s = "";
			while (from < end && ++lo < Colors.Count) {
				var t = Colors[lo].position;
				if (!comment && t > from) s += lo >= Colors.Count ? Text[from..] : Text[from..Math.Min(t, end)];
				comment = Colors[lo].color == ParseDictionary.Type.Comment;
				from = t;
			}
			if (from < to && !comment) s += Text[from..end];
			return s;
		}
	}
	
	private static string TrimEnd(string txt, int separators = 0) {
		GetCharAbs(' ', out var s, txt);
		GetCharAbs('\r', out var r, txt);
		GetCharAbs('\t', out var t,txt);
		int l;
		if (separators == 1) GetCharAbs('\n', out l, txt);
		else l = int.MaxValue;
		var min = Math.Min(Math.Min(s, l),Math.Min(r,t));
		return min < int.MaxValue ? txt[..min] : txt;
	}
	private static void GetCharAbs(char c, out int location, string txt = "") {
		if ((location = /*(txt.Length == 0 ? codeL : txt)*/txt.IndexOf(c)) < 0) location = int.MaxValue;
	}
	private static Value CollapseScalar(Value i) {
		while (i.Values.Length == 1 /*&& !i.Parentheses*/)
			i = i.Values[0];
		return i;
	}
	private static Value UnCollapseScalar(Value i) {
		if (i.Values.Length == 0)
			i.Values = [new(i.Leaf, i.Error, i.String) {Operand = i.Operand}];
		return i;
	}
	[GeneratedRegex(@"^[a-zA-Z0-9\s,]*$")]
	private static partial Regex MyRegex();
	#endregion

	private void FillDefault() {
		
		C(["i", "I"], Complex.i); // complex imaginary unit
		C(["j", "J"], Quaternion.j); // quaternionic imaginary unit
		C(["k", "K"], Quaternion.k); // quaternionic imaginary unit
		
		
		C(["∞", "infty", "Infty", "INFTY", "infinity", "Infinity", "INFINITY"], (Real)double.PositiveInfinity); // infinity
		C(["π", "pi", "Pi", "PI"], (Real)Math.PI); // half rotation
		C(["τ", "tau", "Tau", "TAU"], (Real)Math.Tau); // full rotation
		C(["e", "E"], (Real)Math.E); // euler number
		C(["φ", "phi", "Phi", "PHI"],(Real)Static.Phi); // golden ratio
		C(["γ", "gamma", "Gamma", "GAMMA"], (Real)Static.Gamma); // euler constant
		C(["ln2", "LN2", "Ln2"],(Real)Static.Ln2); // ln(2)
		C(["ln10", "LN10", "Ln10"],(Real)Static.Ln10); // ln(10)
		C(["sqrt2", "SQRT2", "Sqrt2", "√2"], (Real)Static.Sqrt2); // sqrt(2)
		C(["sqrt3", "SQRT3", "Sqrt3", "√3"], (Real)Static.Sqrt3); // sqrt(3)
		C(["sqrt5", "SQRT5", "Sqrt5", "√5"], (Real)Static.Sqrt5); // sqrt(5)
		C(["apery", "APERY", "Apery", "ζ3"],(Real)Static.Apery); // Apery's constant
		C(["δ", "feigenbaum", "Feigenbaum", "FEIGENBAUM"],(Real)Static.Feigenbaum); // Feigenbaum constant
		C(["g", "G", "catalan", "Catalan", "CATALAN"], (Real)Static.Catalan); // Catalan's constant
		//foreach(var d in GenericConstants().Values) C([d.String], d.Leaf);
		
		//CallFunction min, max, mul, sum, prod, vec, ln, nsinhc, nsinc, re, im, neg, inv, compMod, cub, trunc, sinhc, ceil;
		// meta
		A(["eval", "Eval"], new Ce(typeof(FuncEval), 0)); // attempts to parse and evaluate every Text in the input
		A(["count", "Count"], OpCount); // counts the number of elements in the vector
		A(["catcount", "CatCount", "Catcount", "totalcount", "TotalCount", "Totalcount"], OpCatCount); // counts the total number of elements in the vector
		A(["cat", "Cat", "concat", "Concat", "concatenate", "Concatenate"], new Ce(typeof(FuncCat), 0)); // Un-nests the vectors: concat((1,2),3,((4,5),6)) = (1,2,3,4,5,6)

		// double arguments:
		A(["min", "Min", "minimum", "Minimum"], new Cf2(Min, OpCode.Min)); // component-wise minimum
		A(["max", "Max", "maximum", "Maximum"], new Cf2(Max, OpCode.Max)); // component-wise maximum
		A(["softmax", "SoftMax", "Softmax", "sftmax", "SftMax", "Sftmax"], new Cf2(SoftMax, OpCode.SoftMax));
		A(["softmin", "SoftMin", "Softmin", "sftmin", "SftMin", "Sftmin"], new Cf2(SoftMin, OpCode.SoftMin));
		A(["add","Add"], new Cf2(ILeaf.Add, OpCode.Add)); // adds all the top layer elements of the input vector
		A(["mul", "Mul", "multiply", "Multiply"], new Cf2(ILeaf.Mul, OpCode.Mul)); // multiplies all the top layer elements of the input vector
		A(["icoef", "Icoef", "ICoef", "imagcoef", "ImagCoef", "Imagcoef"], new Cf2((x, y) => Mul(Neg(x), y), OpCode.ImCoef)); // = re(-a*b), imaginary coefficient icoef(a+bi,i)=b, icoef(r+ai+bj+ck,j)=b, icoef(a+bi,1)=-a
		A(["compmod", "CompMod", "Compmod", "cmod", "CMod", "Cmod"], new Cf2(CompMod, OpCode.CompMod)); // component-wise remainder, returns 0 when dividing by zero
		A(["expb", "ExpB", "Expb"], new Cf2(ExpB, OpCode.ExpB)); // b^x
		A(["logb", "LogB", "Logb"], new Cf2(LogB, OpCode.LogB)); // log_b(x)
		A(["softabsb", "SoftAbsb", "Softabsb", "sftabsb", "SftAbsb", "Sftabsb", "softplusb", "SoftPlusb", "Softplusb", "sftplusb", "SftPlusb", "Sftplusb"], new Cf2(SoftAbsB, OpCode.SoftAbsB)); // = e^(1+ln(z))
		A(["softnegb", "SoftNegb", "Softnegb", "sftnegb", "SftNegb", "Sftnegb", "softminusb", "SoftMinusb", "Softminusb", "sftminusb", "SftMinusb", "Sftminusb"], new Cf2(SoftNegB, OpCode.SoftNegB)); // = e^(1+ln(z))
		A(["softclamp01b", "SoftClamp01B", "softclampbnorm", "SoftClampBNorm", "Softclampbnorm"], new Cf2(SoftClamp01B, OpCode.SoftClamp01B)); // component-wise clamp

		// triple arguments
		A(["clamp", "Clamp"], new Cf3(/*T*/Clamp, OpCode.Clamp)); // component-wise clamp
		A(["softclamp", "SoftClamp", "Softclamp","sftclamp", "SftClamp", "Sftclamp" ], new Cf3(SoftClamp, OpCode.SoftClamp)); // natural soft clamp
		A(["softmaxb", "SoftMaxB", "Softmaxb", "sftmaxb", "SftMaxB", "Sftmaxb"], new Cf3(SoftMaxB, OpCode.SoftMaxB));
		A(["softminb", "SoftMinB", "Softminb", "sftminb", "SftMinB", "Sftminb"], new Cf3(SoftMinB, OpCode.SoftMinB));
		
		// quadruple arguments
		A(["product", "Product", "prod", "Prod", "Π"], new Ce(typeof(Product), 0)); // iterative product
		A(["sum", "Sum", "Σ"], new Ce(typeof(Sum), 0)); // iterative sum
		A(["vector", "Vector", "vec", "Vec"], new Ce(typeof(Vector), 0)); // iterative vector builder
		A(["softclampb", "SoftClampB", "Softclampb", "sftclampb", "SftClampB", "Sftclampb"], new Cf4(SoftClampB, OpCode.SoftClampB)); // soft clamp with a custom base
		
		// exp/log
		A(["exp10", "Exp10", "expdec", "ExpDec", "Expdec"], new Cf(Exp10, OpCode.Exp10)); // 10^x
		A(["exp2", "Exp2", "expbin", "ExpBin", "Expbin"], new Cf(Exp2, OpCode.Exp2)); // 2^x
		A(["exp", "Exp", "exponential", "Exponential"], new Cf(/*T*/Exp, OpCode.Exp)); // e^x
		A(["log10", "Log10", "logdec", "LogDec", "Logdec"], new Cf(Log10, OpCode.Log10)); // log_10(x)
		A(["log2", "Log2", "logbin", "LogBin", "Logbin"], new Cf(Log2, OpCode.Log2)); // log_2(x)
		A(["ln", "log", "Ln", "Log", "logarithm", "Logarithm"], new Cf(/*T*/Log, OpCode.Log)); // ln(x)

		// sincs
		A(["sinhc", "sinhc", "Sinch", "Sinch"], new Cf(Sinhc, OpCode.Sinhc));
		A(["nsinhc", "Nsinhc", "nsinch", "Nsinch", "sinchpi", "SinchPi", "Sinchpi", "sinhcpi", "SinhcPi", "Sinhcpi"], new Cf(Nsinhc, OpCode.Nsinhc));
		A(["sinc", "Sinc"], new Cf(Sinc, OpCode.Sinc));
		A(["nsinc", "Nsinc", "sincpi", "SincPi", "Sincpi"], new Cf(Nsinc, OpCode.Nsinc));

		// coscs
		A(["coshc", "coshc", "Cosch", "Cosch"], new Cf(Coshc, OpCode.Coshc));
		A(["ncoshc", "Ncoshc", "ncosch", "Ncosch", "coschpi", "CoschPi", "Coschpi", "coshcpi", "CoshcPi", "Coscpi"], new Cf(Ncoshc, OpCode.Ncoshc));
		A(["cosc", "Cosc"], new Cf(Cosc, OpCode.Cosc));
		A(["ncosc", "Ncosc", "coscpi", "CoscPi", "Coscpi"], new Cf(Ncosc, OpCode.Ncosc));

		// arc hyperbolics
		A(["acosh", "Acosh", "arccosh", "ArcCosh", "Arccosh"], new Cf(/*T*/Acosh, OpCode.Acosh));
		A(["asinh", "Asinh", "arcsinh", "ArcSinh", "Arcsinh"], new Cf(/*T*/Asinh, OpCode.Asinh));
		A(["atanh", "Atanh", "arctanh", "ArcTanh", "Arctanh"], new Cf(/*T*/Atanh, OpCode.Atanh));
		A(["asech", "Asech", "arcsech", "ArcSech", "Arcsech"], new Cf(Asech, OpCode.Asech));
		A(["acsch", "Acsch", "arccsch", "ArcCsch", "Arccsch"], new Cf(Acsch, OpCode.Acsch));
		A(["acoth", "Acoth", "arccoth", "ArcCoth", "Arccoth"], new Cf(/*T*/Acoth, OpCode.Acoth));

		// hyperbolics
		A(["cosh", "Cosh"], new Cf(/*T*/Cosh, OpCode.Cosh));
		A(["sinh", "Sinh"], new Cf(/*T*/Sinh, OpCode.Sinh));
		A(["tanh", "Tanh"], new Cf(/*T*/Tanh, OpCode.Tanh));
		A(["sech", "Sech"], new Cf(Sech, OpCode.Sech));
		A(["csch", "Csch"], new Cf(Csch, OpCode.Csch));
		A(["coth", "Coth"], new Cf(/*T*/Coth, OpCode.Coth));

		// arc trigs
		A(["acos", "Acos", "arccos", "ArcCos", "Arccos"], new Cf(/*T*/Acos, OpCode.Acos));
		A(["asin", "Asin", "arcsin", "ArcSin", "Arcsin"], new Cf(/*T*/Asin, OpCode.Asin));
		A(["atan", "Atan", "arctan", "ArcTan", "Arctan"], new Cf(/*T*/Atan, OpCode.Atan));
		A(["asec", "Asec", "arcsec", "ArcSec", "Arcsec"], new Cf(Asec, OpCode.Asec));
		A(["acsc", "Acsc", "arccsc", "ArcCsc", "Arccsc"], new Cf(Acsc, OpCode.Acsc));
		A(["acot", "Acot", "arccot", "ArcCot", "Arccot"], new Cf(/*T*/Acot, OpCode.Acot));

		// trigs
		A(["cos", "Cos"], new Cf(/*T*/Cos, OpCode.Cos));
		A(["sin", "Sin"], new Cf(/*T*/Sin, OpCode.Sin));
		A(["tan", "Tan"], new Cf(/*T*/Tan, OpCode.Tan));
		A(["sec", "Sec"], new Cf(Sec, OpCode.Sec));
		A(["csc", "Csc"], new Cf(Csc, OpCode.Csc));
		A(["cot", "Cot"], new Cf(/*T*/Cot, OpCode.Cot));

		// unary
		A(["true", "True"], new Cf((x) => /*T*/(Real)(SqrAbs(x) >= 1 ? 1 : 0), OpCode.True)); // = size >= 1
		A(["false", "False"], new Cf((x) => /*T*/(Real)(SqrAbs(x) < 1 ? 1 : 0), OpCode.False)); // = size < 1
		A(["real", "Real", "re", "Real"], new Cf(T_Re, OpCode.Re)); // real part: re(a+bi) = a
		A(["imag", "Imag", "im", "Im"], new Cf(T_I, OpCode.Im)); // imaginary sum: im(r+ai+bj+ck) = a+b+c
		A(["immg", "Immg", "ImMg", "immag", "ImMag", "Immag"], new Cf(ImMag, OpCode.ImMag)); // imaginary magnitude immg(r+ai+bj+ck) = sqrt(a^2+b^2+c^2)
		A(["frac", "Frac"], new Cf(/*T*/Frac, OpCode.Frac)); // = fractional part
		A(["trunc", "Trunc", "truncate", "Truncate"], new Cf(/*T*/Truncate, OpCode.Trunc)); // = whole part
		A(["floor", "Floor"], new Cf(/*T*/Floor, OpCode.Floor)); // = round down
		A(["round", "Round", "rnd", "Rnd"], new Cf(/*T*/Round, OpCode.Round)); // = round
		A(["ceiling", "Ceiling", "ceil", "Ceil"], new Cf(/*T*/Ceiling, OpCode.Ceil)); // = round up
		A(["cyc", "Cyc", "cycle", "Cycle", "lmod", "Lmod", "pfrac", "Pfrac"], new Cf(/*T*/Cycle, OpCode.Cycle)); // = positive frac cycle. cyc(1.25)=0.25, cyc(-.75)=0.25. Also equals x-floor(x)
		A(["clamp01", "Clamp01", "clampnorm", "ClampNorm", "Clampnorm"], new Cf(Clamp01, OpCode.Clamp01)); // component-wise clamp
		A(["softclamp01", "SoftClamp01", "softclampnorm", "SoftClampNorm", "Softclampnorm"], new Cf(SoftClamp01, OpCode.Clamp01)); // component-wise clamp
		A(["sign", "Sign", "sgn", "Sgn"],OpSign); // = z/|z|
		A(["neg", "Neg", "negative", "Negative"], new Cf(Neg, OpCode.Neg)); // = -z
		A(["inv","Inv", "inverse", "Inverse"], new Cf(/*T*/Inv, OpCode.Inv)); // = 1/z
		A(["compabs", "CompAbs", "Compabs", "cabs", "CAbs", "Cabs"], OpCompAbs); // component-abs: absri(a+bi) = |a|+|b|i
		A(["sqrabs", "SqrAbs", "Sqrabs", "sqrnorm", "SqrNorm", "Sqrnorm"], OpSqrAbs); // = |z|^2; sqrabs(a+bi) = a^2+b^2
		A(["abs", "Abs", "absolute", "Absolute", "norm", "Norm"], OpAbs); // = |z|
		A(["arg", "Arg", "argument", "Argument", "phase", "Phase", "angle", "Angle"], new Cf(T_Arg, OpCode.Arg)); // argument, the angle from (0,0). arg(-1)=pi
		A(["conj","Conj","conjugate","Conjugate"],OpConj);// conjugate: negates all imaginary units, conj(r+ai+bj+dk) = r-ai-bj-bk
		A(["softabs", "SoftAbs", "Softabs", "sftabs", "SftAbs", "Sftabs", "softplus", "SoftPlus", "Softplus", "sftplus", "SftPlus", "Sftplus"], new Cf(SoftAbs, OpCode.SoftAbs)); // = e^(1+ln(z))
		A(["softneg", "SoftNeg", "Softneg", "sftneg", "SftNeg", "Sftneg", "softminus", "SoftMinus", "Softminus", "sftminus", "SftMinus", "Sftminus"], new Cf(SoftNeg, OpCode.SoftNeg)); // = e^(1+ln(z))

		// powers
		A(["√", "sqrt", "Sqrt", "squareroot", "SquareRoot","Squareroot"], new Cf(/*T*/Sqrt, OpCode.Sqrt)); // square root = z^(1/2)
		A(["sqr", "Sqr", "square", "Square"], OpSqr); // square = z^2
		A(["cbrt", "Cbrt", "cuberoot", "CubeRoot", "Cuberoot"], new Cf(Cbrt, OpCode.Cbrt)); // cube root = z^(1/3)
		A(["cube", "Cube", "cub", "Cub"], new Cf(/*T*/Cub, OpCode.Cub)); // cube = z^3
		A(["quart", "hypercube", "HyperCube", "Hypercube", "tesseract", "Tesseract"], new Cf(/*T*/Quart, OpCode.Quart)); // z^4

		// colors
		A(["rgb2hsv", "Rgb2hsv", "Rgb2Hsv", "rgbtohsv", "RgbToHsv", "Rgbtohsv"], new Ce(typeof(FuncRgb2Hsv)));
		A(["hsv2rgb", "Hsv2rgb", "Hsv2Rgb", "hsvtorgb", "HsvToRgb", "Hsvtorgb"], new Ce(typeof(FuncHsv2Rgb)));
		A(["log2hsv", "Log2hsv", "Log2Hsv", "logtohsv", "LogToHsv", "Logtohsv"], new Ce(typeof(FuncLog2Hsv)));
		A(["lin2hsv", "Lin2hsv", "Lin2Hsv", "lintohsv", "LinToHsv", "Lintohsv"], new Ce(typeof(FuncLin2Hsv)));
		A(["log2hsvc", "Log2hsv", "Log2HsvC", "logtohsvc", "LogToHsvC", "Logtohsvc"], new Ce(typeof(FuncLog2HsvC)));
		A(["lin2hsvc", "Lin2hsv", "Lin2HsvC", "lintohsvc", "LinToHsvC", "Lintohsvc"], new Ce(typeof(FuncLin2HsvC)));
		A(["log2rgb", "Log2rgb", "Log2Rgb", "logtorgb", "LogToRgb", "Logtorgb"], new Ce(typeof(FuncLog2Rgb)));
		A(["lin2rgb", "Lin2rgb", "Lin2Rgb", "lintorgb", "LinToRgb", "Lintorgb"], new Ce(typeof(FuncLin2Rgb)));
		A(["log2rgbc", "Log2rgbc", "Log2RgbC", "logtorgbc", "LogToRgbC", "Logtorgbc"], new Ce(typeof(FuncLog2RgbC)));
		A(["lin2rgbc", "Lin2rgbc", "Lin2RgbC", "lintorgbc", "LinToRgbC", "Lintorgbc"], new Ce(typeof(FuncLin2RgbC)));

		// specials
		A(["fact", "Fact", "factorial", "Factorial"], OpFact); // factorial
		A(["gauss", "Gauss"], new Cf(Gauss, OpCode.Gauss)); // gauss e^(-z^2)
		A(["Γ", "gamma", "Gamma"], new Cf(Gamma, OpCode.Gamma)); // gamma function = (xz1)!
		A(["gamma"], new Cf(Gamma, OpCode.Gamma)); // gamma function = (xz1)!
		A(["ζ", "zeta", "Zeta", "riemannzeta","RiemannZeta", "Riemannzeta"], new Cf(/*T*/Zeta, OpCode.Zeta)); // riemann zeta function
		return;
		void C(string[] name, ILeaf/*<T>*/ v) {
			foreach (var n in name)if(!_caseInsensitive || n.Equals(n, StringComparison.CurrentCultureIgnoreCase))
				Context.Insert(new(new Value(v, 0, n), ParseDictionary.Type.DefaultC), n);
		}
		void A(string[] name, CallFunction c) {
			foreach (var n in name) if(!_caseInsensitive || n.Equals(n, StringComparison.CurrentCultureIgnoreCase))
				Context.Insert(new(DefaultFunctions[n] = c, ParseDictionary.Type.DefaultF), n);
		}
	}
}
/*public class ComparserR : Comparser<Real> { override protected Value GenericConstants() => None; }
public class ComparserC : Comparser<Complex> { override protected Value GenericConstants() => new([new(Complex.i, 0, "i")]); }
public class ComparserQ : Comparser<Quaternion> { override protected Value GenericConstants() => new([new(Quaternion.i, 0, "i"), new(Quaternion.j, 0, "j"), new(Quaternion.k, 0, "k")]); }*/