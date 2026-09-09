using Comparser.Comparser;
using Comparser.Comparser.Numbers;
using System.Diagnostics;
using System.Globalization;
namespace Comparser.Forms;
public partial class PlotControl : ParentControl {
	#region Structures
	public record SceState(string s, string c, string e, int l);
	public class Output(string name, RichTextBox rgb, RichTextBox code, EventHandler rgbChanged, EventHandler codeChanged) {
		public readonly TextField
			Rgb = new(rgb, rgbChanged, "hsv2rgb(log2hsv(v))", ["v", "c", "z", "t", "x", "y", "f", "w", "h", "l"]),
			Code = new(code, codeChanged, "z*sin(t)", ["z", "t"]);
		public readonly string Name = name;
		public int Clip = 0;
	}
	public class AxisControls {
		public EventHandler? Changed;
		public AxisControls(IComparser context, RichTextBox s, RichTextBox c, RichTextBox e, Button ls, Button lc, Button le, /*CheckBox l, */IPlotAxis axis, AxisControls? old = null) {
			_context = context;
			_axis = axis;
			//(LogBox = l).CheckedChanged += ChangeL;
			S = new(s, ChangeS, old?.S.Text ?? "");
			C = new(c, ChangeC, old?.C.Text ?? "");
			E = new(e, ChangeE, old?.E.Text ?? "");
			Ls = ls;ls.Click += (_,_) => Lock(0);
			_lc = lc;lc.Click += (_, _) => Lock(1);
			_le = le;le.Click += (_, _) => Lock(2);
			S.Box.Enabled = C.Box.Enabled = E.Box.Enabled = false;
			(S.Box.Text, C.Box.Text, E.Box.Text, _locked) = _axis.GetSce();
			S.Box.Enabled = C.Box.Enabled = E.Box.Enabled = true;
			//logBox.Tag = dBox.Tag = sBox.Tag = Axis = this;
		}
		public SceState GetState() => new(S.Text, C.Text, E.Text, _locked);
		public States? States;
		private readonly IComparser _context;
		public readonly TextField S, C, E;
		public readonly Button Ls;
		private readonly Button _lc, _le;
		private int _locked;
		//public object? ExpS, ExpD;
		//public bool Log = false;
		//public readonly RichTextBox SBox;
		//public readonly RichTextBox DBox;
		//public readonly CheckBox LogBox;
		private readonly IPlotAxis _axis;
		private void Lock(int newLocked) {
			L(false);
			_axis.SetL(_locked = _locked == newLocked ? -1 : newLocked);
			L(true);
			States?.Log(Ls); // any of them will work
			return;
			void L(bool l) {
				(_locked switch { 0 => S, 1 => C, 2 => E, _ => null })?.Box.ReadOnly = l;
				if(_locked switch { 0 => Ls, 1 => _lc, 2 => _le, _ => null } is { } b)
					SetLock(_context, b, l);
			}
		}
		private void ChangeS(object? sender, EventArgs e) => Change(S, C, E, _axis.SetS);
		private void ChangeC(object? sender, EventArgs e) => Change(C, S, E, _axis.SetC);
		private void ChangeE(object? sender, EventArgs e) => Change(E, S, C, _axis.SetE);
		//private void ChangeL(object? sender, EventArgs e) => SetSce(_axis.SetLog(LogBox.Checked));
		public void SetSce(SceState sce, bool changeAxis = false) {
			//if (S.Box is null || C.Box is null || E.Box is null)
			//	return;
			//var (s, c, e) = (S.Box.ReadOnly, C.Box.ReadOnly, E.Box.ReadOnly);
			S.Box.Enabled = C.Box.Enabled = E.Box.Enabled = false;
			(S.Box.Text, C.Box.Text, E.Box.Text, var wasLocked) = sce;
			if (changeAxis) {
				_axis.SetSce(Eval(_context, S), Eval(_context, E)); // TODO var ml = locked; locked = 0; start = S; end = E; locked = ml;
				Lock(-1); //remove previous lock (so that the next call will 100% succeed in locking a specific lock, instead of toggling it off)
				Lock(wasLocked);
			}
			S.Box.Enabled = C.Box.Enabled = E.Box.Enabled = true;
			Changed?.Invoke(this, EventArgs.Empty);
			//S.Box.ReadOnly = s; C.Box.ReadOnly = c; E.Box.ReadOnly = e;
		}
		private void Change(TextField change, TextField first, TextField second, Func<object?, (string, string)> del) {
			if (/*change.Box is null ||*/ !change.Box.Enabled /*|| first.Box is null || second.Box is null*/)
				return;
			//var (fir, sec) = (first.Box.ReadOnly, second.Box.ReadOnly);
			first.Box.Enabled = second.Box.Enabled = false;
			(first.Box.Text, second.Box.Text) = del(Eval(_context, change));
			first.Box.Enabled = second.Box.Enabled = true;
			//first.Box.ReadOnly = fir; second.Box.ReadOnly = sec;
			States?.Log(Ls); // any of them will work
		}
		public void SetLength(int l) => SetSce(_axis.SetLength(l));
	}
	#endregion

