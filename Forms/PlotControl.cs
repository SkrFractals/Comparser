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
				(_locked switch { 0 => S, 1 => C, 2 => E, _ => null })?.Box?.Enabled = !l;
				if(_locked switch { 0 => Ls, 1 => Lc, 2 => Le, _ => null } is Button b)
					SetLock(b, l);
			}
		}
		private void ChangeS(object? sender, EventArgs e) => Change(S, C, E, _axis.SetS);
		private void ChangeC(object? sender, EventArgs e) => Change(C, S, E, _axis.SetC);
		private void ChangeE(object? sender, EventArgs e) => Change(E, S, C, _axis.SetE);
		//private void ChangeL(object? sender, EventArgs e) => SetSce(_axis.SetLog(LogBox.Checked));
		public void SetSce((string s, string c, string e) sce) {
			if (S.Box is null || C.Box is null || E.Box is null)
				return;
			var (s, c, e) = (S.Box.Enabled, C.Box.Enabled, E.Box.Enabled);

			S.Box.Enabled = C.Box.Enabled = E.Box.Enabled = false;
			(S.Box.Text, C.Box.Text, E.Box.Text) = sce;
			S.Box.Enabled = s; C.Box.Enabled = c; E.Box.Enabled = e;
		}
		private void Change(TextField change, TextField first, TextField second, Func<object?, (string, string)> del) {
			if (change.Box is null || !change.Box.Enabled || first.Box is null || second.Box is null)
				return;
			var (fir, sec) = (first.Box.Enabled, second.Box.Enabled);
			first.Box.Enabled = second.Box.Enabled = false;
			(first.Box.Text, second.Box.Text) = del(Eval(_context, change));
			first.Box.Enabled = fir; second.Box.Enabled = sec;
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
		_s.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
	}
	public class Output(string name, RichTextBox rgb, RichTextBox code, EventHandler rgbChanged, EventHandler codeChanged) {
		public TextField Rgb = new(rgb, rgbChanged), Code = new(code, codeChanged);
		public string Name = name;
	}
	private readonly TextField fy = new(), w = new(), h = new(), tf = new(), tl = new();
	List<Output> outputs = [];
	public PlotControl(MenuControl? root, ParentForm parent) : base(root, parent) {
		InitializeComponent();
		_s = new PlotSettings {
			Location = new Point(0, 0),
			Size = splitContainer.Panel1.Size
		};
		splitContainer.Panel1.Controls.Add(_s);
		splitContainer.Panel1.AutoScroll = true;
		splitContainer.Panel1.AutoScrollMinSize = _s.Size + new Size(6, 6);
		//_s.AutoScrollMinSize = _s.Size + new Size(6, 6);
		//Size min = new(480, 1 + ClientSize.Height - splitContainer.Panel2MinSize);
		//FormP?.Size = min; FormP?.MinimumSize = min;
		splitContainer.Panel1MinSize = 320;
		splitContainer.Panel2MinSize = 1;
		plotBox.Location = new(0, 0);
		plotBox.Size = splitContainer.Panel2.Size;
		_s.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
		//_s.rgbBox.Location = _s.codeBox.Location = new(0, 0);
		//_s.codeBox.Size = _s.splitContainer.Panel1.Size;
		//_s.rgbBox.Size = _s.splitContainer.Panel2.Size;
		if (root == null || root?.Set?.Context is not { } context)
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
		_s.ratioButton.Click += Ratio;
		_s.outputSelect.SelectedIndexChanged += SelectOutput;
		Init();
		//var m = FormStartPosition.Manual;
		//_set = new(root, _setForm = new());
		//_setForm?.StartPosition = m;
		parent.SetMinSize();
		parent.Text = "Comparser - Plotter";
	}
	private void Init() {
		_s.modeSelect.SelectedIndex = 0;
		//ModeSelected(_s.modeSelect, EventArgs.Empty);
		TlChanged(_s.itlBox, EventArgs.Empty);
		TfChanged(_s.itfBox, EventArgs.Empty);
		FyChanged(_s.fyBox, EventArgs.Empty);
		SelectOutput(_s.outputSelect, EventArgs.Empty);
	}
	public void ReEval(IPlot p) {
		for(int i = 0; i < outputs.Count; ++i)
		GetPlot()?.SetCode(i, Parse(outputs[i].Code));
		p.SetDirty();
	}
	public void SetContext(){
		if (Root == null || Root?.Set?.Context is not { } context)
			return;
		var p = GetPlot();
		if (p is null)
			return;
		InputX = new(context, _s.ixsBox, _s.ixcBox, _s.ixeBox, _s.ixsButton, _s.ixcButton, _s.ixeButton, p.GetAxis()[0], InputX);
		InputY = new(context, _s.iysBox, _s.iycBox, _s.iyeBox, _s.iysButton, _s.iycButton, _s.iyeButton, p.GetAxis()[1], InputY);
		InputT = new(context, _s.itsBox, _s.itcBox, _s.iteBox, _s.itsButton, _s.itcButton, _s.iteButton, p.GetAxis()[2], InputT);
		OutputY = new(context, _s.oysBox, _s.oycBox, _s.oyeBox, _s.oysButton, _s.oycButton, _s.oyeButton, p.GetAxis()[3], InputY);
		_s.outputSelect.Items.Clear();
		_s.outputSelect.Items.AddRange(GetPlot()?.GetOutputs() ?? []);
		Init();
		ReEval(p);
	}
	private bool _ratio = true;
	private void Ratio(object? sender, EventArgs e) {
		SetLock(_s.ratioButton, _ratio = !_ratio);
		GetPlot()?.SetLockRatio(_ratio);
	}
	private static void SetLock(Button b, bool locked) => b.Text = locked ? "🔒" : "🔓";
	
	private void Add(object? sender, EventArgs e) {
		if (_s.outputSelect.Text == "") {
			Err();
			return;
		}
		foreach (var o in outputs) if (o.Name == _s.outputSelect.Text) { Err(); return; }
		outputs.Add(new(_s.outputSelect.Text, _s.rgbBox, _s.codeBox, RgbChanged, CodeChanged));
		_s.outputSelect.Items.Add(_s.outputSelect.Text);
		SelectOutput(null, EventArgs.Empty);
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
			_s.rgbBox.Enabled = _s.codeBox.Enabled = false;
			return;
		}
		var o = outputs[i];
		o.Rgb?.Box?.Text = o.Rgb.Text;
		o.Code?.Box?.Text = o.Code.Text;
		_s.rgbBox.Enabled = _s.codeBox.Enabled = true;
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
	private void Resized(object? sense, EventArgs e) {
		if (!(w.Box?.Enabled ?? false))
			return;
		if (!(h.Box?.Enabled ?? false))
			return;

		Refresh(w, splitContainer.Panel2.Width.ToString());
		Refresh(h, splitContainer.Panel2.Height.ToString());
		UpdatePlot();
		if (GetPlot() is not { } p || InputX == null || InputY == null || InputT == null || OutputY == null)
			return;
		RefreshSce(p, InputX, 0);
		RefreshSce(p, InputY, 1);
		RefreshSce(p, InputT, 2);
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
		f.Box?.Enabled = false;
		//var s = splitContainer.Panel2.Height.ToString();
		if ((f.Box?.Text ?? "") != s) f.Box?.Text = s;
		Parse(f, true);
		f.Box?.Enabled = true;
	}
	private void WidthChanged(object? sender, EventArgs e) {
		if (!(w.Box?.Enabled ?? false))
			return;
		var s = (int)(GetContext()?.AsDouble(Eval(w)) ?? plotBox.Width) - plotBox.Width;
		w.Box?.Enabled = false;
		Width += s;
		w.Box?.Enabled = true;
	}
	private void HeightChanged(object? sender, EventArgs e) {
		if (!(h.Box?.Enabled ?? false))
			return;
		var s = (int)(GetContext()?.AsDouble(Eval(h)) ?? plotBox.Height) - plotBox.Height;
		h.Box?.Enabled = false;
		Height += s;
		h.Box?.Enabled = true;
	}
	private int _length = 1;
	private void TfChanged(object? sender, EventArgs e) => GetPlot()?.SetFrame(Math.Min(_length - 1, (int)(GetContext()?.AsDouble(Eval(tf)) ?? 0)));
	private void TlChanged(object? sender, EventArgs e) => InputT?.SetLength(_length = Math.Max(1, (int)(GetContext()?.AsDouble(Eval(tl)) ?? 1)));
	private void ModeSelected(object? sender, EventArgs e) => Set2d(_s, GetPlot(), (PlotMode)_s.modeSelect.SelectedIndex);
	private static void Set2d(PlotSettings s, IPlot? p, PlotMode mode) {
		p?.ChangeY(mode);
		bool enabled = mode == PlotMode.Xy;
		s.fyBox.Enabled = s.oysLabel.Visible = s.oysButton.Visible = s.oysBox.Visible = s.oycLabel.Visible = s.oycButton.Visible = s.oycBox.Visible = s.oyeLabel.Visible = s.oyeButton.Visible = s.oyeBox.Visible = !enabled;
		s.iysLabel.Visible = s.iysButton.Visible = s.iysBox.Visible = s.iycLabel.Visible = s.iycButton.Visible = s.iycBox.Visible = s.iyeLabel.Visible = s.iyeButton.Visible = s.iyeBox.Visible = enabled;
	}

	private void FyChanged(object? sender, EventArgs e) => GetPlot()?.SetFixedY(Eval(fy));

	//private void PlotButton_Click(object sender, EventArgs e) => Root?.ShowC(_setForm, FormP);
	public override Size GetSize() => new(120, Pad + 4 * (Pad + RowHeight));
	public override void SetDark(bool dark) { base.SetDark(dark); }
	private IPlot? GetPlot() => GetContext()?.GetPlot();
	private IComparser? GetContext() => Root?.Set?.Context;
	private void Fps_Tick(object? sender, EventArgs e) {
		if (_s.animatedBox.Checked)
			nextButton_Click(_s.nextButton, EventArgs.Empty);
		UpdatePlot();
	}
	private void UpdatePlot() {
		if (GetPlot() is not { } p)
			return;
		plotBox.Image = p.Update(plotBox.Width, plotBox.Height);
		//_dirty = false;
	}

	private void prevButton_Click(object sender, EventArgs e) {
		var l = (int)(GetContext()?.AsDouble(tl.Value) ?? 1);
		tf.Text = ((l - 1 + (int)(GetContext()?.AsDouble(Eval(w)) ?? 0)) % l).ToString();
	}
	private void nextButton_Click(object sender, EventArgs e) 
		=> tf.Text = ((1 + (int)(GetContext()?.AsDouble(Eval(w)) ?? 0)) % (int)(GetContext()?.AsDouble(tl.Value) ?? 1)).ToString();
}