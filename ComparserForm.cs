using Comparser.Numbers;

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
		public object? Exp = null;
		public string Text = "";
		public Label Index = index;
		public TextBox Expression = expression;
		public Label Result = result; 
		public Button Del = del;
	}

	private const int rowHeight = 32, iSize = 64, pad = 2;
	private int decimals;
	int algebra = 1;
	private readonly IComparser[] algebras = [new ComparserR(), new ComparserC(), new ComparserQ()];
	private IComparser _context;
	//private readonly List<FuncRow> CustomFunctions = [];
	private readonly List<ExpRow> ExpressionRows = [];
	private readonly List<Button> swaps = [];
	public ComparserForm() {
		_context = algebras[algebra];
		//FailedFunction = new(_context, "", [([], "")]);
		InitializeComponent();
		SetMinSize();
		DecimalBox_TextChanged(decimalBox, new());
	}
	private void Eval(int index) {
		if (innerPanel.Visible) {
			var row = ExpressionRows[index];
			object eval = algebra switch {
				1 => new Comparser<Complex>.Value([new(Complex.MakeR(index), "x")]),
				2 => new Comparser<Quaternion>.Value([new(Quaternion.MakeR(index), "x")]),
				_ => new Comparser<Real>.Value([new(Real.MakeR(index), "x")])
			};
			var v = row.Exp != null && row.Text == row.Expression.Text && row.Text != "" 
				? _context.Eval(row.Exp, eval) 
				: _context.ParseEval(row.Text = row.Expression.Text = Clean(row.Expression.Text), out row.Exp, eval);
			row.Result.Text = v.ToString();
		}
	}
	private void CodeBox_TextChanged(object? sender, EventArgs e) {
		logLabel.Text = _context.ReadCode(codeBox.Text);
		for (int i = 0; i < ExpressionRows.Count; ++i)
			Eval(i);
	}
	private void CoreLayout() {
		var c = innerPanel.Controls;
		c.Clear();
		c.Add(decLabel);
		c.Add(decimalBox);
		c.Add(expBox);
		int y = expBox.Bottom + pad;
		int tab = 1;
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
		for (int i = 0; i < ExpressionRows.Count; ++i, y += (rowHeight + pad) << 1) {
			var row = ExpressionRows[i];
			row.Index.Top = row.Expression.Top = row.Del.Top = y;
			row.Result.Top = y + rowHeight + pad;
			row.Expression.TabIndex = ++tab;
			row.Del.TabIndex = ++tab;
			if (i < ExpressionRows.Count - 1) {
				var s = swaps[i];
				s.Top = y + rowHeight + (pad >> 1);
				s.TabIndex = ++tab;
				c.Add(s);
			}
			c.Add(row.Index);
			c.Add(row.Expression);
			c.Add(row.Result);
			c.Add(row.Del);
		}
		logPanel.Height = codeBox.Height = innerPanel.Height - (logPanel.Top = codeBox.Top = y) - pad;
		/*var (fdelL, defW) = FuncDim();
		for (int i = 0; i < CustomFunctions.Count; ++i) {
			var row = CustomFunctions[i];
			row.Def.Width = defW;
			row.Del.Left = fdelL;
		}*/
		var (expW, resW, edelL, swapL) = ExpDim();
		for (int i = 0; i < ExpressionRows.Count; ++i) {
			var row/*(_, exp, res, del)*/ = ExpressionRows[i];
			row.Expression.Width = expW;
			row.Result.Width = resW;
			row.Del.Left = edelL;
			if (i < ExpressionRows.Count - 1)
				swaps[i].Left = swapL;
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
		innerPanel.Width - iSize - (rowHeight << 1) - (pad << 2) - pad,
		innerPanel.Width - (rowHeight << 1) - (((pad << 1) + pad) << 1),//innerPanel.Width - rSize - (rowHeight << 1) - pad - (pad << 1),
		innerPanel.Width - ((rowHeight + pad) << 1),
		innerPanel.Width - rowHeight - pad);
	private void ExpAdd(object? sender, EventArgs e) {
		var i = ExpressionRows.Count;
		string si = i.ToString();
		int expt = 5 + 3 + i;
		var a = AnchorStyles.Top;
		ExpRow row = new(
			new() {
				Name = "index" + si,
				Text = "x=" + si.ToString() + ":",
				AutoSize = true,
				ForeColor = Color.White,
				Font = new Font("Consolas", rowHeight >> 1),
				Anchor = a | AnchorStyles.Left,
				Location = new(pad, 0),
				Size = new(20, rowHeight)
			},
			new() {
				Name = "exp" + si, Text = "1+2x",
				Anchor = a | AnchorStyles.Left | AnchorStyles.Right,
				Tag = i,
				Location = new(iSize + 2 * pad, 0),
				Size = new(0, rowHeight)
			},
			new() {
				Name = "result" + si,
				AutoSize = true,
				ForeColor = Color.White,
				Font = new Font("Consolas", rowHeight >> 1),
				Anchor = a | AnchorStyles.Left | AnchorStyles.Right,
				Location = new(pad, 0),
				Size = new(0, rowHeight)
			},
			new() {
				Name = "delexp" + si,
				Text = "X",
				BackColor = Color.LightGray,
				Anchor = a | AnchorStyles.Right,
				Tag = i,
				Location = new(0, 0),
				Size = new(rowHeight, (rowHeight << 1) + pad)
			});
		if (i > 0) {
			Button swap = new() {
				Name = "swap" + si,
				Text = "🗘", // "↕"
				BackColor = Color.LightGray,
				Anchor = a | AnchorStyles.Right,
				Tag = i - 1,
				Location = new(0, 0),
				Size = new(rowHeight, (rowHeight << 1) + pad)
			};
			swap.Click += ExpSwapped;
			swaps.Add(swap);
		}
		row.Expression.TextChanged += ExpChanged;
		row.Del.Click += ExpDeleted;
		ExpressionRows.Add(row);
		MakeLayout();
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
		var rowA = ExpressionRows[s];
		var rowB = ExpressionRows[s + 1];
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
		for (int i = d + 1; i < ExpressionRows.Count; ++i) {
			var rowTo = ExpressionRows[i - 1];
			var row = ExpressionRows[i];
			rowTo.Text = row.Text;
			rowTo.Expression.Text = row.Expression.Text;
			rowTo.Result.Text = row.Result.Text;
			rowTo.Exp = row.Exp;
		}
		// remove controls
		ExpressionRows.RemoveAt(ExpressionRows.Count - 1);
		if (ExpressionRows.Count > 0)
			swaps.RemoveAt(ExpressionRows.Count - 1);
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
		Math.Max((pad << 2) + pad + 48 + (rowHeight << 1) + iSize, 320),
		pad + (3 + (ExpressionRows.Count << 1)) * (rowHeight + pad))
		) + new Size(6, 6); // account for the padding between the two panels

	private void DecimalBox_TextChanged(object? sender, EventArgs e) {
		var old = decimals;
		_ = int.TryParse(decimalBox.Text, out decimals);
		if (old != decimals)
			for (int i = 0; i < ExpressionRows.Count; ++i)
				Eval(i);
	}
	private void AlgebraBox_SelectedIndexChanged(object? sender, EventArgs e) {
		algebra = algebraBox.SelectedIndex;
		_context = algebras[algebra];
	}
}