	#region Variables
	private readonly PlotSettings _s;
	private readonly TextField _fy = new(), _w = new(), _h = new(), _tf = new(), _tl = new();
	private readonly List<Output> _outputs = [];
	private AxisControls? _inputX, _inputY, _inputT, _outputY;
	private bool _rangeX = true, _rangeY = true, _rangeO = true, _rangeT = true, _lockedRes;
	private Task? _draw;
	private Bitmap? bmp;
	private Stopwatch plotDelay = new();
	private int _finishedImage, _length = 1, _frame;
	private double _fixedY;
	#endregion

	#region Inits
	public PlotControl() {
		InitializeComponent();
		_s = new PlotSettings();
		splitContainer.Panel1.Controls.Add(_s);
		_s.Location = new Point(0, 0);
		_s.Size = splitContainer.Panel1.Size;
		_s.Dock = DockStyle.Fill;
	}
	public PlotControl(MenuControl? root, ParentForm parent) : base(root, parent) {
		InitializeComponent();
		_s = new PlotSettings {
			Location = new Point(0, 0),
			Size = splitContainer.Panel1.Size,
			Dock = DockStyle.Fill
		};
		splitContainer.Panel1.Controls.Add(_s);
		splitContainer.Panel1.AutoScroll = true;
		splitContainer.Panel1.AutoScrollMinSize = _s.Size + new Size(6, 6);
		//splitContainer.Panel1MinSize = 320;
		//splitContainer.Panel2MinSize = 1;
		//plotBox.Location = new(0, 0);
		//plotBox.Size = splitContainer.Panel2.Size;
		_s.Dock = DockStyle.Fill;
		if (root == null || SettingsControl.Context is not { } context)
			return;
		var p = GetPlot();
		if (p is null)
			return;
		_inputX = new(context, _s.ixsBox, _s.ixcBox, _s.ixeBox, _s.ixsButton, _s.ixcButton, _s.ixeButton, p.GetAxis()[0]);
		_inputY = new(context, _s.iysBox, _s.iycBox, _s.iyeBox, _s.iysButton, _s.iycButton, _s.iyeButton, p.GetAxis()[1]);
		_inputT = new(context, _s.itsBox, _s.itcBox, _s.iteBox, _s.itsButton, _s.itcButton, _s.iteButton, p.GetAxis()[2]);
		_outputY = new(context, _s.oysBox, _s.oycBox, _s.oyeBox, _s.oysButton, _s.oycButton, _s.oyeButton, p.GetAxis()[3]);
		_s.modeSelect.SelectedIndexChanged += ModeSelected;
		_w = new(_s.widthBox, WidthChanged);
		_h = new(_s.heightBox, HeightChanged);
		_fy = new(_s.fyBox, FyChanged);
		_tf = new(_s.itfBox, TfChanged);
		_tl = new(_s.itlBox, TlChanged);
		_s.addButton.Click += Add;
		_s.delButton.Click += Del;
		_s.xRangeButton.Click += LockRangeX;
		_s.yRangeButton.Click += LockRangeY;
		_s.oyRangeButton.Click += LockRangeO;
		_s.tRangeButton.Click += LockRangeT;
		_s.lockResButton.Click += Res;
		_s.nextButton.Click += nextButton_Click;
		_s.prevButton.Click += prevButton_Click;
		_s.animatedBox.CheckedChanged += AniChanged;
		_s.outputSelect.SelectedIndexChanged += SelectOutput;
		_s.clipSelect.SelectedIndexChanged += SelectClip;
		_s.buildButton.Click += ClickBuild;
		_s.saveButton.Click += ClickSave;
		_s.loadButton.Click += ClickLoad;
		Init();
		parent.SetMinSize();
		parent.Text = "Comparser - Plotter";
	}
	private void Init() {
		bool oldState = _myStates.ContainsKey(SettingsControl.Context!);
		var s = oldState ? _myStates[SettingsControl.Context!] : _myStates[SettingsControl.Context!] = new();
		s.Suppressed = true;
		_s.modeSelect.SelectedIndex = 0;
		//ModeSelected(_s.modeSelect, EventArgs.Empty);
		TlChanged(_s.itlBox, EventArgs.Empty);
		TfChanged(_s.itfBox, EventArgs.Empty);
		ModeSelected(null, EventArgs.Empty);
		FyChanged(_s.fyBox, EventArgs.Empty);
		SelectOutput(_s.outputSelect, EventArgs.Empty);
		PeekRange(_s.xRangeButton, _rangeX);
		PeekRange(_s.yRangeButton, _rangeY);
		PeekRange(_s.oyRangeButton, _rangeO);
		PeekRange(_s.tRangeButton, _rangeT);
		GetPlot()?.LockRangeX(_rangeX);
		GetPlot()?.LockRangeY(_rangeY);
		GetPlot()?.LockRangeO(_rangeO);
		GetPlot()?.LockRangeT(_rangeT);
		Refresh(_tf, _frame.ToString());
		Refresh(_tl, _length.ToString());
		Refresh(_fy, _fixedY.ToString(CultureInfo.InvariantCulture));
		//Refresh(tl, _length.ToString());
		s.Suppressed = false;
		if (oldState)
			return;
		_inputX?.States = s;
		_inputY?.States = s;
		_inputT?.States = s;
		_outputY?.States = s;
		_inputX?.Changed += (_, _) => DirtyImage();
		_inputY?.Changed += (_, _) => DirtyImage();
		_inputT?.Changed += (_, _) => DirtyImage();
		_outputY?.Changed += (_, _) => DirtyImage();
		_ = new LogSce(_inputX!, s); // input X / DONE
		_ = new LogSce(_inputY!, s); // 2d input Y / DONE
		_ = new LogSce(_inputT!, s); // input time / DONE
		_ = new LogSce(_outputY!, s); // 1D output Y / DONE
		_ = new LogCombo(_s.modeSelect, s); // plot mode / DONE
		_ = new LogText(_s.fyBox/*fy*/, s); // fixedY / DONE
		_ = new LogPlotSize(_s.widthBox, _s.heightBox, _inputX!, _inputY!, _outputY!, s); // width x height
		_ = new LogOutput(_s, this, s); // output codes / DONE
		_ = new LogLock(_s.tRangeButton, s); // lock input time / DONE
		_ = new LogLock(_s.xRangeButton, s); // lock input X / DONe
		_ = new LogLock(_s.yRangeButton, s); // lock 2D input Y / DONE
		_ = new LogLock(_s.oyRangeButton, s); // lock 1D output Y / DONE
		_ = new LogLock(_s.lockResButton, s); // resolution lock / DONE
		_ = new LogAni(_s.itlBox, _s.itfBox, _inputT!, _s.animatedBox, s); // TODO frame length and index + animate (animate should not update index during animation, only the animated flag)
	}
	public void SetContext() {
		if (Root == null || SettingsControl.Context is not { } context)
			return;
		var p = GetPlot();
		if (p is null)
			return;
		_inputX = new(context, _s.ixsBox, _s.ixcBox, _s.ixeBox, _s.ixsButton, _s.ixcButton, _s.ixeButton, p.GetAxis()[0], _inputX);
		_inputY = new(context, _s.iysBox, _s.iycBox, _s.iyeBox, _s.iysButton, _s.iycButton, _s.iyeButton, p.GetAxis()[1], _inputY);
		_inputT = new(context, _s.itsBox, _s.itcBox, _s.iteBox, _s.itsButton, _s.itcButton, _s.iteButton, p.GetAxis()[2], _inputT);
		_outputY = new(context, _s.oysBox, _s.oycBox, _s.oyeBox, _s.oysButton, _s.oycButton, _s.oyeButton, p.GetAxis()[3], _outputY);
		_s.outputSelect.Items.Clear();
		_s.outputSelect.Items.AddRange(GetPlot()?.GetOutputs() ?? []);
		Init();
		ReEval(/*p*/);
	}
	public void ReEval(/*IPlot p*/) {
		for (var i = 0; i < _outputs.Count; ++i)
			GetPlot()?.SetCode(i, Parse(_outputs[i].Code));
		//p.SetDirty();
		DirtyImage();
	}
	#endregion

