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
		darkButton.UseVisualStyleBackColor = true;
		darkButton.UseMnemonic = false;
		darkButton.Click += darkButton_Click;
		// 
		// algebraBox
		// 
		algebraBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
		algebraBox.FormattingEnabled = true;
		algebraBox.Items.AddRange(new object[] { "REAL", "COMPLEX", "QUATERNION" });
		algebraBox.Location = new System.Drawing.Point(140, 4);
		algebraBox.Name = "algebraBox";
		algebraBox.Size = new System.Drawing.Size(141, 23);
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
		decLabel.UseMnemonic = false;
		decLabel.Text = "Decimals:";
		// 
		// decimalBox
		// 
		decimalBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		decimalBox.Location = new System.Drawing.Point(77, 3);
		decimalBox.Name = "decimalBox";
		decimalBox.Size = new System.Drawing.Size(57, 23);
		decimalBox.TabIndex = 0;
		decimalBox.Text = "3";
		decimalBox.TextChanged += DecimalBox_TextChanged;
		// 
		// SettingsControl
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		BackColor = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)64)), ((int)((byte)64)));
		Controls.Add(decLabel);
		Controls.Add(decimalBox);
		Controls.Add(algebraBox);
		Controls.Add(darkButton);
		Dock = DockStyle.None;
		MinimumSize = new System.Drawing.Size(320, 320);
		MaximumSize = new System.Drawing.Size(320, 320);
		Size = new System.Drawing.Size(320, 320);
		ResumeLayout(false);
		PerformLayout();
	}
	#endregion
	private System.Windows.Forms.Button darkButton;
	private System.Windows.Forms.TextBox decimalBox;
	private Label decLabel;
	private System.Windows.Forms.ComboBox algebraBox;
}