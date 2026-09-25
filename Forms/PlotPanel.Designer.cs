using System.ComponentModel;
namespace Comparser.Forms;
partial class PlotPanel {
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	private IContainer components = null;

	/// <summary> 
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing) {
		if (disposing && (components != null)) { components.Dispose(); }
		base.Dispose(disposing);
	}

	#region Component Designer generated code
	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent() {
		components = new Container();
		fps = new System.Windows.Forms.Timer(components);
		plotBox = new PictureBox();
		openPlot = new OpenFileDialog();
		savePlot = new SaveFileDialog();
		saveMp4 = new SaveFileDialog();
		savePngs = new SaveFileDialog();
		savePng = new SaveFileDialog();
		((ISupportInitialize)plotBox).BeginInit();
		SuspendLayout();
		// 
		// fps
		// 
		fps.Enabled = true;
		fps.Interval = 20;
		fps.Tick += Fps_Tick;
		// 
		// plotBox
		// 
		plotBox.Dock = DockStyle.Fill;
		plotBox.Location = new Point(0, 0);
		plotBox.Name = "plotBox";
		plotBox.Size = new Size(320, 320);
		plotBox.SizeMode = PictureBoxSizeMode.StretchImage;
		plotBox.TabIndex = 1;
		plotBox.TabStop = false;
		plotBox.Click += PlotClick;
		plotBox.Paint += PlotBox_Paint;
		plotBox.MouseDown += plotBox_MouseDown;
		plotBox.MouseMove += plotBox_MouseMove;
		plotBox.MouseUp += plotBox_MouseUp;
		plotBox.MouseWheel += PlotBox_MouseWheel;
		// 
		// openPlot
		// 
		openPlot.FileName = "openPlot";
		openPlot.Filter = "COMPARSER plotters (*.plot)|*.plot";
		openPlot.FileOk += openPlot_FileOk;
		// 
		// savePlot
		// 
		savePlot.FileName = "savePlot";
		savePlot.Filter = "COMPARSER plotters (*.plot)|*.plot";
		savePlot.FileOk += savePlot_FileOk;
		// 
		// saveMp4
		// 
		saveMp4.FileName = "saveMp4";
		saveMp4.Filter = "MP4 video (*.mp4)|*.mp4";
		saveMp4.FileOk += saveMp4_FileOk;
		// 
		// savePngs
		// 
		savePngs.FileName = "savePngs";
		savePngs.Filter = "PNG series (*.png)|*.png";
		savePngs.FileOk += savePngs_FileOk;
		// 
		// savePng
		// 
		savePng.FileName = "savePng";
		savePng.Filter = "PNG image (*.png)|*.png";
		savePng.FileOk += savePng_FileOk;
		// 
		// PlotPanel
		// 
		AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(64, 64, 64);
		Controls.Add(plotBox);
		Name = "PlotPanel";
		Size = new Size(320, 320);
		SizeChanged += Resized;
		((ISupportInitialize)plotBox).EndInit();
		ResumeLayout(false);
	}
	#endregion
	private System.Windows.Forms.Timer fps;
	private PictureBox plotBox;
	private OpenFileDialog openPlot;
	private SaveFileDialog savePlot;
	private SaveFileDialog saveMp4;
	private SaveFileDialog savePngs;
	private SaveFileDialog savePng;
}