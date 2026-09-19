using Comparser.Comparser.Numbers;
using Comparser.Forms.Controls;
using System.Reflection.Metadata.Ecma335;
using static Comparser.Forms.PlotPanel;

namespace Comparser.Forms.Core;

// When any control "sender" finishes a transaction, it should call MyStates[SettingsControl.Context!].Log(sender)
public class States {
	public bool Suppressed;
	private readonly Stack<ILogSet> _undo = [], _redo = [];
	internal readonly Dictionary<Control, ILogSet> D = [];
	internal void Log(Control sender, byte action = 0) { // sender calls this when it finished a transaction
		if (Suppressed || !D.TryGetValue(sender, out var s)) return;
		s.LogUndo(action, true);
		_undo.Push(s);
	}
	internal void Undo() {
		if (Suppressed || _undo.Count < 1 || _undo.Pop() is not { } s) return;
		Suppressed = true;
		try {
			if (s.TryUndoFailed()) { Suppressed = false; return; }
			_redo.Push(s);
		} finally { Suppressed = false; }
	}
	internal void Redo() {
		// goes through the redo stack, until one successfully happens (because it can store redo that were invalidated)
		while (!Suppressed && _redo.Count > 0 && _redo.Pop() is { } s) 
			if(Load(s)) return;
	}
	internal void Deserialize(ILogSet log, string s, ref int read) {
		log.DeSerialize(s, ref read);
		Load(log);
	}
	private bool Load(ILogSet s) {
		Suppressed = true;
		try {
			if (s.TryRedoFailed()) { Suppressed = false; return false; }
			_undo.Push(s);
		} finally { Suppressed = false; }
		return true;
	}
}
internal interface ILogSet {
	public void LogUndo(byte action = 0, bool clear = false);
	public void LogRedo(byte action);
	public bool TryUndoFailed();
	public bool TryRedoFailed();
	public virtual string Serialize() => "";
	public virtual void DeSerialize(string s, ref int read) { }

}
record LogState<T>(byte action, T state);
internal abstract class LogSet<T>(States t) : ILogSet { // Every child calls LogUndo after binding its controls, to record the initial state

	protected States MyStates = t;
	
	protected readonly Stack<LogState<T>> Undo = [], Redo = [];
	public void LogUndo(byte action = 0, bool clear = false) { if (clear) Redo.Clear(); Undo.Push(new(action, GetState())); }
	public void LogRedo(byte action) => Redo.Push(new(action, GetState()));
	public bool TryUndoFailed() {
		if (Undo.Count < 2) return true;
		var p = Undo.Pop();
		LogRedo(p.action);
		RestoreUndo(p, Undo.Peek()); // no longer can fail
		return false;
	}
	public bool TryRedoFailed() {
		if (Redo.Count < 1) return true;
		RestoreRedo(Undo.Peek(), Redo.Pop());
		LogUndo();
		return false;
	}
	virtual protected void RestoreUndo(LogState<T> from, LogState<T> to) => RestoreRedo(from, to);
	protected abstract void RestoreRedo(LogState<T> from, LogState<T> to);
	protected abstract T GetState();
	protected string Read(string s, ref int read) {
		var i = s.IndexOf(';', read);
		var r = read;
		read += 1 + i;
		return s[r..i];
	}
	protected string WriteSce(SceState s) => s.s + ";" + s.c + ";" + s.e + ";" + s.l + ";";
	protected SceState ReadSce(string s, ref int read) => new(Read(s, ref read), Read(s, ref read), Read(s, ref read), int.TryParse(Read(s, ref read), out var l) ? l : -1);
}

internal class LogSce : LogSet<SceState> { // triple textbox pinnable range
																		
