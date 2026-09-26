using System.ComponentModel;
namespace Comparser.Forms;
partial class SettingsPanel {
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
		components = new System.ComponentModel.Container();
		toolTips = new ToolTip(components);
		darkButton = new Button();
		algebraBox = new ComboBox();
		decLabel = new Label();
		decimalBox = new TextBox();
		autoButton = new Button();
		reportButton = new Button();
		autoLabel = new Label();
		reportLabel = new Label();
		plotTasksLabel = new Label();
		autoBox = new TextBox();
		taskBox = new TextBox();
		reportBox = new TextBox();
		plotButton = new Button();
		plotBox = new TextBox();
		plotLabel = new Label();
		xMemBox = new CheckBox();
		preEvalBox = new CheckBox();
		allowStringsBox = new CheckBox();
		chunkBox = new TextBox();
		drawTasksLabel = new Label();
		drawTaskBox = new TextBox();
		mp4TaskBox = new TextBox();
		drawChunkBox = new TextBox();
		drawChunksLabel = new Label();
		plotChunksLabel = new Label();
		previewSelect = new ComboBox();
		previewLabel = new Label();
		mp4Label = new Label();
		frameRateBox = new TextBox();
		framerateLabel = new Label();
		panel1 = new Panel();
		localeSelect = new ComboBox();
		localeLabel = new Label();
		ssBox = new TextBox();
		ssLabel = new Label();
		panel1.SuspendLayout();
		SuspendLayout();
		// 
		// darkButton
		// 
		darkButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		darkButton.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		darkButton.Location = new Point(797, 3);
		darkButton.Name = "darkButton";
		darkButton.Size = new Size(30, 23);
		darkButton.TabIndex = 2;
		darkButton.Text = "L";
		darkButton.UseMnemonic = false;
		darkButton.UseVisualStyleBackColor = true;
		darkButton.Click += darkButton_Click;
		// 
		// algebraBox
		// 
		algebraBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		algebraBox.Enabled = false;
		algebraBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		algebraBox.FormattingEnabled = true;
		algebraBox.Items.AddRange(new object[] { "REAL", "COMPLEX", "QUATERNION" });
		algebraBox.Location = new Point(143, 3);
		algebraBox.Name = "algebraBox";
		algebraBox.Size = new Size(154, 23);
		algebraBox.TabIndex = 1;
		algebraBox.Text = "QUATERNION";
		// 
		// decLabel
		// 
		decLabel.AutoSize = true;
		decLabel.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		decLabel.Location = new Point(3, 6);
		decLabel.Name = "decLabel";
		decLabel.Size = new Size(70, 15);
		decLabel.TabIndex = 3;
		decLabel.Text = "Decimals:";
		decLabel.UseMnemonic = false;
		// 
		// decimalBox
		// 
		decimalBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		decimalBox.Location = new Point(96, 4);
		decimalBox.Name = "decimalBox";
		decimalBox.Size = new Size(41, 23);
		decimalBox.TabIndex = 0;
		decimalBox.Text = "3";
		decimalBox.TextChanged += DecimalBox_TextChanged;
		// 
		// autoButton
		// 
		autoButton.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		autoButton.Location = new Point(143, 32);
		autoButton.Name = "autoButton";
		autoButton.Size = new Size(154, 23);
		autoButton.TabIndex = 4;
		autoButton.Text = "DELAYED AUTOMATIC";
		autoButton.UseMnemonic = false;
		autoButton.UseVisualStyleBackColor = true;
		autoButton.Click += autoButton_Click;
		// 
		// reportButton
		// 
		reportButton.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		reportButton.Location = new Point(143, 61);
		reportButton.Name = "reportButton";
		reportButton.Size = new Size(154, 23);
		reportButton.TabIndex = 6;
		reportButton.Text = "ONGOING";
		reportButton.UseMnemonic = false;
		reportButton.UseVisualStyleBackColor = true;
		reportButton.Click += reportButton_Click;
		// 
		// autoLabel
		// 
		autoLabel.AutoSize = true;
		autoLabel.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		autoLabel.Location = new Point(3, 35);
		autoLabel.Name = "autoLabel";
		autoLabel.Size = new Size(84, 15);
		autoLabel.TabIndex = 7;
		autoLabel.Text = "Auto Build:";
		autoLabel.UseMnemonic = false;
		// 
		// reportLabel
		// 
		reportLabel.AutoSize = true;
		reportLabel.Font = new Font("Consolas", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
		reportLabel.Location = new Point(3, 65);
		reportLabel.Name = "reportLabel";
		reportLabel.Size = new Size(91, 15);
		reportLabel.TabIndex = 9;
		reportLabel.Text = "Report Logs:";
		reportLabel.UseMnemonic = false;
		// 
		// plotTasksLabel
		// 
		plotTasksLabel.AutoSize = true;
		plotTasksLabel.Font = new Font("Consolas", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
		plotTasksLabel.Location = new Point(140, 211);
		plotTasksLabel.Name = "plotTasksLabel";
		plotTasksLabel.Size = new Size(84, 15);
		plotTasksLabel.TabIndex = 9;
		plotTasksLabel.Text = "Plot Tasks:";
		plotTasksLabel.UseMnemonic = false;
		// 
		// autoBox
		// 
		autoBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		autoBox.Location = new Point(96, 32);
		autoBox.Name = "autoBox";
		autoBox.Size = new Size(41, 23);
		autoBox.TabIndex = 10;
		autoBox.Text = "5000";
		autoBox.TextChanged += autoBox_TextChanged;
		// 
		// taskBox
		// 
		taskBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		taskBox.Location = new Point(230, 207);
		taskBox.Name = "taskBox";
		taskBox.Size = new Size(67, 23);
		taskBox.TabIndex = 10;
		taskBox.Text = "1";
		taskBox.TextChanged += taskBox_TextChanged;
		// 
		// reportBox
		// 
		reportBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		reportBox.Location = new Point(96, 62);
		reportBox.Name = "reportBox";
		reportBox.Size = new Size(41, 23);
		reportBox.TabIndex = 11;
		reportBox.Text = "5000";
		reportBox.TextChanged += reportBox_TextChanged;
		// 
		// plotButton
		// 
		plotButton.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		plotButton.Location = new Point(143, 91);
		plotButton.Name = "plotButton";
		plotButton.Size = new Size(154, 23);
		plotButton.TabIndex = 13;
		plotButton.Text = "DELAYED AUTOMATIC";
		plotButton.UseMnemonic = false;
		plotButton.UseVisualStyleBackColor = true;
		plotButton.Click += plotButton_Click;
		// 
		// plotBox
		// 
		plotBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		plotBox.Location = new Point(96, 92);
		plotBox.Name = "plotBox";
		plotBox.Size = new Size(41, 23);
		plotBox.TabIndex = 14;
		plotBox.Text = "1000";
		plotBox.TextChanged += plotBox_TextChanged;
		// 
		// plotLabel
		// 
		plotLabel.AutoSize = true;
		plotLabel.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		plotLabel.Location = new Point(3, 95);
		plotLabel.Name = "plotLabel";
		plotLabel.Size = new Size(70, 15);
		plotLabel.TabIndex = 15;
		plotLabel.Text = "Auto Plot";
		plotLabel.UseMnemonic = false;
		// 
		// xMemBox
		// 
		xMemBox.AutoSize = true;
		xMemBox.Location = new Point(9, 267);
		xMemBox.Name = "xMemBox";
		xMemBox.Size = new Size(134, 19);
		xMemBox.TabIndex = 16;
		xMemBox.Text = "Use Plotted Memory";
		xMemBox.UseVisualStyleBackColor = true;
		xMemBox.CheckedChanged += xMemBox_CheckedChanged;
		// 
		// preEvalBox
		// 
		preEvalBox.AutoSize = true;
		preEvalBox.Checked = true;
		preEvalBox.CheckState = CheckState.Checked;
		preEvalBox.Location = new Point(9, 219);
		preEvalBox.Name = "preEvalBox";
		preEvalBox.Size = new Size(92, 19);
		preEvalBox.TabIndex = 18;
		preEvalBox.Text = "Pre-Evaluate";
		preEvalBox.UseVisualStyleBackColor = true;
		preEvalBox.CheckedChanged += preEvalBox_CheckedChanged;
		// 
		// allowStringsBox
		// 
		allowStringsBox.AutoSize = true;
		allowStringsBox.Checked = true;
		allowStringsBox.CheckState = CheckState.Checked;
		allowStringsBox.Location = new Point(9, 242);
		allowStringsBox.Name = "allowStringsBox";
		allowStringsBox.Size = new Size(95, 19);
		allowStringsBox.TabIndex = 18;
		allowStringsBox.Text = "Allow Strings";
		allowStringsBox.UseVisualStyleBackColor = true;
		allowStringsBox.CheckedChanged += allowStringsBox_CheckedChanged;
		// 
		// chunkBox
		// 
		chunkBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		chunkBox.Location = new Point(230, 236);
		chunkBox.Name = "chunkBox";
		chunkBox.Size = new Size(67, 23);
		chunkBox.TabIndex = 19;
		chunkBox.Text = "8";
		chunkBox.TextChanged += chunkBox_TextChanged;
		// 
		// drawTasksLabel
		// 
		drawTasksLabel.AutoSize = true;
		drawTasksLabel.Font = new Font("Consolas", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
		drawTasksLabel.Location = new Point(140, 268);
		drawTasksLabel.Name = "drawTasksLabel";
		drawTasksLabel.Size = new Size(84, 15);
		drawTasksLabel.TabIndex = 20;
		drawTasksLabel.Text = "Draw Tasks:";
		drawTasksLabel.UseMnemonic = false;
		// 
		// drawTaskBox
		// 
		drawTaskBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		drawTaskBox.Location = new Point(230, 265);
		drawTaskBox.Name = "drawTaskBox";
		drawTaskBox.Size = new Size(67, 23);
		drawTaskBox.TabIndex = 21;
		drawTaskBox.Text = "1";
		drawTaskBox.TextChanged += drawTaskBox_TextChanged;
		// 
		// mp4TaskBox
		// 
		mp4TaskBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		mp4TaskBox.Location = new Point(230, 323);
		mp4TaskBox.Name = "mp4TaskBox";
		mp4TaskBox.Size = new Size(67, 23);
		mp4TaskBox.TabIndex = 21;
		mp4TaskBox.Text = "1";
		mp4TaskBox.TextChanged += mp4TaskBox_TextChanged;
		// 
		// drawChunkBox
		// 
		drawChunkBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		drawChunkBox.Location = new Point(230, 294);
		drawChunkBox.Name = "drawChunkBox";
		drawChunkBox.Size = new Size(67, 23);
		drawChunkBox.TabIndex = 22;
		drawChunkBox.Text = "1";
		drawChunkBox.TextChanged += drawChunkBox_TextChanged;
		// 
		// drawChunksLabel
		// 
		drawChunksLabel.AutoSize = true;
		drawChunksLabel.Font = new Font("Consolas", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
		drawChunksLabel.Location = new Point(133, 297);
		drawChunksLabel.Name = "drawChunksLabel";
		drawChunksLabel.Size = new Size(91, 15);
		drawChunksLabel.TabIndex = 23;
		drawChunksLabel.Text = "Draw Chunks:";
		drawChunksLabel.UseMnemonic = false;
		// 
		// plotChunksLabel
		// 
		plotChunksLabel.AutoSize = true;
		plotChunksLabel.Font = new Font("Consolas", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
		plotChunksLabel.Location = new Point(133, 244);
		plotChunksLabel.Name = "plotChunksLabel";
		plotChunksLabel.Size = new Size(91, 15);
		plotChunksLabel.TabIndex = 24;
		plotChunksLabel.Text = "Plot Chunks:";
		plotChunksLabel.UseMnemonic = false;
		// 
		// previewSelect
		// 
		previewSelect.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		previewSelect.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		previewSelect.FormattingEnabled = true;
		previewSelect.Items.AddRange(new object[] { "No", "1 task", "33%", "50%", "66%" });
		previewSelect.Location = new Point(143, 120);
		previewSelect.Name = "previewSelect";
		previewSelect.Size = new Size(154, 23);
		previewSelect.TabIndex = 25;
		previewSelect.Text = "50%";
		previewSelect.SelectedIndexChanged += previewSelect_SelectedIndexChanged;
		// 
		// previewLabel
		// 
		previewLabel.AutoSize = true;
		previewLabel.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		previewLabel.Location = new Point(3, 123);
		previewLabel.Name = "previewLabel";
		previewLabel.Size = new Size(133, 15);
		previewLabel.TabIndex = 26;
		previewLabel.Text = "Preview Sequences:";
		previewLabel.UseMnemonic = false;
		// 
		// mp4Label
		// 
		mp4Label.AutoSize = true;
		mp4Label.Font = new Font("Consolas", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
		mp4Label.Location = new Point(126, 326);
		mp4Label.Name = "mp4Label";
		mp4Label.Size = new Size(98, 15);
		mp4Label.TabIndex = 27;
		mp4Label.Text = "Export Tasks:";
		mp4Label.UseMnemonic = false;
		// 
		// frameRateBox
		// 
		frameRateBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		frameRateBox.Location = new Point(230, 352);
		frameRateBox.Name = "frameRateBox";
		frameRateBox.Size = new Size(67, 23);
		frameRateBox.TabIndex = 28;
		frameRateBox.Text = "30";
		frameRateBox.TextChanged += frameRateBox_TextChanged;
		// 
		// framerateLabel
		// 
		framerateLabel.AutoSize = true;
		framerateLabel.Font = new Font("Consolas", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
		framerateLabel.Location = new Point(42, 355);
		framerateLabel.Name = "framerateLabel";
		framerateLabel.Size = new Size(182, 15);
		framerateLabel.TabIndex = 29;
		framerateLabel.Text = "Frame Rate (Mp4+preview):";
		framerateLabel.UseMnemonic = false;
		// 
		// panel1
		// 
		panel1.AutoScroll = true;
		panel1.Controls.Add(ssLabel);
		panel1.Controls.Add(ssBox);
		panel1.Controls.Add(localeLabel);
		panel1.Controls.Add(localeSelect);
		panel1.Controls.Add(algebraBox);
		panel1.Controls.Add(framerateLabel);
		panel1.Controls.Add(mp4TaskBox);
		panel1.Controls.Add(frameRateBox);
		panel1.Controls.Add(allowStringsBox);
		panel1.Controls.Add(mp4Label);
		panel1.Controls.Add(decimalBox);
		panel1.Controls.Add(previewLabel);
		panel1.Controls.Add(decLabel);
		panel1.Controls.Add(previewSelect);
		panel1.Controls.Add(autoButton);
		panel1.Controls.Add(plotChunksLabel);
		panel1.Controls.Add(reportButton);
		panel1.Controls.Add(drawChunksLabel);
		panel1.Controls.Add(autoLabel);
		panel1.Controls.Add(drawChunkBox);
		panel1.Controls.Add(reportLabel);
		panel1.Controls.Add(drawTaskBox);
		panel1.Controls.Add(plotTasksLabel);
		panel1.Controls.Add(drawTasksLabel);
		panel1.Controls.Add(taskBox);
		panel1.Controls.Add(chunkBox);
		panel1.Controls.Add(autoBox);
		panel1.Controls.Add(preEvalBox);
		panel1.Controls.Add(reportBox);
		panel1.Controls.Add(xMemBox);
		panel1.Controls.Add(plotButton);
		panel1.Controls.Add(plotLabel);
		panel1.Controls.Add(plotBox);
		panel1.Location = new Point(0, 0);
		panel1.Name = "panel1";
		panel1.Size = new Size(320, 320);
		panel1.TabIndex = 30;
		// 
		// localeSelect
		// 
		localeSelect.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		localeSelect.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		localeSelect.FormattingEnabled = true;
		localeSelect.Items.AddRange(new object[] { "No", "1 task", "33%", "50%", "66%" });
		localeSelect.Location = new Point(143, 149);
		localeSelect.Name = "localeSelect";
		localeSelect.Size = new Size(154, 23);
		localeSelect.TabIndex = 30;
		localeSelect.Text = "English";
		// 
		// localeLabel
		// 
		localeLabel.AutoSize = true;
		localeLabel.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		localeLabel.Location = new Point(0, 152);
		localeLabel.Name = "localeLabel";
		localeLabel.Size = new Size(70, 15);
		localeLabel.TabIndex = 31;
		localeLabel.Text = "Language:";
		localeLabel.UseMnemonic = false;
		// 
		// ssBox
		// 
		ssBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		ssBox.Location = new Point(230, 178);
		ssBox.Name = "ssBox";
		ssBox.Size = new Size(67, 23);
		ssBox.TabIndex = 32;
		ssBox.Text = "1";
		ssBox.TextChanged += ssBox_TextChanged;
		// 
		// ssLabel
		// 
		ssLabel.AutoSize = true;
		ssLabel.Font = new Font("Consolas", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
		ssLabel.Location = new Point(112, 181);
		ssLabel.Name = "ssLabel";
		ssLabel.Size = new Size(112, 15);
		ssLabel.TabIndex = 33;
		ssLabel.Text = "Super Sampling:";
		ssLabel.UseMnemonic = false;
		// 
		// SettingsPanel
		// 
		AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(64, 64, 64);
		Controls.Add(panel1);
		Controls.Add(darkButton);
		Location = new Point(15, 15);
		Name = "SettingsPanel";
		Size = new Size(320, 320);
		Load += SettingsPanel_Load;
		panel1.ResumeLayout(false);
		panel1.PerformLayout();
		ResumeLayout(false);
	}
	private ToolTip toolTips;
	private System.Windows.Forms.CheckBox preEvalBox;
	private System.Windows.Forms.CheckBox allowStringsBox;
	private System.Windows.Forms.Label autoLabel;
	private System.Windows.Forms.Label reportLabel;
	private System.Windows.Forms.TextBox autoBox;
	private System.Windows.Forms.TextBox taskBox;
	private System.Windows.Forms.TextBox reportBox;
	private System.Windows.Forms.Button reportButton;
	private System.Windows.Forms.Button autoButton;
	#endregion
	private System.Windows.Forms.Button darkButton;
	private System.Windows.Forms.TextBox decimalBox;
	private System.Windows.Forms.Label decLabel;
	private System.Windows.Forms.ComboBox algebraBox;
	private System.Windows.Forms.Button plotButton;
	private System.Windows.Forms.TextBox plotBox;
	private System.Windows.Forms.Label plotLabel;
	private System.Windows.Forms.Label plotTasksLabel;
	private System.Windows.Forms.CheckBox xMemBox;
	private TextBox chunkBox;
	private Label drawTasksLabel;
	private TextBox drawTaskBox;
	private TextBox mp4TaskBox;
	private TextBox drawChunkBox;
	private Label drawChunksLabel;
	private Label plotChunksLabel;
	private ComboBox previewSelect;
	private Label previewLabel;
	private Label mp4Label;
	private TextBox frameRateBox;
	private Label framerateLabel;
	private Panel panel1;
	private Label localeLabel;
	private ComboBox localeSelect;
	private Label ssLabel;
	private TextBox ssBox;
}