	#region Actions
	private void ClickBuild(object? sender, EventArgs e) => UpdatePlot(true);
	private void ClickSave(object? sender, EventArgs e) {
		MessageBox.Show("This is not yet implemented, will be very soon.", "NOT IMPLEMENTED YET");
	}
	private void ClickLoad(object? sender, EventArgs e) {
		MessageBox.Show("This is not yet implemented, will be very soon.", "NOT IMPLEMENTED YET");
	}
	#endregion

	#region Locks
	private void LockRangeX(object? sender, EventArgs e) { PeekRange(_s.xRangeButton, _rangeX = !_rangeX); GetPlot()?.LockRangeX(_rangeX); LogState(_s.xRangeButton); }
	private void LockRangeY(object? sender, EventArgs e) { PeekRange(_s.yRangeButton, _rangeY = !_rangeY); GetPlot()?.LockRangeY(_rangeY); LogState(_s.yRangeButton); }
	private void LockRangeO(object? sender, EventArgs e) { PeekRange(_s.oyRangeButton, _rangeO = !_rangeO); GetPlot()?.LockRangeO(_rangeO); LogState(_s.oyRangeButton); }
	private void LockRangeT(object? sender, EventArgs e) { PeekRange(_s.tRangeButton, _rangeT = !_rangeT); GetPlot()?.LockRangeT(_rangeT); LogState(_s.tRangeButton); }
	private void PeekRange(Button rb, bool l) {
		SetLock(SettingsControl.Context, rb, l);
		/*for (int ia = 0; ia < a.Length; ++ia) {
			var ib = new Button?[3] { a[ia]?.Ls, a[ia]?.Lc, a[ia]?.Le };
			for (int i = 0; i < 3; ++i) if (ib[i] is Button b){
				b.Enabled = !_rangeY;
				b.BackColor = _rangeY || b.Text == Static.LockedSymbol ? Color.Red : GetContext()?.GetColor().b ?? Color.Black;
			}
		}*/
	}
	private void Res(object? sender, EventArgs e) {
		SetLock(SettingsControl.Context, _s.lockResButton, _lockedRes = !_lockedRes);
		GetPlot()?.SetLockRes(_lockedRes);
		Resized(null, EventArgs.Empty);
		LogState(_s.lockResButton);
	}
	private static void SetLock(IComparser? c, Button b, bool locked) {
		b.Text = locked ? Static.LockedSymbol : Static.UnlockedSymbol;
		b.BackColor = locked ? Color.Red : c?.GetColor().b ?? Color.Black;
	}
	#endregion

