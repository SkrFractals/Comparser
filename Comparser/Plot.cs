using Comparser.Comparser.Numbers;
using Comparser.Forms;
using System.Drawing.Imaging;
using static Comparser.Comparser.Numbers.ILeaf;
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
	public bool Update(out bool memory, int w, int h, int l, bool noPreview, ref CancellationTokenSource cancel);
	public IPlotAxis[] GetAxis();
	public void DelOutput(int output);
	public void DelAll();
	public void AddOutput(string name, object? newRgb, object? newCode);
	public void SetCode(int output, object? code);
	public void SetRgb(int output, object? rgb);
	public void SetClip(int output, int clip);
	public object[] GetOutputs();
	public void LockRangeX(bool l);
	public void LockRangeY(bool l);
	public void LockRangeO(bool l);
	public void LockRangeT(bool l);
	public void SetLockRes(bool l);
	public int GetPercent();
	public bool InPlace((object? x, object? y) a);
	public (Point p, Size s) GetPlace((object? x, object? y) a);
	public void SetFinished(Action<object?, object?, Comparser.Plot.BitmapReady, int, CancellationToken/*, string*/> finishedImage);
	public void SoftCancel(CancellationTokenSource cancel);
	public object? GetExpressionArgs(int index);
}
public /*abstract*/  partial class Comparser/*<T>*/{
	public partial class Plot : IPlot {
		public bool InPlace((object? x, object? y) a) {
			if (a.x == null || a.y == null)
				return true;
			PlotAxis ax = (PlotAxis)a.x, ay = (PlotAxis)a.y;
			var d = GetPlace(a);
			var y = Mode switch {
				PlotMode.Xy => InputY,
				_ => OutputY
			};
			
			return d.p is { X: 0, Y: 0 } && d.s.Width == ax.length && d.s.Height == ay.length && InputX.length == ax.length && y.length == ay.length;
		}
		public (Point p, Size s) GetPlace((object? x, object? y) a) {
			var y = Mode switch {
				PlotMode.Xy => InputY,
				_ => OutputY
			};
			PlotAxis? ax = (PlotAxis?)a.x, ay = (PlotAxis?)a.y;
			if (ax == null || ay == null) return (new(0, 0), new(0, 0));
			Point p = new(ValueToScreenLin(ax.start, InputX.start, InputX.d), ValueToScreenLin(ay.start, y.start, y.d));
			Point e = new(ValueToScreenLin(ax.end, InputX.start, InputX.d), ValueToScreenLin(ay.end, y.start, y.d));
			return (p, new(e.X - p.X, e.Y - p.Y));
		}

		public object GetExpressionArgs(int index) => new Value([
				new(new Real(index), FailReason.Success, "x"),
				new(InputX.start, FailReason.Success, "xstart"),
				new(InputX.center, FailReason.Success, "xcenter"),
				new(InputX.end, FailReason.Success, "xend"),
				new(InputY.start, FailReason.Success, "ystart"),
				new(InputY.center, FailReason.Success, "ycenter"),
				new(InputY.end, FailReason.Success, "yend"),
				new(InputY.Sample(FixedY), FailReason.Success, "yfixed"),
				new(OutputY.start, FailReason.Success, "oystart"),
				new(OutputY.center, FailReason.Success, "oycenter"),
				new(OutputY.end, FailReason.Success, "oyend"),
				new(InputT.start, FailReason.Success, "tstart"),
				new(InputT.center, FailReason.Success, "tcenter"),
				new(InputT.end, FailReason.Success, "tend"),
				new(new Real(index), FailReason.Success, "t"),	
				new(new Real(Frame), FailReason.Success, "f"),	
				new(new Real(InputT.length), FailReason.Success, "l"),	
				new(new Real(InputX.length), FailReason.Success, "w"),
				new(new Real(Mode == PlotMode.Xy ? InputY.length : OutputY.length), FailReason.Success, "h"),
			]);
		public void LockRangeX(bool l) => LockedRangeX = l;
		public void LockRangeY(bool l) => LockedRangeY = l;
		public void LockRangeO(bool l) => LockedRangeO = l;
		public void LockRangeT(bool l) => LockedRangeT = l;
		public void SetLockRes(bool l) => LockedRes = l;
		//public void SetDirty() => _dirtyX = _dirtyXy = _dirtyRgb = true;

		public class Values {
			//public PlotAxis? X;
			public (Value[] div, int remainingTasks)[] V = [];
			//public volatile int[] remainingTasks;
			public (int targetDivStart, int targetDivEnd, int taskIndex, int taskCount)[] TaskData = [];
		}

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
			public int Clip;
			public readonly string Name = name;
			//public ColorMode Mode = mode;
			public Expression? ColorCodeRgb = newRgb; // default=rgb(value)
			//public Expression? ColorCodeHsv = newHsv; // default=hsv(value)
			//public PlotAxis A = newA; // axis
			public PlotEval? Eval = newEval; // evaluator (code, default=z)
			public readonly Values Values = new(); // evaluated value buffer
										//public ChannelProperties R = new(), G = new(), B = new(), H = new(), S = new(), V = new();
			private Value[] _args = [];
			public void PrepareArgs(int w, int h, int l,int tasks) {
				if (_args.Length < tasks)
					_args = new Value[tasks];
				for(int t = 0; t< tasks; ++t)
					_args[t] = new([
						new(nan, FailReason.Success, "v"),
						new(nan, FailReason.Success, "c"),
						new(nan, FailReason.Success, "z"),
						new(nan, FailReason.Success, "t"),
						new(nan, FailReason.Success, "x"),
						new(nan, FailReason.Success, "y"),
						new(nan, FailReason.Success, "f"),
						new((Real)w, FailReason.Success, "w"),
						new((Real)h, FailReason.Success, "h"),
						new((Real)l, FailReason.Success, "l")
					]);
			}
			public (double, double, double) ProcessColor(Value? value, (double r, double g, double b) c, ILeaf z, ILeaf t, double x, double y, double f, int taskIndex) {
				var args = _args[taskIndex];
				args.Values[0].Values = [value ?? new()];
				args.Values[1].Values = [new((Real)c.r), new((Real)c.g), new((Real)c.b)];
				args.Values[2].Leaf = z;
				args.Values[3].Leaf = t;
				args.Values[4].Leaf = (Real)x;
				args.Values[5].Leaf = (Real)y;
				args.Values[6].Leaf = (Real)f;
				var rgb = ColorCodeRgb?.Eval(0, args, SettingsPanel.DrawTasks <= 1);
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
				double Get(int i) => Clip switch { 0 => Math.Clamp(rgb.Values[i].Re(), 0, 1), 1 => Loop(rgb.Values[i].Re()), _ => rgb.Values[i].Re() };
				double Loop(double i) => Static.Cycle(i);
				//byte Get(int i) => (byte)Math.Clamp(255 * T.Re(rgb.Values[i].GetLeaf()), 0, 255);
			}
		}


		public void DelOutput(int output) => OutputR.RemoveAt(output);
		public void DelAll() => OutputR.Clear();
		public void AddOutput(string name, object? newRgb, object? newCode) => OutputR.Add(new(name, new(newCode), newRgb as Expression));
		public void SetCode(int output, object? code) => OutputR[output].Eval = new(code);
		public void SetRgb(int output, object? rgb) => OutputR[output].ColorCodeRgb = rgb as Expression;
		public void SetClip(int output, int clip) => OutputR[output].Clip = clip;
		public object[] GetOutputs() {
			object[] r = new object[OutputR.Count];
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
		public bool LockedRangeX = true, LockedRangeY = true, LockedRangeO = true, LockedRangeT = true, LockedRes;
		public readonly Comparser/*<T>*/ Context;
		//private Bitmap _bmp = new(1,1);
		//private Color[] _linesX = [], _linesY = [], _linesO = [], _linesT = [];
		//private bool _dirtyX = true, _dirtyXy = true, _dirtyRgb = true;
		private readonly IPlotAxis[] _axis;
		public Plot(Comparser/*<T>*/ context) {
			var i = Complex.one - (Complex)1;
			_axis = [InputX = new(context, -10 * unit, 20 * unit, 1),
				InputY = new(context, 10 * i, -20 * i, 1),
				InputT = new(context, zero, unit, 1),
				OutputY = new(context, -10 * unit, 20 * unit, 1)];
			OutputR = [];
			Context = context;
			_renderIx = InputX;
			_renderIy = InputY;
			_renderOy = OutputY;
			//Eval = [new(Context = comparser, "x!")];
			//OutputHsv = new(Context = comparser, "[repeatValue=1; (1)s(x)=sqrabs(x)] (arg(x)+pi)360/tau, 1-exp(-s(x)), sqrt(s(x))%repeatValue /* repeatValue: Value cycle slowness, (1)s(x): caches sqrabs for reuse", _x);
			//Update(width, height, length, new CancellationToken());
		}
		public int GetPercent() {
			int done = 0, total = 0;
			foreach (var o in OutputR) {
				done += o.Eval?.GetPercent() ?? 0;
				++total;
			}
			total = (total + 1) * InputY.length;
			foreach (var p in _percent)
				done += p;
			return total == 0 ? 0 : 100 * done / total;
		}
		private int[] _percent = [];
		public void ChangeMode(PlotMode xy) {
			Mode = xy;
			//_dirtyX = _dirtyXy = true;
			//Update(InputX.length, InputY.length, InputT.length, new CancellationToken());
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
			_dirty |= ResizeDim(LockedRangeX, w/*, ref _linesX*/, InputX);
			_dirty |= ResizeDim(LockedRangeY, h/*, ref _linesY*/, InputY);
			_dirty |= ResizeDim(LockedRangeO, h/*, ref _linesO*/, OutputY);
			_dirty |= ResizeDim(LockedRangeT, l/*, ref _linesT*/, InputT);
			return;

			ILeaf NewBounds(PlotAxis a) => a.Locked switch { 0 => a.start, 2 => a.end, _ => a.center };
			void Adjust(PlotAxis a, ILeaf sce) {
				var locked = a.Locked;
				a.Locked = -1; // do it in left align mode for simplicity
				a.start = locked switch { 0 => sce, 2 => Sub(sce, Mul(a.d, (Real)a.length)), _ => Sub(sce, Mul((Real)(a.length >> 1), a.d))  };
				a.Locked = locked; // and switch back to whatever mode we were in
			}

			bool ResizeDim(bool locked, int size/*, ref Color[] lines*/, /*ref bool dirty,*/ PlotAxis a) {
				var p = a.length;
				if (locked) {
					//if (R(ref lines))
					if(a.length == size)	return false;
					a.length = size;
					a.d = Mul(a.d, (Real)((double)p / size));
				} else {
					var b = NewBounds(a);
					
					if(a.length == size)	//if (R(ref lines))
						return false;
					a.length = size;
					Adjust(a, b);
				}
				return true;
				/*bool R(ref Color[] lines) {
					if (size == lines.Length)
						return true;
					lines = new Color[a.length = size];
					return false;
				}*/
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
			InputY.Shift(dy);
			OutputY.Shift(dy);
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
		public class BitmapReady(Bitmap? bmp = null, int finished = 0) {
			public Bitmap? D = bmp;
			public int F = finished;
		}
		public class Renders {
			private readonly List<(Expression? e, PlotEval? v, int c)> _outs = [];
			private int _length = -1;
			private PlotMode _mode;
			private BitmapReady?[] _bitmaps = []; // [frames]
            //public Bitmap?[] WorkMaps = [];
            private int _w, _h, _previewBitmap, _previews = -1;
			public int RenderDiv;
			//private bool preview = true;
			private Bitmap?[] _pBmp = [];
			//private Bitmap?[] _pwBmp = [];
			private int _currentFrame;
			public void Cancel() {
				RenderDiv = 0;
				_bitmaps[_currentFrame] = new();
			}
			public bool GetBitmap(out bool memory, int drawn, bool animate, out BitmapReady bmp/*, out Bitmap workMap*/, int w, int h, int length, int frame, PlotMode mode, List<PlotOutput> rgbs, bool dirty = false, bool dirtied = false) {
				_currentFrame = frame;
				w = Math.Max(1, w); h = Math.Max(1, h);
				bool match = Match(), sameSize = w == _w && h == _h && length == _length && mode == _mode /* && !dirty*/;
				if (match && sameSize && !dirty) {
					_previewBitmap = Math.Min(drawn, _previews);
					if (_previewBitmap < _previews) {

						//b = _pBmp[_previewBitmap];
						//wb = _pwBmp[_previewBitmap];
						RenderDiv = _previews - _previewBitmap;
						//if (b == null/* || wb == null*/) {
						bmp = new(_pBmp[_previewBitmap] = new(w >> RenderDiv, h >> RenderDiv), 0/*1*/);
						//workMap = _pwBmp[_previewBitmap] = new Bitmap(w >> RenderDiv, h >> RenderDiv);
						return memory = false;// false;
						//}
						//bmp = b;
						//workMap = wb;
						//return false;
					}
					var b = _bitmaps[frame]; // = pBmp[previews-1];
					RenderDiv = 0;
					if (b?.F >= 2) {
						bmp = b;
						return memory = true;
					}
					
					//wb = WorkMaps[frame];//= pwBmp[previews-1];
					if ((b?.D == null || b.D.Width != w || b.D.Height != h/*|| wb == null*/)) {
						bmp = _bitmaps[frame] = new(new(w, h));
						//workMap = WorkMaps[frame] = new(w, h);
						return memory = false;// false;
					}
                    bmp = b;
					//workMap = wb;
					return memory = true; //return false;// true;
				}
				if (length != _length) 
					_bitmaps = new BitmapReady?[length];
				//if(_bitmaps[frame] != null)
				_bitmaps[frame]?.F = 0;
				memory = false;
				_mode = mode; _w = w;_h = h;
				_length = length;
				_previewBitmap = 0;
				_previews = animate ? 0 : Math.Max(0,(int)Math.Log2(Math.Min(w, h)) - 4);
				_pBmp = new Bitmap[_previews];
				//_pwBmp = new Bitmap[_previews];
				for (int i = 0; i < _previews; ++i) {
					RenderDiv = _previews - i;
					//_pwBmp[i] = new Bitmap(w >> RenderDiv, h >> RenderDiv);
					_pBmp[i] = new(w >> RenderDiv, h >> RenderDiv);
				}
				//WorkMaps = new Bitmap[length];
				//_bitmaps = new Bitmap[length];
				if (_previews > 0) {
					//workMap = _pwBmp[0]!;
					bmp = new(_pBmp[0]!);
				} else {
					//workMap = WorkMaps[frame] = new(w,h);
					bmp = _bitmaps[frame] = new(new(w, h));
				}
				RenderDiv = _previews;
				return !dirtied && !sameSize;
				bool Match() {
					var match = true;
					for (var i = 0; i < rgbs.Count; ++i) {
						var ri = rgbs[i];
						if (i < _outs.Count) {
							var (e, v, c) = _outs[i];
							if (e == ri.ColorCodeRgb && c == ri.Clip && v == ri.Eval)
								continue;
							_outs[i] = (ri.ColorCodeRgb, ri.Eval, ri.Clip);
						} else _outs.Add((ri.ColorCodeRgb, ri.Eval, ri.Clip));
						match = false;
					}
					if (rgbs.Count >= _outs.Count)
						return match;
					match = false;
					_outs.RemoveRange(rgbs.Count, _outs.Count - rgbs.Count);
					return match;
				}
			}
		}
		public void SoftCancel(CancellationTokenSource cancel) {
			if (_drawn > 0)
				cancel.Cancel();
			else
				_softCancel = cancel;
		}
		private Task[] _taskArr = [];
		private bool _dirty;
		private CancellationTokenSource? _softCancel;
		private readonly Renders _r = new();
		//private (Point p, Size s) _renderingAt;
		private int _drawn; // how many preview frames have already been drawn?
		//private int _done; // how many preview frames are ready to draw?
		private PlotAxis? _renderIx, _renderIy, _renderOy;
		private Task? _drawTask;
		
		/*private sealed class RenderRequest
		{
			public readonly long Id;
			public readonly PlotAxis X;
			public readonly PlotAxis Y;
			public readonly PlotAxis OutputY;
			public readonly PlotAxis T;
			//public readonly int Frame;
			//public readonly PlotMode Mode;
			public readonly CancellationToken Token;

			public RenderRequest(PlotAxis x, PlotAxis iy, PlotAxis oy, PlotAxis t
			//, int frame, int mode
			, CancellationToken token)
			{
				X = new(x);
				Y = new(iy);
				OutputY = new(oy);
				T = new(t);
				//Frame = frame;
				//Mode = mode;
				Token = token;
			}
		}*/
		
		public bool Update(out bool memory, int w, int h, int l, bool noPreview, ref CancellationTokenSource cancel) {
			memory = false;
			bool DoSoftCancel(ref CancellationTokenSource cancel) {
				if (_softCancel == cancel) {
					if (_drawn > 0) { 
						cancel.Cancel();
						_dirty = true;
						//FinishedImage(null, null, null, 0, cancel.Token);
						_softCancel = null;
						return true;
					}
				} else _softCancel = null;
				return false;
			}
			if(_drawTask is { IsCompleted: false }) {//if (Static.TaskRunning(_taskArr)) {
				if(DoSoftCancel(ref cancel))
					return false;
				//Console.WriteLine("working");
				return !cancel.IsCancellationRequested;
				
			}
			var newCancel = new CancellationTokenSource();
			var newCancelToken = newCancel.Token;

			bool dirtied = false; 
			// if size changed, it will resize everything and mark things dirty
			Resize(w, h, l); _dirty |= InputX.DirtyL; 
			// prepare axis lines and plot values if they are dirty
			Color[] linesY, linesX = InputX.lines;
			// = InputX.DirtyL ? Lines(InputX) : InputX.lines;
			int tasks = SettingsPanel.DrawTasks, chunks = tasks <= 1 ? 1 : SettingsPanel.DrawChunks;
			if (_percent.Length != tasks) _percent = new int[tasks];
			else
				for (int task = 0; task < tasks; ++task)
					_percent[task] = 0;
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
					//if(OutputY.DirtyL)
					linesY = OutputY.lines; //	Lines(OutputY, _linesO);
					foreach (var o in OutputR) {
						if (o.Eval?.Null() ?? true) continue;
						
						// Refresh 1D (X,FixedY) output values
						o.Eval.GetPlotX(noPreview, o.Values, out var d, InputX, InputY, FixedY, new(InputT), Frame);
						Dirtied(d, o);
					}
					break;
				default: // XY mode:
					/*dirty = InputY.DirtyL || _dirtyXy;
					_dirtyXy = false;
					if (dirty) {
						Lines(InputX, _linesX); // X input axis lines
						Lines(InputY, _linesY); // Y input axis lines
					}*/
					//if (InputY.DirtyL)
					//	Lines(InputY, _linesY);
					linesY = InputY.lines;
					foreach (var o in OutputR) {
						if (o.Eval?.Null() ?? true) continue;
						o.Eval.Cancel = newCancelToken;
						// Refresh 2D (X,Y) output values
						o.Eval.GetPlotXy(noPreview, o.Values, out var d, InputX, InputY, new(InputT), Frame);
						Dirtied(d, o);
					}
					break;
					void Dirtied(bool d, PlotOutput o) {
						if (!d)
							return;
						dirtied = true;
						//Console.WriteLine("Dirtied");
						o.Eval?.Cancel = newCancelToken;
						//_renderIt = InputT;
						_dirty = true;
						_drawn = 0;//_done = 0;
					}
			}
			void Lines(PlotAxis a, Color[] axis) {
				//if (axis.Length != a.length)
				//	axis = new Color[a.length];//a.length = axis.Length;
				//throw new("given the axis a different length of colors to draw axes to, than the last length it was set to.");
				for (var i = 0; i < axis.Length; ++i) // combine axis lines
					axis[i] = Max(axis[i], a.lines[i]);
			}
			
			var dirt = _dirty;
			if (dirtied) {
				//Console.WriteLine("Dirtied: "+InputX.length);
				cancel = newCancel;
				_softCancel = null;
				//if (OutputR.Count > 0 && OutputR[0].Values.X.start != InputX.start) {_renderIx = new(InputX);}
				_renderIx = new(InputX);//new(InputX); // these new should not be necessary - or maybe yes to prevent crashes from changing WH while generating
				_renderIy = new(InputY);//new(InputY);
				_renderOy = new(OutputY); //new(OutputY);
			}
			if (DoSoftCancel(ref cancel))
				return false;
			//DoSoftCancel(ref cancel);
			_dirty = false;
			if (_r.GetBitmap(out memory, _drawn, noPreview, out var bmp, w, h, l, Frame, Mode, OutputR, dirt, dirtied)){
				//Console.WriteLine("DirtiedBmp");
				if (!memory) {
					cancel.Cancel();
					return false;
				}
			}
			//	) { // nothing has changed, no need to redraw the screen
				//Console.WriteLine("Returned");
			//	return false;
			//}
			bool notReady = false;
			foreach (var o in OutputR)
				if (o.Values.V.Length <= _r.RenderDiv || Volatile.Read(ref o.Values.V[_r.RenderDiv].remainingTasks) > 0)
					notReady = true;
			if (notReady || OutputR.Count == 0) {
				//if(Static.notinplace)Console.WriteLine("notReady");
				return false;
			}
			if (memory && bmp.F >= 1) {
				FinishedImage?.Invoke(
					_renderIx != null ? new PlotAxis(_renderIx) : null,
					new PlotAxis(Mode == PlotMode.Xy ? InputY : OutputY),
					bmp, 0, CancellationToken.None/*, "MEM"*/);
				return false;
			}
			/*++_done;
		foreach (var o in OutputR)
			if (o.Values.V[done].remainingTasks <= 0)
				_done = o.Values.Done;
		if (_drawn >= _done)
			return prevBmp;*/
			//Console.WriteLine("StartDraw");
			unsafe {
				//Console.WriteLine("StartDraw: W"+bmp.d.Width + " T"+Static.Time.ElapsedMilliseconds);
				//var nbmp = new Bitmap(bw), bh);
				var renderBitmap = bmp;
				int bw = renderBitmap.D!.Width, bh = renderBitmap.D!.Height;
				if (0 == linesX.Length || 0 == linesY.Length)
					return false;
				var lb = renderBitmap.D.LockBits(new(0, 0, bw, bh), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
				var ptr = (byte*)(void*)lb.Scan0;
				var t = InputT.Sample(Frame);
				
				/*PlotAxis? renderIx = _renderIx,
					renderIy = _renderIy,
					renderOy = _renderOy;*/
				//var rDiv = _r.RenderDiv;
				//var newCancelTokenR = newCancelToken;
				var renderToken = cancel.Token;
				switch (Mode) {
					case PlotMode.XContour:
					case PlotMode.XFill:
						var yz = _renderOy!.Sample(FixedY);
						SplitTasks(MultiX, _renderOy);
						break;
						void MultiX(float yf, float subChunkLength, int taskIndex) => Static.Multi(yf,subChunkLength, taskIndex, TaskDrawX, tasks, chunks);
						void TaskDrawX(int ys, int ye, int taskIndex) {
							for (var y = ys; y < ye; ++y) {
								if (renderToken.IsCancellationRequested)
									break;
								
								var p = ptr + lb.Stride * y;
								var yColor = linesY[y];
								for (int x = 0, intPtr = 0; x < bw; ++x, ++intPtr, p += 3) {
									if (renderToken.IsCancellationRequested)
										break;
									var z = Add(_renderIx!.Sample(x), yz);
									var rgbc = Max(linesX[x], yColor);
									(double r, double g, double b) c = (rgbc.R / 255.0, rgbc.G / 255.0, rgbc.B / 255.0);
									Value[] ov;
									//T v;
									if (Mode == PlotMode.XContour)
										foreach (var o in OutputR) {
											if (o.Eval?.Null() ?? true) continue;
											Value[] prev = o.Values.V[_r.RenderDiv].div[x].GetValues(), next = o.Values.V[_r.RenderDiv].div[Math.Min(x + 1, o.Values.V[_r.RenderDiv].div.Length - 1)].GetValues();
											for (int i = 0; i < prev.Length; ++i)
												if (C(_renderOy!.ValueToScreen(prev[i].GetLeaf()), _renderOy!.ValueToScreen(next[i].GetLeaf())))
													c = o.ProcessColor(prev[i], c, z, t, x, y, Frame, taskIndex);
										}
									else
										foreach (var o in OutputR)
											if (!(o.Eval?.Null() ?? true) && (ov = o.Values.V[_r.RenderDiv].div).Length > intPtr)
												foreach (var prevV in ov[intPtr].GetValues())
													if (y < _renderOy!.ValueToScreen(zero) == _renderOy!.ValueToScreen(prevV.GetLeaf()) < y)
														c = o.ProcessColor(prevV, c, z, t, x, y, Frame, taskIndex);
									(p[2], p[1], p[0]) = GetRgb(c);
								}
								++_percent[taskIndex];
								continue;
								bool C(int v, int n) => y < v != y <= n || y <= v != y < n;
							}
						}

					default:
						SplitTasks(MultiXy, _renderIy);
						break;
						void MultiXy(float yf, float subChunkLength, int taskIndex) => Static.Multi(yf, subChunkLength, taskIndex, TaskDrawXy, tasks, chunks);
						void TaskDrawXy(int ys, int ye, int taskIndex) {
							var intPtr = ys * bw;
							for (var y = ys; y < ye; ++y) {
								
								if (renderToken.IsCancellationRequested)
									break;
								var yz2 = _renderIy!.Sample(y);
								byte* p = ptr + lb.Stride * y;
								var yColor = linesY[y];
								
								for (var x = 0; x < bw; ++x, ++intPtr, p += 3) {
									if (renderToken.IsCancellationRequested)
										break;
									var z = Add(_renderIx!.Sample(x), yz2);
									var rgbc = Max(linesX[x], yColor);
									(double r, double g, double b) c = (rgbc.R / 255.0, rgbc.G / 255.0, rgbc.B / 255.0);
									foreach (var o in OutputR)
										if (!(o.Eval?.Null() ?? true)) {
											var ov = o.Values.V[_r.RenderDiv].div;
											if (ov.Length <= intPtr) {
												Console.WriteLine("Error: Values out of bounds!");
												return;
											}
											c = o.ProcessColor(ov[intPtr], c, z, t, x, y, Frame, taskIndex);
										}
									(p[2], p[1], p[0]) = GetRgb(c);
								}
								++_percent[taskIndex];
							}
						}
				}
				
                //(_r.Bitmaps[Frame], _r.WorkMaps[Frame]) = (_r.WorkMaps[Frame], _r.Bitmaps[Frame]);
                
				void SplitTasks(Action<float,float,int> del, PlotAxis? rY) {
					//Console.WriteLine("SplitTasks");
					foreach (var o in OutputR)
						o.PrepareArgs(bw, bh, InputT.length, tasks);
					_drawTask = Task.Run(() => DrawTask(del, rY)/*, cancel*/);
				}
				void DrawTask(Action<float,float,int> del, PlotAxis? rY) {
					Static.TaskManager(ref _taskArr, tasks, chunks, bh, renderToken, del);
					
					renderBitmap.D.UnlockBits(lb);
					var rbmp = renderBitmap;
					if (renderToken.IsCancellationRequested) {
						rbmp.D = null;
						_dirty = true;
						_drawn = 0;
						_r.Cancel();
						//Console.WriteLine("FinishCancel " + Static.Time.ElapsedMilliseconds);
					} else {
						renderBitmap.F = 1; // mark is as finished, so that it is safe to just pick it from the memory and return it without drawing it again
						//Console.WriteLine("Finish W" + renderBitmap.d.Width + " T" + Static.Time.ElapsedMilliseconds);
						++_drawn;
						//if (OutputR[0].Values.X?.start != _renderIx?.start)throw new("inconsistent axis!");
					}
					FinishedImage?.Invoke(_renderIx != null ? new PlotAxis(_renderIx) : null, rY != null ? new PlotAxis(rY) : null, rbmp, _r.RenderDiv, renderToken/*, "FIN"*/);
				}
				(byte, byte, byte) GetRgb((double r, double g, double b) c) => ((byte)Math.Clamp(c.r * 255, 0, 255), (byte)Math.Clamp(c.g * 255, 0, 255),(byte)Math.Clamp(c.b * 255, 0, 255));
            }
			return true;
				
			Color Max(Color a, Color b) => Color.FromArgb(Math.Max(a.R, b.R), Math.Max(a.G, b.G), Math.Max(a.B, b.B));
		}
		public static int ValueToScreenLin(ILeaf value, ILeaf start, ILeaf d) => (int)Math.Round(Div(Sub(value, start), d).Re());//length * ILeaf.D2(value - start, end - start, Static.Div);
		public static ILeaf ScreenToValueLin(int x, ILeaf start, ILeaf d) => Add(start, Mul((Real)x, d));//INumber<ILeaf>.Lerp(start, end, new((double)x / length));
		public static int ValueToScreenLog(ILeaf value, ILeaf start, ILeaf d) => ValueToScreenLin(D1(value, Math.Log), start, d);//length * ((ILeaf.D1(value, Math.Log) - start) / (end - start));
		public static ILeaf ScreenToValueLog(int x, ILeaf start, ILeaf d) => D1(ScreenToValueLin(x, start, d),Math.Exp); //ILeaf.D1(ScreenToValueLin(length, x, start, end), Math.Exp);

		public void SetFinished( Action<object?, object?, BitmapReady, int, CancellationToken/*, string*/> finishedImage) => FinishedImage = finishedImage;
		public Action<object?, object?, BitmapReady, int, CancellationToken/*, string*/>? FinishedImage;
	}
}