	internal LogSce(AxisControls sce, States t) : base(t) { _sce = sce; LogUndo(); /*t.D[sce.S.Box!] = t.D[sce.C.Box!] = t.D[sce.E.Box!] = t.D[sce.Lc] = t.D[sce.Le]*/t.D[sce.Ls] = this; }
	private readonly AxisControls _sce;
	override protected SceState GetState() => _sce.GetState(); // log three texts and which one is pinned
	override protected void RestoreRedo(LogState<SceState> _, LogState<SceState> to) => _sce.SetSce(to.state, true); // disables textboxes, puts the new values in, and true forwards the new values to rewrite the actual axis object.
	public string Serialize() => WriteSce(Undo.Peek().state);
	public void DeSerialize(string s, ref int read) 
		=> Redo.Push(new(0, ReadSce(s, ref read)));
	
}
record PlotSizeState(string w, string h, SceState ix, SceState iy, SceState oy);
internal class LogPlotSize : LogSet<PlotSizeState> { // triple textbox pinnable range
	internal LogPlotSize(RichTextBox w, RichTextBox h, AxisControls ix, AxisControls iy, AxisControls oy, States t) : base(t) { _ix = ix; _iy = iy; _oy = oy; _w = w; _h = h; LogUndo(); t.D[w] = t.D[h] = this; }
	private readonly AxisControls _ix, // Axes: InputX, InputY, OutputY
		_iy, // Axes: InputX, InputY, OutputY
		_oy; // Axes: InputX, InputY, OutputY
	private readonly RichTextBox _w, _h; // Width x Height
	override protected PlotSizeState GetState() => new(_w.Text, _h.Text, _ix.GetState(), _iy.GetState(), _oy.GetState()); // log three texts and which one is pinned
	override protected void RestoreRedo(LogState<PlotSizeState> _, LogState<PlotSizeState> to) {
		_w.Tag = _h.Tag = true;
		_w.Text = to.state.w;
		_h.Text = to.state.h;
		_w.Tag = _h.Tag = false;
		_ix.SetSce(to.state.ix, true);
		_iy.SetSce(to.state.iy, true);
		_oy.SetSce(to.state.oy, true);
	}

	public string Serialize() {
		var s = Undo.Peek();
		return s.state.w + ";" + s.state.h + ";" + WriteSce(s.state.ix) + WriteSce(s.state.iy) + WriteSce(s.state.oy);
	}
	public void DeSerialize(string s, ref int read) 
		=> Redo.Push(new(0, new(Read(s, ref read), Read(s, ref read), ReadSce(s, ref read),ReadSce(s, ref read),ReadSce(s, ref read))));
}
record AniState(string l, string f, SceState it, bool a); // Length, Frame, Time Axis, Animated
internal class LogAni : LogSet<AniState> { // triple textbox pinnable range
	internal LogAni(RichTextBox l, RichTextBox f, AxisControls it, CheckBox a, States t) : base(t) { _it = it; _l = l; _f = f; _a = a; LogUndo(); t.D[l] = t.D[f] = t.D[a] = this; } // TODO add LogState to animatedClick
	private readonly AxisControls _it; // Axis Time
	private readonly RichTextBox _l, _f; // Length / Frame
	private readonly CheckBox _a; // animated
	override protected AniState GetState() => new(_l.Text, _f.Text, _it.GetState(), _a.Checked); // log three texts and which one is pinned
	override protected void RestoreRedo(LogState<AniState> _, LogState<AniState> to) {
		_l.Text = to.state.l;
		_f.Text = to.state.f;
		_it.SetSce(to.state.it, true);
		_a.Checked = to.state.a;
	}
	public string Serialize() {
		var s = Undo.Peek();
		return s.state.l + ";" + s.state.f + ";" + WriteSce(s.state.it) + ";" + (s.state.a ? "1" : "0") + ";";
	}
	public void DeSerialize(string s, ref int read) 
		=> Redo.Push(new(0,new(Read(s, ref read), Read(s, ref read), ReadSce(s, ref read), Read(s, ref read) == "1")));
}
internal class LogText : LogSet<string> { // for a single code box
	internal LogText(RichTextBox text, States t) : base(t) { _text = text; LogUndo(); t.D[text] = this; }
	private readonly RichTextBox _text;
	override protected string GetState() => _text.Text;
	override protected void RestoreRedo(LogState<string> _, LogState<string> to) => _text.Text = to.state; // it will trigger textChanged which will reparse and recolor the box, and tries to push undo which will be blocked
	public string Serialize() {
		var s = Undo.Peek();
		return s.state;
	}
	public void DeSerialize(string s, ref int read) 
		=> Redo.Push(new(0,Read(s, ref read)));
}

