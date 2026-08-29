using System.ComponentModel;
namespace Comparser;
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
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		BackColor = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)64)), ((int)((byte)64)));
		Size = new System.Drawing.Size(320, 320);
		Load += ParentControl_Load;
		ResumeLayout(false);
	}

	#endregion
}