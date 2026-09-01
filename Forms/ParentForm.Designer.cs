namespace Comparser.Forms;
partial class ParentForm {
	/// <summary>
	///  Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary>
	///  Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing) {
		if (disposing && (components != null)) { components.Dispose(); }
		base.Dispose(disposing);
	}

	#region Windows Form Designer generated code
	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent() {
		innerPanel = new System.Windows.Forms.Panel();
		outerPanel = new System.Windows.Forms.Panel();
		outerPanel.SuspendLayout();
		SuspendLayout();
		// 
		// innerPanel
		// 
		innerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		innerPanel.BackColor = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)64)), ((int)((byte)64)));
		innerPanel.Location = new System.Drawing.Point(3, 3);
		innerPanel.Name = "innerPanel";
		innerPanel.Size = new System.Drawing.Size(320, 320);
		innerPanel.TabIndex = 2;
		// 
		// outerPanel
		// 
		outerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		outerPanel.AutoScroll = true;
		outerPanel.BackColor = System.Drawing.Color.White;
		outerPanel.Controls.Add(innerPanel);
		outerPanel.Location = new System.Drawing.Point(12, 12);
		outerPanel.Name = "outerPanel";
		outerPanel.Size = new System.Drawing.Size(326, 326);
		outerPanel.TabIndex = 3;
		// 
		// ParentForm
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		BackColor = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)64)), ((int)((byte)64)));
		ClientSize = new System.Drawing.Size(350, 350);
		Controls.Add(outerPanel);
		Text = "Comparser - Complex Computer Parser";
		FormClosing += ParentForm_FormClosing;
		outerPanel.ResumeLayout(false);
		ResumeLayout(false);
	}
	#endregion

	private System.Windows.Forms.Panel innerPanel;
	private System.Windows.Forms.Panel outerPanel;
}