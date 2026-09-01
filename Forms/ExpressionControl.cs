using Comparser.Comparser;
using Comparser.Comparser.Numbers;
namespace Comparser.Forms;
public partial class ExpressionControl : ParentControl {
		public class ExpRow(Label index, RichTextBox expression, Label result, Button del) {
		public object? Exp;
		public string Text = "";
		public readonly Label Index = index;
		public readonly RichTextBox Expression = expression;
		public readonly Label Result = result; 
		public readonly Button Del = del;
	}
	private bool _dark;
	private const int InputSize = 64;
	private readonly List<ExpRow> _expressionRows = [];
	private readonly List<Button> _swaps = [];
	public ExpressionControl() => InitializeComponent();
	public ExpressionControl(MenuControl root, ParentForm parent) : base(root, parent){
		InitializeComponent();
		parent.SetMinSize();
		parent.Text = "Comparser - Expression Evaluator";
	}
	public void ReEval() {
		for (var i = 0; i < _expressionRows.Count; ++i)
			Eval(i, false);
	}
	private void Eval(int index, bool cachedParse = true) {
		if (!Visible)
			return;
		var row = _expressionRows[index];
		object eval = Root?.Set?.Algebra switch {
			1 => new Comparser<Complex>.Value([new(Complex.MakeR(index), 0, "x")]),
			2 => new Comparser<Quaternion>.Value([new(Quaternion.MakeR(index), 0, "x")]),
			_ => new Comparser<Real>.Value([new(Real.MakeR(index), 0, "x")])
		};
		object? v;
		if (cachedParse && row.Exp != null && row.Text == row.Expression.Text && row.Text != "") {
			v = Root?.Set?.Context?.Eval(row.Exp, eval);
		} else {
			(int, Color)[] colors = [];
			v = Root?.Set?.Context?.ParseEval(new CancellationTokenSource().Token, row.Text = row.Expression.Text/*Clean()*/, 0,out row.Exp, out colors, eval);
			ComparserControl.ApplyColors(colors, row.Expression);
		}
		
	
		if(v != null)
			row.Result.Text = Root?.Set?.Context?.ToString(v,  Root.Set.Decimals);
	}

	public override void CoreLayout() {
		var c = Controls;
		c.Clear();
		c.Add(expBox);
		var y = Pad + expBox.Bottom;
		var tab = -1;
		for (var i = 0; i < _expressionRows.Count; ++i, y += (RowHeight + Pad) << 1) {
			var row = _expressionRows[i];
			row.Index.Top = row.Expression.Top = row.Del.Top = y;
			row.Result.Top = y + RowHeight + Pad;
			row.Expression.TabIndex = ++tab;
			row.Del.TabIndex = ++tab;
			if (i < _expressionRows.Count - 1) {
				var s = _swaps[i];
				s.Top = y + RowHeight + (Pad >> 1);
				s.TabIndex = ++tab;
				c.Add(s);
			}
			c.Add(row.Index);
			c.Add(row.Expression);
			c.Add(row.Result);
			c.Add(row.Del);
		}
		var (expW, resW, edelL, swapL) = ExpDim();
		for (var i = 0; i < _expressionRows.Count; ++i) {
			var row = _expressionRows[i];
			row.Expression.Width = expW;
			row.Result.Width = resW;
			row.Del.Left = edelL;
			if (i < _expressionRows.Count - 1)
				_swaps[i].Left = swapL;
		}
		SetDark(_dark);
	}
	private (int expW, int resW, int delL, int swapL) ExpDim() => (
		Width - InputSize - (RowHeight << 1) - (Pad << 2) - Pad,
		Width - (RowHeight << 1) - (((Pad << 1) + Pad) << 1),//innerPanel.Width - rSize - (rowHeight << 1) - pad - (pad << 1),
		Width - ((RowHeight + Pad) << 1),
		Width - RowHeight - Pad);
	private void ExpAdd(object? sender, EventArgs e) {
		var i = _expressionRows.Count;
		string si = i.ToString();
		var a = AnchorStyles.Top;
		ExpRow row = new(
			new() {
				Name = "index" + si,
				Text = "x=" + si + ":",
				AutoSize = true,
				Font = new Font("Consolas", RowHeight >> 1),
				Anchor = a | AnchorStyles.Left,
				Location = new(Pad, 0),
				UseMnemonic = false,
				Size = new(20, RowHeight)
			},
			new() {
				Name = "exp" + si, Text = "1+2x",
				Anchor = a | AnchorStyles.Left | AnchorStyles.Right,
				Tag = i,
				Location = new(InputSize + 2 * Pad, 0),
				Size = new(0, RowHeight),
				Multiline = false
			},
			new() {
				Name = "result" + si,
				AutoSize = true,
				Font = new Font("Consolas", RowHeight >> 1),
				Anchor = a | AnchorStyles.Left | AnchorStyles.Right,
				Location = new(Pad, 0),
				UseMnemonic = false,
				Size = new(0, RowHeight)
			},
			new() {
				Name = "delexp" + si,
				Text = "X",
				Anchor = a | AnchorStyles.Right,
				Tag = i,
				Location = new(0, 0),
				UseMnemonic = false,
				Size = new(RowHeight, (RowHeight << 1) + Pad)
			});
		if (i > 0) {
			Button swap = new() {
				Name = "swap" + si,
				Text = "🗘", // "↕"
				Anchor = a | AnchorStyles.Right,
				Tag = i - 1,
				Location = new(0, 0),
				UseMnemonic = false,
				Size = new(RowHeight, (RowHeight << 1) + Pad)
			};
			swap.Click += ExpSwapped;
			_swaps.Add(swap);
		}
		ComparserControl.InitRichTextBox(row.Expression, ExpChanged);
		row.Del.Click += ExpDeleted;
		_expressionRows.Add(row);
		FormP?.MakeLayout();
		Eval(i);
	}
	private void ExpChanged(object? sender, EventArgs e) => Eval((int?)((Control?)sender)?.Tag ?? 0);

