using Comparser.Comparser.Numbers;
namespace Comparser.Comparser;
public abstract partial class Comparser<T> where T : unmanaged, INumber<T> {
	public partial class GpuValue {
		private readonly T _leaf = T.NaN(); // just a complex value sitting here as the tree's leaf.
		public readonly GpuValue[] Values = []; // Children tree links. For example if I am 1+2x, i will have an OpCode.Add, and Values=[GpuValue(OpCode.Constant, Leaf=1),GpuValue(OpCode.Mul, [GpuValue(OpCode.Constant,2),GpuValue(OpCode.Argument,[0])])]
		public Expression? Operand;
		private OpCode _op = OpCode.Nop; // OpCode that will get applied to my Values
		private readonly int[] _arg = []; // if this is an argument, it will have the nested indices which value from the arguments to pick, in this array
		private CallCustom? _def; // if i am a custom function call, have a pointer to which one, the unique pointer will get collected and compiled into the Function library
		//public virtual GpuProgram CompileGPU(GpuValue args) { TODO }
		public static GpuValue CollapseScalar(GpuValue i) {
			while (i.Values.Length == 1)
				i = i.Values[0];
			return i;
		}
		public GpuValue(GpuValue[] values, OpCode op) {
			_op = op;
			Values = values;
		}
		public GpuValue(OpCode op, GpuValue value, CallCustom? def = null) {
			_op = op;
			Values = [value];
			_def = def;
		}
		public GpuValue(T value) {
			_leaf = value;
			_op = OpCode.Leaf;
		}
		public GpuValue() { }
		public GpuValue(int[] arg) {
			_arg = arg;
			_op = OpCode.Argument;
		}
		private bool IsNaN() => _leaf.IsNaN();
	}
}
	/*public bool Match(GpuValue a) { // defArguments.Match(callArguments)
			if (!Leaf.IsNaN())
				return T.AreEqual(Leaf, a.Leaf); // callArguments always starts with Values
			if (Values.Length == 0) return true;
			if (Values.Length != a.Values.Length) return false;
			var m = true;
			for (var i = 0; i < Values.Length; ++i)
				m &= Values[i].Match(a.Values[i]);
			return m;
		}
		public static GpuValue OperateValue(GpuValue av, Func<GpuValue, object?, GpuValue> o, object? data) {
			int s;
			var vA = (av = CollapseScalar(av)).Values;
			GpuValue vals = new(new GpuValue[s = vA.Length]);
			if (vA.Length == 0) vA = [new(av.Leaf)];
			for (int a = 0; a < s; ++a)
				vals.Values[a] = ((vA[a] = CollapseScalar(vA[a])).Values.Length) == 0 ? o(vA[a], data) : OperateValue(vA[a], o, data);
			if (s != 0)
				return vals;
			vals.Values = [o(av, data)];
			return vals;
		}
		public static GpuValue OperateData(GpuValue av, Func<T, object?, T> o, object? data = null) {
			int s;
			var vA = (av = CollapseScalar(av)).Values;
			GpuValue vals = new(new GpuValue[s = vA.Length]);
			if (vA.Length == 0) vA = [new(av.Leaf)];
			for (int a = 0; a < s; ++a)
				vals.Values[a] = ((vA[a] = CollapseScalar(vA[a])).Values.Length) == 0 ? new(o(vA[a].Leaf, data)) : OperateData(vA[a], o, data);
			if (s != 0)
				return vals;
			vals.Leaf = o(av.Leaf, data);
			return vals;
		}
		public static GpuValue Operate(GpuValue av, Func<T, T> o) {
			int s;
			var vA = (av = CollapseScalar(av)).Values;
			GpuValue vals = new(new GpuValue[s = vA.Length]);
			if (vA.Length == 0) vA = [new(av.Leaf)];
			for (int a = 0; a < s; ++a)
				vals.Values[a] = ((vA[a] = CollapseScalar(vA[a])).Values.Length) == 0 ? new(o(vA[a].Leaf)) : Operate(vA[a], o);
			if (s != 0)
				return vals;
			vals.Leaf = o(av.Leaf);
			return vals;
		}
		public static GpuValue Operate2(GpuValue av, GpuValue bv, Func<T, T, T> o) {
			GpuValue[] vA = (av = CollapseScalar(av)).Values, vB = (bv = CollapseScalar(bv)).Values;
			int a = 0, b = 0, s = Math.Max(vA.Length, vB.Length);
			GpuValue vals = new(new GpuValue[s]);
			if (vA.Length == 0) vA = [new(av.Leaf)];
			if (vB.Length == 0) vB = [new(bv.Leaf)];
			for (var i = 0; i < s; ++i) {
				int an, bn;
				vals.Values[i] = (an = (vA[a] = CollapseScalar(vA[a])).Values.Length)
					+ (bn = (vB[b] = CollapseScalar(vB[b])).Values.Length) == 0
						? new(o(vA[a].Leaf, vB[b].Leaf))
						: Operate2(
							an == 0 ? new([new(vA[a].Leaf)]) : vA[a],
							bn == 0 ? new([new(vB[b].Leaf)]) : vB[b], o);
				a = (a + 1) % vA.Length;
				b = (b + 1) % vB.Length;
			}
			if (s != 0)
				return vals;
			vals.Leaf = o(av.Leaf, bv.Leaf);
			return vals;
		}
		public static GpuValue Operate3(GpuValue av, GpuValue bv, GpuValue cv, Func<T, T, T, T> o) {
			GpuValue[] vA = (av = CollapseScalar(av)).Values, vB = (bv = CollapseScalar(bv)).Values, vC = (cv = CollapseScalar(cv)).Values;
			int a = 0, b = 0, c = 0, s = Math.Max(vC.Length, Math.Max(vA.Length, vB.Length));
			GpuValue vals = new(new GpuValue[s]);
			if (vA.Length == 0) vA = [new(av.Leaf)];
			if (vB.Length == 0) vB = [new(bv.Leaf)];
			if (vC.Length == 0) vC = [new(cv.Leaf)];
			for (int i = 0; i < s; ++i) {
				int an, bn, cn;
				vals.Values[i] = (an = (vA[a] = CollapseScalar(vA[a])).Values.Length)
					+ (bn = (vB[b] = CollapseScalar(vB[b])).Values.Length)
					+ (cn = (vC[c] = CollapseScalar(vC[c])).Values.Length) == 0
						? new(o(vA[a].Leaf, vB[b].Leaf, vC[c].Leaf))
						: Operate3(
							an == 0 ? new([new(vA[a].Leaf)]) : vA[a],
							bn == 0 ? new([new(vB[b].Leaf)]) : vB[b],
							cn == 0 ? new([new(vC[c].Leaf)]) : vC[c], o);
				a = (a + 1) % vA.Length;
				b = (b + 1) % vB.Length;
				c = (c + 1) % vC.Length;
			}
			if (s != 0)
				return vals;
			vals.Leaf = o(av.Leaf, bv.Leaf, cv.Leaf);
			return vals;
		}*/
		/*public GpuValue Copy() => new(Leaf, Op, Arg, Op.Negative, Text) { Values = CopyValues(Values)};
		private static GpuValue[] CopyValues(GpuValue[] c) {
			var v = new GpuValue[c.Length];
			for (var i = 0; i < c.Length; ++i)
				v[i] = c[i].Copy();
			return v;
		}*/
