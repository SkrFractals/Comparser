using Comparser.Comparser.Numbers;
using static Comparser.Comparser.Numbers.ILeaf;
namespace Comparser.Comparser;
public /*abstract*/  partial class Comparser/*<ILeaf> where ILeaf : unmanaged, IScalar<ILeaf>*/{
	public partial class GpuValue {
		private static readonly GpuValue Ln2 = new((Real)(Math.Log(2))),
			Ln10 = new((Real)Math.Log(10)),
			Iln2 = new((Real)(1.0/Math.Log(2))),
			Iln10 = new((Real)(1.0/Math.Log(10))),
			QTau = new((Real)(Math.PI / 2));

		private class Subs(Comparser/*<T>*/ context, CancellationToken cancel) {
			private static readonly Value 
				X0 = new([new(zero, 0, "x")]), // pattern match x as 0
				X = new([new(nan, 0, "x")]), // passable argument x
				Xy = new([new(nan, 0, "x"), new(nan, 0, "y")]), // passable xy
				Xyz = new([new(nan, 0, "x"), new(nan, 0, "y"), new(nan, 0, "z")]), // passable xyz
				Xyzw = new([new(nan, 0, "x"), new(nan, 0, "y"), new(nan, 0, "z"), new(nan, 0, "w")]); // passable xyzw
			public readonly Comparser/*<T>*/ Context = context;
			public readonly CallCustom Sinc = new([(X0, new(new(context, "1", cancel), out _, None), null), 
					(X, new(new(context, "sin(x)inv(x)", cancel), out _, X), null)]),
				// alternative using condition instead of pattern matching: var sinc = new CallCustom([(x, new(context, "sin(x)/x", x), new(context, "x==0", x))]);
				Nsinc = new([(X0, new(new(context, "1", cancel), out _, None), null), 
					(X, new(new(context, "sin(pix)inv(pix)", cancel), out _, X), null)]),
				Sinhc = new([(X0, new(new(context, "1",  cancel), out _,None), null), 
					(X, new(new(context, "sinh(x)inv(x)",  cancel), out _,X), null)]),
				Nsinhc = new([(X0, new(new(context, "1",  cancel), out _,None), null),
					(X, new(new(context, "sinh(pix)inv(pix)", cancel), out _, X), null)]),
				Cosc = new([(X0, new(new(context, "0", cancel), out _, None), null), 
					(X, new(new(context, "(1+neg(cos(x)))/x",  cancel), out _,X), null)]),
				Ncosc = new([(X0, new(new(context, "0", cancel), out _, None), null),
					(X, new(new(context, "(1+neg(cos(pix)))inv(pix)", cancel), out _, X), null)]),
				Coshc = new([(X0, new(new(context, "0", cancel), out _, None), null),
					(X, new(new(context, "(1+neg(cosh(x)))inv(x)", cancel), out _, X), null)]),
				Ncoshc = new([(X0, new(new(context, "0",  cancel), out _,None), null),
					(X, new(new(context, "(1+neg(cosh(pix)))inv(pix)", cancel), out _, X), null)]),
				Tanh = new([(X, new(new(context, "sinh(x)inv(cosh(x))",  cancel), out _,X), null)]),
				Coth = new([(X, new(new(context, "cosh(x)inv(sinh(x))", cancel), out _, X), null)]),
				Tan = new([(X, new(new(context, "sin(x)inv(cos(x))",  cancel), out _,X), null)]),
				Cot = new([(X, new(new(context, "cos(x)inv(sin(x))",  cancel), out _,X), null)]),
				Frac = new([(X, new(new(context, "x+neg(trunc(x))", cancel), out _, X), null)]),
				Cycle = new([(X, new(new(context, "x+neg(floor(x))", cancel), out _, X), null)]),
				Sgn = new([(X0, new(new(context, "0", cancel), out _, None), null), 
					(X, new(new(context, "xinv(abs(x))", cancel), out _, X), null)]),
				Clamp = new([(Xyz, new(new(context, "min(max(x,y),z)",  cancel), out _, Xyz), null)]),
				Lerp = new([(Xyz, new(new(context, "x(1-z)+yz",  cancel), out _, Xyz), null)]),
				SftAbs = new([(X, new(new(context, "log(1+exp(x))",  cancel), out _, X), null)]),
				//SftNeg = new([(X, new(new(context, "neg(log(1+exp(neg(x))))",  cancel), out _, Xy), null)]),
				SftMax = new([(Xy, new(new(context, "log(exp(x)+exp(y))",  cancel), out _, Xy), null)]),
				//SftMin = new([(Xy, new(new(context, "neg(log(exp(neg(x))+exp(neg(y))))",  cancel), out _, Xy), null)]),
				SftClamp = new([(Xyz, new(new(context, "x+log(1+exp(y+neg(x)))+neg(log(1+exp(z+neg(z))))",  cancel), out _, Xyz), null)]),
				
				// ILeafODO these could use a precomputed log(b)
				SftAbsB = new([(Xy, new(new(context, "log(1+exp(log(y)x))inv(log(y))",  cancel), out _, Xy), null)]),
				SftNegB = new([(Xy, new(new(context, "neg(log(1+exp(neg(log(y)x))))inv(log(y))",  cancel), out _, Xy), null)]),
				SftMaxB = new([(Xyz, new(new(context, "log(exp(log(z)x)+exp(log(z)y))inv(log(z))",  cancel), out _, Xyz), null)]),
				SftMinB = new([(Xyz, new(new(context, "neg(log(exp(log(z)neg(x))+exp(log(z)neg(y))))inv(log(z))",  cancel), out _, Xyz), null)]),
				SftClampB = new([(Xyzw, new(new(context, "x+(log(1+exp(log(w)(y+neg(x))))+neg(log(1+exp(log(w)(z+neg(x))))))inv(log(w))", cancel), out _, Xyzw), null)]),
				SoftClamp01B = new([(Xy, new(new(context, "x+(log(1+exp(log(y)(neg(x))))+neg(log(1+exp(log(y)(1+neg(z))))))inv(log(y))", cancel), out _, Xy), null)]),
				
				ExpB = new([(Xy, new(new(context, "exp(log(y)x)",  cancel), out _, Xy), null)]),
				LogB = new([(Xy, new(new(context, "log(x)inv(log(y))",  cancel), out _, Xy), null)]),
				ImMag = new([(X, new(new(context, "abs(x+neg(re(x)))",  cancel), out _,X), null)]),
				Square = new([(X, new(new(context, "xx", cancel), out _, X), null)]), // ILeafODO maybe later implement it for performance
				Cub = new([(X, new(new(context, "xxx", cancel), out _, X), null)]),
				Quart = new([(X, new(new(context, "sqr(sqr(x))",  cancel), out _,X), null)]);
				// ILeafODO rgb2hsb, hsv2rgb, log2hsv, lin2hsv, exp2hsv, log2rgb, lin2rgb, exp2rgb... or maybe actually implement those...?
		}
		private static Subs? _subs;
		public bool ParseByteCode(CancellationToken cancel, Comparser/*<T>*/ context, out string print, out byte[] code, bool getCode = true, bool getPrint = false) {
			
			List<byte> header = []; // header string
			WriteInt(ToBytes(zero).Length, header); // leaf size -> header
			List<List<byte>> funcCodes = [header]; // first slot is reserved for the function counter
			List<string[]> funcPrints = []; // print function definitions
			Dictionary<CallCustom, int> calls = []; // found custom function recalls
			List<CallCustom> newCalls = []; // found custom function calls for parsing
			int parsedCalls = 0; // already parsed newCalls -> funcPrints + funcCodes
			
			// replacement custom functions (those that branch or use the x argument multiple times):
			if (_subs == null || _subs.Context != context)
				_subs = new(context, cancel);
			
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
						OpCode.Clamp => new(OpCode.Call, v, _subs!.Clamp),
						OpCode.Lerp => new(OpCode.Call, v, _subs!.Lerp),
						OpCode.SoftClamp => new(OpCode.Call, v, _subs!.SftClamp),
						OpCode.SoftClampB => new(OpCode.Call, v, _subs!.SftClampB),
						OpCode.SoftMax => new(OpCode.Call, v, _subs!.SftMax),
						OpCode.SoftMaxB => new(OpCode.Call, v, _subs!.SftMaxB),
						OpCode.ExpB => new(OpCode.Call, v, _subs!.ExpB),
						//OpCode.ExpB => new(OpCode.Exp, new([new(OpCode.Log, v.Values[1]), v.Values[0]], OpCode.Mul)),
						OpCode.LogB => new(OpCode.Call, v, _subs!.LogB),
						//OpCode.LogB => new([new(OpCode.Log, v.Values[0]), new(OpCode.Inv, new(OpCode.Log, v.Values[1]))], OpCode.Mul),
						OpCode.More => new(OpCode.Neg, new(OpCode.Less, new(OpCode.Neg, v))),
						OpCode.MoreEqual => new(OpCode.Neg, new(OpCode.LessEqual, new(OpCode.Neg, v))),
						OpCode.Min => new(OpCode.Neg, new(OpCode.Max, new(OpCode.Neg, v))),
						OpCode.SoftMin => new(OpCode.Neg, new(OpCode.SoftMax, new(OpCode.Neg, v))), 
						OpCode.SoftMinB => new(OpCode.Call, v, _subs!.SftMinB),//new(OpCode.Neg, new(OpCode.SoftMaxB, new(OpCode.Neg, v))), 
						OpCode.SoftAbs => new(OpCode.Call, v, _subs!.SftAbs),//new([new(zero), v], OpCode.SoftMax),
						OpCode.SoftAbsB => new(OpCode.Call, v, _subs!.SftAbsB),//new([new(zero), v], OpCode.SoftMaxB),
						OpCode.SoftNeg => new(OpCode.Neg, new(OpCode.SoftAbs, new(OpCode.Neg, v))),//new(OpCode.Call, v, subs!.SftNeg),//
						OpCode.SoftNegB => new(OpCode.Call, v, _subs!.SftNegB),//new([new(zero), v], OpCode.SoftMinB),
						OpCode.Exp10 => new(OpCode.Exp, new([Ln10, v], OpCode.Mul)),
						OpCode.Exp2 => new(OpCode.Exp, new([Ln2, v], OpCode.Mul)), 
						OpCode.Log10 => new([new(OpCode.Log, v), Iln10], OpCode.Mul),
						OpCode.Log2 => new([new(OpCode.Log, v), Iln2], OpCode.Mul),
						OpCode.Inc => new([v, new(Real.unit)], OpCode.Add),
						OpCode.Dec => new([v, new(-Real.unit)], OpCode.Add),
						OpCode.Sinc => new(OpCode.Call, v, _subs!.Sinc),
						OpCode.Nsinc => new(OpCode.Call, v, _subs!.Nsinc),
						OpCode.Sinhc => new(OpCode.Call, v, _subs!.Sinhc),
						OpCode.Nsinhc => new(OpCode.Call, v, _subs!.Nsinhc),
						OpCode.Cosc => new(OpCode.Call, v, _subs!.Cosc),
						OpCode.Ncosc => new(OpCode.Call, v, _subs!.Ncosc),
						OpCode.Coshc => new(OpCode.Call, v, _subs!.Coshc),
						OpCode.Ncoshc => new(OpCode.Call, v, _subs!.Ncoshc),
						OpCode.Tanh => new(OpCode.Call, v, _subs!.Tanh),
						OpCode.Coth => new(OpCode.Call, v, _subs!.Coth),
						OpCode.Sech => new(OpCode.Inv, new(OpCode.Cosh, v)),
						OpCode.Csch => new(OpCode.Inv, new(OpCode.Sinh, v)),
						OpCode.Tan => new(OpCode.Call, v, _subs!.Tan),
						OpCode.Cot => new(OpCode.Call, v, _subs!.Cot),
						OpCode.Sec => new(OpCode.Inv, new(OpCode.Cos, v)),
						OpCode.Csc => new(OpCode.Inv, new(OpCode.Sin, v)),
						OpCode.Asech => new(OpCode.Acosh, new(OpCode.Inv, v)),
						OpCode.Acsch => new(OpCode.Asinh, new(OpCode.Inv, v)),
						OpCode.Asin => new([QTau, new(OpCode.Neg, new(OpCode.Acos, v))], OpCode.Add),
						OpCode.Acot => new([QTau, new(OpCode.Neg, new(OpCode.Atan, v))], OpCode.Add),
						OpCode.Asec => new(OpCode.Acos, new(OpCode.Inv, v)),
						OpCode.Acsc => new(OpCode.Asin, new(OpCode.Inv, v)),
						OpCode.ImMag => new(OpCode.Call, v, _subs!.ImMag),
						OpCode.ImCoef => new(OpCode.Call, new(OpCode.Re, new(OpCode.Neg, new(OpCode.Mul,v)))),
						OpCode.Frac => new(OpCode.Call, v, _subs!.Frac),
						OpCode.Ceil => new(OpCode.Neg, new(OpCode.Floor, new(OpCode.Neg, v))),
						OpCode.Cycle => new(OpCode.Call, v, _subs!.Cycle),
						OpCode.Clamp01 =>new([v, new (zero), new(unit)],OpCode.Clamp),
						OpCode.SoftClamp01B => new(OpCode.Call, v, _subs!.SoftClamp01B),
						OpCode.SoftClamp01 => new([v, new(zero), new(unit)],OpCode.SoftClamp),
						OpCode.Sgn => new(OpCode.Call, v, _subs!.Sgn),
						OpCode.Sqrt => new([v, new((Real)(.5))], OpCode.Pow),
						OpCode.Sqr => new(OpCode.Call, v, _subs!.Square),
						OpCode.Cbrt => new([v, new((Real)(1.0/3))], OpCode.Pow),
						OpCode.Cub => new(OpCode.Call, v, _subs!.Cub),
						OpCode.Quart => new(OpCode.Call, v, _subs!.Quart),
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
					printF += "FUNCTION " + f + ":" + funcPrints[f].Length + " DEFINITIONS" + "\n"; // FUNCILeafION<index>: definitions count
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
							foreach (var i in ToBytes(v._leaf))
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
