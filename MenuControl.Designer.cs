using System.ComponentModel;
namespace Comparser;
partial class MenuControl {
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
		setButton = new System.Windows.Forms.Button();
		codeButton = new System.Windows.Forms.Button();
		expButton = new System.Windows.Forms.Button();
		plotButton = new System.Windows.Forms.Button();
		SuspendLayout();
		// 
		// setButton
		// 
		setButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		setButton.Location = new System.Drawing.Point(3, 3);
		setButton.Name = "setButton";
		setButton.Size = new System.Drawing.Size(314, 23);
		setButton.TabIndex = 0;
		setButton.Text = "SETTINGS";
		setButton.UseVisualStyleBackColor = true;
		setButton.Click += SetButton_Click;
		// 
		// codeButton
		// 
		codeButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		codeButton.Location = new System.Drawing.Point(3, 32);
		codeButton.Name = "codeButton";
		codeButton.Size = new System.Drawing.Size(314, 228);
		codeButton.TabIndex = 2;
		codeButton.Text = "CODE";
		codeButton.UseVisualStyleBackColor = true;
		codeButton.Click += CodeButton_Click;
		// 
		// expButton
		// 
		expButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		expButton.Location = new System.Drawing.Point(3, 266);
		expButton.Name = "expButton";
		expButton.Size = new System.Drawing.Size(314, 23);
		expButton.TabIndex = 3;
		expButton.Text = "EXPRESSIONS";
		expButton.UseVisualStyleBackColor = true;
		expButton.Click += ExpButton_Click;
		// 
		// plotButton
		// 
		plotButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		plotButton.Location = new System.Drawing.Point(3, 295);
		plotButton.Name = "plotButton";
		plotButton.Size = new System.Drawing.Size(314, 23);
		plotButton.TabIndex = 4;
		plotButton.Text = "PLOT";
		plotButton.UseVisualStyleBackColor = true;
		plotButton.Click += PlotButton_Click;
		// 
		// MenuControl
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		BackColor = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)64)), ((int)((byte)64)));
		Controls.Add(setButton);
		Controls.Add(codeButton);
		Controls.Add(expButton);
		Controls.Add(plotButton);
		Size = new System.Drawing.Size(320, 320);
		ResumeLayout(false);
	}

	#endregion
	private System.Windows.Forms.Button setButton;
	private System.Windows.Forms.Button codeButton;
	private System.Windows.Forms.Button expButton;
	private System.Windows.Forms.Button plotButton;
}