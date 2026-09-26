using Comparser.Comparser.Numbers;
using Comparser.Forms;
using System.Drawing.Imaging;
using System.Numerics;
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
	public (int, int) GetPercent();
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
			var (p, s) = GetPlace(a);
			var y = Mode switch {
				PlotMode.Xy => InputY,
				_ => OutputY
			};
			
			return p is { X: 0, Y: 0 } && s.Width == ax.length / SettingsPanel.SuperSampling && s.Height == ay.length / SettingsPanel.SuperSampling && InputX.length == ax.length && y.length == ay.length;
		}
		public (Point p, Size s) GetPlace((object? x, object? y) a) {
			var y = Mode switch {
				PlotMode.Xy => InputY,
				_ => OutputY
			};
			PlotAxis? ax = (PlotAxis?)a.x, ay = (PlotAxis?)a.y;
			if (ax == null || ay == null) return (new(0, 0), new(0, 0));
			Point p = new(ValueToScreenLin(ax.start, InputX.start, InputX.d)/ SettingsPanel.SuperSampling, ValueToScreenLin(ay.start, y.start, y.d)/ SettingsPanel.SuperSampling);
			Point e = new(ValueToScreenLin(ax.end, InputX.start, InputX.d)/ SettingsPanel.SuperSampling, ValueToScreenLin(ay.end, y.start, y.d)/ SettingsPanel.SuperSampling);
			return (p, new(e.X - p.X, e.Y - p.Y));
		}

		public object GetExpressionArgs(int index) => new Value([
				new(new Real(index), FailReason.Success, "x"),
				new(InputX.start, FailReason.Success, "xs"),
				new(InputX.center, FailReason.Success, "xc"),
				new(InputX.end, FailReason.Success, "xe"),
				new(InputY.start, FailReason.Success, "ys"),
				new(InputY.center, FailReason.Success, "yc"),
				new(InputY.end, FailReason.Success, "ye"),
				new(InputY.Sample(FixedY), FailReason.Success, "yf"),
				new(OutputY.start, FailReason.Success, "oys"),
				new(OutputY.center, FailReason.Success, "oyc"),
				new(OutputY.end, FailReason.Success, "oye"),
				new(InputT.start, FailReason.Success, "ts"),
				new(InputT.center, FailReason.Success, "tc"),
				new(InputT.end, FailReason.Success, "te"),
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

		public class Values {
			public (Value[] div, int remainingTasks)[] V = [];
			public (int targetDivStart, int targetDivEnd, int taskIndex, int taskCount)[] TaskData = [];
		}

		public class PlotOutput(string name, PlotEval? newEval, Expression? newRgb) {
			public int Clip;
			public readonly string Name = name;
			public Expression? ColorCodeRgb = newRgb; // default=rgb(value)
			public PlotEval? Eval = newEval; // evaluator (code, default=z)
			public readonly Values Values = new(); // evaluated value buffer
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
			public Vector3 ProcessColor(Value? value, Vector3 c, ILeaf z, ILeaf t, double x, double y, double f, int taskIndex) {
				var args = _args[taskIndex];
				args.Values[0].Values = [value ?? new()];
				args.Values[1].Values = [new((Real)c.X), new((Real)c.Y), new((Real)c.Z)];
				args.Values[2].Leaf = z;
				args.Values[3].Leaf = t;
				args.Values[4].Leaf = (Real)x;
				args.Values[5].Leaf = (Real)y;
				args.Values[6].Leaf = (Real)f;
				var rgb = ColorCodeRgb?.Eval(0, args, SettingsPanel.DrawTasks <= 1);
				if (rgb is null)
					return Vector3.Zero;
				if (rgb.Values.Length >= 3) {
					return new Vector3((float)Get(0), (float)Get(1), (float)Get(2));
				}
				rgb = UnCollapseScalar(rgb);
				if (rgb.Values.Length != 1)
					return Vector3.Zero;
				var light = (float)Get(0);
				return new Vector3(light);
				double Get(int i) => Clip switch { 0 => Math.Clamp(rgb.Values[i].Re(), 0, 1), 1 => Loop(rgb.Values[i].Re()), _ => rgb.Values[i].Re() };
				double Loop(double i) => Static.Cycle(i);
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
		public readonly List<PlotOutput> OutputR;
		public PlotMode Mode = PlotMode.XContour;
		public double FixedY;
		public int Frame;
		public int SelectedOutput = -1;
		public bool LockedRangeX = true, LockedRangeY = true, LockedRangeO = true, LockedRangeT = true, LockedRes;
		public readonly Comparser/*<T>*/ Context;
		private readonly IPlotAxis[] _axis;
		public Plot(Comparser/*<T>*/ context) {
			var i = Numbers.Complex.one - (Numbers.Complex)1;
			_axis = [InputX = new(context, -10 * unit, 20 * unit, 1),
				InputY = new(context, 10 * i, -20 * i, 1),
				InputT = new(context, zero, unit, 1),
				OutputY = new(context, -10 * unit, 20 * unit, 1)];
			OutputR = [];
			Context = context;
			_renderIx = InputX;
			_renderIy = InputY;
			_renderOy = OutputY;
		}
		private int _totalD;
		public (int, int) GetPercent() {
			int done = 0, total = 0, doneD = 0;
			foreach (var o in OutputR) {
				done += o.Eval?.GetPercent() ?? 0;
				total += o.Eval?.GetTotalPercent() ?? 0;
			}
			
			foreach (var p in _percent)
				doneD += p;
			return (P(done,total), P(doneD, _totalD));
			static int P(int d, int t) => t == 0 ? 0 : 100 * d / t;
		}
		private int[] _percent = [];
		public void ChangeMode(PlotMode xy) => Mode = xy;
		public (double, string) SetFixedY(object? yf) {
			FixedY = Context.AsDouble(yf);
			if (double.IsNaN(FixedY)) FixedY = 0;
			return (FixedY, InputY.Sample(FixedY).ToString(3));
		}
		public void SetFrame(object? t) {
			Frame = (int)Context.AsDouble(t);
		}
		public void Resize(int w, int h, int l) { // on screen panel resize
			if (LockedRes)
				return;
			_dirty |= ResizeDim(LockedRangeX, w, InputX);
			_dirty |= ResizeDim(LockedRangeY, h, InputY);
			_dirty |= ResizeDim(LockedRangeO, h, OutputY);
			_dirty |= ResizeDim(LockedRangeT, l, InputT);
			return;

			ILeaf NewBounds(PlotAxis a) => a.Locked switch { 0 => a.start, 2 => a.end, _ => a.center };
			void Adjust(PlotAxis a, ILeaf sce) {
				var locked = a.Locked;
				a.Locked = -1; // do it in left align mode for simplicity
				a.start = locked switch { 0 => sce, 2 => Sub(sce, Mul(a.d, (Real)a.length)), _ => Sub(sce, Mul((Real)(a.length >> 1), a.d))  };
				a.Locked = locked; // and switch back to whatever mode we were in
			}

			bool ResizeDim(bool locked, int size, PlotAxis a) {
				var p = a.length;
				if (locked) {
					if(a.length == size)	return false;
					a.length = size;
					a.d = Mul(a.d, (Real)((double)p / size));
				} else {
					var b = NewBounds(a);
					if(a.length == size)
						return false;
					a.length = size;
					Adjust(a, b);
				}
				return true;
			}
		}
		public void ZoomContinuous(int x, int y, double size) {
			InputX.Zoom(x, size);
			InputY.Zoom(y, size);
			OutputY.Zoom(y, size);
		}
		public void Shift(int dx, int dy) {
			if (dx != 0)
				InputX.Shift(dx);
			if (dy == 0)
				return;
			InputY.Shift(dy);
			OutputY.Shift(dy);
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
            private int _w, _h, _previewBitmap, _previews = -1;
            private PlotAxis? _myOy;
			public int RenderDiv;
			private Bitmap?[] _pBmp = [];
			private int _currentFrame;
			public void Cancel() {
				RenderDiv = 0;
				_bitmaps[_currentFrame] = new();
			}
			public bool GetBitmap(out bool memory, int drawn, int maxDiv, out BitmapReady bmp, int w, int h, PlotAxis oy, int length, int frame, PlotMode mode, List<PlotOutput> rgbs, bool dirty = false, bool dirtied = false) {
				_currentFrame = frame;
				w = Math.Max(1, w); h = Math.Max(1, h);
				bool match = Match(), sameSize = w == _w && h == _h && length == _length && mode == PlotMode.Xy == (_mode == PlotMode.Xy);
				if (match && sameSize && !dirty && mode == _mode) {
					_previewBitmap = Math.Min(drawn, _previews);
					if (_previewBitmap < _previews) {

						RenderDiv = _previews - _previewBitmap;
						bmp = new(_pBmp[_previewBitmap] = new(w >> RenderDiv, h >> RenderDiv));
						return memory = false;
					}
					var b = _bitmaps[frame];
					RenderDiv = 0;
					if (b?.F >= 2) {
						bmp = b;
						return memory = true;
					}
					if (b?.D == null || b.D.Width != w || b.D.Height != h) {
						bmp = _bitmaps[frame] = new(new(w, h));
						return memory = false;
					}
                    bmp = b;
					return memory = false;//true; 
				}
				if (length != _length) 
					_bitmaps = new BitmapReady?[length];
				_bitmaps[frame]?.F = 0;
				memory = false;
				_mode = mode; _w = w;_h = h;
				_length = length;
				_previewBitmap = 0;
				_previews = Math.Min(maxDiv, Math.Max(0,(int)Math.Log2(Math.Min(w, h)) - 4));
				_pBmp = new Bitmap[_previews];
				for (var i = 0; i < _previews; ++i)
					_pBmp[i] = new(w >> (RenderDiv = _previews - i), h >> RenderDiv);
				bmp = _previews > 0 ? new(_pBmp[0]!) : _bitmaps[frame] = new(new(w, h));
				RenderDiv = _previews;
				return !dirtied && !sameSize;
				bool Match() {
					var localMatch = true;
					for (var i = 0; i < rgbs.Count; ++i) {
						var ri = rgbs[i];
						if (i < _outs.Count) {
							var (e, v, c) = _outs[i];
							if (e == ri.ColorCodeRgb && c == ri.Clip && v == ri.Eval)
								continue;
							_outs[i] = (ri.ColorCodeRgb, ri.Eval, ri.Clip);
						} else _outs.Add((ri.ColorCodeRgb, ri.Eval, ri.Clip));
						localMatch = false;
					}
					if (mode != PlotMode.Xy && (_myOy == null || LeafNotEquals(_myOy.start, oy.start) || LeafNotEquals(_myOy.d, oy.d))) {
						localMatch = false;
						_myOy = new(oy);
					}
					if (rgbs.Count >= _outs.Count)
						return localMatch;
					localMatch = false;
					_outs.RemoveRange(rgbs.Count, _outs.Count - rgbs.Count);
					return localMatch;
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
		private int _drawn; // how many preview frames have already been drawn?
		private PlotAxis? _renderIx, _renderIy, _renderOy;
		private Task? _drawTask;
		
		public bool Update(out bool memory, int w, int h, int l, bool noPreview, ref CancellationTokenSource cancel) {
			memory = false;

			var ss = SettingsPanel.SuperSampling;
			int ws = w * ss, hs = h * ss;

			if (_drawTask is { IsCompleted: false }) {
				if(DoSoftCancel(ref cancel))
					return false;
				//Console.WriteLine("working");
				return !cancel.IsCancellationRequested;
				
			}
			var newCancel = new CancellationTokenSource();
			var newCancelToken = newCancel.Token;

			var dirtied = false; 
			// if size changed, it will resize everything and mark things dirty
			Resize(ws, hs, l); _dirty |= InputX.DirtyL; 
			// prepare axis lines and plot values if they are dirty
			Color[] linesY, linesX = InputX.lines;
			// = InputX.DirtyL ? Lines(InputX) : InputX.lines;
			int tasks = SettingsPanel.DrawTasks, chunks = tasks <= 1 ? 1 : SettingsPanel.DrawChunks;
			switch (Mode) {
				case PlotMode.XContour:
				case PlotMode.XFill:
					linesY = OutputY.lines; 
					foreach (var o in OutputR) {
						if (o.Eval?.Null() ?? true) continue;
						// Refresh 1D (X,FixedY) output values
						o.Eval.GetPlotX(noPreview, o.Values, out var d, InputX, InputY, FixedY, new(InputT), Frame);
						Dirtied(d, o);
					}
					break;
				default: // XY mode:
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
						_dirty = true;
						_drawn = 0;
					}
			}
			
			var dirt = _dirty;
			if (dirtied) {
				//Console.WriteLine("Dirtied: "+InputX.length);
				cancel = newCancel;
				_softCancel = null;
				_renderIx = new(InputX); // these new should not be necessary - or maybe yes to prevent crashes from changing WH while generating
				_renderIy = new(InputY);
				_renderOy = new(OutputY);
			}
			if (DoSoftCancel(ref cancel))
				return false;
			_dirty = false;
			var maxDiv = int.MaxValue;
			foreach (var o in OutputR) 
				maxDiv = Math.Min(maxDiv, o.Values.V.Length - 1);
			
			if (_r.GetBitmap(out memory, _drawn, noPreview ? 0 : maxDiv, out var bmp, w, h, OutputY, l, Frame, Mode, OutputR, dirt, dirtied)){
				//Console.WriteLine("DirtiedBmp");
				if (!memory) {
					// should we re-evaluate GetPlot?
					cancel.Cancel();
					return false;
				}
			} else if(!dirtied) {
				// TODO test if this doesn't break anything
				// if we only changed some setting, that doesn't affect GetPlot values, but only wants to re-render RgbEval, then assign a new cancel
				foreach (var o in OutputR)
					o.Eval?.Cancel = newCancel.Token;
				cancel = newCancel;
			}
			// GetPlot tasks still running?
			bool notReady = false;
			foreach (var o in OutputR) {
				if (Volatile.Read(ref o.Values.V[_r.RenderDiv].remainingTasks) > 0)
					notReady = true;
			}
			if (notReady || OutputR.Count == 0)
				return false;
			// Do we already have the full resolution image pre-rendered? If yes, just return it back without re-rendering
			if (memory && bmp.F >= 1) {
				FinishedImage?.Invoke(
					_renderIx != null ? new PlotAxis(_renderIx) : null,
					new PlotAxis(Mode == PlotMode.Xy ? InputY : OutputY),
					bmp, 0, CancellationToken.None/*, "MEM"*/);
				return false;
			}
			// Prepare Progress percentage reporting:
			_totalD = (Mode == PlotMode.Xy ? InputY.length : OutputY.length) >> _r.RenderDiv;
			if (_percent.Length != tasks) _percent = new int[tasks];
			else
				for (int task = 0; task < tasks; ++task)
					_percent[task] = 0;
			
			unsafe {
				//Console.WriteLine("StartDraw: W"+bmp.d.Width + " T"+Static.Time.ElapsedMilliseconds);
				var renderBitmap = bmp;
				int bw = renderBitmap.D!.Width, bh = renderBitmap.D!.Height;
				if (0 == linesX.Length || 0 == linesY.Length)
					return false;
				var lb = renderBitmap.D.LockBits(new(0, 0, bw, bh), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
				var ptr = (byte*)(void*)lb.Scan0;
				var t = InputT.Sample(Frame);
				var ssss = ss * ss;
				var bwss = bw * ss;
				var bhss = bh * ss;
				var renderToken = cancel.Token;
				switch (Mode) {
				case PlotMode.XContour:
				case PlotMode.XFill:
					var yz = _renderOy!.Sample(FixedY * ss);
					SplitTasks(MultiX, _renderOy);
					break;
					void MultiX(float yf, float subChunkLength, int taskIndex) => Static.Multi(yf, subChunkLength, taskIndex, TaskDrawX, tasks, chunks);
					void TaskDrawX(int ys, int ye, int taskIndex) {
						var p = ptr + ys * lb.Stride;
						ys *= ss;
						ye *= ss;
						Vector3[] buffer = new Vector3[bw];
						
						for (var y = ys; y < ye; ++y) {
							if (renderToken.IsCancellationRequested)
								break;
							var ym = y % ss == 0;
							
							//var yColor = linesY[y];
							var yss = (y + 1) % ss != 0;
							for (int x = 0, intPtr = 0; x < bwss; ++x, ++intPtr) {
								if (renderToken.IsCancellationRequested)
									break;
								var xs = x / ss;
								if (x % ss == 0 && ym) buffer[xs] = Vector3.Zero;
								var z = Add(_renderIx!.Sample(x), yz);
								//var rgbc = Max(linesX[x], yColor);
								//(double r, double g, double b) c = (rgbc.R / 255.0, rgbc.G / 255.0, rgbc.B / 255.0);
								Value[] ov;
								//T v;
								var c = Vector3.Zero;
								if (Mode == PlotMode.XContour)
									foreach (var o in OutputR) {
										if (o.Eval?.Null() ?? true) continue;
										Value[] prev = o.Values.V[_r.RenderDiv].div[x].GetValues(), next = o.Values.V[_r.RenderDiv].div[Math.Min(x + 1, o.Values.V[_r.RenderDiv].div.Length - 1)].GetValues();
										for (var i = 0; i < prev.Length; ++i)
											if (C(_renderOy!.ValueToScreen(prev[i].GetLeaf()), _renderOy!.ValueToScreen(next[i].GetLeaf())))
												c = o.ProcessColor(prev[i], c, z, t, x, y, Frame, taskIndex);
									}
								else
									foreach (var o in OutputR)
										if (!(o.Eval?.Null() ?? true) && (ov = o.Values.V[_r.RenderDiv].div).Length > intPtr)
											foreach (var prevV in ov[intPtr].GetValues())
												if (y < _renderOy!.ValueToScreen(zero) == _renderOy!.ValueToScreen(prevV.GetLeaf()) < y)
													c = o.ProcessColor(prevV, c, z, t, x, y, Frame, taskIndex);
								buffer[xs] += c / ssss;

								if ((x + 1) % ss != 0 || yss)
									continue;
								(p[2], p[1], p[0]) = GetRgb(buffer[xs]);
								p += 3;
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
						var p = ptr + ys * lb.Stride;
						ys *= ss;
						ye *= ss;
						var intPtr = ys * bwss;
						Vector3[] buffer = new Vector3[bw];
						
						for (var y = ys; y < ye; ++y) {
							if (renderToken.IsCancellationRequested)
								break;
							var ym = y % ss == 0;
							var yz2 = _renderIy!.Sample(y);
						
							//var yColor = linesY[y];
							var yss = (y + 1) % ss != 0;
							for (var x = 0; x < bwss; ++x, ++intPtr) {
								if (renderToken.IsCancellationRequested)
									break;
								var xs = x / ss;
								if (x % ss == 0 && ym) 
									buffer[xs] = Vector3.Zero;
								var z = Add(_renderIx!.Sample(x), yz2);
								//var rgbc = Max(linesX[x], yColor);
								//Vector3 c = new(rgbc.R / 255.0f, rgbc.G / 255.0f, rgbc.B / 255.0f);
								var c = Vector3.Zero;
								foreach (var o in OutputR)
									if (!(o.Eval?.Null() ?? true)) {
										var ov = o.Values.V[_r.RenderDiv].div;
										if (ov.Length <= intPtr) {
											Console.WriteLine("Error: Values out of bounds!");
											return;
										}
										c = o.ProcessColor(ov[intPtr], c, z, t, x, y, Frame, taskIndex);
									}
								buffer[xs] += c / ssss;
								if ((x + 1) % ss != 0 || yss)
									continue;
								(p[2], p[1], p[0]) = GetRgb(buffer[xs]);
								p += 3;
							}
							++_percent[taskIndex];
						}
					}
				}

				void SplitTasks(Action<float,float,int> del, PlotAxis? rY) {
					//Console.WriteLine("SplitTasks");
					foreach (var o in OutputR)
						o.PrepareArgs(bwss, bhss, InputT.length, tasks);
					_drawTask = Task.Run(() => DrawTask(del, rY)/*, cancel*/);
				}
				void DrawTask(Action<float,float,int> del, PlotAxis? rY) {
					Static.TaskManager(ref _taskArr, tasks, chunks, bh, renderToken, del);
					
					renderBitmap.D.UnlockBits(lb);
					var rBmp = renderBitmap;
					if (renderToken.IsCancellationRequested) {
						rBmp.D = null;
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
					FinishedImage?.Invoke(
						_renderIx != null ? new PlotAxis(_renderIx) : null, 
						rY != null ? new PlotAxis(rY) : null, 
						rBmp, _r.RenderDiv, renderToken/*, "FIN"*/);
				}
				(byte, byte, byte) GetRgb(Vector3 c) => (
					(byte)Math.Clamp(c.X * 255, 0, 255), 
					(byte)Math.Clamp(c.Y * 255, 0, 255),
					(byte)Math.Clamp(c.Z * 255, 0, 255));
            }
			return true;
			/*void Lines(PlotAxis a, Color[] axis) {
				for (var i = 0; i < axis.Length; ++i) // combine axis lines
					axis[i] = Max(axis[i], a.lines[i]);
			}*/
			bool DoSoftCancel(ref CancellationTokenSource cancel) {
				if (_softCancel == cancel) {
					if (_drawn > 0) { 
						cancel.Cancel();
						_dirty = true;
						_softCancel = null;
						return true;
					}
				} else _softCancel = null;
				return false;
			}
			//Color Max(Color a, Color b) => Color.FromArgb(Math.Max(a.R, b.R), Math.Max(a.G, b.G), Math.Max(a.B, b.B));
		}
		public static int ValueToScreenLin(ILeaf value, ILeaf start, ILeaf d) => (int)Math.Round(Div(Sub(value, start), d).Re());//length * ILeaf.D2(value - start, end - start, Static.Div);
		public static ILeaf ScreenToValueLin(int x, ILeaf start, ILeaf d) => Add(start, Mul((Real)x, d));//INumber<ILeaf>.Lerp(start, end, new((double)x / length));
		public static int ValueToScreenLog(ILeaf value, ILeaf start, ILeaf d) => ValueToScreenLin(D1(value, Math.Log), start, d);//length * ((ILeaf.D1(value, Math.Log) - start) / (end - start));
		public static ILeaf ScreenToValueLog(int x, ILeaf start, ILeaf d) => D1(ScreenToValueLin(x, start, d),Math.Exp); //ILeaf.D1(ScreenToValueLin(length, x, start, end), Math.Exp);

		public void SetFinished( Action<object?, object?, BitmapReady, int, CancellationToken/*, string*/> finishedImage) => FinishedImage = finishedImage;
		public Action<object?, object?, BitmapReady, int, CancellationToken/*, string*/>? FinishedImage;
	}
}