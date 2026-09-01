using System.Diagnostics.CodeAnalysis;
namespace Comparser.Basic;
public class CodeControl : RichTextBox {
	public CodeControl() {
		DetectUrls = false;
		Multiline = true;
		_myFont = new("Consolas", 12, FontStyle.Bold);
		Font = new("Consolas", 12,  FontStyle.Bold);
	}
	private Font _myFont;
	public override sealed bool Multiline { get => base.Multiline; set => base.Multiline = value; }
	[AllowNull] public override sealed Font Font { get => base.Font; set => base.Font = value; }
	override protected void WndProc(ref Message m) {
		const int WM_PASTE = 0x0302;
		if (m.Msg == WM_PASTE) {
			PastePlainText();
			return;
		}
		base.WndProc(ref m);
	}
	private void PastePlainText() {
		var text = Clipboard.GetText();
		if (string.IsNullOrEmpty(text))
			return;
		var start = SelectionStart;
		Select(start, (SelectedText = text).Length);
		SelectionFont = _myFont;
		Select(start + text.Length, 0);
	}
}