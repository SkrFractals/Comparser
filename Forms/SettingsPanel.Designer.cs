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
		darkButton = new System.Windows.Forms.Button();
		algebraBox = new System.Windows.Forms.ComboBox();
		decLabel = new System.Windows.Forms.Label();
		decimalBox = new System.Windows.Forms.TextBox();
		autoButton = new System.Windows.Forms.Button();
		reportButton = new System.Windows.Forms.Button();
		autoLabel = new System.Windows.Forms.Label();
		reportLabel = new System.Windows.Forms.Label();
		autoBox = new System.Windows.Forms.TextBox();
		reportBox = new System.Windows.Forms.TextBox();
		plotButton = new System.Windows.Forms.Button();
		plotBox = new System.Windows.Forms.TextBox();
		plotLabel = new System.Windows.Forms.Label();
		xMemBox = new System.Windows.Forms.CheckBox();
		xyMemBox = new System.Windows.Forms.CheckBox();
		preEvalBox = new System.Windows.Forms.CheckBox();
		SuspendLayout();
		// 
		// darkButton
		// 
		darkButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
		darkButton.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
		darkButton.Location = new System.Drawing.Point(287, 3);
		darkButton.Name = "darkButton";
		darkButton.Size = new System.Drawing.Size(30, 23);
		darkButton.TabIndex = 2;
		darkButton.Text = "L";
		darkButton.UseMnemonic = false;
		darkButton.UseVisualStyleBackColor = true;
		darkButton.Visible = false;
		darkButton.Click += darkButton_Click;
		// 
		// algebraBox
		// 
		algebraBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		algebraBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
		algebraBox.FormattingEnabled = true;
		algebraBox.Items.AddRange(new object[] { "REAL", "COMPLEX", "QUATERNION" });
		algebraBox.Location = new System.Drawing.Point(163, 3);
		algebraBox.Name = "algebraBox";
		algebraBox.Size = new System.Drawing.Size(118, 23);
		algebraBox.TabIndex = 1;
		algebraBox.Visible = false;
		algebraBox.SelectedIndexChanged += AlgebraBox_SelectedIndexChanged;
		// 
		// decLabel
		// 
		decLabel.AutoSize = true;
		decLabel.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
		decLabel.Location = new System.Drawing.Point(3, 6);
		decLabel.Name = "decLabel";
		decLabel.Size = new System.Drawing.Size(70, 15);
		decLabel.TabIndex = 3;
		decLabel.Text = "Decimals:";
		decLabel.UseMnemonic = false;
		decLabel.Visible = false;
		// 
		// decimalBox
		// 
		decimalBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
		decimalBox.Location = new System.Drawing.Point(90, 4);
		decimalBox.Name = "decimalBox";
		decimalBox.Size = new System.Drawing.Size(67, 23);
		decimalBox.TabIndex = 0;
		decimalBox.Visible = false;
		decimalBox.Text = "3";
		decimalBox.TextChanged += DecimalBox_TextChanged;
		// 
		// autoButton
		// 
		autoButton.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
		autoButton.Location = new System.Drawing.Point(163, 32);
		autoButton.Name = "autoButton";
		autoButton.Size = new System.Drawing.Size(154, 23);
		autoButton.TabIndex = 4;
		autoButton.Text = "DELAYED AUTOMATIC";
		autoButton.UseMnemonic = false;
		autoButton.UseVisualStyleBackColor = true;
		autoButton.Visible = false;
		autoButton.Click += autoButton_Click;
		// 
		// reportButton
		// 
		reportButton.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
		reportButton.Location = new System.Drawing.Point(163, 61);
		reportButton.Name = "reportButton";
		reportButton.Size = new System.Drawing.Size(154, 23);
		reportButton.TabIndex = 6;
		reportButton.Text = "ONGOING";
		reportButton.UseMnemonic = false;
		reportButton.UseVisualStyleBackColor = true;
		reportButton.Visible = false;
		reportButton.Click += reportButton_Click;
		// 
		// autoLabel
		// 
		autoLabel.AutoSize = true;
		autoLabel.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
		autoLabel.Location = new System.Drawing.Point(3, 35);
		autoLabel.Name = "autoLabel";
		autoLabel.Size = new System.Drawing.Size(84, 15);
		autoLabel.TabIndex = 7;
		autoLabel.Text = "Auto Build:";
		autoLabel.UseMnemonic = false;
		autoLabel.Visible = false;
		// 
		// reportLabel
		// 
		reportLabel.AutoSize = true;
		reportLabel.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)238));
		reportLabel.Location = new System.Drawing.Point(3, 65);
		reportLabel.Name = "reportLabel";
		reportLabel.Size = new System.Drawing.Size(91, 15);
		reportLabel.TabIndex = 9;
		reportLabel.Text = "Report Logs:";
		reportLabel.UseMnemonic = false;
		reportLabel.Visible = false;
		// 
		// autoBox
		// 
		autoBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
		autoBox.Location = new System.Drawing.Point(90, 32);
		autoBox.Name = "autoBox";
		autoBox.Size = new System.Drawing.Size(67, 23);
		autoBox.TabIndex = 10;
		autoBox.Visible = false;
		autoBox.Text = "5000";
		autoBox.TextChanged += autoBox_TextChanged;
		// 
		// reportBox
		// 
		reportBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
		reportBox.Location = new System.Drawing.Point(90, 62);
		reportBox.Name = "reportBox";
		reportBox.Size = new System.Drawing.Size(67, 23);
		reportBox.Text = "5000";
		reportBox.TabIndex = 11;
		reportBox.Visible = false;
		reportBox.TextChanged += reportBox_TextChanged;
		// 
		// plotButton
		// 
		plotButton.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
		plotButton.Location = new System.Drawing.Point(163, 91);
		plotButton.Name = "plotButton";
		plotButton.Size = new System.Drawing.Size(154, 23);
		plotButton.TabIndex = 13;
		plotButton.Text = "DELAYED AUTOMATIC";
		plotButton.UseMnemonic = false;
		plotButton.UseVisualStyleBackColor = true;
		plotButton.Visible = false;
		plotButton.Click += plotButton_Click;
		// 
		// plotBox
		// 
		plotBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
		plotBox.Location = new System.Drawing.Point(90, 92);
		plotBox.Name = "plotBox";
		plotBox.Size = new System.Drawing.Size(67, 23);
		plotBox.TabIndex = 14;
		plotBox.Text = "1000";
		plotBox.Visible = false;
		plotBox.TextChanged += plotBox_TextChanged;
		// 
		// plotLabel
		// 
		plotLabel.AutoSize = true;
		plotLabel.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold);
		plotLabel.Location = new System.Drawing.Point(3, 95);
		plotLabel.Name = "plotLabel";
		plotLabel.Size = new System.Drawing.Size(70, 15);
		plotLabel.TabIndex = 15;
		plotLabel.Text = "Auto Plot";
		plotLabel.UseMnemonic = false;
		plotLabel.Visible = false;
		// 
		// xMemBox
		// 
		xMemBox.AutoSize = true;
		xMemBox.Location = new System.Drawing.Point(9, 121);
		xMemBox.Name = "xMemBox";
		xMemBox.Size = new System.Drawing.Size(101, 19);
		xMemBox.TabIndex = 16;
		xMemBox.Text = "Memory X->Y";
		xMemBox.UseVisualStyleBackColor = true;
		xMemBox.Visible = false;
		xMemBox.CheckedChanged += xMemBox_CheckedChanged;
		// 
		// xyMemBox
		// 
		xyMemBox.AutoSize = true;
		xyMemBox.Location = new System.Drawing.Point(9, 146);
		xyMemBox.Name = "xyMemBox";
		xyMemBox.Size = new System.Drawing.Size(123, 19);
		xyMemBox.TabIndex = 17;
		xyMemBox.Text = "XY->RGB Memory";
		xyMemBox.UseVisualStyleBackColor = true;
		xyMemBox.Visible = false;
		xyMemBox.CheckedChanged += xyMemBox_CheckedChanged;
		// 
		// preEvalBox
		// 
		preEvalBox.AutoSize = true;
		preEvalBox.Checked = true;
		preEvalBox.CheckState = System.Windows.Forms.CheckState.Checked;
		preEvalBox.Location = new System.Drawing.Point(9, 171);
		preEvalBox.Name = "preEvalBox";
		preEvalBox.Size = new System.Drawing.Size(92, 19);
		preEvalBox.TabIndex = 18;
		preEvalBox.Text = "Pre-Evaluate";
		preEvalBox.UseVisualStyleBackColor = true;
		preEvalBox.Visible = false;
		// 
		// SettingsPanel
		// 
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		BackColor = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)64)), ((int)((byte)64)));
		Controls.Add(preEvalBox);
		Controls.Add(xyMemBox);
		Controls.Add(xMemBox);
		Controls.Add(plotLabel);
		Controls.Add(plotBox);
		Controls.Add(plotButton);
		Controls.Add(reportBox);
		Controls.Add(autoBox);
		Controls.Add(reportLabel);
		Controls.Add(autoLabel);
		Controls.Add(reportButton);
		Controls.Add(autoButton);
		Controls.Add(decLabel);
		Controls.Add(decimalBox);
		Controls.Add(algebraBox);
		Controls.Add(darkButton);
		Location = new System.Drawing.Point(15, 15);
		Size = new System.Drawing.Size(320, 320);
		ResumeLayout(false);
		PerformLayout();
	}
	private System.Windows.Forms.CheckBox preEvalBox;
	private System.Windows.Forms.Label autoLabel;
	private System.Windows.Forms.Label reportLabel;
	private System.Windows.Forms.TextBox autoBox;
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
	private System.Windows.Forms.CheckBox xMemBox;
	private System.Windows.Forms.CheckBox xyMemBox;
}