internal class LogLock : LogSet<bool> { // for the lock buttons
	internal LogLock(Button l, States t) : base(t) { _lock = l; LogUndo(); t.D[l] = this; }
	private readonly Button _lock;
	override protected bool GetState() => _lock.Text == Static.LockedSymbol;
	override protected void RestoreRedo(LogState<bool> _, LogState<bool> to) { if ((_lock.Text == Static.LockedSymbol) != to.state) _lock.PerformClick(); }
	public string Serialize() {
		var s = Undo.Peek();
		return (s.state ? "1" : "0") + ";";
	}
	public void DeSerialize(string s, ref int read) 
		=> Redo.Push(new(0,Read(s, ref read) == "1"));
}
internal enum OutputAction {
	Select, // or edit
	Add,
	Remove
}
record OutState(string s, string r, string c, int l);
internal class LogOutput : LogSet<OutState> { // type, selectName, rgbCode, codeEval
	internal LogOutput(PlotSettingsControl s, PlotPanel c , States t) : base(t) { _s = s; _c = c; LogUndo(); t.D[_s.outputSelect] = t.D[_s.rgbBox] = t.D[_s.codeBox] = this; }
	private readonly PlotSettingsControl _s;
	private readonly PlotPanel _c;
	override protected OutState GetState() => new((string?)_s.outputSelect.SelectedItem ?? "", _s.rgbBox.Text, _s.codeBox.Text, _s.clipSelect.SelectedIndex);
	override protected void RestoreUndo(LogState<OutState> from, LogState<OutState> to) {
		// remove: from = (remove, fallback), to (_, removed)
		// add: from = (add, added), to (_, backTo)
		switch ((OutputAction)from.action) {
			case OutputAction.Add:
				_s.outputSelect.Text = from.state.s;
				_c.Del(_s.outputSelect, EventArgs.Empty);
				break;
			case OutputAction.Remove:
				_s.outputSelect.Text = to.state.s; // re-add it back
				_c.Add(_s.outputSelect, EventArgs.Empty);
				break;
		}
		LoadBack(to.state);
	}
	override protected void RestoreRedo(LogState<OutState> from, LogState<OutState> to) {
		switch ((OutputAction)to.action) {
			case OutputAction.Add:
				_s.outputSelect.Text = to.state.s; // re-add it back
				_c.Add(_s.outputSelect, EventArgs.Empty);
				break;
			case OutputAction.Remove: // re-remove it away again
				_s.outputSelect.Text = from.state.s;
				_c.Del(_s.outputSelect, EventArgs.Empty);
				break;
		}
		LoadBack(to.state);
	}
	private void LoadBack(OutState s) {
		_s.outputSelect.SelectedItem = s.s;
		_s.rgbBox.Text = s.r;
		_s.codeBox.Text = s.c;
		_s.clipSelect.SelectedIndex = s.l;
	}
	
	public string Serialize() {
		var s = Undo.Peek();
		var total = _s.outputSelect.Items.Count + ";";
		foreach(int o in _s.outputSelect.Items)
			total += o + ";";
		return total + ";" + s.state.s + ";" + s.state.r + ";" +s.state.c + ";" + s.state.l + ";";
	}
	public void DeSerialize(string s, ref int read) {
		_s.outputSelect.Items.Clear();
		var outputs = int.TryParse(Read(s, ref read), out var os) ? os : -1;
		if (outputs <= 0)
			return;
		for (int i = 0; i < outputs; ++i)
			_s.outputSelect.Items.Add(Read(s, ref read));
		Redo.Push(new(0, new(Read(s, ref read), Read(s, ref read), Read(s, ref read), int.TryParse(Read(s, ref read), out var l) ? l : -1)));
	}

}
internal class LogCombo : LogSet<int> { // for the code
	internal LogCombo(ComboBox combo, States t): base(t)  { _combo = combo; LogUndo(); t.D[combo] = this; }
	private readonly ComboBox _combo;
	override protected int GetState() => _combo.SelectedIndex;
	override protected void RestoreRedo(LogState<int> _, LogState<int> to) => _combo.SelectedIndex = to.state;
	
	public string Serialize() => Undo.Peek().state + ";";
	public void DeSerialize(string s, ref int read) 
		=> Redo.Push(new(0,  int.TryParse(Read(s, ref read), out var l) ? l : -1 ));
}