namespace Comparser.Comparser;
public abstract partial class Comparser<T>{
	public class Value {
		// simple value if it is only a number not containing an expression term (can remain present even if it is replaced with a term)
		public T Leaf = T.NaN();
		// Vector elements (nestable)
		public Value[] Values = [];
		// Main term and operand (second term)
		public Expression? Term, Operand;
		// Operator (eval = term <operator> operand), if it is a pure parent Operator, it only evaluates the term
		public Operator Op = new();
		// Argument index binding (if non-negative, it will get replaced by the argument value with this Arg index)
		public readonly int[] Arg = [];
		// 1 = stack overflow
		public int Error;
		// special type
		public object? Data;
		// After parsing will contain the original parsed text, even if error occurs (this also naturally works with argument parsing and matching)
		public string Text; // The original text that this input has been parsed from, even if it fails parsing
		
		// Multiple functions:
		// In constant/variable/argument bindings, this is the alias name that will get replaced with the Leaf value whenever it is detected
		// In a string data type, it contains the string value
		public string String; // The original text that this input has been parsed from, even if it fails parsing
		public override string ToString() => CollapseScalar(this).Pv(-1);
		public string ToString(int decimals) => Text + " = " + CollapseScalar(this).Pv(decimals);
		private string Pv(int decimals, string a = "", string b = "") {
			if (Values.Length <= 0)
				return Error > 0 ? PrintError() : Leaf.IsNaN() && Text != "" ? Text : Leaf.ToString(decimals);
			var s = a;
			for (var i = 0; i < Values.Length; ++i) {
				Values[i] = CollapseScalar(Values[i]);
				if (i > 0) s += ", ";
				s += Values[i].Pv(decimals, "(", ")");
			}
			return s + b;
		}
		private string PrintError() => (Error & 1) > 0 ? "Stack Overflow." : "";
		private string Tl() {
			if (Values.Length <= 0)
				return Text;
			var s = "";
			for (var i = 0; i < Values.Length; ++i)
				s += (Values[i] = CollapseScalar(Values[i])).Tl() + "\n";
			return s;
		}
		public string ToLines() => CollapseScalar(this).Tl();
		public T GetLeaf() => Values.Length > 0 ? Values[0].GetLeaf() : Leaf;
		public Value[] GetValues() {
			var c = CollapseScalar(this);
			return c.Values.Length == 0 ? [new(c.Leaf)] : c.Values;
		}
		public string GetString() => Values.Length > 0 ? Values[0].GetString() : String;
		
		#region Constructors
		public Value(T value, Operator op, int[]? arg = null, Expression? term = null, Expression? operand = null, bool negative = false, string text = "") {
			Leaf = value; Term = term; Operand = operand; Op = op; Op.Negative = negative; Arg = arg ?? []; String = Text = text;
		}
		public Value(T value, int error = 0, string text = "") { Error = error; Leaf = value; String = Text = text; }
		public Value(int error = 0, string text = "") { Error = error; String = Text = text; }
		public Value(Value[] values, int error = 0, string text = "") { Error = error; Values = values; String = Text = text; }
		public Value(string text, int[] arg) { Arg = arg; String = Text = text; }
		#endregion
		
