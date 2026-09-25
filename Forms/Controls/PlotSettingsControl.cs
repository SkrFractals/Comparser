namespace Comparser.Forms.Controls;

public partial class PlotSettingsControl : UserControl {
	// TODO find a way so that click Buttons won't take away the focus from RichTextBoxes
	public bool IsVisible = true;
	public PlotSettingsControl() => InitializeComponent();
	public void Block() { SetBlock(true); buildButton.Text = "CANCEL"; }
	public void Unblock() { SetBlock(false); buildButton.Text = "OK"; }
	private void SetBlock(bool b) {
		//animatedBox.Enabled = prevButton.Enabled = nextButton.Enabled = loadButton.Enabled = saveButton.Enabled = !b;
		animatedBox.Visible = 
		itfBox.Visible = itlBox.Visible =
			prevButton.Visible = nextButton.Visible = loadButton.Visible = saveButton.Visible = saveSelect.Visible = IsVisible = !b;
		SetBlock(panel, /*itfBox.ReadOnly = itlBox.ReadOnly =*/ b);
	}
	private void SetBlock(object c, bool r) {
		if (c is not Control cr)
			return;
		cr.Visible = !r;//cr.Tag = r;
		foreach (var ch in cr.Controls)
			SetBlock(ch, r);
	}
}
