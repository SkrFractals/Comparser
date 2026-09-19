#define UNSAFEPARSE
using Comparser.Comparser;
using Comparser.Forms.Controls;
using Comparser.Forms.Core;
using System.Diagnostics;
using static Comparser.Forms.Core.IPanel;
namespace Comparser.Forms;
public partial class ComparserPanel : UserControl, IPanel {
	#region IPanel
	private readonly ControlVar _var;
	public ControlVar GetVar() => _var;
	public Size GetSize() => new((Pad << 1) + 64, 120);
	public void SetDark(bool dark) {
		BaseSetDark(this);
		DrawLogsAndColors();
	}
	public void PerformClose() { }
	public void CoreLayout() { }
	#endregion

	#region Inits
	public ComparserPanel() => InitializeComponent();
	public ComparserPanel(MenuPanel root, ParentForm parent) : this() {
		InitVar(ref _var, this, root, parent, "Comparser - Code & Build");
		splitContainer.Panel1MinSize = 3 * Pad + 2 * RowHeight;
		splitContainer.Panel2MinSize = 2 * Pad + RowHeight;
		_lines = new(codeBox);
		codeBox.Location = new(0, 0);
		codeBox.Size = new(splitContainer.Panel2.Width, splitContainer.Panel2.Height); //splitContainer.Panel1.Size;
		_lines.Location = codeBox.Location;
		_lines.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Top;
		_lines.Height = codeBox.Height;
		_lines.Width = 0;
		// all the events that should update Line Numbers
		codeBox.SizeChanged += (_, _) => RefreshLines();
		codeBox.FontChanged += (_, _) => RefreshLines();
		codeBox.VScroll += (_, _) => RefreshLines();
		codeBox.Resize += (_, _) => RefreshLines();
		codeBox.KeyUp += (_, _) => RefreshLines();
		codeBox.MouseUp += (_, _) => RefreshLines();
		// changing selection/moving caret should reset the build timer, only inactivity lets it tick
		codeBox.SelectionChanged += (_, _) => _codeTime.Restart();
		InitRichTextBox(codeBox, CodeBox_TextChanged);
		splitContainer.Panel2.Controls.Add(_lines);
		Init();
	}
	private void Init() {
		bool oldState = _myStates.ContainsKey(SettingsPanel.Context!);
		var s = oldState ? _myStates[SettingsPanel.Context!] : _myStates[SettingsPanel.Context!] = new();
		s.Suppressed = true;
		// initialize controls here
		s.Suppressed = false;
		if (oldState)
			return;
		// bind state transactions
	}
	#endregion

	#region Variables
	public bool CodeChanged;
	private readonly LineNumberControl? _lines;
	private readonly Stopwatch _freeTime = new(), _codeTime = new();
	private enum ParseState { Free, Parsing, Cancelled, Finished }
	private volatile ParseState _parsing = ParseState.Free;
	private (int position, Color color)[] _colors = [];
	private List<(Color color, string log)> _logs = [];
	private string _toParse = "";
	private CancellationTokenSource _cancel = new();
	private CancellationToken _token;
	private Stopwatch _buildTime = new();
	private bool _dirtyResult, _dirtyLog;
	#endregion

	#region Events
	private void Fps_Tick(object? sender, EventArgs e) {
		switch (_parsing) {
			case ParseState.Finished:
				FinishParse();
				goto case ParseState.Cancelled;
			case ParseState.Cancelled:
				_freeTime.Restart();
				_parsing = ParseState.Free;
				goto case ParseState.Free;
			case ParseState.Free:
				fps.Interval = 100;
				if (_dirtyResult && /*!CodeChanged &&*/ _freeTime.ElapsedMilliseconds > 100)
					DrawLogsAndColors();
				break;
			case ParseState.Parsing:
				fps.Interval = (int)Math.Min(SettingsPanel.ReportingDelay, 100 + _buildTime.ElapsedMilliseconds);
				if (logBox == null || SettingsPanel.ReportingMode == SettingsPanel.Reporting.Silent)
					return;
				var r = "BUILDING: " + Math.Floor(_buildTime.ElapsedMilliseconds / 1000.0) + "s\n";
				if (SettingsPanel.ReportingMode >= SettingsPanel.Reporting.Report)
					r += "Remaining text:\n" + SettingsPanel.Context?.ParsePeek();
				UpdateLog(r);
				break;
		}
		if (!CodeChanged || !SettingsPanel.AutoBuild)
			return;
		if (_codeTime.ElapsedMilliseconds < SettingsPanel.BuildDelay)
			return;
		if (SettingsPanel.ReportingMode == SettingsPanel.Reporting.Silent)
			UpdateLog("BUILDING...");
		Build();
	}
	private void CodeBox_TextChanged(object? sender, EventArgs e) {
		CodeChanged = true;
		buildButton.Text = "BUILD";
		_codeTime.Restart();
		RefreshLines();
	}
	private void OpenLog(object? sender, EventArgs e) {
		_var.Root.ShowC(_var.Root.LogForm, _var.Form);
		if (_dirtyLog)
			TransferLog();
	}
	private void CancelBuild(object sender, EventArgs e) {
		if (_parsing == ParseState.Parsing)
			_cancel.Cancel();
		else if (CodeChanged)
			Build();
		else if (_dirtyResult)
			DrawLogsAndColors();
	}
	#endregion

