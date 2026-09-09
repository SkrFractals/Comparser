using Comparser.Forms;
using System.Drawing.Imaging;
namespace Comparser.Comparser;
public enum PlotMode : byte {
	XFill, // X -> Fill Y
	XContour, // X -> Contour Y
	Xy // XY -> Color BMP
}

public interface IPlot {

	public void ChangeMode(PlotMode xy);
	public (double, string) SetFixedY(object? yf);
	public void SetFrame(object? t);
	public void Resize(int w, int h, int l);
	public void ZoomContinuous(int x, int y, double size);
	public void ZoomBinary(int x, int y, bool zoomIn) => ZoomContinuous(x, y, zoomIn ? .5 : 2);
	public void Shift(int dx, int dy);
	public Bitmap Update(int w, int h, int l);
	public IPlotAxis[] GetAxis();
	public void DelOutput(int output);
	public void AddOutput(string name, object? newRgb, object? newCode);
	public void SetCode(int output, object? code);
	public void SetRgb(int output, object? rgb);
	public object[] GetOutputs();
	public void LockRangeX(bool l);
	public void LockRangeY(bool l);
	public void LockRangeO(bool l);
	public void LockRangeT(bool l);
	public void SetLockRes(bool l);
}
public abstract partial class Comparser<T>{
	public partial class Plot : IPlot {
		public void LockRangeX(bool l) => LockedRangeX = l;
		public void LockRangeY(bool l) => LockedRangeY = l;
		public void LockRangeO(bool l) => LockedRangeO = l;
		public void LockRangeT(bool l) => LockedRangeT = l;
		public void SetLockRes(bool l) => LockedRes = l;
		//public void SetDirty() => _dirtyX = _dirtyXy = _dirtyRgb = true;

		public class PlotOutput(string name, PlotEval? newEval, /*PlotOutput.ColorMode mode,*/ Expression? newRgb/*, Expression? newHsv*/) {
			// TODO TextField?
			/*public class ChannelProperties {
				public enum Bounds { Clamp, Loop }
				public enum Scale { Lin, Log, Exp, Custom }
				public Bounds B = Bounds.Clamp;
				public Scale S = Scale.Lin;
				public Expression? CustomScale;
			}*/
			//public enum ColorMode { Rgb, Hsv }

			public string Name = name;
			//public ColorMode Mode = mode;
			public Expression? ColorCodeRgb = newRgb; // default=rgb(value)
			//public Expression? ColorCodeHsv = newHsv; // default=hsv(value)
			//public PlotAxis A = newA; // axis
			public PlotEval? Eval = newEval; // evaluator (code, default=z)
			public Value[] Values = []; // evaluated value buffer
										//public ChannelProperties R = new(), G = new(), B = new(), H = new(), S = new(), V = new();
			private Value _args = new();
			public void PrepareArgs(int w, int h, int l) {
				_args = new([
					new(T.NaN(), FailReason.Success, "v"),
					new(T.NaN(), FailReason.Success, "c"),
					new(T.NaN(), FailReason.Success, "z"),
					new(T.NaN(), FailReason.Success, "t"),
					new(T.NaN(), FailReason.Success, "x"),
					new(T.NaN(), FailReason.Success, "y"),
					new(T.NaN(), FailReason.Success, "f"),
					new(T.MakeR(w), FailReason.Success, "w"),
					new(T.MakeR(h), FailReason.Success, "h"),
					new(T.MakeR(l), FailReason.Success, "l")
				], FailReason.Success);
			}
			public (double, double, double) ProcessColor(Value? value, (double r, double g, double b) c, T z, T t, double x, double y, double f) {
				_args.Values[0].Values = [value ?? new()];
				_args.Values[1].Values = [new(T.MakeR(c.r)), new(T.MakeR(c.g)), new(T.MakeR(c.b))];
				_args.Values[2].Leaf = z;
				_args.Values[3].Leaf = t;
				_args.Values[4].Leaf = T.MakeR(x);
				_args.Values[5].Leaf = T.MakeR(y);
				_args.Values[6].Leaf = T.MakeR(f);
				var rgb = ColorCodeRgb?.Eval(0, _args);
				if (rgb is null)
					return (0,0,0);
				if (rgb.Values.Length >= 3) {
					var r = (Get(0), Get(1), Get(2));
					return r;
				}
				if (rgb.Values.Length != 1)
					return (0,0,0);
				var light = Get(0);
				return (light, light, light);
				double Get(int i) => T.Re(rgb.Values[i].GetLeaf());
				//byte Get(int i) => (byte)Math.Clamp(255 * T.Re(rgb.Values[i].GetLeaf()), 0, 255);
			}
		}


