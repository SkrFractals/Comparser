using System.ComponentModel;
namespace Comparser.Forms;
partial class SettingsControl {
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
		autoBox = new TextBox();
		reportBox = new TextBox();
		plotButton = new Button();
		plotBox = new TextBox();
		plotLabel = new Label();
		xMemBox = new CheckBox();
		xyMemBox = new CheckBox();
		SuspendLayout();
		// 
		// darkButton
		// 
		darkButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		darkButton.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		darkButton.Location = new Point(287, 3);
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
		algebraBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		algebraBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		algebraBox.FormattingEnabled = true;
		algebraBox.Items.AddRange(new object[] { "REAL", "COMPLEX", "QUATERNION" });
		algebraBox.Location = new Point(163, 4);
		algebraBox.Name = "algebraBox";
		algebraBox.Size = new Size(118, 23);
		algebraBox.TabIndex = 1;
		algebraBox.Text = "COMPLEX";
		algebraBox.SelectedIndexChanged += AlgebraBox_SelectedIndexChanged;
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
		decimalBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		decimalBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		decimalBox.Location = new Point(100, 4);
		decimalBox.Name = "decimalBox";
		decimalBox.Size = new Size(57, 23);
		decimalBox.TabIndex = 0;
		decimalBox.Text = "3";
		decimalBox.TextChanged += DecimalBox_TextChanged;
		// 
		// autoButton
		// 
		autoButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		autoButton.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		autoButton.Location = new Point(163, 33);
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
		reportButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		reportButton.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		reportButton.Location = new Point(163, 62);
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
		autoLabel.Location = new Point(3, 37);
		autoLabel.Name = "autoLabel";
		autoLabel.Size = new Size(70, 15);
		autoLabel.TabIndex = 7;
		autoLabel.Text = "Building:";
		autoLabel.UseMnemonic = false;
		// 
		// reportLabel
		// 
		reportLabel.AutoSize = true;
		reportLabel.Font = new Font("Consolas", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 238);
		reportLabel.Location = new Point(3, 66);
		reportLabel.Name = "reportLabel";
		reportLabel.Size = new Size(98, 15);
		reportLabel.TabIndex = 9;
		reportLabel.Text = "Build Report:";
		reportLabel.UseMnemonic = false;
		// 
		// autoBox
		// 
		autoBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		autoBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		autoBox.Location = new Point(100, 33);
		autoBox.Name = "autoBox";
		autoBox.Size = new Size(57, 23);
		autoBox.TabIndex = 10;
		autoBox.Text = "5000";
		autoBox.TextChanged += autoBox_TextChanged;
		// 
		// reportBox
		// 
		reportBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		reportBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		reportBox.Location = new Point(100, 62);
		reportBox.Name = "reportBox";
		reportBox.Size = new Size(57, 23);
		reportBox.TabIndex = 12;
		reportBox.Text = "1000";
		reportBox.TextChanged += reportBox_TextChanged;
		// 
		// plotButton
		// 
		plotButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
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
		plotBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		plotBox.Font = new Font("Consolas", 9.75F, FontStyle.Bold);
		plotBox.Location = new Point(100, 92);
		plotBox.Name = "plotBox";
		plotBox.Size = new Size(57, 23);
		plotBox.TabIndex = 14;
		plotBox.Text = "5000";
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
		plotLabel.Text = "Plotting:";
		plotLabel.UseMnemonic = false;
		// 
		// xMemBox
		// 
		xMemBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		xMemBox.AutoSize = true;
		xMemBox.Location = new Point(56, 121);
		xMemBox.Name = "xMemBox";
		xMemBox.Size = new Size(101, 19);
		xMemBox.TabIndex = 16;
		xMemBox.Text = "Memory X->Y";
		xMemBox.UseVisualStyleBackColor = true;
		xMemBox.CheckedChanged += xMemBox_CheckedChanged;
		// 
		// xyMemBox
		// 
		xyMemBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		xyMemBox.AutoSize = true;
		xyMemBox.Location = new Point(163, 120);
		xyMemBox.Name = "xyMemBox";
		xyMemBox.Size = new Size(123, 19);
		xyMemBox.TabIndex = 17;
		xyMemBox.Text = "XY->RGB Memory";
		xyMemBox.UseVisualStyleBackColor = true;
		xyMemBox.CheckedChanged += xyMemBox_CheckedChanged;
		// 
		// SettingsControl
		// 
		AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(64, 64, 64);
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
		Name = "SettingsControl";
		ResumeLayout(false);
		PerformLayout();
	}
	private System.Windows.Forms.Label autoLabel;
	private System.Windows.Forms.Label reportLabel;
	private System.Windows.Forms.TextBox autoBox;
	private System.Windows.Forms.TextBox reportBox;
	private System.Windows.Forms.Button reportButton;
	private System.Windows.Forms.Button autoButton;
	#endregion
	private System.Windows.Forms.Button darkButton;
	private System.Windows.Forms.TextBox decimalBox;
	private Label decLabel;
	private System.Windows.Forms.ComboBox algebraBox;
	private Button plotButton;
	private TextBox plotBox;
	private Label plotLabel;
	private CheckBox xMemBox;
	private CheckBox xyMemBox;
}