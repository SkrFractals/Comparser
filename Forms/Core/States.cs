using Comparser.Comparser.Numbers;
using Comparser.Forms.Controls;
using static Comparser.Forms.PlotPanel;

namespace Comparser.Forms.Core;

// When any control "sender" finishes a trasaction, it should call MyStates[SettingsControl.Context!].Log(sender)
public class States {
	public bool Suppressed;
	private Stack<ILogSet> Undos = [], Redos = [];
	internal Dictionary<Control, ILogSet> D = [];
	internal void Log(Control sender, byte action = 0) { // sender calls this when it finished a transaction
		if (Suppressed || !D.TryGetValue(sender, out var s)) return;
		s.LogUndo(action, true);
		Undos.Push(s);
	}
	internal void Undo() {
		if (Suppressed || Undos.Count < 1 || Undos.Pop() is not ILogSet s) return;
		Suppressed = true;
		try {
			if (s.TryUndoFailed()) { Suppressed = false; return; }
			Redos.Push(s);
		} finally { Suppressed = false; }
	}
	internal void Redo() {
		// goes through the redo stack, until one successfully happens (because it can store redos that were invalidated)
		while (!Suppressed && Redos.Count > 0 && Redos.Pop() is ILogSet s) {
			Suppressed = true;
			try {
				if (s.TryRedoFailed()) { Suppressed = false; continue; }
				Undos.Push(s);
			} finally { Suppressed = false; }
			return;
		}
	}
}
internal interface ILogSet {
	public abstract void LogUndo(byte action = 0, bool clear = false);
	public void LogRedo(byte action);
	public abstract bool TryUndoFailed();
	public abstract bool TryRedoFailed();
}
record LogState<T>(byte Action, T State);
internal abstract class LogSet<T> : ILogSet { // Every child calls LogUndo after binding its controls, to record the initial state
	
	protected Stack<LogState<T>> Undos = [];
	protected Stack<LogState<T>> Redos = [];
	public void LogUndo(byte action = 0, bool clear = false) { if (clear) Redos.Clear(); Undos.Push(new(action, GetState())); }
	public void LogRedo(byte action) => Redos.Push(new(action, GetState()));
	public bool TryUndoFailed() {
		if (Undos.Count < 2) return true;
		var p = Undos.Pop();
		LogRedo(p.Action);
		RestoreUndo(p, Undos.Peek()); // no longer can fail
		return false;
	}
	public bool TryRedoFailed() {
		if (Redos.Count < 1) return true;
		RestoreRedo(Undos.Peek(), Redos.Pop());
		LogUndo();
		return false;
	}
	protected virtual void RestoreUndo(LogState<T> from, LogState<T> to) => RestoreRedo(from, to);
	protected abstract void RestoreRedo(LogState<T> from, LogState<T> to);
	internal abstract T GetState();	
}

internal class LogSce : LogSet<SceState> { // triple textbox pinnable range
																		
	internal LogSce(AxisControls sce, States t) { Sce = sce; LogUndo(); /*t.D[sce.S.Box!] = t.D[sce.C.Box!] = t.D[sce.E.Box!] = t.D[sce.Lc] = t.D[sce.Le]*/t.D[sce.Ls] = this; }
	private AxisControls Sce;
	internal override SceState GetState() => Sce.GetState(); // log three texts and which one is pinned
	protected override void RestoreRedo(LogState<SceState> _, LogState<SceState> to) => Sce.SetSce(to.State, true); // disables textboxes, puts the new values in, and true forwards the new values to rewrite the actual axis object.
}
record PlotSizeState(string w, string h, SceState ix, SceState iy, SceState oy);
internal class LogPlotSize : LogSet<PlotSizeState> { // triple textbox pinnable range
	internal LogPlotSize(RichTextBox w, RichTextBox h, AxisControls ix, AxisControls iy, AxisControls oy, States t) { Ix = ix; Iy = iy; Oy = oy; W = w; H = h; LogUndo(); t.D[w] = t.D[h] = this; }
	private AxisControls Ix, Iy, Oy; // Axes: InputX, InputY, OutputY
	private RichTextBox W, H; // Width x Height
	internal override PlotSizeState GetState() => new(W.Text, H.Text, Ix.GetState(), Iy.GetState(), Oy.GetState()); // log three texts and which one is pinned
	protected override void RestoreRedo(LogState<PlotSizeState> _, LogState<PlotSizeState> to) {
		W.Tag = H.Tag = true;
		W.Text = to.State.w;
		H.Text = to.State.h;
		W.Tag = H.Tag = false;
		Ix.SetSce(to.State.ix, true);
		Iy.SetSce(to.State.iy, true);
		Oy.SetSce(to.State.oy, true);
	}
}
record AniState(string l, string f, SceState it, bool a); // Length, Frame, Time Axis, Animated
internal class LogAni : LogSet<AniState> { // triple textbox pinnable range
	internal LogAni(RichTextBox l, RichTextBox f, AxisControls it, CheckBox a, States t) { It = it; L = l; F = f; A = a; LogUndo(); t.D[l] = t.D[f] = t.D[a] = this; } // TODO add LogState to animatedClick
	private AxisControls It; // Axis Time
	private RichTextBox L, F; // Length / Frame
	private CheckBox A; // animated
	internal override AniState GetState() => new(L.Text, F.Text, It.GetState(), A.Checked); // log three texts and which one is pinned
	protected override void RestoreRedo(LogState<AniState> _, LogState<AniState> to) {
		L.Text = to.State.l;
		F.Text = to.State.f;
		It.SetSce(to.State.it, true);
		A.Checked = to.State.a;
	}
}