		public void DelOutput(int output) => OutputR.RemoveAt(output);
		public void AddOutput(string name, object? newRgb, object? newCode) => OutputR.Add(new(name, new(newCode), newRgb as Expression));
		public void SetCode(int output, object? code) => OutputR[output].Eval = new(code);
		public void SetRgb(int output, object? rgb) {
			OutputR[output].ColorCodeRgb = rgb as Expression;
			//_dirtyRgb = true;
		}
		public object[] GetOutputs() {
			string[] r = new string[OutputR.Count];
			for (int i = 0; i < OutputR.Count; ++i) r[i] = OutputR[i].Name;
			return r;
		}

		public IPlotAxis[] GetAxis() => _axis;
		public readonly PlotAxis InputX, InputY, InputT, OutputY;
		//public PlotControl.AxisControls ControlInputX, ControlInputY, ControlInputT, ControlOutputY;
		public readonly List<PlotOutput> OutputR; /* = [
			new(false, new(10),new(-10)), // first output (everywhere) [H]
			new(false, new(0,10),new(0,-10))]; // second output (only X->Y modes) [H]*/
		public PlotMode Mode = PlotMode.XContour;
		public double FixedY;
		public int Frame;
		public int SelectedOutput = -1;
		public bool LockedRangeX = true, LockedRangeY = true, LockedRangeO = true, LockedRangeT = true, LockedRes = false;
		public readonly Comparser<T> Context;
		//private Bitmap _bmp = new(1,1);
		private Color[] _linesX = [], _linesY = [], _linesO = [], _linesT = [];
		//private bool _dirtyX = true, _dirtyXy = true, _dirtyRgb = true;
		private readonly IPlotAxis[] _axis;
		public Plot(Comparser<T> context, int width = 0, int height = 0, int length = 1) {
			_axis = [InputX = new(context, T.Zero(), T.MakeR(1), 1),
				InputY = new(context, T.Zero(), T.One() - T.MakeR(1), 1),
				InputT = new(context, T.Zero(), T.MakeR(1), 1),
				OutputY = new(context, T.Zero(), T.MakeR(1), 1)];
			OutputR = [];
			Context = context;
			//Eval = [new(Context = comparser, "x!")];
			//OutputHsv = new(Context = comparser, "[repeatValue=1; (1)s(x)=sqrabs(x)] (arg(x)+pi)360/tau, 1-exp(-s(x)), sqrt(s(x))%repeatValue /* repeatValue: Value cycle slowness, (1)s(x): caches sqrabs for reuse", _x);
			Update(width, height, length);
		}
		public void ChangeMode(PlotMode xy) {
			Mode = xy;
			//_dirtyX = _dirtyXy = true;
			Update(InputX.length, InputY.length, InputT.length);
		}
		public (double, string) SetFixedY(object? yf) {
			FixedY = Context.AsDouble(yf);
			if (double.IsNaN(FixedY)) FixedY = 0;
			//_dirtyX = true;
			return (FixedY, InputY.Sample(FixedY).ToString(3));
		}
		public void SetFrame(object? t) {
			Frame = (int)Context.AsDouble(t);
			//_dirtyX = true;
		}
		public void Resize(int w, int h, int l) { // on screen panel resize
			if (LockedRes)
				return;
			//bool r = false;
			_dirty |= ResizeDim(LockedRangeX, w, ref _linesX, InputX);
			_dirty |= ResizeDim(LockedRangeY, h, ref _linesY, InputY);
			_dirty |= ResizeDim(LockedRangeO, h, ref _linesO, OutputY);
			_dirty |= ResizeDim(LockedRangeT, l, ref _linesT, InputT);
			return;

			T NewBounds(PlotAxis a) => a.Locked switch { 0 => a.start, 2 => a.end, _ => a.center };
			void Adjust(PlotAxis a, T sce) {
				var l = a.Locked;
				a.Locked = -1; // do it in left align mode for simplicity
				a.start = l switch { 0 => sce, 2 => sce - a.d * a.length, _ => sce - .5 * a.d * a.length };
				a.Locked = l; // and switch back to what3ever mode we were in
			}

			bool ResizeDim(bool locked, int size, ref Color[] lines, /*ref bool dirty,*/ PlotAxis a) {
				var p = a.length;
				if (locked) {
					if (R(ref lines))
						return false;
					a.d *= (double)p / size;
				} else {
					var b = NewBounds(a);
					if (R(ref lines))
						return false;
					Adjust(a, b);
				}
				return true;
				bool R(ref Color[] lines) {
					if (size == lines.Length)
						return true;
					lines = new Color[a.length = size];
					return false;
				}
				/*var p = new int[a.Length];
				for (int i = 0; i < a.Length; ++i) p[i] = a[i].length;
				if (locked) {
					if (R(ref lines))
						return false;
					for (int i = 0; i < a.Length; ++i) a[i].d *= (double)p[i] / size;
				} else {
					var b = new T[a.Length];
					for (int i = 0; i < a.Length; ++i) b[i] = NewBounds(a[i]);
					if (R(ref lines))
						return false;
					for (int i = 0; i < a.Length; ++i) 
						Adjust(a[i], b[i]);
				}
				bool R(ref Color[] lines) {
					if (size == lines.Length)
						return true;
					lines = new Color[size];
					foreach (var aa in a)
						aa.length = size;
					return false;
				}*/
			}
		}
		public void ZoomContinuous(int x, int y, double size) {
			InputX.Zoom(x, size);
			InputY.Zoom(y, size);
			OutputY.Zoom(y, size);
			/*if (InputX.Zoom(x, size))
				_dirtyXy = _dirtyX = true;
			//var zy = (double)y / InputR[1].length;
			if(InputY.Zoom(y, size))
				_dirtyXy = true;;
			if(OutputY.Zoom(y, size))
				_dirtyXy = true;;*/
			//foreach (var o in OutputR)
			//	o.A.Zoom(y, size);
		}
		public void Shift(int dx, int dy) {
			if (dx != 0)
				InputX.Shift(dx);
			if (dy == 0)
				return;
			InputY.Shift(dx);
			OutputY.Shift(dx);
			//double rx = (double)dx / InputR[0].length, ry = (double)dy / InputR[1].length;
			/*if (dx != 0 && InputX.Shift(dx))
				_dirtyXy = _dirtyX = true;
			if (dy == 0)
				return;
			if (InputY.Shift(dx) && Mode != PlotMode.Xy)
				_dirtyXy = true;
			if (OutputY.Shift(dx) && Mode == PlotMode.Xy)
				_dirtyXy = true;*/
		}
		public class Renders {
			public List<Expression?> MyRgb;
			public int Length = -1;
			public PlotMode Mode;
			public Bitmap?[] Bitmaps; // [frames]
			public int W, H;
			public bool GetBitmap(out Bitmap bmp, int w, int h, int length, int frame, PlotMode mode, List<PlotOutput>? rgbs, bool dirty = false) {
				if (RgbMatch() && w == W && h == H && length == Length && mode == Mode && !dirty) {
					if (Bitmaps[frame] == null) {
						bmp = Bitmaps[frame] = new(w, h);
						return false;
					}
					bmp = Bitmaps[frame];
					return true;
				}
				Mode = mode;
				bmp = (Bitmaps = new Bitmap[Length = length])[frame] = new Bitmap(W = w, H = h);
				return false;
				bool RgbMatch() {
					var match = true;
					for (var i = 0; i < rgbs.Count; ++i) {
						if (i < MyRgb.Count) {
							if (MyRgb[i] == rgbs[i].ColorCodeRgb)
								continue;
							MyRgb[i] = rgbs[i].ColorCodeRgb;
						} else MyRgb.Add(rgbs[i].ColorCodeRgb);
						match = false;
					}
					if (rgbs.Count >= MyRgb.Count)
						return match;
					match = false;
					MyRgb.RemoveRange(rgbs.Count, MyRgb.Count - rgbs.Count);
					return match;
				}
			}
		}
		private bool _dirty;
		private Renders R = new();
		public Bitmap Update(int w, int h, int l) {
			Resize(w, h, l); _dirty |= InputX.DirtyL; // if size changed, it will resize everything and mark things dirty
								 //var dirty = InputX.DirtyL;
								 // prepare axis lines and plot values if they are dirty
			if (InputX.DirtyL)
				Lines(InputX, _linesX);
			switch (Mode) {
				case PlotMode.XContour:
				case PlotMode.XFill:
					/*dirty |= _dirtyX;
					_dirtyX = false;
					dirty |= OutputY.DirtyL;
					if (dirty) {
						Lines(InputX, _linesX);
						//foreach (var l in OutputR)
						Lines(OutputY, _linesY);
					}*/
					if(OutputY.DirtyL)
						Lines(OutputY, _linesO);
					foreach (var o in OutputR) {
						if (o.Eval?.Null() ?? true) continue;
						o.Values = o.Eval.GetPlotX(out var d, InputX, InputY, FixedY, InputT, (int)Frame);
						_dirty |= d; // Refresh 1D (X,FixedY) output values
					}
					break;
				default: // XY mode:
					/*dirty = InputY.DirtyL || _dirtyXy;
					_dirtyXy = false;
					if (dirty) {
						Lines(InputX, _linesX); // X input axis lines
						Lines(InputY, _linesY); // Y input axis lines
					}*/
					if (InputY.DirtyL)
						Lines(InputY, _linesY);
					foreach (var o in OutputR) {
						if (o.Eval?.Null() ?? true) continue;
						o.Values = o.Eval.GetPlotXy(out var d, InputX, InputY, InputT, (int)Frame);
						_dirty |= d; // Refresh 2D (X,Y) output values
					}
					break;
			}
			void Lines(PlotAxis a, Color[] axis) {
				//if (axis.Length != a.length)
				//	axis = new Color[a.length];//a.length = axis.Length;
				//throw new("given the axis a different length of colors to draw axes to, than the last length it was set to.");
				for (var i = 0; i < axis.Length; ++i) // combine axis lines
					axis[i] = Max(axis[i], a.lines[i]);
			}
			if (R.GetBitmap(out var _bmp, w, h, l, Frame, Mode, OutputR, _dirty)) // nothing has changed, no need to redraw the screen
				return _bmp;
			_dirty = false;
			unsafe {
				if (_bmp.Width != _linesX.Length || _bmp.Height != _linesY.Length)
					return _bmp;
				var lb = _bmp.LockBits(new(0, 0, _bmp.Width, _bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
				byte* p, ptr = (byte*)(void*)lb.Scan0;
				var intPtr = 0;
				var t = InputT.Sample(Frame);

				switch (Mode) {
					case PlotMode.XContour:
					case PlotMode.XFill:
						foreach (var o in OutputR)
							o.PrepareArgs(InputX.length, OutputY.length, InputT.length);
						var yz = OutputY.Sample(FixedY);
						for (var y = 0; y < _bmp.Height; ++y) {

							p = ptr + lb.Stride * y;
							var yColor = _linesY[y];
							for (var x = intPtr = 0; x < _bmp.Width; ++x, ++intPtr, p += 3) {
								var z = InputX.Sample(x) + yz;
								var rgbc = Max(_linesX[x], yColor);
								(double r, double g, double b) c = (rgbc.R / 255.0, rgbc.G / 255.0, rgbc.B / 255.0);
								//T v;
								if (Mode == PlotMode.XContour)
									foreach (var o in OutputR) {
										if (o.Eval?.Null() ?? true) continue;
										Value[] prev = o.Values[x].GetValues(), next = o.Values[Math.Min(x + 1, o.Values.Length - 1)].GetValues();
										for (int i = 0; i < prev.Length; ++i)
											if (C(OutputY.ValueToScreen(prev[i].GetLeaf()), OutputY.ValueToScreen(next[i].GetLeaf())))
												c = o.ProcessColor(prev[i], c, z, t, x, y, Frame);
									}
								else foreach (var o in OutputR) if (!(o.Eval?.Null() ?? true)) foreach (var prevV in o.Values[intPtr].GetValues())
									if ((y < OutputY.ValueToScreen(T.Zero())) == (OutputY.ValueToScreen(prevV.GetLeaf()) < y))
										c = o.ProcessColor(prevV, c, z, t, x, y, Frame);
								(p[2], p[1], p[0]) = GetRgb(c);
							}
							continue;
							bool C(int v, int n) => y < v != y <= n || y <= v != y < n;
						}
						break;
					default:
						foreach (var o in OutputR)
							o.PrepareArgs(InputX.length, InputY.length, InputT.length);
						for (var y = 0; y < _bmp.Height; ++y) {
							yz = InputY.Sample(y);
							p = ptr + lb.Stride * y;
							var yColor = _linesY[y];
							for (var x = 0; x < _bmp.Width; ++x, ++intPtr, p += 3) {
								var z = InputX.Sample(x) + yz;
								var rgbc = Max(_linesX[x], yColor);
								(double r, double g, double b) c = (rgbc.R / 255.0, rgbc.G / 255.0, rgbc.B / 255.0);
								foreach (var o in OutputR) if (!(o.Eval?.Null() ?? true))
									foreach (var prevV in o.Values[intPtr].GetValues())
										c = o.ProcessColor(prevV, c, z, t, x, y, Frame);
								(p[2], p[1], p[0]) = GetRgb(c);
							}
						}
						break;
				}
				(byte, byte, byte) GetRgb((double r, double g, double b) c) => ((byte)Math.Clamp(c.r * 255, 0, 255), (byte)Math.Clamp(c.g * 255, 0, 255),(byte)Math.Clamp(c.b * 255, 0, 255));
				_bmp.UnlockBits(lb);
			}
			return _bmp;
			Color Max(Color a, Color b) => Color.FromArgb(Math.Max(a.R, b.R), Math.Max(a.G, b.G), Math.Max(a.B, b.B));
			(double r, double g, double b) MaxD((double r, double g, double b) a, (double r, double g, double b) b)
				=> (Math.Max(a.r, b.r), Math.Max(a.g, b.g), Math.Max(a.b, b.b));
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