using Comparser.Comparser.Numbers;
namespace Comparser.Comparser;
public abstract partial class Comparser<T> {
	public partial class PlotEval {
		public class PlotFrame(bool centerPixels = true) {
			private readonly double _c = centerPixels ? .5 : 0;
			private static Complex MapC((Complex s, Complex x, Complex y) a, int x, int y) => a.s + (x + .5) * a.x + (y + .5) * a.y;
			private static Complex Map((Complex s, Complex x, Complex y) a, int x, int y) => a.s + x * a.x + y * a.y;
			private readonly unsafe delegate*<(Complex, Complex, Complex), int, int, Complex> _map = centerPixels ? &MapC : &Map;
			private static double MapClCu((Complex s, Complex x, Complex y) a, int x) => a.s.R + (x + .5) * a.x.R;
			private static double MapClu((Complex s, Complex x, Complex y) a, int x) => a.s.R + x * a.x.R;
			private static double MapClCv((Complex s, Complex x, Complex y) a, int y) => a.s.I + (y + .5) * a.y.I;
			private static double MapClv((Complex s, Complex x, Complex y) a, int y) => a.s.I + y * a.y.I;
			private readonly unsafe delegate*<(Complex, Complex, Complex), int, double> _mapClU = centerPixels ? &MapClCu : &MapClu;
			private readonly unsafe delegate*<(Complex, Complex, Complex), int, double> _mapClV = centerPixels ? &MapClCv : &MapClv;
			private readonly unsafe delegate*<double, int> _rnd = centerPixels ? &Static.Floor : &Static.Round;
			private T _mSx2 = T.NaN(), _mDx2 = T.NaN(), _mSy2 = T.NaN(), _mDy2 = T.NaN(), _t2 = T.NaN(); // ax.S, ax.d, ay.x, ay.d, frame
			private T _mSx1 = T.NaN(), _mDx1 = T.NaN(), _mY1 = T.NaN(), _t1 = T.NaN(); // ax.S, ax.d, yC, frame
			private int _mLx1, _mLx2, _mLy2; // X length of 1D, X length of 2D, Y length od 2D
			private Value[] _plotX = [], _plotXy = [], _memX = [], _memXy = [];
			public unsafe Value[] GetPlotXy(out bool changed, Expression exp, Plot.PlotAxis ax, Plot.PlotAxis ay, Plot.PlotAxis at, T frame, double recallTolerance, bool refresh = false) {
				T aS = ax.start + ay.start, mSx, mSy, mDx, mDy = mDx = mSy = mSx = T.Zero();
				int mLx = 0, mLy = 0, x = 0, y = 0, yw = 0;
				if (+(frame - _t2) <= +at.d * recallTolerance) // the time of this frame is within tolerance to the memorized time
					(mSx, mDx, mLx, mSy, mDy, mLy) = (_mSx2, _mDx2, _mLx2, _mSy2, _mDy2, _mLy2);
				(_plotXy, _memXy) = (_memXy, _plotXy); // swap mem
				if (_plotXy.Length != (_mLx2 = ax.length) * (_mLy2 = ay.length))
					_plotXy = new Value[ax.length * ay.length]; // length mismatch: re-alloc
				(_mLx2, _mLy2, _mSx2, _mSy2, _mDx2, _mDy2) = (ax.length, ay.length, ax.start, ay.start, ax.d, ay.d);
				if (AxisMatchA(mSx, mDx, ax) && AxisMatchA(mSy, mDy, ay)) {
					changed = false; 
					(_plotXy, _memXy) = (_memXy, _plotXy);
					return _plotXy;
				}
				changed = true;
				Value args = new([new(T.NaN(), "x"), new(T.NaN(), "y"), new(_t2 = frame, "t")]);
				if (refresh) // refresh completely without trying to transfer anything from memory
					return Rows(ay.length, Finish);
				double dXs = +ax.d, dYs = +ay.d;
				_t2 = frame;
				(Complex s, Complex x, Complex y) pm, mp;
				if (Math.Min(mLx, mLy) == 0 || FailAffineMap(mSx + mSy, Math.Min(dXs, dYs)))
					return Rows(ay.length, Finish); // Planes don't coincide.
				Complex cul = _map(mp, 0, 0), cur = _map(mp, mLx, 0), cdl = _map(mp, 0, mLy), cdr = _map(mp, mLx, mLy);
				(Complex l, Complex h) 
					i1 = cul.I < cdr.I ? (cul, cdr) : (cdr, cul), 
					i2 = cur.I < cdl.I ? (cur, cdl) : (cdl, cur),
					r1 = cul.R < cdr.R ? (cul, cdr) : (cdr, cul), 
					r2 = cur.R < cdl.R ? (cur, cdl) : (cdl, cur);
				//Complex uv, bounds, dBounds, top, bottom, left, right, midLeft, midRight;
				var (top, mid) = i1.l.I < i2.l.I ? (i1.l, r2) : (i2.l, r1);
				Complex bounds,
					bottom = i1.h.I >= i2.h.I ? i1.h : i2.h,
					left = r1.l.R < r2.l.R ? r1.l : r2.l,
					right = r1.h.R >= r2.h.R ? r1.h : r2.h;
				var (midLeft, midRight) = mid.l.R < mid.h.R ? (mid.l, mid.h) : (mid.h, mid.l);
				if (top.I >= ay.length || bottom.I < 0 || left.R >= ax.length || right.R < 0)
					return Rows(ay.length, Finish); // parallelogram is out of image bounds, just re-eval everything early
				Rows(Math.Min(ay.length, (int)top.I), Finish);
				// is the accumulated error in non-collinear axis over the bounding parallelogram exceeding 1 pixel in that other axis?
				bool dCol = Static.Sqr(mp.y.R / mp.x.R) * +(cur - cul) < dYs && Static.Sqr(mp.x.I / mp.y.I) * +(cdl - cul) < dXs,
					sCol = Static.Sqr(mp.y.I / mp.x.I) * +(cur - cul) < dYs && Static.Sqr(mp.x.R / mp.y.R) * +(cdl - cul) < dXs;
				if (dCol || sCol) {
					bounds = new(Math.Min(ax.length, Math.Floor(left.R)), Math.Min(ax.length, Math.Ceiling(right.R)));
					double modX, modY, pX = 0, phaseX = -Math.Min(0, bounds.R), phaseY = -Math.Min(0, top.I);
					int mulX = mLx, mulY = 1, py = 0, ex = Math.Min(ax.length, (int)bounds.I/* - 1*/);
					if (sCol) {
						mp = (Complex.Swap(mp.s), Complex.Swap(mp.x), Complex.Swap(mp.y));
						pm = (Complex.Swap(pm.s), Complex.Swap(pm.x), Complex.Swap(pm.y));
						(mulX, mulY) = (mulY, mulX);
						modX = mp.y.I; modY = mp.x.R;
					} else (modX, modY) = (mp.x.R, mp.y.I);
					Action xtest = Math.Abs(modX) <= 1 ? NoX : YesX;
					Action ytest = Math.Abs(modY) <= 1 ? NoY : YesY;
					void NoX() {
						for (Begin(); x < ex; ++x)
							_plotXy[x + yw] = _memXy[_rnd(_mapClU(pm, x)) * mulX + py * mulY];
					}
					void YesX() {
						for (Begin(); x < ex; ++pX, ++x)
							_plotXy[x + yw] = (pX % modX < 1) ? _memXy[_rnd(_mapClU(pm, x)) * mulX + py * mulY] : Eval();
					}
					Rows(Math.Min(ay.length, (int)bottom.I), ytest);
					void YesY() { pX = phaseX; py = _rnd(_mapClV(pm, y)); if (phaseY % modY < 1) xtest(); Finish(); ++phaseY; }
					void NoY() { py = _rnd(_mapClV(pm, y)); xtest(); Finish(); }
				} else {
					// parallelogram bounds incrementing rows, determine if the left/right bvertex is the higher one, and set up the ordering of the phases:
					top = new(top.R, top.I + 1);
					bottom = new(bottom.R, bottom.I - 1);
					bool rightFirst = midRight.I < midLeft.I;
					(midLeft, midRight) = rightFirst 
						? (new Complex(midLeft.R + 1, midLeft.I - 1), new Complex(midRight.R - 1, midRight.I + 1)) 
						: (new(midLeft.R + 1, midLeft.I + 1), new(midRight.R - 1, midRight.I - 1));
					double pDet = pm.x.R * pm.y.I - pm.y.R * pm.x.I, inv00 = pm.y.I / pDet, inv01 = -pm.y.R / pDet, inv10 = -pm.x.I / pDet, inv11 = pm.x.R / pDet; // inverse transform to determine one unique memory pixel
					double bri = bottom.I - midRight.I, bli = bottom.I - midLeft.I, lti = midLeft.I - top.I, rti = midRight.I - top.I; // subtractions used many times
					double secL = (bottom.R - midLeft.R) / bli, secR = (bottom.R - midRight.R) / bri; // the second interval bounds accumulators
					Complex[] d = new Complex[3], i = new Complex[4], bK = new Complex[4]; // incremental step of both bounds per row, vertices sorted from top, bounds keyframes per vertex (to eliminate incremental drifts when stepping over vertices)
					d[0] = new((midLeft.R - top.R) / lti, (midRight.R - top.R) / rti);
					d[2] = new(secL, secR);
					i[0] = top;
					i[3] = bottom;
					bK[0] = new(Math.Ceiling(top.R), Math.Floor(top.R));
					bK[3] = new(Math.Ceiling(bottom.R), Math.Floor(bottom.R));
					(d[1], i[1], i[2], bK[1], bK[2]) = midLeft.I > midRight.I // is left under right? (we will assume left comes first, so they will be swapped)
						? (new Complex(d[1].R, secR), midRight, midLeft, // right is first
							new Complex(Math.Ceiling(Static.Lerp(top.R, midLeft.R, rti / lti)), Math.Floor(midRight.R)), // 2nd keyframe is right vertex, and left vertex is in-between top-left
							new Complex(Math.Ceiling(midLeft.R), Math.Floor(Static.Lerp(bottom.R, midRight.R, bli / bri)))) // 3rd keyframe is left vertex, and right vertex is in-between right-bottom
						: (new(secL, d[0].I), midLeft, midRight, // left is first
							new(Math.Ceiling(midLeft.R), Math.Floor(Static.Lerp(top.R, midRight.R, lti / rti))), // 2nd keyframe is left vertex, and right vertex is in-between top-right
							new(Math.Ceiling(Static.Lerp(bottom.R, midLeft.R, bri / bli)), Math.Floor(midRight.R))); // 3rd keyframe is right vertex, and left vertex is in-between left-bottom
					Complex dBounds, uv;
					for (var p = 0; p < 4;) {
						if (i[++p].I < 0) continue;
						for (; p < 4; Rows((int)i[p].I, PgRow), ++p) { // iterate rows until each next vertex
							var p1 = p - 1;
							dBounds = d[p1]; // how much will each bounds value be incremented each row?
							bounds = INumber<Complex>.Lerp(bK[p1], bK[p], (y - i[p1].I) / (i[p].I - i[p1].I)) // take the initial bounds for this interval (lerp out location between keyframes)
								+ new Complex(Math.Max(dBounds.R, 0), Math.Min(dBounds.I, 0))
								- new Complex(Math.Min(dBounds.R, 0), Math.Max(dBounds.I, 0))
								+ new Complex(1, -1); // make one initial step if it is inwards, to avoid sampling outside the memory, better have 1px on the edge re-eval than to ask for bounds for every pixel
							if (i[p].I < ay.length)
								continue;
							Rows(ay.length, PgRow); // the vertex is below the bottom, co just continue all the way to the bottom, and then break
							break;
						}
						break;
					}
					void PgRow() {
						Begin();
						for (int e = Math.Min(ax.length, (int)bounds.I); x < e; _plotXy[x + yw] = Fracs(out var p) ? _memXy[p] : Eval(), ++x)
							uv = _map(pm, x, y);
						Finish();
						// ReSharper disable AccessToModifiedClosure
						bounds += dBounds;
						// ReSharper restore AccessToModifiedClosure
					}
					bool Fracs(out int p) {
						int px = _rnd(uv.R), py = _rnd(uv.I);
						double du = uv.R - (px + _c), dv = uv.I - (py + _c);
						p = px + py * mLx;
						return Test(inv00 * du + inv01 * dv) && Test(inv10 * du + inv11 * dv);
					}
				}
				return Rows(ay.length, Finish);

				bool Test(double t) => t is < .5 and >= -.5;
				bool AxisMatchA(T s, T d, Plot.PlotAxis a) => Math.Max(+(s - a.start), +((d - a.d) * a.length)) <= +a.d * recallTolerance;
				Value[] Rows(int ye, Action a) { for (; y < ye; a(), ++y) (x, yw) = (0, y * ax.length); return _plotXy; }
				void Begin() { int e = Math.Min(ax.length, (int)bounds.R); for (x = 0, yw = y * ax.length; x < e; ++x) E(); } // to the left of the outer bounds
				void Finish() { for (; x < ax.length; ++x) E(); } // to the right of the outer bounds
				Value Eval() { var l = args.Values; l[0].Leaf = ax.Sample(x); l[1].Leaf = ay.Sample(y); return exp.Eval(0, args); }
				void E() => _plotXy[x + yw] = Eval();
				bool FailAffineMap(T mS, double e) {
					T s; double xx = +mDx, xy = mDx | mDy, yy = +mDy, d = xx * yy - xy * xy; // Gram matrix of the old plot's two basis vectors.
					if (Math.Abs(d) <= 1e-8 || !(InPlane(mDx, mDy, ax.d, xx, xy, yy, d, e *= e)
						&& InPlane(mDx, mDy, ay.d, xx, xy, yy, d, e)
						&& InPlane(mDx, mDy, s = aS - mS, xx, xy, yy, d, e))) {
						mp = pm = default;
						return true;
					} // u = (q|oldDx)*yy-(q|oldDy)*x; v = (q|oldDy)*xx-(q|oldDx)*xy
					pm = (C(mDx, mDy, s /*= aS - mS*/, xx, xy, yy, d), C(mDx, mDy, ax.d, xx, xy, yy, d), C(mDx, mDy, ay.d, xx, xy, yy, d));
					(xx, xy, yy) = (+ax.d, ax.d | ay.d, +ay.d);
					mp = (C(ax.d, ay.d, -s, xx, xy, yy, d = xx * yy - xy * xy), C(ax.d, ay.d, mDx, xx, xy, yy, d), C(ax.d, ay.d, mDy, xx, xy, yy, d));
					return false;
				}
				static Complex C(T dx, T dy, T q, double xx, double xy, double yy, double det) {
					double qx = q | dx, qy = q | dy;
					return new((qx * yy - qy * xy) / det, (qy * xx - qx * xy) / det);
				}
				static bool InPlane(T dx, T dy, T q, double xx, double xy, double yy, double det, double tolerance) {
					double qx = q | dx, qy = q | dy, u = (qx * yy - qy * xy) / det, v = (qy * xx - qx * xy) / det;
					return +(q - dx * u - dy * v) <= tolerance;
				}
			}

