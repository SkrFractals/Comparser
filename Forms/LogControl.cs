namespace Comparser.Forms;
public partial class LogControl : ParentControl {
	public LogControl() => InitializeComponent();
	public LogControl(MenuControl root, ParentForm parent) : base(root, parent) {
		InitializeComponent();
		parent.SetMinSize();
		parent.Text = "Comparser - Logs";
	} 
	public override Size GetSize() => new(64, 64);
	/*public override void SetDark(bool dark) {
		var bf = dark ? (Color.Black, Color.White) : (Color.White, Color.Black);
		logBox.BackColor = bf.Item1;
		logBox.ForeColor = bf.Item2;
	}*/
	public void Transfer(string? copy) {
		ComparserControl.NativeMethods.SendMessage(logBox.Handle, ComparserControl.NativeMethods.WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
		logBox.SuspendLayout();
		try {
			logBox.Rtf = copy;
		} finally {
			ComparserControl.NativeMethods.SendMessage(logBox.Handle, ComparserControl.NativeMethods.WmSetRedraw, new IntPtr(1), IntPtr.Zero);
			logBox.Invalidate();
			logBox.ResumeLayout();
		}
	}
	//public RichTextBox getLogBox => logBox;
}