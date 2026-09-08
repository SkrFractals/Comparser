using Comparser.Comparser.Numbers;
using Comparser.Forms;
namespace Comparser.Comparser;
public interface IPlotAxis {
	public (string, string) SetS(object? v);
	public (string, string) SetC(object? v);
	public (string, string) SetE(object? v);
	//public string SetLog(bool l);
	public void SetLength(int l);
	public void SetLockRange(bool l); // TODO make control
	public PlotControl.SceState GetSce();
	
}
public abstract partial class Comparser<T>{
	public partial class Plot {
		public class PlotAxis(T initStart, T initStep, int initLength = 0) : IPlotAxis {
			public (string, string) SetS(object? v) {
				T t;
				if (v is Value n && !(t=n.GetLeaf()).IsNaN())
					start = t;
				return (center.ToString()!, end.ToString()!);
			}
			public (string, string) SetC(object? v){
				T t;
				if (v is Value n && !(t=n.GetLeaf()).IsNaN())
					center = t;
				return (start.ToString()!, end.ToString()!);
			}
			public (string, string) SetE(object? v){
				T t;
				if (v is Value n && !(t=n.GetLeaf()).IsNaN())
					end = t;
				return (start.ToString()!, center.ToString()!);
			}
			//public string SetLog(bool l);
			public void SetLength(int l) => length = l;
			public void SetLockRange(bool l) => LockRange = l;
			public PlotControl.SceState GetSce() => (start.ToString()!, center.ToString()!, end.ToString()!, Loc);
			
			/*public bool log
			{
				get;
				set
				{
					if (value == (field = value))
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
					if (T.AreEqual(value, field = value)) return;
					DirtyL = true;
				}
			} = initStart; // how many steps from 0 to the left/top edge?
			public T d
			{
				get;
				set
				{
					if (T.AreEqual(value, field = value)) return;
					DirtyL = true;
				}
			} = initStep; // how much T space will one pixel to the right move?
			public T Sample(double i) => start + i * d; // right/bottom edge in T space
			public T size => length * d;
			public T center
			{
				get => (start + end) / 2; // right/bottom edge in T space
				set => start += center - value;
			}
			public T end
			{
				get => Sample(length); // right/bottom edge in T space
				set => d = (value - start) / length;
			}
			public Func<int, T, T, T> Sv = ScreenToValueLin;//initLog ? ScreenToValueLog : ScreenToValueLin;
			public Func<T, T, T, int> Vs = ValueToScreenLin;//initLog ? ValueToScreenLog : ValueToScreenLin;
			public int length
			{
				get;
				set
				{
					if (value != (field = value)) DirtyL = true;
				}
			} = initLength;
			public bool DirtyL = true; //, _dirtyR = true; // Lines / PlotRange
			public Color[] lines {
				get
				{
					if (!DirtyL) return field;
					DirtyL = false;
					const int divBase = 10;
					Lin(start, end);
					return field;
				
				void Lin(T lStart, T lEnd) {
					(lStart, lEnd) = (T.D2(lStart, lEnd, Math.Min), T.D2(lStart, lEnd, Math.Max));
					var h = Lc(lEnd - lStart);
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
			public bool Adjust(double zoomOut) { // called when resizing the window and not lock ranged, returns if it happened, when it does, it should keep the memory inside
				if (LockRange) return false;
				// TODO change to step/start and calculate memory (the eval should remember its last start/shift/x/y and then look at the new ones and figure out which pixels are reused (move them and then reeval the rest))
				T c = center, diff = start - c;
				d *= zoomOut;
				start = c + diff * zoomOut;
				//_dirtyR = true;
				return true;
			}
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