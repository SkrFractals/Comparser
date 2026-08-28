namespace Comparser.Comparser;
public abstract partial class Comparser<T> {
	public partial class PlotEval {
		private Expression _exp;
		private string _text;
		private T _mSt = T.NaN();
		private T _mDt = T.NaN();
		private int _mLt;
		private readonly Comparser<T> _context;

		private static readonly Value Xyt = new([new(T.NaN(), 0, "x"), new(T.NaN(), 0, "y"), new(T.NaN(), 0, "t")]);
		public PlotEval(Comparser<T> context, string text) => _exp = new(_context = context, _text = text, Xyt);
		public void ReParse(string text) {
			if (_text == (_text = text)) return;
			_exp = new(_context, text, Xyt);
		}
		private PlotFrame[] _plot = [];

		// Axis is a Complex base range struct, the plot coord bases are each a complex/quat number, generic T.
		// x: (S = input x value on the left side of the screen, step = step value, pixel[x]-pixel[x-1], length = screen width, S + step*length = input x value on the right side of the screen)
		// y: (S = input y value on the left side of the screen, step = step value, pixel[y]-pixel[y-1], length = screen height, S + step*length = input y value on the right side of the screen)
		// t: (S = input time on the left side of the screen, step = step value, time[x]-time[x-1], length = animation length, S + step*length = input time on the right side of the screen)
		// all x,y,t are complex numbers, animation will lerp time complex input from t.S to t.S + t.step * t.length.
		// basically the frame want to plot a function f(x,y,t), with x = T.Lerp(axisX.S,axisX.S+axisX.step*axisX.length,screen.x/screen.width), same for y, and t = T.Lerp(axisT.S, ..., frame/animationLength) 
		public Value[] GetPlotX(out bool changed, Plot.PlotAxis ax, Plot.PlotAxis ay, double y, Plot.PlotAxis at, int frame, double recallTolerance = .5) {
			if (recallTolerance >= 1) throw new("tolerance must be less than a full pixel, otherwise we could get memory transfer indexing mismatches!");
			TransferOverlap(at, recallTolerance *= recallTolerance); // all distances are squared
			_mSt = at.start;
			_mDt = at.d;
			return _plot[frame].GetPlotX(out changed, _exp, ax, ay,  at, y, _mSt + frame * _mDt, recallTolerance);
		}
		public Value[] GetPlotXy(out bool changed, Plot.PlotAxis ax, Plot.PlotAxis ay, Plot.PlotAxis at, int frame, double recallTolerance = .5) {
			if (recallTolerance >= 1) throw new("tolerance must be less than a full pixel, otherwise we could get memory transfer indexing mismatches!");
			TransferOverlap(at, recallTolerance *= recallTolerance); // all distances are squared
			_mSt = at.start;
			_mDt = at.d;
			return _plot[frame].GetPlotXy(out changed, _exp, ax, ay, at, _mSt + frame * _mDt, recallTolerance);
		}
		public class AxisOverlap {
			public static bool New(Plot.PlotAxis a, T mSa, T mDa, int mLa, /*int memoryLength,*/ double recallTolerance, out AxisOverlap o)
				=> (o = new(a, mSa, mDa, mLa, recallTolerance/*, memoryLength*/)).Dmm < 0;

