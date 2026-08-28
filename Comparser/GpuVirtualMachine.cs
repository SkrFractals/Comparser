using Comparser.Comparser.Numbers;
namespace Comparser.Comparser;
public abstract partial class Comparser<T> where T : unmanaged, INumber<T> {
	public class GpuVirtualMachine {

		#region Data
		public class VmValue {
			public VmValue() { }
			public VmValue(T leaf) => Leaf = leaf;
			public VmValue(VmValue[] values) => Values = values;
			public VmValue[] Values = [];
			public T Leaf = T.NaN();
			public int Def = -1; // TODO link this
		}
		public class Op(VmValue input, OpCode myCode) {
			public readonly OpCode MyCode = myCode;
			public VmValue MyValue = new(), Input = input;
		}
		#endregion

		#region Program
		private readonly int _leafSize;
		private readonly Dictionary<int, (VmValue input, int condition, int definition)[]> _definitions = [];
		private readonly int _location;
		private readonly byte[] _code;
		private readonly ushort _stackOverflow;
		#endregion
		
		#region Public
		public GpuVirtualMachine(byte[] code, ushort stackOverflowLimit = 999) {
			_stackOverflow = stackOverflowLimit;
			_code = code;
			int read = 0;
			_leafSize = ReadInt(ref read); // first header int = leaf size
			var functions = ReadInt(ref read); // second header int = function count
			for (var f = 0; f < functions; ++f) {
				var defCount = ReadInt(ref read);
				var def = _definitions[f] = new(VmValue, int, int)[defCount];
				// Read function definitions:
				for (var d = 0; d < defCount; ++d) 
					def[d] = (
						Eval(new(), ref read, 0, false), 
						code[read++] == 255 ? -1 : Skip(new(), ref read), 
						Skip(new(), ref read));
				_location = read;
			}
		}
		public VmValue Eval(VmValue input) { int pc = _location; return Eval(input, ref pc); }
		#endregion
		
