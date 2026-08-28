using Comparser.Comparser.Numbers;
namespace Comparser.Comparser;
public abstract partial class Comparser<T> {
		#region Call Functions
	// abstract parent
	public abstract class CallFunction(int cacheSize = 1) {
		//protected readonly I[]? Def = def;
		//public readonly string Name = name;
		public readonly EvalCache Cache = new(cacheSize);
		public abstract Expression Call(Comparser<T> context, ref string text, Value args);
		// how to use: e.Insert(args, e.GetEval(args) ? e.result.Eval : base.Eval([], args).v); 
		public class EvalCache(int size = 1) {
			private List<(Value args, Value eval)> Debug = []; // TODO remove this when I'm finished debugging
			private int _filled;
			private Evaluated? _cache;
			public Evaluated? Result;
			public bool GetEval(Value args) {
				for (var c = Result = _cache; c != null; Result = c, c = c.Next) {
					if (args.SameArg(c.Args)) {
						if (Result != c) {
							Result!.Next = c.Next;
							c.Next = _cache;
							_cache = c;
						}
						Result = c;
						return true;
					}
					if (c.Next == null) break;
				}
				return false;
			}
			public Value Insert(Value args, Value eval) {
				if (size <= _filled) {
					if (size == 1) _cache = null; 
					else Result?.Next = null; 
				} else ++_filled;
				if (size > 0) _cache = new(_cache, args, eval);
				Debug.Add((args,eval));
				return eval;
			}
			//public void Reset() { _filled = 0; cache = null; }
		}
		public class Evaluated(Evaluated? next, Value args, Value eval) {
			public readonly Value Eval = eval.Copy();
			public readonly Value Args = args.Copy();
			public Evaluated? Next = next;
		}
	}
	// Expressions.Functions:
	private class Ce(Type type, int cache = 1) : CallFunction(cache) {
		public override Expression Call(Comparser<T> context, ref string text, Value args) {
			object[] a = [context, this, text, args]; // activator arguments
			var n = (FunctionExpression)Activator.CreateInstance(type, a)!;
			text = (string)a[2]; // ref string text
			return n;
		}
	}
	// Single/Double/Triple argument delegated functions
	public class Cf(Func<T, T> del, OpCode op, int cache = 1) : CallFunction(cache) {
		public override Expression Call(Comparser<T> context, ref string text, Value args) => new FuncOperator(context, this, del, op, ref text, args);
	}
	public class Cf2(Func<T, T, T> del, OpCode op, int cache = 1) : CallFunction(cache) {
		public override Expression Call(Comparser<T> context, ref string text, Value args) => new FuncOperator2(context, this, del, op, ref text, args);
	}
	public class Cf3(Func<T, T, T, T> del, OpCode op, int cache = 1) : CallFunction(cache) {
		public override Expression Call(Comparser<T> context, ref string text, Value args) => new FuncOperator3(context, this, del, op, ref text, args);
	}
	#endregion

	#region Function Expressions - Operators
	protected abstract class FunctionExpression : Expression {
		private readonly CallFunction _parent;
		protected readonly OpCode _op;
		protected FunctionExpression(Comparser<T> context, CallFunction parent, OpCode op, ref string text, Value args) 
			: base(context, ref text, out _, args) => (_parent, _op) = (parent, op);
		protected FunctionExpression(Comparser<T> context, CallFunction parent, OpCode op, Value input) 
			: base(context, input) => (_parent, _op) = (parent, op);
		public override Value Eval(ushort depth, Value args/*, string text = ""*/) {
			var v = base.Eval(depth, args/*, text*/);
			return _parent.Cache.GetEval(v) ? _parent.Cache.Result?.Eval! : _parent.Cache.Insert(v, EvalF(depth, v, args));
		}
		protected abstract Value EvalF(ushort depth, Value v, Value args);
		public override GpuValue GpuParse(ushort depth) => new(_op, base.GpuParse(depth));
	}
	private class FuncTextOperator(Comparser<T> context, CallFunction parent, Func<ushort, string, Value> del, ref string text, Value args)  : FunctionExpression(context, parent, OpCode.NotAvailable, ref text, args) {
		override protected Value EvalF(ushort depth, Value v, Value args) => Value.OperateString(depth, v, del);
	}
	private class FuncEval(Comparser<T> context, CallFunction parent, ref string text, Value args, int cache = 0) 
		: FuncTextOperator(context, parent, (d, x) => d > context._stackOverflow ? StackOverflow : new Expression(context, x, args, cache).Eval((ushort)(1 + d), args), ref text, args) { }
	private class FuncOperator : FunctionExpression {
		private readonly Func<T, T> _del;
		public FuncOperator(Comparser<T> context, CallFunction parent, Func<T, T> del, OpCode op, ref string text, Value args) : base(context, parent, op, ref text, args) => _del = del;
		public FuncOperator(Comparser<T> context, CallFunction parent, Func<T, T> del, OpCode op, Value input) : base(context, parent, op, input) => _del = del;
		override protected Value EvalF(ushort _, Value v, Value args) => Value.Operate(v, _del);
	}
	private class FuncOperator2(Comparser<T> context, CallFunction parent, Func<T, T, T> comp, OpCode op, ref string text, Value args) : FunctionExpression(context, parent, op, ref text, args) {
		override protected Value EvalF(ushort _, Value v, Value args) {
			switch (v.Values.Length) {
			case 0: return v;
			case 1: return v.Values[0];
			default:
				var s = v.Values[0];
				for (var c = 1; c < v.Values.Length; ++c)
					s = Value.Operate2(s, v.Values[c], comp, (x, _) => x);
				return s;
			}
		}
	}
	private class FuncOperator3(Comparser<T> context, CallFunction parent, Func<T, T, T, T> comp, OpCode op, ref string text, Value args) : FunctionExpression(context, parent, op, ref text, args) {
		override protected Value EvalF(ushort _, Value v, Value args) => v.Values.Length == 3 ? Value.Operate3(v.Values[0], v.Values[1], v.Values[2], comp) : new();
	}
	#endregion

