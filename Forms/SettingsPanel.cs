using Comparser.Comparser;
using Comparser.Forms.Core;
using static Comparser.Forms.Core.IPanel;
namespace Comparser.Forms;

public partial class SettingsPanel : UserControl, IPanel {
	#region IControl
	private readonly ControlVar _var;
	public ControlVar GetVar() => _var;
	public Size GetSize() => new(320, 320);//new( Pad * 5 + decLabel.Width + decimalBox.Width + algebraBox.Width + darkButton.Width, 
		//(Pad << 1) + RowHeight + Pad);
	public void SetDark(bool dark) { BaseSetDark(this);	darkButton.Text = dark ? "☾" : "☀︎"; }
	public void PerformClose() { }
	public void CoreLayout() { }
	#endregion

	#region Variables
	//private SettingsLayout _s = new();
	public static bool MemX, MemXy;
	public static int Decimals = 3;
	public static int Algebra = 1;
	private static readonly int MaxTasks = Environment.ProcessorCount - (Environment.ProcessorCount >> 3); // use up to 7/8 of all cores (more gap with more cores)
	public static int Tasks =  Environment.ProcessorCount - (Environment.ProcessorCount >> 3); 
	private readonly IComparser[] _algebras = [new ComparserR(), new ComparserC(), new ComparserQ()];
	public static IComparser? Context;
	private bool _darkMode = true, _preEvaluate = true;
	public static int ReportingDelay = 1000, BuildDelay = 5000, PlotDelay = 5000;
	public static Reporting ReportingMode = Reporting.Report;
	public static bool AutoBuild = true, AutoPlot = true;
	public enum Reporting : byte { Silent = 0, Timer = 1, Report = 2 }
	#endregion

	#region Inits
	public SettingsPanel() => InitializeComponent();
	public SettingsPanel(MenuPanel root, ParentForm parent) : this() {
		InitVar(ref _var, this, root, parent, "Comparser - App Settings");
		algebraBox.SelectedIndex = 1;//AlgebraBox_SelectedIndexChanged(algebraBox, EventArgs.Empty);
		Context = _algebras[Algebra];
		DecimalBox_TextChanged(decimalBox, EventArgs.Empty);
		_var.Root.Code?.CodeChanged = true;
		parent.SetMinSize();
		parent.Text = "Comparser - Settings";
		reportBox_TextChanged(reportBox, EventArgs.Empty);
		autoBox_TextChanged(reportBox, EventArgs.Empty);
		UpdateAuto();
		UpdateReport();
		preEvalBox.Checked = true;
		taskBox.Text = MaxTasks.ToString();
		plotLabel.Text = "Auto Plot";
		autoLabel.Text = "Auto Build:";
		decLabel.Text = "Decimals:";
		reportLabel.Text = "Report Logs:";
		
		// blocks scrolling over int from changing their value
		algebraBox.MouseWheel += ComboBox_MouseWheel;
	}
	#endregion

	#region Events
	private void DecimalBox_TextChanged(object? sender, EventArgs e) {
		var old = Decimals;
		_ = int.TryParse(decimalBox.Text, out Decimals);
		Context?.SetDecimals(Decimals);
		if (old != Decimals)
			_var.Root.Code?.CodeChanged = true; // re-parse so the prints and expressions update
	}
	private void AlgebraBox_SelectedIndexChanged(object? sender, EventArgs e) {
		Algebra = algebraBox.SelectedIndex;
		_var.Root.Code?.CodeChanged = true; // reparse
		Context = _algebras[Algebra];
		_var.Root.Plot?.SetContext();
	}

	private void darkButton_Click(object sender, EventArgs e) {
		_darkMode = !_darkMode;
		SetDarkMode();
	}
	private void autoButton_Click(object sender, EventArgs e) {
		AutoBuild = !AutoBuild;
		UpdateAuto();
	}
	private void reportButton_Click(object sender, EventArgs e) {
		ReportingMode = (Reporting)(((int)ReportingMode + 1) % 3);
		UpdateReport();
	}
	private void autoBox_TextChanged(object sender, EventArgs e) {
		if (!int.TryParse(autoBox.Text, out BuildDelay) || BuildDelay < 100) BuildDelay = 100;
	}
	private void reportBox_TextChanged(object sender, EventArgs e) {
		if (!int.TryParse(reportBox.Text, out ReportingDelay) || ReportingDelay < 100) ReportingDelay = 100;
	}
	private void taskBox_TextChanged(object sender, EventArgs e) {
		if (!int.TryParse(taskBox.Text, out Tasks) || Tasks < 1) Tasks = 1;
		if (Tasks > MaxTasks) Tasks = MaxTasks;
	}
	private void preEvalBox_CheckedChanged(object sender, EventArgs e) {
		_preEvaluate = preEvalBox.Checked;
		SetPreEval();
	}
	private void plotBox_TextChanged(object sender, EventArgs e) {
		if (!int.TryParse(plotBox.Text, out PlotDelay) || PlotDelay < 100) PlotDelay = 100;
	}

	private void plotButton_Click(object sender, EventArgs e) {
		AutoPlot = !AutoPlot;
		UpdatePlot();
	}
	private void xMemBox_CheckedChanged(object sender, EventArgs e) {
		MemX = xMemBox.Checked;
		MessageBox.Show("This feature is quite advanced and has not yet been fully debugged, it's only about 80% functional. I do not recommend turning this on yet.", "WARNING: Not debugged!");
	}

	private void xyMemBox_CheckedChanged(object sender, EventArgs e) {
		MemXy = xyMemBox.Checked;
		MessageBox.Show("This feature is quite advanced and has not yet been fully debugged, but may be functional.", "WARNING: Not debugged!");
	}
	#endregion

	#region Actions
	public void SetDarkMode() {
		Context?.SetDarkMode(_darkMode);
		_var.Root.SetDark(_darkMode);
	}
	public void SetPreEval() => Context?.SetPreEvaluate(_preEvaluate);
	private void UpdateAuto() => autoButton.Text = AutoBuild ? "DELEAYED AUTOMATIC" : "MANUAL";
	private void UpdateReport() => reportButton.Text = ReportingMode switch {
		Reporting.Silent => "SILENT",
		Reporting.Timer => "ONLY TIME",
		Reporting.Report => "REPORT STATE",
		_ => "???"
	};
	private void UpdatePlot() => plotButton.Text = AutoPlot ? "DELEAYED AUTOMATIC" : "MANUAL";
	#endregion	
	
	public static void ComboBox_MouseWheel(object? sender, MouseEventArgs e) =>	((HandledMouseEventArgs)e).Handled = true;
}