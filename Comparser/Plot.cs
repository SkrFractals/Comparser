using System.Drawing.Imaging;
namespace Comparser.Comparser;
public abstract partial class Comparser<T>{
	public abstract partial class Plot {
		public class PlotOutput(PlotAxis newA, PlotEval newEval, PlotOutput.ColorMode mode, Expression newRgb, Expression newHsv) {
			public class ChannelProperties {
				public enum Bounds { Clamp, Loop }
				public enum Scale { Lin, Log, Exp, Custom }
				public Bounds B = Bounds.Clamp;
				public Scale S = Scale.Lin;
				public Expression? CustomScale;
			}
			public enum ColorMode { Rgb, Hsv }
			public ColorMode Mode = mode;
			public Expression ColorCodeRgb = newRgb; // default=rgb(value)
			public Expression ColorCodeHsv = newHsv; // default=hsv(value)
			public PlotAxis A = newA; // axis
			public PlotEval Eval = newEval; // evaluator (code, default=z)
			public Value[] Values = []; // evaluated value buffer
			public ChannelProperties R = new(), G = new(), B = new(), H = new(), S = new(), V = new();
			public Color ProcessColor(T value) => Color.White; // TODO process the Hsv/RgbColor
		}

		public enum DrawMode : byte {
			XFill, // X -> Fill Y
			XContour, // X -> Contour Y
			Xy // XY -> Color BMP
		}
		public readonly List<PlotAxis> InputR; /* = [
			new(false, new(-10),new(10)), // X input (everywhere) [W]
			new(false, new(0,10),new(0,-10))]; // Y input (only XY->HSV modes) [H]*/
		public readonly List<PlotOutput> OutputR; /* = [
			new(false, new(10),new(-10)), // first output (everywhere) [H]
			new(false, new(0,10),new(0,-10))]; // second output (only X->Y modes) [H]*/
		public DrawMode Mode = DrawMode.XContour;
		public int FixedY = 0;
		public int Frame = 0;
		public bool LockAspectRatio = true;
		protected readonly Comparser<T> Context;
		private Bitmap _bmp = new(0,0);
		private Color[] _linesX = [], _linesY = [];
		private bool _dirtyX = true, _dirtyXy = true;
		
		protected Plot(Comparser<T> comparser, List<PlotAxis> inputR, List<PlotOutput> outputR, int width, int height) {
			InputR = inputR;
			OutputR = outputR;
			Context = comparser;
			//Eval = [new(Context = comparser, "x!")];
			//OutputHsv = new(Context = comparser, "[repeatValue=1; (1)s(x)=sqrabs(x)] (arg(x)+pi)360/tau, 1-exp(-s(x)), sqrt(s(x))%repeatValue /* repeatValue: Value cycle slowness, (1)s(x): caches sqrabs for reuse", _x);
			Update(width, height);
		}
		public void Resize(int w, int h) { // on screen panel resize
			int pw = InputR[0].length, ph = InputR[1].length, s = w * h;
			if (ResizeX() && ResizeY())
				return;
			_bmp = new(w, h);
			
			
			int r;
			if (LockAspectRatio && (r = pw * h - ph * w) != 0) {
				// TODO finish this
				
				if (r > 0) { // TODO test if this is not backwards!
					var a = (double)ph * w / (pw * h);
					if (InputR[1].Adjust(a)) {
						// resize to keep aspect ratio - shrink memoryX to the original
					} else if (w != pw) {
						// changed scale - completely dirty X memory
					}
					foreach (var o in InputR) o.Adjust(a);
				} else InputR[0].Adjust((double)pw * h / (ph * w));
				return;
			}
			_dirtyX = _dirtyXy = true;
			return;
			bool ResizeX() {
				if (w == _linesX.Length)
					return true;
				_linesX = new Color[w];
				_dirtyX = true;
				InputR[0].length = w;
				return false;
			}
			bool ResizeY() {
				if (h == _linesY.Length)
					return true;
				_linesY = new Color[w];
				_dirtyXy = true;
				InputR[1].length = h;
				return false;
			}
		}
		public void ZoomContinuous(int x, int y, double size) {
			if(InputR[0].Zoom(x, size))
				_dirtyXy = _dirtyX = true;
			var zy = (double)y / InputR[1].length;
			if(InputR[1].Zoom(y, size))
				_dirtyXy = true;;
			foreach (var o in OutputR)
				o.A.Zoom(y, size);
		}
		public void ZoomBinary(int x, int y, bool zoomIn) => ZoomContinuous(x, y, zoomIn ? .5 : 2);
		public void Shift(int dx, int dy) {
			//double rx = (double)dx / InputR[0].length, ry = (double)dy / InputR[1].length;
			if (dx != 0 && InputR[0].Shift(dx))
				_dirtyXy = _dirtyX = true;
			if (dx != 0 && InputR[1].Shift(dx))
				_dirtyXy = true;
			foreach (var o in OutputR)
				o.A.Shift(dy);
		}
		

