using Comparser.Comparser;
using Comparser.Comparser.Numbers;
using Comparser.Forms.Core;
using static Comparser.Forms.Core.IPanel;
namespace Comparser.Forms;
public partial class ExpressionPanel : UserControl, IPanel {
	#region IPanel
	private readonly ControlVar _var;
	public ControlVar GetVar() => _var;
	public Size GetSize() => new(Math.Max((Pad << 2) + Pad + 48 + (RowHeight << 1) + InputSize, 240), 
		(Pad << 2) + (1 + (_expressionRows.Count << 1)) * (RowHeight + Pad));
	public void SetDark(bool dark) { _dark = dark; BaseSetDark(this); }
	public void PerformClose() { }
	public void CoreLayout() {
		var c = Controls;
		c.Clear();
		c.Add(expBox);
		var y = Pad + expBox.Bottom;
		var tab = -1;
		for (var i = 0; i < _expressionRows.Count; ++i, y += (RowHeight + Pad) << 1) {
			var row = _expressionRows[i];
			row.Index.Top = row.Del.Top = row.Field.Box.Top = y;
			row.Result.Top = y + RowHeight + Pad;
			row.Field.Box.TabIndex = ++tab;
			row.Del.TabIndex = ++tab;
			if (i < _expressionRows.Count - 1) {
				var s = _swaps[i];
				s.Top = y + RowHeight + (Pad >> 1);
				s.TabIndex = ++tab;
				c.Add(s);
			}
			c.Add(row.Index);
			c.Add(row.Field.Box);
			c.Add(row.Result);
			c.Add(row.Del);
		}
		var (expW, resW, edelL, swapL) = ExpDim();
		for (var i = 0; i < _expressionRows.Count; ++i) {
			var row = _expressionRows[i];
			row.Field.Box.Width = expW;
			row.Result.Width = resW;
			row.Del.Left = edelL;
			if (i < _expressionRows.Count - 1)
				_swaps[i].Left = swapL;
		}
		SetDark(_dark);
	}
	#endregion

	#region Structures
	protected class ExpRow(Label index, RichTextBox expression, Label result, Button del, EventHandler textChanged) {
		public readonly TextField Field = new(expression, textChanged);
		public readonly Label Index = index;
		public readonly Label Result = result;
		public readonly Button Del = del;
	}
	#endregion

	#region Inits
	public ExpressionPanel() {
		InitializeComponent();
		string plot = " of the plotter.\n",
			it = "Input Time ",
			ix = "Input X ",
			iy = "Input Y ",
			oy = "Output Y ",
			start = "Start",
			center = "Center",
			end = "End";
		SetupControl(expBox, "Here you can evaluate various expressions. For testing or getting some text output.\nRunningthese expressions won't re-parse the program liek using print commands.\nYou also have a lot of input values available here, such as:\n"
			+ "x = the index of the expression. The number you see on its left."
			+ "t = the input time sample. the same t value the expression below had received for this frame.\n"
			+ "ts = " + it + start + plot
			+ "tc = " + it + center + plot
			+ "te = " + it + end + plot
			+ "xs = " + ix + start + plot
			+ "xc = " + ix + center + plot
			+ "xe = " + ix + end + plot
			+ "ys = " + iy + start + plot
			+ "yc = " + iy + center + plot
			+ "ye = " + iy + end + plot
			+ "oys = " + oy + start + plot
			+ "oyc = " + oy + center + plot
			+ "oye = " + oy + end + plot
			+ "ye = Fixed Y" + plot
			+ "w = the screen space horizontal size - the width of the image.\n"
			+ "h = the screen space vertical size - the height of the image.\n"
			+ "f = the animation frame index. Not sampled Time Axis, it is the value in the textbox between the previous and next frame buttons in the animation row.\n"
			+ "l = the animation length, the same value as the leftmost textbox in the animation row.\n");
	}
	public ExpressionPanel(MenuPanel root, ParentForm parent) : this() => InitVar(ref _var, this, root, parent, "Comparser -  Expression Evaluator");
	#endregion

	#region Variables
	private bool _dark;
	private int _controlTabIndex;
	private const int InputSize = 64;
	private readonly List<ExpRow> _expressionRows = [];
	private readonly List<Button> _swaps = [];
	#endregion