			private AxisOverlap(Plot.PlotAxis a, T mSa, T mDa, int mLa, double recallTolerance/*, int memoryLength*/) {
				//_memL = memoryLength;
				Dm = (_me = (Ms = mSa) + (_mLa = mLa) * mDa) - Ms; // Ms = start of memory line, _me = end of memory line
				_ae = a.start + (Da = (A = a).size);  // _ae = end of asked axis line, Da = size of the asked line (directional vector)
				var ds = a.start - Ms;
				var de = _ae - _me; // dm = memory vector, da = axis vector, ds,de = memory -> axis (between starts and ends)
				if (+ds <= (SqrE = +a.d * recallTolerance) && +de <= SqrE) // |t.step| is the pixel size, as long as the difference is smaller that than, I'll accept it
					return; // axis almost identical to memory, no need to change anything
				Dmm = +Dm;
				Daa = +Da;
				Dma = T.Dot(Dm, Da);
				Dms = T.Dot(Dm, ds);
				Das = T.Dot(Da, ds);
				var t0 = Dms / Dmm;
				Perp = ds - Dm * t0;
			}
			public bool NoOverlap() {
				double mStart = Dms / Dmm, mEnd = T.Dot(_ae - Ms, Dm) / Dmm, // mStart-mEnd should be the inverse lerp of the overlap in memory's reference frame
					aStart = -Dms / Daa, aEnd = T.Dot(_me - A.start, Da) / Daa; // aStart-aEnd should be the inverse lerp of the overlap in asked axis's reference frame
				if (aStart > aEnd) (aStart, aEnd, mStart, mEnd) = (aEnd, aStart, mEnd, mStart); // make sure I can iterate forward. Assuming they already had matching starts and ends, I also flip the memory interval
				_mapA = (mEnd - mStart) / (aEnd - aStart);
				_mapB = mStart - _mapA * aStart;
				// is this the range on the time axis line, that has a matching overlap with the time memory line?
				IaStart = Math.Max(0, (int)Math.Floor(aStart * A.length) - 1);
				IaEnd = Math.Min(A.length, (int)Math.Ceiling(aEnd * A.length) + 1);
				// this code might not account for negative _mapA!
				/*var iMStart = (int)Math.Round(_mapA * IaStart + _mapB);
				if (iMStart >= _memL) // beginning of axis's overlap if beyond the array end of the memory - no overlap
					return true;
				if (iMStart < 0) // beginning of axis's overlap if before the array of the memory begins - shift the bounds to that
					IaStart = Math.Min((int)Math.Round((iMStart - _mapB) / _mapA), A.length - 1);
				var iMEnd = (int)Math.Round(_mapA * IaEnd + _mapB);
				if (iMEnd < 0) // end of axis's overlap if before the array of the memory begins - no overlap 
					return true;
				if (iMEnd >= _memL) // beginning of axis's overlap if beyond the array end of the memory - shift the bounds to that
					IaEnd = Math.Max((int)Math.Round((iMEnd - _mapB) / _mapA) + 1, 0);*/
				// this should:
				double iaForM0 = -_mapB / _mapA, iaForM1 = (/*_memL*/_mLa - 1 - _mapB) / _mapA;
				IaStart = Math.Max(IaStart, (int)Math.Ceiling(Math.Min(iaForM0, iaForM1)));
				IaEnd   = Math.Min(IaEnd, (int)Math.Floor(Math.Max(iaForM0, iaForM1)) + 1);
				return IaStart >= IaEnd;
			}
			public int Map(int iat) => (int)Math.Round(_mapA * iat + _mapB);
			public readonly double SqrE, Dmm = -1, Daa, Dma, Dms, Das;
			private double _mapA, _mapB;
			private readonly T _me, _ae;
			public readonly T Ms, Dm, Da, Perp;
			private readonly int _mLa;
			public int IaStart, IaEnd;
			//public object[] mem;
			public readonly Plot.PlotAxis A;
		}
		private void TransferOverlap(Plot.PlotAxis t, double recallTolerance) {
			if (AxisOverlap.New(t, _mSt, _mDt, _mLt, /*_plot.Length,*/ recallTolerance, out var o))
				return; // identical axis up to the tolerance
			if (+o.Perp <= o.SqrE) {
				// aSt lies on A's line
				var mem = _plot;
				if (_plot.Length != o.A.length)
					New(); // length mismatch: re-alloc
				if (o.NoOverlap())
					return; // no overlap
				// now that we hopefully have the actual overlap bound for both, and the mapping between them, go through the overlap on the new axis level, and check if the corresponding overlap points in memory are usable.
				if (AxisMismatch(o, _mDt, out var mt)) {
					if (!DistanceMismatch(o, mt * _mDt, o.IaStart))
						_plot[o.IaStart] = mem[mt]; // we already calculated the first mt in the condition, so use that for the first step
					for (var iat = ++o.IaStart; iat < o.IaEnd; ++iat) 
						// the memory-asked steps or phases don't match, so we should compare every frame distance
						if(!DistanceMismatch(o,(mt = o.Map(iat)) * _mDt, iat))
							_plot[iat] = mem[mt]; // the closest time frame in the memory overlap is close enough in time to this time frame on the new axis - transfer it.
					return;
				}
				for (var iat = o.IaStart; iat < o.IaEnd; ++iat) // the memory-asked axes match, so we won't need to compare every frame distance:
					_plot[iat] = mem[o.Map(iat)]; // transfer it.
				return;
			}
			// not a parallel overlap - try to check a single point overlap at least:
			double den = o.Dma * o.Dma - o.Dmm * o.Daa,
				cm = (o.Dma * o.Das - o.Daa * o.Dms) / den,
				ca = (o.Dmm * o.Das - o.Dma * o.Dms) / den;
			T pm = o.Ms + o.Dm * cm, pa = o.A.start + o.Da * ca;
			if (+(pm - pa) <= +o.A.d) {
				// Memory and asked axis times intersect, im = inverse lerp on memory, ia = inverse lerp on axis
				int imt = (int)Math.Round(cm * _mLt), iat = (int)Math.Round(ca * o.A.length);
				// are both indices with the range of the array? And are the closest rounded points actually close to each other in frame space?
				if (imt >= 0 && imt < _mLt && iat >= 0 && iat < o.A.length && !DistanceMismatch(o, imt * _mDt, iat)) {
					var recover = _plot[imt]; // take that single frame that memory and new axis share
					New()[iat] = recover; // put it into out new timeline
					return;
				}
			}
			// no time intersection, scrap it all, and make a new timeline
			New();
			return;
			PlotFrame[] New() {
				_plot = new PlotFrame[_mLt = t.length];
				for (int i = _mLt; 0 <= --i; _plot[i] = new()) { }
				return _plot;
			}
		}
		public static bool AxisMismatch(AxisOverlap o, T mD, out int mt) 
			=> DistanceMismatch(o, (mt = o.Map(o.IaStart)) * mD, o.IaStart) || StepMismatch(o, mD);
		// test if the steps are equal or opposite to each other, so that the affine map could remain in lockstep for the whole range
		public static bool StepMismatch(AxisOverlap o, T step, int range = int.MaxValue)
			=> Math.Min(+((step - o.A.d) * (range = Math.Min(range, o.A.length))), +((step + o.A.d) * range)) > o.SqrE;
		// test if the Overlap memory point offset in sample space maps to the same pixel as iAth sample of the axis
		public static bool DistanceMismatch(AxisOverlap o, T offset, int iA) => +(o.Ms + offset - o.A.Sample(iA)) > o.SqrE;
	}
}
//return (_plot[frame]/* ?? (_plot[frame] = new())*/).GetPlot(xa, ya, y, _mSt + frame*_mStepT);