	private void ExpSwapped(object? sender, EventArgs e) {
		int s = (int?)((Control?)sender)?.Tag ?? 0;
		// swap with invisible panel, so they don't trigger reevaluations mid-swap
		Visible = false;
		var rowA = _expressionRows[s];
		var rowB = _expressionRows[s + 1];
		(rowA.Expression.Text, rowA.Result.Text, rowA.Exp, rowA.Text, rowB.Expression.Text, rowB.Result.Text, rowB.Exp, rowB.Text) 
			= (rowB.Expression.Text, rowB.Result.Text, rowB.Exp,rowB.Text, rowA.Expression.Text, rowA.Result.Text, rowA.Exp, rowA.Text);
		Visible = true;
		// reevaluate with swapped indices
		Eval(s);
		Eval(s + 1);
	}
	private void ExpDeleted(object? sender, EventArgs e) {
		var d = ((int?)((Control?)sender)?.Tag) ?? 0;
		// delete with invisible panel, so they don't trigger reevaluations mid-delete
		Visible = false;
		SuspendLayout();
		//outerPanel.SuspendLayout();
		// shake down texts
		for (int i = d + 1; i < _expressionRows.Count; ++i) {
			var rowTo = _expressionRows[i - 1];
			var row = _expressionRows[i];
			rowTo.Text = row.Text;
			rowTo.Expression.Text = row.Expression.Text;
			rowTo.Result.Text = row.Result.Text;
			rowTo.Exp = row.Exp;
		}
		// remove controls
		_expressionRows.RemoveAt(_expressionRows.Count - 1);
		if (_expressionRows.Count > 0)
			_swaps.RemoveAt(_expressionRows.Count - 1);
		// remake layout without re-evaluation:
		FormP?.SetMinSize();
		CoreLayout();
		ResumeLayout(false);
		//outerPanel.ResumeLayout(false);
		Visible = true;
		//outerPanel.PerformLayout();
		//innerPanel.PerformLayout();
	}

	//private static string Clean(string t) // forbidden symbols in expressions
	//	=> //t.ToLower()//.Replace(":", "").Replace(";", "").Replace("|", "")
	//	t.Replace("\t", "").Replace("\r", "").Replace("\n", "");
	public override Size GetSize() => new(
		Math.Max((Pad << 2) + Pad + 48 + (RowHeight << 1) + InputSize, 240),
		(Pad << 2) + (1 + (_expressionRows.Count << 1)) * (RowHeight + Pad));
	public override void SetDark(bool dark) => base.SetDark(_dark = dark);
	/*private void SetDark(ExpRow exp) {
		exp.Del.BackColor = exp.Expression.BackColor = _bf.b; 
		exp.Del.ForeColor = exp.Expression.ForeColor = _bf.f;
	}*/
}