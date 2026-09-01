using Comparser.Comparser.Numbers;
using System.Text.RegularExpressions;
namespace Comparser.Comparser;
public interface IComparser {
	//public object Parse(string text, int from, object? args = null);
	// Re-evaluates already parsed expression with new arguments
	public object Eval(object exp, object? args = null);
	// Parses and evaluates the text with selected arguments (and returns the expression for possible re-evaluation)
	//public object ParseEval(string text, int from, out object expr, object? args = null);
	public object ParseEval(CancellationToken cancel, string text, int from, out object expr, out List<(int position, Color color)> colors, object? args = null);
	// Parses and evaluates the text with selected arguments (and returns the expression for possible re-evaluation, and eats the parsed part, allows it to be incomplete - leaving the remainder in the ref text)
	//public object ParseEval(string text, ref int from, object? args = null);
	// Parses and evaluates the text with selected arguments (without returning the parsed expression, only immediate one-time evaluation)
	//public object ParseEval(string text, int from, object? args = null);
	public string ToString(object value, int decimals = -1, bool pure = false);
	public void SetDarkMode(bool dark);
	public void SetDecimals(int decimals);
	public (Color b, Color f) GetColor();
	public (Color e, Color s) GetErrorSuccessColor();
	public List<(Color color, string log)> ReadCode(string text, CancellationToken cancel, out List<(int position, Color color)> colors);
}

public abstract partial class Comparser<T>(bool caseInsensitive = true, ushort stackOverflowLimit = 499, ushort doOverflowLimit = 499, ushort loopLimit = 499, ushort iteratorLimit = 499) : IComparser where T : unmanaged, INumber<T> {
	