	#region Function Expressions - Vectors
	// extracts terms from a vector using indices in: [expression]. Example: (0a,1b,2c,(30d,31e),5f)[3,2,(5,1,3)] = (30d,31e),2c,(5,1,(30d,31e))
	private class FuncIndex(Comparser<T> context, Value input, Value indices) : Expression(context, input) {
		public override Value Eval(ushort depth, Value args/*, string text = ""*/) 
			=> depth > Context._stackOverflow ? StackOverflow : Value.OperateValue(EvalValue((ushort)(1 + depth), CollapseScalar(indices), args) ?? indices, Take, base.Eval(depth, args/*, text*/));
		private Value Take(Value from, object? i) {
			if (i is not Value v)
				return from;
			v = CollapseScalar(v);
			var index = T.Re(CollapseScalar(from).Leaf);
			int integer;
			return double.IsNaN(index) || (integer = (int)Math.Round(index)) < 0 || integer >= v.Values.Length ? None : v.Values[integer];
		}
		public override GpuValue GpuParse(ushort depth) => new([GpuParseValue(depth, indices), base.GpuParse(depth)], OpCode.Index);
	}
	private class FuncCat(Comparser<T> context, CallFunction parent, ref string text, Value args) : FunctionExpression(context, parent, OpCode.Cat, ref text, args) {
		override protected Value EvalF(ushort depth, Value v, Value args) {
			List<Value> cat = [];
			if (0 == v.Values.Length) cat.Add(new(v.Leaf));
			else Operate(ref cat, v);
			return new([.. cat]);
		}
		private static void Operate(ref List<Value> cat, Value v) {
			var vV = v.Values;
			var s = vV.Length;
			if (0 == s) cat.Add(new(v.Leaf));
			else for (var i = 0; i < s; ++i)
				Operate(ref cat, vV[i]);
		}
	}
	// counts the elements in a vector
	private class FuncCount(Comparser<T> context, CallFunction parent, ref string text, Value args) : FunctionExpression(context, parent, OpCode.Count, ref text, args) {
		override protected Value EvalF(ushort depth, Value v, Value args) => new(T.MakeR(Math.Max(1, CollapseScalar(v).Values.Length)));
	}
	// iterative sum/product: name(<index>,<from>,<to>,expression(k<index>))
	// "to" can be smaller than "from", works both ways (does not return additive/multiplicative identity when in the wrong order, just iterates backwards)
	private abstract class Iterator : FunctionExpression {
		protected Iterator(Comparser<T> context, CallFunction parent, OpCode op, ref string text, Value args) : base(context, parent, op, ref text, args) {
			var iteratorIndex = args.Values.Length;
			if (V.Values.Length != 4) {
				_expr = new(Context, "", _args = None);
				return;
			}
			_args = new(new Value[iteratorIndex + 1]);
			Array.Copy(args.Values, _args.Values, iteratorIndex);
			_args.Values[iteratorIndex] = new(T.NaN(), 0, V.Values[0].Text);
			_expr =  new(Context, V.Values.Length == 4 ? V.Values[3].Text : "", _args);
		}
		private readonly Expression _expr;
		private readonly Value _args;
		override protected Value EvalF(ushort depth, Value v, Value args) {
			if (v.Values.Length != 4)
				return new(T.NaN());
			int from = (int)Math.Round(T.Re(v.Values[1].Leaf)),
				to = (int)Math.Round(T.Re(v.Values[2].Leaf));
			if (Math.Abs(from - to) > Context._iterLimit)
				return new(T.NaN()); // iteration range over limit, perhaps accidental huge/infinity value in the range?
			var iteratorIndex = args.Values.Length;
			//_args = new(new Value[iteratorIndex + 1]);
			Array.Copy(args.Values, _args.Values, iteratorIndex);
			_args.Values[iteratorIndex] = new(T.NaN(), v.Error, v.Values[0].Text);
			//var exp = new Expression(Context, v.Values[3].Text, ni);
			return Result(EvalK, from, to);
			Value EvalK(int f) {
				_args.Values[iteratorIndex].Leaf = T.MakeR(f);
				return depth < Context._stackOverflow ? _expr.Eval((ushort)(1 + depth), _args) : None;
			}
		}
		virtual protected void Op(ref Value result, Value iteration) => result = iteration;
		protected abstract Value Result(Func<int, Value> eval, int from, int to);
		static protected void Iterate(Action<int> iter, int from, int to) {
			// add the other iterations all the way to "to"
			while (from < to) iter(++from);
			while (from > to) iter(--from);
		}
		public override GpuValue GpuParse(ushort depth) => depth > Context._stackOverflow ? new() : new(_op, base.GpuParse((ushort)(1 + depth)), new([(NaN(_args), _expr, null)]));
		private static Value NaN(Value a) {
			if (a.Values.Length <= 0)
				return a;
			foreach (var i in a.Values)
				NaN(i);
			a.Leaf = T.NaN();
			return a;
		}
	}
	private abstract class CollapseIterator(Comparser<T> context, CallFunction parent, OpCode op, ref string text, Value args) : Iterator(context, parent, op, ref text, args) { 
		override protected Value Result(Func<int, Value> eval, int from, int to) {
			var sum = eval(from).Copy(); // prepare first iteration as the initial vector
			Iterate(IterK, from, to);
			return sum;
			void IterK(int f) {
				var v = eval(f);
				Op(ref sum, v);
				//for (var j = v.Values.Length; 0 <= --j; Op(ref sum[j], v[j])) { }
			}
		}
	}
	// return a vector of sums of iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (x,2x); sum(0,1,3,exp(k0)) => (1+2+3,2+4+6) => (6,12); // 6 is the sum of x term, evaluated with k0=1..3, 12 is the sum of 2x term, evaluated with k0=1..3
	private class Sum(Comparser<T> context, CallFunction parent, ref string text, Value args) : CollapseIterator(context, parent, OpCode.Sum, ref text, args) { 
		override protected void Op(ref Value result, Value iteration) => result = Value.Operate2(result, iteration, INumber<T>.Add, (x, y) => x + y);
	}
	// return a vector of products of iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (x,2x); prod(0,1,3,exp(k0)) => (1*2*3,2*4*6) => (6,48); // 6 is the product of x term, evaluated with k0=1..3, 48 is the product of 2x term, evaluated with k0=1..3
	private class Product(Comparser<T> context, CallFunction parent, ref string text, Value args) : CollapseIterator(context, parent, OpCode.Prod, ref text, args) {
		override protected void Op(ref Value result, Value iteration) => result = Value.Operate2(result, iteration, INumber<T>.Mul, (x, _) => x);
	}
	// returns a vector of first elements of evaluated iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (3x,2x,4x); vector(0,1,3,exp(k0)) => (3*1,3*2,3*3) => (3,6,9); // only took the first 3x term, evaluated with k0=1..3
	private class Vector(Comparser<T> context, CallFunction parent, ref string text, Value args) : Iterator(context, parent, OpCode.Vec,ref text, args) {
		override protected Value Result(Func<int, Value> eval, int from, int to) { 
			var size = 1 + Math.Abs(from - to);
			Value sum = new(new Value[size]) { Values = { [0] = eval(from)/*.Values[0]*/ } };
			var iteratorIndex = 0;
			Iterate(IterK, from, to);
			return sum;
			void IterK(int f) => Op(ref sum.Values[++iteratorIndex], eval(f)/*.Values[0]*/);
		}
	}
	#endregion

