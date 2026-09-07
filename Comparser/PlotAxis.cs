using Comparser.Comparser.Numbers;
namespace Comparser.Comparser;
public interface IPlotAxis {
	public (string c, string e) SetS(object? v);
	public (string s, string e) SetC(object? v);
	public (string s, string c) SetE(object? v);
	public void SetL(int l);
	//public string SetLog(bool l);
	public (string s, string c, string e) SetLength(int l);
	public void SetLockRange(bool l); // TODO make control
	public (string s, string c, string e) GetSce();
	
}
public abstract partial class Comparser<T>{
	public partial class Plot {
		public class PlotAxis(Comparser<T> c, T initStart, T initStep, int initLength = 0) : IPlotAxis {

			private Comparser<T> _context = c;
			public void SetL(int l) => Locked = l;

			public (string c, string e) SetS(object? v) {
				//if (_lc && _le)
				//	return GetCe();
				T t;
				if (v is Value n && !(t=n.GetLeaf()).IsNaN())
					start = t;
				return GetCe();
			}
			public (string s, string e) SetC(object? v) {
				//if (_ls && _le)
				//	return GetSe();
				T t;
				if (v is Value n && !(t=n.GetLeaf()).IsNaN())
					center = t;
				return GetSe();
			}
			public (string s, string c) SetE(object? v) {
				//if (_ls && _lc)
				//	return GetSc();
				T t;
				if (v is Value n && !(t=n.GetLeaf()).IsNaN())
					end = t;
				return GetSc();
			}
			//public string SetLog(bool l);
			public (string s, string c, string e) SetLength(int l) {
				var prevE = end;
				length = l;
				end = prevE;
				return GetSce();
			}
			public void SetLockRange(bool l) => LockRange = l;
			public (string s, string c, string e) GetSce() => (start.ToString(_context.Decimals), center.ToString(_context.Decimals), end.ToString(_context.Decimals));
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
					Sv = value ? ScreenToValueLog : ScreenToValueLin;
					Vs = value ? ValueToScreenLog : ValueToScreenLin;
				}
			} = initLog;*/
			public bool LockRange;
			public T start
			{
				get;
				set
				{
					var prevCenter = center;
					var prevEnd = end;
					if (T.AreEqual(field, field = value)) return;
					switch (Locked) {
						case 1: d = 2 * (prevCenter - value) / length;break;
						case 2: d = (prevEnd - value) / length; break;
					}
					DirtyL = true;
				}
			} = initStart; // how many steps from 0 to the left/top edge?
			public T d
			{
				get;
				set
				{
					if (T.AreEqual(field, field = value)) return;
					DirtyL = true;
				}
			} = initStep; // how much T space will one pixel to the right move?
			public T Sample(double i) => start + i * d; // right/bottom edge in T space
			public T size => length * d;
			public T center
			{
				get => (start + end) / 2; // right/bottom edge in T space
				set {
					T n;
					switch (Locked) {
						case 0:
							n = 2 * (value - start) / length;
							if (T.AreEqual(d, n)) return;
							d = n;
							return;
						case 2: n = 2 * value - end;  /*n = 2 * (end - value);*/ break;
						default: n = start + value - center; /*n = value - .5 * length * d;*/ break;
					}
					if (T.AreEqual(start, n)) return;
					start = n;
				}
			}
			public T end
			{
				get => Sample(length); // right/bottom edge in T space
				set {
					T n;
					switch (Locked) {
						case 0:
							n = (value - start) / length;
							if (T.AreEqual(d, n)) return;
							d = n;
							return;
						case 1: n = 2 * center - value;/*value - .5 * length * d;*/break;
						default:/* n = value - d * length; */n = start + value - end; break;
					}
					if (T.AreEqual(start, n)) return;
					start = n;
				} 
			}
			public Func<int, T, T, T> Sv = ScreenToValueLin;//initLog ? ScreenToValueLog : ScreenToValueLin;
			public Func<T, T, T, int> Vs = ValueToScreenLin;//initLog ? ValueToScreenLog : ValueToScreenLin;
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
				
				void Lin(T lStart, T lEnd) { 
					field = new Color[length];
					return; // TODO this
					(lStart, lEnd) = (T.D2(lStart, lEnd, Math.Min), T.D2(lStart, lEnd, Math.Max));
					var h = Lc(lEnd - lStart);
					//if (lEnd.Equals(lStart)) return;
					var mod = T.D1(lEnd - lStart, (x) => Math.Log(Math.Abs(x)) / Math.Log(divBase) % 1); 
					byte divided = 64;
					var lineB = new T[4];
					lineB[0] = divided * (1 - mod);
					for (int i = 1; i < lineB.Length - 2; ++i)
						lineB[i] = (divided >>= 1) * (1 + mod);
					lineB[^2] = T.One() * divided;
					lineB[^1] = mod * divided;
					for (byte b = 0; b < lineB.Length; ++b, h /= 10) {
						T fs = T.Floor(lStart / h), fe = T.Floor(lEnd / h);
						for (int i = (int)T.Mix(fs, Math.Min), e = (int)T.Mix(fe, Math.Max); i <= e; ++i)
							T.IndexAndAddToRgb(field, T.Floor( length * T.D2(i * h - start, end - start, Static.Div)), lineB[b]);
					}
				}
				T Lc(T c) => T.D1(c, (x) => Math.Round(Math.Pow(divBase, Math.Floor(Math.Log(Math.Abs(x)) / Math.Log(divBase)))));
				}
			} = []; // plot lines
			/*public bool Adjust(double zoomOut) { // called when resizing the window and not lock ranged, returns if it happened, when it does, it should keep the memory inside
				if (LockRange) return false;
				// TODO change to step/start and calculate memory (the eval should remember its last start/shift/x/y and then look at the new ones and figure out which pixels are reused (move them and then reeval the rest))
				T c = center, diff = start - c;
				d *= zoomOut;
				start = c + diff * zoomOut;
				//_dirtyR = true;
				return true;
			}*/
			public bool Zoom(int c, double zoomSize) {
				if (LockRange) return false;
				start = INumber<T>.Lerp(start, end, (double)c / length) + c * (d *= zoomSize);
				return true;//_dirtyR = true;
			}
			public bool Shift(int pixels) {
				if (LockRange) return false;
				start += pixels * d;
				//_dirtyR = true;
				return true;
			}

			public int ValueToScreen(T v) => Vs(v, start, d);
			public T ScreenToValue(int x) => Sv(x, start, d);
		}
	}
}