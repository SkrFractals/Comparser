namespace Comparser.Forms.Core;
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
		var resources = new System.ComponentModel.ComponentResourceManager(typeof(ParentForm));
		innerPanel = new Panel();
		outerPanel = new Panel();
		openFileDialog1 = new OpenFileDialog();
		saveFileDialog1 = new SaveFileDialog();
		outerPanel.SuspendLayout();
		SuspendLayout();
		// 
		// innerPanel
		// 
		innerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		innerPanel.BackColor = Color.FromArgb(64, 64, 64);
		innerPanel.Location = new Point(3, 3);
		innerPanel.Name = "innerPanel";
		innerPanel.Size = new Size(320, 320);
		innerPanel.TabIndex = 2;
		// 
		// outerPanel
		// 
		outerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		outerPanel.AutoScroll = true;
		outerPanel.BackColor = Color.White;
		outerPanel.Controls.Add(innerPanel);
		outerPanel.Location = new Point(12, 12);
		outerPanel.Name = "outerPanel";
		outerPanel.Size = new Size(326, 326);
		outerPanel.TabIndex = 3;
		// 
		// openFileDialog1
		// 
		openFileDialog1.FileName = "openFileDialog1";
		// 
		// ParentForm
		// 
		AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(64, 64, 64);
		ClientSize = new Size(350, 350);
		Controls.Add(outerPanel);
		Icon = (Icon)resources.GetObject("$this.Icon");
		MinimumSize = new Size(120, 120);
		Name = "ParentForm";
		Text = "Comparser - Complex Computer Parser";
		FormClosing += ParentForm_FormClosing;
		outerPanel.ResumeLayout(false);
		ResumeLayout(false);
	}
	#endregion

	private System.Windows.Forms.Panel innerPanel;
	private System.Windows.Forms.Panel outerPanel;
	private OpenFileDialog openFileDialog1;
	private SaveFileDialog saveFileDialog1;
}