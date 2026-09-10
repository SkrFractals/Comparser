namespace Comparser.Forms.Controls;
public partial class PlotSettingsControl : UserControl {
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
	/*public RichTextBox IxsBox => ixsBox;
public RichTextBox IxcBox => ixcBox;
public RichTextBox IxeBox => ixeBox;
public RichTextBox IysBox => iysBox;
public RichTextBox IycBox => iycBox;
public RichTextBox IyeBox => iyeBox;
public RichTextBox ItsBox => itsBox;
public RichTextBox ItcBox => itcBox;
public RichTextBox IteBox => iteBox;
public RichTextBox OysBox => oysBox;
public RichTextBox OycBox => oycBox;
public RichTextBox OyeBox => oyeBox;
public RichTextBox ItfBox => itfBox;
public RichTextBox ItlBox => itlBox;
public RichTextBox FyBox => fyBox;
public Button PrevButton => prevButton;
public Button NextButton => nextButton;
public Button AddButton => addButton;
public ComboBox OutputSelect => outputSelect;
public ComboBox ModeSelect => modeSelect;
public RichTextBox CodeBox => codeBox;
public RichTextBox WidthBox => widthBox;
public RichTextBox HeightBox => heightBox;
public CheckBox AnimatedBox => animatedBox;*/

}