internal class LogText : LogSet<string> { // for a single code box
	internal LogText(RichTextBox text, States t) { Text = text; LogUndo(); t.D[text] = this; }
	private RichTextBox Text;
	internal override string GetState() => Text.Text;
	protected override void RestoreRedo(LogState<string> _, LogState<string> to) => Text.Text = to.State; // it will trigger textChanged which will re-parse and recolor the box, and tries to push undo which will be blocked
}

internal class LogLock : LogSet<bool> { // for the lock buttons
	internal LogLock(Button l, States t) { Lock = l; LogUndo(); t.D[l] = this; }
	private Button Lock;
	internal override bool GetState() => Lock.Text == Static.LockedSymbol;
	protected override void RestoreRedo(LogState<bool> _, LogState<bool> to) { if ((Lock.Text == Static.LockedSymbol) != to.State) Lock.PerformClick(); }
}
internal enum OutputAction {
	Select, // or edit
	Add,
	Remove
}
record OutState(string s, string r, string c, int l);
internal class LogOutput : LogSet<OutState> { // type, selectName, rgbCode, codeEval
	internal LogOutput(PlotSettingsControl s, PlotPanel c , States t) { S = s; C = c; LogUndo(); t.D[S.outputSelect] = t.D[S.rgbBox] = t.D[S.codeBox] = this; }
	private PlotSettingsControl S;
	private PlotPanel C;
	internal override OutState GetState() => new((string?)S.outputSelect.SelectedItem ?? "", S.rgbBox.Text, S.codeBox.Text, S.clipSelect.SelectedIndex);
	protected override void RestoreUndo(LogState<OutState> from, LogState<OutState> to) {
		// remove: from = (remove, fallbacked), to (_, removed)
		// add: from = (add, added), to (_, backto)
		switch ((OutputAction)from.Action) {
			case OutputAction.Add:
				S.outputSelect.Text = from.State.s;
				C.Del(S.outputSelect, EventArgs.Empty);
				break;
			case OutputAction.Remove:
				S.outputSelect.Text = to.State.s; // re-add it back
				C.Add(S.outputSelect, EventArgs.Empty);
				break;
		}
		LoadBack(to.State);
	}
	protected override void RestoreRedo(LogState<OutState> from, LogState<OutState> to) {
		switch ((OutputAction)to.Action) {
			case OutputAction.Add:
				S.outputSelect.Text = to.State.s; // re-add it back
				C.Add(S.outputSelect, EventArgs.Empty);
				break;
			case OutputAction.Remove: // re-remove it away again
				S.outputSelect.Text = from.State.s;
				C.Del(S.outputSelect, EventArgs.Empty);
				break;
		}
		LoadBack(to.State);
	}
	private void LoadBack(OutState s) {
		S.outputSelect.SelectedItem = s.s;
		S.rgbBox.Text = s.r;
		S.codeBox.Text = s.c;
		S.clipSelect.SelectedIndex = s.l;
	}
}
internal class LogCombo : LogSet<int> { // for the code
	internal LogCombo(ComboBox combo, States t) { Combo = combo; LogUndo(); t.D[combo] = this; }
	private ComboBox Combo;
	internal override int GetState() => Combo.SelectedIndex;
	protected override void RestoreRedo(LogState<int> _, LogState<int> to) => Combo.SelectedIndex = to.State;
}