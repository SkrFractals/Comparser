using Comparser.Comparser.Numbers;
namespace Comparser.Comparser;
public abstract partial class Comparser<T> where T : unmanaged, INumber<T> {
	public partial class GpuValue {
		private static readonly GpuValue Ln2 = new(T.MakeR(Math.Log(2))),
			Ln10 = new(T.MakeR(Math.Log(10))),
			Iln2 = new(T.MakeR(1.0/Math.Log(2))),
			Iln10 = new(T.MakeR(1.0/Math.Log(10))),
			QTau = new(T.MakeR(Math.PI / 2));
		
		public bool ParseByteCode(CancellationToken cancel, Comparser<T> context, out string print, out byte[] code, bool getCode = true, bool getPrint = false) {
			
			List<byte> header = []; // header string
			WriteInt(INumber<T>.ToBytes(T.Zero()).Length, header); // leaf size -> header
			List<List<byte>> funcCodes = [header]; // first slot is reserved for the function counter
			List<string[]> funcPrints = []; // print function definitions
			Dictionary<CallCustom, int> calls = []; // found custom function recalls
			List<CallCustom> newCalls = []; // found custom function calls for parsing
			int parsedCalls = 0; // already parsed newCalls -> funcPrints + funcCodes
			
			#region Subtitution Custom Functions
			// replacement custom functions (those that branch or use the x argument multiple times):
			Value x = new([new(T.NaN(), 0, "x")]), // passable argument x
				x0 = new([new(T.Zero(), 0, "x")]); // pattern match x as 0
			CallCustom sinc = new([(x0, new(new(context, "1", cancel), out _, None), null), 
					(x, new(new(context, "sin(x)/x", cancel), out _, x), null)]),
				// alternative using condition instead of pattern matching: var sinc = new CallCustom([(x, new(context, "sin(x)/x", x), new(context, "x==0", x))]);
				nsinc = new([(x0, new(new(context, "1", cancel), out _, None), null), 
					(x, new(new(context, "sin(pix)/(pix)", cancel), out _, x), null)]),
				sinhc = new([(x0, new(new(context, "1",  cancel), out _,None), null), 
					(x, new(new(context, "sinh(x)/x",  cancel), out _,x), null)]),
				nsinhc = new([(x0, new(new(context, "1",  cancel), out _,None), null),
					(x, new(new(context, "sinh(pix)/(pix)", cancel), out _, x), null)]),
				cosc = new([(x0, new(new(context, "0", cancel), out _, None), null), 
					(x, new(new(context, "(1-cos(x))/x",  cancel), out _,x), null)]),
				ncosc = new([(x0, new(new(context, "0", cancel), out _, None), null),
					(x, new(new(context, "(1-cos(pix))/(pix)", cancel), out _, x), null)]),
				coshc = new([(x0, new(new(context, "0", cancel), out _, None), null),
					(x, new(new(context, "(1-cosh(x))/x", cancel), out _, x), null)]),
				ncoshc = new([(x0, new(new(context, "0",  cancel), out _,None), null),
					(x, new(new(context, "(1-cosh(pix))/(pix)", cancel), out _, x), null)]),
				tanh = new([(x, new(new(context, "sinh(x)/cosh(x)",  cancel), out _,x), null)]),
				coth = new([(x, new(new(context, "cosh(x)/sinh(x)", cancel), out _, x), null)]),
				tan = new([(x, new(new(context, "sin(x)/cos(x)",  cancel), out _,x), null)]),
				cot = new([(x, new(new(context, "cos(x)/sin(x)",  cancel), out _,x), null)]),
				frac = new([(x, new(new(context, "x-trunc(x)", cancel), out _, x), null)]),
				sgn = new([(x0, new(new(context, "0", cancel), out _, None), null), 
					(x, new(new(context, "x/abs(x)", cancel), out _, x), null)]),
				immag = new([(x, new(new(context, "abs(x-re(x))",  cancel), out _,x), null)]),
				sqr = new([(x, new(new(context, "xx", cancel), out _, x), null)]),
				cub = new([(x, new(new(context, "xxx", cancel), out _, x), null)]),
				quart = new([(x, new(new(context, "sqr(sqr(x))",  cancel), out _,x), null)]);
			#endregion
			
			#region Parse
			bool success = true;
			string printE = "", printF = "";
			try {
				code = getCode ? MakeByteCode() : [];
				if (getPrint) {
					PrintByteCodeR("", this, ref printF);
					PrintFunctions();
				}
			} catch (Exception) {
				code = [];
			} finally {
				print = printF + printE; // functions that the expression is calling, and the expression itself (reversed order, so the functions the expression is dependent on are printed before it)
			}
			return success;
			#endregion
			
			byte[] MakeByteCode() {
				List<byte> expByte = []; // specify the byte size of the numbers (real vs complex vs quaternion)
				MakeByteCodeS(this);
				
				WriteInt(funcCodes.Count, funcCodes[0]); // finally write the function counter, now that we are really finished
				int c = 0, funcLength = 0; // final byte indexer, final funcCode total length
				foreach (var i in funcCodes) funcLength += i.Count;
				byte[] finalBytes = new byte[expByte.Count + funcLength];
				foreach (var code in funcCodes)
				foreach (var i in code)
					finalBytes[c++] = i; // write function definition bytes (before the expression that ws parsed first, so the functions the expression is dependent on are printed before it)
				foreach (var i in expByte)
					finalBytes[c++] = i; // write expression bytes
				return finalBytes;
				
				void MakeByteCodeS(GpuValue v) {
					GpuValue? s = v._op switch { // substitute
						OpCode.More => new(OpCode.Neg, new(OpCode.Less, new(OpCode.Neg, v))),
						OpCode.MoreEqual => new(OpCode.Neg, new(OpCode.LessEqual, new(OpCode.Neg, v))),
						OpCode.Min => new(OpCode.Neg, new(OpCode.Max, new(OpCode.Neg, v))),
						OpCode.SoftMin => new(OpCode.Neg, new(OpCode.SoftMax, new(OpCode.Neg, v))), 
						OpCode.SoftAbs => new([new(T.Zero()), v], OpCode.SoftMax), 
						OpCode.SoftNeg => new([new(T.Zero()), v], OpCode.SoftMin), 
						OpCode.Exp10 => new(OpCode.Exp, new([Ln10, v], OpCode.Mul)),
						OpCode.Exp2 => new(OpCode.Exp, new([Ln2, v], OpCode.Mul)), 
						OpCode.Log10 => new([new(OpCode.Log, v), Iln10], OpCode.Mul),
						OpCode.Log2 => new([new(OpCode.Log, v), Iln2], OpCode.Mul),
						OpCode.Sinc => new(OpCode.Call, v, sinc),
						OpCode.Nsinc => new(OpCode.Call, v, nsinc),
						OpCode.Sinhc => new(OpCode.Call, v, sinhc),
						OpCode.Nsinhc => new(OpCode.Call, v, nsinhc),
						OpCode.Cosc => new(OpCode.Call, v, cosc),
						OpCode.Ncosc => new(OpCode.Call, v, ncosc),
						OpCode.Coshc => new(OpCode.Call, v, coshc),
						OpCode.Ncoshc => new(OpCode.Call, v, ncoshc),
						OpCode.Tanh => new(OpCode.Call, v, tanh),
						OpCode.Coth => new(OpCode.Call, v, coth),
						OpCode.Sech => new(OpCode.Inv, new(OpCode.Cosh, v)),
						OpCode.Csch => new(OpCode.Inv, new(OpCode.Sinh, v)),
						OpCode.Tan => new(OpCode.Call, v, tan),
						OpCode.Cot => new(OpCode.Call, v, cot),
						OpCode.Sec => new(OpCode.Inv, new(OpCode.Cos, v)),
						OpCode.Csc => new(OpCode.Inv, new(OpCode.Sin, v)),
						OpCode.Asech => new(OpCode.Acosh, new(OpCode.Inv, v)),
						OpCode.Acsch => new(OpCode.Asinh, new(OpCode.Inv, v)),
						OpCode.Asin => new([QTau, new(OpCode.Neg, new(OpCode.Acos, v))], OpCode.Add),
						OpCode.Acot => new([QTau, new(OpCode.Neg, new(OpCode.Atan, v))], OpCode.Add),
						OpCode.Asec => new(OpCode.Acos, new(OpCode.Inv, v)),
						OpCode.Acsc => new(OpCode.Asin, new(OpCode.Inv, v)),
						OpCode.ImMag => new(OpCode.Call, v, immag),
						OpCode.ImCoef => new(OpCode.Call, new(OpCode.Re, new(OpCode.Neg, new(OpCode.Mul,v)))),
						OpCode.Frac => new(OpCode.Call, v, frac),
						OpCode.Ceil => new(OpCode.Neg, new(OpCode.Floor, new(OpCode.Neg, v))),
						OpCode.Sgn => new(OpCode.Call, v, sgn),
						OpCode.Sqrt => new([v, new(T.MakeR(.5))], OpCode.Pow),
						OpCode.Sqr => new(OpCode.Call, v, sqr),
						OpCode.Cbrt => new([v, new(T.MakeR(1.0/3))], OpCode.Pow),
						OpCode.Cub => new(OpCode.Call, v, cub),
						OpCode.Quart => new(OpCode.Call, v, quart),
						OpCode.Gauss => new(OpCode.Exp, new(OpCode.Neg, new(OpCode.Sqr, v))),
						_ => null
					};
					if (s != null) { // substituted
						v._op = OpCode.Nop; // neutralize the substituted OpCode
						MakeByteCodeS(s); // substitution has been done, try again (to substitute further if there are parts to be substituted again)
						return;
					}
					if (v._op == OpCode.NotAvailable) {
						success = false;
						return;
					}
					MakeByteCodeR(v, expByte); // didn't substitute, proceed to make the byte code
				}
			}
			string PrintByteCodeR(string ind, GpuValue v, ref string p) {
				p += ind + "OP " + v._op + "\n";
				if (v._def != null)
					p += "FUNCTION " + CollectCall(v._def);
				ind += " "; // add indent for this op/call
				switch (v.Values.Length) {
				case 0: // Leaf/Argument
					return p + (v._arg.Length == 0 ? ind + "LEAF " + v._leaf : ind + "ARG " + v._arg) + "\n";
				case 1: // CollapseScalar
					return PrintByteCodeR(ind , v.Values[0], ref p); 
				default:
					p += ind + "VECTOR " + v.Values.Length + " elements" + "\n";
					foreach (var i in v.Values)
						PrintByteCodeR(ind + " ", i, ref p);
					return p;
				}
			}
			void PrintFunctions() {
				printF = "FUNCTIONS " + funcPrints.Count + "\n";
				for (int f = 0; f < funcPrints.Count; ++f) {
					printF += "FUNCTION " + f + ":" + funcPrints[f].Length + " DEFINITIONS" + "\n"; // FUNCTION<index>: definitions count
					foreach (var s in funcPrints[f])
						printF += s; // print all the pre-parsed definitions
				}
			}
			int CollectCall(CallCustom v) {
				if (calls.TryGetValue(v, out var index))
					return index;
				calls[v] = index = calls.Count;
				newCalls.Add(v);
				// prepare the slots for the parses before the recursion could start doing more of them mid this call
				funcPrints.Add(new string[v.Def.Length]); 
				funcCodes.Add([]);
				while (parsedCalls < newCalls.Count) {
					var i = parsedCalls++;
					var def = newCalls[i].Def;
					var code = funcCodes[i + 1]; // funcCodes[0] is the global header; function N is stored at N+1!
					if (getCode) //WriteInt(i, code); // function call index (not needed since I finally write them in order)
						WriteInt(def.Length, code); // expected definitions count
					//int argDegs = 0;
					for (var d = 0; d < def.Length; ++d) {
						var dd = def[d];
						var input = Expression.GpuParseValue(0, dd.input);
						MakeArgDefs(input);
						var definition = dd.def.GpuParse(0);
						var condition = dd.condition?.GpuParse(0);
						if (getCode) {
							MakeByteCodeR(input, code); // input (argument pattern matching)
							if (condition == null) code.Add(255); // 255 byte implies no condition
							else {
								code.Add(0);
								MakeByteCodeR(condition, code); // condition
							}
							MakeByteCodeR(definition, code); // definition
						}
						if (!getPrint)
							continue;
						var fs = " PATTERN" + d;
						PrintByteCodeR(" ", input, ref fs);
						if (condition != null) {
							fs += " CONDITION" + d + "\n";
							PrintByteCodeR(" ", condition, ref fs); // no condition = (1), otherwise print it
						}
						fs += " DEF" + d + "\n";
						PrintByteCodeR(" ", definition, ref fs);
						funcPrints[index][d] = fs; // array was resized before the possible recursions started, this slot is ready
						continue;
						
						void MakeArgDefs(GpuValue recurse) {
							if (recurse.Values.Length > 0) {
								foreach (var t in recurse.Values)
									MakeArgDefs(t);
								return;
							}
							if (!recurse.IsNaN() || recurse.Operand == null)
								return;
							recurse._def = new([(dd.input, recurse.Operand, null)]);
						}
					}
				}
				return index;
			}
			void MakeByteCodeR(GpuValue v, List<byte> b) {
				while (true) {
					if (v._def != null) {
						b.Add((byte)(v._op == OpCode.Call ? v._op : OpCode.DefaultArg)); // OpCode CALL/ITERATOR
						WriteInt(CollectCall(v._def), b); // FUNCTION INDEX
					}
					switch (v.Values.Length) {
					case 0: // Leaf/Argument
						if (v._arg.Length == 0) {
							b.Add((byte)OpCode.Leaf); // OpCode LEAF
							foreach (var i in INumber<T>.ToBytes(v._leaf))
								b.Add(i); // leaf bytes (expectation set at the very first byte of the bytecode, before the function definitions)
							return;
						}
						b.Add((byte)OpCode.Argument); // OpCode ARGUMENT
						WriteInt(v._arg.Length, b); // how many argument nests to expect
						foreach (var i in v._arg)
							WriteInt(i, b); // nests
						return;
					case 1: // CollapseScalar
						v = v.Values[0];
						continue;
					default: // Vector
						b.Add((byte)OpCode.Vector); // OpCode VECTOR
						WriteInt(v.Values.Length, b); // expected vector elements
						foreach (var i in v.Values)
							MakeByteCodeR(i, b); // print each element
						return;
					}
				}
			}
			// new: i <= 251 ? [i] : [256-bytes, byte[bytes-1], byte[bytes-2],...,byte[0]] 
			void WriteInt(int i, List<byte> b) {
				if (i < 0) throw new("WriteInt only takes non-negative integers!");
				List<byte> iB = [];
				do { iB.Add((byte)(i & 255)); i >>= 8; } while (i > 255);
				var c = (byte)iB.Count;
				if (c != 1 || iB[0] > 251) {
					b.Add((byte)(256 - c)); // write decremented c (so it nicely fits into 0-3 and can be later checked with one inequality)
					while(0 < c--) b.Add(iB[i]); // in reverse order, so the highest significant bits come first
				} else b.Add(iB[0]);
			}
		}
	}
}
