using System.ComponentModel;
namespace Comparser;
partial class ExpressionControl {
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
		expBox = new System.Windows.Forms.Button();
		SuspendLayout();
		// 
		// expBox
		// 
		expBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		expBox.Location = new System.Drawing.Point(3, 3);
		expBox.Name = "expBox";
		expBox.Size = new System.Drawing.Size(314, 23);
		expBox.TabIndex = 3;
		expBox.Text = "ADD EXPRESSION";
		expBox.UseVisualStyleBackColor = true;
		expBox.Click += ExpAdd;
		// 
		// ExpressionControl
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		BackColor = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)64)), ((int)((byte)64)));
		Controls.Add(expBox);
		Size = new System.Drawing.Size(320, 320);
		ResumeLayout(false);
	}

	#endregion
	private System.Windows.Forms.Button expBox;
}