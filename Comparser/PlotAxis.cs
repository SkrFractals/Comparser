using Comparser.Comparser.Numbers;
using static Comparser.Comparser.Numbers.ILeaf;
using static Comparser.Forms.PlotPanel;
namespace Comparser.Comparser;
public interface IPlotAxis {
	public (string c, string e) SetS(object? v);
	public (string s, string e) SetC(object? v);
	public (string s, string c) SetE(object? v);
	public void SetSce(object? s, object? e);
	public void SetL(int l);
	//public string SetLog(bool l);
	public SceState SetLength(int l);
	public void SetLockRange(bool l); // ILeaf/*<T>*/ODO make control
	public SceState GetSce();
	
}
public /*abstract*/  partial class Comparser/*<T>*/{
	public partial class Plot {
		public class PlotAxis(Comparser/*<T>*/ c, ILeaf/*<T>*/ initStart, ILeaf/*<T>*/ initStep, int initLength = 0) : IPlotAxis {

			public PlotAxis(PlotAxis copy, int div = 0) : this(copy._context, copy.start, copy.d, copy.length) 
				=> d = Mul(d, (Real)((double)length / (length >>= div)));

            private readonly Comparser/*<T>*/ _context = c;
			public void SetL(int l) => Locked = l;

			public (string c, string e) SetS(object? v) {
				//if (_lc && _le)
				//	return GetCe();
				ILeaf/*<T>*/ t;
				if (v is Value vs && !(t = vs.GetLeaf()).IsNaN())
					start = t;
				return GetCe();
			}
			public (string s, string e) SetC(object? v) {
				//if (_ls && _le)
				//	return GetSe();
				ILeaf/*<T>*/ t;
				if (v is Value vc && !(t = vc.GetLeaf()).IsNaN())
					center = t;
				return GetSe();
			}
			public (string s, string c) SetE(object? v) {
				//if (_ls && _lc)
				//	return GetSc();
				ILeaf/*<T>*/ t;
				if (v is Value ve && !(t = ve.GetLeaf()).IsNaN())
					end = t;
				return GetSc();
			}
			public void SetSce(object? s, object? e) {
				Locked = 0; // do not restore, lock restoration is called right after this
				ILeaf/*<T>*/ t;
				if (s is Value vs && !(t = vs.GetLeaf()).IsNaN())
					start = t;
				if (e is Value ve && !(t = ve.GetLeaf()).IsNaN())
					start = t;
			}
			//public string SetLog(bool l);
			public SceState SetLength(int l) {
				var prevE = end;
				length = l;
				end = prevE;
				return GetSce();
			}
			public void SetLockRange(bool l) => _lockRange = l;
			public SceState GetSce() => new(start.ToString(_context.Decimals), center.ToString(_context.Decimals), end.ToString(_context.Decimals), Locked);
			private (string c, string e) GetCe() => (center.ToString(_context.Decimals), end.ToString(_context.Decimals));
			private (string s, string e) GetSe() => (start.ToString(_context.Decimals), end.ToString(_context.Decimals));
			private (string s, string c) GetSc() => (start.ToString(_context.Decimals), center.ToString(_context.Decimals));

			/*public bool log
			{
				get;
				set
				{
					if (field == (field = value))
						return;
					DirtyL = true;
					Sv = value ? ScreenIToValueLog : ScreenToValueLin;
					Vs = value ? ValueToScreenLog : ValueToScreenLin;
				}
			} = initLog;*/
			private bool _lockRange;
			public ILeaf/*<T>*/ start
			{
				get;
				set
				{
					var prevCenter = center;
					var prevEnd = end;
					if (field == (field = value)) return;
					switch (Locked) {
						case 1: d = Mul((Real)(2.0 / length), Sub(prevCenter, value)); break;
						case 2: d = Div(Sub(prevEnd, value), (Real)length); break;
					}
					DirtyL = true;
				}
			} = initStart; // how many steps from 0 to the left/top edge?
			public ILeaf/*<T>*/ d
			{
				get;
				set
				{
					if (field == (field = value)) return;
					DirtyL = true;
				}
			} = initStep; // how much ILeaf/*<T>*/ space will one pixel to the right move?
			public ILeaf/*<T>*/ Sample(double i) => Add(start, Mul((Real)i, d)); // right/bottom edge in ILeaf/*<T>*/ space
			public ILeaf/*<T>*/ size => Mul((Real)length, d);
			public ILeaf/*<T>*/ center
			{
				get => Div(Add(start, end), (Real)2); // right/bottom edge in ILeaf/*<T>*/ space
				private set {
					ILeaf/*<T>*/ n;
					switch (Locked) {
						case 0:
							n = Mul((Real)(2.0 / length), Sub(value, start));
							if (d == n) return;
							d = n;
							return;
						case 2: n = Sub(Mul((Real)2, value), end);  /*n = 2 * (end - value);*/ break;
						default: n = Sub(Add(start, value), center); /*n = value - .5 * length * d;*/ break;
					}
					if (start == n) return;
					start = n;
				}
			}
			public ILeaf/*<T>*/ end
			{
				get => Sample(length); // right/bottom edge in ILeaf/*<T>*/ space
				private set {
					ILeaf/*<T>*/ n;
					switch (Locked) {
						case 0:
							n = Div(Sub(value, start), (Real)length);
							if (d == n) return;
							d = n;
							return;
						case 1: n = Sub(Mul((Real)2, center), value);/*value - .5 * length * d;*/break;
						default:/* n = value - d * length; */n = Sub(Add(start, value), end); break;
					}
					if (start == n) return;
					start = n;
				} 
			}
			public readonly Func<int, ILeaf/*<T>*/, ILeaf/*<T>*/, ILeaf/*<T>*/> Sv = ScreenToValueLin;//initLog ? ScreenIToValueLog : ScreenILeaf/*<T>*/oValueLin;
			public readonly Func<ILeaf/*<T>*/, ILeaf/*<T>*/, ILeaf/*<T>*/, int> Vs = ValueToScreenLin;//initLog ? ValueILeaf/*<T>*/oScreenLog : ValueILeaf/*<T>*/oScreenLin;
			public int length
			{
				get;
				set
				{
					if (field != (field = value)) DirtyL = true;
				}
			} = initLength;
			public bool DirtyL = true; //, _dirtyR = true; // Lines / PlotRange
			public int Locked = -1;
			public Color[] lines {
				get
				{
					if (!DirtyL) return field;
					DirtyL = false;
					const int divBase = 10;
					Lin(start, end);
					return field;
				
				void Lin(ILeaf/*<T>*/ lStart, ILeaf/*<T>*/ lEnd) { 
					field = new Color[length];
					// TODO this
					/*(lStart, lEnd) = (ILeaf<T>.*/ //D2(lStart, lEnd, Math.Min), ILeaf/*<T>*/.D2(lStart, lEnd, Math.Max));
					//var h = Lc(Sub(lEnd, lStart));
					/*if (lEnd.Equals(lStart)) return;*/
					//var mod = ILeaf/*<T>*/.D1(Sub(lEnd, lStart), x => Math.Log(Math.Abs(x)) / Math.Log(divBase) % 1); 
					//byte divided = 64;
					//var lineB = new ILeaf/*<T>*/[4];
					//lineB[0] = Mul((Real)divided, Sub((Real)1, mod));
					//for (int i = 1; i < lineB.Length - 2; ++i)
					//	lineB[i] = Mul((Real)(divided >>= 1), Add((Real)1, mod));
					//lineB[^2] = Mul(one, (Real)divided);
					//lineB[^1] = Mul(mod, (Real)divided);
					//for (byte b = 0; b < lineB.Length; ++b, h = Div(h, (Real)10)) {
					//	ILeaf/*<T>*/ fs = Floor(Div(lStart, h)), fe = Floor(Div(lEnd, h));
					//	for (int i = (int)ILeaf/*<T>*/.Mix(fs, Math.Min), e = (int)ILeaf.Mix(fe, Math.Max); i <= e; ++i)
					//		ILeaf/*<T>*/.IndexAndAddToRgb(field, Floor(Mul((Real)length, D2(Sub(Mul((Real)i, h), start), Sub(end, start), Static.Div))), lineB[b]);
					//}
					
				}
				ILeaf/*<T>*/ Lc(ILeaf/*<T>*/ c) => ILeaf/*<T>*/.D1(c, (x) => Math.Round(Math.Pow(divBase, Math.Floor(Math.Log(Math.Abs(x)) / Math.Log(divBase)))));
				}
			} = []; // plot lines
			public bool Zoom(int c, double zoomSize) {
				if (_lockRange) return false;
				var l = Locked; // temporarily disable pinning, so that moving start will only translate the image
				Locked = -1;
				start = Sub(Lerp(start, end, (double)c / length), Mul((Real)c, d = Mul(d,(Real)zoomSize)));
				Locked = l;
				return true;//_dirtyR = true;
			}
			public bool Shift(int pixels) {
				if (_lockRange) return false;
				var l = Locked; // temporarily disable pinning, so that moving start will only translate the image
				Locked = -1;
				start = Add(start, Mul((Real)pixels, d));
				Locked = l;
				return true;
			}

			public int ValueToScreen(ILeaf/*<T>*/ v) => Vs(v, start, d);
			public ILeaf ScreenToValue(int x) => Sv(x, start, d);
		}
	}
}