	#region Interface
	private static Value AsInput(object? e) => e as Value ?? new();
	//public object Parse(string text, int from, object? args) => new Expression(this, text, AsInput(args), from);
	public object Eval(object exp, object? args) => exp is Expression e ? e.Eval(0, AsInput(args)) : None;
	//public object ParseEval(string text, int from, out object expr, object? args) { var e = (Expression)Parse(text, from, args); expr = e; return e.Eval(0, AsInput(args)); }
	public object ParseEval(CancellationToken cancel, string text, int from, out object expr, out List<(int position, Color color)> colors, object? args) {
		var read = new Reader(this, text, cancel, from);
		var e = new Expression(read, out _, AsInput(args), from);
		expr = e; // export expression
		colors = read.Colors; // export colors
		return e.Eval(0, AsInput(args));
	}
	//public object ParseEval(string text, ref int from, object? args) => new Expression(this, text, ref from, out _, AsInput(args)).Eval(0, AsInput(args));
	//public object ParseEval(string text, int from, object? args) => ParseEval(text, ref from, args);
	public string ToString(object value, int decimals = -1, bool pure = false) => AsInput(value).ToString(decimals, pure);
	public void SetDarkMode(bool dark) => _darkMode = dark;
	public void SetDecimals(int decimals) => _decimals = decimals;
	public List<(Color, string)> ReadCode(string text, CancellationToken cancel, out List<(int, Color)> colors) {

		Context.Clear();
		FillDefault(); // collects the default functions and constants into the Dictionary
		Dictionary<string, List<(Value input, Expression def, Expression? cond)>> parsedF = [];
		Dictionary<string, Value> parsedC = [];
		var brackets = 0;
		var reader = new Reader(this, _caseInsensitive ? text.ToLower() : text, cancel);
		List<(int i, int c)> back = [];
		string e;
		List<(Color, string)> log = [];
		Dictionary<string, CallFunction> userFunctions = [];
		ReadLines("", reader);
		if (brackets > 0)
			log.Add((Color.Red, "Missing " + brackets + "x END BRACKET and eof. Not fatal but probably wrong."));
		colors = reader.Colors;
		return log;

		//string CleanWhite(string cl) => cl.Replace(" ", "").Replace("\t", "").Replace("\r", "");
		void ReadLines(string pref, Reader read) {
			//_context.CustomFunctions = new Comparser.CallFunction[CustomFunctions.Count];
			while (read.From < read.Text.Length) {
				if (cancel.IsCancellationRequested)
					return;
				// (<cacheIntExpression>)functionName(<argumentsExpression>:<defaultArgumentExpression>)=<definitionExpression>
				// functionName(<argument1Expression>,<argument2Expression>):<conditionExpression>?<ifExpression>:<elseExpression>
				// constantName:<constantExpression>
				// commandName:<commandArgumentExpression>

				int beforeI = 0, cache = 1;
				Value eval;
				if (read.TrimStart(1))
					return;
				switch (read.nextChar) {

				// TODO must account for whitespace everywhere!

				case '/': // Comment
					if (!read.IsComment())
						Cl(FailReason.Unexpected);
					break;
				case '}': // this must be and out of a branch I'm in, just ignore it
					if (--brackets < 0) {
						Cl(FailReason.Unexpected); // closing a bracket that wasn't started
						break;
					}
					var bb = back[brackets];
					var (bi, bc) = (bb.i, bb.c + 1);
					if (bi >= 0) {
						// at the end of while
						if (bc <= _loop) {
							// repeat
							read.From = bi; // goto */
							back[brackets] = (bi, bc); // increment stack overflow counter
							break;
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
						break;
					}
					if (read.GotoFirstFailed([':'], 1, out _, out _))
						break;
					beforeI = read.From;
					if ("_N" != (e = read.GotoFirstFailed(0, ['{'], 1, out _, out _) ? "Failed to find a START BRACKET after ELSE." : SkipElses())) { FailEnd(); }
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
					Value args = None;
					read.TrimStart();
					beforeI = read.From;
					string name = "";
					Commands commandCode = Commands.None;
					int f;
					bool command = true, doContinue = false;
					// debug:
					/*var bbb = i;
					if (read.GotoFirstFailed(['(', ':', '\n', ';'], out s, out var found, false, 0, false, false, [false, false, true, true]) || s > 1) {
						e = "";
					} else if (TrimEnd(codeL[(beforeI + (name = TrimEnd(codeL[beforeI..(i + found)])).Length)..(i + found)]).Length > 0) {
						e = "";
					} else if (IsFunc()) {
						if (FailArgs(out args)) {
							e = "";
						}else if (read.GotoFirstFailed([':'], out _, out _)) {
							e = "";
						}
					}
					i = bbb;*/
					int s;
					if ("" != (e = LoadDef())) {
						if (s > 1) --read.From; // if we got trigger by a separator, go back to keep it for the error forward
						Err( /*true*/);
						break;
					}
					/*if ("" != (e =  ? "No definition COLON found on this line."
						:  ? "Unexpected text after definition NAME."
						: name.Length == 0 ? "No definition name." : !IsAlphaNumeric(name) ? "Failed to parse name: " + name
						: IsFunc() ? FailArgs(out args) ? "Failed to parse ARGUMENTS." : FuncEndFailed() : "")) {
						if (s > 1) --read.From; // if we got trigger by a separator, go back to keep it for the error forward
						Err();
						break;
					}*/
					if (args.Values.Length == 0) { // it is a constant or code call:
						
						if (cache != 1) {
							e = name + " is a constant/call and doesn't support caching. Not fatal but doesn't do anything.";
							Lg();
						}
						read.AddC(beforeI, beforeI + name.Length,commandCode == Commands.None ? ParseDictionary.Type.UserC : ParseDictionary.Type.Action);
						//read.AddC(beforeI + name.Length, read.From,ParseDictionary.Type.Text);
						read.TrimStart(1);
						var expression = new Expression(read, out _, args);
						//CollectColors(expression);
						eval = expression.Eval(0, None);
						TryCommand();
						if (doContinue)
							goto case '}';
						if (command)
							break;
						eval.String = name;
						Cl(eval.GetLeaf().IsNaN() ? FailReason.BadExpression : FailReason.Success);
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
					read.AddC(beforeI, beforeI + name.Length,ParseDictionary.Type.UserF);
					if (!userFunctions.ContainsKey(name)) // create the custom function if this is its first definition
						Context.Insert(new(userFunctions[name] = new CallCustom([]), ParseDictionary.Type.UserF), name);
					LoadTernary();
					break;
					void LoadTernary() {
						List<(Expression, Expression)> conditionals = [];
						LoadFunc(out var defaultBranch);
						while (!read.GotoFirstFailed(0,['?'], 1, out _, out _)) {
							if (cancel.IsCancellationRequested)
								return;
							LoadFunc(out var trueExp);
							if (read.GotoFirstFailed(0,[':'], 1, out _, out _)) {
								AddF(args, conditionals);
								return;
							}
							conditionals.Add((trueExp, defaultBranch));
							LoadFunc(out defaultBranch);
						}
						AddF(args, conditionals, defaultBranch); // no if
					}
					string LoadDef() {
						int found;
						while (!read.GotoFirstFailed(0, ['(', ':', '\n', ';'], 1, out s, out found, false, 0, false, false, [false, false, false /*true*/, true]) && s == 2) ;
						if (s < 0 || s > 1)
							return "No definition COLON found on this line.";
						//if (TrimEnd(read.Text[(beforeI + (name = TrimEnd(read.Text[beforeI..(f = Math.Min(read.From - 1, found))])).Length)..f]).Length > 0)
						//	return "Unexpected text after definition NAME.";
						//if (TrimEnd(read.Uncomment(beforeI + (name = TrimEnd(read.Text[beforeI..(f = Math.Min(read.From - 1, found))])).Length)..f]).Length > 0)
						name = TrimEnd(read.Uncomment(beforeI, f = Math.Min(read.From - 1, found)), 1);
						//read.From = beforeI + name.Length;
						return TrimEnd(read.Uncomment(beforeI + name.Length, f), 1).Length > 0 ? "Unexpected text after definition NAME."
							: name.Length == 0 ? "No definition name."
							: IsFunc() ? FailArgs(out args) ? "Failed to parse ARGUMENTS." : FuncEndFailed() : "";
					}
					string FuncEndFailed() {
						while (!read.GotoFirstFailed(0, ['(', ':', '\n', ';'], 1, out s, out _, false, 0, false, true, [true, true, false /*true*/, true]) && s == 2) ;
						if (s < 0 || s > 1)
							return "No definition COLON found on this line.";
						switch (s) {
							case 0:
								// cache
								if ("" != (e = FailEvalClose(out eval) ? "Failed to parse CACHE size."
									: eval.Values.Length != 1 ? "Multiple values in the CACHE size expression: " + eval
									: eval.Values[0].Leaf.IsNaN() ? "CACHE size evaluated as NaN." : "")) {
									//ColorError();
									//FailEnd();
									return e;
								}
								cache = (int)Math.Round(T.Re(eval.Values[0].Leaf));
								return e;
							case 1: return "";
							default: return Fc();
						}
					}
					void TryCommand() {
						switch (commandCode) {
						case Commands.Print:
							log.Add((GetColor(ParseDictionary.Type.Text), ToString(eval, _decimals)));
							Cl(FailReason.Success);
							break;
						case Commands.PrintValue:
							log.Add((GetColor(ParseDictionary.Type.Text), ToString(eval, _decimals, true)));
							Cl(eval.GetLeaf().IsNaN() ? FailReason.BadExpression : FailReason.Success);
							break;
						case Commands.PrintString:
							log.Add((GetColor(ParseDictionary.Type.Text), ToString(eval, _decimals, true)));
							Cl(FailReason.Success);
							break;
						case Commands.Do:
							if (pref.Length > _doOverflow) {
								e = "DO overflow limit exceeded.";
								Lg();
							} else {
								var expand = eval.ToLines(); // ToString(true) recursively exports only string values as lines
								var newRead = new Reader(this, expand, cancel);
								ReadLines(pref + (read.Line + 1) + "/", newRead);
								read.AppendC(newRead.Colors);
								
							}
							Cl(FailReason.Success);
							break;
						case Commands.If:
							Conditional((-1, 0));
							break;
						case Commands.While:
							var b = GetBack();
							Conditional(b.i < 0 ? (read.From, 0) : (read.From, b.c));
							break;
						case Commands.Return:
							EndLoop(false);
							break;
						case Commands.Break:
							EndLoop();
							break;
						case Commands.Continue:
							EndLoop(true, false);
							break;
						default:
							command = false;
							break;

							void EndLoop(bool onlyLoops = true, bool dontContinue = true) {
								for (var loops = (int)T.Re(eval.GetLeaf()); loops > 0;) {
									if (cancel.IsCancellationRequested)
										return;
									if (0 < brackets-- || read.GotoFirstFailed([], 1, out _, out _, false, 1)) {
										e = "Couldn't find an END bracket to escape from.";
										FailEnd();
									}
									if (onlyLoops && back[brackets].i < 0)
										continue; // not a loop, don't count that
									if (loops == 1) {
										--read.From; // go back to the end bracket
										++brackets;
										if (dontContinue)
											back[brackets] = (-1, 0);
										doContinue = true;
										return;
									}
									back[brackets] = (-1, 0);
									--loops;
								}
								Cl(FailReason.Unexpected);
							}
						}
					}
					bool IsFunc() {
						// processes the name and returns whether it is supposed to be a new function
						if (s == 0) {
							//read.AddC(beforeI, beforeI + name.Length,ParseDictionary.Type.UserF);
							return true;
						}
						commandCode = name switch {
							"print" => Commands.Print,
							"printvalue" => Commands.PrintValue,
							"printstring" => Commands.PrintString,
							"do" => Commands.Do,
							"return" => Commands.Return,
							"break" => Commands.Break,
							"continue" => Commands.Continue,
							_ => Commands.None
						};
						//read.AddC(beforeI, beforeI+name.Length, commandCode == Commands.None ? ParseDictionary.Type.UserC : ParseDictionary.Type.Do);
						return false;
					}
					void IsFailed(Expression expression) {
						var v = CollapseScalar(expression.V);
						if (v.Values.Length != 0 || v.Term != null || !v.Leaf.IsNaN())
							return;// CollectColors(expression);
						read.AddC(beforeI, read.From, ParseDictionary.Type.Error);
						failFunc = FailReason.BadExpression;
						
					}
					void LoadFunc(out Expression expression) {
						read.TrimStart(1);
						beforeI = read.From;
						IsFailed(expression = new(read, out _, args, cache));
					}
					void FailEnd() {
						Lg();
						Cl(FailReason.Success);
					}
					void AddF(Value input, List<(Expression ifTrue, Expression question)> conditional, Expression? dBranch = null) {
						if (parsedF.TryGetValue(name, out var pfn)) {
							for (var m = pfn.Count; 0 <= --m;)
								if (pfn[m].input.SameArg(input))
									pfn.RemoveAt(m); // already have the definition with the same arguments - mutate it
						} else pfn = parsedF[name] = [];
						foreach (var condI in conditional)
							pfn.Add((args, condI.ifTrue, condI.question));
						if (dBranch != null)
							pfn.Add((args, dBranch, null));
						// create/update the compiled function:
						((CallCustom)userFunctions[name]).Def = [.. parsedF[name]];
						Cl(failFunc);
					}
					void Err( /*bool unexpected = false*/) {
						//ColorError();
						Lg();
						Cl(0 <= s ? FailReason.Unexpected : FailReason.Success);
					}
					//codeLine = (nextChar = s.IndexOf(';')) >= 0 ? s[(nextChar + 1)..] + s1 : (nextChar = s1.IndexOf(';')) >= 0 ? s1[(nextChar + 1)..] : "";
					//}
					(int i, int c) GetBack() => back.Count > brackets ? back[brackets] : (-1, -1);
					void Conditional((int i, int c) rb) {
						if (brackets < back.Count) back[brackets] = rb;
						else back.Add(rb);
						/*if ("" != (e = FailEval(out eval) ? "Failed to parse CONDITION." : "")) {
							FailEnd();
							return;
						}*/
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
								var failCond = INumber<T>.IsFalse(eval.Leaf);
								if (read.GotoFirstFailed(['{'], 1, out _, out _))
									return "Failed to find a START BRACKET.";
								if (rb.c > 0)
									return SkipElses();
								if (!failCond)
									return "_T"; // enter true block
								if (read.GotoFirstFailed([], 1, out _, out _, false, 1)) // skip this block
									return "Failed to find an END BRACKET after skipping a failed condition";
								if (read.GotoFirstFailed([':'], 1, out _, out _))
									return "_N"; // No else block
								//if (read.GotoFirstFailed(['{'], 1, out _, out _))
								//	return "Failed to find a START BRACKET when trying to enter an ELSE branch.";
							}
						}
						
					}
					string SkipElses() {
						do {
							if (cancel.IsCancellationRequested)
								return "Cancelled";
							e = read.GotoFirstFailed([], 1, out _, out _, false, 1) ? "Failed to find an END BRACKET after WHILE ELSE." : "_W";
							if (e != "_W") return e;
						} while ("" == (e = read.GotoFirstFailed([':'], 1, out _, out _) ? "_N" : read.GotoFirstFailed(['{'], 1, out _, out _) ? "Failed to find a START BRACKET." : "" ));
						return e;
					}
				}
				continue;

				//void ColorError() => read.AddC(beforeI, ParseDictionary.Type.Error);
				//void ColorText(int at = -1) => read.AddC(0 <= at ? at : read.From, ParseDictionary.Type.Text);
				//void CollectColors(Expression expression) => CollectC(expression, col);
				//void AddColor(int start, ParseDictionary.Type t) => AddC(start, t, col);
				//string NoWhite(string s) => s.Replace(" ", "").Replace("\t", "").Replace("\r", "");
				bool FailEval(out Value evaluated, bool isArg = false) {
					//++i;//exp = exp[1..]; // eat initial symbol that triggered it
					var expression = new Expression(read, out _, None, 0, 0, isArg);
					//CollectColors(expression);
					evaluated = UnCollapseScalar(expression.Eval(0, None));
					return false;
				}
				bool FailEvalClose(out Value evaluated, bool isArg = false) {
					if (FailEval(out evaluated, isArg) || read.GotoFirstFailed(0, [')'], 1, out _, out _))
						return true; // closing parenthesis after the argument expression
					//++i;//exp = exp[1..];
					return false;
				}
				void Cl(FailReason reason,bool fromError = false) {
					read.TrimStart();
					var before = read.From;
					var fail = read.GotoFirstFailed(['}', ';', '\n'], 0, out _, out var found) /* && i < codeL.Length*/;
					var s = beforeI;
					if (reason != FailReason.Success || fail && /*i*/before < read.Text.Length) {
						e = reason switch { FailReason.Unexpected => "Unexpected text: ", FailReason.BadExpression => "Bad expression: ", _ => "?" }
							+ read.Text[Math.Max(read.From - 12, 0)..read.From].Replace("\n", "") + "|" + read.Text[read.From..Math.Min(read.From + 12, read.Text.Length)].Replace("\n", ""); 
						GotoNext();
						read.AddC(reason != FailReason.Success ? s : beforeI, read.From, ParseDictionary.Type.Error);
						Lg();
					} else {
						GotoNext();
						if(fromError)
							read.AddC(/*reason != FailReason.Success ? s : */beforeI, read.From, ParseDictionary.Type.Error);
					}
					read.TrimStart(2);
					return;
					void GotoNext() {
						if (!fail)
							return;
						if (reason != FailReason.BadExpression)
							beforeI = before;
						if (found != int.MaxValue) {
							//beforeI = i; // mark error from here, and towards +goto:
							//i = found + 1; // the first found separator or block end. 
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
				bool FailArgs(out Value args) => FailEvalClose(out args, true) || FailArgValues(args.Values); // || FailArgTerms(args.Values);
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
	//public Value MakeArgs((string alias, T value)[] pairs) => new(pairs.Select(p => new Value(p.value, 0, p.alias)).ToArray());
	#endregion

	#region Enums
	public enum Commands : byte {
		None = 0,
		Print = 1,
		PrintValue = 2,
		PrintString = 3,
		Do = 4,
		If = 5,
		While = 6,
		Return = 7,
		Break = 8,
		Continue = 9,
	}
	public enum FailReason : byte {
		Success = 0,
		Unexpected = 1,
		BadExpression = 2
	}
	#endregion
	
	#region Content
	private bool _darkMode = true;
	private int _decimals = 3;
	private Color GetColor(ParseDictionary.Type type) => _darkMode ? _darkColors[type] : _lightColors[type];

	public (Color b, Color f) GetColor() =>_darkMode ?  (_darkColors[ParseDictionary.Type.Back], _darkColors[ParseDictionary.Type.Fore]) 
		: (_lightColors[ParseDictionary.Type.Back], _lightColors[ParseDictionary.Type.Fore]);
	public (Color e, Color s) GetErrorSuccessColor() => _darkMode ?  (_darkColors[ParseDictionary.Type.Error], _darkColors[ParseDictionary.Type.Success]) 
		: (_lightColors[ParseDictionary.Type.Error], _lightColors[ParseDictionary.Type.Success])  ;
	
	private readonly Dictionary<ParseDictionary.Type, Color> _darkColors = new() {
		[ParseDictionary.Type.Action] = Color.FromArgb(255, 255, 0),
		[ParseDictionary.Type.UserF] = Color.FromArgb(128, 128, 255),
		[ParseDictionary.Type.DefaultF] = Color.FromArgb(64,64,255),
		[ParseDictionary.Type.Arg] = Color.FromArgb(192, 96, 0),
		[ParseDictionary.Type.UserC] = Color.FromArgb(0,255,48),
		[ParseDictionary.Type.DefaultC] = Color.FromArgb(0,160,0),
		[ParseDictionary.Type.Number] = Color.FromArgb(192, 0, 192),
		[ParseDictionary.Type.Text] = Color.White,
		[ParseDictionary.Type.Comment] =  Color.FromArgb(64,64,64),
		[ParseDictionary.Type.String] =  Color.FromArgb(255, 0, 255),
		[ParseDictionary.Type.Error] = Color.Red,
		[ParseDictionary.Type.Success] = Color.Green,
		[ParseDictionary.Type.Back] = Color.Black,
		[ParseDictionary.Type.Fore] = Color.White
	};
	private readonly Dictionary<ParseDictionary.Type, Color> _lightColors = new() {
		[ParseDictionary.Type.Action] =  Color.FromArgb(192, 192, 0),
		[ParseDictionary.Type.UserF] = Color.Blue,
		[ParseDictionary.Type.DefaultF] = Color.FromArgb(0,0,192),
		[ParseDictionary.Type.Arg] = Color.FromArgb(160, 80, 0),
		[ParseDictionary.Type.UserC] = Color.FromArgb(0,192,32),
		[ParseDictionary.Type.DefaultC] =Color.FromArgb(0,128,0),
		[ParseDictionary.Type.Number] = Color.FromArgb(128, 0, 128),
		[ParseDictionary.Type.Text] = Color.Black,
		[ParseDictionary.Type.Comment] = Color.FromArgb(192,192,192),
		[ParseDictionary.Type.String] = Color.Purple,
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
			Action = 1 << 0,			// obj = (byte)CodeCall callIndex | print, do...
			UserF = 1 << 1,			// obj = (Dictionary<CallFunction> )
			DefaultF = 1 << 2,		// obj = (Dictionary<CallFunction> )
			Arg = 1 << 3,			// obj = (Value container, int index) index | arguments
			UserC = 1 << 4,			// obj = (Value value) | arguments, user constants, default constants and generic constants
			DefaultC = 1 << 5,		// obj = (Value value) | arguments, user constants, default constants and generic constants
			Number = (1 << 5) + 1,	// not dictionary, just for parsing colors - direct numbers
			Text = (1 << 5) + 2,	// not dictionary, just for parsing colors - generic code text
			Comment = (1 << 5) + 3,	// not dictionary, just for parsing colors - comment
			String = (1 << 5) + 4,	// not dictionary, just for parsing colors - "string"
			Error = (1 << 5) + 5,	// not dictionary, just for parsing colors - error
			Success = (1 << 5) + 6,	// not dictionary, just for parsing colors - success
			Back = (1 << 5) + 7,	// not dictionary, just for parsing colors - background
			Fore = (1 << 5) + 8		// not dictionary, just for parsing colors - non-code text
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

	private readonly ushort _stackOverflow = stackOverflowLimit, _doOverflow = doOverflowLimit, _loop = loopLimit, _iterLimit = iteratorLimit;
	private readonly bool _caseInsensitive = caseInsensitive;
	static protected readonly Value None = new();
	private static readonly Value StackOverflow = new(1);
	//private Dictionary<string, CallFunction> _customFunctions = []; // user-defined functions
	//private Value _customConstants = None; // user-defined constants
	private static readonly Cf OpFact = new(T.Factorial, OpCode.Factorial);
	private static readonly Cf OpSqr = new(T.Sqr, OpCode.Sqr);
	private static readonly Cf OpConj = new(INumber<T>.Conj, OpCode.Conj);
	private static readonly Ce OpCount = new(typeof(FuncCount), 0);
	private static readonly Ce OpCatCount = new(typeof(FuncCatCount), 0);
	private static readonly Cf OpAbs = new((z) => T.MakeR(INumber<T>.Abs(z)), OpCode.Abs);
	private static readonly Cf OpSqrAbs = new(INumber<T>.SqrAbs, OpCode.SqrAbs);
	private static readonly Cf OpAbsRi = new(T.AbsComp, OpCode.Absri);
	private static readonly Cf OpSign = new(INumber<T>.Sign, OpCode.Sgn);
	protected abstract Value GenericConstants();
	protected readonly ParseDictionary Context = new();
	#endregion
	
	#region Helpers
	public class Reader(Comparser<T> context, string text, CancellationToken cancel, int from = 0) {
		public char nextChar => From < Text.Length ? Text[From] : ';';
		public readonly Comparser<T> Context = context;
		public int From = from, Line = 1;
		public readonly CancellationToken Cancel = cancel;
		public readonly string Text = text;
		public readonly List<(int position, Color color)> Colors = [(0, context.GetColor(ParseDictionary.Type.Text))];
		private readonly List<(int, bool)> _comments = [(0,false)];
		private void GetChar(char c, out int location, int? i = null, string? dtext = null) {
			int f = i ?? From;
			string d = dtext ?? Text;
			if (f >= d.Length || (location = d.IndexOf(c, f)) < 0) location = int.MaxValue;
		}
		/*public bool CharAtAbs(char c, int offset, string? dtext = null) {
			var t = dtext ?? Text;
			return offset < t.Length && t[offset] == c;
		}*/
		public bool CharAtRel(char c, int offset) {
			var t = Text;
			return (offset += From) < t.Length && t[offset] == c;
		}
		//public void AddStart(int start, ParseDictionary.Type t) {//TrimColors(start);//if(Colors.Count == 0 || start >= Colors[^1].position)Colors.Add((start, Context.GetColor(t)));//}
		public void AddC(int start, int end, ParseDictionary.Type colorStart) => AddC(start, end, Context.GetColor(colorStart));
		public void AddC(int start, int end, Color colorStart) {
			if (end <= start)
				return; // no range, don't do anything
			int ip = Colors.Count;
			var returnColor = Context.GetColor(ParseDictionary.Type.Text); // default return color
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
			//while (++e < Colors.Count && Colors[e].position < end) ;//Colors.Insert(s,(end, colorEnd));//Colors.Insert(s, colorStart);//if(Colors.Count == 0 || start >= Colors[^1].position)Colors.Add((start, c));
		}
		public void AppendC(List<(int, Color)> append) {
			foreach (var c in append)
				Colors.Add(c);
		}
		/*private void TrimColors(int start) {
			int i = Colors.Count;
			while(i > 0 && Colors[i - 1].position)
			while (Colors.Count > 0 && start < Colors[^1].position)
				Colors.RemoveAt(Colors.Count - 1);
		}*/
		public bool IsComment() => CharAtRel('*', 1) && !GotoFirstFailed([], 1, out _, out _, true);
		public bool TrimStart(int separators = 0, bool real = true) {
			// TODO should also trim comments
			while (Text.Length > From)
				switch (Text[From]) {
				case '/':
					if (!GotoFirstFailed([], separators, out _, out _, true, 0,false,true,null, real)) // TODO i just changed this to !, is that ok?
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
		// real = 0: do not advance from, line and colorsand comments
		// real = 1: only advance from and line, not colors and comments
		// real = 2: advance everything
		public bool GotoFirstFailed(int offset, int real, char[] c, int separators, out int s, out int found, bool comment = false, int skip = 0, bool str = false, bool otherMustBeNext = true, bool[]? foundCantBeNext = null) {
			int bi = From, bl = Line;
			TrimStart(separators, real < 2);
			found = int.MaxValue;
			s = -1;
			
			var failed= Text.Length <= (From += offset) || GotoFirstFailed(c, separators, out s, out found,  comment,  skip ,  str, otherMustBeNext, foundCantBeNext, real > 0);
			if (real >= 1 && !failed)
				return failed;
			From = bi;
			Line = bl;
			return failed;
		}
		// it will only advance if it succeeded
		public bool GotoFirstFailed(int offset, char[] c, int separators, out int s, out int found, bool comment = false, int skip = 0, bool str = false, bool otherMustBeNext = true, bool[]? foundCantBeNext = null) 
			=> GotoFirstFailed(offset, 0, c, separators, out s, out found, comment, skip, str, otherMustBeNext, foundCantBeNext)
				|| GotoFirstFailed(c, separators, out s, out found, comment, skip, str, otherMustBeNext, foundCantBeNext);
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
		/// <param name="foundCantBeNext">which of the target characters are not allowed to have non-ignored cahracted before them?</param>
		/// <param name="real">if set to false, it will not collect comment data (used for peeking without changing the state). But make sure to restore From and Line manually!</param>
		/// <returns></returns>
		public bool GotoFirstFailed(char[] c, int separators, out int s, out int found, bool comment = false, int skip = 0, bool str = false, bool otherMustBeNext = true, bool[]? foundCantBeNext = null, bool real = true) {
			bool r, wasComment = comment, wasSkip = skip > 0, wasString = str;
			found = int.MaxValue;
			s = -1;
			int startComment = -1;
			if (comment) {
				if (real) {
					_comments.Add((From, true));
					startComment = From; //AddC(From, ParseDictionary.Type.Comment);
				}
				From += 2; // found dash and star, starting a comment, eat that first dash and star
				r = Perform(ref s, ref found);
				if (!real)
					return r;
				if (startComment >= 0)
					AddC(startComment, From, ParseDictionary.Type.Error);
				if(startComment >= 0)
					_comments.Add((From, false));
			} else r = Perform(ref s, ref found);
			
			return r;
			bool Perform(ref int s, ref int found) {
				var searches = new int[c.Length];
				if (TrimStart(separators, real))
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
				while (int.MaxValue != (next = Math.Min( /*Math.Min(ln, */strMark /*)*/, Math.Min(Math.Min(braStart, braEnd), Math.Min(commDash, search))))) {
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
						GetChar('"', out braStart);
						if (comment)
							break; // inside a comment, doesn't count
						if (!str && skip == 0)
							return true;
						if (Text[From - 2] != '\\' && (str = !str) == false && wasString)
							return false; // found end of string
						break;
					case '/': // comment
						if (otherMustBeNext && Text[From] != '/' && skip == 0 && !comment && !str)
							return true;
						bool endComment = next > 0 && Text[next - 1] == '*', newComment = next < Text.Length - 1 && Text[next + 1] == '*';
						//if (newComment) ++next;
						Eat();
						GetChar('/', out commDash);
						if (str)
							break; // inside a string, doesn't count
						var prev = comment;
						if (!(comment = newComment || !endComment)) {
							if (real) {
								if (prev) AddC(startComment, From, ParseDictionary.Type.Comment);
								startComment = -1;
								_comments.Add((From, false));
							}
							if (wasComment)
								return false; // found the end of the comment
						} else if (prev != comment) { 
							if(real)
								_comments.Add((startComment = next, true));
						} else if (!comment && skip == 0 && (!newComment || endComment))
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
						//if (next != 0 && skip == 0 && !comment && !str)
						//	return true; // didn't find it
						return true; // found something wrong
					}
				}
				//if(mustBeNext)
				//if(GotoFirstFailed([';','\n'], out _,false,0,false,false))
				//	i = codeL.Length;
				return true; // didn't find it

				void Eat() {
					if (ln < next) 
						++Line;
					From = next + 1;
					TrimStart(separators, real);
				}
			}
		}
		public string Uncomment(int from, int to) {
			int lo = 0, hi = _comments.Count, mid;
			//var mSy = _mSy2 - yC;
			while (lo < hi) {
				mid = lo + hi >> 1;
				int d = _comments[mid].Item1;
				if (d == from || hi == 1+lo)
					break;
				if (d < from) lo = mid + 1; // target is closer to d stepped towards hi
				else hi = mid; // target is closer to d stepped away from hi
			}
			var comment = _comments[Math.Min(lo,_comments.Count-1)].Item2;
			var end = Math.Min(Text.Length, to);
			var s = "";
			while (from < end && ++lo < _comments.Count) {
				var t = _comments[lo].Item1;
				if (!comment && t > from) s += lo >= _comments.Count ? Text[from..] : Text[from..t];
				comment = _comments[lo].Item2;
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
	/*private void CollectC(Expression expression, List<(int position, Color color)> col) {
		foreach (var c in expression.Colors)
			AddC(c.start, c.color, col);
	}*/
	
	private static Value CollapseScalar(Value i) {
		while (i.Values.Length == 1)
			i = i.Values[0];
		return i;
	}
	private static Value UnCollapseScalar(Value i) {
		if (i.Values.Length == 0)
			i.Values = [new(i.Leaf, i.Error, i.String) {Operand = i.Operand}];
		return i;
	}
	//private static int SafeCollapse(Value[] v, int i) => v.Length == 0 ? 0 : (v[i] = CollapseScalar(v[i])).Values.Length;
	//private static T SafeLeaf(Value[] v, int i) => v.Length == 0 ? T.NaN() : v[i].Leaf;
	//private static string SafeText(Value[] v, int i) => v.Length == 0 ? "" : v[i].Text;
	[GeneratedRegex(@"^[a-zA-Z0-9\s,]*$")]
	private static partial Regex MyRegex();
	#endregion

	private void FillDefault() {
		C("π", INumber<T>.C_Pi());
		C("pi", INumber<T>.C_Pi());
		C("τ", INumber<T>.C_Pi());
		C("tau", INumber<T>.C_Tau());
		C("e", INumber<T>.C_E());
		C("gamma", INumber<T>.C_Gamma());
		C("γ", INumber<T>.C_Gamma());
		C("one", T.One());
		foreach(var d in GenericConstants().Values)
			C(d.String, d.Leaf);
		
		CallFunction min, max, mul, sum, prod, vec, ln, nsinhc, nsinc, re, im, neg, inv, compMod, cub, trunc, sinhc, ceil;
		// meta
		A("eval", new Ce(typeof(FuncEval), 0)); // attempts to parse and evaluate every Text in the input
		A("count", OpCount); // counts the number of elements in the vector
		A("catcount", OpCatCount); // counts the total number of elements in the vector
		A("totalcount", OpCatCount); // counts the total number of elements in the vector
		A("concat", new Ce(typeof(FuncCat), 0)); // Un-nests the vectors: concat((1,2),3,((4,5),6)) = (1,2,3,4,5,6)
		A("cat", new Ce(typeof(FuncCat), 0)); // Un-nests the vectors: concat((1,2),3,((4,5),6)) = (1,2,3,4,5,6)

		// double arguments:
		A("minimum", min = new Cf2(T.Min, OpCode.Min)); // component-wise minimum
		A("maximum", max = new Cf2(T.Max, OpCode.Max)); // component-wise maximum
		A("min", min); // component-wise minimum
		A("max", max); // component-wise maximum
		A("softmax", new Cf2(INumber<T>.SoftMax, OpCode.SoftMax));
		A("softmin", new Cf2(INumber<T>.SoftMin, OpCode.SoftMin));
		A("add", new Cf2(INumber<T>.Add, OpCode.Add)); // adds all the top layer elements of the input vector
		A("mul", mul = new Cf2(INumber<T>.Mul, OpCode.Mul)); // multiplies all the top layer elements of the input vector
		A("multiply", mul);
		A("icoef", new Cf2((x, y) => -x * y, OpCode.ImCoef)); // = re(-a*b), imaginary coefficient icoef(a+bi,i)=b, icoef(r+ai+bj+ck,j)=b, icoef(a+bi,1)=-a
		A("compmod", compMod = new Cf2(INumber<T>.CompMod, OpCode.CompMod));
		A("cmod", compMod); // component-wise remainder, returns 0 when dividing by zero

		// triple arguments
		A("clamp", new Cf3(T.Clamp, OpCode.Clamp)); // component-wise clamp

		// quadruple arguments
		A("product", prod = new Ce(typeof(Product), 0)); // iterative product
		A("prod", prod); // iterative product
		A("Π", prod); // iterative product
		A("sum", sum = new Ce(typeof(Sum), 0)); // iterative sum
		A("Σ", sum);
		A("vector", vec = new Ce(typeof(Vector), 0)); // iterative vector builder
		A("vec", vec); // iterative vector builder

		// exp/log
		A("exp10", new Cf(INumber<T>.Exp10, OpCode.Exp10)); // 10^x
		A("exp2", new Cf(INumber<T>.Exp2, OpCode.Exp2)); // 2^x
		A("exp", new Cf(T.Exp, OpCode.Exp)); // e^x
		A("log10", new Cf(INumber<T>.Log10, OpCode.Log10)); // log_10(x)
		A("log2", new Cf(INumber<T>.Log2, OpCode.Log2)); // log_2(x)
		A("log", ln = new Cf(T.Log, OpCode.Log)); // ln(x)
		A("ln", ln); // ln(x)

		// sincs
		A("nsinhc", nsinhc = new Cf(INumber<T>.Nsinhc, OpCode.Nsinhc));
		A("sinchpi", nsinhc);
		A("sinhc", sinhc = new Cf(INumber<T>.Sinhc, OpCode.Sinhc));
		A("sinch", sinhc);
		A("nsinc", nsinc = new Cf(INumber<T>.Nsinc, OpCode.Nsinc));
		A("sincpi", nsinc);
		A("sinc", new Cf(INumber<T>.Sinc, OpCode.Sinc));

		// coscs
		A("coshc", new Cf(INumber<T>.Coshc, OpCode.Coshc));
		A("coshcpi", new Cf(INumber<T>.Ncoshc, OpCode.Ncoshc));
		A("ncoshc", new Cf(INumber<T>.Ncoshc, OpCode.Ncoshc));
		A("cosc", new Cf(INumber<T>.Cosc, OpCode.Cosc));
		A("coscpi", new Cf(INumber<T>.Ncosc, OpCode.Ncosc));
		A("ncosc", new Cf(INumber<T>.Ncosc, OpCode.Ncosc));

		// arc hyperbolics
		A("acosh", new Cf(T.Acosh, OpCode.Acosh));
		A("asinh", new Cf(T.Asinh, OpCode.Asinh));
		A("atanh", new Cf(T.Atanh, OpCode.Atanh));
		A("asech", new Cf(INumber<T>.Asech, OpCode.Asech));
		A("acsch", new Cf(INumber<T>.Acsch, OpCode.Acsch));
		A("acoth", new Cf(T.Acoth, OpCode.Acoth));

		// hyperbolics
		A("cosh", new Cf(T.Cosh, OpCode.Cosh));
		A("sinh", new Cf(T.Sinh, OpCode.Sinh));
		A("tanh", new Cf(T.Tanh, OpCode.Tanh));
		A("sech", new Cf(INumber<T>.Sech, OpCode.Sech));
		A("csch", new Cf(INumber<T>.Csch, OpCode.Csch));
		A("coth", new Cf(T.Coth, OpCode.Coth));

		// arc trigs
		A("acos", new Cf(T.Acos, OpCode.Acos));
		A("asin", new Cf(T.Asin, OpCode.Asin));
		A("atan", new Cf(T.Atan, OpCode.Atan));
		A("asec", new Cf(INumber<T>.Asec, OpCode.Asec));
		A("acsc", new Cf(INumber<T>.Acsc, OpCode.Acsc));
		A("acot", new Cf(T.Acot, OpCode.Acot));

		// trigs
		A("cos", new Cf(T.Cos, OpCode.Cos));
		A("sin", new Cf(T.Sin, OpCode.Sin));
		A("tan", new Cf(T.Tan, OpCode.Tan));
		A("sec", new Cf(INumber<T>.Sec, OpCode.Sec));
		A("csc", new Cf(INumber<T>.Csc, OpCode.Csc));
		A("cot", new Cf(T.Cot, OpCode.Cot));

		// unary
		A("true", new Cf((x) => T.MakeR(T.Re(INumber<T>.SqrAbs(x)) >= 1 ? 1 : 0), OpCode.True)); // = size >= 1
		A("false", new Cf((x) => T.MakeR(T.Re(INumber<T>.SqrAbs(x)) < 1 ? 1 : 0), OpCode.False)); // = size < 1
		A("real", re = new Cf(INumber<T>.T_Re, OpCode.Re)); // real part: re(a+bi) = a
		A("re", re); // real part
		A("imag", im = new Cf(INumber<T>.T_I, OpCode.Im)); // imaginary sum: im(r+ai+bj+ck) = a+b+c
		A("im", im); // imaginary sum
		A("immg", new Cf((x) => T.MakeR(T.ImMag(x)), OpCode.ImMag)); // imaginary magnitude immg(r+ai+bj+ck) = sqrt(a^2+b^2+c^2)
		A("frac", new Cf(T.Frac, OpCode.Frac)); // = fractional part
		A("trunc", trunc = new Cf(T.Trunc, OpCode.Trunc)); // = whole part
		A("truncate", trunc); // = whole part
		A("floor", new Cf(T.Floor, OpCode.Floor)); // = round down
		A("round", new Cf(T.Round, OpCode.Round)); // = round
		A("ceiling", ceil = new Cf(T.Ceil, OpCode.Ceil)); // = round up
		A("ceil", ceil); // = round up
		A("sign",OpSign); // = z/|z|
		A("sgn", OpSign); // = z/|z|
		A("negative", neg = new Cf(INumber<T>.Neg, OpCode.Neg)); // = -z
		A("neg", neg); // = -z
		A("inverse", inv = new Cf(T.Inv, OpCode.Inv)); // = 1/z
		A("inv", inv); // = 1/z
		A("absri", OpAbsRi); // component-abs: absri(a+bi) = |a|+|b|i
		A("compabs", OpAbsRi); // component-abs
		A("cabs", OpAbsRi); // component-abs
		A("sqrabs", OpSqrAbs); // = |z|^2; sqrabs(a+bi) = a^2+b^2
		A("absolute", OpAbs); // = |z|
		A("abs", OpAbs); // = |z|
		A("norm", OpAbs); // = |z|
		A("arg", new Cf(INumber<T>.T_Arg, OpCode.Arg)); // argument, the angle from (0,0). arg(-1)=pi
		A("conjugate",OpConj);
		A("conj", OpConj); // conjugate: negates all imaginary units, conj(r+ai+bj+dk) = r-ai-bj-bk
		// powers
		A("sqrt", new Cf(T.Sqrt, OpCode.Sqrt)); // square root = z^(1/2)
		A("sqr", OpSqr); // square = z^2
		A("cbrt", new Cf(INumber<T>.Cbrt, OpCode.Cbrt)); // cube root = z^(1/3)
		A("cube", cub = new Cf(T.Cub, OpCode.Cub)); // cube = z^3
		A("cub", cub); // cube
		A("quart", new Cf(T.Quart, OpCode.Quart)); // z^4

		// specials
		A("fact", OpFact); // factorial
		A("factorial", OpFact); // factorial
		A("gauss", new Cf(T.Gauss, OpCode.Gauss)); // gauss e^(-z^2)
		A("Γ", new Cf(T.Gamma, OpCode.Gamma)); // gamma function = (xz1)!
		A("gamma", new Cf(T.Gamma, OpCode.Gamma)); // gamma function = (xz1)!
		A("ζ", new Cf(T.Zeta, OpCode.Zeta)); // riemann zeta function
		A("zeta", new Cf(T.Zeta, OpCode.Zeta)); // riemann zeta function
		A("softabs", new Cf(INumber<T>.SoftAbs, OpCode.SoftAbs)); // = e^(1+ln(z))
		A("softneg", new Cf(INumber<T>.SoftNeg, OpCode.SoftNeg)); // = e^(1+ln(z))
		return;
		void C(string name, T v) => Context.Insert(new(new Value(v, 0, name), ParseDictionary.Type.DefaultC), name);
		void A(string name, CallFunction c) => Context.Insert(new(c, ParseDictionary.Type.DefaultF), name);
	}
}
public class ComparserR : Comparser<Real> { override protected Value GenericConstants() => None; }
public class ComparserC : Comparser<Complex> { override protected Value GenericConstants() => new([new(Complex.i, 0, "i")]); }
public class ComparserQ : Comparser<Quaternion> { override protected Value GenericConstants() => new([new(Quaternion.i, 0, "i"), new(Quaternion.j, 0, "j"), new(Quaternion.k, 0, "k")]); }



/*while (!found && (!comment && skip > 0 && (plus != int.MaxValue || minus != int.MaxValue) || comment && comm != int.MaxValue)) {
	var best = comm < plus && comm < minus;
	if ((comment || best) && comm != int.MaxValue) {
		if (comment) {
			comment &= codeLine[comm - 1] != '*' || comm + 1 < codeLine.Length && codeLine[comm + 1] == '*';
			EatComm();
		} else if (best) {
			comment |= comm + 1 < codeLine.Length && codeLine[comm + 1] == '*';
			EatComm();
		}
		bool EatComm() {
			if(eat = Eat(comm))
				GetChar('/', out comm);
			else if (FailNewLine())
				return true;

		}
	}
	if (!comment && skip > 0) {
		if (plus != int.MaxValue && plus < minus && plus < comm) {
			eat = Eat(plus);
			++skip;
			GetChar('{', out plus);
		} else if (minus != int.MaxValue && minus < comm) {
			eat = Eat(minus);
			if (--skip == 0) {
				// TODO enter the else branch if skipIf and :
			} else GetChar('}', out minus);
		}
	}
	continue;*/

/*foreach (var p in parsedC)
	if (p.String == name) {
		// mutate variable
		p.Values = eval.Values;
		p.Leaf = eval.Leaf; // probably not needed?

		//_customConstants = new([.. parsedC]);
		return;
	}
// new variable
parsedC.Add(eval);*/
//_customConstants = new([.. parsedC]);