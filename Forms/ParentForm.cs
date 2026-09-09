namespace Comparser.Forms;

public partial class ParentForm : Form {
	private ParentControl? _myControl;
	public bool ActuallyClose = false; // plot should set it to true
	public ParentForm() => InitializeComponent();

	public void Attach(ParentControl control) => innerPanel.Controls.Add(_myControl = control);
	public void MakeLayout() {
		innerPanel.Visible = false;
		SetMinSize();
		innerPanel.SuspendLayout();
		_myControl?.CoreLayout();
		//var d = innerPanel.MinimumSize.Height - outerPanel.Height + 6;if (d > 0) Height += d; // this messes up the layout width
		innerPanel.ResumeLayout(false);
		innerPanel.Visible = true;
	}
	public void SetMinSize() => outerPanel.AutoScrollMinSize = (innerPanel.MinimumSize = _myControl?.GetSize() ?? new(0,0)) + new Size(6, 6); // account for the padding between the two panels
	
	public void SetDark(bool dark) => _myControl?.SetDark(dark);
	private void ParentForm_FormClosing(object sender, FormClosingEventArgs e) {
		if (ActuallyClose || e.CloseReason != CloseReason.UserClosing) 
			return;
		e.Cancel = true;
		Hide();
	}
}
