namespace Comparser.Forms;
public partial class MenuControl : ParentControl {
	private readonly ParentForm? _setForm, _codeForm, _expForm;
	public readonly ParentForm? LogForm;
	public readonly ComparserControl? Code;
	public readonly LogControl? Log;
	public readonly SettingsControl? Set;
	public readonly ExpressionControl? Exp;
	public MenuControl() => InitializeComponent();
	public MenuControl(MenuControl? root, ParentForm parent) : base(root, parent) {
		InitializeComponent();
		Log = new(this, LogForm = new());
		Code = new(this, _codeForm = new());
		Exp = new(this, _expForm = new());
		Set = new(this, _setForm = new());
		var m = FormStartPosition.Manual;
		_codeForm?.StartPosition = m;
		_expForm?.StartPosition = m;
		_setForm?.StartPosition = m;
		parent.SetMinSize();
		parent.Text = "Comparser - Complex Computer Parser";
		}
	private void SetButton_Click(object sender, EventArgs e) => ShowC(_setForm, FormP);
	private void CodeButton_Click(object sender, EventArgs e) => ShowC(_codeForm, FormP);
	private void ExpButton_Click(object sender, EventArgs e) => ShowC(_expForm, FormP);
	public void ShowC(ParentForm? f, ParentForm? p) {
		if (f == null)
			return;
		f.Location = p?.Location ?? new();
		if(!f.Visible)
			f.Show(this);
	}
	private void PlotButton_Click(object sender, EventArgs e) {
		//throw new System.NotImplementedException();
	}
	public override Size GetSize() => new(120, Pad + 4 * (Pad + RowHeight));
	public override void SetDark(bool dark) {
		base.SetDark(dark);
		/*var bf = dark ? (Color.Black, Color.White) : (Color.White, Color.Black);
		codeButton.BackColor = expButton.BackColor = setButton.BackColor = plotButton.BackColor = bf.Item1;
		codeButton.ForeColor = expButton.ForeColor = setButton.ForeColor = plotButton.ForeColor = bf.Item2;*/
		_expForm?.SetDark(dark);
		_setForm?.SetDark(dark);
		_codeForm?.SetDark(dark);
		LogForm?.SetDark(dark);
	}
	public override void PerformClose() {
		LogForm?.ActuallyClose = true;
		_setForm?.ActuallyClose = true;
		_codeForm?.ActuallyClose = true;
		_expForm?.ActuallyClose = true;
	}
}