			public Value[] GetPlotX(out bool changed, Expression exp, Plot.PlotAxis ax, Plot.PlotAxis ay, Plot.PlotAxis at, double y, T frame, double recallTolerance, bool refresh = false) {
				Value[] memY = []; changed = true;
				int memYo = -1, mLx = 0;
				T mSx = T.Zero(), mDx = T.Zero(), yC = ay.Sample(y); // y coordinate
				var sqrEy = +at.d * recallTolerance;
				(_plotX, _memX) = (_memX, _plotX); // swap mem
				if (_plotX.Length != (_mLx1 = ax.length)) _plotX = new Value[ax.length]; // length mismatch: re-alloc

				// remember this evaluated X axis
				Remember(); // fetch a 
				Value args = new([new(T.NaN(), "x"), new(_mY1 = yC, "y"), new(_t1 = frame, "t")]);
				_mY1 = yC;
				_t1 = frame;
				if (memYo < 0) return ReEval(); // no memory
				(_mSx1, _mDx1) = (ax.start, ax.d);
				// we have some memory Y match
				if (AxisOverlap.New(ax, mSx, mDx, mLx, recallTolerance, out var o)) {
					if (memY != _plotX) // x-axis is not identical to the memory	
						for (var x = 0; x < _plotX.Length; _plotX[x] = memY[x++ + memYo]) { } // the identical memory is the 2D plot, not already our 1D one
					// transfer the whole Y slice form the 2d memory (tolerance is < pixel, so the array lengths should match)
					(_plotX, _memX) = (_memX, _plotX); // switch back
					changed = false;
					return _plotX;
				}

				// no overlap: jut re-eval everything
				if (refresh || !(+o.Perp <= o.SqrE) || o.NoOverlap()) return ReEval();
				// eval 0-iaStart (that isn't in the memory)
				ReEval(0, o.IaStart);
				if (AxisMismatch(o, mDx, out var mt)) {
					var dScale = Math.Max(1, (int)Math.Round(Math.Abs(T.Re(o.A.d / mDx)))); // can do T.Re as they are co-linear
					if (StepMismatch(o, mDx * dScale, o.IaEnd - o.IaStart))
						return ReEval(); // the ratio between asked-memory step sizes is not an integer, the interlacing won't work, so re-eval all
					int x, phase; // find the phase when the axis maps to memory ( then we will loop: once "take from memory" then (dScale-1) times "re-eval") 
					for (phase = 0; phase < dScale && o.IaStart < o.IaEnd; _plotX[o.IaStart] = Eval(o.IaStart), mt = o.Map(++o.IaStart), ++phase)
						if (!DistanceMismatch(o, mt * mDx, o.IaStart))
							break; // stop evaluating once we find the phase match,then we can proceed with the interlacing
					if (phase == dScale) // failed to find any match that might be repeating every phase, just re-eval all:
						return ReEval(o.IaStart);
					for (phase = 0, x = o.IaStart; x < o.IaEnd; ++x) // do the interlacing
						_plotX[x] = phase++ % dScale == 0 ? memY[o.Map(x) + memYo] : Eval(x); // phase 0 % dScale = transfer, otherwise eval
				} else
					for (var x = o.IaStart; x < o.IaEnd; ++x) // the memory-asked axis bases match, so the transfer can be much simpler:
						_plotX[x] = memY[o.Map(x) + memYo];
				// eval the rest: iaEnd-length (that isn't in the memory)
				return ReEval(o.IaEnd);

				void Remember() {
					if (!(+(frame - _t1) < sqrEy)) return;
					// the time of this frame is within tolerance to the memorized time
					if (+(_mY1 - yC) <= +sqrEy) {
						(memY, memYo, mSx, mDx, mLx) = (_plotX, 0, _mSx1, _mDx1, _mLx1); // 1D memory Y match
						return;
					}
					//if (!CrossDimensionalMemory)
					//	return;
					// this might be an overkill, as 1D memory is far more likely to be matched
					// 1D memory didn't match, but the memorized 2D y-axis is still matching the asked one, so maybe there will be a matching X line here?
					int lo = 0, hi = _mLy2;
					var mSy = _mSy2 - yC;
					while (lo < hi) {
						var mid = lo + hi >> 1;var d = mSy + mid * _mDy2;
						var nd = +d;
						if (nd <= sqrEy) { // 2D memory Y match
							(memY, memYo, mSx, mDx) = (_plotXy, mid * (mLx = _mLx2), _mSx2, _mDx2);
							break;
						}
						if (+(d + _mDy2) < nd) lo = mid + 1; // target is closer to d stepped towards hi
						else hi = mid; // target is closer to d stepped away from hi
					}
				}
				//  we haven't found any memory Y match, or only might be intersecting at 0-1 points, 1 point intersection is not worth finding so just render the whole X line
				// So re-eval the whole frame
				Value[] ReEval(int start = 0, int end = int.MaxValue) {
					for (int x = start, iEnd = Math.Min(end, _plotX.Length); x < iEnd; ++x) // eval at the whole X axis range
						_plotX[x] = Eval(x); // eval at this x coordinate
					return _plotX;
				}
				Value Eval(int x) {
					args.Values[0].Leaf = ax.Sample(x);
					return exp.Eval(0, args);
				}
			}
		}
	}
}

