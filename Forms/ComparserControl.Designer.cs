using System.ComponentModel;
namespace Comparser.Forms;
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
		logButton = new System.Windows.Forms.Button();
		logBox = new System.Windows.Forms.RichTextBox();
		codeBox = new System.Windows.Forms.RichTextBox();
		buildButton = new System.Windows.Forms.Button();
		splitContainer = new System.Windows.Forms.SplitContainer();
		((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
		splitContainer.Panel1.SuspendLayout();
		splitContainer.Panel2.SuspendLayout();
		splitContainer.SuspendLayout();
		SuspendLayout();
		// 
		// fps
		// 
		fps.Enabled = true;
		fps.Tick += Fps_Tick;
		// 
		// logButton
		// 
		logButton.Location = new System.Drawing.Point(3, 3);
		logButton.Name = "logButton";
		logButton.Size = new System.Drawing.Size(67, 32);
		logButton.TabIndex = 1;
		logButton.Text = "LOGS";
		logButton.UseVisualStyleBackColor = true;
		logButton.Click += OpenLog;
		// 
		// logBox
		// 
		logBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		logBox.AutoSize = true;
		logBox.DetectUrls = false;
		logBox.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)238));
		logBox.Location = new System.Drawing.Point(76, 3);
		logBox.Name = "logBox";
		logBox.ReadOnly = true;
		logBox.Size = new System.Drawing.Size(235, 69);
		logBox.TabIndex = 0;
		logBox.Text = "[logs]";
		// 
		// codeBox
		// 
		codeBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		codeBox.Location = new System.Drawing.Point(3, 3);
		codeBox.MinimumSize = new System.Drawing.Size(0, 32);
		codeBox.Name = "codeBox";
		codeBox.Size = new System.Drawing.Size(308, 205);
		codeBox.TabIndex = 0;
		codeBox.Text = "";
		// 
		// buildButton
		// 
		buildButton.Location = new System.Drawing.Point(3, 41);
		buildButton.Name = "buildButton";
		buildButton.Size = new System.Drawing.Size(67, 32);
		buildButton.TabIndex = 2;
		buildButton.Text = "OK";
		buildButton.UseMnemonic = false;
		buildButton.UseVisualStyleBackColor = true;
		buildButton.Click += CancelBuild;
		// 
		// splitContainer
		// 
		splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		splitContainer.Location = new System.Drawing.Point(3, 3);
		splitContainer.Name = "splitContainer";
		splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
		// 
		// splitContainer.Panel1
		// 
		splitContainer.Panel1.Controls.Add(buildButton);
		splitContainer.Panel1.Controls.Add(logBox);
		splitContainer.Panel1.Controls.Add(logButton);
		// 
		// splitContainer.Panel2
		// 
		splitContainer.Panel2.Controls.Add(codeBox);
		splitContainer.Size = new System.Drawing.Size(314, 314);
		splitContainer.SplitterDistance = 75;
		splitContainer.SplitterWidth = 12;
		splitContainer.TabIndex = 3;
		// 
		// ComparserControl
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		BackColor = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)64)), ((int)((byte)64)));
		Controls.Add(splitContainer);
		splitContainer.Panel1.ResumeLayout(false);
		splitContainer.Panel1.PerformLayout();
		splitContainer.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
		splitContainer.ResumeLayout(false);
		ResumeLayout(false);
	}
	private System.Windows.Forms.Button buildButton;
	#endregion
	private System.Windows.Forms.Timer fps;
	private System.Windows.Forms.RichTextBox logBox;
	private System.Windows.Forms.Button logButton;
	private System.Windows.Forms.RichTextBox codeBox;
	private System.Windows.Forms.SplitContainer splitContainer;
}