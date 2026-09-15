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
        taskLabel = new Label();
        autoBox = new TextBox();
        taskBox = new TextBox();
        reportBox = new TextBox();
        plotButton = new Button();
        plotBox = new TextBox();
        plotLabel = new Label();
        xMemBox = new CheckBox();
        xyMemBox = new CheckBox();
        preEvalBox = new CheckBox();
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
        algebraBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
        algebraBox.FormattingEnabled = true;
        algebraBox.Items.AddRange(new object[] { "REAL", "COMPLEX", "QUATERNION" });
        algebraBox.Location = new Point(163, 3);
        algebraBox.Name = "algebraBox";
        algebraBox.Text = "QUATERNION";
        algebraBox.Size = new Size(154, 23);
        algebraBox.Enabled = false;
        algebraBox.TabIndex = 1;
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
        // taskLabel
        // 
        taskLabel.AutoSize = true;
        taskLabel.Font = new Font("Consolas", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
        taskLabel.Location = new Point(195, 124);
        taskLabel.Name = "taskLabel";
        taskLabel.Size = new Size(49, 15);
        taskLabel.TabIndex = 9;
        taskLabel.Text = "Tasks:";
        taskLabel.UseMnemonic = false;
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
        taskBox.Location = new Point(250, 121);
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
        xMemBox.Location = new Point(9, 121);
        xMemBox.Name = "xMemBox";
        xMemBox.Size = new Size(101, 19);
        xMemBox.TabIndex = 16;
        xMemBox.Text = "Memory X->Y";
        xMemBox.UseVisualStyleBackColor = true;
        xMemBox.CheckedChanged += xMemBox_CheckedChanged;
        // 
        // xyMemBox
        // 
        xyMemBox.AutoSize = true;
        xyMemBox.Location = new Point(9, 146);
        xyMemBox.Name = "xyMemBox";
        xyMemBox.Size = new Size(123, 19);
        xyMemBox.TabIndex = 17;
        xyMemBox.Text = "XY->RGB Memory";
        xyMemBox.UseVisualStyleBackColor = true;
        xyMemBox.CheckedChanged += xyMemBox_CheckedChanged;
        // 
        // preEvalBox
        // 
        preEvalBox.AutoSize = true;
        preEvalBox.Checked = true;
        preEvalBox.CheckState = CheckState.Checked;
        preEvalBox.Location = new Point(9, 171);
        preEvalBox.Name = "preEvalBox";
        preEvalBox.Size = new Size(92, 19);
        preEvalBox.TabIndex = 18;
        preEvalBox.Text = "Pre-Evaluate";
        preEvalBox.UseVisualStyleBackColor = true;
        // 
        // SettingsPanel
        // 
        AutoScaleMode = AutoScaleMode.None;
        BackColor = Color.FromArgb(64, 64, 64);
        Controls.Add(preEvalBox);
        Controls.Add(xyMemBox);
        Controls.Add(xMemBox);
        Controls.Add(plotLabel);
        Controls.Add(plotBox);
        Controls.Add(plotButton);
        Controls.Add(reportBox);
        Controls.Add(autoBox);
        Controls.Add(taskBox);
        Controls.Add(taskLabel);
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
        ResumeLayout(false);
        PerformLayout();
    }
    private System.Windows.Forms.CheckBox preEvalBox;
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
	private System.Windows.Forms.Label taskLabel;
	private System.Windows.Forms.CheckBox xMemBox;
	private System.Windows.Forms.CheckBox xyMemBox;
}