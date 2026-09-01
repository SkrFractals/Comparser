#define UNSAFEPARSE
namespace Comparser.Forms;
public partial class ComparserControl : ParentControl {
	public ComparserControl() => InitializeComponent();
	public ComparserControl(MenuControl root, ParentForm parent) : base(root, parent)  {
		InitializeComponent();
		splitContainer.Panel1MinSize = 3 * Pad + 2 * RowHeight;
		splitContainer.Panel2MinSize = 2 * Pad + RowHeight;
		_lines = new(codeBox);
		codeBox.Location = new(0, 0);
		codeBox.Size = new(314, 216); //splitContainer.Panel1.Size;
		_lines.Location = codeBox.Location;
		_lines.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Top;
		_lines.Height = codeBox.Height;
		_lines.Width = 0;
		codeBox.SizeChanged += (_, _) => UpdateLineNumbers();
		codeBox.FontChanged += (_, _) => UpdateLineNumbers();
		codeBox.VScroll += (_, _) => UpdateLineNumbers();
		codeBox.Resize += (_, _) => UpdateLineNumbers();
		codeBox.KeyUp += (_, _) => UpdateLineNumbers();
		codeBox.MouseUp += (_, _) => UpdateLineNumbers();
		InitRichTextBox(codeBox, CodeBox_TextChanged);
		splitContainer.Panel2.Controls.Add(_lines);
	
		parent.SetMinSize();
	}
	private readonly LineNumberControl? _lines;
	public bool CodeChanged;
	private bool _modifyCode = false, _editingText = true, _parsing, _parsingFinished;
	private List<(int position, Color color)> _colors = [];
	private List<(Color color, string log)> _logs = [];
	private string _toParse = "";
	private CancellationTokenSource _cancel = new();
	private CancellationToken _token;
	private double _buildTime;
	private void CodeBox_TextChanged(object? sender, EventArgs e) {
		if (_modifyCode)
			return;
		CodeChanged |= _editingText;
		UpdateLineNumbers();
	}
	private Color GetForeColor() => Root?.Set?.Context?.GetColor().f ?? Color.White;
	private Color GetErrorColor() => Root?.Set?.Context?.GetErrorSuccessColor().e ?? Color.Red;
	private Color GetSuccessColor() => Root?.Set?.Context?.GetErrorSuccessColor().s ?? Color.Green;
	private void Fps_Tick(object? sender, EventArgs e) {
		_buildTime += fps.Interval;
		if (_parsingFinished)
			FinishParse();
		if (_parsing) {
			logBox.SelectionStart = 0;
			logBox.SelectionLength = 0;
			logBox.Text = "";
			logBox.SelectionColor = GetForeColor();
			logBox.AppendText("BUILDING: "+ Math.Floor(_buildTime/1000) + "s");
			logBox.AppendText(_toParse);
		}
		if (!CodeChanged || _parsing)
			return;
		CodeChanged = false;
		_parsing = true;
		_toParse = codeBox.Text;
		_token = (_cancel = new()).Token;
		abortButton.Enabled = true;
		_buildTime = 0;
		Task.Run(Parse, _token);
	}
	private void Parse() {
#if UNSAFEPARSE
		_logs = Root?.Set?.Context?.ReadCode(_toParse, _token, out _colors) ?? [];
#else
		try { _logs = Root?.Set?.Context?.ReadCode(_toParse, token, out _colors) ?? []; } catch (Exception e) {
			_logs = [(Color.Red,e.Message), (Color.Red, e.StackTrace ?? "")];
		}
#endif
		_parsingFinished = true;
		_parsing = false;
	}
	private void FinishParse() {
		abortButton.Enabled = false;
		_editingText = false;
		_parsingFinished = false;
		_modifyCode = true;
		ApplyColors(_colors, codeBox);
		_modifyCode = false;
		// append logs (errors, prints...)
		logBox.SelectionStart = 0;
		logBox.SelectionLength = 0;
		logBox.Text = "";
		if (_token.IsCancellationRequested)
			_logs = [(GetErrorColor(), "BUILD CANCELLED")];
		else {
			var fail = false;
			foreach (var t in _logs) {
				if (t.color != GetErrorColor())
					continue;
				fail = true;
				break;
			}
			if (fail) {
				logBox.SelectionColor = GetErrorColor();
				logBox.AppendText("BUILD FAILED\n");
			} else {
				logBox.SelectionColor = GetSuccessColor();
				logBox.AppendText("BUILD SUCCESS " + Math.Floor(_buildTime) + "ms\n");
			}
		}
		foreach (var t in _logs) {
			logBox.SelectionColor = t.color;
			logBox.AppendText(t.log + "\n");
		}
		Root?.Log?.Transfer(logBox.Rtf);
		// evaluate expression fields with this newly parsed program
		Root?.Exp?.ReEval();
		_editingText = true;
	}
	public override Size GetSize() => new((Pad << 1) + 64, 120);
	public override void SetDark(bool dark) { base.SetDark(dark); FinishParse(); }
	private void OpenLog(object? sender, EventArgs e) => Root?.ShowC(Root?.LogForm, FormP);
	private void UpdateLineNumbers() {
		if (_lines == null)
			return;
		using var g = _lines.CreateGraphics();
		int lineCount = codeBox.Lines.Length, width = (int)Math.Ceiling(
			g.MeasureString(lineCount.ToString(), codeBox.Font).Width
		) + 10;
		var offset = width - _lines.Width;
		if (offset != 0) {
			_lines.Width = width;
			codeBox.Left += offset;
			codeBox.Width -= offset;
		}
		_lines.Invalidate();
	}
	private void CancelBuild(object sender, EventArgs e) => _cancel.Cancel();
	
