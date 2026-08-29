using Comparser.Comparser.Numbers;
using System.Text.RegularExpressions;
namespace Comparser.Comparser;
public interface IComparser {
	public object Parse(string text, int from, object? args = null);
	// Re-evaluates already parsed expression with new arguments
	public object Eval(object exp, object? args = null);
	// Parses and evaluates the text with selected arguments (and returns the expression for possible re-evaluation)
	public object ParseEval(string text, int from, out object expr, object? args = null);
	// Parses and evaluates the text with selected arguments (and returns the expression for possible re-evaluation, and eats the parsed part, allows it to be incomplete - leaving the remainder in the ref text)
	public object ParseEval(string text, ref int from, object? args = null);
	// Parses and evaluates the text with selected arguments (without returning the parsed expression, only immediate one-time evaluation)
	public object ParseEval(string text, int from, object? args = null);
	public string ToString(object value, int decimals = -1);
	public void SetDarkMode(bool dark);
	public List<(Color color, string log)> ReadCode(string text, out List<(int position, Color color)> colors);
	
}

public abstract partial class Comparser<T>(bool caseInsensitive = true, ushort stackOverflowLimit = 499, ushort doOverflowLimit = 499, ushort loopLimit = 499, ushort iteratorLimit = 499) : IComparser where T : unmanaged, INumber<T> {


	public static bool CharAt(string text, char c, int offset) => offset < text.Length && text[offset] == c;
	public static bool TrimStart(string text, ref int from) {
		while (text.Length > from)
			switch (text[from]) {
			case ' ':
			case '\t':
			case '\r': ++from; break;
			default: return from >= text.Length;
			}
		return from >= text.Length;
	}
	