	#region Actions
	private void Build() {
		_codeTime.Stop();
		_freeTime.Stop();
		CodeChanged = false;
		_parsing = ParseState.Parsing;
		fps.Interval = 100;
		_toParse = codeBox.Text;
		_token = (_cancel = new()).Token;
		buildButton.Text = "CANCEL";
		_buildTime = Stopwatch.StartNew();
		Task.Run(Parse, _token);
	}
	private void UpdateLog(string r) {
		NativeMethods.SendMessage(logBox.Handle, NativeMethods.WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
		logBox.SuspendLayout();
		try {
			logBox.SelectionStart = logBox.SelectionLength = 0;
			logBox.Text = "";
			logBox.SelectionColor = GetForeColor();
			logBox.AppendText(r);
		} finally {
			NativeMethods.SendMessage(logBox.Handle, NativeMethods.WmSetRedraw, new IntPtr(1), IntPtr.Zero);
			logBox.Invalidate();
			logBox.ResumeLayout();
		}
		TransferLog();
	}
	private void Parse() {
		Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
#if UNSAFEPARSE
		_logs = SettingsPanel.Context?.ReadCode(_toParse, _token, out _colors) ?? [];
#else
		try { _logs =SettingsControl.Context?.ReadCode(_toParse, token, out _colors) ?? []; } catch (Exception e) {
			_logs = [(Color.Red,e.Message), (Color.Red, e.StackTrace ?? "")];
		}
#endif
		_parsing = _token.IsCancellationRequested ? ParseState.Cancelled : ParseState.Finished;
	}
	private void FinishParse() {
		_buildTime.Stop();
		_dirtyResult = true;
		buildButton.Text = "OK";
		//DrawLogsAndColors();
	}
	private void TransferLog() {
		if (_var.Root.Log?.GetVar().Form.Visible ?? false) {
			_var.Root.Log?.Transfer(logBox.Rtf);
			_dirtyLog = false;
		} else _dirtyLog = true;
	}
	private void DrawLogsAndColors() {
		_dirtyResult = false;
		_freeTime.Stop();
		codeBox.TextChanged -= CodeBox_TextChanged;
		// apply codeBox type colors
		ApplyColors(_colors, codeBox);
		// append logs (errors, prints...)
		NativeMethods.SendMessage(logBox.Handle, NativeMethods.WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
		logBox.SuspendLayout();
		try {
			logBox.SelectionStart = logBox.SelectionLength = 0;
			logBox.Text = "";
			if (_token.IsCancellationRequested)
				_logs = [(GetErrorColor(), "BUILD CANCELLED")];
			else {
				var fail = false;
				foreach (var (color, log) in _logs) {
					if (color != GetErrorColor())
						continue;
					fail = true;
					break;
				}
				if (fail) {
					logBox.SelectionColor = GetErrorColor();
					logBox.AppendText("BUILD FAILED\n");
				} else {
					logBox.SelectionColor = GetSuccessColor();
					logBox.AppendText("BUILD SUCCESS " + _buildTime.ElapsedMilliseconds + "ms\n");
				}
			}
			foreach (var (color, log) in _logs) {
				logBox.SelectionColor = color;
				logBox.AppendText(log + "\n");
			}
		} finally {
			NativeMethods.SendMessage(logBox.Handle, NativeMethods.WmSetRedraw, new IntPtr(1), IntPtr.Zero);
			logBox.Invalidate();
			logBox.ResumeLayout();
		}
		TransferLog();
		// evaluate expression fields with this newly parsed program
		_var.Root.Exp?.ReEval();
		_var.Root.Plot?.ReEval(/*SettingsControl.Context?.GetPlot()!*/);
		codeBox.TextChanged += CodeBox_TextChanged;
	}
	private void RefreshLines() {
		if (_lines == null)
			return;
		using var g = _lines.CreateGraphics();
		int lineCount = codeBox.Lines.Length, width = (int)Math.Ceiling(
			g.MeasureString(lineCount.ToString(), codeBox.Font).Width
		) + 10;
		var offset = width - _lines.Width;
		if (offset != 0) {
			// update width of the line panel, and adjust the codeBox to it
			_lines.Width = width;
			codeBox.Left += offset;
			codeBox.Width -= offset;
		}
		_lines.Invalidate();
	}
	#endregion

	#region Getters
	private Color GetForeColor() => SettingsPanel.Context?.GetColor().f ?? Color.White;
	private Color GetErrorColor() => SettingsPanel.Context?.GetErrorSuccessColor().e ?? Color.Red;
	private Color GetSuccessColor() => SettingsPanel.Context?.GetErrorSuccessColor().s ?? Color.Green;
	#endregion

	#region Code Handling
	public static class NativeMethods {
		public const uint WmSetRedraw = 0x000B;
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
	}
	public static void InitRichTextBox(RichTextBox box, EventHandler? textChanged) {
		box.WordWrap = box.DetectUrls = false;
		box.AllowDrop = true;//box.EnableAutoDragDrop = true;
		box.Font = new("Consolas", 12, FontStyle.Bold, GraphicsUnit.Point, 238);
		_ = box.Handle;
		box.KeyDown += Override_KeyDown;
		box.DragEnter += Override_DragEnter;
		box.DragDrop += Override_DragDrop;
		box.TextChanged += textChanged;
	}

	// for some reason, the first click dones't get through to the text box, this at least points the caret, but a drag selection still won't happen on first click...
	public static void Override_MouseDown(object? sender, MouseEventArgs e) {
		if (sender is not RichTextBox box || e.Button != MouseButtons.Left)
			return;
		var clickPoint = e.Location;
		_ = box.Focus();
		_ = box.BeginInvoke(() => {
			if (box.IsDisposed || !box.IsHandleCreated)
				return;
			box.SelectionStart = box.GetCharIndexFromPosition(clickPoint);
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
	public static void ApplyColors((int position, Color color)[] colors, RichTextBox box) {
		if (box.TextLength == 0 || colors.Length == 0)
			return;
		// Save the user's current state. Keep the selection valid if the text changed.
		int oldSelectionStart, oldSelectionLength = Math.Min(box.SelectionLength, box.TextLength - (oldSelectionStart = Math.Clamp(box.SelectionStart, 0, box.TextLength)));
		var hadFocus = box.Focused;
		try {
			box.SuspendLayout(); // Prevent visible redraw while applying several ranges.
			NativeMethods.SendMessage(box.Handle, NativeMethods.WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
			for (int from, l, i = 0; i < colors.Length; ++i)
				if (0 < (l = i + 1 < colors.Length ? colors[i + 1].position : box.TextLength) - (from = colors[i].position)) {
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

	#region LogState
	private void LogState(Control c, byte action = 0) => _myStates[SettingsPanel.Context!].Log(c, action);
	override protected bool ProcessCmdKey(ref Message msg, Keys k) {
		if (SettingsPanel.Context is not { } c)
			return base.ProcessCmdKey(ref msg, k);
		switch (k) {
		case Keys.Control | Keys.Z:
			_myStates[c].Undo();
			return true;
		case Keys.Control | Keys.Y:
			_myStates[c].Redo();
			return true;
		default:
			return base.ProcessCmdKey(ref msg, k);
		}
	}
	private readonly Dictionary<IComparser, States> _myStates = [];
	#endregion
	
	private void openCode_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
		if (SettingsPanel.Context is not { } c)
			return;
		var file = openCode.FileName;
		if (!File.Exists(file)) {
			MessageBox.Show("Selected file couldn't be found.", "NO FILE");
			return;
		}
		var read = 0;
		_myStates[c].Deserialize(_myStates[c].D[codeBox],File.ReadAllText(file), ref read); // read code
	}

	private void saveCode_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
		if (SettingsPanel.Context is not { } c)
			return;
		var d = _myStates[c].D;
		File.WriteAllText(saveCode.FileName,d[codeBox].Serialize()); // outputs
		MessageBox.Show("Code saved.", "SAVED");
	}
}