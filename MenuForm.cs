using Comparser.Comparser;
using Comparser.Comparser.Numbers;
using System.Drawing.Imaging;
using Timer = System.Threading.Timer;
namespace Comparser;
public abstract partial class MenuForm : Form {

	// COMBOBOX SELECT .txt CODE, buttons IMPORT CODE, SAVE CODE

	/*public enum DrawMode : byte {
		XFill, // X -> Fill Y
		XContour, // X -> Contour Y
		XyHsvLog, // XY -> LogHSV
		XyHsvLin, // XY -> LinHSV
		XyHsvExp, // XY -> ExpHSV
		XyHsv // XY -> Expressions HSV
	}
	public class Axis(bool initLog, Complex initS, Complex initE, int initLength = 0) {
		public bool log { get; set { if (value != (field = value)) dirty = true; } } = initLog;
		public Complex s { get; set { if (Complex.AreEqual(value, field = value)) return;dirty = true;  } } = initS;
		public Complex e { get; set { if (Complex.AreEqual(value, field = value)) return; dirty = true; } } = initE;
		public Func<int, int, Complex, Complex, Complex> Sv = initLog ? ScreenToValueLog : ScreenToValueLin;
		public Func<int, Complex, Complex, Complex, Complex> Vs = initLog ? ValueToScreenLog : ValueToScreenLin;
		public int length { get; set { if (value != (field = value))dirty = true;  } } = initLength;
		private bool dirty = true;
		public double[] lines { get => dirty ? (field = Lines()) : field; set; } = []; // plot lines
		private double[]  Lines() {
			dirty = false;
			// TODO
			return [];
		}
	}
	public List<Axis> InputR = [
		new(false, new(-10),new(10)), // X input (everywhere) [W]
		new(false, new(0,10),new(0,-10))]; // Y input (only XY->HSV modes) [H]
	public List<Axis> OutputR = [
		new(false, new(10),new(-10)), // first output (everywhere) [H]
		new(false, new(0,10),new(0,-10))]; // second output (only X->Y modes) [H]
	public Comparser.Gpu.Comparser<Complex>.Expression OutputHsv;
	public DrawMode Mode = DrawMode.XContour;
	private readonly Comparser.Gpu.Comparser<Complex> _context = new ComparserC();
	private readonly Comparser.Gpu.Comparser<Complex>.Value _x = new([new(Complex.NaN(), "x")]), _xy = new([new(Complex.NaN(), "x"), new(Complex.NaN(), "y")]);
	public Comparser.Gpu.Comparser<Complex>.Expression Eval;
	private Complex[] _plotX = [];
	private Complex[] _plotXY = [];
	public bool LockAspectRatio = true;

	public MenuForm() {
		InitializeComponent();
		OutputHsv = new(_context, "[repeatValue=1; (1)s(x)=sqrabs(x)] (arg(x)+pi)360/tau, 1-exp(-s(x)), sqrt(s(x))%repeatValue /* repeatValue: Value cycle slowness, (1)s(x): caches sqrabs for reuse", _x);
		Eval = new(_context, "x!", _xy);
		expBox.Visible = false;
		Init();
		Update();
	}
	public virtual void Init() {
		_context = new ComparserC();
	}

	private void Resize() {
		var pw = InputR[0].length, ph = InputR[1].length;


		int w = screen.Width, h = screen.Height, s = w * h;
		if (_plotX.Length != w) {
			_plotX = new Complex[w];
			 InputR[0].length = w;
		}
		if ( h != InputR[1].length) InputR[1].length = h;
		if (s != _plotXY.Length) {
			_plotXY = new Complex[s];
		}

	}

	private void Update(){

		Bitmap bmp = new(screen.Width, screen.Height);
		Complex[] plotGamma = new Complex[screen.Width + 1],
			plotFact = new Complex[screen.Width + 1];
		Comparser<Complex>.Value arg, argX = new([new("x")]); // define a single-argument function input with the alias "x"
		Comparser<Complex>.Expression
			factFunc = new(context, "x!", argX),
			gammaFunc = new(context, "gamma(x)", argX); // gamma function with an input x

		switch (Mode) {
			case DrawMode.XContour:
			case DrawMode.XFill:
				Axis(ref IRanges[0], screen.Width);
				var ay = Axis(LogY, screen.Height, ys, ye);
				break;
		}

		var ax = Axis(LogX, screen.Width, xs, xe);
		var ay = Axis(LogY, screen.Height, ys, ye);


		for (var x = 0; x <= screen.Width; ++x) {
			// convert screen space into input space and make argument input from it mapping x:inputX
			arg = context.MakeArgs([("x", svx(screen.Width, x, xs, xe))]);
			// evaluate functions with x:inputX
			plotFact[x] = E(factFunc, arg); // "x!"
			plotGamma[x] = E(gammaFunc, arg); // "gamma(x)"
		}
		Func<int, Complex> YT = Mode == DrawMode.Fill ? GetYTest : NoYTest;
		unsafe {
			var l = bmp.LockBits(new(0, 0, screen.Width, screen.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
			byte* p, ptr = (byte*)(void*)l.Scan0;
			for (var y = 0; y < screen.Height; ++y) {
				p = ptr + l.Stride * y;
				var yTest = YT(y);
				for (var x = 0; x < screen.Width; ++x) {
					p[2] = 0;//(byte)(T(plotGamma) ? 255 : 0); // gamma
					p[1] = (byte)Math.Max(ax.r[x], ay.r[y]); //(byte)(x == screen.Width / 2 || y == screen.Height / 2 ? 255 : 0); // axes
					p[0] = (byte)Math.Max(ax.i[x], ay.i[y]);//(byte)(T(plotFact) ? 255 : 0); // factorial
					p += 3;
					continue;

					bool T(Complex[] plot) => Test(Complex.Re(plot[x]), Complex.Re(plot[x + 1]));
				}
				continue;

				(bool, bool) Test(Complex value, Complex next) => Mode switch {
					DrawMode.Fill => (yTest < 0 == value < yTest, yTest < 0 == value < yTest),
					DrawMode.Contour => C(value, next),
					_ => false
				};
				(bool, bool) C(double value, double next) {
					int v = vsy(screen.Height, value, ys, ye), n = vsy(screen.Height, next, ys, ye);
					return y < v != y <= n || y <= v != y < n;
				}
			}

			bmp.UnlockBits(l);
			screen.Image = bmp;
		}
		return;

		(double[] r, double[] i) Axis(bool log, int screenLength, Complex start, Complex end) {
			const int divBase = 10;
			(double[] r, double[] i) axis = (new double[screenLength], new double[screenLength]);
			if (log) {
				var eStart = Complex.D1(start, Math.Exp);
				var eEnd = Complex.D1(end, Math.Exp);
				Log(eStart, eEnd);
				//Log(Complex.Re(eStart), Complex.Re(eEnd), Complex.Im(eStart), Complex.Im(eEnd));
			} else {

				Lin(Complex.Re(start), Complex.Re(end), axis.r);
				Lin(Complex.Im(start), Complex.Im(end), axis.i);

			}
			return axis;
			void Abs(ref double si, ref double ei) {
				(si, ei) = (Math.Min(si, ei), Math.Max(si, ei));
				//return (ei - si) < 1e-8;
			}
			void Lin(Complex lStart, Complex lEnd) {
				(lStart, lEnd) = (Complex.D2(lStart, lEnd, Math.Min), Complex.D2(lStart, lEnd, Math.Max));
				var h = Lc(lEnd - lStart);
				var mod = Complex.D1(lEnd - lStart, (x) => Math.Log(Math.Abs(x)) / Math.Log(divBase) % 1); //Complex.D1(lEnd - lStart, (x) => Math.Pow(2,Math.Log(Math.Abs(x)) / Math.Log(divBase) % 1));
				byte d = 64;
				_lineB[0] = Complex.ToPair(d * (1 - mod));
				for (int i = 1; i < _lineB.Length - 2; ++i)
					_lineB[i] = Complex.ToPair((d >>= 1) * (1 + mod));
				_lineB[^2] = (d, d);
				_lineB[^1] = Complex.ToPair(mod * d);

				lineB[0] = 128;
				for(byte i = 1, d = 1; i < lineB.Length; ++i, d <<= 1)
					lineB[i] = (byte)(lineB[0] / (d*mod));
				lineB[^1] = (byte)(lineB[^1] / (mod * mod * 2));
				lineB[^2] = (byte)(lineB[^2] / mod);
				for (byte b = 0; b < _lineB.Length; ++b, h /= 10) {
					Complex fs = Complex.Floor(lStart / h), fe = Complex.Floor(lEnd / h);
					for (int i = (int)Complex.Mix(fs, Math.Min), e = (int)Complex.Mix(fe,Math.Max); i <= e; ++i)
						DrawLine( Complex.ToPair(Complex.Floor(ValueToScreenLin(screenLength, i * h, start, end))), _lineB[b]);
				}
			}
			void DrawLine((double r, double i) s, (double r, double i) a) {
				if (s.r >= 0 && s.r < screenLength)
					axis.r[(int)s.r] += a.r;
				if (s.i >= 0 && s.i < screenLength)
					axis.i[(int)s.i] += a.i;
			}
			void Log(Complex eStart, Complex eEnd) {
				(eStart, eEnd) = (Complex.D2(eStart,eEnd, Math.Min), Complex.D2(eStart, eEnd, Math.Max));
				var scale = divBase*Lc(eEnd); // find some power of 10 that is above the ei bounds, to go down from there
				// main tiling squares (only this dividing should have equal distances visually)
				//double minS = Complex.Mix(eStart, eEnd, Math.Min);
				while (true) {
					scale /= divBase;
					byte f = 0;
					DrawLine(Complex.ToPair(Complex.Floor(ValueToScreenLog(screenLength, scale, start, end))), (64, 64));
					Lg(scale, scale * divBase, 32);
					var s = Complex.ToPair(divBase * scale);
					var es = Complex.ToPair(eStart);
					if (s.r < es.r && s.i < es.i)
						break;
				}
				return;
				// subdivisions of each subinterval into 10 linear pieces.
				void Lg(Complex from, Complex to, byte b) {
					if (b < 12)
						return;
					var newTo = from;
					for (double i = 0; i <= .95; ) {
						var newFrom = newTo;
						DrawLine(Complex.ToPair(Complex.Floor(ValueToScreenLog(screenLength, newTo = (i += 1.0 / 9) * to + (1 - i) * from, start, end))), (b, b));
						//if(newFrom < ei && newTo > si)
						Lg(newFrom, newTo, ++b);
					}
				}
				var scale = divBase*L(ei); // find some power of 10 that is above the ei bounds, to go down from there
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
				}
			}
			Complex Lc(Complex c) => Complex.D1(c, (x) => Math.Round(Math.Pow(divBase, Math.Floor(Math.Log(Math.Abs(x))/Math.Log(divBase)))));
			double L(double x) => Math.Round(Math.Pow(divBase, Math.Floor(Math.Log(Math.Abs(x))/Math.Log(divBase))));
		}
		Complex GetYTest(int y) => svy(screen.Height, y, ys, ye);
		Complex NoYTest(int _) => Complex.Zero();

		Complex E(Comparser.Gpu.Comparser<Complex>.Expression e, Comparser.Gpu.Comparser<Complex>.Value a) => e.Eval(0, a).GetLeaf();
	}
	public static Complex ValueToScreenLin(int length, Complex value, Complex start, Complex end) => length * Complex.D2(value - start, end - start, INumber<Complex>.Div);
	public static Complex ScreenToValueLin(int length, int x, Complex start, Complex end) => INumber<Complex>.Lerp(start, end, new((double)x / length));
	public static Complex ValueToScreenLog(int length, Complex value, Complex start, Complex end) => length * ((Complex.D1(value, Math.Log) - start) / (end - start));
	public static Complex ScreenToValueLog(int length, int x, Complex start, Complex end) => Complex.D1(ScreenToValueLin(length, x, start, end), Math.Exp);
	private void fps_Tick(object? sender, EventArgs e) {
		IRanges[0].s *= .01;
		IRanges[0].e *= .01;
		ORanges[0].s *= .01;
		ORanges[0].e *= .01;
		Update();

	}
	private readonly (double r, double i)[] _lineB = new (double r, double i)[4];
	//  x = l(Log(v) - s)(e-s);
}/*bool C(double value, double next) {
					var v = ValueToScreen(screen.Height, value, ys, ye);
					return y < v != y < ValueToScreen(screen.Height, next, ys, ye);
				}*/
/*double xc = 0;
	var xRange = 20;
	var center = (int)(screen.Width / 2.0 - xc * screen.Width / xRange);
unsafe {
			Complex[] plotGamma = new Complex[screen.Width + 1],
				plotFact = new Complex[screen.Width + 1];
			fixed (Complex* ptrFact = plotFact)
				INumber<Complex>.I_Factorial_Batched(ptrFact, plotFact.Length, new(), screen.Width, xRange, center);
			fixed (Complex* ptrGamma = plotGamma)
				INumber<Complex>.I_Gamma_Batched(ptrGamma, plotGamma.Length, new(), screen.Width, xRange, screen.Width / 2);
			var l = bmp.LockBits(new(0, 0, screen.Width, screen.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
			byte* p, ptr = (byte*)(void*)l.Scan0;
			for (int y = 0; y < screen.Height; ++y) {
				p = ptr + l.Stride * y;
				double g = Complex.Re(plotGamma[0]), f = Complex.Re(plotFact[0]);
				bool bf = false, bg = false;
				for (int x = 0; x < screen.Width; ++x) {
					var yt = (double)y / screen.Height;
					var yr = yt * ye + ys * (1 - yt);
					bool fact = bf || (bf = f < yr != (f = Complex.Re(plotFact[0])) < yr),
						gamma = bg || (bg = g < yr != (g = Complex.Re(plotGamma[0])) < yr);
					p[2] = (byte)(gamma ? 255 : 0); // gamma
					p[1] = (byte)(x == screen.Width / 2 || y == screen.Height / 2 ? 255 : 0); // axes
					p[0] = (byte)(fact ? 255 : 0); // factorial
					p += 3;
				}
			}

			bmp.UnlockBits(l);
			screen.Image = bmp;
		}*/
// this part tries to detect the best base, that would divide the first loop into 2-5 main squares
/*double GetDiff() => Math.Abs(ValueToScreenLog(screenLength, divBase, start, end) - ValueToScreenLog(screenLength, 1, start, end)) / (double)screenLength;
var dif = GetDiff();
while (dif <  .5 / 2.5) {
	divBase *= divBase;
	dif = GetDiff();
}
while (dif > .5) {
	divBase = Math.Sqrt(divBase);
	dif = GetDiff();
}
var diff = (byte)(255.0*Math.Abs(dif)); // dynamic top brightness*/
}