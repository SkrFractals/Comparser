using Comparser.Comparser;
using Comparser.Comparser.Numbers;

namespace Comparser;

// TODO:
// plot - boundaries, drawing/resizing, coloring settings

public partial class ComparserForm : Form {

	/*public class FuncRow(TextBox def, Button del) {
		//public Comparser.Expression? Exp = null;
		//public string Text = "";
		public TextBox Def = def; 
		public Button Del = del;
	}*/
	public class ExpRow(Label index, TextBox expression, Label result, Button del) {
		public object? Exp;
		public string Text = "";
		public readonly Label Index = index;
		public readonly TextBox Expression = expression;
		public readonly Label Result = result; 
		public readonly Button Del = del;
	}

	private const int RowHeight = 32, InputSize = 64, Pad = 2;
	private int _decimals;
	private int _algebra = 1;
	private readonly IComparser[] _algebras = [new ComparserR(), new ComparserC(), new ComparserQ()];
	private IComparser _context;
	//private readonly List<FuncRow> CustomFunctions = [];
	private readonly List<ExpRow> _expressionRows = [];
	private readonly List<Button> _swaps = [];
	public ComparserForm() {
		_context = _algebras[_algebra];
		//FailedFunction = new(_context, "", [([], "")]);
		InitializeComponent();
		SetMinSize();
		DecimalBox_TextChanged(decimalBox, EventArgs.Empty);
	}
	private void Eval(int index, bool cachedParse = true) {
		if (!innerPanel.Visible)
			return;
		var row = _expressionRows[index];
		object eval = _algebra switch {
			1 => new Comparser<Complex>.Value([new(Complex.MakeR(index), "x")]),
			2 => new Comparser<Quaternion>.Value([new(Quaternion.MakeR(index), "x")]),
			_ => new Comparser<Real>.Value([new(Real.MakeR(index), "x")])
		};
		var v = cachedParse && row.Exp != null && row.Text == row.Expression.Text && row.Text != "" 
			? _context.Eval(row.Exp, eval) 
			: _context.ParseEval(row.Text = row.Expression.Text = Clean(row.Expression.Text), out row.Exp, eval);
		row.Result.Text = _context.ToString(v, _decimals);
	}
	private void CodeBox_TextChanged(object? sender, EventArgs e) {
		logLabel.Text = _context.ReadCode(codeBox.Text);
		for (int i = 0; i < _expressionRows.Count; ++i)
			Eval(i, false);
	}
	private void CoreLayout() {
		var c = innerPanel.Controls;
		c.Clear();
		c.Add(decLabel);
		c.Add(decimalBox);
		c.Add(expBox);
		var y = expBox.Bottom + Pad;
		var tab = 1;
		/*for (int i = 0; i < CustomFunctions.Count; ++i, y += rowHeight + pad) {
			var row = CustomFunctions[i];
			row.Def.Top = row.Del.Top = y;
			row.Def.TabIndex = ++tab;
			row.Del.TabIndex = ++tab;
			c.Add(row.Def);
			c.Add(row.Del);
		}
		expBox.Top = y;
		expBox.TabIndex = ++tab;
		y += rowHeight + pad;*/
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
		logPanel.Height = codeBox.Height = innerPanel.Height - (logPanel.Top = codeBox.Top = y) - Pad;
		/*var (fdelL, defW) = FuncDim();
		for (int i = 0; i < CustomFunctions.Count; ++i) {
			var row = CustomFunctions[i];
			row.Def.Width = defW;
			row.Del.Left = fdelL;
		}*/
		var (expW, resW, edelL, swapL) = ExpDim();
		for (var i = 0; i < _expressionRows.Count; ++i) {
			var row/*(_, exp, res, del)*/ = _expressionRows[i];
			row.Expression.Width = expW;
			row.Result.Width = resW;
			row.Del.Left = edelL;
			if (i < _expressionRows.Count - 1)
				_swaps[i].Left = swapL;
		}
		c.Add(codeBox);
		codeBox.TabIndex = ++tab;
		c.Add(logPanel);
	}
	private void MakeLayout() {
		//CodeBox_TextChanged(null, new());
		innerPanel.Visible = false;
		SetMinSize();
		innerPanel.SuspendLayout();
		CoreLayout();
		var d = innerPanel.MinimumSize.Height - outerPanel.Height + 6;
		if (d > 0) 
			Height += d;
		innerPanel.ResumeLayout(false);
		innerPanel.Visible = true;
	}
	/*private (int delL, int defW) FuncDim() => (
		innerPanel.Width - rowHeight - pad,
		innerPanel.Width - rowHeight - (pad << 1) - pad);*/
	/*private void FuncAdd(object? sender, EventArgs e) {
		var i = CustomFunctions.Count;
		string si = i.ToString();
		var a = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
		FuncRow row = new(
			new() {
				Name = "def" + si,
				Text = "1+2x", Anchor = a,
				Location = new(pad, 0),
				Size = new(0, rowHeight)
			},
			new() {
				Name = "delfunc" + si,
				Text = "X",
				BackColor = Color.LightGray,
				Anchor = AnchorStyles.Right | AnchorStyles.Top,
				Location = new(0, 0),
				Size = new(rowHeight, rowHeight)
			});
		row.Def.TextChanged += FuncChanged;
		row.Del.Click += FuncDeleted;
		CustomFunctions.Add(row);
		MakeLayout();
	}*/
	private (int expW, int resW, int delL, int swapL) ExpDim() => (
		innerPanel.Width - InputSize - (RowHeight << 1) - (Pad << 2) - Pad,
		innerPanel.Width - (RowHeight << 1) - (((Pad << 1) + Pad) << 1),//innerPanel.Width - rSize - (rowHeight << 1) - pad - (pad << 1),
		innerPanel.Width - ((RowHeight + Pad) << 1),
		innerPanel.Width - RowHeight - Pad);
	private void ExpAdd(object? sender, EventArgs e) {
		var i = _expressionRows.Count;
		string si = i.ToString();
		var a = AnchorStyles.Top;
		ExpRow row = new(
			new() {
				Name = "index" + si,
				Text = "x=" + si + ":",
				AutoSize = true,
				ForeColor = Color.White,
				Font = new Font("Consolas", RowHeight >> 1),
				Anchor = a | AnchorStyles.Left,
				Location = new(Pad, 0),
				Size = new(20, RowHeight)
			},
			new() {
				Name = "exp" + si, Text = "1+2x",
				Anchor = a | AnchorStyles.Left | AnchorStyles.Right,
				Tag = i,
				Location = new(InputSize + 2 * Pad, 0),
				Size = new(0, RowHeight)
			},
			new() {
				Name = "result" + si,
				AutoSize = true,
				ForeColor = Color.White,
				Font = new Font("Consolas", RowHeight >> 1),
				Anchor = a | AnchorStyles.Left | AnchorStyles.Right,
				Location = new(Pad, 0),
				Size = new(0, RowHeight)
			},
			new() {
				Name = "delexp" + si,
				Text = "X",
				BackColor = Color.LightGray,
				Anchor = a | AnchorStyles.Right,
				Tag = i,
				Location = new(0, 0),
				Size = new(RowHeight, (RowHeight << 1) + Pad)
			});
		if (i > 0) {
			Button swap = new() {
				Name = "swap" + si,
				Text = "🗘", // "↕"
				BackColor = Color.LightGray,
				Anchor = a | AnchorStyles.Right,
				Tag = i - 1,
				Location = new(0, 0),
				Size = new(RowHeight, (RowHeight << 1) + Pad)
			};
			swap.Click += ExpSwapped;
			_swaps.Add(swap);
		}
		row.Expression.TextChanged += ExpChanged;
		row.Del.Click += ExpDeleted;
		_expressionRows.Add(row);
		MakeLayout();
		Eval(i);
	}
	/*private void FuncChanged(object? sender, EventArgs e) {
		if (innerPanel.Visible)
			SetFunc();
	}*/
	private void ExpChanged(object? sender, EventArgs e) => Eval(((int?)((Control?)sender)?.Tag) ?? 0);

