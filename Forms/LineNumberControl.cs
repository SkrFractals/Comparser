namespace Comparser.Forms;
public sealed partial class LineNumberControl : UserControl {
	private readonly RichTextBox? _textBox;
	public LineNumberControl() => InitializeComponent();
	public LineNumberControl(RichTextBox? textBox) {
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
		BackColor = SystemColors.Control;
		ForeColor = SystemColors.ControlDarkDark;
		_textBox = textBox;
	}
	override protected void OnPaint(PaintEventArgs e) {
		base.OnPaint(e);
		if (_textBox == null)
			return;
		var lastLine = 1 + _textBox.GetLineFromCharIndex(_textBox.GetCharIndexFromPosition(new(0, _textBox.ClientSize.Height)));
		using var brush = new SolidBrush(ForeColor);
		for (var line = _textBox.GetLineFromCharIndex(_textBox.GetCharIndexFromPosition(new(0, 0))); line <= lastLine; line++) {
			var charIndex = _textBox.GetFirstCharIndexFromLine(line);
			if (charIndex < 0)
				break;
			var number = (line + 1).ToString();
			e.Graphics.DrawString(number, _textBox.Font, brush, ClientSize.Width - e.Graphics.MeasureString(number, _textBox.Font).Width - 5, _textBox.GetPositionFromCharIndex(charIndex).Y);
		}
	}
}