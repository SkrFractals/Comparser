using Comparser.Comparser;
namespace Comparser.Forms;
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
		Root?.Set?.Context?.SetDecimals(Root.Set.Decimals);
		if (old != Decimals)
			Root?.Code?.CodeChanged = true; // re-parse so the prints and expressions update
			//Root?.Exp?.ReEval();
	}
	private void AlgebraBox_SelectedIndexChanged(object? sender, EventArgs e) {
		Algebra = algebraBox.SelectedIndex;
		Context = _algebras[Algebra];
		Root?.Code?.CodeChanged = true; // reparse
	}
	
	private void darkButton_Click(object sender, EventArgs e) {
		Context?.SetDarkMode(_darkMode = !_darkMode);
		Root?.SetDark(_darkMode);
	}
	public override void SetDark(bool dark) {
		base.SetDark(dark);
		/*var bf = dark ? (Color.Black, Color.White,  "☾") : (Color.White, Color.Black, "☀︎");
		decimalBox.BackColor = algebraBox.BackColor = darkButton.BackColor =  bf.Item1;
		decimalBox.ForeColor = algebraBox.ForeColor = darkButton.ForeColor =  bf.Item2;
		Context?.SetDarkMode(dark);*/
		darkButton.Text =  dark ? "☾" : "☀︎";
	}
}