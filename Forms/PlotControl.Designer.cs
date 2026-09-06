using System.ComponentModel;
namespace Comparser.Forms;
partial class PlotControl {
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
		splitContainer = new SplitContainer();
		plotBox = new PictureBox();
		((ISupportInitialize)splitContainer).BeginInit();
		splitContainer.Panel2.SuspendLayout();
		splitContainer.SuspendLayout();
		((ISupportInitialize)plotBox).BeginInit();
		SuspendLayout();
		// 
		// fps
		// 
		fps.Enabled = true;
		fps.Tick += Fps_Tick;
		// 
		// splitContainer
		// 
		splitContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		splitContainer.FixedPanel = FixedPanel.Panel1;
		splitContainer.Location = new Point(3, 3);
		splitContainer.Name = "splitContainer";
		// 
		// splitContainer.Panel1
		// 
		splitContainer.Panel1.AutoScroll = true;
		// 
		// splitContainer.Panel2
		// 
		splitContainer.Panel2.Controls.Add(plotBox);
		splitContainer.Panel2.SizeChanged += Resized;
		splitContainer.Size = new Size(314, 314);
		splitContainer.SplitterDistance = 256;
		splitContainer.SplitterWidth = 12;
		splitContainer.TabIndex = 3;
		// 
		// plotBox
		// 
		plotBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		plotBox.Location = new Point(3, 3);
		plotBox.Name = "plotBox";
		plotBox.Size = new Size(0, 308);
		plotBox.TabIndex = 1;
		plotBox.TabStop = false;
		// 
		// PlotControl
		// 
		AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(64, 64, 64);
		AutoSize = false;
		Controls.Add(splitContainer);
		Name = "PlotControl";
		splitContainer.Panel2.ResumeLayout(false);
		((ISupportInitialize)splitContainer).EndInit();
		splitContainer.ResumeLayout(false);
		((ISupportInitialize)plotBox).EndInit();
		ResumeLayout(false);
	}
	#endregion
	private System.Windows.Forms.Timer fps;
	private System.Windows.Forms.SplitContainer splitContainer;
	private PictureBox plotBox;
}