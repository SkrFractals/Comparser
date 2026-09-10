using System.ComponentModel;
namespace Comparser.Forms;
partial class MenuPanel {
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
		setButton = new Button();
		codeButton = new Button();
		expButton = new Button();
		plotButton = new Button();
		SuspendLayout();
		// 
		// setButton
		// 
		setButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		setButton.Location = new Point(4, 4);
		setButton.Margin = new Padding(4);
		setButton.Name = "setButton";
		setButton.Size = new Size(313, 32);
		setButton.TabIndex = 9;
		setButton.Text = "SETTINGS";
		setButton.UseMnemonic = false;
		setButton.UseVisualStyleBackColor = true;
		setButton.Click += SetButton_Click;
		// 
		// codeButton
		// 
		codeButton.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		codeButton.Location = new Point(4, 44);
		codeButton.Margin = new Padding(4);
		codeButton.Name = "codeButton";
		codeButton.Size = new Size(313, 192);
		codeButton.TabIndex = 10;
		codeButton.Text = "CODE";
		codeButton.UseMnemonic = false;
		codeButton.UseVisualStyleBackColor = true;
		codeButton.Click += CodeButton_Click;
		// 
		// expButton
		// 
		expButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		expButton.Location = new Point(3, 244);
		expButton.Margin = new Padding(4);
		expButton.Name = "expButton";
		expButton.Size = new Size(313, 32);
		expButton.TabIndex = 11;
		expButton.Text = "EXPRESSIONS";
		expButton.UseMnemonic = false;
		expButton.UseVisualStyleBackColor = true;
		expButton.Click += ExpButton_Click;
		// 
		// plotButton
		// 
		plotButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		plotButton.Location = new Point(4, 284);
		plotButton.Margin = new Padding(4);
		plotButton.Name = "plotButton";
		plotButton.Size = new Size(313, 32);
		plotButton.TabIndex = 12;
		plotButton.Text = "PLOT";
		plotButton.UseMnemonic = false;
		plotButton.UseVisualStyleBackColor = true;
		plotButton.Click += PlotButton_Click;
		// 
		// MenuControl
		// 
		AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(64, 64, 64);
		Controls.Add(setButton);
		Controls.Add(codeButton);
		Controls.Add(expButton);
		Controls.Add(plotButton);
		Size = new Size(320, 320);
		Name = "MenuControl";
		ResumeLayout(false);
	}

	#endregion

	public Button setButton;
	public Button codeButton;
	public Button expButton;
	public Button plotButton;
}