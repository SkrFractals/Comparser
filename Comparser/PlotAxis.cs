using Comparser.Comparser.Numbers;
namespace Comparser.Comparser;
public abstract partial class Comparser<T>{
	public abstract partial class Plot {
		public class PlotAxis(bool initLog, T initStart, T initStep, int initLength = 0){
			public bool log
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
			} = initLog;
			public bool LockRange = false;
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
			public T end => Sample(length); // right/bottom edge in T space
			public Func<int, T, T, T> Sv = initLog ? ScreenToValueLog : ScreenToValueLin;
			public Func<T, T, T, int> Vs = initLog ? ValueToScreenLog : ValueToScreenLin;
			public int length
			{
				get;
				set
				{
					if (value != (field = value)) DirtyL = true;
				}
			} = initLength;
			public bool DirtyL = true; //, _dirtyR = true; // Lines / PlotRange
			public Color[] lines
			{
				get
				{
					if (!DirtyL) return field;
					DirtyL = false;
				const int divBase = 10;
				//(double[] r, double[] i) axis = (new double[length], new double[length]);
				if (log) {
					var eStart = T.D1(start, Math.Exp);
					var eEnd = T.D1(end, Math.Exp);
					Log(eStart, eEnd);
					//Log(T.Re(eStart), T.Re(eEnd), T.Im(eStart), T.Im(eEnd));
				} else { Lin(start, end); }
				return field;
				
				/*void Abs(ref double si, ref double ei) {
					(si, ei) = (Math.Min(si, ei), Math.Max(si, ei));
					//return (ei - si) < 1e-8;
				}*/
				void Lin(T lStart, T lEnd) {
					(lStart, lEnd) = (T.D2(lStart, lEnd, Math.Min), T.D2(lStart, lEnd, Math.Max));
					var h = Lc(lEnd - lStart);
					var mod = T.D1(lEnd - lStart, (x) => Math.Log(Math.Abs(x)) / Math.Log(divBase) % 1); //T.D1(lEnd - lStart, (x) => Math.Pow(2,Math.Log(Math.Abs(x)) / Math.Log(divBase) % 1));
					byte divided = 64;
					var lineB = new T[4];
					lineB[0] = divided * (1 - mod);
					for (int i = 1; i < lineB.Length - 2; ++i)
						lineB[i] = (divided >>= 1) * (1 + mod);
					lineB[^2] = T.One() * divided;
					lineB[^1] = mod * divided;

					/*lineB[0] = 128;
					for(byte i = 1, d = 1; i < lineB.Length; ++i, d <<= 1)
						lineB[i] = (byte)(lineB[0] / (d*mod));
					lineB[^1] = (byte)(lineB[^1] / (mod * mod * 2));
					lineB[^2] = (byte)(lineB[^2] / mod);*/
					for (byte b = 0; b < lineB.Length; ++b, h /= 10) {
						T fs = T.Floor(lStart / h), fe = T.Floor(lEnd / h);
						for (int i = (int)T.Mix(fs, Math.Min), e = (int)T.Mix(fe, Math.Max); i <= e; ++i)
							T.IndexAndAddToRgb(field, T.Floor( length * T.D2(i * h - start, end - start, Static.Div)), lineB[b]);
					}
					//length * T.D2(value - start, end - start, Static.Div);
				}
				/*void DrawLine(T s, T a) {
					
					if (s.r >= 0 && s.r < length)
						axis.r[(int)s.r] += a.r;
					if (s.i >= 0 && s.i < length)
						axis.i[(int)s.i] += a.i;
				}*/
				void Log(T eStart, T eEnd /*double sr, double er,double si, double ei, byte[] a, Func<T, double> take*/) {
					(eStart, eEnd) = (T.D2(eStart, eEnd, Math.Min), T.D2(eStart, eEnd, Math.Max));
					var scale = divBase * Lc(eEnd); // find some power of 10 that is above the ei bounds, to go down from there
					// main tiling squares (only this dividing should have equal distances visually)
					//double minS = T.Mix(eStart, eEnd, Math.Min);
					while (true /*scale > minS*/) {
						scale /= divBase;
						//byte f = 0;
						IndexLog(scale, 64);
						//T.IndexAndAddToRgb(field,T.Floor(ValueToScreen(scale)), 64 * T.One());
						Lg(scale, scale * divBase, 32);
						var s = divBase * scale;
						var es = eStart;
						
						if(T.Mix(s,Math.Max) < T.Mix(es, Math.Min))
							break;
					}
					return;
					// subdivisions of each subinterval into 10 linear pieces.
					void Lg(T from, T to, byte b) {
						if (b < 12)
							return;
						var newTo = from;
						for (double i = 0; i <= .95;) {
							var newFrom = newTo;
							IndexLog(newTo = (i += 1.0 / 9) * to + (1 - i) * from, b);
							//T.IndexAndAddToRgb(field,T.Floor(Vs(newTo = (i += 1.0 / 9) * to + (1 - i) * from)), b * T.One());
							//if(newFrom < ei && newTo > si)
							Lg(newFrom, newTo, ++b);
						}
					}
					void IndexLog(T v, byte b) => T.IndexAndAddToRgb(field,T.Floor(length * T.D2(T.D1(v, Math.Log) - start, end - start, Static.Div)), b * T.One());
					
					/*var scale = divBase*L(ei); // find some power of 10 that is above the ei bounds, to go down from there
					// main tiling squares (only this dividing should have equal distances visually)
					while (scale > si) {
						scale /= divBase;
						if (scale >= ei)
							continue;
						var screenSpaceLine = ValueToScreenLog(screenLength, scale, start, end);
						if(screenSpaceLine > 0 && screenSpaceLine < screenLength)
							a[screenSpaceLine] = Math.Max(lineB[0], a[screenSpaceLine]);
						if(divBase * scale > si)
							Lg(scale, scale * divBase, 0);
					}
					return;
					// subdivisions of each subinterval into 10 linear pieces.
					void Lg(double from, double to, byte b) {
						if (b >= lineB.Length)
							return;
						var newTo = from;
						for (double i = 0; i <= .95; ) {
							var newFrom = newTo;
							newTo = (i += 1.0 / 9) * to + (1 - i) * from;
							var screenSpaceLine = ValueToScreenLog(screenLength, newTo, start, end);
							if(screenSpaceLine > 0 && screenSpaceLine < screenLength)
								a[screenSpaceLine] = Math.Max(b, a[screenSpaceLine]);
							if(newFrom < ei && newTo > si)
								Lg(newFrom, newTo, --b);
						}
					}*/
				}
				T Lc(T c) => T.D1(c, (x) => Math.Round(Math.Pow(divBase, Math.Floor(Math.Log(Math.Abs(x)) / Math.Log(divBase)))));
				//double L(double x) => Math.Round(Math.Pow(divBase, Math.Floor(Math.Log(Math.Abs(x)) / Math.Log(divBase))));
				}
				//private set;
			} = []; // plot lines
			public bool Adjust(double zoomOut) { // called when resizing the window and not lock ranged, returns if it happened, when it does, it should keep the memory inside
				if (LockRange) return false;
				// TODO change to step/start and calculate memory (the eval should remember its last start/shift/x/y and then look at the new ones and figure out which pixels are reused (move them and then reeval the rest))
				T center = (end + start) / 2, diff = start - center;
				d *= zoomOut;
				start = center + diff * zoomOut;
				//_dirtyR = true;
				return true;
			}
			public bool Zoom(int center, double zoomSize) {
				if (LockRange) return false;
				start = INumber<T>.Lerp(start, end, (double)center / length) + center * (d *= zoomSize);
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