	#region Interface
	private static Value AsInput(object? e) => e as Value ?? new();
	public object Parse(string text, int from, object? args) => new Expression(this, text, AsInput(args), from);
	public object Eval(object exp, object? args) => (exp as Expression ?? new Expression(this, "", None)).Eval(0, AsInput(args));
	public object ParseEval(string text, int from, out object expr, object? args) { var e = (Expression)Parse(text, from, args); expr = e; return e.Eval(0, AsInput(args)); }
	public object ParseEval(string text, ref int from, object? args) => new Expression(this, text, ref from, out _, AsInput(args)).Eval(0, AsInput(args));
	public object ParseEval(string text, int from, object? args) => ParseEval(text, ref from, args);
	public string ToString(object value, int decimals = -1) => AsInput(value).ToString(decimals);
	public void SetDarkMode(bool dark) => _darkMode = dark;
	public List<(Color, string)> ReadCode(string text, out List<(int, Color)> colors) {
		
		Context.Clear();
		FillDefault(); // collects the default functions and constants into the Dictionary
		Dictionary<string, List<(Value input, Expression def, Expression? cond)>> parsedF = [];
		Dictionary<string, Value> parsedC = [];
		var brackets = 0;
		colors = [];
		List<(int position, Color color)> col = [(0,GetColor(ParseDictionary.Type.Text))];
		List<(int i, int c)> back = [];
		string e;
		List<(Color, string)> log = [];
		Dictionary<string, CallFunction> userFunctions = [];
		ReadLines("", _caseInsensitive ? text.ToLower() : text);
		if (brackets > 0) 
			log.Add((Color.Red, "Missing " + brackets + "x END BRACKET and eof. Not fatal but probably wrong."));
		colors = col;
		return log;
		
		//string CleanWhite(string cl) => cl.Replace(" ", "").Replace("\t", "").Replace("\r", "");
		void ReadLines(string pref, string codeL, int i = 0) {
			var line = 1; // line counter for logging
			//_context.CustomFunctions = new Comparser.CallFunction[CustomFunctions.Count];
				while (i < codeL.Length) {
					
					// (<cacheIntExpression>)functionName(<argumentsExpression>:<defaultArgumentExpression>)=<definitionExpression>
					// functionName(<argument1Expression>,<argument2Expression>):<conditionExpression>?<ifExpression>:<elseExpression>
					// constantName:<constantExpression>
					// commandName:<commandArgumentExpression>
					
					int cache = 1;
					//var name = "";
					Value eval;
					TrimStart(codeL, ref i);
					int beforeI = 0;
					void ColorError() => AddColor(beforeI, ParseDictionary.Type.Error);
					void ColorText(int at = -1) => AddColor(0 <= at ? at : i, ParseDictionary.Type.Text);
					switch (codeL[i]) {

					// TODO must account for whitespace everywhere!

					case '/': // Comment
						if (!CharAt(text, '*', 1 + i) || GotoFirstFailed([], out _, true))
							Cl(true);
						break;
					case '}': // this must be and out of a branch I'm in, just ignore it
						if (--brackets < 0) {
							Cl(true); // closing a bracket that wasn't started
							break;
						}
						var bb = back[brackets];
						var (bi, bc) = (bb.i, bb.c + 1);
						if (bi >= 0) {
							// at the end of while
							if (bc <= _loop) {
								// repeat
								i = bi; // goto */
								back[brackets] = (bi, bc); // increment stack overflow counter
								break;
							}
							//fail exit
							back[brackets] = (-1, 1);
							e = "WHILE loop limit exceeded.";
							Lg();
						}
						++i; // eat end bracket
						if (back[brackets].c > 0) {
							// no else expected
							back[brackets] = (bi, 0);
							break;
						}
						if (GotoFirstFailed([':'], out _))
							break;
						beforeI = i;
						if ("" != (e = GotoFirstFailed(['{'], out _) ? "Failed to find a START BRACKET after ELSE."
							: GotoFirstFailed([], out _, false, 1) ? "Failed to find an END BRACKET after skipping an ELSE branch." : "")) {
							Lg();
							Cl(false);
						}
						break;
					case ';': // separator
					case '\t': // space
						++i; // eat it
						break;
					case '!': // WHILE
						var b = GetBack();
						Conditional(b.i < 0 ? (i, 0) : (i, b.c));
						break;
					case '?': // IF
						Conditional((-1, 0));
						break;
					case ':':
					case '{': // these are unexpected when finding them out of the blue, they are always searched for and eaten with GotoFirstFailed instead, when legit
						Cl(true);
						break;
					case '(': // CACHE
						beforeI = ++i;
						if ("" != (e = FailEvalClose(out eval) ? "Failed to parse CACHE size."
							: eval.Values.Length != 1 ? "Multiple values in the CACHE size expression: " + eval
							: eval.Values[0].Leaf.IsNaN() ? "CACHE size evaluated as NaN." : "")) {
							ColorError();
							FailEnd();
							break;
						}
						cache = (int)Math.Round(T.Re(eval.Values[0].Leaf));
						goto default;
					default:
						Value args = None;
						//string s, s1;
						//var sc = codeLine;
						if (_caseInsensitive)
							codeL = codeL.ToLower(); //.Replace(" ", "");//.TrimEnd(' ');
						TrimStart(codeL, ref i);
						beforeI = i;
						string name = "";

						int s = 0, commandCode = -1;
						bool IsFunc() {
							// processes the name and returns whether it is supposed to be a new function
							if (s == 0) {
								AddColor(beforeI, ParseDictionary.Type.UserF);
								ColorText(beforeI + name.Length);
								return true;
							}
							commandCode = name switch {
								"print" => 0,
								"do" => 1,
								"return" => 2,
								"break" => 3,
								"continue" => 4,
								_ => -1
							};
							AddColor(beforeI, commandCode < 0 ? ParseDictionary.Type.UserC : ParseDictionary.Type.Do);
							ColorText(beforeI + name.Length);
							return false;
						}
						if ("" != (e = GotoFirstFailed(['(', ';', ':'], out s, false, 0, false, false) || s == 2 ? "No COLON, SEMICOLON or parenthesis found on this line."
							: TrimEnd(codeL[(beforeI + (name = TrimEnd(codeL[beforeI..(i - 1)])).Length)..(i - 1)]).Length > 0 ? "Unexpected text after definition NAME."
							: name.Length == 0 ? "No definition name." : !IsAlphaNumeric(name) ? "Failed to parse name: " + name
							: IsFunc() && FailArgs(out args) ? "Failed to parse ARGUMENTS." : GotoFirstFailed([':'], out _) ? Fc() : "")) {
							Err();
							break;
						}
						//int nameI = i;
						//codeLine = codeLine[(nextChar + 1)..];
						//(s, s1) = (sc[..nextChar], codeLine[(nextChar + 1)..]);
						if (args.Values.Length == 0) { // it is a constant or code call:
							if (cache != 1) {
								e = name + " is a constant/call and doesn't support caching. Not fatal but doesn't do anything.";
								Lg();
							}

							var expression = new Expression(this, codeL, ref i, out _, args);
							CollectColors(expression);
							eval = expression.Eval(0, None);
							bool doContinue = false, command = true;
							switch (commandCode) {
							case 0: // print
								log.Add((Color.Black, eval.ToString()));
								Cl(true);
								break;
							case 1: // do
								if (pref.Length > _doOverflow) {
									e = "DO overflow limit exceeded.";
									Lg();
								} else {
									var expand = eval.ToLines() /*.Split('\n')*/; // ToString(true) recursively exports only string values as lines
									ReadLines(pref + (line + 1) + "/", expand);
								}
								Cl(true);
								break;
							case 2: // return
								EndLoop(false);
								break;
							case 3: // break
								EndLoop();
								break;
							case 4: // continue
								EndLoop(true, false);
								break;
							default:
								command = false;
								break;

								void EndLoop(bool onlyLoops = true, bool dontContinue = true) {
									for (var loops = (int)T.Re(eval.GetLeaf()); loops > 0;) {
										if (0 < brackets-- || GotoFirstFailed([], out _, false, 1)) {
											e = "Couldn't find an END bracket to escape from.";
											Lg();
											Cl(true);
										}
										if (onlyLoops && back[brackets].i < 0)
											continue; // not a loop, don't count that
										if (loops == 1) {
											--i; // go back to the end bracket
											++brackets;
											if (dontContinue)
												back[brackets] = (-1, 0);
											doContinue = true;
											return;
										}
										back[brackets] = (-1, 0);
										--loops;
									}
									Cl(true);
								}
							}
							if (doContinue)
								goto case '}';
							if (command)
								break;
							eval.String = name;
							Cl(true);
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
						if (!userFunctions.ContainsKey(name)) // create the custom function if this is its first definition
							Context.Insert(new(userFunctions[name] = new CallCustom([]), ParseDictionary.Type.UserF), name);

						Expression defaultBranch = new(this, codeL, ref i, out _, args, cache);
						CollectColors(defaultBranch);
						//s1 = s1.TrimStart(' '); 
						Conds();
						void Conds() {
							for (List<(Expression, Expression)> conditionals = []; !GotoFirstFailed(['?'], out _);
								defaultBranch = new(this, codeL, ref i, out _, args, cache)) {
								CollectColors(defaultBranch);
								Expression trueExp = new(this, codeL, ref i, out _, args, cache);
								CollectColors(trueExp);
								if (GotoFirstFailed([':'], out _)) {
									AddF(args, conditionals);
									return;
								}
								conditionals.Add((trueExp, defaultBranch));
							}
							AddF(args, [], defaultBranch); // no if
						}
						break;

						void FailEnd() {
							Lg();
							Cl(false);
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
							Cl(false);
						}
						void Err() {
							ColorError();
							Lg();
							Cl(false);
						}
						//codeLine = (nextChar = s.IndexOf(';')) >= 0 ? s[(nextChar + 1)..] + s1 : (nextChar = s1.IndexOf(';')) >= 0 ? s1[(nextChar + 1)..] : "";
						//}
						(int i, int c) GetBack() => back.Count > brackets ? back[brackets] : (-1, -1);
						void Conditional((int i, int c) rb) {
							if (brackets < back.Count) back[brackets] = rb;
							else back.Add(rb);
							if ("" != (e = FailEval(out eval) ? "Failed to parse CONDITION." : "")) {
								FailEnd();
								return;
							}
							while (eval.Values.Length > 0)
								eval = eval.Values[0];
							var fail = INumber<T>.IsFalse(eval.Leaf);
							e = GotoFirstFailed(['{'], out _) ? "Failed to find a START BRACKET after a CONDITION."
								: fail ? GotoFirstFailed([], out _, false, 1)
									? "Failed to find an END BRACKET after skipping a failed condition"
									//: loops > 0 ? "_I" // not first while, brackets DONT, loops DONT 
									: GotoFirstFailed([':'], out _) ? "_N" // no :, bracket DONT, loops=0
										: GotoFirstFailed(['{'], out _) ? "Failed to find a START BRACKET when trying to enter an ELSE branch." :
											rb.c == 0 ? "_E" : GotoFirstFailed([], out _, false, 1) ? "Failed to find an END BRACKET after WHILE ELSE." : "_W" // else, ++brackets, loops=0
								: "_T"; // true (loops DONT, ++brackets)
							if (e[0] == '_') { // "loop" during WHILE = stack overflow limit, "loop" during IF = skip :{} after ending a block
								switch (e[1]) {
								case 'T':
									++brackets;
									if (rb.i < 0) rb.c = 0;
									break;
								case 'W': rb.i = -1; break;
								case 'E':
									var il = rb.i >= 0;
									if (!il || rb.c == 0) ++brackets;
									if (il) rb.i = -1;
									goto case 'N';
								case 'N': rb.c = 1; break;
								}
							} else FailEnd();
						}
					}
					continue;

					void CollectColors(Expression expression) {
						foreach (var c in expression.Colors)
							AddColor(c.start, c.color);
					}
					void AddColor(int start, ParseDictionary.Type t) {
						while (col.Count > 0 && start < col[^1].position)
							col.RemoveAt(col.Count - 1);
						col.Add((start,GetColor(t)));
					}
					string TrimEnd(string txt) {
						GetChar(' ', out var s, txt);
						GetChar('\r', out var r, txt);
						GetChar('\t', out var t,txt);
						var min = Math.Min(s,Math.Min(r,t));
						return min < int.MaxValue ? txt[..min] : txt;
					}
					//string NoWhite(string s) => s.Replace(" ", "").Replace("\t", "").Replace("\r", "");
					bool FailEval(out Value evaluated, bool isArg = false) {
						//++i;//exp = exp[1..]; // eat initial symbol that triggered it
						var expression = new Expression(this, codeL, ref i, out _, None, 0, 0, isArg);
						CollectColors(expression);
						evaluated =  UnCollapseScalar(expression.Eval(0, None));
						return false;
					}
					bool FailEvalClose(out Value evaluated, bool isArg = false) {
						if (FailEval(out evaluated, isArg) || GotoFirstFailed([')'], out _))
							return true; // closing parenthesis after the argument expression
						//++i;//exp = exp[1..];
						return false;
					}
					void Cl(bool unexpected) {
						if (!(unexpected || (GotoFirstFailed(['}', ';', '\n'], out _) && i < codeL.Length)))
							return;
						e = "Unexpected text from: " + codeL[i..Math.Min(i+12,codeL.Length)];
						ColorError(); i = codeL.Length;	ColorText(); Lg();
					}
					void Lg() {
						// TODO prepare tooltip pointers
						log.Add((Color.Red,"Line " + pref + line + ": " + e));
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
					bool GotoFirstFailed(char[] c, out int s, bool comment = false, int skip = 0, bool str = false, bool mustBeNext = true) {
						
						bool wasComment = comment, wasSkip = skip > 0, wasString = str;
						if (comment) {
							AddColor(i, ParseDictionary.Type.Comment);
							i += 2; // found dash and star, starting a comment, eat that first dash and star
						}
						s = -1;
						var searches = new int[c.Length];
						TrimStart(text, ref i);
						var search = int.MaxValue;
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
						int next;
						while (int.MaxValue != (next = Math.Min(Math.Min(ln, strMark), Math.Min(Math.Min(braStart, braEnd), Math.Min(commDash, search))))) {
							search = int.MaxValue;
							for (s = 0; s < c.Length; ++s) {
								if (codeL[next] == c[s]) {
									var isNotNext = next != i && mustBeNext;
									Eat();
									GetChar(c[s], out searches[s]);
									if (comment || skip > 0)
										continue;
									return isNotNext; // found it as the next non white space character or not?
								}
								search = Math.Min(search, searches[s]);
							}
							// skip comment and braces:
							switch (codeL[next]) {
							case '\n': // new line
								++line;
								Eat();
								GetChar('\n', out ln);
								break;
							case '"': // string
								if (mustBeNext && codeL[i] != '"' && skip == 0 && !comment && !str)
									return true;
								Eat();
								GetChar('"', out braStart);
								if (comment)
									break; // inside a comment, doesn't count
								if (!str && skip == 0)
									return true;
								if (codeL[i - 2] != '\\' && (str = !str) == false && wasString)
									return false; // found end of string
								break;
							case '/': // comment
								if (mustBeNext && codeL[i] != '/' && skip == 0 && !comment && !str)
									return true;
								bool endComment = next > 0 && codeL[next - 1] == '*', newComment = next < codeL.Length - 1 && codeL[next + 1] == '*';
								//if (newComment) ++next;
								Eat();
								GetChar('/', out commDash);
								if (str)
									break; // inside a string, doesn't count
								if (!(comment = newComment || !endComment) && wasComment) {
									AddColor(i, ParseDictionary.Type.Text);
									return false; // found the end of the comment
								}
								if (!comment && skip == 0 && (!newComment || endComment))
									return true; // not skipping anything and it's not a begin comment

								break;
							case '{':
								if (mustBeNext && codeL[i] != '{' && skip == 0 && !comment && !str)
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
								if (mustBeNext && codeL[i] != '}' && skip == 0 && !comment && !str)
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
								if (next != 0 && skip == 0 && !comment && !str)
									return true; // didn't find it
								return true; // found something wrong

							}
						}
						void Eat() {
							//codeLine = codeLine[(++from)..];
							int fromS = i;
							i = next + 1;
							TrimStart(codeL, ref i);
							int plus = i - fromS;
							if (braStart != int.MaxValue) braStart -= plus;
							if (braEnd != int.MaxValue) braEnd -= plus;
							if (commDash != int.MaxValue) commDash -= plus;
							if (search != int.MaxValue) search -= plus;
							if (strMark != int.MaxValue) strMark -= plus;
						}
						return true; // didn't find it
					}
					void GetChar(char c, out int location, string txt = "") {
						if ((location = (txt.Length == 0 ? codeL : txt).IndexOf(c)) < 0) location = int.MaxValue;
				}
			}
		}
	}
	#endregion
	
	#region Generic Interface
	public Expression T_Parse(string text, int from, Value? args = null) => new(this, text, args ?? None, from);
	public Value T_Eval(Expression exp, Value? args = null) => exp.Eval(0, args ?? None);
	public Value T_ParseEval(string text, int from, out Expression expr, Value? args = null) { var e = T_Parse(text, from, args); expr = e; return e.Eval(0, args ?? None); }
	public Value T_ParseEval(string text, ref int from, Value? args = null) => new Expression(this, text, ref from, out _, args ?? None).Eval(0, args ?? None);
	public Value T_ParseEval(string text, int from, Value? args = null) => T_ParseEval(text, ref from, args);
	public Value MakeArgs((string alias, T value)[] pairs) => new(pairs.Select(p => new Value(p.value, 0, p.alias)).ToArray());
	#endregion

	#region Content
	public bool _darkMode = true;
	private Color GetColor(ParseDictionary.Type type) => _darkMode ? _darkColors[type] : _lightColors[type];

	private readonly Dictionary<ParseDictionary.Type, Color> _darkColors = new() {
		[ParseDictionary.Type.Do] = Color.FromArgb(255, 255, 128),
		[ParseDictionary.Type.UserF] = Color.FromArgb(128, 128, 255),
		[ParseDictionary.Type.DefaultF] = Color.Blue,
		[ParseDictionary.Type.Arg] = Color.Yellow,
		[ParseDictionary.Type.UserC] = Color.LightGreen,
		[ParseDictionary.Type.DefaultC] = Color.Green,
		[ParseDictionary.Type.Number] = Color.FromArgb(192, 0, 192),
		[ParseDictionary.Type.Text] = Color.White,
		[ParseDictionary.Type.Comment] = Color.Gray,
		[ParseDictionary.Type.String] =  Color.FromArgb(255, 128, 255),
		[ParseDictionary.Type.Error] = Color.Red
	};
	private readonly Dictionary<ParseDictionary.Type, Color> _lightColors = new() {
		[ParseDictionary.Type.Do] =  Color.FromArgb(192, 192, 0),
		[ParseDictionary.Type.UserF] = Color.Blue,
		[ParseDictionary.Type.DefaultF] = Color.DarkBlue,
		[ParseDictionary.Type.Arg] = Color.FromArgb(128, 128, 0),
		[ParseDictionary.Type.UserC] = Color.Green,
		[ParseDictionary.Type.DefaultC] = Color.DarkGreen,
		[ParseDictionary.Type.Number] = Color.FromArgb(64, 0, 64),
		[ParseDictionary.Type.Text] = Color.Black,
		[ParseDictionary.Type.Comment] = Color.Gray,
		[ParseDictionary.Type.String] = Color.Purple,
		[ParseDictionary.Type.Error] = Color.Red
	};
	
	public class ParseDictionary(ParseDictionary? parent = null, int depth = 0) {
		public struct S(object obj, Type type) {
			public readonly object Obj = obj;
			public readonly Type Type = type;
		}
		public enum Type : byte {
			//NameSpace = 1 << 0,	// obj = Package pkg, with constants and functions | bitmap 
			Do = 1 << 0,			// obj = (byte)CodeCall callIndex | print, do...
			UserF = 1 << 1,			// obj = (Dictionary<CallFunction> )
			DefaultF = 1 << 2,		// obj = (Dictionary<CallFunction> )
			Arg = 1 << 3,			// obj = (Value container, int index) index | arguments
			UserC = 1 << 4,			// obj = (Value value) | arguments, user constants, default constants and generic constants
			DefaultC = 1 << 5,		// obj = (Value value) | arguments, user constants, default constants and generic constants
			Number = (1 << 5) + 1,	// not dictionary, just for parsing colors
			Text = (1 << 5) + 2,	// not dictionary, just for parsing colors
			Comment = (1 << 5) + 3,	// not dictionary, just for parsing colors
			String = (1 << 5) + 4,	// not dictionary, just for parsing colors
			Error = (1 << 5) + 5	// not dictionary, just for parsing colors
			
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
		/// <param name="types">types I'm interested in (will stop going deeper, if _children don't contain anymore)</param>
		/// <returns>all found entries sorted from largest</returns>
		public List<(string name, S obj)> Get(string text, byte types) {
			var nest = this;
			var c = 0;
			List<(string name, S obj)> r = [];
			while ((nest = nest.GetNext(text, types, out var name, out var addList, ref c)) != null) 
				foreach (var a in addList)
					if (((byte)a.Type & types) > 0)
						r.Add((name, a));
			return r;
		}
		// gets the longest matching list of names first, then if none of them fit.
		// If asked again with the same nest and c, it will return it's smaller parent, and until all possible matches have been depleted
		private ParseDictionary? GetNext(string text, byte types, out string name, out List<S> d, ref int c) {
			if (c < 0) {
				(name,d) = parent == null ? ("",[]) : (text[..(depth - 1)], parent._d);
				return parent;
			}
			if ((types & _deeper) > 0 && text.Length > c && _next.TryGetValue(text[c], out var n)) {
				++c;
				return n.GetNext(text, types, out name, out d, ref c);
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
	private static readonly Cf Fact = new(T.Factorial, OpCode.Factorial);
	protected abstract Value GenericConstants();
	protected readonly ParseDictionary Context = new();
	#endregion
	
	#region Helpers
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
		
		void C(string name, T v) => Context.Insert(new(new Value(v, 0, name), ParseDictionary.Type.DefaultC), name);
		C("pi", INumber<T>.C_Pi());
		C("tau", INumber<T>.C_Tau());
		C("e", INumber<T>.C_E());
		C("gamma", INumber<T>.C_Gamma());
		C("one", T.One());
		foreach(var d in GenericConstants().Values)
			C(d.String, d.Leaf);
		
		void A(string name, CallFunction c) => Context.Insert(new(c, ParseDictionary.Type.DefaultF), name);
		CallFunction min, max, mul, prod, vec, ln, nsinhc, nsinc, re, im, sign, neg, inv, abs, conj, compMod, cub, cabs, trunc, sinhc, ceil;
		// meta
		A("eval", new Ce(typeof(FuncEval), 0)); // attempts to parse and evaluate every Text in the input
		A("count", new Ce(typeof(FuncCount), 0)); // counts the number of elements in the vector
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
		A("sum", new Ce(typeof(Sum), 0)); // iterative sum
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
		A("sign", sign = new Cf(INumber<T>.Sign, OpCode.Sgn)); // = z/|z|
		A("sgn", sign); // = z/|z|
		A("negative", neg = new Cf(INumber<T>.Neg, OpCode.Neg)); // = -z
		A("neg", neg); // = -z
		A("inverse", inv = new Cf(T.Inv, OpCode.Inv)); // = 1/z
		A("inv", inv); // = 1/z
		A("absri", cabs = new Cf(T.AbsComp, OpCode.Absri)); // component-abs: absri(a+bi) = |a|+|b|i
		A("compabs", cabs); // component-abs
		A("cabs", cabs); // component-abs
		A("sqrabs", new Cf(INumber<T>.SqrAbs, OpCode.SqrAbs)); // = |z|^2; sqrabs(a+bi) = a^2+b^2
		A("absolute", abs = new Cf(INumber<T>.T_Abs, OpCode.Abs)); // = |z|
		A("abs", abs); // = |z|
		A("norm", abs); // = |z|
		A("arg", new Cf(INumber<T>.T_Arg, OpCode.Arg)); // argument, the angle from (0,0). arg(-1)=pi
		A("conjugate", conj = new Cf(INumber<T>.Conj, OpCode.Conj));
		A("conj", conj); // conjugate: negates all imaginary units, conj(r+ai+bj+dk) = r-ai-bj-bk
		// powers
		A("sqrt", new Cf(T.Sqrt, OpCode.Sqrt)); // square root = z^(1/2)
		A("sqr", new Cf(T.Sqr, OpCode.Sqr)); // square = z^2
		A("cbrt", new Cf(INumber<T>.Cbrt, OpCode.Cbrt)); // cube root = z^(1/3)
		A("cube", cub = new Cf(T.Cub, OpCode.Cub)); // cube = z^3
		A("cub", cub); // cube
		A("quart", new Cf(T.Quart, OpCode.Quart)); // z^4

		// specials
		A("fact", Fact); // factorial
		A("factorial", Fact); // factorial
		A("gauss", new Cf(T.Gauss, OpCode.Gauss)); // gauss e^(-z^2)
		A("gamma", new Cf(T.Gamma, OpCode.Gamma)); // gamma function = (xz1)!
		A("zeta", new Cf(T.Zeta, OpCode.Zeta)); // riemann zeta function
		A("softabs", new Cf(INumber<T>.SoftAbs, OpCode.SoftAbs)); // = e^(1+ln(z))
		A("softneg", new Cf(INumber<T>.SoftNeg, OpCode.SoftNeg)); // = e^(1+ln(z))
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