	private void ExpSwapped(object? sender, EventArgs e) {
		int s = ((int?)((Control?)sender)?.Tag) ?? 0;
		// swap with invisible panel, so they don't trigger reevaluations mid-swap
		innerPanel.Visible = false;
		var rowA = _expressionRows[s];
		var rowB = _expressionRows[s + 1];
		(rowA.Expression.Text, rowA.Result.Text, rowA.Exp, rowA.Text, rowB.Expression.Text, rowB.Result.Text, rowB.Exp, rowB.Text) 
			= (rowB.Expression.Text, rowB.Result.Text, rowB.Exp,rowB.Text, rowA.Expression.Text, rowA.Result.Text, rowA.Exp, rowA.Text);
		innerPanel.Visible = true;
		// reevaluate with swapped indices
		Eval(s);
		Eval(s + 1);
	}
	private void ExpDeleted(object? sender, EventArgs e) {
		int d = ((int?)((Control?)sender)?.Tag) ?? 0;
		// delete with invisible panel, so they don't trigger reevaluations mid-delete
		innerPanel.Visible = false;
		innerPanel.SuspendLayout();
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
		SetMinSize();
		CoreLayout();
		innerPanel.ResumeLayout(false);
		//outerPanel.ResumeLayout(false);
		innerPanel.Visible = true;
		//outerPanel.PerformLayout();
		//innerPanel.PerformLayout();
	}
	/*private void FuncDeleted(object? sender, EventArgs e) {
		int d = ((int?)((Control?)sender)?.Tag) ?? 0;
		// delete with invisible panel, so they don't trigger reevaluations mid-delete
		innerPanel.Visible = false;
		// shake down texts
		for (int i = d + 1; i < CustomFunctions.Count; ++i) 
			CustomFunctions[i - 1].Def = CustomFunctions[i].Def;
		// remove controls
		CustomFunctions.RemoveAt(CustomFunctions.Count - 1);
		// remake layout and re-evaluate:
		innerPanel.Visible = true;
		MakeLayout();
	}*/
	//private void ScrollPanel_Resize(object sender, EventArgs e) => innerPanel.Size = new(outerPanel.Width - 6, outerPanel.Height - 6);
	private static string Clean(string t) // forbidden symbols in expressions
		=> t.ToLower()//.Replace(":", "").Replace(";", "").Replace("|", "")
		.Replace("\t", "").Replace("\r", "").Replace("\n", "");
	private void SetMinSize() => outerPanel.AutoScrollMinSize = (innerPanel.MinimumSize = new Size(
		Math.Max((Pad << 2) + Pad + 48 + (RowHeight << 1) + InputSize, 320),
		Pad + (3 + (_expressionRows.Count << 1)) * (RowHeight + Pad))
		) + new Size(6, 6); // account for the padding between the two panels

	private void DecimalBox_TextChanged(object? sender, EventArgs e) {
		var old = _decimals;
		_ = int.TryParse(decimalBox.Text, out _decimals);
		if (old != _decimals)
			for (int i = 0; i < _expressionRows.Count; ++i)
				Eval(i);
	}
	private void AlgebraBox_SelectedIndexChanged(object? sender, EventArgs e) {
		_algebra = algebraBox.SelectedIndex;
		_context = _algebras[_algebra];
	}
}
