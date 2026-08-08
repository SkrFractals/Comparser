using System.Xml.Linq;

namespace Expressions;

public partial class MainForm : Form {
	private const int rowHeight = 24, iSize = 48, pad = 2;
	private int decimals;
	private readonly Comparser _context = new();
	private readonly List<(TextBox def, Button delete)> CustomFunctions = [];
	private readonly List<(Label index, TextBox expression, Label result, Button delete)> ExpressionRows = [];
	private readonly List<Button> swaps = [];
	public MainForm() {
		InitializeComponent();
		SetMinSize();
		DecimalBox_TextChanged(decimalBox, new());
	}
	private void Eval(int index) {
		if (innerPanel.Visible) {
			var (_, e, r, _) = ExpressionRows[index];
			var (_, v) = _context.Eval(e.Text = Clean(e.Text), (["x"], [new Complex(index)]));
			string text = v[0].ToString(decimals);
			for(int i = 1; i < v.Length; ++i)
				text = text + "; " + v[i].ToString(decimals);
			r.Text = text;
		}
	}
	private static readonly Comparser.CallCustom FailedFunction = new("", [], "");
	private void SetFunc() {
		List<string> used = [];
		string NotUsed(string t) {
			int i = 0;
			string nt = t;
			while (used.Contains(nt))
				nt = t + i++.ToString();
			used.Add(nt);
			return nt;
		}
		_context.CustomFunctions = new Comparser.CallFunction[CustomFunctions.Count];
		for (int i = 0; i < CustomFunctions.Count; ++i) {
			var (def, _) = CustomFunctions[i];
			var s = def.Text.Split('=');
			int l, e;
			if (s.Length != 2 || (s[1] = Clean(s[1])).Length == 0 || (l = (s[0] = Clean(s[0])).IndexOf('(')) < 1 || (e = s[0].IndexOf(')')) < 2 || l >= e - 1 || okdef()) {
				_context.CustomFunctions[i] = FailedFunction;
				continue;
			}
			var name = NotUsed(CleanName(s[0][..l]));
			var args = CleanName(s[0][(l + 1)..e]);
			def.Text = name + "(" + args + s[0][e..] + "=" + s[1];
			_context.CustomFunctions[i] = new Comparser.CallCustom(name.Replace(" ", ""), args.Replace(" ", "").Split(","), s[1]);
			bool okdef() {
				for (int w = e + 1; ++w < s[0].Length;) if (s[0][w] != ' ') return true;
				return false;
			}
		}
		for (int i = 0; i < ExpressionRows.Count; ++i)
			Eval(i);
	}
	private void CoreLayout() {
		var c = innerPanel.Controls;
		c.Clear();
		c.Add(decLabel);
		c.Add(decimalBox);
		c.Add(funcBox);
		int y = funcBox.Bottom + 2;
		int tab = 1;
		for (int i = 0; i < CustomFunctions.Count; ++i, y += rowHeight + pad) {
			var (def, del) = CustomFunctions[i];
			def.Top = del.Top = y;
			def.TabIndex = ++tab;
			del.TabIndex = ++tab;
			c.Add(def);
			c.Add(del);
		}
		expBox.Top = y;
		expBox.TabIndex = ++tab;
		c.Add(expBox);
		y += rowHeight + pad;
		for (int i = 0; i < ExpressionRows.Count; ++i, y += (rowHeight + pad) << 1) {
			var (ind, exp, res, del) = ExpressionRows[i];
			ind.Top = exp.Top = del.Top = y;
			res.Top = y + rowHeight + pad;
			exp.TabIndex = ++tab;
			del.TabIndex = ++tab;
			if (i < ExpressionRows.Count - 1) {
				var s = swaps[i];
				s.Top = y + rowHeight + (pad >> 1);
				s.TabIndex = ++tab;
				c.Add(s);
			}
			c.Add(ind);
			c.Add(exp);
			c.Add(res);
			c.Add(del);
		}
		var (fdelL, defW) = FuncDim();
		for (int i = 0; i < CustomFunctions.Count; ++i) {
			var (def, del) = CustomFunctions[i];
			def.Width = defW;
			del.Left = fdelL;
		}
		var (expW, resW, edelL, swapL) = ExpDim();
		for (int i = 0; i < ExpressionRows.Count; ++i) {
			var (_, exp, res, del) = ExpressionRows[i];
			exp.Width = expW;
			res.Width = resW;
			del.Left = edelL;
			if (i < ExpressionRows.Count - 1)
				swaps[i].Left = swapL;
		}
	}
	private void MakeLayout() {
		SetFunc();
		innerPanel.Visible = false;
		SetMinSize();
		innerPanel.SuspendLayout();
		//outerPanel.SuspendLayout();
		CoreLayout();
		innerPanel.ResumeLayout(false);
		//outerPanel.ResumeLayout(false);
		innerPanel.Visible = true;
		//outerPanel.PerformLayout();
		//innerPanel.PerformLayout();
	}
	private (int delL, int defW) FuncDim() => (
		innerPanel.Width - rowHeight - pad,
		innerPanel.Width - rowHeight - (pad << 1) - pad);
	private void FuncAdd(object? sender, EventArgs e) {
		var i = CustomFunctions.Count;
		string si = i.ToString();
		var a = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
		(TextBox def, Button del) t = (
			new() {
				Name = "def" + si,
				Text = "1+2x", Anchor = a,
				Location = new(0, 0),
				Size = new(pad, rowHeight)
			},
			new() {
				Name = "delfunc" + si,
				Text = "X",
				BackColor = Color.LightGray,
				Anchor = AnchorStyles.Right | AnchorStyles.Top,
				Location = new(0, 0),
				Size = new(rowHeight, rowHeight)
			});
		t.def.TextChanged += FuncChanged;
		t.del.Click += FuncDeleted;
		CustomFunctions.Add(t);
		MakeLayout();
	}
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
		(Label index, TextBox expression, Label result, Button del) t = (
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
		t.expression.TextChanged += ExpChanged;
		t.del.Click += ExpDeleted;
		ExpressionRows.Add(t);
		MakeLayout();
	}
	private void FuncChanged(object? sender, EventArgs e) {
		if (innerPanel.Visible)
			SetFunc();
	}
	private void ExpChanged(object? sender, EventArgs e) => Eval(((int?)((Control?)sender)?.Tag) ?? 0);

