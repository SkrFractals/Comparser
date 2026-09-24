using Comparser.Comparser;
using Comparser.Comparser.Numbers;
using Comparser.Forms.Controls;
using Comparser.Forms.Core;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using static Comparser.Forms.Core.IPanel;
namespace Comparser.Forms;

public partial class PlotPanel : UserControl, IPanel {
	#region IPanel
	private readonly ControlVar _var;
	public ControlVar GetVar() => _var;
	public Size GetSize() => new(0, 0);
	public void SetDark(bool dark) => BaseSetDark(this);
	public void PerformClose() { }
	public void CoreLayout() { }
	#endregion

	#region Structures
	public record SceState(string s, string c, string e, int l);
	public class Output(string name, RichTextBox rgb, RichTextBox code, EventHandler rgbChanged, EventHandler codeChanged) {
		public readonly TextField
			Rgb = new(rgb, rgbChanged, "log2rgbc(v)", ["v", "c", "z", "t", "x", "y", "f", "w", "h", "l"]),
			Code = new(code, codeChanged, "z!", ["z", "t"]);
		public readonly string Name = name;
		public int Clip;
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
			Ls = ls; ls.Click += (_, _) => Lock(0);
			_lc = lc; lc.Click += (_, _) => Lock(1);
			_le = le; le.Click += (_, _) => Lock(2);
			S.Box.Tag = C.Box.Tag = E.Box.Tag = true;
			(S.Box.Text, C.Box.Text, E.Box.Text, _locked) = _axis.GetSce();
			S.Box.Tag = C.Box.Tag = E.Box.Tag = false;
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
				if (_locked switch { 0 => Ls, 1 => _lc, 2 => _le, _ => null } is { } b)
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
			S.Box.Tag = C.Box.Tag = E.Box.Tag = true;
			(S.Box.Text, C.Box.Text, E.Box.Text, var wasLocked) = sce;
			if (changeAxis) {
				_axis.SetSce(Eval(_context, S), Eval(_context, E));
				Lock(-1); //remove previous lock (so that the next call will 100% succeed in locking a specific lock, instead of toggling it off)
				Lock(wasLocked);
			}
			S.Box.Tag = C.Box.Tag = E.Box.Tag = false;
			Changed?.Invoke(this, EventArgs.Empty);
			//S.Box.ReadOnly = s; C.Box.ReadOnly = c; E.Box.ReadOnly = e;
		}
		private void Change(TextField change, TextField first, TextField second, Func<object?, (string, string)> del) {
			if (/*change.Box is null ||*/ change.Box.Tag is true /*|| first.Box is null || second.Box is null*/)
				return;
			//var (fir, sec) = (first.Box.ReadOnly, second.Box.ReadOnly);
			first.Box.Tag = second.Box.Tag = true;
			(first.Box.Text, second.Box.Text) = del(Eval(_context, change));
			Parse(first, false);
			Parse(second, false);
			first.Box.Tag = second.Box.Tag = false;
			//first.Box.ReadOnly = fir; second.Box.ReadOnly = sec;
			States?.Log(Ls); // any of them will work
			Changed?.Invoke(this, EventArgs.Empty);
			States?.Log(change.Box); // any of them will work
		}
		public void SetLength(int l) => SetSce(_axis.SetLength(l));
	}
	#endregion

	#region Inits
	public PlotPanel() {
		InitializeComponent();
		_s = new();
	}
	public PlotPanel(MenuPanel root, ParentForm parent) : this() {
		InitVar(ref _var, this, root, parent, "Comparser - Plotter");
		_s = _var.Root.PlotSet!.S;
		if (SettingsPanel.Context is not { } context || GetPlot() is not { } p)
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
		_s.saveMp4.Click += ClickMp4;
		_s.saveButton.Click += ClickSave;
		_s.loadButton.Click += ClickLoad;
		_s.fysButton.Click += FysClick;
		_s.fycButton.Click += FycClick;
		_s.fyeButton.Click += FyeClick;
		_var.Form.FormBorderStyle = FormBorderStyle.Sizable;
		Init();
		parent.SetMinSize();
		parent.Text = "Comparser - Plotter";
	}
	private void Init() {
		if (GetPlot() is { } p)
			p.SetFinished(OnFinished);
		bool oldState = _myStates.ContainsKey(SettingsPanel.Context);
		var s = oldState ? _myStates[SettingsPanel.Context] : _myStates[SettingsPanel.Context] = new();
		var sup = States.Suppressed;
		States.Suppressed = true;
		_s.modeSelect.SelectedIndex = 2;
		//ModeSelected(_s.modeSelect, EventArgs.Empty);
		Resized(null, EventArgs.Empty);
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
		States.Suppressed = sup;
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
		_inputY?.Changed += FyChanged;

		// blocks scrolling over them from changing their value
		_s.modeSelect.MouseWheel += SettingsPanel.ComboBox_MouseWheel;
		_s.outputSelect.MouseWheel += SettingsPanel.ComboBox_MouseWheel;

		
		
		//_ = new LogSce(_inputX!, s); // input X / DONE
		//_ = new LogSce(_inputY!, s); // 2d input Y / DONE
		//_ = new LogSce(_inputT!, s); // input time / DONE
		//_ = new LogSce(_outputY!, s); // 1D output Y / DONE
		_ = new LogCombo(_s.modeSelect, s); // plot mode / DONE
		_ = new LogText(_s.fyBox/*fy*/, s); // fixedY / DONE
		_ = new LogPlotSize(_s.widthBox, _s.heightBox, _inputX!, _inputY!, _outputY!, s); // width x height
		_ = new LogOutput(_s, this, s); // output codes / DONE
		_ = new LogLock(_s.tRangeButton, s); // lock input time / DONE
		_ = new LogLock(_s.xRangeButton, s); // lock input X / DONe
		_ = new LogLock(_s.yRangeButton, s); // lock 2D input Y / DONE
		_ = new LogLock(_s.oyRangeButton, s); // lock 1D output Y / DONE
		_ = new LogLock(_s.lockResButton, s); // resolution lock / DONE
		_ = new LogAni(_s.itlBox, _s.itfBox, _inputT!, _s.animatedBox, s);
		
		Add(null, EventArgs.Empty);
	}
	public void SetContext() {
		if (SettingsPanel.Context is not { } context || GetPlot() is not { } p)
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
		var sup = States.Suppressed;
		States.Suppressed = true;
		_cancel.Cancel();
		for (var i = 0; i < Outputs.Count; ++i) {
			GetPlot()?.SetCode(i, Parse(Outputs[i].Code, false));
			GetPlot()?.SetRgb(i, Parse(Outputs[i].Rgb, false));
		}
		//p.SetDirty();
		DirtyImage();
		States.Suppressed = sup;
	}
	private void PlotClick(object? sense, EventArgs e) { }
	#endregion

	#region Variables
	private readonly PlotSettingsControl _s;
	private readonly TextField _fy = new(), _w = new(), _h = new(), _tf = new(), _tl = new();
	public readonly List<Output> Outputs = [];
	private AxisControls? _inputX, _inputY, _inputT, _outputY;
	private bool _rangeX = true, _rangeY = true, _rangeO = true, _rangeT = true, _lockedRes;
	//private Task? _draw;
	private Bitmap? _bmp;
	private readonly Stopwatch _plotDelay = new();
	//private volatile bool _finishedImage;
	private bool _dirtyImage;
	private int _length = 1, _frame;
	private double _fixedY;
	#endregion

	#region Actions
	private void ClickBuild(object? sender, EventArgs e) {
		if (/*_dirtyImage &&*/ _drawing || _div > 0) {
			_s.buildButton.Text = "PLOT" ;
			_cancel.Cancel();
			_div = 0;
			_finished = _drawing = false;
			_cancelled = _dirtyImage = true;
			return;
		}
		
		UpdatePlot(true);
	}
	private void ClickMp4(object? sender, EventArgs e) => saveMp4.ShowDialog();
	private void ClickSave(object? sender, EventArgs e) => savePlot.ShowDialog();
	private void ClickLoad(object? sender, EventArgs e) => openPlot.ShowDialog();
	#endregion

	#region Locks
	private void LockRangeX(object? sender, EventArgs e) { PeekRange(_s.xRangeButton, _rangeX = !_rangeX); GetPlot()?.LockRangeX(_rangeX); LogState(_s.xRangeButton); }
	private void LockRangeY(object? sender, EventArgs e) { PeekRange(_s.yRangeButton, _rangeY = !_rangeY); GetPlot()?.LockRangeY(_rangeY); LogState(_s.yRangeButton); }
	private void LockRangeO(object? sender, EventArgs e) { PeekRange(_s.oyRangeButton, _rangeO = !_rangeO); GetPlot()?.LockRangeO(_rangeO); LogState(_s.oyRangeButton); }
	private void LockRangeT(object? sender, EventArgs e) { PeekRange(_s.tRangeButton, _rangeT = !_rangeT); GetPlot()?.LockRangeT(_rangeT); LogState(_s.tRangeButton); }
	private void PeekRange(Button rb, bool l) {
		SetLock(SettingsPanel.Context, rb, l);
		/*for (int ia = 0; ia < a.Length; ++ia) {
			var ib = new Button?[3] { a[ia]?.Ls, a[ia]?.Lc, a[ia]?.Le };
			for (int i = 0; i < 3; ++i) if (ib[i] is Button b){
				b.Enabled = !_rangeY;
				b.BackColor = _rangeY || b.Text == Static.LockedSymbol ? Color.Red : GetContext()?.GetColor().b ?? Color.Black;
			}
		}*/
	}
	private void Res(object? sender, EventArgs e) {
		SetLock(SettingsPanel.Context, _s.lockResButton, _lockedRes = !_lockedRes);
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
		foreach (var o in Outputs) if (o.Name == _s.outputSelect.Text) { Err(); return; }
		var sup = States.Suppressed;
		States.Suppressed = true;
		Outputs.Add(new(_s.outputSelect.Text, _s.rgbBox, _s.codeBox, RgbChanged, CodeChanged));
		GetPlot()?.AddOutput(_s.outputSelect.Text, null, null);
		var c = _s.outputSelect.Items.Count;
		_s.outputSelect.Items.Add(_s.outputSelect.Text);
		_s.outputSelect.SelectedIndex = c;
		States.Suppressed = sup;
		LogState(_s.outputSelect, (byte)OutputAction.Add);
		DirtyImage();
		return;
		static void Err() => MessageBox.Show("You must write a unique name to the combo box to the right before adding.", "Error: No name.");
	}
	public void DelAll() {
		_s.outputSelect.Items.Clear();
		Outputs.Clear();
		if (GetPlot() is not { } p)
			return;
		p.DelAll();
	}

	public void Del(object? sender, EventArgs e) {
		if (_s.outputSelect.Text == "") {
			Err();
			return;
		}
		bool notfound = true;
		foreach (var o in Outputs) if (o.Name == _s.outputSelect.Text) { notfound = false; break; }
		if (notfound) {
			Err();
			return;
		}
		var sup = States.Suppressed;
		States.Suppressed = true;
		var at = _s.outputSelect.SelectedIndex;
		Outputs.RemoveAt(at);
		RebuildOutputSelect();
		SelectOutput(null, EventArgs.Empty);
		States.Suppressed = sup;
		LogState(_s.outputSelect, (byte)OutputAction.Remove);
		DirtyImage();
		if (GetPlot() is not { } p)
			return;
		p.DelOutput(at);
		return;
		static void Err() => MessageBox.Show("You must have some output selected to delete one.", "Error: No name.");
	}
	private void RebuildOutputSelect() {
		_s.outputSelect.Items.Clear();
		foreach (var o in Outputs)
			_s.outputSelect.Items.Add(o.Name);
	}
	private void SelectOutput(object? sender, EventArgs e) {
		var i = _s.outputSelect.SelectedIndex;
		if (Outputs.Count <= i || i < 0) {
			_s.rgbBox.ReadOnly = _s.codeBox.ReadOnly = true;
			return;
		}
		var o = Outputs[i];
		o.Rgb.Box.Text = o.Rgb.Text;
		o.Code.Box.Text = o.Code.Text;
		_s.clipSelect.SelectedIndex = o.Clip;
		_s.rgbBox.ReadOnly = _s.codeBox.ReadOnly = false;
		LogState(_s.outputSelect);
	}
	private void SelectClip(object? sender, EventArgs e) {
		var i = _s.outputSelect.SelectedIndex;
		if (Outputs.Count <= i || i < 0)
			return;
		GetPlot()?.SetClip(i, Outputs[i].Clip = _s.clipSelect.SelectedIndex);
		LogState(_s.clipSelect);
	}
	public void CodeChanged(object? sender, EventArgs e) {
		var i = _s.outputSelect.SelectedIndex;
		if (i < 0 || i >= Outputs.Count)
			return;
		_cancel.Cancel();
		GetPlot()?.SetCode(i, Parse(Outputs[i].Code));
		LogState(_s.codeBox);
		DirtyImage();
	}
	public void RgbChanged(object? sender, EventArgs e) {
		var i = _s.outputSelect.SelectedIndex;
		if (i < 0 || i >= Outputs.Count)
			return;
		_cancel.Cancel();
		GetPlot()?.SetRgb(i, Parse(Outputs[i].Rgb));
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
		if (_lockedRes || _s.widthBox.Tag is true || _s.heightBox.Tag is true)
			return;
		plotBox.Dock = DockStyle.Fill;
		Refresh(_w, plotBox.Width.ToString());
		Refresh(_h, plotBox.Height.ToString());
		RefreshAxes();
		LogState(_s.widthBox); // any of them will log the whole state
	}
	private void RefreshAxes() {
		if (GetPlot() is not { } p)
			return;
		p.Resize(plotBox.Width, plotBox.Height, _length);
		if (_inputX == null || _inputY == null || _outputY == null)
			return;
		RefreshSce(p, _inputX, 0);
		RefreshSce(p, _inputY, 1);
		//RefreshSce(p, InputT, 2);
		RefreshSce(p, _outputY, 3);
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
		f.Box.Tag = true;
		if (f.Box.Text != s) f.Box.Text = s;
		Parse(f, false);
		f.Box.Tag = false;
	}
	private void WidthChanged(object? sender, EventArgs e) {
		if (_s.widthBox.Tag is true)
			return;
		_s.widthBox.Tag = true;
		int extra = _var.Form.Width - _var.Form.GetInnerPanel().Width,
			desired = (int)(SettingsPanel.Context.AsDouble(Eval(SettingsPanel.Context, _w)));
		plotBox.Dock = DockStyle.Fill;
		_var.Form.Width = extra + desired;
		if (plotBox.Width != desired) {
			plotBox.Dock = DockStyle.None;
			plotBox.Width = desired;
		}
		_s.widthBox.Tag = false;
		LogState(_s.widthBox);
		DirtyImage();
	}
	private void HeightChanged(object? sender, EventArgs e) {
		if (_s.heightBox.Tag is true)
			return;
		_s.heightBox.Tag = true;
		int extra = _var.Form.Height - _var.Form.GetInnerPanel().Height,
			desired = (int)(SettingsPanel.Context.AsDouble(Eval(SettingsPanel.Context, _h)));
		plotBox.Dock = DockStyle.Fill;
		_var.Form.Height = extra + desired;
		if (plotBox.Height != desired) {
			plotBox.Dock = DockStyle.None;
			plotBox.Height = desired;
		}
		_s.heightBox.Tag = false;
		LogState(_s.heightBox);
		DirtyImage();
	}
	private void ModeSelected(object? sender, EventArgs e) { Set2D(_s, GetPlot(), (PlotMode)_s.modeSelect.SelectedIndex); LogState(_s.modeSelect); DirtyImage(); }
	private static void Set2D(PlotSettingsControl s, IPlot? p, PlotMode mode) {
		p?.ChangeMode(mode);
		s.fyBox.ReadOnly = mode == PlotMode.Xy;
		//bool enabled = mode == PlotMode.Xy;
		//s.oysLabel.Visible = s.oysButton.Visible = s.oysBox.Visible = s.oycLabel.Visible = s.oycButton.Visible = s.oycBox.Visible = s.oyeLabel.Visible = s.oyeButton.Visible = s.oyeBox.Visible = !enabled;
		//s.fyBox.ReadOnly = s.iysLabel.Visible = s.iysButton.Visible = s.iysBox.Visible = s.iycLabel.Visible = s.iycButton.Visible = s.iycBox.Visible = s.iyeLabel.Visible = s.iyeButton.Visible = s.iyeBox.Visible = enabled;
	}
	private void FyChanged(object? sender, EventArgs e) => ChangeFy(Eval(this, _fy));
	private void FysClick(object? sender, EventArgs e) {
		ChangeFy(/*_inputY?.S.Value*/0.0);
		if (GetPlot() is not { } p)
			return;
		var (s, _, _, _) = p.GetAxis()[1].GetSce();
		Refresh(_fy, s);
	}
	private void FycClick(object? sender, EventArgs e) {
		ChangeFy( /*_inputY?.C.Value*/plotBox.Height / 2.0);
		if (GetPlot() is not { } p)
			return;
		var (_, c, _, _) = p.GetAxis()[1].GetSce();
		Refresh(_fy, c);
	}
	private void FyeClick(object? sender, EventArgs e) {
		ChangeFy( plotBox.Height);
		if (GetPlot() is not { } p)
			return;
		var (_, _, end, _) = p.GetAxis()[1].GetSce();
		Refresh(_fy, end);
	}
	private void ChangeFy(object? e) {
		(_fixedY, var v) = GetPlot()?.SetFixedY(e) ?? (0, "?");
		_s.fyLabel.Text = "Y: " + v;
		LogState(_s.fyBox);
		DirtyImage();
	}

	#endregion

	#region Time
	private void TfChanged(object? sender, EventArgs e) {
		GetPlot()?.SetFrame(Math.Min(_length - 1, _frame = (int)(SettingsPanel.Context.AsDouble(Eval(this, _tf)))));
		DirtyImage();
		UpdatePlot(true);
		if (_s.animatedBox.Checked || _mp4Cancel != null)
			return; // do not log undo for automatic animation frame advances
		LogState(_s.itfBox);

	}
	private void TlChanged(object? sender, EventArgs e) {
		/*InputT?.SetLength(*/
		_length = Math.Max(1, (int)(SettingsPanel.Context.AsDouble(Eval(this, _tl))))/*)*/;
		if (GetPlot() is not { } p || _inputT == null)
			return;
		p.Resize(plotBox.Width, plotBox.Height, _length);//UpdatePlot();
		RefreshSce(p, _inputT, 2);
		TfChanged(null, EventArgs.Empty);
		LogState(_s.itlBox);
		DirtyImage();
	}
	private void AniChanged(object? sender, EventArgs e) => LogState(_s.animatedBox);
	private bool InPlace() {
		if (GetPlot() is { } p)
			return p.InPlace(_renderAxes);
		return true;
	}
	private void OnFinished(object? x, object? y, Comparser.Comparser.Plot.BitmapReady bmp, int outDiv, CancellationToken renderToken/*, string message = ""*/) {
		if (IsDisposed || Disposing || !IsHandleCreated) {
			OnFinUi(x, y, bmp, outDiv, renderToken); 
			return; 
		}
		BeginInvoke((MethodInvoker)(() => OnFinUi(x, y, bmp, outDiv, renderToken/*, message*/)));
	}
	//private int bmpCount = 0;
	private void OnFinUi(object? x, object? y, Comparser.Comparser.Plot.BitmapReady bmp, int outDiv, CancellationToken renderToken/*, string message = ""*/) {
		if (bmp.D != null && !renderToken.IsCancellationRequested /*&& renderToken == _cancel.Token*/) {
			//bmp.Save(@"C:\Temp\debug"+ bmpCount++ +"." + bmp.Width + "." + outDiv + "." + message + "+.bmp");
			_bmp = bmp.D;
			_renderAxes = (x, y);
			plotBox.Invalidate();
			plotBox.Update();
		}
		if((_div = outDiv) > 0)
			PerformUpdate();
		else {
			if(_exportBmps.Length < _length)
				_exportBmps = new Bitmap[_length];
			_exportBmps[_frame] = bmp.D;
			bmp.F = 2;
			_finished = true;
			_drawing = false;
			_s.buildButton.Text = "OK";
		}
		//Console.WriteLine("FinUi");
	}
	private (object?, object?) _renderAxes;
	private bool _drawing, _forced, _finished;
	private void UpdatePlot(bool forced = false) {
		/*if (_finishedImage) {
			//if(_div <= 0)
			//	_s.Unblock();
			_finishedImage = false;
			//_plotLocation = _renderLocation;
			if (_loadBmp == _bmp)
				return;
			(_bmp, _loadBmp) = (_loadBmp, _bmp);
			plotBox.Invalidate();
			//if (bmp != null)
			//    plotBox.Image = bmp;
			return;
		}*/
		if (forced)
			_forced = true;
		if (_drawing)
			return;
		// draw blocks/delays
		if (!_cancelled && (SettingsPanel.AutoPlot && _dirtyImage || _div > 0) || _forced) {

			if (!(_forced || _s.animatedBox.Checked || _mp4Cancel != null || _div > 0) && InPlace()) {
				if (_plotDelay.IsRunning) {
					if (_plotDelay.ElapsedMilliseconds < SettingsPanel.PlotDelay/* && InPlace()*/)
						return;
					_plotDelay.Stop();
				} else _plotDelay.Restart();
			}
		} else return;
		PerformUpdate();
	}
	private void PerformUpdate() {
		//if (!inplace) Console.WriteLine("NotInplace");
		
		//_s.Block();
		//_newRenderLocation = (new(0, 0), new(plotBox.Width, plotBox.Height));
		//Stopwatch w = Stopwatch.StartNew();
		
		// TODO if the plot Location is entirely outside the view, then consider it non-animated
		bool mem = false;
		if (GetPlot() is { } p && (_drawing = p.Update(out mem, plotBox.Width, plotBox.Height, _length, SettingsPanel.PreviewLoad == 0 || _animated || _mp4Cancel != null || !InPlace() && SettingsPanel.UseMem, ref _cancel))) {
			_cancelled = _dirtyImage = _forced = false;
		}
		if (mem)
			return;
		_finished = false;
		_s.buildButton.Text = "CANCEL";
		//Console.WriteLine("PlotChange");
	}
	private int _div = 0;
	private CancellationTokenSource _cancel = new();
	private void DirtyImage(bool restartTimer = true) {
		if (restartTimer)
			_plotDelay.Restart();
		_dirtyImage = true;
		//_exportBmps = [];
		
		_finished = false;
		_s.buildButton.Text = _drawing ? "CANCEL": "PLOT" ;
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
	private IPlot? GetPlot() => SettingsPanel.Context.GetPlot();
	//private IComparser? GetContext() => SettingsControl.Context;
	#endregion

	#region LogState
	private void LogState(Control c, byte action = 0) => _myStates[SettingsPanel.Context].Log(c, action);
	
	public bool Undo() => _myStates[SettingsPanel.Context].Undo();
	public bool Redo() => _myStates[SettingsPanel.Context].Redo();
	private readonly Dictionary<IComparser, States> _myStates = [];
	#endregion

	private bool _animated, _cancelled;
	private void Fps_Tick(object? sender, EventArgs e) {
		/*if (_mp4Cancel != null && _frame != _encMp4) {
			if((_frame = _encMp4) < _length)
				_s.itfBox.Text = _frame.ToString();
			else {
				_s.itfBox.Text = (_frame = 0).ToString();
				_s.Unblock();
			}
		}*/
		if(_refreshAxes)
		{
			_refreshAxes = false;
			RefreshAxes();
		}
		//Console.WriteLine("Tick " + Static.Time.ElapsedMilliseconds);
		_s.percentLabel.Text = (GetPlot()?.GetPercent().ToString() ?? "0") + "%";
		if ((_animated = _s.animatedBox.Checked) && !_drawing && _div == 0 && _finished)
			nextButton_Click(_s.nextButton, EventArgs.Empty);
		UpdatePlot();
		//Console.WriteLine("TickEnd " + Static.Time.ElapsedMilliseconds);
	}
	private void PlotBox_Paint(object sender, PaintEventArgs e) {
		//Console.WriteLine("PlotBox_Paint " + Static.Time.ElapsedMilliseconds);
		//GetVar().Form.Text = "null" + counter++;
		if (_bmp == null)
			return;
		// Faster rendering with crisp pixels
		e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
		// some safety code to ensure no crashes
		byte attempt = 0;
		while (attempt < 10) {
			try {
				if (GetPlot() is not { } plot)
					continue;
				var (p, s) = plot.GetPlace(_renderAxes);
				//GetVar().Form.Text = p.ToString() + " " + counter++;
				e.Graphics.DrawImage(_bmp, new Rectangle(p.X, p.Y, s.Width, s.Height));
				attempt = 10;
			} catch (Exception) {
				++attempt;
				Thread.Sleep(10 + 10 * attempt * attempt);
			}
		}
		//Console.WriteLine("PlotBox_PaintEnd " + Static.Time.ElapsedMilliseconds);
	}
	private int _counter = 0;
	private bool _dragging, _dragged/*, dirtyDrag*/;
	private Point _lastCursor;
	//private (Point p, Size s) _plotLocation, _renderLocation, _newRenderLocation;
	private void plotBox_MouseDown(object sender, MouseEventArgs e) {
		_dragging = true;
		_lastCursor = e.Location;
		_dragged = false;
	}
	private void plotBox_MouseUp(object sender, MouseEventArgs e) {
		_dragging = false;
		if (_dragged)
			return;
		OpenSettings();
	}
	public void OpenSettings() => _var.Root.ShowC(_var.Root.PlotSetForm, _var.Form);
	private void plotBox_MouseMove(object sender, MouseEventArgs e) {
		if (!_dragging)
			return;
		var delta = _lastCursor;
		_dragged = true;
		_lastCursor = e.Location;
		delta = new(delta.X - _lastCursor.X, delta.Y - _lastCursor.Y);
		if (delta is { X: 0, Y: 0 })
			return;
		//Console.WriteLine("MoveCancel: " + delta.X + " " + delta.Y + " " +  Static.Time.ElapsedMilliseconds);
		GetPlot()?.SoftCancel(_cancel);//_cancel.Cancel(); // cancel if there was a running task rendering at old location, so it doesn't continue rendering more outdated frames
		DirtyImage(false); // make me want to start a render
		GetPlot()?.Shift(delta.X, delta.Y);
		plotBox.Invalidate(); // draw the image and the new shifted location
		plotBox.Update();
		_refreshAxes = true;
		
		
		//Console.WriteLine("Invalidate: " +  Static.Time.ElapsedMilliseconds);
		
	}
	private bool _refreshAxes = false;
	private void PlotBox_MouseWheel(object sender, MouseEventArgs e) {
		
		var delta = e.Delta;
		if (delta == 0)
			return;
		_dragged = true;
		GetPlot()?.SoftCancel(_cancel);// cancel if there was a running task rendering at old scale, so it doesn't continue rendering more outdated frames
		GetPlot()?.ZoomBinary(e.Location.X, e.Location.Y, delta > 0);
		
		//var centerX = (float)e.Location.X / plotBox.Width;
		//var centerY = (float)e.Location.Y / plotBox.Height;
		/*if (delta > 0) {
			_plotLocation = (new(_plotLocation.p.X * 2 - e.Location.X, _plotLocation.p.Y * 2 - e.Location.Y), _plotLocation.s * 2);
			_renderLocation = (new(_renderLocation.p.X * 2 - e.Location.X, _renderLocation.p.Y * 2 - e.Location.Y), _renderLocation.s * 2);

			GetPlot()?.ZoomBinary(e.Location.X, e.Location.Y, true);

		} else {
			_plotLocation = (new((_plotLocation.p.X + e.Location.X) / 2, (_plotLocation.p.Y + e.Location.Y) / 2), _plotLocation.s / 2);
			_renderLocation = (new((_renderLocation.p.X + e.Location.X) / 2, (_renderLocation.p.Y + e.Location.Y) / 2), _renderLocation.s / 2);
			GetPlot()?.ZoomBinary(e.Location.X, e.Location.Y, false);
		}*/
		DirtyImage(false); // make me want to start a render
		_refreshAxes = true;//RefreshAxes();
		plotBox.Invalidate(); // draw the image and the new shifted location
		plotBox.Update();
	}
	private void openPlot_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
		if (SettingsPanel.Context is not { } c)
			return;
		var file = openPlot.FileName;
		if (!File.Exists(file)) {
			MessageBox.Show("Selected file couldn't be found.", "NO FILE");
			return;
		}
		var t = File.ReadAllText(file);
		var s = _myStates[c];
		var read = 0;
		D(_s.widthBox); // read resolution and xy axes
		D(_s.itfBox); // read animation axes
		D(_s.tRangeButton); // read animation lock
		D(_s.xRangeButton); // read input x lock
		D(_s.yRangeButton); // read input y lock
		D(_s.oyRangeButton); // read output y lock
		D(_s.lockResButton); // read resolution lock
		D(_s.modeSelect); // read plot mode
		D(_s.fyBox); // read fixed y
		D(_s.outputSelect); // read output codes and selections
		//MessageBox.Show("Plotter file loaded.", "LOADED");
		var sup = States.Suppressed;
		States.Suppressed = true;
		CodeChanged(_s.codeBox, EventArgs.Empty);
		RgbChanged(_s.rgbBox, EventArgs.Empty);
		States.Suppressed = sup;
		return;
		void D(Control c) => s.Deserialize(s.D[c], t, ref read);
	}
	private void savePlot_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
		if (SettingsPanel.Context is not { } c)
			return;
		var d = _myStates[c].D;
		/*string[] ss = [S(_s.widthBox), // resolution
			S(_s.itfBox), // animation
			S(_s.tRangeButton), // range time
			S(_s.xRangeButton), // range x
			S(_s.yRangeButton),// range y
			S(_s.oyRangeButton), // range oy
			S(_s.lockResButton), // res lock
			S(_s.modeSelect), // mode
			S(_s.fyBox), // fixed y
			S(_s.outputSelect)]; // outputs*/
		
		File.WriteAllText(savePlot.FileName,
		S(_s.widthBox) // resolution
		+ S(_s.itfBox) // animation
		+ S(_s.tRangeButton) // range time
		+ S(_s.xRangeButton) // range x
		+ S(_s.yRangeButton)// range y
		+ S(_s.oyRangeButton) // range oy
		+ S(_s.lockResButton) // res lock
		+ S(_s.modeSelect) // mode
		+ S(_s.fyBox) // fixed y
		+ S(_s.outputSelect)); // outputs
		return;
		//MessageBox.Show("Plotter file saved.", "SAVED");
		string S(Control c) => d[c].Serialize();
	}
	
	#region MP4

	private bool SavePng() {
		if (_exportBmps.Length <= _frame)
			_exportBmps = new Bitmap?[_length];
		//if (_frame < 0 || encodedPng[_frame] >= 2 || ExportBmps[_frame] != null)
		//	return false;
		if (_exportBmps[_frame] == null)
			return true;
		_encodedPng[_frame] = 1;
		try {
			_ = MakeTemp();
			// new memorystream solution
			var m = _msPngs[_frame] ??= new();
			_exportBmps[_frame]?.Save(m, System.Drawing.Imaging.ImageFormat.Png);
			_msPngs[_frame]?.Flush();

			_encodedPng[_frame] = 3;
			return false;
		} catch (Exception) {
			_encodedPng[_frame] = 0;
			return true;
		}
	}
	internal static string GetRootSaveDir() {
		string baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Comparser");
		//if (!DisableSaving)
		_ = Directory.CreateDirectory(baseDir);
		return baseDir;
	}
	/*internal static string GetGensSaveDir() {
		var dir = Path.Combine(GetRootSaveDir(), "gen");
		if (!Directory.Exists(dir))
			_ = Directory.CreateDirectory(dir);
		return dir;
	}*/
	private string MakeTemp() {
		string path = Path.Combine(GetRootSaveDir(), /*Index.ToString()*/"PNG");
		if (!Directory.Exists(path))
			_ = Directory.CreateDirectory(path);
		return path;
	}
	private int SavePngs() {
		//allocPngType = PngType.Yes; // ensures FinishTasks will want to start threads exporting PNGs
		_pngFailed = 0; // reset failure attempt counter, every png write fail will increment it, and if it reaches 1000 it will cancel the FinishTasks
		//tryPng = previewFrames;

		for (int enc = 0; ! _mp4CancelToken.IsCancellationRequested && enc < _length; Thread.Sleep(100))
			while (enc < _length && _encodedPng[enc] >= 2)
				++enc;
		//FinishTasks(true, true, (short _) => false); // FinishTasks will keep writing PNGs in parallel, until they are all finished (or cancel requested)

		return _pngFailed >= MaxPngFails ? 2 : _mp4CancelToken.IsCancellationRequested ? 1 : 0; // If the export was cancelled from outside - terminate the ffmpeg process
	}
	private (int, int, string) GetPngFormat() {
		int n = 1, nf = _length;
		for (var number = nf; number >= 10; number /= 10)
			++n;
		return (n, nf, "D" + n);
	}
	internal string SavePngs(string pngPath) {
		//exportType = ScheduledTask.Pngs;
		//FinishTasks(true, true, _ => false); // Make sure there are no bitmap generation tasks still running
		if (SavePngs() == 2) 
			return Fail("PNG saving failed"); // failed to save
		if (_mp4CancelToken.IsCancellationRequested)
			return ""; // just cancelled, return with no error
		pngPath = pngPath[..^4]; // remove the ".png"
		var (n, nf, d) = GetPngFormat();
		var maxGenerationTasks = Math.Max(1, SettingsPanel.Mp4Tasks - 1); // task count
		if (maxGenerationTasks <= 1) {
			for (int i = 0; i < _length; ++i) 
				MakePng(i);
			return ""; // finished
		}
		var po = new ParallelOptions {
			MaxDegreeOfParallelism = SettingsPanel.Mp4Tasks,
			CancellationToken = _mp4CancelToken
		};
		var fail = "";
		var result = Parallel.For(0, _length, (i, state) => { MakePng(i); });
		void MakePng(int i) {
			var fileTo = $"{pngPath}_{i.ToString(d)}.png";
			for (var attempt = 0; attempt < 10; ++attempt) {
				try {
					// new memory stream solution
					using FileStream fs = new FileStream(fileTo, FileMode.OpenOrCreate, FileAccess.Write);
					if (_msPngs[i] is { } ms) {
						ms.Position = 0;
						ms.CopyTo(fs);
					}
					fs.Flush();
					fs.Close();
					break;
				} catch (Exception ex) {
					Thread.Sleep(10 + 10 * attempt * attempt); // wait and try again 10 times if failed
				}
			}
		}
		while (!result.IsCompleted)
			Thread.Sleep(100); // Wait until p.for is finished
		return fail; // finished
	}
	private int _encodedMp4, _pngFailed, _encMp4;
	public int SelectedFps = 60;
	private CancellationTokenSource? _mp4Cancel;
	private CancellationToken _mp4CancelToken;
	private byte[] _encodedPng = [];
	private MemoryStream?[] _msPngs = [];
	private const int MaxPngFails = 10;
	private Bitmap?[] _exportBmps = [];

	private void saveMp4_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
		_mp4Cancel?.Cancel();
		_mp4CancelToken = (_mp4Cancel = new()).Token;
		_encodedPng = new byte[_length];
		for (var i = 0; i < _msPngs.Length; ++i) {
			_msPngs[i]?.Dispose();
			_encodedPng[i] = 0;
			_msPngs[i] = null;
		}
		_msPngs = new MemoryStream[_length];
		_ = MakeTemp();
		_encodedMp4 = 0;
		_s.animatedBox.Checked = true;
		_s.itfBox.Text = "0";
		//_s.animatedBox.Checked = false;
		_exportBmps = [];
		//_s.animatedBox.Checked = true;
		//_s.Block();
		Task.Run(() =>
		{
			 PngsToMp4(saveMp4.FileName);
			// _s.Unblock();
			_mp4Cancel = null;
			BeginInvoke(StopMp4);
		});
	}
	private void StopMp4() {
		//_s.Unblock();
		_s.animatedBox.Checked = false;
	}

	/*bool TryPngBitmaps(FractalTask task) {
		if (mp4CancelToken.IsCancellationRequested || bitmapsFinished < previewFrames) // Do not write when cancelled
			return false;
		// Png is starting from scratch - cleanup the temp files and start png index from after the preview
		if (tryPng < 0) {
			tryPng = previewFrames;
			CleanupTempFiles();
		}
		if (allocPngType == PngType.No) {
			//tryPng = bitmapsFinished;
			return false;
		}
		// Increment png index to the first one that hasn;t been encoded yet
		while (tryPng < _length && encodedPng[tryPng] >= 2)
			++tryPng;
		// are all pngs encoded? If so abort thes attempt as finished
		if (tryPng >= _length)
			return false;
		// find an image avaiable to encode png from the first one thet needs to be encoded to that + maxTasks:
		var bitmapIndex = tryPng;
		for (var mx = Math.Min(_length, tryPng + allocMaxTasks); bitmapIndex < mx && encodedPng[bitmapIndex] >= 1; ++bitmapIndex) { }
		if (bitmapIndex >= _length || encodedPng[bitmapIndex] >= 1 || bitmapIndex >= _length || bitmapState[bitmapIndex] < BitmapState.Unlocked)
			return false;
		// this one is avaiable, so do it:
		encodedPng[bitmapIndex] = 1;
		if (IsCancelRequested())
			return false; // Do not write gif frames when cancelled
		task.Start(-3, () => TryPngBitmap(task.TaskIndex, bitmapIndex));
		return true;
	}
	void TryPngBitmap(short taskIndex, int bitmapIndex) {
		var stop = BeginTask();
		//  try to save png, if it failed, decrease the png index to this one so it can be tried again later
		if (SavePng(bitmapIndex - previewFrames))
			tryPng = Math.Min(tryPng, (short)bitmapIndex);
		FinishTask(tasks[taskIndex], stop, "PNG:" + bitmapIndex);
	}*/
	private string PngsToMp4(string mp4Path) {
		var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
		if (!File.Exists(ffmpegPath))
			return Fail("Ffmpeg.exe not found"); // if ffmpeg doesn't exist, return failure immediately
		//FinishTasks(true, true, _ => false); // Make sure there are no bitmap generation tasks still running
		try {
			File.Delete(mp4Path);  // Delete existing file if present
		} catch (IOException ex) {
			return Fail("Failed to delete existing file: " + ex.Message); // return failure if deletion fails
		}
		//Task.Run(() => SavePngs("PNGS"));
		//SavePngs();
		// Start FFmpeg in a parallel process to encode the PNG sequence
		using var ffmpegProcess = new Process {
			StartInfo = new ProcessStartInfo {
				FileName = ffmpegPath,
				//Arguments = $"-y -framerate {SelectedFps} -i {temp}/{filePrefix}image_%0{n}d.png -vf \"scale=iw:ih\" -movflags +faststart -c:v libx264 -profile:v high444 -level 5.2 -preset veryslow -crf 18 -pix_fmt yuv444p -timeout {pngTime.Elapsed.TotalMilliseconds * 2000 + 2000000} \"{mp4Path}\"",
				Arguments = $"-y -framerate {SelectedFps} -f image2pipe -vcodec png -i pipe:0 -vf \"scale=iw:ih\" -movflags +faststart -c:v libx264 -profile:v high444 -level 5.2 -preset veryslow -crf 18 -pix_fmt yuv444p \"{mp4Path}\"",
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardError = true, // will get progress from this
				//RedirectStandardOutput = true, // not needed so far
				CreateNoWindow = true
			}
		};
		string fail = ""; // setup error listener
		try {
			// start pipe:
			//var pipeServer = new NamedPipeServerStream("mypipe", PipeDirection.Out);
			//pipeServer.WaitForConnection();
			
			// start ffmpeg
			if (!ffmpegProcess.Start())
				return Fail("Ffmpeg failed to start");
			//ffmpegProcess.BeginErrorReadLine();// Begin reading error asynchronously

			//ffmpegProcess.BeginOutputReadLine();  // Read standard output asynchronously
			//allocPngType = PngType.Yes; // ensures FinishTasks will want to start threads exporting PNGs
			_pngFailed = 0; // reset failure attempt counter, every png write fail will increment it, and if it reaches 1000 it will cancel the FinishTasks
						   //tryPng = previewFrames; // makes sure that we reexport any missing files, with these settings, the parallel thread elsewhere will export all the pngs as tmp first then rename to png

			// report completion
			var frameRegex = new Regex(@"frame=\s*(\d+)", RegexOptions.Compiled);
			ffmpegProcess.ErrorDataReceived += (s, e) => {
				if (e.Data == null) return;
				var match = frameRegex.Match(e.Data);
				if (match.Success) {
					_encodedMp4 = ushort.Parse(match.Groups[1].Value);
				}
			};
			ffmpegProcess.BeginErrorReadLine();
			// will check if that other parallel thread elsewhere finished exporting all the pngs into the memory streams, and will dump these streams sequentially into the ffmpeg's input
			using (var inputStream = ffmpegProcess.StandardInput.BaseStream) {
				for (;!_mp4CancelToken.IsCancellationRequested && _encMp4 < _length; Thread.Sleep(100)) {
					while (_encMp4 < _length && _frame < _exportBmps.Length && _exportBmps[_frame] != null && _encodedPng[_encMp4] >= 2) {
						if (_msPngs[_encMp4++] is not { } ms)
							continue;
						ms.Position = 0;
						ms.CopyTo(inputStream); // Write memory stream directly to FFmpeg's input stream
					}
					
					SavePng();
				}
			}
			if (_pngFailed >= MaxPngFails || _mp4CancelToken.IsCancellationRequested) { // If the export was cancelled from outside - terminate the ffmpeg process
				if (ffmpegProcess.StandardInput.BaseStream.CanWrite) {
					ffmpegProcess.StandardInput.Write("q");  // Send 'q' to FFmpeg to terminate gracefully
					ffmpegProcess.StandardInput.Flush();     // Ensure the command is sent
					Thread.Sleep(500);  // Give FFmpeg some time to exit gracefully
				}
				if (!ffmpegProcess.HasExited) 
					ffmpegProcess.Kill();  // Force terminate if graceful shutdown isn't possible
			}
			// Wait for the process to exit
			ffmpegProcess.WaitForExit();
		} catch (Exception ex) {
			return Fail("Exception: " + ex.Message); // return exception error
		}
		if (_pngFailed >= MaxPngFails)
			fail += ";Failed to save PNGs";
		return fail != "" ? Fail("Ffmpeg errors: " + fail) : ""; // return fail or success
	}
	private static string Fail(string log) {
		Console.WriteLine(log);
		return log;
	}
	#endregion
}


//splitContainer.Panel1.Controls.Add(_s);
//splitContainer.Panel1.AutoScroll = true;
//splitContainer.Panel1.AutoScrollMinSize = _s.Size + new Size(6, 6);
//splitContainer.Panel1MinSize = 320;
//splitContainer.Panel2MinSize = 1;
//plotBox.Location = new(0, 0);
//plotBox.Size = splitContainer.Panel2.Size;
//_s.Dock = DockStyle.Fill;