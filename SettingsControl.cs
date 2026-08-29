using Comparser.Comparser;
namespace Comparser;
public partial class SettingsControl : ParentControl {
	public int Decimals = 3;
	public int Algebra = 1;
	private readonly IComparser[] _algebras = [new ComparserR(), new ComparserC(), new ComparserQ()];
	public IComparser? Context;
	private bool _darkMode;
	public SettingsControl() => InitializeComponent();
	public SettingsControl(MenuControl root, ParentForm parent) : base(root, parent) {
		Context = _algebras[Algebra];
		InitializeComponent();
		DecimalBox_TextChanged(decimalBox, EventArgs.Empty);
		darkButton_Click(darkButton, EventArgs.Empty);
		Root?.Code?.CodeChanged = true;
		parent.SetMinSize();
	}

	public override Size GetSize() => new(
		Pad * 5 + decLabel.Width + decimalBox.Width + algebraBox.Width + darkButton.Width,
		(Pad << 1) + RowHeight + Pad);

	private void DecimalBox_TextChanged(object? sender, EventArgs e) {
		var old = Decimals;
		_ = int.TryParse(decimalBox.Text, out Decimals);
		if (old != Decimals)
			Root?.Exp?.ReEval();
	}
	private void AlgebraBox_SelectedIndexChanged(object? sender, EventArgs e) {
		Algebra = algebraBox.SelectedIndex;
		Context = _algebras[Algebra];
		Root?.Code?.CodeChanged = true; // reparse
	}
	
	private void darkButton_Click(object sender, EventArgs e) {
		Root?.SetDark(_darkMode = !_darkMode);
	}
	public override void SetDark(bool dark){
		Context?.SetDarkMode(dark);
		if (dark) { 
			SetColors(Color.Black, Color.White);
			darkButton.Text = "☾";
		} else {
			SetColors(Color.White, Color.Black);	
			darkButton.Text = "☀︎";
		}
		return;
		void SetColors(Color back, Color fore) {
			decimalBox.BackColor = algebraBox.BackColor = darkButton.BackColor = back;
			decimalBox.ForeColor = algebraBox.ForeColor = darkButton.ForeColor = fore;
		}
	}
}