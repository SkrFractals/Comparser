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
		SuspendLayout();
		// 
		// darkButton
		// 
		darkButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
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
		decLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 238);
		decLabel.Location = new Point(3, 6);
		decLabel.Name = "decLabel";
		decLabel.Size = new Size(60, 15);
		decLabel.TabIndex = 3;
		decLabel.Text = "Decimals:";
		decLabel.UseMnemonic = false;
		// 
		// decimalBox
		// 
		decimalBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
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
		autoLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 238);
		autoLabel.Location = new Point(3, 37);
		autoLabel.Name = "autoLabel";
		autoLabel.Size = new Size(55, 15);
		autoLabel.TabIndex = 7;
		autoLabel.Text = "Building:";
		autoLabel.UseMnemonic = false;
		// 
		// reportLabel
		// 
		reportLabel.AutoSize = true;
		reportLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 238);
		reportLabel.Location = new Point(3, 66);
		reportLabel.Name = "reportLabel";
		reportLabel.Size = new Size(80, 15);
		reportLabel.TabIndex = 9;
		reportLabel.Text = "Build Report:";
		reportLabel.UseMnemonic = false;
		// 
		// autoBox
		// 
		autoBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
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
		reportBox.Location = new Point(100, 62);
		reportBox.Name = "reportBox";
		reportBox.Size = new Size(57, 23);
		reportBox.TabIndex = 12;
		reportBox.Text = "1000";
		reportBox.TextChanged += reportBox_TextChanged;
		// 
		// SettingsControl
		// 
		AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(64, 64, 64);
		AutoSize = false;
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
}