namespace Comparser.Forms.Core;

public partial class ParentForm : Form {
	private IPanel? _myControl;
	public bool ActuallyClose = false; // plot should set it to true
	public ParentForm() {
		InitializeComponent();
		FormBorderStyle = FormBorderStyle.Sizable;
		/*ControlBox = MinimizeBox = MaximizeBox = false;*/
	}
	public void Attach(IPanel control) {
		var uc = (UserControl)(_myControl = control);
		innerPanel.Controls.Add(uc);
		uc.Dock = DockStyle.Fill;
	}
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
	public Panel GetInnerPanel() => innerPanel;

	/*private const int WM_GETMINMAXINFO = 0x0024;

	// Desired minimum client width, including your padding.
	private const int MinimumClientWidth = 8;

	[StructLayout(LayoutKind.Sequential)]
	private struct POINT
	{
		public int X;
		public int Y;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct MINMAXINFO
	{
		public POINT Reserved;
		public POINT MaxSize;
		public POINT MaxPosition;
		public POINT MinTrackSize;
		public POINT MaxTrackSize;
	}
	protected override void WndProc(ref Message m)
	{
		base.WndProc(ref m);

		if (m.Msg == WM_GETMINMAXINFO)
		{
			var info = Marshal.PtrToStructure<MINMAXINFO>(m.LParam);

			// Width includes the left and right non-client borders.
			int nonClientWidth = Width - ClientSize.Width;

			info.MinTrackSize.X =
				MinimumClientWidth + nonClientWidth;

			Marshal.StructureToPtr(info, m.LParam, true);
		}
	}*/
}