	#region Function Expressions - Custom
	// User defined custom expression functions
	public class CallCustom(/*string name, */(Value input, Expression def, Expression? condition)[] def, int cache = 1) : CallFunction(cache) {
		public (Value input, Expression def, Expression? condition)[] Def = def;
		public override Expression Call(Comparser<T> context, ref string text, Value args) => new CustomFunc(context, this, ref text, args);
	}
	private class CustomFunc(Comparser<T> context, CallCustom parent, ref string text, Value args) : FunctionExpression(context, parent, OpCode.Call, ref text, args) {
		override protected Value EvalF(ushort depth, Value v, Value args) {
			if (depth > Context._stackOverflow)
				return StackOverflow;
			var match = -1; for (var m = 0; m < parent.Def.Length; ++m)
				if (parent.Def[m].input.Match(v) && Cond((ushort)(1 + depth), parent.Def[m].condition, v)) {
					match = m;
					break;
				}
				//var ok = true; for (var id = 0; id < parent.Def[m].input.Values.Length; ++id) ok &= parent.Def[m].input[id].Match(v[id]);if (ok) { match = m; break; }
				return match == -1 ? None : parent.Def[match].def.EvalCopy((ushort)(1 + depth), v); // failed to match any available argument list ? else eval.
		}
		private static bool Cond(ushort depth, Expression? e, Value v) {
			if (e == null)
				return true;
			var l = e.Eval(depth, v);
			while (l.Values.Length > 0)
				l = l.Values[0];
			return INumber<T>.IsTrue(l.Leaf);
		}
		public override GpuValue GpuParse(ushort depth) => depth > Context._stackOverflow ? new() : new(OpCode.Call, base.GpuParse((ushort)(1 + depth)), parent);
	}
	#endregion
}