#define UNSAFEPARSE
using Comparser.Comparser;

namespace Comparser.Forms;
public partial class PlotControl : ParentControl {
	public class AxisControls {
		public AxisControls(IComparser context, RichTextBox s, RichTextBox c, RichTextBox e, /*CheckBox l, */IPlotAxis axis) {
			_context = context;
			_axis = axis;
			//(LogBox = l).CheckedChanged += ChangeL;
			S = new(s, ChangeS);
			C = new(c, ChangeC);
			E = new(e, ChangeE);
			//logBox.Tag = dBox.Tag = sBox.Tag = Axis = this;
		}
		private readonly IComparser _context;
		public TextField S, C, E;
		//public object? ExpS, ExpD;
		//public bool Log = false;
		//public readonly RichTextBox SBox;
		//public readonly RichTextBox DBox;
		public readonly CheckBox LogBox;
		private readonly IPlotAxis _axis;
		private void ChangeS(object? sender, EventArgs e) => Change(S, C, E, _axis.SetS);
		private void ChangeC(object? sender, EventArgs e) => Change(C, S, E, _axis.SetC);
		private void ChangeE(object? sender, EventArgs e) => Change(E, S, C, _axis.SetE);
		//private void ChangeL(object? sender, EventArgs e) => SetSce(_axis.SetLog(LogBox.Checked));
		public void SetSce((string s, string c, string e) sce) {
			S.Box.Enabled = C.Box.Enabled = E.Box.Enabled = false;
			(S.Box.Text, C.Box.Text, E.Box.Text) = sce;
			S.Box.Enabled = C.Box.Enabled = E.Box.Enabled = true;
		}
		private void Change(TextField change, TextField first, TextField second, Func<object?, (string, string)> del) {
			if (!change.Box.Enabled)
				return;
			first.Box.Enabled = second.Box.Enabled = false;
			(first.Box.Text, second.Box.Text) = del(Eval(_context, change));
			first.Box.Enabled = second.Box.Enabled = true;
		}
	}
	private readonly TextField? _fixedY;
	private readonly AxisControls? InputX, InputY, InputT, OutputY;
	public PlotControl() => InitializeComponent();
	public PlotControl(MenuControl? root, ParentForm parent) : base(root, parent) {
		InitializeComponent();
		if (root == null || root?.Set?.Context is not { } context)
			return;
		var p = GetPlot();
		if (p is null)
			return;
		_fixedY = new(fyBox, FixedY);
		InputX = new(context, ixsBox, ixcBox, ixeBox, p.GetAxis()[0]);
		InputY = new(context, iysBox, iycBox, iyeBox, p.GetAxis()[1]);
		InputT = new(context, itsBox, itcBox, iteBox, p.GetAxis()[2]);
		OutputY = new(context, oysBox, oycBox, oyeBox, p.GetAxis()[3]);
		//var m = FormStartPosition.Manual;
		//_set = new(root, _setForm = new());
		//_setForm?.StartPosition = m;
		parent.SetMinSize();
		parent.Text = "Comparser - Plotter";
	}
	//private void PlotButton_Click(object sender, EventArgs e) => Root?.ShowC(_setForm, FormP);
	public override Size GetSize() => new(120, Pad + 4 * (Pad + RowHeight));
	public override void SetDark(bool dark) { base.SetDark(dark); }
	private void FixedY(object? sender, EventArgs e) => GetPlot()?.SetFixedY(Eval(_fixedY!));
	private IPlot? GetPlot() => Root?.Set?.Context?.GetPlot();
	private void Fps_Tick(object? sender, EventArgs e) {
		if (GetPlot() is not { } p)
			return;
		plotBox.Image = p.Update(plotBox.Width, plotBox.Height);
		//_dirty = false;
	}
}