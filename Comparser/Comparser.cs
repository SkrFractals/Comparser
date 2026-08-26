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

public abstract partial class Comparser<T>(ushort stackOverflowLimit = 999, ushort doOverflowLimit = 999, ushort loopLimit = 999, ushort iteratorLimit = 999) : IComparser where T : unmanaged, INumber<T> {
	
	#region Interface
	private static Value AsInput(object? e) => e as Value ?? new();
	public object Parse(string text, object? args) => new Expression(this, text, AsInput(args));
	public object Eval(object exp, object? args) => (exp as Expression ?? new Expression(this, "", None)).Eval(0, AsInput(args));
	public object ParseEval(string text, out object expr, object? args) { var e = (Expression)Parse(text, args); expr = e; return e.Eval(0, AsInput(args)); }
	public object ParseEval(ref string text, object? args) => new Expression(this, ref text, out _, AsInput(args)).Eval(0, AsInput(args));
	public object ParseEval(string text, object? args) => ParseEval(ref text, args);
	public string ToString(object value, int decimals = -1) => AsInput(value).ToString(decimals);
	public string ReadCode(string text) {
		Dictionary<string, List<(Value input, Expression def, Expression? cond)>> parsedF = [];
		List<Value> parsedC = [];
		bool skipIf = false;
		int returnIndex = -1, loops = 0, i, skip = 0;
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
					ReadLine();
				continue;

				void ReadLine() {
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
								if (--skip == 0) {
									// TODO enter the else branch if skipIf and :	
								} else minus = codeLine.IndexOf('{');
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
					// read format: (<cacheIntExpression>)functionName(<argumentsExpression>)=<definitionExpression>

					int cache = 1;
					string e, name = "";
					Value eval;
					switch (codeLine[0]) {
						case '/': // Comment
							if (codeLine.Length > 1 && codeLine[1] == '*') 
								comment = true;
							else
								Cl();
							return;
						case '}': // this must be and out of a branch I'm in, just ignore it
							if (returnIndex >= 0) {
								if (++loops > _loop) {
									e = "WHILE loop limit exceeded.";
									Lg();
									loops = 0;
									goto case ';';
								}
								skipIf = false;
								codeLine = code[i = returnIndex];
								returnIndex = -1;
								return;
							}
							loops = 0;
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
							skipIf = loops == 0;
							goto case '{';
						case ':': // got here from } so it must be an else we will skip:
							if (skipIf) {
								while ((minus = codeLine.IndexOf('{')) < 0)
									codeLine = code[++i];
								codeLine = codeLine[(minus + 1)..];
								return;
							} goto case '{';
						case '{':
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
							
							int l;
							string s, s1;
							var sc = codeLine;
							sc = Clean(sc).Replace(" ", "");//.TrimEnd(' ');
							if ("" != (e = (l = sc.IndexOf('(')) < 0
									? IsAlphaNumeric(name = sc) ? name != "" ? "" : "No name." : Fn() // constant
									: l < 1 ? "No name."
									//: sc.Length <= l + 2 /*|| sc[^1] != ')'*/ ? "There must be arguments and a closing parenthesis between the opening parenthesis and the colon sign."
									: IsAlphaNumeric(name = sc[..l]) ? "" : Fn())) {
								(s, s1) = (sc, "");
								Err();
								return;
							}
							if (l > 0) {
								sc = sc[l..];
								if ("" != (e = FailArgs(ref sc, out args) ? "Failed to parse arguments." : (sc = sc.TrimEnd(' ')).Length == 0 || sc[0] != ':' ? "Missing definition colon." : "")) {
									(s, s1) = (sc, "");
									Err();
									return;
								}
							}
							if ((minus = sc.IndexOf(':')) < 0) {
								e = "There must be a colon sign between the name and definition.";
								Lg();
								Cl();
								return;
							}
							(s, s1) = (sc[..minus], sc[(minus + 1)..]);
							if (args.Values.Length == 0) { // it is a constant or code call:
								if (cache != 1) {
									e = name + " is a constant/call and doesn't support caching.";
									Lg();
								}
								eval = new Expression(this, s1, args).Eval(0, None, name);
								switch (name) {
								case "print":
									log += eval + "\n";
									End();
									return;
								case "do":
									var cl = codeLine;
									if (pref.Length > _doOverflow) {
										e = "DO overflow limit exceeded.";
										Lg();
									} else {
										var expand = eval.ToLines().Split('\n'); // ToString(true) recursively exports only string values as lines
										ReadLines(pref + (i+1) + "/", expand);
										codeLine = cl;
									}
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
								End();
								return;
							} // it is a function:
							Expression defaultBranch = new(this, ref s1, out _, args, cache);
							s1 = s1.TrimStart(' '); 
							for (List<(Expression, Expression)> conditionals = []; s1.Length > 0 && s1[0] == '?'; 
								s1 = s1[1..].TrimStart(' '), defaultBranch = new(this, ref s1, out _, args, cache)) {
								//Expression cond = defaultBranch;
								s1 = s1[1..].TrimStart(' ');
								Expression trueExp = new(this, ref s1, out _, args, cache);
								if ((s1 = s1.TrimStart(' '))[0] != ':') {
									AddF(args, conditionals);
									return;
								}
								conditionals.Add((trueExp, defaultBranch));
							}
							AddF(args, [], defaultBranch); // no if
							return;
							
							void AddF(Value input, List<(Expression ifTrue, Expression question)> conditional, Expression? dBranch = null) {
								if (parsedF.TryGetValue(name, out var pfn)) {
									for(var m = pfn.Count; 0 <= --m;)
										if (pfn[m].input.SameArg(input))
											pfn.RemoveAt(m); // already have the definition with the same arguments - mutate it
									//pfn.Add(parsedF[input]); 
								} else pfn = parsedF[name] = [];
								foreach (var condI in conditional)
									pfn.Add((args,condI.ifTrue, condI.question));
								if(dBranch != null)
									pfn.Add((args, dBranch, null));
								// create/update the compiled function:
								if (_customFunctions.TryGetValue(name, out var c) && c is CallCustom cf)
									cf.Def = [.. parsedF[name]];
								else _customFunctions[name] =  new CallCustom([.. parsedF[name]]);
								End();
							}
							void Err() { Lg(); End(); }
							void End() => codeLine = (minus = s.IndexOf(';')) >= 0 ? s[(minus + 1)..] + s1 : (minus = s1.IndexOf(';')) >= 0 ? s1[(minus + 1)..] : "";
							void Lg() {
								log += "Line " + pref + (i + 1) + ": " + e + "\n";
								e = "";
							}
					}
					return;
					
					bool FailEval(ref string exp, out Value evaluated, char close = ')') {
						exp = exp[1..]; // eat initial symbol that triggered it
						evaluated = UnCollapseScalar(new Expression(this, ref exp, out _, None).Eval(0, None));
						if (exp.Length == 0 || exp[0] != close)
							return true; // closing parenthesis after the argument expression
						exp = exp[1..];
						return false;
					}
					void Cl() => codeLine = (minus = codeLine.IndexOf(';')) < 0 ? "" : codeLine[(minus + 1)..];
					string Fn() => "Failed to parse name: " + name;
					bool FailArgs(ref string exp, out Value args) => FailEval(ref exp, out args) || FailArgValues(args.Values); // || FailArgTerms(args.Values);
					static bool IsAlphaNumeric(string strToCheck) => MyRegex().IsMatch(strToCheck);
					static string Clean(string t) // forbidden symbols in expressions
						=> t.ToLower()//.Replace(":", "").Replace(";", "").Replace("|", "")
						.Replace("\t", "").Replace("\r", "").Replace("\n", "");
					bool FailArgValues(Value[] v) {
						if (v.Length == 0) return true;
						var fail = false;
						foreach (var iv in v)
							fail |= iv.Values.Length > 0
								? FailArgValues(iv.Values)
								: iv.Leaf.IsNaN() && (iv.Text == "" || !IsAlphaNumeric(iv.Text));
						return fail;
					}
				}
			}
		}
	}
	#endregion
	
	#region Generic Interface
	public Expression T_Parse(string text, Value? args = null) => new(this, text, args ?? None);
	public Value T_Eval(Expression exp, Value? args = null) => exp.Eval(0, args ?? None);
	public Value T_ParseEval(string text, out Expression expr, Value? args = null) { var e = T_Parse(text, args); expr = e; return e.Eval(0, args ?? None); }
	public Value T_ParseEval(ref string text, Value? args = null) => new Expression(this, ref text, out _, args ?? None).Eval(0, args ?? None);
	public Value T_ParseEval(string text, Value? args = null) => T_ParseEval(ref text, args);
	public Value MakeArgs((string alias, T value)[] pairs) => new(pairs.Select(p => new Value(p.value, p.alias)).ToArray());
	#endregion

	#region Content
	private readonly ushort _stackOverflow = stackOverflowLimit, _doOverflow = doOverflowLimit, _loop = loopLimit, _iterLimit = iteratorLimit;
	static protected readonly Value None = new();
	private Dictionary<string, CallFunction> _customFunctions = []; // user-defined functions
	private Value _customConstants = None; // user-defined constants
	private static readonly Cf Fact = new(T.Factorial, OpCode.Factorial);
	protected abstract Value GenericConstants();
	#endregion
	
	#region Helpers
	private static Value CollapseScalar(Value i) {
		while (i.Values.Length == 1)
			i = i.Values[0];
		return i;
	}
	private static Value UnCollapseScalar(Value i) {
		if (i.Values.Length == 0)
			i.Values = [new(i.Leaf, i.String) {Operand = i.Operand}];
		return i;
	}
	//private static int SafeCollapse(Value[] v, int i) => v.Length == 0 ? 0 : (v[i] = CollapseScalar(v[i])).Values.Length;
	//private static T SafeLeaf(Value[] v, int i) => v.Length == 0 ? T.NaN() : v[i].Leaf;
	//private static string SafeText(Value[] v, int i) => v.Length == 0 ? "" : v[i].Text;
	[GeneratedRegex(@"^[a-zA-Z0-9\s,]*$")]
	private static partial Regex MyRegex();
	#endregion
	
	
}
public class ComparserR : Comparser<Real> { override protected Value GenericConstants() => None; }
public class ComparserC : Comparser<Complex> { override protected Value GenericConstants() => new([new(Complex.i, "i")]); }
public class ComparserQ : Comparser<Quaternion> { override protected Value GenericConstants() => new([new(Quaternion.i, "i"), new(Quaternion.j, "j"), new(Quaternion.k, "k")]); }

