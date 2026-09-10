using Comparser.Comparser;
using Comparser.Forms.Controls;
using static System.Windows.Forms.Control;
namespace Comparser.Forms.Core;

public interface IPanel{
	protected const int RowHeight = 27, Pad = 3;
	//protected readonly MenuControl? Root;
	//public readonly ParentForm? FormP;
	//protected UserControl MyPanel;
	//protected ParentControl(MenuControl? root, UserControl myPanel, ParentForm parent) : this() {
	//Root = root ?? (MenuControl)this;
	//(FormP = parent).Attach(this);
	//}
	public static void InitVar(ref ControlVar var, IPanel self, MenuPanel root, ParentForm form, string title) {
		var = new(root, form);
		self.AttachToParent();
		form.SetMinSize();
		form.Text = title;
	}
	public void AttachToParent() => GetVar().Form.Attach(this);
	public void CoreLayout();
	public Size GetSize();
	//public ParentForm GetForm() => GetVar().Form;
	public ControlVar GetVar();
	public class TextField {
		public TextField() => Box = new();
		public TextField(RichTextBox box, EventHandler textChanged, string text = "", string[]? args = null) {
			Exp = null;
			Value = null;
			Text = text;
			Args = args ?? [];
			ComparserPanel.InitRichTextBox(Box = box, textChanged);
		}
		public readonly string[] Args = [];
		public object? Exp;
		public object? ArgsV;
		public object? Value;
		public string Text = "";
		public readonly RichTextBox Box;
	}
	public readonly struct ControlVar(MenuPanel root, ParentForm form) {
		public readonly ParentForm Form = form;
		public readonly MenuPanel Root = root;
	}

	public static void BaseSetDark(IPanel self) => self.DarkC(((UserControl)self).Controls, SettingsPanel.Context?.GetColor() ?? (Color.Black, Color.White));
	public void SetDark(bool dark);
		
	private void DarkC(ControlCollection c, (Color back, Color fore) color) {
		foreach (var o in c) {
			if (o is not Control oc)
				continue;
			switch (o) {
				case Button:
				case TextBox:
				case ComboBox:
				case RichTextBox:
				case LineNumberControl:
					oc.BackColor = oc.BackColor == Color.Red ? Color.Red : color.back;
					oc.ForeColor = color.fore;
					DarkC(oc.Controls, color);
					break;
				case Label:
					oc.ForeColor = color.back;
					break;
				case Panel:
				case PlotSettingsControl:
					DarkC(oc.Controls, color);
					break;
				case SplitContainer s:
					oc.BackColor = color.fore;
					s.Panel1.BackColor = color.back;
					s.Panel2.BackColor = color.back;
					DarkC(s.Panel1.Controls, color);
					DarkC(s.Panel2.Controls, color);
					break;
			}
		}
	}
	/*private void ParentControl_Load(object sender, EventArgs e) {
		MinimumSize = MaximumSize = new(0, 0);
		FormP?.MaximumSize = new(0, 0);
		FormP?.MinimumSize = new(240, 160);
		Dock = DockStyle.Fill;
		Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left;
	}*/
	public void PerformClose();
	public static object? Eval(IPanel self, TextField field, bool cachedParse = true, object? args = null, CancellationToken? cancel = null)
		=> self is Control { Enabled: true } && field.Box is { Enabled: true, ReadOnly: false, Tag: not true } ? Eval(SettingsPanel.Context, field, cachedParse, args, cancel) : null;
	public static object? Parse(TextField field, bool cachedParse = true, object? args = null, CancellationToken? cancel = null)
		=> Parse(SettingsPanel.Context, field, cachedParse, args, cancel);
	public static object? Eval(IComparser? c, TextField field, bool cachedParse = true, object? args = null, CancellationToken? cancel = null) { 
		// TODO call re-eval together with expressionControl
		if (c == null)
			return null;
		if (cachedParse && field.Exp != null && field.Text == field.Box.Text)
			return field.Value = c.Eval(field.Exp, args);
		field.Value = c.ParseEval(cancel ?? new CancellationTokenSource().Token, field.Text = field.Box.Text, 0, out field.Exp, out var colors, PrepareArgs(c, field, args));
		ComparserPanel.ApplyColors(colors, field.Box);
		return field.Value;
	}
	public static object? Parse(IComparser? c, TextField field, bool cachedParse = true, object? args = null, CancellationToken? cancel = null) { 
		// TODO call re-eval together with expressionControl
		if (c is null || cachedParse && field.Exp != null && field.Text == field.Box.Text)
			return null;
		field.Exp = c.Parse(cancel ?? new CancellationTokenSource().Token, field.Text = field.Box.Text, 0, out var colors, PrepareArgs(c, field, args));
		ComparserPanel.ApplyColors(colors, field.Box);
		return field.Exp;
	}
	private static object? PrepareArgs(IComparser? c, TextField field, object? args) => args == null && field.Args.Length > 0 ? (field.ArgsV ??= c?.MakeArgs(field.Args)) : args;

}
