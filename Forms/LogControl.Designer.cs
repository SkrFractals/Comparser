using System.ComponentModel;
namespace Comparser.Forms;
partial class LogControl {
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
		logBox = new System.Windows.Forms.RichTextBox();
		SuspendLayout();
		// 
		// logBox
		// 
		logBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		logBox.AutoSize = true;
		logBox.DetectUrls = false;
		logBox.Font = new System.Drawing.Font("Consolas", 10F);
		logBox.Location = new System.Drawing.Point(3, 3);
		logBox.Name = "logBox";
		logBox.ReadOnly = true;
		logBox.Size = new System.Drawing.Size(314, 314);
		logBox.TabIndex = 0;
		logBox.Text = "[logs]";
		// 
		// LogControl
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		BackColor = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)64)), ((int)((byte)64)));
		Controls.Add(logBox);
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion
	private System.Windows.Forms.RichTextBox logBox;
}