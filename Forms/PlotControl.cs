#define UNSAFEPARSE
using Comparser.Comparser;
using Comparser.Comparser.Numbers;
namespace Comparser.Forms;

public partial class PlotControl : ParentControl {
	private readonly PlotSettings _s;
	public class AxisControls {
		public AxisControls(IComparser context, RichTextBox s, RichTextBox c, RichTextBox e, Button ls, Button lc, Button le, /*CheckBox l, */IPlotAxis axis, AxisControls? old = null) {
			_context = context;
			_axis = axis;
			//(LogBox = l).CheckedChanged += ChangeL;
			S = new(s, ChangeS, old?.S.Text ?? "");
			C = new(c, ChangeC, old?.C.Text ?? "");
			E = new(e, ChangeE, old?.E.Text ?? "");
			Ls = ls;ls.Click += (_,_) => Lock(0, _axis.SetL);
			Lc = lc;lc.Click += (_, _) => Lock(1, _axis.SetL);
			Le = le;le.Click += (_, _) => Lock(2, _axis.SetL);
			if (S.Box is null || C.Box is null || E.Box is null)
				return;
			S.Box.Enabled = C.Box.Enabled = E.Box.Enabled = false;
			(S.Box.Text, C.Box.Text, E.Box.Text) = _axis.GetSce();
			S.Box.Enabled = C.Box.Enabled = E.Box.Enabled = true;

			//logBox.Tag = dBox.Tag = sBox.Tag = Axis = this;
		}
		private readonly IComparser _context;
		public TextField S, C, E;
		public Button Ls, Lc, Le;
		private int _locked = -1;
		//public object? ExpS, ExpD;
		//public bool Log = false;
		//public readonly RichTextBox SBox;
		//public readonly RichTextBox DBox;
		//public readonly CheckBox LogBox;
		private readonly IPlotAxis _axis;
		private void Lock(int newLocked, Action<int> del) {
			L(false);
			del(_locked = _locked == newLocked ? -1 : newLocked);
			L(true);
			void L(bool l) {
				(_locked switch { 0 => S, 1 => C, 2 => E, _ => null })?.Box?.ReadOnly = l;
				if(_locked switch { 0 => Ls, 1 => Lc, 2 => Le, _ => null } is Button b)
					SetLock(_context, b, l);
			}
		}
		private void ChangeS(object? sender, EventArgs e) => Change(S, C, E, _axis.SetS);
		private void ChangeC(object? sender, EventArgs e) => Change(C, S, E, _axis.SetC);
		private void ChangeE(object? sender, EventArgs e) => Change(E, S, C, _axis.SetE);
		//private void ChangeL(object? sender, EventArgs e) => SetSce(_axis.SetLog(LogBox.Checked));
		public void SetSce((string s, string c, string e) sce) {
			if (S.Box is null || C.Box is null || E.Box is null)
				return;
			//var (s, c, e) = (S.Box.ReadOnly, C.Box.ReadOnly, E.Box.ReadOnly);

			S.Box.Enabled = C.Box.Enabled = E.Box.Enabled = false;
			(S.Box.Text, C.Box.Text, E.Box.Text) = sce;
			S.Box.Enabled = C.Box.Enabled = E.Box.Enabled = true;
			//S.Box.ReadOnly = s; C.Box.ReadOnly = c; E.Box.ReadOnly = e;
		}
		private void Change(TextField change, TextField first, TextField second, Func<object?, (string, string)> del) {
			if (change.Box is null || !change.Box.Enabled || first.Box is null || second.Box is null)
				return;
			//var (fir, sec) = (first.Box.ReadOnly, second.Box.ReadOnly);
			first.Box.Enabled = second.Box.Enabled = false;
			(first.Box.Text, second.Box.Text) = del(Eval(_context, change));
			first.Box.Enabled = second.Box.Enabled = true;
			//first.Box.ReadOnly = fir; second.Box.ReadOnly = sec;
		}
		public void SetLength(int l) => SetSce(_axis.SetLength(l));
		
	}
	private AxisControls? InputX, InputY, InputT, OutputY;
	public PlotControl() : base() {
		InitializeComponent();
		_s = new PlotSettings();
		splitContainer.Panel1.Controls.Add(_s);
		_s.Location = new Point(0, 0);
		_s.Size = splitContainer.Panel1.Size;
		_s.Dock = DockStyle.Fill;
	}
	public class Output(string name, RichTextBox rgb, RichTextBox code, EventHandler rgbChanged, EventHandler codeChanged) {
		public TextField Rgb = new(rgb, rgbChanged, "re(v),0,icoef(v,i)", ["v", "x", "y", "f", "w", "h", "l"]), Code = new(code, codeChanged, "z", ["z", "t", "x", "y", "f", "w", "h", "l"]);
		public string Name = name;
	}
	private readonly TextField fy = new(), w = new(), h = new(), tf = new(), tl = new();
	List<Output> outputs = [];
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
		InputX = new(context, _s.ixsBox, _s.ixcBox, _s.ixeBox, _s.ixsButton, _s.ixcButton, _s.ixeButton, p.GetAxis()[0]);
		InputY = new(context, _s.iysBox, _s.iycBox, _s.iyeBox, _s.iysButton, _s.iycButton, _s.iyeButton, p.GetAxis()[1]);
		InputT = new(context, _s.itsBox, _s.itcBox, _s.iteBox, _s.itsButton, _s.itcButton, _s.iteButton, p.GetAxis()[2]);
		OutputY = new(context, _s.oysBox, _s.oycBox, _s.oyeBox, _s.oysButton, _s.oycButton, _s.oyeButton, p.GetAxis()[3]);
		_s.modeSelect.SelectedIndexChanged += ModeSelected;
		w = new(_s.widthBox, WidthChanged);
		h = new(_s.heightBox, HeightChanged);
		fy = new(_s.fyBox, FyChanged);
		tf = new(_s.itfBox, TfChanged);
		tl = new(_s.itlBox, TlChanged);
		_s.addButton.Click += Add;
		_s.delButton.Click += Del;
		_s.xRangeButton.Click += LockRangeX;
		_s.yRangeButton.Click += LockRangeY;
		_s.oyRangeButton.Click += LockRangeO;
		_s.tRangeButton.Click += LockRangeT;
		_s.lockResButton.Click += Res;
		_s.nextButton.Click += nextButton_Click;
		_s.prevButton.Click += prevButton_Click;
		_s.outputSelect.SelectedIndexChanged += SelectOutput;
		Init();
		parent.SetMinSize();
		parent.Text = "Comparser - Plotter";
	}
	private void Init() {
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
		Refresh(tf, _frame.ToString());
		Refresh(tl, _length.ToString());
		Refresh(fy, _fixedY.ToString());
		//Refresh(tl, _length.ToString());
	}
	public void ReEval(IPlot p) {
		for(int i = 0; i < outputs.Count; ++i)
		GetPlot()?.SetCode(i, Parse(outputs[i].Code));
		p.SetDirty();
	}
	public void SetContext(){
		if (Root == null || SettingsControl.Context is not { } context)
			return;
		var p = GetPlot();
		if (p is null)
			return;
		InputX = new(context, _s.ixsBox, _s.ixcBox, _s.ixeBox, _s.ixsButton, _s.ixcButton, _s.ixeButton, p.GetAxis()[0], InputX);
		InputY = new(context, _s.iysBox, _s.iycBox, _s.iyeBox, _s.iysButton, _s.iycButton, _s.iyeButton, p.GetAxis()[1], InputY);
		InputT = new(context, _s.itsBox, _s.itcBox, _s.iteBox, _s.itsButton, _s.itcButton, _s.iteButton, p.GetAxis()[2], InputT);
		OutputY = new(context, _s.oysBox, _s.oycBox, _s.oyeBox, _s.oysButton, _s.oycButton, _s.oyeButton, p.GetAxis()[3], OutputY);
		_s.outputSelect.Items.Clear();
		_s.outputSelect.Items.AddRange(GetPlot()?.GetOutputs() ?? []);
		Init();
		ReEval(p);
	}
	private bool _rangeX = true, _rangeY = true, _rangeO = true, _rangeT = true, _lockedRes = false;
	private void LockRangeX(object? sender, EventArgs e) { PeekRange(_s.xRangeButton, _rangeX = !_rangeX); GetPlot()?.LockRangeX(_rangeX); }
	private void LockRangeY(object? sender, EventArgs e) { PeekRange(_s.yRangeButton, _rangeY = !_rangeY); GetPlot()?.LockRangeY(_rangeY); }
	private void LockRangeO(object? sender, EventArgs e) { PeekRange(_s.oyRangeButton, _rangeO = !_rangeO); GetPlot()?.LockRangeO(_rangeO); }
	private void LockRangeT(object? sender, EventArgs e) { PeekRange(_s.tRangeButton, _rangeT = !_rangeT); GetPlot()?.LockRangeT(_rangeT); }
	private void PeekRange(Button rb, bool l) {
		SetLock(SettingsControl.Context, rb, l);
		/*for (int ia = 0; ia < a.Length; ++ia) {
			var ib = new Button?[3] { a[ia]?.Ls, a[ia]?.Lc, a[ia]?.Le };
			for (int i = 0; i < 3; ++i) if (ib[i] is Button b){
				b.Enabled = !_rangeY;
				b.BackColor = _rangeY || b.Text == "🔒" ? Color.Red : GetContext()?.GetColor().b ?? Color.Black;
			}
		}*/
	}
	private void Res(object? sender, EventArgs e) {
		SetLock(SettingsControl.Context, _s.lockResButton, _lockedRes = !_lockedRes);
		GetPlot()?.SetLockRes(_lockedRes);
		Resized(null, EventArgs.Empty);
	}
	private static void SetLock(IComparser? c, Button b, bool locked) {
		b.Text = locked ? "🔒" : "🔓";
		b.BackColor = locked ? Color.Red : c?.GetColor().b ?? Color.Black;
	}
	
	private void Add(object? sender, EventArgs e) {
		if (_s.outputSelect.Text == "") {
			Err();
			return;
		}
		foreach (var o in outputs) if (o.Name == _s.outputSelect.Text) { Err(); return; }
		outputs.Add(new(_s.outputSelect.Text, _s.rgbBox, _s.codeBox, RgbChanged, CodeChanged));
		GetPlot()?.AddOutput(_s.outputSelect.Text, null, null);
		var c = _s.outputSelect.Items.Count;
		_s.outputSelect.Items.Add(_s.outputSelect.Text);
		_s.outputSelect.SelectedIndex = c;//SelectOutput(null, EventArgs.Empty);
		return;
		static void Err() => MessageBox.Show("You must write a unique name to the combo box to the right before adding.", "Error: No name.");
	}
	private void Del(object? sender, EventArgs e) {
		if (_s.outputSelect.Text == "") {
			Err();
			return;
		}
		bool notfound = true;
		foreach (var o in outputs) if (o.Name == _s.outputSelect.Text) { notfound = false; break;}
		if (notfound) {
			Err();
			return;
		}
		outputs.RemoveAt(_s.outputSelect.SelectedIndex);
		SelectOutput(null, EventArgs.Empty);
		return;
		static void Err() => MessageBox.Show("You must have some output selected to delete one.", "Error: No name.");
	}
	private void SelectOutput(object? sender, EventArgs e) {
		var i = _s.outputSelect.SelectedIndex;
		if (outputs.Count <= i || i < 0) {
			_s.rgbBox.ReadOnly = _s.codeBox.ReadOnly = true;
			return;
		}
		var o = outputs[i];
		o.Rgb?.Box?.Text = o.Rgb.Text;
		o.Code?.Box?.Text = o.Code.Text;
		_s.rgbBox.ReadOnly = _s.codeBox.ReadOnly = false;
	}
	private void CodeChanged(object? sender, EventArgs e) {
		var i = _s.outputSelect.SelectedIndex;
		if (i < 0 || i >= outputs.Count)
			return;
		GetPlot()?.SetCode(i, Parse(outputs[i].Code));
	}
	private void RgbChanged(object? sender, EventArgs e) {
		var i = _s.outputSelect.SelectedIndex;
		if (i < 0 || i >= outputs.Count)
			return;
		GetPlot()?.SetRgb(i, Parse(outputs[i].Rgb));
	}
	private int prevW = 0;
	/*private void ResizedForm(object? sender, EventArgs e) {
		splitContainer.FixedPanel = (splitContainer.Panel1.Width < 320) == (prevW < Width) ? FixedPanel.Panel2 : FixedPanel.Panel1;
		prevW = Width;
	}*/
	private void Resized(object? sense, EventArgs e) {
		if (_lockedRes)
			return;
		if (!(w.Box?.Enabled ?? false))
			return;
		if (!(h.Box?.Enabled ?? false))
			return;
		Refresh(w, splitContainer.Panel2.Width.ToString());
		Refresh(h, splitContainer.Panel2.Height.ToString());
		UpdatePlot();
		if (GetPlot() is not { } p || InputX == null || InputY == null /*|| InputT == null*/ || OutputY == null)
			return;
		RefreshSce(p, InputX, 0);
		RefreshSce(p, InputY, 1);
		//RefreshSce(p, InputT, 2);
		RefreshSce(p, OutputY, 3);
	}
	private void RefreshSce(IPlot p, AxisControls? a, int i) {
		if (a == null)
			return;
		var (s,c,e) = p.GetAxis()[i].GetSce();
		Refresh(a.S, s);
		Refresh(a.C, c);
		Refresh(a.E, e);
	}
	private void Refresh(TextField f, string s) {
		//var ro = !(f.Box?.Enabled ?? true); 
		f.Box?.Enabled = false;
		//var s = splitContainer.Panel2.Height.ToString();
		if ((f.Box?.Text ?? "") != s) f.Box?.Text = s;
		Parse(f, true);
		f.Box?.Enabled = true;
		//f.Box?.ReadOnly = ro;
	}
	private void WidthChanged(object? sender, EventArgs e) {
		if (!(w.Box?.Enabled ?? false))
			return;
		var s = (int)(SettingsControl.Context?.AsDouble(Eval(w)) ?? plotBox.Width) - plotBox.Width;
		w.Box?.Enabled = false;
		Width += s;
		w.Box?.Enabled = true;
	}
	private void HeightChanged(object? sender, EventArgs e) {
		if (!(h.Box?.Enabled ?? false))
			return;
		var s = (int)(SettingsControl.Context?.AsDouble(Eval(h)) ?? plotBox.Height) - plotBox.Height;
		h.Box?.Enabled = false;
		Height += s;
		h.Box?.Enabled = true;
	}
	private int _length = 1, _frame = 0;
	private void TfChanged(object? sender, EventArgs e) => GetPlot()?.SetFrame(Math.Min(_length - 1, _frame = (int)(SettingsControl.Context?.AsDouble(Eval(tf)) ?? 0)));
	private void TlChanged(object? sender, EventArgs e) {
		/*InputT?.SetLength(*/
		_length = Math.Max(1, (int)(SettingsControl.Context?.AsDouble(Eval(tl)) ?? 1))/*)*/;
		UpdatePlot();
		if (GetPlot() is not { } p || InputT == null)
			return;
		RefreshSce(p, InputT, 2);
		TfChanged(null, EventArgs.Empty);
	}
	private void ModeSelected(object? sender, EventArgs e) => Set2d(_s, GetPlot(), (PlotMode)_s.modeSelect.SelectedIndex);
	private static void Set2d(PlotSettings s, IPlot? p, PlotMode mode) {
		p?.ChangeMode(mode);
		s.fyBox.ReadOnly = mode == PlotMode.Xy;
		//bool enabled = mode == PlotMode.Xy;
		//s.oysLabel.Visible = s.oysButton.Visible = s.oysBox.Visible = s.oycLabel.Visible = s.oycButton.Visible = s.oycBox.Visible = s.oyeLabel.Visible = s.oyeButton.Visible = s.oyeBox.Visible = !enabled;
		//s.fyBox.ReadOnly = s.iysLabel.Visible = s.iysButton.Visible = s.iysBox.Visible = s.iycLabel.Visible = s.iycButton.Visible = s.iycBox.Visible = s.iyeLabel.Visible = s.iyeButton.Visible = s.iyeBox.Visible = enabled;
	}
	private double _fixedY = 0;
	private void FyChanged(object? sender, EventArgs e) {
		string v;
		(_fixedY, v) = GetPlot()?.SetFixedY(Eval(fy)) ?? (0, "?");
		_s.fyLabel.Text = "Y: " + v;
	}

	//private void PlotButton_Click(object sender, EventArgs e) => Root?.ShowC(_setForm, FormP);
	public override Size GetSize() => new(120, Pad + 4 * (Pad + RowHeight));
	public override void SetDark(bool dark) { base.SetDark(dark); }
	private IPlot? GetPlot() => SettingsControl.Context?.GetPlot();
	//private IComparser? GetContext() => SettingsControl.Context;
	private void Fps_Tick(object? sender, EventArgs e) {
		if (_s.animatedBox.Checked)
			nextButton_Click(_s.nextButton, EventArgs.Empty);
		UpdatePlot();
	}
	private void UpdatePlot() {
		if (GetPlot() is not { } p)
			return;
		plotBox.Image = p.Update(splitContainer.Panel2.Width, splitContainer.Panel2.Height, _length);
		//plotBox.Size = plotBox.Image.Size;
		//_dirty = false;
	}

	private void prevButton_Click(object? sender, EventArgs e) {
		tf.Box?.Text = ((_length - 1 + _frame) % _length).ToString();
		//var l = (int)(GetContext()?.AsDouble(tl.Value) ?? 1);
		//tf.Text = ((l - 1 + (int)(GetContext()?.AsDouble(Eval(tf)) ?? 0)) % l).ToString();
	}
	private void nextButton_Click(object? sender, EventArgs e) 
		=> tf.Box?.Text = ((1 + _frame) % _length).ToString();//tf.Text = ((1 + (int)(GetContext()?.AsDouble(Eval(tf)) ?? 0)) % (int)(GetContext()?.AsDouble(tl.Value) ?? 1)).ToString();
}