		public bool Match(Value a) => MatchP(UnCollapseScalar(a));
		private bool MatchP(Value a) { // defArguments.Match(callArguments)
			if (!Leaf.IsNaN())
				return a.Leaf.IsNaN() || T.AreEqual(Leaf, a.Leaf); 
			if (Values.Length == 0) return true; // callArguments always starts with Values
			if (Values.Length < a.Values.Length) return false;
			if (Values.Length > a.Values.Length) {
				var newVal = new Value[Values.Length];
				// copy missing arguments:
				for (int i = 0; i < a.Values.Length; ++i)
					newVal[i] = a.Values[i];
				for (int i = a.Values.Length; i < Values.Length; ++i)
					newVal[i] = Values[i];
				a.Values = newVal;
			}
			var m = true;
			var to = Math.Min(Values.Length, a.Values.Length);
			for (var i = 0; i < to; ++i)
				m &= Values[i].MatchP(a.Values[i]);
			return m;
		}
		public bool SameArg(Value a) => SameArgP(UnCollapseScalar(a));
		private bool SameArgP(Value a) { // defArguments.SameArg(callArguments)
			if (!Leaf.IsNaN())
				return T.AreEqual(Leaf, a.Leaf); 
			if (Values.Length == 0) return true; // callArguments always starts with Values
			if (Values.Length != a.Values.Length) return false;
			var m = true;
			for (var i = 0; i < Values.Length; ++i)
				m &= Values[i].SameArgP(a.Values[i]);
			return m;
		}
		public static Value OperateString(ushort depth, Value av, Func<ushort, string, Value> o) {
			var vA = (av = CollapseScalar(av)).Values;
			var s = vA.Length;
			Value vals = new(new Value[s]);
			if (vA.Length == 0) vA = [new(av.Leaf, av.Error, av.String) { Error = av.Error } ];
			for (int an, a = 0; a < s; ++a)
				vals.Values[a] = (an = (vA[a] = CollapseScalar(vA[a])).Values.Length) == 0 
					? o(depth, vA[a].String) 
					: OperateString(depth, an == 0 ? new([new(vA[a].Leaf, vA[a].Error)]) : vA[a], o);
			if (s != 0)
				return vals;
			vals.Leaf = CollapseScalar(o(depth, av.String)).Leaf;
			vals.String = av.String;
			vals.Error = av.Error;
			return vals;
		}
		public static Value OperateValue(Value av, Func<Value, object?, Value> o, object? data) {
			int s;
			var vA = (av = CollapseScalar(av)).Values;
			Value vals = new(new Value[s = vA.Length]);
			if (vA.Length == 0) vA = [new(av.Leaf, av.Error, av.String)];
			for (int a = 0; a < s; ++a) {
				vals.Values[a] = ( /*an = */(vA[a] = CollapseScalar(vA[a])).Values.Length) == 0 ? o(vA[a], data) : OperateValue(vA[a], o, data);
				vals.Values[a].Error |= vA[a].Error;
			}
			if (s != 0)
				return vals;
			vals.Values = [o(av, data)];
			vals.Text = av.Text;
			vals.Error = av.Error;
			return vals;
		}
		public static Value OperateData(Value av, Func<T, object?, T> o, object? data = null) {
			int s;
			var vA = (av = CollapseScalar(av)).Values;
			Value vals = new(new Value[s = vA.Length]);
			if (vA.Length == 0) vA = [new(av.Leaf, av.Error, av.String)];
			for (int a = 0; a < s; ++a)
				vals.Values[a] = (/*an = */(vA[a] = CollapseScalar(vA[a])).Values.Length) == 0 ? new(o(vA[a].Leaf, data), vA[a].Error) : OperateData(vA[a], o, data);
			if (s != 0)
				return vals;
			vals.Leaf = o(av.Leaf, data);
			vals.String = av.String;
			vals.Error = av.Error;
			return vals;
		}
		public static Value Operate(Value av, Func<T, T> o) {
			int s;
			var vA = (av = CollapseScalar(av)).Values;
			Value vals = new(new Value[s = vA.Length]);
			if (vA.Length == 0) vA = [new(av.Leaf, av.Error, av.String)];
			for (int a = 0; a < s; ++a)
				vals.Values[a] = (/*an = */(vA[a] = CollapseScalar(vA[a])).Values.Length) == 0 ? new(o(vA[a].Leaf), vA[a].Error) : Operate(vA[a], o);
			if (s != 0)
				return vals;
			vals.Leaf = o(av.Leaf);
			vals.String = av.String;
			vals.Error = av.Error;
			return vals;
		}
		public static Value Operate2(Value av, Value bv, Func<T, T, T> o, Func<string, string, string> so) {
			// DEBUG
			/*var oe = o(T.MakeR(3), T.MakeR(3));
			var avO = av.Op;
			var bvO = bv.Op;
			var avl = av.Leaf;
			var bvl = bv.Leaf;
			var av0 = av.Values.Length == 0 ? null : av.Values[0];
			var bv0 = bv.Values.Length == 0 ? null : bv.Values[0];
			var a0L = av0 == null ? T.NaN() : av0.Leaf;
			var b0L = bv0 == null ? T.NaN() : bv0.Leaf;
			var a0O = av0?.Op ?? null;
			var b0O = bv0?.Op ?? null;*/
			Value[] vA = (av = CollapseScalar(av)).Values, vB = (bv = CollapseScalar(bv)).Values;
			int a = 0, b = 0, s = Math.Max(vA.Length, vB.Length);
			Value vals = new(new Value[s]);
			if (vA.Length == 0) vA = [new(av.Leaf, av.Error, av.String)];
			if (vB.Length == 0) vB = [new(bv.Leaf, bv.Error, bv.String)];
			for (var i = 0; i < s; ++i) {
				int an, bn;
				vals.Values[i] = (an = (vA[a] = CollapseScalar(vA[a])).Values.Length) 
					+ (bn = (vB[b] = CollapseScalar(vB[b])).Values.Length) == 0
					? new(o(vA[a].Leaf, vB[b].Leaf), vA[a].Error, so(vA[a].String, vB[b].String)) 
					: Operate2(
						an == 0 ? new([new(vA[a].Leaf,vA[a].Error)]) : vA[a],
						bn == 0 ? new([new(vB[b].Leaf,vA[a].Error)]) : vB[b], o, so);
				a = (a + 1) % vA.Length;
				b = (b + 1) % vB.Length;
			}
			if (s != 0)
				return vals;
			vals.Leaf = o(av.Leaf, bv.Leaf);
			vals.String = so(av.String, bv.String);
			vals.Error = av.Error | bv.Error;
			return vals;
		}
		public static Value Operate3(Value av, Value bv, Value cv, Func<T, T, T, T> o) {
			Value[] vA = (av = CollapseScalar(av)).Values, vB = (bv = CollapseScalar(bv)).Values, vC = (cv = CollapseScalar(cv)).Values;
			int a = 0, b = 0, c = 0, s = Math.Max(vC.Length, Math.Max(vA.Length, vB.Length));
			Value vals = new(new Value[s]);
			if (vA.Length == 0) vA = [new(av.Leaf, av.Error, av.String)];
			if (vB.Length == 0) vB = [new(bv.Leaf, bv.Error, bv.String)];
			if (vC.Length == 0) vC = [new(cv.Leaf, cv.Error, cv.String)];
			for (int i = 0; i < s; ++i) {
				int an, bn, cn;
				vals.Values[i] = (an = (vA[a] = CollapseScalar(vA[a])).Values.Length) 
					+ (bn = (vB[b] = CollapseScalar(vB[b])).Values.Length) 
					+ (cn = (vC[c] = CollapseScalar(vC[c])).Values.Length) == 0 
					? new(o(vA[a].Leaf, vB[b].Leaf, vC[c].Leaf), vA[a].Error | vB[b].Error | vC[c].Error) 
					: Operate3(
						an == 0 ? new([new(vA[a].Leaf, vA[a].Error)]) : vA[a],
						bn == 0 ? new([new(vB[b].Leaf, vB[b].Error)]) : vB[b],
						cn == 0 ? new([new(vC[c].Leaf, vC[c].Error)]) : vC[c], o);
				a = (a + 1) % vA.Length;
				b = (b + 1) % vB.Length;
				c = (c + 1) % vC.Length;
			}
			if (s != 0)
				return vals;
			vals.Leaf = o(av.Leaf, bv.Leaf, cv.Leaf);
			vals.String = av.String;
			vals.Error = av.Error | bv.Error | cv.Error;
			return vals;
		}
		public Value Copy() => new(Leaf, Op, Arg, Term, Operand, Op.Negative, Text) { Values = CopyValues(Values)/*, Terms = CopyValues(Terms)*/ };
		private static Value[] CopyValues(Value[] c) {
			var v = new Value[c.Length];
			for (var i = 0; i < c.Length; ++i)
				v[i] = c[i].Copy();
			return v;
		}
	}
	public class Nest(byte value) {
		public Nest? Next;
		public readonly byte V = value;
	}
}