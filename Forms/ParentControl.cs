using Comparser.Comparser;
namespace Comparser.Forms;
public partial class ParentControl : UserControl {
	protected ParentControl() => InitializeComponent();
	protected const int RowHeight = 32, Pad = 3;
	protected readonly MenuControl? Root;
	public readonly ParentForm? FormP;
	protected ParentControl(MenuControl? root, ParentForm parent) {
		Root = root ?? (MenuControl)this;
		(FormP = parent).Attach(this);
		InitializeComponent();
	}
	public class TextField {
		public TextField(RichTextBox box, EventHandler textChanged, string text = "") {
			Exp = null;
			Value = null;
			Text = text;
			ComparserControl.InitRichTextBox(Box = box, textChanged);
		}
		public object? Exp;
		public object? Value;
		public string Text;
		public RichTextBox Box;
	}
	public virtual void CoreLayout() { }
	public virtual Size GetSize() => new(0,0);
	public virtual void SetDark(bool dark) {
		//var c = dark ? (Root.Set.Context.GetColor(ParseDictionary.Type.Back), Color.White) : (Color.White, Color.Black);
		DarkC(Controls, Root?.Set?.Context?.GetColor() ?? (Color.Black, Color.White));
	}
	private void DarkC(ControlCollection c, (Color back, Color fore) color) {
		foreach (var o in c) {
			if (o is not Control oc)
				continue;
			switch (o) {
			case Button: case TextBox: case ComboBox: case Basic.CodeControl: case RichTextBox: case LineNumberControl:
				oc.BackColor = color.back;
				oc.ForeColor = color.fore;
				break;
			case Label:
				oc.ForeColor = color.back;
				break;
			case Panel p:
				DarkC(p.Controls, color);
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
	private void ParentControl_Load(object sender, EventArgs e) {
		FormP?.MaximumSize = MinimumSize = MaximumSize = new(0, 0);
		FormP?.MinimumSize = new(240, 160);
		Dock = DockStyle.Fill;
		Anchor = AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
	}
	public virtual void PerformClose() { }
	protected object? Eval(TextField field, bool cachedParse = true, object? args = null, CancellationToken? cancel = null)
		=> !Visible || !Enabled ? null : Eval(Root?.Set?.Context, field, cachedParse, args, cancel);
	public static object? Eval(IComparser? c, TextField field, bool cachedParse = true, object? args = null, CancellationToken? cancel = null) { // TODO call re-eval together with expressionControl
		if (c is null)
			return null;
		if (cachedParse && field.Exp != null && field.Text == field.Box.Text) 
			return field.Value = c.Eval(field.Exp, args);
		field.Value =  c.ParseEval(cancel ?? new CancellationTokenSource().Token, field.Text = field.Box.Text, 0,out field.Exp, out var colors, args);
		ComparserControl.ApplyColors(colors, field.Box);
		return field.Value;
	}
}