	#region Outputs
	public void Add(object? sender, EventArgs e) {
		if (_s.outputSelect.Text == "") {
			Err();
			return;
		}
		foreach (var o in _outputs) if (o.Name == _s.outputSelect.Text) { Err(); return; }
		_outputs.Add(new(_s.outputSelect.Text, _s.rgbBox, _s.codeBox, RgbChanged, CodeChanged));
		GetPlot()?.AddOutput(_s.outputSelect.Text, null, null);
		var c = _s.outputSelect.Items.Count;
		_s.outputSelect.Items.Add(_s.outputSelect.Text);
		_s.outputSelect.SelectedIndex = c;//SelectOutput(null, EventArgs.Empty);
		//_s.clipSelect.SelectedIndex = 0;
		LogState(_s.outputSelect, (byte)OutputAction.Add);
		DirtyImage();
		return;
		static void Err() => MessageBox.Show("You must write a unique name to the combo box to the right before adding.", "Error: No name.");
	}
	public void Del(object? sender, EventArgs e) {
		if (_s.outputSelect.Text == "") {
			Err();
			return;
		}
		bool notfound = true;
		foreach (var o in _outputs) if (o.Name == _s.outputSelect.Text) { notfound = false; break;}
		if (notfound) {
			Err();
			return;
		}
		_outputs.RemoveAt(_s.outputSelect.SelectedIndex);
		SelectOutput(null, EventArgs.Empty);
		LogState(_s.outputSelect, (byte)OutputAction.Remove);
		DirtyImage();
		return;
		static void Err() => MessageBox.Show("You must have some output selected to delete one.", "Error: No name.");
	}
	private void SelectOutput(object? sender, EventArgs e) {
		var i = _s.outputSelect.SelectedIndex;
		if (_outputs.Count <= i || i < 0) {
			_s.rgbBox.ReadOnly = _s.codeBox.ReadOnly = true;
			return;
		}
		var o = _outputs[i];
		o.Rgb.Box.Text = o.Rgb.Text;
		o.Code.Box.Text = o.Code.Text;
		_s.clipSelect.SelectedIndex = o.Clip;
		_s.rgbBox.ReadOnly = _s.codeBox.ReadOnly = false;
		LogState(_s.outputSelect);
	}
	private void SelectClip(object? sender, EventArgs e) {
		var i = _s.outputSelect.SelectedIndex;
		if (_outputs.Count <= i || i < 0)
			return;
		GetPlot()?.SetClip(i, _outputs[i].Clip = _s.clipSelect.SelectedIndex);
		LogState(_s.clipSelect);
	}
	public void CodeChanged(object? sender, EventArgs e) {
		var i = _s.outputSelect.SelectedIndex;
		if (i < 0 || i >= _outputs.Count)
			return;
		GetPlot()?.SetCode(i, Parse(_outputs[i].Code));
		LogState(_s.codeBox);
		DirtyImage();
	}
	public void RgbChanged(object? sender, EventArgs e) {
		var i = _s.outputSelect.SelectedIndex;
		if (i < 0 || i >= _outputs.Count)
			return;
		GetPlot()?.SetRgb(i, Parse(_outputs[i].Rgb));
		//GetPlot()?.SetDirty();
		LogState(_s.rgbBox);
		DirtyImage();
	}
	#endregion

