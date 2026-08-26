using Comparser.Comparser.Numbers;
namespace Comparser.Comparser;
public abstract partial class Comparser<T> {
	public class Operator(byte order = 0, bool right = false, int eatOp = 1) {
		public bool Negative;
		public readonly byte Order = order; // order of operations
		public readonly bool Right = right; // right-associativity
		public int EatOp = eatOp; // using an operator symbol? (if false, it is an operator-less multiplication)
		public virtual T Op(T value, T operand) => Negative ? -value : value;
		public virtual string SOp(string value, string operand) => value;
		public virtual GpuValue Gop(GpuValue term, GpuValue operand) => term;
		protected T Neg(T term) => Negative ? -term : term;
		protected double Neg(double term) => Negative ? -term : term;
		protected GpuValue GpuNeg(GpuValue term) => Negative ? new(OpCode.Neg, term) : term;
	}
	private class Less(bool orEqual) : Operator(1, false, orEqual ? 2 : 1) { 
		public override T Op(T value, T operand) => INumber<T>.True(orEqual
			? Neg(T.Re(value)) <= T.Re(operand) 
			: Neg(T.Re(value)) < T.Re(operand));
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], orEqual ? OpCode.LessEqual : OpCode.Less);
	}
	private class More(bool orEqual) : Operator(1, false, orEqual ? 2 : 1) { public override T Op(T value, T operand) => INumber<T>.True(orEqual
		?  Neg(T.Re(value)) >= T.Re(operand) : Neg(T.Re(value))> T.Re(operand));
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], orEqual ? OpCode.MoreEqual : OpCode.More);
	}
	private class Equal() : Operator(1) {
		public override T Op(T value, T operand) => INumber<T>.True(T.AreEqual(Neg(value), operand)); 
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], OpCode.Equal);
	}
	private class Exclamation(byte order = 6) : Operator(order,false, order == 1 ? 2 : 1) {
		public override T Op(T value, T operand) => INumber<T>.True(!T.AreEqual(Neg(value), operand));
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], OpCode.NotEqual);
	}
	private class Add() : Operator(2) { 
		public override T Op(T value, T operand) => Neg(value) + operand;
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], OpCode.Add);
		public override string SOp(string value, string operand) => value + operand;
	}
	private class Sub() : Operator(2) {	
		public override T Op(T value, T operand) => Neg(value) - operand;
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), new(OpCode.Neg, operand)], OpCode.Add);
		public override string SOp(string value, string operand) => operand == "" ? value : value.Replace(operand, "");
	}
	private class Mod() : Operator(3) {
		public override T Op(T value, T operand) => Neg(value) % operand; 
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], OpCode.Mod);
	}
	private class Div() : Operator(3) {
		public override T Op(T value, T operand) => operand.Is0() ? T.NaN() : Neg(value) / operand;
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), new(OpCode.Inv, operand)],OpCode.Mul);
	}
	private class LDiv() : Operator(3) {
		public override T Op(T value, T operand) => operand.Is0() ? T.NaN() : T.LDiv(Neg(value), operand); 
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([new(OpCode.Inv, operand), GpuNeg(term)], OpCode.Mul);
	}
	// if ": Operator(4, ...)", then 1/2*3 could be 1/6, but left associativity for "*/" matches computer evals
	private class Mul(bool eatOp = true) : Operator(3, false, eatOp ? 1 : 0) {
		public override T Op(T value, T operand) => Neg(value) * operand; 
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], OpCode.Mul);
	}
	private class Pow() : Operator(5, true) {
		public override T Op(T value, T operand) => Neg(operand.Is0() ? T.MakeR(1) : value ^ operand); 
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], OpCode.Pow);
	}
	private class Root() : Operator(5, true) {
		public override T Op(T value, T operand) => Neg(operand.Is0() ? T.MakeR(1) : value ^ T.Inv(operand)); 
		public override GpuValue Gop(GpuValue term, GpuValue operand)
			=> new([GpuNeg(term), operand], OpCode.Pow);
	}
	// Implemented as encapsulated function, these Ops are just for parsing:
	private class Index() : Operator(6) { public override T Op(T value, T operand) => T.NaN(); }
}