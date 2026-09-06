using System.ComponentModel;
namespace Comparser.Forms;
partial class ParentControl {
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
		SuspendLayout();
		// 
		// ParentControl
		// 
		BackColor = Color.FromArgb(64, 64, 64);
		AutoScaleMode = AutoScaleMode.None;
		AutoSize = false;
		MaximumSize = new Size(320, 320);
		MinimumSize = new Size(320, 320);
		Name = "ParentControl";
		Size = new Size(320, 320);
		Load += ParentControl_Load;
		ResumeLayout(false);
	}

	#endregion
}