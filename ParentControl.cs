namespace Comparser;
public partial class ParentControl : UserControl {
	public ParentControl() {
		InitializeComponent();
	}
	protected const int RowHeight = 32, Pad = 2;
	protected readonly MenuControl? Root;
	protected readonly ParentForm? _parent;
	public ParentControl(MenuControl? root, ParentForm parent) {
		Root = root ?? (MenuControl)this;
		(_parent = parent).Attach(this);
		InitializeComponent();
	}
	public virtual void CoreLayout() { }
	public virtual Size GetSize() => new(0,0);
	public virtual void SetDark(bool dark) { }
	private void ParentControl_Load(object sender, EventArgs e) => Anchor = AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
}