	#region Ranges
	//private int prevW = 0;
	/*private void ResizedForm(object? sender, EventArgs e) {
		splitContainer.FixedPanel = (splitContainer.Panel1.Width < 320) == (prevW < Width) ? FixedPanel.Panel2 : FixedPanel.Panel1;
		prevW = Width;
	}*/
	private void Resized(object? sense, EventArgs e) {
		if (_lockedRes)
			return;
		if (!_w.Box.Enabled)
			return;
		if (!_h.Box.Enabled)
			return;
		Refresh(_w, splitContainer.Panel2.Width.ToString());
		Refresh(_h, splitContainer.Panel2.Height.ToString());
		if (GetPlot() is not { } p)
			return;
		p.Resize(splitContainer.Panel2.Width, splitContainer.Panel2.Height, _length);
		if (_inputX == null || _inputY == null /*|| InputT == null*/ || _outputY == null)
			return;
		RefreshSce(p, _inputX, 0);
		RefreshSce(p, _inputY, 1);
		//RefreshSce(p, InputT, 2);
		RefreshSce(p, _outputY, 3);
		LogState(_s.widthBox); // any of them will log the whole state
		DirtyImage();
	}
	private void RefreshSce(IPlot p, AxisControls? a, int i) {
		if (a == null)
			return;
		var (s, c, e, _) = p.GetAxis()[i].GetSce();
		Refresh(a.S, s);
		Refresh(a.C, c);
		Refresh(a.E, e);
	}
	private void Refresh(TextField f, string s) {
		//var ro = !(f.Box?.Enabled ?? true); 
		f.Box.Enabled = false;
		//var s = splitContainer.Panel2.Height.ToString();
		if (f.Box.Text != s) f.Box.Text = s;
		Parse(f);
		f.Box.Enabled = true;
		//f.Box?.ReadOnly = ro;
	}
	private void WidthChanged(object? sender, EventArgs e) {
		if (_w.Box.Enabled)
			return;
		var s = (int)(SettingsControl.Context?.AsDouble(Eval(_w)) ?? plotBox.Width) - plotBox.Width;
		// resize the panel2, and do not make it retrigger this same textbox
		_w.Box.Enabled = false;
		splitContainer.FixedPanel = FixedPanel.Panel1;
		Width += s;
		splitContainer.FixedPanel = FixedPanel.Panel2;
		_w.Box.Enabled = true;
		LogState(_s.widthBox);
		DirtyImage();
	}
	private void HeightChanged(object? sender, EventArgs e) {
		if (_h.Box.Enabled)
			return;
		var s = (int)(SettingsControl.Context?.AsDouble(Eval(_h)) ?? plotBox.Height) - plotBox.Height;
		_h.Box.Enabled = false;
		splitContainer.FixedPanel = FixedPanel.Panel1;
		Height += s;
		splitContainer.FixedPanel = FixedPanel.Panel2;
		_h.Box.Enabled = true;
		LogState(_s.heightBox);
		DirtyImage();
	}
	private void ModeSelected(object? sender, EventArgs e) { Set2D(_s, GetPlot(), (PlotMode)_s.modeSelect.SelectedIndex); LogState(_s.modeSelect); DirtyImage(); }
	private static void Set2D(PlotSettings s, IPlot? p, PlotMode mode) {
		p?.ChangeMode(mode);
		s.fyBox.ReadOnly = mode == PlotMode.Xy;
		//bool enabled = mode == PlotMode.Xy;
		//s.oysLabel.Visible = s.oysButton.Visible = s.oysBox.Visible = s.oycLabel.Visible = s.oycButton.Visible = s.oycBox.Visible = s.oyeLabel.Visible = s.oyeButton.Visible = s.oyeBox.Visible = !enabled;
		//s.fyBox.ReadOnly = s.iysLabel.Visible = s.iysButton.Visible = s.iysBox.Visible = s.iycLabel.Visible = s.iycButton.Visible = s.iycBox.Visible = s.iyeLabel.Visible = s.iyeButton.Visible = s.iyeBox.Visible = enabled;
	}
	private void FyChanged(object? sender, EventArgs e) {
		(_fixedY, var v) = GetPlot()?.SetFixedY(Eval(_fy)) ?? (0, "?");
		_s.fyLabel.Text = "Y: " + v;
		LogState(_s.fyBox);
		DirtyImage();
	}
	#endregion

