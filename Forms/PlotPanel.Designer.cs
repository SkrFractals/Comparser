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
        ((ISupportInitialize)plotBox).BeginInit();
        SuspendLayout();
        // 
        // fps
        // 
        fps.Enabled = true;
        fps.Interval = 50;
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
        plotBox.MouseDown += plotBox_MouseDown;
        plotBox.MouseMove += plotBox_MouseMove;
        plotBox.MouseUp += plotBox_MouseUp;
        plotBox.Paint += PlotBox_Paint;
        plotBox.MouseWheel += PlotBox_MouseWheel;
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
}