namespace Comparser.Comparser;
public abstract partial class Comparser<T> {
	public enum OpCode : byte {
		Nop = 0,
		NotAvailable = 1,
		
		// Values
		Leaf,
		Argument,
		DefaultArg,
		//ArgumentPath, // what do you mean by this?

		// Functions
		Call,
		
		// Operators
		Neg,
		Less,
		LessEqual,
		//More,
		//MoreEqual,
		Equal,
		NotEqual,
		Add,
		Mod,
		Mul,
		Inv,
		Pow,
		
		// DefaultFunctions
		True,	// bool
		//False,
		Max,	// sorts
		//Min,
		SoftMax,
		//SoftMin,
		//SoftAbs,
		//SoftNeg,
		Clamp,
		Exp,	//explog
		//Exp10,
		//Exp2,
		Log,
		//Log10,
		//Log2,
		//Nsinhc,	//special trigs
		//Sinhc,
		//Nsinc,
		//Sinc,
		//Cosc,
		
		//hyp
		Cosh,
		Sinh,
		//Tanh,
		//Coth,
		//Sech,
		//Csch,
		
		//trig
		Cos,
		Sin,
		//Tan,
		//Cot,
		//Sec,
		//Csc,
		
		// archyp
		Acosh,
		Asinh,
		Atanh,		// = [A=X];Mul(Log(Mul(Add(1,A),NegAdd(1,Neg(A)))),.5)
		Acoth,		// = [A=X];Mul(Log(Mul(Add(1,A),NegAdd(A,-1))),.5)
		//Asech,
		//Acsch,
		
		//arctrig
		Acos,
		//Asin,
		Atan,		// =  // [A=X];Mul(Neg(U),Mul(Log(Mul(Add(V,A),NegAdd(V,Neg(A)))),.5))
		//Acot,		
		//Asec,
		//Acsc,
		
		// Components
		Re,	
		Im,
		ImMag,
		ImCoef,
		//Frac,
		Trunc,
		Floor,
		Round,
		//Ceil,
		//Sgn,
		Absri,
		SqrAbs,
		Abs,
		Arg,
		Conj,
		CompMod,
		//Sqrt,
		//Sqr,
		//Cbrt,
		//Cub,
		//Quart,
		
		// Special
		Factorial,
		//Gauss,
		Gamma,
		Zeta,
		
		// Vectors
		Vector, // Values[]
		Sum,	// call SUM<index,from,to>(from,to,expr)
		Prod,	// call PROD<index,from,to>(expr)
		Vec,	// call VEC<index,from,to>(expr)
		Index,	// vector[i] Extract Indices
		Cat,	// concatenate nested vector into top level
		Count,  // count the number of top level elements
		
		// ------------------------------------------------------------------------
		
		// Substituted during bytecode making
		More,		// = Less(Neg(X))
		MoreEqual,	// = LessEqual(Neg(X))
		False,		// = 1 - True(X)
		Min,		// = Neg(Max(Neg(X)))
		SoftMin,	// = Neg(SoftMax(Neg(X)))
		SoftAbs,	// = SoftMax(0,X)
		SoftNeg,	// = Neg(SoftMax(0,Neg(X)))
		Exp10,		// = Exp(Mul(ln10,X))
		Exp2,		// = Exp(Mul(ln2,X))
		Log10,		// = Div(Log(X),ln10)
		Log2,		// = Div(Log(X),ln2)
		Nsinhc,		// = X == 0 ? 1 : [A=Mul(pi,X)];Div(Sinh(A),A)
		Sinhc,		// = X == 0 ? 1 : Div(Sinh(X),X)
		Nsinc,		// = X == 0 ? 1 : [A=Mul(pi,X)];Div(Sin(A),A)
		Sinc,		// = X == 0 ? 1 : Div(Sin(X),X)
		Cosc,		// = X == 0 ? 0 : (1-cos(x))/x
		Ncosc,		// = X == 0 ? 0 : (1-cos(pix))/(pix)
		Coshc,		// = X == 0 ? 0 : (1-cosh(x))/x
		Ncoshc,		// = X == 0 ? 0 : (1-cosh(pix))/(pix)
		// hyp
		//Cosh,
		//Sinh,		// = Mul(Sgn(X),Sqrt(Add(Sqr(Cosh(X)),-1)))
		Tanh,		// = Mul(Sinh(X),Inv(Cosh(X)))
		Coth,		// = Mul(Cosh(X),Inv(Sinh(X)))
		Sech,		// = Inv(Cosh(X))
		Csch,		// = Inv(Sinh(X))
		// trig
		//Cos,
		//Sin,		// = Cos(Add(qtau,Neg(X))
		Tan,		// = Mul(Ain(X),Inv(Cos(X)))
		Cot,		// = Mul(Cos(X),Inv(Sin(X)))
		Sec,		// = Inv(Cos(X))
		Csc,		// = Inv(Sin(X))
		// archyp
		//Acosh,
		//Asinh,	// = ?
		//Atanh,
		//Acoth		// = ?
		Asech,		// = Acosh(Inv(X))
		Acsch,		// = Asinh(Inv(X))
		// arctrig
		//Acos,
		Asin,		// = Add(qtau,Neg(Acos(X)))
		//Atan,		// = Mul(Sgn(x),Acos(Inv(Sqrt(Add(1,Sqr(X))))))
		Acot,		// = Add(qtau,Neg(Atan(X)))
		Asec,		// = Acos(Inv(X))
		Acsc,		// = Add(qtau,Neg(Acos(Div(X)))
		// components
		Frac,		// = Sub(X,Trunc(X))
		Ceil,		// = Neg(Floor(Neg(X)))
		Sgn,		// = Mul(X,Inv(Abs(X)))
		Sqrt,		// = Pow(X,.5)
		Cbrt,		// = Pow(X,1.0/3)
		Sqr,		// = Mul(X,X)
		Cub,		// = Mul(X,X,X)
		Quart,		// = Mul(X,X,X,X)
		// Special
		Gauss,		// = Exp(Neg(Sqr(X)))
	}
}