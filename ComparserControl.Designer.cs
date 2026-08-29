using System.ComponentModel;
namespace Comparser;
partial class ComparserControl {
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
		components = new System.ComponentModel.Container();
		fps = new System.Windows.Forms.Timer(components);
		logBox = new System.Windows.Forms.RichTextBox();
		codeBox = new System.Windows.Forms.RichTextBox();
		SuspendLayout();
		// 
		// fps
		// 
		fps.Enabled = true;
		fps.Tick += Fps_Tick;
		// 
		// logBox
		// 
		logBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		logBox.AutoSize = true;
		logBox.BackColor = System.Drawing.Color.Black;
		logBox.ForeColor = System.Drawing.Color.White;
		logBox.Location = new System.Drawing.Point(3, 3);
		logBox.Name = "logBox";
		logBox.ReadOnly = true;
		logBox.Size = new System.Drawing.Size(314, 46);
		logBox.TabIndex = 0;
		logBox.Text = "[logs]";
		// 
		// codeBox
		// 
		codeBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		codeBox.BackColor = System.Drawing.Color.Black;
		codeBox.ForeColor = System.Drawing.Color.White;
		codeBox.Location = new System.Drawing.Point(3, 55);
		codeBox.MinimumSize = new System.Drawing.Size(0, 32);
		codeBox.Name = "codeBox";
		codeBox.Size = new System.Drawing.Size(314, 262);
		codeBox.TabIndex = 5;
		codeBox.Text = "";
		codeBox.TextChanged += CodeBox_TextChanged;
		// 
		// ComparserControl
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		BackColor = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)64)), ((int)((byte)64)));
		Controls.Add(logBox);
		Controls.Add(codeBox);
		Size = new System.Drawing.Size(320, 320);
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion
	private System.Windows.Forms.Timer fps;
	private System.Windows.Forms.RichTextBox codeBox;
	private System.Windows.Forms.RichTextBox logBox;
}