	#region Events
	private void ExpChanged(object? sender, EventArgs e) => Eval((int?)((Control?)sender)?.Tag ?? 0);
	private void ExpSwapped(object? sender, EventArgs e) {
		int s = (int?)((Control?)sender)?.Tag ?? 0;
		// swap with invisible panel, so they don't trigger reevaluations mid-swap
		Visible = false;
		var rowA = _expressionRows[s];
		var rowB = _expressionRows[s + 1];
		(rowA.Field.Box.Text, rowA.Result.Text, rowA.Field.Exp, rowA.Field.Text, rowB.Field.Box.Text, rowB.Result.Text, rowB.Field.Exp, rowB.Field.Text)
			= (rowB.Field.Box.Text, rowB.Result.Text, rowB.Field.Exp, rowB.Field.Text, rowA.Field.Box.Text, rowA.Result.Text, rowA.Field.Exp, rowA.Field.Text);
		Visible = true;
		// reevaluate with swapped indices
		Eval(s);
		Eval(s + 1);
	}
	private void ExpDeleted(object? sender, EventArgs e) {
		var d = (int?)((Control?)sender)?.Tag ?? 0;
		// delete with invisible panel, so they don't trigger reevaluations mid-delete
		Visible = false;
		SuspendLayout();
		//outerPanel.SuspendLayout();
		// shake down texts
		for (int i = d + 1; i < _expressionRows.Count; ++i) {
			var rowTo = _expressionRows[i - 1];
			var row = _expressionRows[i];
			rowTo.Field.Text = row.Field.Text;
			rowTo.Field.Box.Text = row.Field.Box.Text;
			rowTo.Result.Text = row.Result.Text;
			rowTo.Field.Exp = row.Field.Exp;
		}
		// remove controls
		_expressionRows.RemoveAt(_expressionRows.Count - 1);
		if (_expressionRows.Count > 0)
			_swaps.RemoveAt(_expressionRows.Count - 1);
		// remake layout without re-evaluation:
		_var.Form.SetMinSize();
		CoreLayout();
		ResumeLayout(false);
		//outerPanel.ResumeLayout(false);
		Visible = true;
		//outerPanel.PerformLayout();
		//innerPanel.PerformLayout();
	}
	#endregion

	#region Actions
	public void ReEval() {
		for (var i = 0; i < _expressionRows.Count; ++i)
			Eval(i, false);
	}
	private void Eval(int index, bool cachedParse = true) {
		if (!Visible)
			return;
		var row = _expressionRows[index];

		object? v, args = GetVar().Root.Plot?.GetPlot().GetExpressionArgs(index);
		
		//object? v, args = new Comparser.Comparser.Value([new(new Real(index), 0, "x")]);
		/*SettingsPanel.Algebra switch {
			1 => new Comparser<Complex>.Value([new(Complex.MakeR(index), 0, "x")]),
			2 => new Comparser<Quaternion>.Value([new(Quaternion.MakeR(index), 0, "x")]),
			_ => new Comparser<Real>.Value([new(Real.MakeR(index), 0, "x")])
		};*/
		if((v = IPanel.Eval(this, row.Field, cachedParse, args)) != null)
			row.Result.Text = SettingsPanel.Context.ToString(v, SettingsPanel.Decimals);
	}
	private static Font f = new ("Consolas", RowHeight / 2.0f);
	private void ExpAdd(object? sender, EventArgs e) {
		var i = _expressionRows.Count;
		string si = i.ToString();
		var a = AnchorStyles.Top;
		
		ExpRow row = new(
			new() {
				Name = "index" + si,
				Text = "x=" + si + ":",
				AutoSize = true,
				Font = f,
				Anchor = a | AnchorStyles.Left,
				Location = new(Pad, 0),
				UseMnemonic = false,
				Size = new(20, RowHeight)
			},
			new() {
				Name = "exp" + si, Text = "1+2x",
				Anchor = a | AnchorStyles.Left | AnchorStyles.Right,
				Font = f,
				Tag = i,
				Location = new(InputSize + 2 * Pad, 0),
				Size = new(0, RowHeight),
				Multiline = false
			},
			new() {
				Name = "result" + si,
				AutoSize = true,
				Font = f,
				Anchor = a | AnchorStyles.Left | AnchorStyles.Right,
				Location = new(Pad, 0),
				UseMnemonic = false,
				Size = new(0, RowHeight)
			},
			new() {
				Name = "delexp" + si,
				Text = "X",
				Anchor = a | AnchorStyles.Right,
				Font = f,
				Tag = i,
				Location = new(0, 0),
				UseMnemonic = false,
				Size = new(RowHeight, (RowHeight << 1) + Pad)
			}, ExpChanged);
		if (i > 0) {
			Button swap = new() {
				Name = "swap" + si,
				Text = "🗘", // "↕"
				Anchor = a | AnchorStyles.Right,
				Font = f,
				Tag = i - 1,
				Location = new(0, 0),
				UseMnemonic = false,
				Size = new(RowHeight, (RowHeight << 1) + Pad)
			};
			swap.Click += ExpSwapped;
			_swaps.Add(swap);
		}
		row.Del.Click += ExpDeleted;
		_expressionRows.Add(row);
		_var.Form.MakeLayout();
		Eval(i);
	}
	#endregion

	#region Getters
	private (int expW, int resW, int delL, int swapL) ExpDim() => (
		Width - InputSize - (RowHeight << 1) - (Pad << 2) - Pad,
		Width - (RowHeight << 1) - ((Pad << 1) + Pad << 1),//innerPanel.Width - rSize - (rowHeight << 1) - pad - (pad << 1),
		Width - (RowHeight + Pad << 1),
		Width - RowHeight - Pad);
	#endregion
	public bool Undo() => true;
	public bool Redo() => true;
	private void SetupControl(Control control, string tip) {
		// Add tooltip and set the next tabIndex
		//myControls.Add(control, tip);
		toolTips.SetToolTip(control, tip/*L(tip)*/); // TODO add localization support L(key)
		control.TabIndex = ++_controlTabIndex;
	}
}