namespace Comparser;
public partial class MenuControl : ParentControl {
	private readonly ParentForm? _setForm, _codeForm, _expForm;
	public readonly ComparserControl? Code;
	public readonly SettingsControl? Set;
	public readonly ExpressionControl? Exp;
	public MenuControl() => InitializeComponent();
	public MenuControl(MenuControl? root, ParentForm parent) : base(root, parent) {
		InitializeComponent();
		Code = new(this, _codeForm = new());
		Exp = new(this, _expForm = new());
		Set = new(this, _setForm = new());
		parent.SetMinSize();
	}
	private void SetButton_Click(object sender, EventArgs e) {
		_setForm?.Show();
		_setForm?.Location = Location;
	}
	private void CodeButton_Click(object sender, EventArgs e) {
		_codeForm?.Show();
		_codeForm?.Location = Location;
	}
	private void ExpButton_Click(object sender, EventArgs e) {
		_expForm?.Show();
		_expForm?.Location = Location;
	}
	private void PlotButton_Click(object sender, EventArgs e) {
		//throw new System.NotImplementedException();
	}
	public override Size GetSize() => new(64, Pad + 4 * (Pad+RowHeight));
	public override void SetDark(bool dark) {
		if (dark) 
			SetColors(Color.Black, Color.White);
		else 
			SetColors(Color.White, Color.Black);
		_expForm?.SetDark(dark);
		_setForm?.SetDark(dark);
		_codeForm?.SetDark(dark);
		return;
		void SetColors(Color back, Color fore) {
			codeButton.BackColor = expButton.BackColor = setButton.BackColor = plotButton.BackColor = back;
			codeButton.ForeColor = expButton.ForeColor = setButton.ForeColor = plotButton.ForeColor = fore;
		}
	}
}