		#region Private
		private int Skip(VmValue input, ref int pc) {
			var p = pc;
			var v = Eval(input, ref pc, 0, false);
			return pc;
		}
		private VmValue Eval(VmValue input, ref int pc, ushort depth = 0, bool evaluate = true) {
			if (++depth > _stackOverflow) 
				return new(); // stack overflow
			var initPc = pc;
			VmValue eval;
			var op = new Op(input, (OpCode)ReadByte(ref pc));
			switch (op.MyCode) {
			case OpCode.DefaultArg:
				op.MyValue.Def = ReadInt(ref pc);
				break;
			case OpCode.Call:
			{
				var definitions = _definitions[ReadInt(ref pc)];
				if (!evaluate)
					break;
				eval = Collapse(Eval(input, ref pc, depth));
				var match = -1;
				for (var m = 0; m < definitions.Length; ++m)
					if (Match(definitions[m].input, eval) && Cond(definitions[m].condition)) {
						match = m;
						break;
					}
				var pointer = definitions[match].definition;
				op.MyValue = match == -1 ? new() : Eval(eval, ref pointer, depth); // failed to match any available argument list ? else eval.
			
				bool Cond(int condPtr) {
					if (condPtr < 0)
						return true;
					var l = Collapse(Eval(eval, ref condPtr, depth));
					while (l.Values.Length > 0)
						l = l.Values[0];
					return INumber<T>.IsTrue(l.Leaf);
				}
			}
				static bool Match(VmValue self, VmValue a) { // defArguments.Match(callArguments)
					if (!self.Leaf.IsNaN())
						return a.Leaf.IsNaN() || T.AreEqual(self.Leaf, a.Leaf); // callArguments always starts with Values
					if (self.Values.Length == 0) return true;
					if (self.Values.Length < a.Values.Length) return false;
					if (self.Values.Length > a.Values.Length) {
						var newVal = new VmValue[self.Values.Length];
						// copy missing arguments:
						for (int i = 0; i < a.Values.Length; ++i)
							newVal[i] = a.Values[i];
						for (int i = a.Values.Length; i < self.Values.Length; ++i)
							newVal[i] = self.Values[i];
						a.Values = newVal;
					}
					var m = true;
					for (var i = 0; i < self.Values.Length; ++i)
						m &= Match(self.Values[i], a.Values[i]);
					return m;
				}
				break;
			// Values:
			case OpCode.Argument:
				var nest = ReadByte(ref pc);
				if (!evaluate) {
					while(nest-- > 0) ReadInt(ref pc);
					break;
				}
				op.MyValue = evaluate ? GetArg(input, ref pc) : new(T.NaN());
				break;

				VmValue GetArg(VmValue recurse, ref int pc) {
					var i = ReadInt(ref pc);
					--nest;
					recurse = Collapse(recurse);
					if (recurse.Values.Length == 0)
						return i == 0 && nest == 0 ? EvalArg(recurse, input) : new(T.NaN());
					var fail = recurse.Values.Length <= i;
					if (nest == 0)
						return fail ? new(T.NaN()) : recurse.Values[i];
					return fail ? new(T.NaN()) : GetArg(recurse.Values[i], ref pc);
					VmValue EvalArg(VmValue arg, VmValue args) {
						if (!arg.Leaf.IsNaN() || arg.Def < 0)
							return new(arg.Leaf);
						var argDef = _definitions[arg.Def][0];
						var argPtr = argDef.definition;
						return depth > _stackOverflow ? new() : Eval(args, ref argPtr, (ushort)(1 + depth));
						//? EvalValue((ushort)(1 + depth), arg, args)
					}
			}
			case OpCode.Leaf:
				op.MyValue = new(ReadT(ref pc));
				break;
			case OpCode.Vector:
				var count = ReadInt(ref pc);
				op.MyValue = new(new VmValue[count]);
				for (int i = 0; i < count; ++i)
					op.MyValue.Values[i] = Eval(input, ref pc, depth);
				break;
			// Unary:
			case OpCode.Neg:
			case OpCode.True:
			case OpCode.Inv:
			case OpCode.Exp:
			case OpCode.Log:
			case OpCode.Cosh:
			case OpCode.Sinh:
			case OpCode.Cos:
			case OpCode.Sin:
			case OpCode.Acosh:
			case OpCode.Asinh:
			case OpCode.Atanh:
			case OpCode.Acoth:
			case OpCode.Acos:
			case OpCode.Atan:
			case OpCode.Re:
			case OpCode.Im:
			case OpCode.Trunc:
			case OpCode.Floor:
			case OpCode.Round:
			case OpCode.Absri:
			case OpCode.SqrAbs:
			case OpCode.Abs:
			case OpCode.Arg:
			case OpCode.Conj:
			case OpCode.Factorial:
			case OpCode.Gamma:
			case OpCode.Zeta:
				eval = Eval(input, ref pc);
				if (!evaluate) break;
				op.MyValue = Operate(eval, op.MyCode);
				break;
			case OpCode.Less:
			case OpCode.LessEqual:
			case OpCode.Equal:
			case OpCode.NotEqual:
			case OpCode.Add:
			case OpCode.Mod:
			case OpCode.Mul:
			case OpCode.Pow:
			case OpCode.Max:
			case OpCode.SoftMax:
				eval = Eval(input, ref pc);
				if (!evaluate) 
					break;
				if (eval.Values.Length > 1) {
					op.MyValue = new([eval.Values[0]]);
					for (int i = 1; i < eval.Values.Length; ++i)
						op.MyValue = Operate2(op.MyValue, eval.Values[i], op.MyCode);
				} else op.MyValue = eval;
				break;
			case OpCode.Clamp:
				eval = Eval(input, ref pc);
				if (!evaluate) 
					break;
				op.MyValue = eval.Values.Length == 3 ? Operate3(eval.Values[0], eval.Values[1], eval.Values[2], op.MyCode) : eval;
				break;
			// Binary:
			
			// Iterators:
			case OpCode.Vec: // Make a vector from iterated expression
			{
				if (PrepareIterator(ref pc, out var definitions, out var from, out var to, out var args))
					break;
				op.MyValue = new(new VmValue[Math.Abs(from - to) + 1]) { Values = { [0] = eval } };
				int iterPtr, i = 0;
				while (from < to) {
					args.Values[^1] = new(T.MakeR(++from)); 	
					iterPtr = definitions[0].definition;
					Iter();
				}
				while (from > to) {
					args.Values[^1] = new(T.MakeR(--from)); 	
					iterPtr = definitions[0].definition;
					Iter();
				}
				void Iter() => op.MyValue.Values[++i] = Eval(args, ref iterPtr, depth);
			}
				break;
			case OpCode.Sum: // Sigma sum iterated expression
			{
				if (PrepareIterator(ref pc, out var definitions, out var from, out var to, out var args))
					break;
				int iterPtr;
				for (op.MyValue = eval; from < to; Iter()) {
					args.Values[^1] = new(T.MakeR(++from));
					iterPtr = definitions[0].definition;
				}
				for (op.MyValue = eval; from > to; Iter()) {
					args.Values[^1] = new(T.MakeR(--from));
					iterPtr = definitions[0].definition;
				}
				void Iter() => op.MyValue = Operate2(op.MyValue, Eval(args, ref iterPtr, depth), OpCode.Add);
				break;
			}
			case OpCode.Prod: // Pi product iterated expression
			{
				if (PrepareIterator(ref pc, out var definitions, out var from, out var to, out var args))
					break;
				int iterPtr;
				for (op.MyValue = eval; from < to; Iter()) {
					args.Values[^1] = new(T.MakeR(++from));
					iterPtr = definitions[0].definition;
				}
				for (op.MyValue = eval; from > to; Iter()) {
					args.Values[^1] = new(T.MakeR(--from));
					iterPtr = definitions[0].definition;
				}
				void Iter() => op.MyValue = Operate2(op.MyValue, Eval(args, ref iterPtr, depth), OpCode.Mul);
				break;
			}
			case OpCode.Index:
				eval = Eval(input, ref pc, depth);
				if (!evaluate) 
					break;
				op.MyValue = OperateValue(eval, OpCode.Index, Eval(input, ref pc, depth));
				break;
			case OpCode.Cat:
				eval = Eval(input, ref pc, depth);
				List<VmValue> cat = [];
				if (0 == eval.Values.Length) cat.Add(new(eval.Leaf));
				else OperateCat(eval);
				op.MyValue = new([.. cat]);
				break;

				void OperateCat(VmValue v) {
					var vV = v.Values;
					var s = vV.Length;
					if (0 == s) cat.Add(new(v.Leaf));
					else
						for (var i = 0; i < s; ++i)
							OperateCat(vV[i]);
				}
			case OpCode.Count:
				eval = Eval(input, ref pc, depth);
				op.MyValue = new(T.MakeR(Math.Max(eval.Values.Length, 1)));
				break;
			}
			return op.MyValue;
			
			bool PrepareIterator(ref int pc, out (VmValue input, int _, int definition)[] definitions, out int from, out int to, out VmValue args) {
				from = to = 0;
				args = new();
				var p = ReadInt(ref pc);
				definitions = [];
				eval = Collapse(Eval(input, ref pc, depth));
				if (eval.Values.Length < 3 || !evaluate) {
					op.MyValue = new(T.NaN()); // missing range
					return true;
				}
				args = new(new VmValue[input.Values.Length + 1]);
				for (int i = 0; i < input.Values.Length; ++i)
					args.Values[i] = input.Values[i];
				from = GetI(eval.Values[1]);
				to = GetI(eval.Values[2]);
				args.Values[^1] = new(T.MakeR(from));
				definitions = _definitions[p];
				var iterPtr = definitions[0].definition;
				eval = Eval(args, ref iterPtr, depth);
				return false;
			}
			int GetI(VmValue a) => (int)Math.Round(T.Re(GetLeaf(a).Leaf));

			VmValue OperateValue(VmValue value, OpCode opCode, VmValue data) {
				int s;
				var vA = (value = Collapse(value)).Values;
				VmValue vals = new(new VmValue[s = vA.Length]);
				if (vA.Length == 0) vA = [new(value.Leaf)];
				for (int a = 0; a < s; ++a)
					vals.Values[a] = (vA[a] = Collapse(vA[a])).Values.Length == 0 ? OpLeaf(vA[a].Leaf) : Operate(vA[a], opCode);
				if (s != 0)
					return vals;
				vals.Values = [OpLeaf(value.Leaf)];
				return vals;
				VmValue OpLeaf(T leaf) => opCode switch {
					OpCode.Index => data.Values.Length > (int)T.Re(leaf) ? data.Values[(int)T.Re(leaf)] : new(),
					_ => new()
				};
			}
			VmValue Operate(VmValue value, OpCode opCode) {
				int s;
				var vA = (value = Collapse(value)).Values;
				VmValue vals = new(new VmValue[s = vA.Length]);
				if (vA.Length == 0) vA = [new(value.Leaf)];
				for (int a = 0; a < s; ++a)
					vals.Values[a] = (vA[a] = Collapse(vA[a])).Values.Length == 0 ? new(OpLeaf(vA[a].Leaf)) : Operate(vA[a], opCode);
				if (s != 0)
					return vals;
				vals.Leaf = OpLeaf(value.Leaf);
				return vals;
				T OpLeaf(T l) => opCode switch {
					OpCode.Neg => -l, OpCode.True => +l < 1 ? T.Zero() : T.MakeR(1),
					OpCode.Inv => T.Inv(l), OpCode.Exp => T.Exp(l), OpCode.Log => T.Log(l),
					OpCode.Cosh => T.Cosh(l), OpCode.Sinh => T.Sinh(l), OpCode.Cos => T.Cos(l), OpCode.Sin => T.Sin(l),
					OpCode.Acosh => T.Acosh(l), OpCode.Asinh => T.Asinh(l), OpCode.Atanh => T.Atanh(l), OpCode.Acoth => T.Acoth(l),
					OpCode.Acos => T.Acos(l), OpCode.Atan => T.Atan(l), OpCode.Re => T.MakeR(T.Re(l)), OpCode.Im =>T.MakeR(T.Im(l)),
					OpCode.Trunc => T.Trunc(l), OpCode.Floor => T.Floor(l), OpCode.Round => T.Round(l), OpCode.Absri => T.AbsComp(l),
					OpCode.SqrAbs => T.MakeR(+l), OpCode.Abs => T.MakeR(INumber<T>.Abs(l)),
					OpCode.Arg => T.MakeR(T.Arg(l)), OpCode.Conj => INumber<T>.Conj(l),
					OpCode.Factorial => T.Factorial(l), OpCode.Gamma => T.Gamma(l), OpCode.Zeta => T.Zeta(l),
					_ => T.NaN()
				};
			}
			VmValue Operate2(VmValue av, VmValue bv, OpCode opCode) {
				VmValue[] vA = (av = Collapse(av)).Values, vB = (bv = Collapse(bv)).Values;
				int a = 0, b = 0, s = Math.Max(vA.Length, vB.Length);
				VmValue vals = new(new VmValue[s]);
				if (vA.Length == 0) vA = [new(av.Leaf)];
				if (vB.Length == 0) vB = [new(bv.Leaf)];
				for (var i = 0; i < s; ++i) {
					int an, bn;
					vals.Values[i] = (an = (vA[a] = Collapse(vA[a])).Values.Length)
						+ (bn = (vB[b] = Collapse(vB[b])).Values.Length) == 0
							? new(OpLeaf(vA[a].Leaf, vB[b].Leaf))
							: Operate2(
								an == 0 ? new([new(vA[a].Leaf)]) : vA[a],
								bn == 0 ? new([new(vB[b].Leaf)]) : vB[b], opCode);
					a = (a + 1) % vA.Length;
					b = (b + 1) % vB.Length;
				}
				if (s != 0)
					return vals;
				vals.Leaf = OpLeaf(av.Leaf, bv.Leaf);
				return vals;
				T OpLeaf(T la, T lb) => opCode switch {
					OpCode.Less => T.MakeR(T.Re(la) < T.Re(lb) ? 1 : 0), OpCode.LessEqual => T.MakeR(T.Re(la) <= T.Re(lb) ? 1 : 0),
					OpCode.Equal => T.MakeR(T.Re(la) - T.Re(lb) < 1e-8 ? 1 : 0), OpCode.NotEqual => T.MakeR(T.Re(la) - T.Re(lb) >= 1e-8 ? 1 : 0), 
					OpCode.Add => la + lb, OpCode.Mod => la % lb, OpCode.CompMod => INumber<T>.CompMod(la, lb), OpCode.Mul => la * lb, OpCode.Pow => la^lb, OpCode.Max => T.Max(la, lb),
					OpCode.SoftMax => INumber<T>.SoftMax(la, lb), 
					_ => T.NaN()
				};
			}
			VmValue Operate3(VmValue av, VmValue bv, VmValue cv, OpCode opCode) {
				VmValue[] vA = (av = Collapse(av)).Values, vB = (bv = Collapse(bv)).Values, vC = (cv = Collapse(cv)).Values;
				int a = 0, b = 0, c = 0, s = Math.Max(vC.Length, Math.Max(vA.Length, vB.Length));
				VmValue vals = new(new VmValue[s]);
				if (vA.Length == 0) vA = [new(av.Leaf)];
				if (vB.Length == 0) vB = [new(bv.Leaf)];
				if (vC.Length == 0) vC = [new(cv.Leaf)];
				for (int i = 0; i < s; ++i) {
					int an, bn, cn;
					vals.Values[i] = (an = (vA[a] = Collapse(vA[a])).Values.Length)
						+ (bn = (vB[b] = Collapse(vB[b])).Values.Length)
						+ (cn = (vC[c] = Collapse(vC[c])).Values.Length) == 0
							? new(OpLeaf(vA[a].Leaf, vB[b].Leaf, vC[c].Leaf))
							: Operate3(
								an == 0 ? new([new(vA[a].Leaf)]) : vA[a],
								bn == 0 ? new([new(vB[b].Leaf)]) : vB[b],
								cn == 0 ? new([new(vC[c].Leaf)]) : vC[c], opCode);
					a = (a + 1) % vA.Length;
					b = (b + 1) % vB.Length;
					c = (c + 1) % vC.Length;
				}
				if (s != 0)
					return vals;
				vals.Leaf = OpLeaf(av.Leaf, bv.Leaf, cv.Leaf);
				return vals;
				T OpLeaf(T la, T lb,T lc) => opCode switch {
					OpCode.Clamp => T.Clamp(la, lb, lc),
					_ => T.NaN()
				};
			}
		}
		private static VmValue Collapse(VmValue i) {
			while (i.Values.Length == 1)
				i = i.Values[0];
			return i;
		}
		private static VmValue GetLeaf(VmValue i) {
			while (i.Values.Length > 0)
				i = i.Values[0];
			return i;
		}
		private byte ReadByte(ref int pc) => _code[pc++];
		// i <= 251 ? [i] : [256-bytes, byte[bytes-1], byte[bytes-2],...,byte[0]] 
		private int ReadInt(ref int pc) {
			var c = _code[pc++];
			if (c <= 251) return c; // the one byte case (0-251 values)
			int result = _code[pc++]; // otherwise read the bytes sequentially from the most significant bit
			while(0 != ++c) result = (result << 8) + _code[pc++]; // iterate until byte overflow (c=255 is one byte (no iterations, keep the initial byte), each c decrement adds one extra byte (c=252 is full 4 bytes)
			return result;
		}
		private T ReadT(ref int pc) {
			var bytes = new byte[_leafSize];
			for (int i = 0; i < _leafSize; ++i) 
				bytes[i] = ReadByte(ref pc);
			return INumber<T>.FromBytes(bytes);
		}
		#endregion
	}
}