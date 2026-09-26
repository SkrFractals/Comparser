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
	public void SetDark(bool dark) { BaseSetDark(this); darkButton.Text = dark ? "☾" : "☀︎"; }
	public void PerformClose() { }
	public void CoreLayout() { }
	#endregion

	#region Variables
	public static bool UseMem;
	public static int Decimals = 3;
	//public static int Algebra = 1;
	private static readonly int MaxTasks = Environment.ProcessorCount - (Environment.ProcessorCount >> 3); // use up to 7/8 of all cores (more gap with more cores)
	public static int Tasks = 1, Chunks = 8, DrawTasks = 1, DrawChunks = 1, Mp4Tasks = 1, SuperSampling = 1;
	//private readonly IComparser[] _algebras = [new ComparserR(), new ComparserC(), new ComparserQ()];
	private bool _preEvaluate = true, _darkMode = true, _allowStrings = true;
	public static readonly IComparser Context = new Comparser.Comparser();
	public static int ReportingDelay = 1000, BuildDelay = 5000, PlotDelay = 5000, PreviewLoad = 3, FrameRate = 30;
	public static Reporting ReportingMode = Reporting.Report;
	public static bool AutoBuild = true;
	public static AutoPlotting AutoPlot = AutoPlotting.OnlyCursor;
	public enum Reporting : byte { Silent = 0, Timer = 1, Report = 2 }
	public enum AutoPlotting : byte { Manual = 0, OnlyCursor = 1, DelayedAuto = 2 }
	private static int _controlTabIndex;
	#endregion

	#region Inits
	public SettingsPanel() => InitializeComponent();
	public SettingsPanel(MenuPanel root, ParentForm parent) : this() {
		InitVar(ref _var, this, root, parent, "Comparser - App Settings");
		algebraBox.Enabled = false;
		//algebraBox.SelectedIndex = 1;//AlgebraBox_SelectedIndexChanged(algebraBox, EventArgs.Empty);
		//Context = _algebras[Algebra];
		DecimalBox_TextChanged(decimalBox, EventArgs.Empty);
		//_var.Root.Code?.CodeChanged = true;
		parent.SetMinSize();
		parent.Text = "Comparser - Settings";
		reportBox_TextChanged(reportBox, EventArgs.Empty);
		autoBox_TextChanged(reportBox, EventArgs.Empty);
		UpdateAuto();
		UpdateReport();
		UpdateFrameRate();
		UpdatePlot();
		preEvalBox.Checked = true;
		var initTasks = Math.Max(MaxTasks, 1);
		taskBox.Text = initTasks.ToString();
		drawTaskBox.Text = initTasks.ToString();
		mp4TaskBox.Text = initTasks.ToString();
		previewSelect.SelectedIndex = 2;
		plotLabel.Text = "Auto Plot";
		autoLabel.Text = "Auto Build:";
		decLabel.Text = "Decimals:";
		reportLabel.Text = "Report Logs:";

		// blocks scrolling over int from changing their value
		algebraBox.MouseWheel += ComboBox_MouseWheel;
		previewSelect.MouseWheel += ComboBox_MouseWheel;
		
		SetupControl(decimalBox, "How many digit after the decimal point to print?");
		SetupControl(algebraBox, "Choose the Comparser algebra. Currently all are merged into a polymorphic one. Later might add Dual variant.");
		SetupControl(autoBox, "The Code window will auto-parse itself after that many milliseconds of inactivity, or not?");
		SetupControl(autoButton, "Will the Code window auto-parse itself?");
		SetupControl(reportBox, "I think this just delays the coloring of the code...? Maybe it's obsolete.");
		SetupControl(reportButton, "What will parsing of the Code window report into the log panel?");
		SetupControl(plotBox, "The Plotter window will auto-plot itself after that many milliseconds of inactivity, or not?");
		SetupControl(plotButton, "Will the Plotter window auto-plot itself?");
		SetupControl(previewSelect, "How large percentage of threads will be assigned to work at fast small resolution previews, that are later replaced by the slower final full resolution?");
		SetupControl(localeSelect, "Choose a localization language");
		
		SetupControl(preEvalBox, "Will constant expression that don't contain any dynamic argument be pre-evaluated into constants? Might not work with meta functions like 'eval'.");
		SetupControl(allowStringsBox, "This could partially disable string operations, could remove some unnecessary fluff from purely numeric expressions, increasing plotter performance.");
		SetupControl(xMemBox, "Will the Evaluation expression (the bottom textbox) attempt to re-use previously evaluated pixels and move them to a new location when only a viewport is moved?\n"
			+ "(This feature is not fully debugged yet, and dones't work correctly yet)");
		
		SetupControl(ssBox, "How many times o sub-divide the image horizontally and vertically for more samples per pixel? A value 3 will take 9 samples per pixel.");
		SetupControl(taskBox, "How many CPU threads are allowed to start for each frame's evaluation? (The Evaluation expression in the bottom textbox)");
		SetupControl(taskBox, "How much will the evaluation of XY plots be subdivided between each task? Helps smoothen the load if different line regions have different evaluation time like mandelbrot.");
		SetupControl(drawTaskBox, "How many CPU threads are allowed to start for each frame's evaluation? (The Evaluation expression in the bottom textbox)");
		SetupControl(drawChunkBox, "How much will the drawing of images be subdivided between each task? This is unlikely to help, as the Rgb Expression is unlikely to vary in complexity.");
		SetupControl(mp4TaskBox, "How many CPU threads are allowed to start for MP4 export? (This is obsolete and not actually parallelized.)");
		SetupControl(frameRateBox, "The framerate of exported MP4. Also the framerate of the preview animator.");
	}
	#endregion

	#region Events
	private void DecimalBox_TextChanged(object? sender, EventArgs e) {
		var old = Decimals;
		_ = int.TryParse(decimalBox.Text, out Decimals);
		Context.SetDecimals(Decimals);
		if (old != Decimals)
			_var.Root.Code?.CodeChanged = true; // re-parse so the prints and expressions update
	}
	/*private void AlgebraBox_SelectedIndexChanged(object? sender, EventArgs e) {
		Algebra = algebraBox.SelectedIndex;
		_var.Root.Code?.CodeChanged = true; // reparse
		Context = _algebras[Algebra];
		_var.Root.Plot?.SetContext();
	}*/

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
	private void chunkBox_TextChanged(object sender, EventArgs e) {
		if (!int.TryParse(chunkBox.Text, out Chunks) || Chunks < 1) Chunks = 1;
	}
	private void ssBox_TextChanged(object sender, EventArgs e) {
		if (!int.TryParse(ssBox.Text, out SuperSampling) || SuperSampling < 1) SuperSampling = 1;
		GetVar().Root.Plot?.ReEval(false);
	}

	private void drawTaskBox_TextChanged(object sender, EventArgs e) {
		if (!int.TryParse(drawTaskBox.Text, out DrawTasks) || DrawTasks < 1) DrawTasks = 1;
		if (DrawTasks > MaxTasks) DrawTasks = MaxTasks;
	}
	private void mp4TaskBox_TextChanged(object sender, EventArgs e) {
		if (!int.TryParse(mp4TaskBox.Text, out Mp4Tasks) || Mp4Tasks < 1) Mp4Tasks = 1;
		if (Mp4Tasks > MaxTasks) Mp4Tasks = MaxTasks;
	}
	private void frameRateBox_TextChanged(object sender, EventArgs e) => UpdateFrameRate();
	private void UpdateFrameRate() {
		if (!int.TryParse(frameRateBox.Text, out FrameRate) || FrameRate < 1) FrameRate = 1;
		GetVar().Root.Plot?.SetFramerate();
	}

	private void drawChunkBox_TextChanged(object sender, EventArgs e) {
		if (!int.TryParse(drawChunkBox.Text, out DrawChunks) || DrawChunks < 1) DrawChunks = 1;
	}
	private void preEvalBox_CheckedChanged(object sender, EventArgs e) {
		_preEvaluate = preEvalBox.Checked;
		SetPreEval();
	}
	private void allowStringsBox_CheckedChanged(object sender, EventArgs e) {
		_allowStrings = allowStringsBox.Checked;
		SetAllowStrings();
	}
	private void plotBox_TextChanged(object sender, EventArgs e) {
		if (!int.TryParse(plotBox.Text, out PlotDelay) || PlotDelay < 100) PlotDelay = 100;
	}

	private void plotButton_Click(object sender, EventArgs e) {
		AutoPlot = (AutoPlotting)(((int)AutoPlot + 1) % 3);
		UpdatePlot();
	}
	private void xMemBox_CheckedChanged(object sender, EventArgs e) {
		UseMem = xMemBox.Checked;
		MessageBox.Show("This feature is quite advanced and has not yet been fully debugged, but may be functional.", "WARNING: Not debugged!");
	}

	#endregion

	#region Actions
	public void SetDarkMode() {
		Context.SetDarkMode(_darkMode);
		_var.Root.SetDark(_darkMode);
	}
	public void SetPreEval() => Context.SetPreEvaluate(_preEvaluate);
	public void SetAllowStrings() => Context.SetAllowStrings(_allowStrings);
	private void UpdateAuto() => autoButton.Text = AutoBuild ? "DELEAYED AUTOMATIC" : "MANUAL";
	private void UpdateReport() => reportButton.Text = ReportingMode switch {
		Reporting.Silent => "SILENT",
		Reporting.Timer => "ONLY TIME",
		Reporting.Report => "REPORT STATE",
		_ => "???"
	};
	private void UpdatePlot() => plotButton.Text = AutoPlot switch {
		AutoPlotting.Manual => "MANUAL",
		AutoPlotting.OnlyCursor => "ONLY CURSOR",
		AutoPlotting.DelayedAuto => "DELAYED AUTOMATIC",
		_ => "???"
	};
	#endregion

	public static void ComboBox_MouseWheel(object? sender, MouseEventArgs e) => ((HandledMouseEventArgs)e).Handled = true;

	private void SettingsPanel_Load(object sender, EventArgs e) { }

	private void previewSelect_SelectedIndexChanged(object sender, EventArgs e) => PreviewLoad = Math.Max(previewSelect.SelectedIndex, 0);
	public bool Undo() => true;
	public bool Redo() => true;
	private void SetupControl(Control control, string tip) {
		// Add tooltip and set the next tabIndex
		//myControls.Add(control, tip);
		toolTips.SetToolTip(control, tip/*L(tip)*/); // TODO add localization support L(key)
		control.TabIndex = ++_controlTabIndex;
	}
	
}