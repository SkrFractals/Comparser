using System.ComponentModel;
namespace Comparser.Forms;
partial class LogPanel {
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
		logBox = new RichTextBox();
		SuspendLayout();
		// 
		// logBox
		// 
		logBox.DetectUrls = false;
		logBox.Dock = DockStyle.Fill;
		logBox.Font = new Font("Consolas", 12F);
		logBox.Location = new Point(0, 0);
		logBox.Name = "logBox";
		logBox.ReadOnly = true;
		logBox.Size = new Size(320, 320);
		logBox.TabIndex = 0;
		logBox.Text = "[logs]";
		// 
		// LogControl
		// 
		AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(64, 64, 64);
		Controls.Add(logBox);
		Size = new Size(320, 320);
		Name = "LogControl";
		ResumeLayout(false);
	}

	#endregion
	private System.Windows.Forms.RichTextBox logBox;
}