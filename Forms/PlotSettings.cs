namespace Comparser.Forms;

public partial class PlotSettings : UserControl {
	public PlotSettings() => InitializeComponent();
	public void Block() { SetEnabled(false); buildButton.Text = "CANCEL"; }
	public void Unblock() { SetEnabled(true); buildButton.Text = "OK"; }
	private void SetEnabled(bool b) {
		panel.Visible = itfBox.ReadOnly = itlBox.ReadOnly = prevButton.Enabled = nextButton.Enabled = loadButton.Enabled = saveButton.Enabled = b;
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
