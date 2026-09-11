using Comparser.Comparser.Numbers;
namespace Comparser.Comparser;
public abstract partial class Comparser<T> where T : unmanaged, INumber<T> {
	public partial class GpuValue {
		private readonly T _leaf = T.nan; // just a complex value sitting here as the tree's leaf.
		public readonly GpuValue[] Values = []; // Children tree links. For example if I am 1+2x, I will have an OpCode.Add, and Values=[GpuValue(OpCode.Constant, Leaf=1),GpuValue(OpCode.Mul, [GpuValue(OpCode.Constant,2),GpuValue(OpCode.Argument,[0])])]
		public Expression? Operand;
		private OpCode _op = OpCode.Nop; // OpCode that will get applied to my Values
		private readonly int[] _arg = []; // if this is an argument, it will have the nested indices which value from the arguments to pick, in this array
		private CallCustom? _def; // if I am a custom function call, have a pointer to which one, the unique pointer will get collected and compiled into the Function library
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
		private bool IsNaN() => T.IsNaN(_leaf);
	}
}