	#region Time
	private void TfChanged(object? sender, EventArgs e) {
		GetPlot()?.SetFrame(Math.Min(_length - 1, _frame = (int)(SettingsControl.Context?.AsDouble(Eval(_tf)) ?? 0)));
		if (_s.animatedBox.Checked)
			return; // do not log undo for automatic animation frame advances
		LogState(_s.itfBox);
		DirtyImage();
	}
	private void TlChanged(object? sender, EventArgs e) {
		/*InputT?.SetLength(*/
		_length = Math.Max(1, (int)(SettingsControl.Context?.AsDouble(Eval(_tl)) ?? 1))/*)*/;
		if (GetPlot() is not { } p || _inputT == null)
			return;
		p.Resize(splitContainer.Panel2.Width, splitContainer.Panel2.Height, _length);//UpdatePlot();
		RefreshSce(p, _inputT, 2);
		TfChanged(null, EventArgs.Empty);
		LogState(_s.itlBox);
		DirtyImage();
	}
	private void AniChanged(object? sender, EventArgs e) => LogState(_s.animatedBox);
	private void Fps_Tick(object? sender, EventArgs e) {
		if (_s.animatedBox.Checked && _finishedImage > 0)
			nextButton_Click(_s.nextButton, EventArgs.Empty);
		UpdatePlot();
	}
	private void UpdatePlot(bool forced = false) {
		if (_finishedImage == 1) {
			_s.Unblock();
			_finishedImage = 2;
			if (bmp != null && plotBox.Image != bmp)
				plotBox.Image = bmp;
			return;
		}
		if (!(_draw?.IsCompleted ?? true))
			return;
		// draw blocks/delays
		if (SettingsControl.AutoPlot || forced) {
			if (!(forced || _s.animatedBox.Checked)) {
				if (plotDelay.IsRunning) {
					if (plotDelay.ElapsedMilliseconds < SettingsControl.PlotDelay)
						return;
					plotDelay.Stop();
				} else plotDelay.Restart();
			}
		} else return;
		_s.Block();
		_draw = Task.Run(() => UpdatePlotAsync(splitContainer.Panel2.Width, splitContainer.Panel2.Height));

		//plotBox.Size = plotBox.Image.Size;
		//_dirty = false;
	}
	private void UpdatePlotAsync(int w, int h) {
		if (GetPlot() is { } p) { bmp = p.Update(w, h, _length); }
		_finishedImage = 1;
		_draw = null;
	}
	private void DirtyImage() {
		_finishedImage = 0;
		_s.buildButton.Text = "PLOT";
	}
	private void prevButton_Click(object? sender, EventArgs e) {
		_tf.Box.Text = ((_length - 1 + _frame) % _length).ToString();
		//var l = (int)(GetContext()?.AsDouble(tl.Value) ?? 1);
		//tf.Text = ((l - 1 + (int)(GetContext()?.AsDouble(Eval(tf)) ?? 0)) % l).ToString();
	}
	private void nextButton_Click(object? sender, EventArgs e)
		=> _tf.Box.Text = ((1 + _frame) % _length).ToString();//tf.Text = ((1 + (int)(GetContext()?.AsDouble(Eval(tf)) ?? 0)) % (int)(GetContext()?.AsDouble(tl.Value) ?? 1)).ToString();
	#endregion

	#region Getters
	public override Size GetSize() => new(120, Pad + 4 * (Pad + RowHeight));
	private IPlot? GetPlot() => SettingsControl.Context?.GetPlot();
	//private IComparser? GetContext() => SettingsControl.Context;
	#endregion

	#region LogState
	private void LogState(Control c, byte action = 0) => _myStates[SettingsControl.Context!].Log(c, action);
	override protected bool ProcessCmdKey(ref Message msg, Keys k) {
		if(SettingsControl.Context is not { } c)
			return base.ProcessCmdKey(ref msg, k);
		if (k == (Keys.Control | Keys.Z)) {
			_myStates[c].Undo();
			return true;
		}
		if (k == (Keys.Control | Keys.Y)) {
			_myStates[c].Redo();
			return true;
		}
		return base.ProcessCmdKey(ref msg, k);
	}
	private readonly Dictionary<IComparser, States> _myStates = [];
	#endregion
}