	#region Code Handling
	private static class NativeMethods {
		public const uint WmSetRedraw = 0x000B;
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
	}
	public static void InitRichTextBox(RichTextBox box, EventHandler? textChanged) {
		box.WordWrap = box.DetectUrls = false;
		box.AllowDrop = true;//box.EnableAutoDragDrop = true;
		box.Font = new("Consolas", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
		_ = box.Handle;
		const uint wmUser = 0x0400;
		const uint emSetEditStyle = wmUser + 69;
		const int sesNoOleDragDrop = 0x1000; // RichEdit flag: disable its built-in OLE drag/drop processing.
		//NativeMethods.SendMessage(box.Handle, emSetEditStyle, new(sesNoOleDragDrop), new(sesNoOleDragDrop));
		box.KeyDown += Override_KeyDown;
		box.DragEnter += Override_DragEnter;
		box.DragDrop += Override_DragDrop;
		//box.MouseDown += Override_MouseDown;
		box.TextChanged += textChanged;
	}

	// for some reason, the first click dones't get through to the text box, this at least points the caret, but a drag selection still won't happen on first click...
	public static void Override_MouseDown(object? sender, MouseEventArgs e) {
		if (sender is not RichTextBox box || e.Button != MouseButtons.Left)
			return;
		var clickPoint = e.Location;
		box.Focus();
		box.BeginInvoke(() => {
			if (box.IsDisposed || !box.IsHandleCreated)
				return;
			box.SelectionStart =  box.GetCharIndexFromPosition(clickPoint);
			box.SelectionLength = 0;
		});
	}
	// handle text pasting, so it can paste unicode, but not fonts
	public static void Override_KeyDown(object? sender, KeyEventArgs e) {
		if (sender is not RichTextBox box || !e.Control || e.KeyCode != Keys.V || !Clipboard.ContainsText(TextDataFormat.UnicodeText))
			return;
		box.SelectedText = Clipboard.GetText(TextDataFormat.UnicodeText);
		e.SuppressKeyPress = true;
	}
	public static void Override_DragEnter(object? sender, DragEventArgs e) 
		=> e.Effect = e.Data?.GetDataPresent(DataFormats.UnicodeText) == true ? DragDropEffects.Copy : DragDropEffects.None;

	public static void Override_DragDrop(object? sender, DragEventArgs e) {
		if (sender is not RichTextBox box || e.Data?.GetDataPresent(DataFormats.UnicodeText) != true || e.Data.GetData(DataFormats.UnicodeText) is not string text) 
			return;
		// Put the caret where the text was dropped.
		var clientPoint = box.PointToClient(new Point(e.X, e.Y));
		var index = box.GetCharIndexFromPosition(clientPoint);
		box.Focus();
		box.SelectionStart = index;
		//box.SelectionLength = 0; // commenting this lets you replace selection with the drag drop
		// Inserting through SelectedText uses the RichTextBox's current formatting.
		box.SelectedText = text;
	}
	public static void ApplyColors(List<(int position, Color color)> colors, RichTextBox box) {
		if (box.TextLength == 0 || colors.Count == 0)
			return;
		// Save the user's current state. Keep the selection valid if the text changed.
		int oldSelectionStart, oldSelectionLength = Math.Min(box.SelectionLength, box.TextLength - (oldSelectionStart = Math.Clamp(box.SelectionStart, 0, box.TextLength)));
		var hadFocus = box.Focused;
		try {
			box.SuspendLayout(); // Prevent visible redraw while applying several ranges.
			NativeMethods.SendMessage(box.Handle, NativeMethods.WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
			for (int from, l, i = 0; i < colors.Count; ++i)
				if (0 < (l = i + 1 < colors.Count ? colors[i + 1].position : box.TextLength) - (from = colors[i].position)) {
					box.Select(from, l);
					box.SelectionColor = colors[i].color;
				}
		} finally {
			box.Select(oldSelectionStart, oldSelectionLength);
			NativeMethods.SendMessage(box.Handle, NativeMethods.WmSetRedraw, new IntPtr(1), IntPtr.Zero);
			box.Invalidate();
			box.ResumeLayout();
			if (hadFocus) { box.Select(oldSelectionStart, oldSelectionLength); }
		}
	}
	#endregion
}