//=> Math.Abs(u - Math.Truncate(u) - .5) <= epsX && Math.Abs(v - Math.Truncate(v) - .5) <= epsY;
/*int foundYp = -1, foundY = 0, lo = 0, hi = _mLy2;
						var failMatch2 = true;
						// try to find a match in 2D memory by halving the 2D memory interval
						T yC2; // y coordinate
						while ((foundY = lo + hi >> 1) != foundYp && (failMatch2 = +(yC2 = _mSy2 + foundY * _mDy2 - yC) >= eSqrY)) {
							// still more interval to halve, and yC didn't match foundY
							foundYp = foundY; // remember previous avg, to test if we have still can continue halving
							if (+(yC2 + _mDy2) < +(yC2 - _mDy2))
								lo = foundY; // yC is above foundY
							else
								hi = foundY; // yC is below foundY
						}
						if (!failMatch2) {*/
/*static (double u, double v) ProjectionCoordinates(T dx, T dy, T q) {
double xx = +dx, xy = dx | dy, yy = +dy, det = xx * yy - xy * xy, qx = q | dx, qy = q | dy;
return ((qx * yy - qy * xy) / det, (qy * xx - qx * xy) / det);
}*/

/*private readonly unsafe delegate*<(Complex, Complex, Complex), int, int, Complex> map = centerPixels ? &MapC : &Map;
			public static Complex MapC((Complex s, Complex x, Complex y) a, int x, int y) => a.s + (x + .5) * a.x + (y + .5) * a.y;
			public static Complex Map((Complex s, Complex x, Complex y) a, int x, int y) => a.s + x * a.x + y * a.y;
			private readonly unsafe delegate*<double, int> rnd = centerPixels ? &Floor : &Round;
			private static int Round(double x) => (int)Math.Round(x);
			private static int Floor(double x) => (int)x;
			private const bool CrossDimensionalMemory = true;
			private T _mSx1 = T.NaN(), _mDx1 = T.NaN(), _mY1 = T.NaN(), _t1 = T.NaN(); // 1D memory,  ax.S, ax.d, yC, frame
			private T _mSx2 = T.NaN(), _mDx2 = T.NaN(), _mSy2 = T.NaN(), _mDy2 = T.Zero(), _t2 = T.NaN(); // 2D memory, ax.S, ax.d, ay.x, ay.d, frame
			private int _mLx1 = 0, _mLx2 = 0, _mLy2 = 0; // 1D: ax.length, 2D: ax.length, ay.length
			private Value[] _plotX = [], _plotXy = [], _memX = [], _memXy = [];
			/// <summary>
			/// Evaluates 1D plot data in XY->HSV mode
			/// </summary>
			/// <param name="exp">Expression to evaluate</param>
			/// <param name="ax">X axis data (X range), horizontal plot</param>
			/// <param name="ay">Y axis data (Y range), vertical plot</param>
			/// <param name="at">T axis data (T range), animation</param>
			/// <param name="frame">requested time frame</param>
			/// <param name="recallTolerance">0=memorized evaluations must be exactly precise, 1=can take memorized pixels that at up to 1 pixel away</param>
			/// <returns></returns>
			public unsafe Value[] GetPlotXy(Expression exp, Plot.PlotAxis ax, Plot.PlotAxis ay, Plot.PlotAxis at, T frame, double recallTolerance = 0.5) {
				T mSx = T.Zero(), mDx = T.Zero(), mSy = T.Zero(), mDy = T.Zero();
				int mLx = 0, mLy = 0, x = 0, y = 0, yw = 0; double u = 0, v = 0;
				Value[] mem = _plotXy;
				if (+(frame - _t2) <= +at.d * recallTolerance) // the time of this frame is within tolerance to the memorized time
					(mSx, mDx, mLx, mSy, mDy, mLy) = (_mSx2, _mDx2, _mLx2, _mSy2, _mDy2, _mLy2);
				(_plotXy, _memXy) = (_memXy, _plotXy); // swap mem
				if(_plotXy.Length != (_mLx2 = ax.length) * (_mLy2 = ay.length)) _plotXy = new Value[ax.length * ay.length];// length mismatch: re-alloc
				(_mLx2, _mLy2, _mSx2, _mSy2, _mDx2, _mDy2) = (ax.length, ay.length, ax.S, ay.S, ax.d, ay.d);
				if (AxisMatchA(mSx, mDx, ax) && AxisMatchA(mSy, mDy, ay))
					return _plotXy;
				Value args = new([new(T.NaN(), "x"), new(T.NaN(), "y"), new(_t2 = frame, "t")]);
				(Complex s, Complex x, Complex y) pm, mp;
				if (Math.Min(mLx, mLy) == 0 || FailAffineMap(mSx + mSy,ax.S + ay.S, Math.Min(+ax.d, +ay.d)))
					return Rows(ay.length, Finish);
				Complex cul = map(mp, 0, 0), cur = map(mp, mLx, 0), cdl = map(mp, 0, mLy), cdr = map(mp, mLx, mLy);
				(int r, int i) boMin =  Sanitize(Complex.ToPair(Complex.Min(Complex.Min(cul, cur), Complex.Min(cdl, cdr)))),
					boMax =  Sanitize(Complex.ToPair(Complex.Max(Complex.Max(cul, cur), Complex.Max(cdl, cdr)))),
					biMin =  boMin, // ideally figure out the largest possible rectangle that is entire inside the parallelogram
					biMax = boMin; // ideally figure out the largest possible rectangle that is entire inside the parallelogram
				//Complex px = pm.Map(1, 0) - pm.Map(0, 0), py = pm.Map(0, 1) - pm.Map(0, 0); // ??? how to use this?
				//double epsX2 = mLx * mLx / +(cul - cur), epsY2 = mLy * mLy / +(cul - cdl); // map scale to prevent upscaling
				Rows(boMin.i, Finish); Rows(biMin.i, OuterRow); Rows(biMax.i, InnerRow); Rows(boMax.i, OuterRow);
				return Rows(ay.length, Finish);

				bool AxisMatchA(T s, T d, Plot.PlotAxis a) => Math.Max(+(s - a.S), +((d - a.d) * a.length)) <= +a.d * recallTolerance;
				(int r, int i) Sanitize((double r, double i) ri) => (Math.Min((int)Math.Floor(ri.r), ax.length), Math.Min((int)Math.Ceiling(ri.i), ay.length));
				Value[] Rows(int ye, Action a) { for (;y < ye; a(), ++y) (x, yw) = (0, y * ax.length); return _plotXy; }
				void OuterRow() { Begin(); End(); }
				void InnerRow() { Begin(); for (;x < biMin.r; ++x) _plotXy[x + yw] = InsideParallel() && Fracs() ? mem[Project()] : Eval(); // between outer and inner
					for (;x < biMax.r; _plotXy[x + yw] = Fracs() ? mem[Project()] : Eval(), ++x) (u, v) = Complex.ToPair(map(pm, x, y)); End(); }
				void Begin() { for (x = 0, yw = y * ax.length; x < boMin.r; ++x) E(); }
				void End() { for (;x < boMax.r; ++x) _plotXy[x + yw] = InsideParallel() && Fracs() ? mem[Project()] : Eval(); Finish(); }
				void Finish() { for (;x < ax.length; ++x) E(); } // to the right of the outer bounds
				bool InsideParallel() { (u, v) = Complex.ToPair(map(pm, x, y)); return u >= 0 && u < mLx && v >= 0 && v < mLy; }
				int Project() => rnd(u) + rnd(v) * mLx;
				bool Fracs() {
					// frac method
					//double du = u - Math.Round(u), dv = v - Math.Round(v); return du * du <= epsX2 && dv * dv <= epsY2;
					// ??? what is inv00-inv11?
					//round trip method:
					var (r, i) = Complex.ToPair(map(mp, rnd(u), rnd(v))); return rnd(r) == x && rnd(i) == y;
				}
				Value Eval() { var l = args.Values; l[0].Leaf = ax.Sample(x); l[1].Leaf = ay.Sample(y); return exp.Eval(0, args); }
				void E() => _plotXy[x + yw] = Eval();
				bool FailAffineMap(T mS, T aS, double e) {
					T s; double x = +mDx, xy = mDx | mDy, y = +mDy, d = x * y - xy * xy; // Gram matrix of the old plot's two basis vectors.
					if (Math.Abs(d) <= 1e-8 || !(InPlane(mDx, mDy, ax.d, x, xy, y, d, e *= e)
						&& InPlane(mDx, mDy, ay.d, x, xy, y, d, e)
						&& InPlane(mDx, mDy, s = aS - mS, x, xy, y, d, e))) {
						mp = pm = default; return true; }
					// u = (q|oldDx)*yy-(q|oldDy)*x; v = (q|oldDy)*xx-(q|oldDx)*xy
					pm = (C(mDx, mDy, s , x, xy, y, d), C(mDx, mDy, ax.d, x, xy, y, d), C(mDx, mDy, ay.d, x, xy, y, d));
					(x, xy, y) = (+ax.d, ax.d | ay.d, +ay.d);
					mp = (C(ax.d, ay.d, -s, x, xy, y, d = x * y - xy * xy), C(ax.d, ay.d, mDx, x, xy, y, d), C(ax.d, ay.d, mDy, x, xy, y, d));
					return false;
				}
				static Complex C(T dx, T dy, T q, double xx, double xy, double yy, double det) {
					double qx = q | dx, qy = q | dy;
					return new((qx * yy - qy * xy) / det, (qy * xx - qx * xy) / det);
				}
				static bool InPlane(T dx, T dy, T q, double xx, double xy, double yy, double det, double tolerance) {
					double qx = q | dx, qy = q | dy, u = (qx * yy - qy * xy) / det, v = (qy * xx - qx * xy) / det;
					return +(q - dx * u - dy * v) <= tolerance;
				}
			}
			/// <summary>
			/// Evaluates 1D plot data in X->Y mode
			/// </summary>
			/// <param name="exp">Expression to evaluate</param>
			/// <param name="ax">X axis data (X range), horizontal plot</param>
			/// <param name="ay">Y axis data (Y range), vertical plot</param>
			/// <param name="at">T axis data (T range), animation</param>
			/// <param name="y">requested y slice (0 = ay.S, 1 = ay.E)</param>
			/// <param name="frame">requested time frame</param>
			/// <param name="recallTolerance">0=memorized evaluations must be exactly precise, 1=can take memorized pixels that at up to 1 pixel away</param>
			/// <returns></returns>
			public Value[] GetPlotX(Expression exp, Plot.PlotAxis ax, Plot.PlotAxis ay, Plot.PlotAxis at, double y, T frame, double recallTolerance = 0.5) {
				Value[] memY = [];
				int memYo = -1, mLx = 0;
				T mSx = T.Zero(), mDx = T.Zero(), yC = ay.Sample(y); // y coordinate
				var sqrEy = +at.d * recallTolerance;
				(_plotX, _memX) =  (_memX, _plotX);// length mismatch: re-alloc
				if(_plotX.Length != (_mLx1 = ax.length)) _plotX = new Value[ax.length];// length mismatch: re-alloc

				// remember this evaluated X axis
				Remember(); // fetch a
				Value args = new([new(T.NaN(), "x"), new(_mY1 = yC, "y"), new(_t1 = frame, "t")]);
				if (memYo < 0) return ReEval();// no memory
				(_mSx1, _mDx1) = (ax.S, ax.d);
				// we have some memory Y match
				if (AxisOverlap.New(ax, mSx, mDx, mLx , recallTolerance, out var o)) {
					if (memY != _memX) //x axis is not identical to the memory
						for (var x = 0; x < _plotX.Length; _plotX[x] = memY[x++ + memYo]) { } // the identical memory is the 2D plot, not already our 1D one
					// transfer the whole Y slice form the 2d memory (tolerance is < pixel, so the array lengths should match)
					(_plotX, _memX) = (_memX, _plotX);// switch back
					return _plotX;
				}

				// no overlap: jut re-eval everything
				if (!(+o.Perp <= o.SqrE) || o.NoOverlap()) return ReEval();
				// eval 0-iaStart (that isn't in the memory)
				ReEval(0, o.IaStart);
				if (AxisMismatch(o, mDx, out var mt)) {
					var dScale = Math.Max(1, (int)Math.Round(Math.Abs(T.Re(o.A.d / mDx)))); // can do T.Re as they are co-linear
					if (StepMismatch(o, mDx * dScale, o.IaEnd-o.IaStart))
						return ReEval(); // the ratio between asked-memory step sizes is not an integer, the interlacing won't work, so re-eval all
					int x, phase; // find the phase when the axis maps to memory ( then we will loop: once "take from memory" then (dScale-1) times "re-eval")
					for (phase = 0; phase < dScale && o.IaStart < o.IaEnd; _plotX[o.IaStart] = Eval(o.IaStart), mt = o.Map(++o.IaStart), ++phase)
						if (!DistanceMismatch(o, mt * mDx, o.IaStart))
							break; // stop evaluating once we find the phase match,then we can proceed with the interlacing
					if (phase == dScale) // failed to find any match that might be repeating every phase, just re-eval all:
						return ReEval(o.IaStart);
					for (phase = 0, x = o.IaStart; x < o.IaEnd; ++x) // do the interlacing
						_plotX[x] = phase++ % dScale == 0 ? memY[o.Map(x) + memYo] : Eval(x); // phase 0 % dScale = transfer, otherwise eval
				} else for (var x = o.IaStart; x < o.IaEnd; ++x) // the memory-asked axis bases match, so the transfer can be much simpler:
						_plotX[x] = memY[o.Map(x) + memYo];
				// eval the rest: iaEnd-length (that isn't in the memory)
				return ReEval(o.IaEnd);

				void Remember() {
					if (!(+(frame - _t1) < sqrEy)) return;
					// the time of this frame is within tolerance to the memorized time
					if (+(_mY1 - yC) <= +sqrEy) {
						(memY, memYo, mSx, mDx, mLx) = (_plotX, 0, _mSx1, _mDx1, _mLx1); // 1D memory Y match
						return;
					}
					//if (!CrossDimensionalMemory)
					//	return;
					// this might be an overkill, as 1D memory is far more likely to be matched
					// 1D memory didn't match, but the memorized 2D y axis is still matching the asked one, so maybe there will be a matching X line here?
					int mid, lo = 0, hi = _mLy2;
					var mSy = _mSy2 - yC;
					while (lo < hi) {
						var d = mSy + (mid = lo + hi >> 1) * _mDy2;
						var nd = +d;
						if (nd <= sqrEy) { // 2D memory Y match
							(memY, memYo, mSx, mDx) = (_plotXy, mid * (mLx = _mLx2), _mSx2, _mDx2);
							break;
						}
						if (+(d + _mDy2) < nd) lo = mid + 1; // target is closer to d stepped towards hi
						else hi = mid; // target is closer to d stepped away from hi
					}
				}
				//  we haven't found any memory Y match, or only might be intersecting at 0-1 points, 1 point intersection is not worth finding so just render the whole X line
				// So re-eval the whole frame
				Value[] ReEval(int start = 0, int end = int.MaxValue) {
					for (int x = start, iEnd = Math.Min(end, _plotX.Length); x < iEnd; ++x) // eval at the whole X axis range
						_plotX[x] = Eval(x); // eval at this x coordinate
					return _plotX;
				}
				Value Eval(int x) {
					args.Values[0].Leaf = ax.Sample(x);
					return exp.Eval(0, args);
				}
			}*/