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
		algebraBox.Location = new Point(163, 3);
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
		decimalBox.Location = new Point(90, 4);
		decimalBox.Name = "decimalBox";
		decimalBox.Size = new Size(67, 23);
		decimalBox.TabIndex = 0;
		decimalBox.Text = "3";
		decimalBox.TextChanged += DecimalBox_TextChanged;
		// 
		// autoButton
		// 
		autoButton.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		autoButton.Location = new Point(163, 32);
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
		reportButton.Location = new Point(163, 61);
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
		plotTasksLabel.Location = new Point(160, 153);
		plotTasksLabel.Name = "plotTasksLabel";
		plotTasksLabel.Size = new Size(84, 15);
		plotTasksLabel.TabIndex = 9;
		plotTasksLabel.Text = "Plot Tasks:";
		plotTasksLabel.UseMnemonic = false;
		// 
		// autoBox
		// 
		autoBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		autoBox.Location = new Point(90, 32);
		autoBox.Name = "autoBox";
		autoBox.Size = new Size(67, 23);
		autoBox.TabIndex = 10;
		autoBox.Text = "5000";
		autoBox.TextChanged += autoBox_TextChanged;
		// 
		// taskBox
		// 
		taskBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		taskBox.Location = new Point(250, 149);
		taskBox.Name = "taskBox";
		taskBox.Size = new Size(67, 23);
		taskBox.TabIndex = 10;
		taskBox.Text = "1";
		taskBox.TextChanged += taskBox_TextChanged;
		// 
		// reportBox
		// 
		reportBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		reportBox.Location = new Point(90, 62);
		reportBox.Name = "reportBox";
		reportBox.Size = new Size(67, 23);
		reportBox.TabIndex = 11;
		reportBox.Text = "5000";
		reportBox.TextChanged += reportBox_TextChanged;
		// 
		// plotButton
		// 
		plotButton.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		plotButton.Location = new Point(163, 91);
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
		plotBox.Location = new Point(90, 92);
		plotBox.Name = "plotBox";
		plotBox.Size = new Size(67, 23);
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
		xMemBox.Location = new Point(9, 172);
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
		preEvalBox.Location = new Point(9, 147);
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
		allowStringsBox.Location = new Point(9, 170);
		allowStringsBox.Name = "allowStringsBox";
		allowStringsBox.Size = new Size(92, 19);
		allowStringsBox.TabIndex = 18;
		allowStringsBox.Text = "Pre-Evaluate";
		allowStringsBox.UseVisualStyleBackColor = true;
		allowStringsBox.CheckedChanged += allowStringsBox_CheckedChanged;
		// 
		// chunkBox
		// 
		chunkBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		chunkBox.Location = new Point(250, 178);
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
		drawTasksLabel.Location = new Point(160, 210);
		drawTasksLabel.Name = "drawTasksLabel";
		drawTasksLabel.Size = new Size(84, 15);
		drawTasksLabel.TabIndex = 20;
		drawTasksLabel.Text = "Draw Tasks:";
		drawTasksLabel.UseMnemonic = false;
		// 
		// drawTaskBox
		// 
		drawTaskBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		drawTaskBox.Location = new Point(250, 207);
		drawTaskBox.Name = "drawTaskBox";
		drawTaskBox.Size = new Size(67, 23);
		drawTaskBox.TabIndex = 21;
		drawTaskBox.Text = "1";
		drawTaskBox.TextChanged += drawTaskBox_TextChanged;
		// 
		// mp4TaskBox
		// 
		mp4TaskBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		mp4TaskBox.Location = new Point(250, 257);
		mp4TaskBox.Name = "mp4TaskBox";
		mp4TaskBox.Size = new Size(67, 23);
		mp4TaskBox.TabIndex = 21;
		mp4TaskBox.Text = "1";
		mp4TaskBox.TextChanged += mp4TaskBox_TextChanged;
		// 
		// drawChunkBox
		// 
		drawChunkBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		drawChunkBox.Location = new Point(250, 236);
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
		drawChunksLabel.Location = new Point(153, 239);
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
		plotChunksLabel.Location = new Point(153, 181);
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
		previewSelect.Location = new Point(163, 120);
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
		// SettingsPanel
		// 
		AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(64, 64, 64);
		Controls.Add(previewLabel);
		Controls.Add(previewSelect);
		Controls.Add(plotChunksLabel);
		Controls.Add(drawChunksLabel);
		Controls.Add(drawChunkBox);
		Controls.Add(drawTaskBox);
		Controls.Add(drawTasksLabel);
		Controls.Add(chunkBox);
		Controls.Add(preEvalBox);
		Controls.Add(xMemBox);
		Controls.Add(plotLabel);
		Controls.Add(plotBox);
		Controls.Add(plotButton);
		Controls.Add(reportBox);
		Controls.Add(autoBox);
		Controls.Add(taskBox);
		Controls.Add(plotTasksLabel);
		Controls.Add(reportLabel);
		Controls.Add(autoLabel);
		Controls.Add(reportButton);
		Controls.Add(autoButton);
		Controls.Add(decLabel);
		Controls.Add(decimalBox);
		Controls.Add(algebraBox);
		Controls.Add(darkButton);
		Location = new Point(15, 15);
		Name = "SettingsPanel";
		Size = new Size(320, 320);
		Load += SettingsPanel_Load;
		ResumeLayout(false);
		PerformLayout();
	}
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
}