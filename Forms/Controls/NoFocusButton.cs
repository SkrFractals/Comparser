namespace Comparser.Forms.Controls;
public class NoFocusButton : Button {
	override protected void WndProc(ref Message m) {
		if (m.Msg == 0x0021) {
			m.Result = 3;
			return;
		}
		base.WndProc(ref m);
	}
}