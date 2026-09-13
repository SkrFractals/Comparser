namespace Comparser.Forms.Controls;

public partial class PlotSettingsControl : UserControl {
    // TODO find a way so that click Buttons won't take away the focus from RichTextBoxes
    public PlotSettingsControl() => InitializeComponent();
    public void Block() { SetBlock(true); buildButton.Text = "CANCEL"; }
    public void Unblock() { SetBlock(false); buildButton.Text = "OK"; }
    private void SetBlock(bool b) {
        prevButton.Enabled = nextButton.Enabled = loadButton.Enabled = saveButton.Enabled = !b;
        SetBlock(panel, itfBox.ReadOnly = itlBox.ReadOnly = b);
    }
    private void SetBlock(object c, bool r) {
        if (c is not Control cr)
            return;
        cr.Tag = r;
        foreach (var ch in cr.Controls)
            SetBlock(ch, r);
    }
}
