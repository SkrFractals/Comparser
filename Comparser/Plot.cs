using Comparser.Comparser.Numbers;
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
	public Bitmap Update(int w, int h, int l, CancellationToken cancel);
	public IPlotAxis[] GetAxis();
	public void DelOutput(int output);
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
			public int Clip;
			public readonly string Name = name;
			//public ColorMode Mode = mode;
			public Expression? ColorCodeRgb = newRgb; // default=rgb(value)
			//public Expression? ColorCodeHsv = newHsv; // default=hsv(value)
			//public PlotAxis A = newA; // axis
			public PlotEval? Eval = newEval; // evaluator (code, default=z)
			public Value[] Values = []; // evaluated value buffer
										//public ChannelProperties R = new(), G = new(), B = new(), H = new(), S = new(), V = new();
			private Value[] _args = [];
			public void PrepareArgs(int w, int h, int l,int tasks) {
				if (_args.Length < tasks)
					_args = new Value[tasks];
				for(int t = 0; t< tasks; ++t)
					_args[t] = new([
						new(T.nan, FailReason.Success, "v"),
						new(T.nan, FailReason.Success, "c"),
						new(T.nan, FailReason.Success, "z"),
						new(T.nan, FailReason.Success, "t"),
						new(T.nan, FailReason.Success, "x"),
						new(T.nan, FailReason.Success, "y"),
						new(T.nan, FailReason.Success, "f"),
						new(T.MakeR(w), FailReason.Success, "w"),
						new(T.MakeR(h), FailReason.Success, "h"),
						new(T.MakeR(l), FailReason.Success, "l")
					]);
			}
			public (double, double, double) ProcessColor(Value? value, (double r, double g, double b) c, T z, T t, double x, double y, double f, int taskIndex) {
				var args = _args[taskIndex];
				args.Values[0].Values = [value ?? new()];
				args.Values[1].Values = [new(T.MakeR(c.r)), new(T.MakeR(c.g)), new(T.MakeR(c.b))];
				args.Values[2].Leaf = z;
				args.Values[3].Leaf = t;
				args.Values[4].Leaf = T.MakeR(x);
				args.Values[5].Leaf = T.MakeR(y);
				args.Values[6].Leaf = T.MakeR(f);
				var rgb = ColorCodeRgb?.Eval(0, args, taskIndex == 0);
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
				double Get(int i) => Clip switch { 0 => Math.Clamp(T.Re(rgb.Values[i].GetLeaf()), 0, 1), 1 => Loop(T.Re(rgb.Values[i].GetLeaf())), _ => T.Re(rgb.Values[i].GetLeaf()) };
				double Loop(double i) => Static.Cycle(i);
				//byte Get(int i) => (byte)Math.Clamp(255 * T.Re(rgb.Values[i].GetLeaf()), 0, 255);
			}
		}


		public void DelOutput(int output) => OutputR.RemoveAt(output);
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
		public readonly Comparser<T> Context;
		//private Bitmap _bmp = new(1,1);
		private Color[] _linesX = [], _linesY = [], _linesO = [], _linesT = [];
		//private bool _dirtyX = true, _dirtyXy = true, _dirtyRgb = true;
		private readonly IPlotAxis[] _axis;
		public Plot(Comparser<T> context) {
			_axis = [InputX = new(context, T.zero, T.unit, 1),
				InputY = new(context, T.zero, T.one - T.unit, 1),
				InputT = new(context, T.zero, T.unit, 1),
				OutputY = new(context, T.zero, T.unit, 1)];
			OutputR = [];
			Context = context;
			//Eval = [new(Context = comparser, "x!")];
			//OutputHsv = new(Context = comparser, "[repeatValue=1; (1)s(x)=sqrabs(x)] (arg(x)+pi)360/tau, 1-exp(-s(x)), sqrt(s(x))%repeatValue /* repeatValue: Value cycle slowness, (1)s(x): caches sqrabs for reuse", _x);
			//Update(width, height, length, new CancellationToken());
		}
		public int GetPercent() {
			int done = 0, total = 0;
			foreach (var o in OutputR) {
				done += o.Eval.GetPercent();
				++total;
			}
			total = (total + 1) * InputY.length;
			foreach (var p in percent)
				done += p;
			return total == 0 ? 0 : 100 * done / total;
		}
		private int[] percent = [];
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
			_dirty |= ResizeDim(LockedRangeX, w, ref _linesX, InputX);
			_dirty |= ResizeDim(LockedRangeY, h, ref _linesY, InputY);
			_dirty |= ResizeDim(LockedRangeO, h, ref _linesO, OutputY);
			_dirty |= ResizeDim(LockedRangeT, l, ref _linesT, InputT);
			return;

			T NewBounds(PlotAxis a) => a.Locked switch { 0 => a.start, 2 => a.end, _ => a.center };
			void Adjust(PlotAxis a, T sce) {
				var locked = a.Locked;
				a.Locked = -1; // do it in left align mode for simplicity
				a.start = locked switch { 0 => sce, 2 => sce - a.d * a.length, _ => sce - .5 * a.d * a.length };
				a.Locked = locked; // and switch back to whatever mode we were in
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
		public class Renders {
			private readonly List<(Expression? e, PlotEval? v, int c)> _outs = [];
			private int _length = -1;
			private PlotMode _mode;
			public Bitmap?[] Bitmaps = []; // [frames]
            public Bitmap?[] WorkMaps = [];
            private int _w, _h;
			public bool GetBitmap(out Bitmap bmp, out Bitmap workMap, int w, int h, int length, int frame, PlotMode mode, List<PlotOutput> rgbs, bool dirty = false) {
				if (Match() && w == _w && h == _h && length == _length && mode == _mode && !dirty) {
					var b = Bitmaps[frame];
					var wb = WorkMaps[frame];
					if (b == null || wb == null) {
						bmp = Bitmaps[frame] = new(w, h);
						workMap = WorkMaps[frame] = new(w, h);
						return false;
					}
                    bmp = b;
					workMap = wb;
					return true;
				}
				_mode = mode;
                workMap = (WorkMaps = new Bitmap[_length = length])[frame] = new Bitmap(Math.Max(1, _w = w), Math.Max(1, _h = h));
                bmp = (Bitmaps = new Bitmap[_length = length])[frame] = new Bitmap(Math.Max(1, _w = w), Math.Max(1, _h = h));
				return false;
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
		private readonly Task[] _taskArr = [];
		private bool _dirty;
		private readonly Renders _r = new();
		public Bitmap Update(int w, int h, int l, CancellationToken cancel) {
			Resize(w, h, l); _dirty |= InputX.DirtyL; // if size changed, it will resize everything and mark things dirty
								 //var dirty = InputX.DirtyL;
								 // prepare axis lines and plot values if they are dirty
			if (InputX.DirtyL)
				Lines(InputX, _linesX);
			int tasks = SettingsPanel.Tasks, chunks = tasks <= 1 ? 1 : 16;
			if (percent.Length != tasks) percent = new int[tasks];
			else
				for (int task = 0; task < tasks; ++task)
					percent[task] = 0;
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
						o.Eval.Cancel = cancel;
						o.Values = o.Eval.GetPlotX(out var d, InputX, InputY, FixedY, InputT, Frame);
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
						o.Eval.Cancel = cancel;
						o.Values = o.Eval.GetPlotXy(out var d, InputX, InputY, InputT, Frame);
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
			if (_r.GetBitmap(out var bmp, out var work, w, h, l, Frame, Mode, OutputR, _dirty)) // nothing has changed, no need to redraw the screen
				return bmp;
			_dirty = false;
			unsafe {
				//var nbmp = new Bitmap(bw), bh);
				int bw = bmp.Width, bh = bmp.Height;
				if (bw != _linesX.Length || bh != _linesY.Length)
					return bmp;
				var lb = work.LockBits(new(0, 0, bw, bh), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
				var ptr = (byte*)(void*)lb.Scan0;
				var t = InputT.Sample(Frame);
				
				switch (Mode) {
					case PlotMode.XContour:
					case PlotMode.XFill:
						
					
						foreach (var o in OutputR)
							o.PrepareArgs(InputX.length, OutputY.length, InputT.length, tasks);
						var yz = OutputY.Sample(FixedY);
						if(tasks <= 1) MultiX(0,bh, 0); else Static.TaskManager(_taskArr, tasks, chunks, bh, cancel, MultiX);
						break;
						void MultiX(float yf, float subChunkLength, int taskIndex) => Multi(yf,subChunkLength, taskIndex, TaskDrawX);
						void TaskDrawX(int ys, int ye, int taskIndex) {
							for (var y = ys; y < ye; ++y) {
								if (cancel.IsCancellationRequested)
									break;
								
								byte* p = ptr + lb.Stride * y;
								var yColor = _linesY[y];
								for (int x = 0, intPtr = 0; x < bw; ++x, ++intPtr, p += 3) {
									if (cancel.IsCancellationRequested)
										break;
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
													c = o.ProcessColor(prev[i], c, z, t, x, y, Frame, taskIndex);
										}
									else
										foreach (var o in OutputR)
											if (!(o.Eval?.Null() ?? true))
												foreach (var prevV in o.Values[intPtr].GetValues())
													if ((y < OutputY.ValueToScreen(T.zero)) == (OutputY.ValueToScreen(prevV.GetLeaf()) < y))
														c = o.ProcessColor(prevV, c, z, t, x, y, Frame, taskIndex);
									(p[2], p[1], p[0]) = GetRgb(c);
								}
								++percent[taskIndex];
								continue;
								bool C(int v, int n) => y < v != y <= n || y <= v != y < n;
							}
						}

					default:
						foreach (var o in OutputR)
							o.PrepareArgs(InputX.length, InputY.length, InputT.length, tasks);
						if(tasks <= 1) MultiXy(0,bh, 0); else Static.TaskManager(_taskArr, tasks, chunks, bh, cancel, MultiXy);
						break;
						void MultiXy(float yf, float subChunkLength, int taskIndex) => Multi(yf, subChunkLength, taskIndex, TaskDrawXy);
						void TaskDrawXy(int ys, int ye, int taskIndex) {
							var intPtr = ys * bw;
							for (var y = ys; y < ye; ++y) {
								
								if (cancel.IsCancellationRequested)
									break;
								var yz2 = InputY.Sample(y);
								byte* p = ptr + lb.Stride * y;
								var yColor = _linesY[y];
								
								for (var x = 0; x < bw; ++x, ++intPtr, p += 3) {
									if (cancel.IsCancellationRequested)
										break;
									var z = InputX.Sample(x) + yz2;
									var rgbc = Max(_linesX[x], yColor);
									(double r, double g, double b) c = (rgbc.R / 255.0, rgbc.G / 255.0, rgbc.B / 255.0);
									foreach (var o in OutputR)
										if (!(o.Eval?.Null() ?? true))
											//foreach (var prevV in o.Values[intPtr].GetValues())
											c = o.ProcessColor(o.Values[intPtr], c, z, t, x, y, Frame, taskIndex);
									(p[2], p[1], p[0]) = GetRgb(c);
								}
								++percent[taskIndex];
							}
						}
				}
                work.UnlockBits(lb);
                (_r.Bitmaps[Frame], _r.WorkMaps[Frame]) = (_r.WorkMaps[Frame], _r.Bitmaps[Frame]);

                void Multi(float yf, float subChunkLength, int taskIndex, Action<int, int, int> taskDraw) {
					//var args = (Value)argsO;


					float chunkDistance = tasks * subChunkLength;
					for (var c = 0; c < chunks; ++c) {
						float chd;
						int y = (int)Math.Round(chd = yf + c * chunkDistance);
						taskDraw(y, (int)Math.Round(chd + subChunkLength), taskIndex);
					}
				}
				(byte, byte, byte) GetRgb((double r, double g, double b) c) => ((byte)Math.Clamp(c.r * 255, 0, 255), (byte)Math.Clamp(c.g * 255, 0, 255),(byte)Math.Clamp(c.b * 255, 0, 255));
              
            }
			return work;
			Color Max(Color a, Color b) => Color.FromArgb(Math.Max(a.R, b.R), Math.Max(a.G, b.G), Math.Max(a.B, b.B));
		}
		public static int ValueToScreenLin(T value, T start, T d) => (int)(T.Re(~d * (value - start))/+d);//length * T.D2(value - start, end - start, Static.Div);
		public static T ScreenToValueLin(int x, T start, T d) => start + x * d;//INumber<T>.Lerp(start, end, new((double)x / length));
		public static int ValueToScreenLog(T value, T start, T d) => ValueToScreenLin(T.D1(value, Math.Log), start, d);//length * ((T.D1(value, Math.Log) - start) / (end - start));
		public static T ScreenToValueLog(int x, T start, T d) => T.D1(ScreenToValueLin(x, start, d),Math.Exp); //T.D1(ScreenToValueLin(length, x, start, end), Math.Exp);
	}
}