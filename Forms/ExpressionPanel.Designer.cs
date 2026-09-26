using System.ComponentModel;
namespace Comparser.Forms;
partial class ExpressionPanel {
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
		toolTips = new ToolTip(components);
		expBox = new Button();
		SuspendLayout();
		// 
		// expBox
		// 
		expBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		expBox.Location = new Point(3, 3);
		expBox.Name = "expBox";
		expBox.Size = new Size(314, 32);
		expBox.TabIndex = 3;
		expBox.Text = "ADD EXPRESSION";
		expBox.UseMnemonic = false;
		expBox.UseVisualStyleBackColor = true;
		expBox.Click += ExpAdd;
		// 
		// ExpressionControl
		// 
		AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(64, 64, 64);
		Controls.Add(expBox);
		Size = new Size(320, 320);
		Name = "ExpressionControl";
		ResumeLayout(false);
	}

	#endregion
	private ToolTip toolTips;
	private System.Windows.Forms.Button expBox;
}