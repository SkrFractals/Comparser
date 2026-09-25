using Comparser.Comparser.Numbers;
namespace Comparser.Comparser;
public /*abstract*/  partial class Comparser/*<T> where T : unmanaged, IScalar<T>*/ {
	#region Call Functions
	// abstract parent
	public abstract class CallFunction(int cacheSize = 0) {
		//protected readonly I[]? Def = def;
		//public readonly string Name = name;
		public readonly EvalCache Cache = new(cacheSize);
		public abstract Expression Call(Reader read, Value args);
		// how to use: e.Insert(args, e.GetEval(args) ? e.result.Eval : base.Eval([], args).v); 
		public class EvalCache(int size = 0) {
			//#if NOCACHE
			//private readonly int _size =  0;
			//#else
			private readonly int _size = size;
			//#endif
			//private List<(Value args, Value eval)> Debug = [];
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
				if (_size <= _filled) {
					if (_size == 1) _cache = null; 
					else Result?.Next = null; 
				} else ++_filled;
				if (_size > 0) _cache = new(_cache, args, eval);
				//Debug.Add((args,eval));
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
		public override Expression Call(Reader read, Value args) => (FunctionExpression)Activator.CreateInstance(type, read, this, args)!; //from = (int)a[3]; // ref int from
	}
	// Single/Double/Triple argument delegated functions
	public class Cf(Func<ILeaf/*<T>*/, ILeaf/*<T>*/> del, OpCode op, int cache = 1) : CallFunction(cache) {
		public override Expression Call(Reader read, Value args) => new FuncOperator(read, this, del, op, args);
	}
	public class Cf2(Func<ILeaf/*<T>*/, ILeaf/*<T>*/, ILeaf/*<T>*/> del, OpCode op, int cache = 1) : CallFunction(cache) {
		public override Expression Call(Reader read, Value args) => new FuncOperator2(read, this, del, op, args);
	}
	public class Cf3(Func<ILeaf/*<T>*/, ILeaf/*<T>*/, ILeaf/*<T>*/, ILeaf/*<T>*/> del, OpCode op, int cache = 1) : CallFunction(cache) {
		public override Expression Call(Reader read, Value args) => new FuncOperator3(read, this, del, op, args);
	}
	public class Cf4(Func<ILeaf/*<T>*/, ILeaf/*<T>*/, ILeaf/*<T>*/, ILeaf/*<T>*/, ILeaf/*<T>*/> del, OpCode op, int cache = 1) : CallFunction(cache) {
		public override Expression Call(Reader read, Value args) => new FuncOperator4(read, this, del, op, args);
	}
	#endregion

	#region Function Expressions - Operators
	protected abstract class FunctionExpression : Expression {
		private readonly CallFunction _parent;
		protected readonly OpCode OpCode;
		protected FunctionExpression(Reader read, CallFunction parent, OpCode op, Value args) 
			: base(read, out _, args) => (_parent, OpCode) = (parent, op);
		protected FunctionExpression(Comparser/*<T>*/ context, CallFunction parent, OpCode op, Value input) 
			: base(context, input) => (_parent, OpCode) = (parent, op);
		public override Value Eval(ushort depth, Value args/*, string text = ""*/, bool allowCache = true, bool collapse = true) {
			var v = base.Eval(depth, args/*, text*/, allowCache, false);
			return allowCache ? _parent.Cache.GetEval(v) ? _parent.Cache.Result?.Eval! : _parent.Cache.Insert(v, EvalF(depth, v, args, allowCache)) : EvalF(depth, v, args, allowCache);
		}
		protected abstract Value EvalF(ushort depth, Value v, Value args, bool allowCache);
		public override GpuValue GpuParse(ushort depth) => new(OpCode, base.GpuParse(depth));
		static protected Value Triple((double a, double b, double c) v) => new([new((Real)v.a), new((Real)(v.b)), new((Real)(v.c))]);
	}
	private class FuncTextOperator(Reader read, CallFunction parent, Func<ushort, string, Value> del, Value args)
		: FunctionExpression(read, parent, OpCode.NotAvailable, args) {
		override protected Value EvalF(ushort depth, Value v, Value args, bool allowCache) => Context.AllowStrings ? Value.OperateString(depth, v, del) : v;
	} // type, read, this, args
	private class FuncEval(Reader read, CallFunction parent, Value args) : FuncTextOperator(read, parent, 
			(d, x) => d > read.Context._stackOverflow ? StackOverflow : new Expression(new(read.Context, x, read.Cancel), out _, args).Eval((ushort)(1 + d), args, false), args) { }
	private class FuncOperator : FunctionExpression {
		private readonly Func<ILeaf, ILeaf> _del;
		public FuncOperator(Reader read, CallFunction parent, Func<ILeaf, ILeaf> del, OpCode op, Value args) : base(read, parent, op, args) => _del = del;
		public FuncOperator(Comparser/*<T>*/ context, CallFunction parent, Func<ILeaf, ILeaf> del, OpCode op, Value input) : base(context, parent, op, input) => _del = del;
		override protected Value EvalF(ushort _, Value v, Value args, bool allowCache) => Value.Operate(v, _del);
	}
	private class FuncOperator2(Reader read, CallFunction parent, Func<ILeaf, ILeaf, ILeaf> comp, OpCode op, Value args) : FunctionExpression(read, parent, op, args) {
		override protected Value EvalF(ushort depth, Value v, Value args, bool allowCache) {
			switch (v.Values.Length) {
			case 0: return v;
			case 1: return v.Values[0];
			default:
				var s = v.Values[0];
				for (var c = 1; c < v.Values.Length; ++c)
					s = Value.Operate2(s, v.Values[c], comp, (x, _) => x, depth, Context, None, allowCache);
				return s;
			}
		}
	}
	private class FuncOperator3(Reader read, CallFunction parent, Func<ILeaf, ILeaf, ILeaf, ILeaf> comp, OpCode op, Value args) : FunctionExpression(read, parent, op, args) {
		override protected Value EvalF(ushort _, Value v, Value args, bool allowCache) => v.Values.Length == 3 ? Value.Operate3(v.Values[0], v.Values[1], v.Values[2], comp) : new();
	}
	private class FuncOperator4(Reader read, CallFunction parent, Func<ILeaf, ILeaf, ILeaf, ILeaf, ILeaf> comp, OpCode op, Value args) : FunctionExpression(read, parent, op, args) {
		override protected Value EvalF(ushort _, Value v, Value args, bool allowCache) => v.Values.Length == 4 ? Value.Operate4(v.Values[0], v.Values[1], v.Values[2], v.Values[3], comp) : new();
	}
	#endregion

	#region Function Expressions - Colors
	private class FuncColorSpace(Reader read, CallFunction parent, Value args, OpCode oc, Func<(double, double, double), (double, double, double)> del) : FunctionExpression(read, parent, oc, args) {
		override protected Value EvalF(ushort depth, Value v, Value args, bool allowCache) => (v = CollapseScalar(v)).Values.Length switch {
			0 => None,
			3 => Triple(del((v.Values[0].Re(), v.Values[1].Re(), v.Values[2].Re()))),
			_ => new(v.Values[0].GetLeaf()),
		};
	}
	private class FuncRgb2Hsv(Reader read, CallFunction parent, Value args) : FuncColorSpace(read, parent, args, OpCode.Rgb2Hsv, Static.Rgb2Hsv) { }
	private class FuncHsv2Rgb(Reader read, CallFunction parent, Value args) : FuncColorSpace(read, parent, args, OpCode.Hsv2Rgb, Static.Hsv2Rgb) { }
	private class FuncHsv(Reader read, CallFunction parent, Value args, OpCode oc, Func<ILeaf/*<T>*/, (double, double, double)> del) : FunctionExpression(read, parent, oc, args) {
		override protected Value EvalF(ushort depth, Value v, Value args, bool allowCache) => Value.OperateValue(v, To, null);
		virtual protected Value To(Value val, object? _) => Triple(del(val.GetLeaf()/*, 1*/));
	}
	private class FuncRgb(Reader read, CallFunction parent, Value args, OpCode oc, Func<ILeaf/*<T>*/, (double, double, double)> del) : FuncHsv(read, parent, args, oc, (_)=>(0,0,0)) {
		override protected Value To(Value val, object? _) => Triple(Static.Hsv2Rgb(del(val.GetLeaf()/*, 1*/)));
	}
	private class FuncLog2Hsv(Reader read, CallFunction parent, Value args) : FuncHsv(read, parent, args, OpCode.Log2Hsv, ILeaf/*<T>*/.Log2Hsv) { }
	private class FuncLog2Rgb(Reader read, CallFunction parent, Value args) : FuncRgb(read, parent, args, OpCode.Log2Rgb, ILeaf/*<T>*/.Log2Hsv) { }
	private class FuncLin2Hsv(Reader read, CallFunction parent, Value args) : FuncHsv(read, parent, args, OpCode.Lin2Hsv, ILeaf/*<T>*/.Lin2Hsv) { }
	private class FuncLin2Rgb(Reader read, CallFunction parent, Value args) : FuncRgb(read, parent, args, OpCode.Lin2Rgb, ILeaf/*<T>*/.Lin2Hsv) { }
	private class FuncLog2HsvC(Reader read, CallFunction parent, Value args) : FuncHsv(read, parent, args, OpCode.Log2HsvC, ILeaf/*<T>*/.Log2HsvC) { }
	private class FuncLog2RgbC(Reader read, CallFunction parent, Value args) : FuncRgb(read, parent, args, OpCode.Log2RgbC, ILeaf/*<T>*/.Log2HsvC) { }
	private class FuncLin2HsvC(Reader read, CallFunction parent, Value args) : FuncHsv(read, parent, args, OpCode.Lin2HsvC, ILeaf/*<T>*/.Lin2HsvC) { }
	private class FuncLin2RgbC(Reader read, CallFunction parent, Value args) : FuncRgb(read, parent, args, OpCode.Lin2RgbC, ILeaf/*<T>*/.Lin2HsvC) { }
	//private class FuncExp2hsv(Reader read, CallFunction parent, Value args) : FuncHsv(read, parent, args, OpCode.Exp2Hsv, INumber<T>.Exp2Hsv) { }
	//private class FuncExp2rgb(Reader read, CallFunction parent, Value args) : FuncRgb(read, parent, args, OpCode.Exp2Rgb, INumber<T>.Exp2Hsv) { }

	#endregion

	#region Function Expressions - Vectors
	// extracts terms from a vector using indices in: [expression]. Example: (0a,1b,2c,(30d,31e),5f)[3,2,(5,1,3)] = (30d,31e),2c,(5,1,(30d,31e))
	private class FuncIndex(Comparser/*<T>*/ context, Value input, Value indices) : Expression(context, input) {
		public override Value Eval(ushort depth, Value args/*, string text = ""*/, bool allowCache = true, bool collapse = true) 
			=> depth > Context._stackOverflow ? StackOverflow : Value.OperateValue(EvalValue((ushort)(1 + depth), CollapseScalar(indices), args, allowCache), Take, base.Eval(depth, args/*, text*/, allowCache));
		private Value Take(Value from, object? i) {
			if (i is not Value v)
				return from;
			v = CollapseScalar(v);
			var index = CollapseScalar(from).Leaf.Re();
			int integer;
			return double.IsNaN(index) || (integer = (int)Math.Round(index)) < 0 || integer >= v.Values.Length ? None : v.Values[integer];
		}
		public override GpuValue GpuParse(ushort depth) => new([GpuParseValue(depth, indices), base.GpuParse(depth)], OpCode.Index);
	}
	private class FuncCat(Reader read, CallFunction parent, Value args) : FunctionExpression(read, parent, OpCode.Cat, args) {
		override protected Value EvalF(ushort depth, Value v, Value args, bool allowCache) {
			List<Value> cat = [];
			if (0 == v.Values.Length) cat.Add(new(v.Leaf));
			else Operate(v);
			return new([.. cat]);
			void Operate(Value r) {
				var vV = r.Values;
				var s = vV.Length;
				if (0 == s) cat.Add(new(r.Leaf));
				else for (var i = 0; i < s; ++i)
					Operate(vV[i]);
			}
		}
	}
	// counts the elements in a vector
	private class FuncCount : FunctionExpression {
		private FuncCount(Reader read, CallFunction parent, Value args) : base(read, parent, OpCode.Count, args) { }
		public FuncCount(Comparser/*<T>*/ context, CallFunction parent, Value args) : base(context, parent, OpCode.Count, args) { }
		override protected Value EvalF(ushort depth, Value v, Value args, bool allowCache) => new((Real)(Math.Max(1, CollapseScalar(v).Values.Length)));
	}
	private class FuncCatCount : FunctionExpression {
		private FuncCatCount(Reader read, CallFunction parent, Value args) : base(read, parent, OpCode.Count, args) { }
		public FuncCatCount(Comparser/*<T>*/ context, CallFunction parent, Value args) : base(context, parent, OpCode.Count, args) { }
		override protected Value EvalF(ushort depth, Value v, Value args, bool allowCache) => new((Real)(Operate(v)));
		private static int Operate(Value v) {
			var vV = v.Values;
			int c = 0, s = vV.Length;
			if (0 == s) return 1;
			for (var i = 0; i < s; ++i)
				c += Operate(vV[i]);
			return c;
		}
	}
	// iterative sum/product: name(<index>,<from>,<to>,expression(k<index>))
	// "to" can be smaller than "from", works both ways (does not return additive/multiplicative identity when in the wrong order, just iterates backwards)
	private abstract class Iterator : FunctionExpression {
		protected Iterator(Reader read, CallFunction parent, OpCode op, Value args) : base(read, parent, op, args) {
			_preEvaluatable = false;
			var iteratorIndex = args.Values.Length;
			if (V.Values.Length != 4) {
				_expr = new(new(read.Context, "", read.Cancel), out _, _args = None);
				return;
			}
			_args = new(new Value[iteratorIndex + 1]);
			Array.Copy(args.Values, _args.Values, iteratorIndex);
			_args.Values[iteratorIndex] = new(nan, 0, V.Values[0].String);
			_expr = new(new(read.Context, V.Values.Length == 4 ? V.Values[3].String : "", read.Cancel), out _, _args);
		}
		private readonly Expression _expr;
		private readonly Value _args;
		override protected Value EvalF(ushort depth, Value v, Value args, bool allowCache) {
			if (v.Values.Length != 4)
				return new(nan);
			int from = (int)Math.Round(v.Values[1].Leaf.Re()),
				to = (int)Math.Round(v.Values[2].Leaf.Re());
			if (Math.Abs(from - to) > Context._iterOverflow)
				return new(nan); // iteration range over limit, perhaps accidental huge/infinity value in the range?
			var iteratorIndex = args.Values.Length;
			//_args = new(new Value[iteratorIndex + 1]);
			var argsCopy = _args.Copy();
			Array.Copy(args.Values, argsCopy.Values, iteratorIndex);//Array.Copy(args.Values, _args.Values, iteratorIndex);
			argsCopy.Values[iteratorIndex] = new(nan, v.Error, v.Values[0].String);//_args.Values[iteratorIndex] = new(nan, v.Error, v.Values[0].String);
			//var exp = new Expression(Context, v.Values[3].Text, ni);
			return Result(EvalK, from, to, allowCache);
			Value EvalK(int f) {
				argsCopy.Values[iteratorIndex].Leaf = (Real)f;//_args.Values[iteratorIndex].Leaf = (Real)f;
				return depth < Context._stackOverflow ? _expr.Eval/*Copy*/((ushort)(1 + depth), argsCopy, allowCache) : None;//return depth < Context._stackOverflow ? _expr.Eval/*Copy*/((ushort)(1 + depth), _args, allowCache) : None;
			}
		}
		virtual protected void Op(ref Value result, Value iteration, bool allowCache) => result = iteration;
		protected abstract Value Result(Func<int, Value> eval, int from, int to, bool allowCache);
		static protected void Iterate(Action<int> iter, int from, int to) {
			// add the other iterations all the way to "to"
			while (from < to) iter(++from);
			while (from > to) iter(--from);
		}
		public override GpuValue GpuParse(ushort depth) => depth > Context._stackOverflow ? new() : new(OpCode, base.GpuParse((ushort)(1 + depth)), new([(NaN(_args), _expr, null)]));
		private static Value NaN(Value a) {
			if (a.Values.Length <= 0)
				return a;
			foreach (var i in a.Values)
				NaN(i);
			a.Leaf = nan;
			return a;
		}
	}
	private abstract class CollapseIterator(Reader read, CallFunction parent, OpCode op, Value args) : Iterator(read, parent, op, args) { 
		override protected Value Result(Func<int, Value> eval, int from, int to, bool allowCache) {
			var sum = eval(from).Copy(); // prepare first iteration as the initial vector
			Iterate(IterK, from, to);
			return sum;
			void IterK(int f) {
				var v = eval(f);
				Op(ref sum, v, allowCache);
				//for (var j = v.Values.Length; 0 <= --j; Op(ref sum[j], v[j])) { }
			}
		}
	}
	// return a vector of sums of iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (x,2x); sum(0,1,3,exp(k0)) => (1+2+3,2+4+6) => (6,12); // 6 is the sum of x term, evaluated with k0=1..3, 12 is the sum of 2x term, evaluated with k0=1..3
	private class Sum(Reader read, CallFunction parent, Value args) : CollapseIterator(read, parent, OpCode.Sum, args) { 
		override protected void Op(ref Value result, Value iteration, bool allowCache) => result = Value.Operate2(result, iteration, ILeaf/*<T>*/.Add, (x, y) => x + y, 0, Context, None, allowCache);
	}
	// return a vector of products of iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (x,2x); prod(0,1,3,exp(k0)) => (1*2*3,2*4*6) => (6,48); // 6 is the product of x term, evaluated with k0=1..3, 48 is the product of 2x term, evaluated with k0=1..3
	private class Product(Reader read, CallFunction parent, Value args) : CollapseIterator(read, parent, OpCode.Prod, args) {
		override protected void Op(ref Value result, Value iteration, bool allowCache) => result = Value.Operate2(result, iteration, ILeaf/*<T>*/.Mul, (x, _) => x, 0, Context, None, allowCache);
	}
	// returns a vector of first elements of evaluated iterated expressions with the extra argument i as the iteration value
	// example: exp(x) = (3x,2x,4x); vector(0,1,3,exp(k0)) => (3*1,3*2,3*3) => (3,6,9); // only took the first 3x term, evaluated with k0=1..3
	private class Vector(Reader read, CallFunction parent, Value args) : Iterator(read, parent, OpCode.Vec, args) {
		override protected Value Result(Func<int, Value> eval, int from, int to, bool allowCache) { 
			var size = 1 + Math.Abs(from - to);
			Value sum = new(new Value[size]) { Values = { [0] = eval(from)/*.Values[0]*/ } };
			var iteratorIndex = 0;
			Iterate(IterK, from, to);
			return sum;
			void IterK(int f) => Op(ref sum.Values[++iteratorIndex], eval(f)/*.Values[0]*/, allowCache);
		}
	}
	#endregion

	#region Function Expressions - Custom
	// User defined custom expression functions
	public class CallCustom(/*string name, */(Value input, Expression def, Expression? condition)[] def, int cache = 0) : CallFunction(cache) {
		public (Value input, Expression def, Expression? condition)[] Def = def;
		public override Expression Call(Reader read, Value args) => new CustomFunc(read, this, args);
	}
	private class CustomFunc(Reader read, CallCustom parent, Value args) : FunctionExpression(read, parent, OpCode.Call, args) {
		override protected Value EvalF(ushort depth, Value v, Value args, bool allowCache) {
			if (depth > Context._stackOverflow)
				return StackOverflow;
			var av = UnCollapseVector(v);
			var match = -1;
			for (var m = 0; m < parent.Def.Length; ++m) {
				if (parent.Def[m].input.Match(v)) {
					if(Cond((ushort)(1 + depth), parent.Def[m].condition, v, allowCache)) {
						match = m;
						break;
					}
					continue;
				}
				if (!(parent.Def[m].input.Match(av) && Cond((ushort)(1 + depth), parent.Def[m].condition, av, allowCache)))
					continue;
				match = m;
				v = av;
				break;
			}
			//var ok = true; for (var id = 0; id < parent.Def[m].input.Values.Length; ++id) ok &= parent.Def[m].input[id].Match(v[id]);if (ok) { match = m; break; }
			return match == -1 ? None : parent.Def[match].def.EvalCopy((ushort)(1 + depth), v, allowCache); // failed to match any available argument list ? else eval.
		}
		private static bool Cond(ushort depth, Expression? e, Value v, bool allowCache) {
			if (e == null) 
				return true;
			
			var l = e.Eval(depth, v, allowCache);
			while (l.Values.Length > 0)
				l = l.Values[0];
			return  l.Leaf.IsTrue();
		}
		public override GpuValue GpuParse(ushort depth) => depth > Context._stackOverflow ? new() : new(OpCode.Call, base.GpuParse((ushort)(1 + depth)), parent);
	}
	#endregion
}