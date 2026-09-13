using Comparser.Forms.Core;
using static Comparser.Forms.Core.IPanel;

namespace Comparser.Forms;
public partial class MenuPanel : UserControl, IPanel {
	#region IPanel
	private readonly ControlVar _var;
	public ControlVar GetVar() => _var;
	public Size GetSize() => new(120, Pad + 5 * (Pad + RowHeight));
	public void SetDark(bool dark) {
		BaseSetDark(this);
		/*var bf = dark ? (Color.Black, Color.White) : (Color.White, Color.Black);
		codeButton.BackColor = expButton.BackColor = setButton.BackColor = plotButton.BackColor = bf.Item1;
		codeButton.ForeColor = expButton.ForeColor = setButton.ForeColor = plotButton.ForeColor = bf.Item2;*/
		_expForm?.SetDark(dark);
		_setForm?.SetDark(dark);
		_codeForm?.SetDark(dark);
		_plotForm?.SetDark(dark);
		PlotSetForm?.SetDark(dark);
		LogForm?.SetDark(dark);
	}
	public void PerformClose() {
		LogForm?.ActuallyClose = true;
		_setForm?.ActuallyClose = true;
		_codeForm?.ActuallyClose = true;
		_expForm?.ActuallyClose = true;
		_plotForm?.ActuallyClose = true;
		PlotSetForm?.ActuallyClose = true;
	}
	public void CoreLayout() { }
	#endregion

	#region Inits
	public MenuPanel() => InitializeComponent();
	public MenuPanel(ParentForm parent) : this() {
		InitVar(ref _var, this, this, parent, "Comparser - Complex Computer Parser");
		Log = new(this, LogForm = new());
		Code = new(this, _codeForm = new());
		Exp = new(this, _expForm = new());
		SettingsPanel set = new(this, _setForm = new());
		PlotSet = new(this, PlotSetForm = new());
		Plot = new(this, _plotForm = new());
		//ParentForm[] p = [LogForm, _codeForm, _expForm, _setForm, _plotForm];
		//foreach (var i in p) i.Show();
		//_plotForm.Show(); // for some reason I have to do this, otherwise the plotter could have its splitContainer permanently docked wrong
		//_plotForm.Size = new(640, 480);
		set.SetDarkMode();
		//_plotForm.Close();
		//foreach (var i in p) i.Close();
		var m = FormStartPosition.Manual;
		_codeForm?.StartPosition = m;
		_expForm?.StartPosition = m;
		_setForm?.StartPosition = m;
		_plotForm?.StartPosition = m;
		PlotSetForm.StartPosition = m;
	}
	#endregion

	#region Variables
	private readonly ParentForm? _setForm, _codeForm, _expForm, _plotForm;
	public readonly ParentForm? LogForm, PlotSetForm;
	public readonly ComparserPanel? Code;
	public readonly LogPanel? Log;
	public readonly PlotPanel? Plot;
	public readonly ExpressionPanel? Exp;
	public readonly PlotSettingsPanel? PlotSet;
	#endregion

	#region Events
	private void SetButton_Click(object sender, EventArgs e) => ShowC(_setForm, _var.Form);
	private void CodeButton_Click(object sender, EventArgs e) => ShowC(_codeForm, _var.Form);
	private void ExpButton_Click(object sender, EventArgs e) => ShowC(_expForm, _var.Form);
	private void PlotButton_Click(object sender, EventArgs e) => ShowC(_plotForm, _var.Form);
	public void ShowC(ParentForm? f, ParentForm? p) {
		if (f == null)
			return;
		f.Location = p?.Location ?? new();
		if(!f.Visible)
			f.Show(this);
	}
	#endregion
}