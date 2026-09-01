namespace Comparser.Forms;
public partial class ParentControl : UserControl {
	protected ParentControl() {
		InitializeComponent();
	}
	protected const int RowHeight = 32, Pad = 3;
	protected readonly MenuControl? Root;
	public readonly ParentForm? FormP;
	protected ParentControl(MenuControl? root, ParentForm parent) {
		Root = root ?? (MenuControl)this;
		(FormP = parent).Attach(this);
		InitializeComponent();
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
}