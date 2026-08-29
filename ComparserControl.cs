namespace Comparser;
public partial class ComparserControl : ParentControl {
	public ComparserControl() => InitializeComponent();
	public ComparserControl(MenuControl root, ParentForm parent) : base(root, parent){
		InitializeComponent();
	}
	private const int InputSize = 64;
	public bool CodeChanged;
	private bool _editingText = true, _parsing, _parsingFinished;
	private List<(int position, Color color)> _colors = [];
	private List<(Color color, string log)> _logs = [];
	private string _toParse = "";
	private void CodeBox_TextChanged(object? sender, EventArgs e) {
		CodeChanged |= _editingText;
	}
	private void Fps_Tick(object? sender, EventArgs e) {
		if (_parsingFinished)
			FinishParse();
		if (!CodeChanged || _parsing)
			return;
		CodeChanged = false;
		_parsing = true;
		_toParse = codeBox.Text;
		Task.Run(Parse);
	}
	private void Parse() {
		_logs = Root?.Set?.Context?.ReadCode(_toParse, out _colors) ?? [];
		_parsingFinished = true;
		_parsing = false;
	}
	private void FinishParse() {
		_editingText = false;
		_parsingFinished = false;
		int oldSelectionStart = codeBox.SelectionStart, oldSelectionLength = codeBox.SelectionLength;
		for (var i = 0; i < _colors.Count; ++i) {
			int from = _colors[i].position,
				to = i + 1 < _colors.Count ? _colors[i + 1].position : codeBox.TextLength;
			if (from >= to)
				continue;
			codeBox.Select(from, to - from);
			codeBox.SelectionColor = _colors[i].color;
		}
		codeBox.Select(oldSelectionStart, oldSelectionLength);
		// append logs (errors, prints...)
		logBox.SelectionStart = 0;
		logBox.SelectionLength = 0;
		logBox.Text = "";
		foreach (var t in _logs) {
			logBox.SelectionColor = t.color;
			logBox.AppendText(t.log + "\n");
		}
		// evaluate expression fields with this newly parsed program
		Root?.Exp?.ReEval();
		_editingText = true;
	}
	public override Size GetSize() => new((Pad << 1) + 64, Pad + 2 * (RowHeight + Pad));
	public override void SetDark(bool dark) {
		if (dark) { 
			SetColors(Color.Black, Color.White);
		} else {
			SetColors(Color.White, Color.Black);	
		}
		void SetColors(Color back, Color fore) {
			codeBox.BackColor = logBox.BackColor = back;
			codeBox.ForeColor = logBox.ForeColor = fore;
		}
		FinishParse(); // recolor
	}
}