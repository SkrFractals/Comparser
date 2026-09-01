namespace Comparser.Forms;
public partial class LogControl : ParentControl {
	public LogControl() => InitializeComponent();
	public LogControl(MenuControl root, ParentForm parent) : base(root, parent) {
		InitializeComponent();
		parent.SetMinSize();
	} 
	public override Size GetSize() => new(64, 64);
	/*public override void SetDark(bool dark) {
		var bf = dark ? (Color.Black, Color.White) : (Color.White, Color.Black);
		logBox.BackColor = bf.Item1;
		logBox.ForeColor = bf.Item2;
	}*/
	public void Transfer(string? box) {
		logBox.Rtf = box;
	}
}