/*public virtual GpuValue Eval(ushort depth, GpuValue args, string text = "") {
			GpuValue result = new(new GpuValue[Values.Length]);
			if (Values.Length == 0)
				result = EvalValue(depth, this, args);
			else
				for (var e = 0; e < Values.Length; result.Values[e] = EvalSingle(depth, e++, args)) { }
			if (text != "") Text = text;
			return CollapseScalar(result);
		}
		public GpuValue EvalCopy(ushort depth, GpuValue args, string text = "") => Copy().Eval(depth, args, text);
		private GpuValue EvalSingle(ushort depth, int arrayIndex, GpuValue args) => arrayIndex < Values.Length ? EvalValue(depth, Values[arrayIndex], args) : new();
		private static GpuValue EvalValue(ushort depth, GpuValue v, GpuValue args) {
			GpuValue ee;
			var a = args.Values;
			return GpuValue.Operate2(
				(ee = v).Term?.Eval(depth, args) ?? new([ee.Arg.V.Length == 0 ? ee : GetArg(0, a)]),
				ee.Operand?.Eval(depth, args) ?? new(), ee.Op.Op);
			GpuValue GetArg(int arg, GpuValue[] aa) {
				var V = ee.Arg.V;
				for (var ai = ee.Arg.V[arg]; ai >= 0 && ai < aa.Length; ++arg) {
					if (arg >= V.Length)
						return aa[V[arg]];
					aa = aa[V[arg]].Values;
				}
				return ee;
			}
		}*/