	private void ExpSwapped(object? sender, EventArgs e) {
		int s = ((int?)((Control?)sender)?.Tag) ?? 0;
		// swap with invisible panel, so they don't trigger reevaluations mid-swap
		innerPanel.Visible = false;
		var (_, expA, resA, _) = ExpressionRows[s];
		var (_, expB, resB, _) = ExpressionRows[s + 1];
		(expA.Text, resA.Text, expB.Text, resB.Text) = (expB.Text, resB.Text, expA.Text, resA.Text);
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
			var (_, expTo, resTo, _) = ExpressionRows[i - 1];
			var (_, exp, res, _) = ExpressionRows[i];
			expTo.Text = exp.Text;
			resTo.Text = res.Text;
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
	private void FuncDeleted(object? sender, EventArgs e) {
		int d = ((int?)((Control?)sender)?.Tag) ?? 0;
		// delete with invisible panel, so they don't trigger reevaluations mid-delete
		innerPanel.Visible = false;
		// shake down texts
		for (int i = d + 1; i < CustomFunctions.Count; ++i) {
			var (defTo, _) = CustomFunctions[i - 1];
			var (def, _) = CustomFunctions[i];
			defTo.Text = def.Text;
		}
		// remove controls
		CustomFunctions.RemoveAt(CustomFunctions.Count - 1);
		// remake layout and re-evaluate:
		innerPanel.Visible = true;
		MakeLayout();
	}
	//private void ScrollPanel_Resize(object sender, EventArgs e) => innerPanel.Size = new(outerPanel.Width - 6, outerPanel.Height - 6);
	private static string Clean(string t) // forbidden symbols in expressions
	=> t.ToLower().Replace(":", "").Replace(";", "").Replace("|", "")
	.Replace("\t", "").Replace("\r", "").Replace("\n", "").Replace("\\", "");
	private static string CleanName(string t) // forbidden symbols in function names
		=> Clean(t.Replace("(", "").Replace(")", "").Replace(".", "")
			.Replace("+", "").Replace("-", "").Replace("*", "").Replace("/", "").Replace("^", ""));
	private void SetMinSize() => outerPanel.AutoScrollMinSize = innerPanel.MinimumSize = new(
		(pad << 2) + pad + 48 + (rowHeight << 1) + iSize,
		pad + (3 + CustomFunctions.Count + ExpressionRows.Count) * (rowHeight + pad));

	private void DecimalBox_TextChanged(object? sender, EventArgs e) {
		var old = decimals;
		_ = int.TryParse(decimalBox.Text, out decimals);
		if (old != decimals)
			for (int i = 0; i < ExpressionRows.Count; ++i)
				Eval(i);
	}
}
