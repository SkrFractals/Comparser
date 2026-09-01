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
		darkButton = new System.Windows.Forms.Button();
		algebraBox = new System.Windows.Forms.ComboBox();
		decLabel = new System.Windows.Forms.Label();
		decimalBox = new System.Windows.Forms.TextBox();
		autoButton = new System.Windows.Forms.Button();
		reportButton = new System.Windows.Forms.Button();
		autoLabel = new System.Windows.Forms.Label();
		label3 = new System.Windows.Forms.Label();
		autoBox = new System.Windows.Forms.TextBox();
		reportBox = new System.Windows.Forms.TextBox();
		SuspendLayout();
		// 
		// darkButton
		// 
		darkButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
		darkButton.Location = new System.Drawing.Point(287, 3);
		darkButton.Name = "darkButton";
		darkButton.Size = new System.Drawing.Size(30, 23);
		darkButton.TabIndex = 2;
		darkButton.Text = "L";
		darkButton.UseMnemonic = false;
		darkButton.UseVisualStyleBackColor = true;
		darkButton.Click += darkButton_Click;
		// 
		// algebraBox
		// 
		algebraBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
		algebraBox.FormattingEnabled = true;
		algebraBox.Items.AddRange(new object[] { "REAL", "COMPLEX", "QUATERNION" });
		algebraBox.Location = new System.Drawing.Point(163, 4);
		algebraBox.Name = "algebraBox";
		algebraBox.Size = new System.Drawing.Size(118, 23);
		algebraBox.TabIndex = 1;
		algebraBox.Text = "COMPLEX";
		algebraBox.SelectedIndexChanged += AlgebraBox_SelectedIndexChanged;
		// 
		// decLabel
		// 
		decLabel.AutoSize = true;
		decLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)238));
		decLabel.Location = new System.Drawing.Point(3, 6);
		decLabel.Name = "decLabel";
		decLabel.Size = new System.Drawing.Size(60, 15);
		decLabel.TabIndex = 3;
		decLabel.Text = "Decimals:";
		decLabel.UseMnemonic = false;
		// 
		// decimalBox
		// 
		decimalBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		decimalBox.Location = new System.Drawing.Point(100, 4);
		decimalBox.Name = "decimalBox";
		decimalBox.Size = new System.Drawing.Size(57, 23);
		decimalBox.TabIndex = 0;
		decimalBox.Text = "3";
		decimalBox.TextChanged += DecimalBox_TextChanged;
		// 
		// autoButton
		// 
		autoButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
		autoButton.Location = new System.Drawing.Point(163, 33);
		autoButton.Name = "autoButton";
		autoButton.Size = new System.Drawing.Size(154, 23);
		autoButton.TabIndex = 4;
		autoButton.Text = "DELAYED AUTOMATIC";
		autoButton.UseMnemonic = false;
		autoButton.UseVisualStyleBackColor = true;
		autoButton.Click += autoButton_Click;
		// 
		// reportButton
		// 
		reportButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
		reportButton.Location = new System.Drawing.Point(163, 62);
		reportButton.Name = "reportButton";
		reportButton.Size = new System.Drawing.Size(154, 23);
		reportButton.TabIndex = 6;
		reportButton.Text = "ONGOING";
		reportButton.UseMnemonic = false;
		reportButton.UseVisualStyleBackColor = true;
		reportButton.Click += reportButton_Click;
		// 
		// autoLabel
		// 
		autoLabel.AutoSize = true;
		autoLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)238));
		autoLabel.Location = new System.Drawing.Point(3, 37);
		autoLabel.Name = "autoLabel";
		autoLabel.Size = new System.Drawing.Size(55, 15);
		autoLabel.TabIndex = 7;
		autoLabel.Text = "Building:";
		autoLabel.UseMnemonic = false;
		// 
		// label3
		// 
		label3.AutoSize = true;
		label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)238));
		label3.Location = new System.Drawing.Point(3, 66);
		label3.Name = "label3";
		label3.Size = new System.Drawing.Size(80, 15);
		label3.TabIndex = 9;
		label3.Text = "Build Report:";
		label3.UseMnemonic = false;
		// 
		// autoBox
		// 
		autoBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		autoBox.Location = new System.Drawing.Point(100, 33);
		autoBox.Name = "autoBox";
		autoBox.Size = new System.Drawing.Size(57, 23);
		autoBox.TabIndex = 10;
		autoBox.Text = "5000";
		autoBox.TextChanged += autoBox_TextChanged;
		// 
		// reportBox
		// 
		reportBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		reportBox.Location = new System.Drawing.Point(100, 62);
		reportBox.Name = "reportBox";
		reportBox.Size = new System.Drawing.Size(57, 23);
		reportBox.TabIndex = 12;
		reportBox.Text = "1000";
		reportBox.TextChanged += reportBox_TextChanged;
		// 
		// SettingsControl
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		BackColor = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)64)), ((int)((byte)64)));
		Controls.Add(reportBox);
		Controls.Add(autoBox);
		Controls.Add(label3);
		Controls.Add(autoLabel);
		Controls.Add(reportButton);
		Controls.Add(autoButton);
		Controls.Add(decLabel);
		Controls.Add(decimalBox);
		Controls.Add(algebraBox);
		Controls.Add(darkButton);
		ResumeLayout(false);
		PerformLayout();
	}
	private System.Windows.Forms.Label autoLabel;
	private System.Windows.Forms.Label label3;
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