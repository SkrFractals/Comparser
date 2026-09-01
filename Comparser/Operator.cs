using Comparser.Comparser.Numbers;
namespace Comparser.Comparser;
public abstract partial class Comparser<T> {
	public enum OpOrder : byte {
		Expression = 0,
		SubExpression = 1,
		Compare = 2,
		Add = 3,
		//Div = 4,
		Mul = 5,
		Pow = 6,
		Unary = 7,
		Index = 8
	}
	public class Operator(OpOrder order = 0, bool right = false, int eatOp = 1) {
	
		public bool Negative;
		public readonly OpOrder Order = order; // order of operations
		public readonly bool Right = right; // right-associativity
		public int EatOp = eatOp; // using an operator symbol? (if false, it is an operator-less multiplication)
		public virtual T Op(T value, T operand) => Negative ? -value : value;
		public virtual string SOp(string value, string operand) => value;
		public virtual GpuValue Gop(GpuValue term, GpuValue operand) => term;
		protected T Neg(T term) => Negative ? -term : term;
		protected double Neg(double term) => Negative ? -term : term;
		protected GpuValue GpuNeg(GpuValue term) => Negative ? new(OpCode.Neg, term) : term;
	}
	private class Less(bool orEqual) : Operator(OpOrder.Compare, false, orEqual ? 2 : 1) { // x < y (less) // x <= y (less equal)
		public override T Op(T value, T operand) => INumber<T>.True(orEqual
			? Neg(T.Re(value)) <= T.Re(operand) 
			: Neg(T.Re(value)) < T.Re(operand));
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], orEqual ? OpCode.LessEqual : OpCode.Less);
	}
	private class More(bool orEqual) : Operator(OpOrder.Compare, false, orEqual ? 2 : 1) { // x > y (more) // x >= y (more equal)
		public override T Op(T value, T operand) => INumber<T>.True(orEqual
		?  Neg(T.Re(value)) >= T.Re(operand) : Neg(T.Re(value))> T.Re(operand));
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], orEqual ? OpCode.MoreEqual : OpCode.More);
	}
	private class Equal() : Operator(OpOrder.Compare) { // x = y (equal)
		public override T Op(T value, T operand) => INumber<T>.True(T.AreEqual(Neg(value), operand)); 
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], OpCode.Equal);
	}
	private class Exclamation(OpOrder order = OpOrder.Index) : Operator(order,false, order < OpOrder.Index ? 2 : 1) { // x! (factorial) // x != y (not equal)
		public override T Op(T value, T operand) => INumber<T>.True(!T.AreEqual(Neg(value), operand));
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], OpCode.NotEqual);
	}
	private class Add() : Operator(OpOrder.Add) { // x + y
		public override T Op(T value, T operand) => Neg(value) + operand;
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], OpCode.Add);
		public override string SOp(string value, string operand) => value + operand;
	}
	private class Sub() : Operator(OpOrder.Add) { // x - y
		public override T Op(T value, T operand) => Neg(value) - operand;
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), new(OpCode.Neg, operand)], OpCode.Add);
		public override string SOp(string value, string operand) => operand == "" ? value : value.Replace(operand, "");
	}
	private class Mul(bool eatOp = true) : Operator(OpOrder.Mul, false, eatOp ? 1 : 0) { // x * y // xy
		public override T Op(T value, T operand) => Neg(value) * operand;
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], OpCode.Mul);
	}// if ": Operator(OpOrder.Div, ...)", for the following 3, then 1/2*3 could be 1/6, but left associativity for "*/" matches computer evals
	private class Mod(bool comp = false) : Operator(OpOrder.Mul) { // x % y (complex mod) // x &% y (comp mod)
		public override T Op(T value, T operand) => comp ? Neg(value) % operand : INumber<T>.CompMod(Neg(value), operand); 
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> comp ? new([GpuNeg(term), operand], OpCode.CompMod) : new([GpuNeg(term), operand], OpCode.Mod);
	}
	private class Div() : Operator(OpOrder.Mul) { // x / y (divide) // /x (inverse)
		public override T Op(T value, T operand) => operand.Is0() ? T.NaN() : Neg(value) / operand;
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), new(OpCode.Inv, operand)],OpCode.Mul);
	}
	private class LDiv() : Operator(OpOrder.Mul) { //  x \ y (left divide)
		public override T Op(T value, T operand) => operand.Is0() ? T.NaN() : T.LDiv(Neg(value), operand); 
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([new(OpCode.Inv, operand), GpuNeg(term)], OpCode.Mul);
	}
	private class Pow() : Operator(OpOrder.Pow, true) { // x ^ y (power)
		public override T Op(T value, T operand) => Neg(operand.Is0() ? T.MakeR(1) : value ^ operand); 
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> GpuNeg(new([term, operand], OpCode.Pow));
	}
	private class Root(bool log = false) : Operator(OpOrder.Pow, true, log ? 2 : 1) { // x $ y x^(1/y) // x $$ y (log_y(x))
		public override T Op(T value, T operand) => Neg(log ? T.Log(value) / T.Log(operand) : operand.Is0() ? T.MakeR(1) : value ^ T.Inv(operand));
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> GpuNeg(log ? new([new(OpCode.Log, term), new(OpCode.Inv, new(OpCode.Log,operand))], OpCode.Mul) : new([term, new(OpCode.Inv, operand)],OpCode.Pow));// new([GpuNeg(term), operand], OpCode.LogB): new([GpuNeg(term), operand], OpCode.Pow);
	}
	private class Sqr() : Operator(OpOrder.Unary, true) { } // x& (sqr(x))
	private class Conj() : Operator(OpOrder.Unary, true) { } // x~ (conj(x))
	private class Abs(bool sqr = false) : Operator(OpOrder.Unary, true, sqr ? 2 : 1) { } // x@ (abs(x)) // x@@ (sqrabs(x))
	private class AbsRi(bool norm = false) : Operator(OpOrder.Unary, true,  norm ? 2 : 1) { } // x| (absri(x)) // x|| (normalize(x))
	private class Count(bool cat = false) : Operator(OpOrder.Unary, true,  cat ? 2 : 1) { } // x| (absri(x)) // x|| (normalize(x))
	
	// Implemented as encapsulated function, these Ops are just for parsing:
	private class Index() : Operator(OpOrder.Index) { public override T Op(T value, T operand) => T.NaN(); }
}