		private Bitmap Update(int w, int h) {
			Resize(w, h); // if size changed, it will resize everything and mark things dirty
			var dirty = InputR[0].DirtyL;
			// prepare axis lines and plot values if they are dirty
			switch (Mode) {
			case DrawMode.XContour:
			case DrawMode.XFill:
				dirty |= _dirtyX;
				_dirtyX = false;
				foreach (var output in OutputR)
					dirty |= output.A.DirtyL;
				if (dirty) {
					Lines(InputR[0], _linesX);
					foreach (var l in OutputR)
						Lines(l.A, _linesY);
				}
				foreach (var o in OutputR) {
					o.Values = o.Eval.GetPlotX(out var d, InputR[0], InputR[1], FixedY, InputR[2], Frame);
					dirty |= d; // Refresh 1D (X,FixedY) output values
				}
				break;
			default: // XY mode:
				dirty = InputR[1].DirtyL || _dirtyXy;
				_dirtyXy = false;
				if (dirty) { 
					Lines(InputR[0], _linesX); // X input axis lines
					Lines(InputR[1], _linesY); // Y input axis lines
				}
				foreach (var t in OutputR) {
					t.Values = t.Eval.GetPlotXy(out var d, InputR[0], InputR[1], InputR[2], Frame);
					dirty |= d; // Refresh 2D (X,Y) output values
				}
				break;

				void Lines(PlotAxis a, Color[] axis) {
					if (axis.Length != a.length)
						throw new("given the axis a different length of colors to draw axes to, than the last length it was set to.");
					for (var i = 0; i < axis.Length; ++i) // combine axis lines
						axis[i] = Max(axis[i], a.lines[i]);
				}
			}
			if (!dirty) // nothing has changed, no need to redraw the screen
				return _bmp;
			dirty = false;
			unsafe {
				var l = _bmp.LockBits(new(0, 0, _bmp.Width, _bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
				byte* p, ptr = (byte*)(void*)l.Scan0;
				var intPtr = 0;
				switch (Mode) {
				case DrawMode.XContour:
				case DrawMode.XFill:
					for (var y = 0; y < _bmp.Height; ++y) {
						p = ptr + l.Stride * y;
						var yColor = _linesY[y];
						for (var x = 0; x < _bmp.Width; ++x, ++intPtr, p += 3) {
							var c = Max( _linesX[x], yColor);
							T v;
							if (Mode == DrawMode.XContour) 
								foreach (var o in OutputR) {
									Value[] prev = o.Values[x].GetValues(), next = o.Values[Math.Min(x + 1, o.Values.Length-1)].GetValues();
									for (int i = 0; i < prev.Length; ++i) 
										if (C(o.A.ValueToScreen(v = prev[i].GetLeaf()), o.A.ValueToScreen( next[i].GetLeaf())))
											c = Max(c, o.ProcessColor(v));
								}
							else foreach (var o in OutputR) foreach (var prevV in o.Values[intPtr].GetValues())
								if (y < 0 == o.A.ValueToScreen(v = prevV.GetLeaf()) < y)
									c = Max(c, o.ProcessColor(v = prevV.GetLeaf()));
							(p[2],p[1],p[0]) = (c.R,c.G,c.B);
						}
						continue;
						bool C(int v, int n) => y < v != y <= n || y <= v != y < n;
					}
					break;
				default:
					for (var y = 0; y < _bmp.Height; ++y) {
						p = ptr + l.Stride * y;
						var yColor = _linesY[y];
						for (var x = 0; x < _bmp.Width;  ++x, ++intPtr, p += 3) {
							var c = Max(_linesX[x], yColor);
							int value;
							foreach (var o in OutputR) 
								foreach (var prevV in o.Values[intPtr].GetValues()) 
									c = Max(c, o.ProcessColor(prevV.GetLeaf()));
							(p[2],p[1],p[0]) = (c.R,c.G,c.B);
						}
					}
					break;
				}
				_bmp.UnlockBits(l);
			}
			return _bmp;
			Color Max(Color a, Color b) => Color.FromArgb(Math.Max(a.R, b.R), Math.Max(a.G, b.G), Math.Max(a.B, b.B));
		}
		public static int ValueToScreenLin(T value, T start, T d) => (int)(T.Re(!d * (value - start))/+d);//length * T.D2(value - start, end - start, Static.Div);
		public static T ScreenToValueLin(int x, T start, T d) => start + x * d;//INumber<T>.Lerp(start, end, new((double)x / length));
		public static int ValueToScreenLog(T value, T start, T d) => ValueToScreenLin(T.D1(value, Math.Log), start, d);//length * ((T.D1(value, Math.Log) - start) / (end - start));
		public static T ScreenToValueLog(int x, T start, T d) => T.D1(ScreenToValueLin(x, start, d),Math.Exp); //T.D1(ScreenToValueLin(length, x, start, end), Math.Exp);
	}
}
// PlotForm: (opened with a button in ComparserFrom, global code in ComparserForm)
// -Mode switch - XFill,XContour, XY
// -check: lock aspect ratio
// -refresh mode: Always, After 1s, Never (always call refresh with movements, after 1s call it after a timer wasn't reset with movements for 1s, never never)
// -Input X Axis
// -Input Y Axis
// -(only X): fixedY
// -Add Output
			
// Outputs:
// -Remove
// -code: default: z
// -color mode: Rgb/Hsv
// -color code (code that is run on each value, and expects to return 3 values):
// --defaultRgb: rgb(value), defaultHsv: hsv(value) ...switches between two codes when you switch color mode
// -Rgb/Hsv, one for eac channel: channelMode - clamp/loop for each, scale - Lin/Log/Exp each, move the code from INumber to here ...also switches two memories
// --it is applied individually to each channel
// Expression (can make vector results)
// Axis (only when X mode): lin/log, Start,End,Center, Start and End changes mutate center, and center edit shifts s and e, LockRange - locks S/E/C.







//private readonly Value _x = new([new(T.NaN(), "x")]), _xy = new([new(T.NaN(), "x"), new(T.NaN(), "y")]);
/*T[] plotGamma = new T[screen.Width + 1],
	plotFact = new T[screen.Width + 1];
Comparser<T>.Value arg, argX = new([new("x")]); // define a single-argument function input with the alias "x"
Comparser<T>.Expression
	factFunc = new(context, "x!", argX),
	gammaFunc = new(context, "gamma(x)", argX); // gamma function with an input x*/

/*for (var x = 0; x <= screen.Width; ++x) {
	// convert screen space into input space and make argument input from it mapping x:inputX
	arg = context.MakeArgs([("x", svx(screen.Width, x, xs, xe))]);
	// evaluate functions with x:inputX
	plotFact[x] = E(factFunc, arg); // "x!"
	plotGamma[x] = E(gammaFunc, arg); // "gamma(x)"
}*/
//foreach (var output in OutputR) {
//Math.Clamp(o.a.Vs(v.GetLeaf(), o.a.start, o.a.d)); //T.Re(!o.a.d * (leaf - o.a.start))/+o.a.d;