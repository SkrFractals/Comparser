using Comparser.Forms.Core;
using static Comparser.Forms.Core.IPanel;

namespace Comparser.Forms;
public partial class LogPanel : UserControl, IPanel {
	#region IPanel
	private readonly ControlVar _var;
	public ControlVar GetVar() => _var;
	public Size GetSize() => new(64, 64);
	public void SetDark(bool dark) => BaseSetDark(this);
	public void PerformClose() { }
	public void CoreLayout() { }
	#endregion

	#region Constructors
	public LogPanel() => InitializeComponent();
	public LogPanel(MenuPanel root, ParentForm parent) : this() 
		=> InitVar(ref _var, this, root, parent, "Comparser -  Logs");
	#endregion

	#region Actions
	public void Transfer(string? copy) {
		_ = ComparserPanel.NativeMethods.SendMessage(logBox.Handle, ComparserPanel.NativeMethods.WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
		logBox.SuspendLayout();
		try {
			logBox.Rtf = copy;
		} finally {
			_ = ComparserPanel.NativeMethods.SendMessage(logBox.Handle, ComparserPanel.NativeMethods.WmSetRedraw, new IntPtr(1), IntPtr.Zero);
			logBox.Invalidate();
			logBox.ResumeLayout();
		}
	}
	#endregion
}