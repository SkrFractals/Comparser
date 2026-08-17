namespace Expressions;

partial class PlotForm {
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing) {
		if (disposing && (components != null)) {
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent() {
		expBox = new TextBox();
		plotBox = new PictureBox();
		blBox = new TextBox();
		brBox = new TextBox();
		splitContainer1 = new SplitContainer();
		trBox = new TextBox();
		tlBox = new TextBox();
		splitContainer2 = new SplitContainer();
		((System.ComponentModel.ISupportInitialize)plotBox).BeginInit();
		((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
		splitContainer1.Panel1.SuspendLayout();
		splitContainer1.Panel2.SuspendLayout();
		splitContainer1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
		splitContainer2.Panel1.SuspendLayout();
		splitContainer2.Panel2.SuspendLayout();
		splitContainer2.SuspendLayout();
		SuspendLayout();
		// 
		// expBox
		// 
		expBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		expBox.Location = new Point(12, 12);
		expBox.Name = "expBox";
		expBox.Size = new Size(381, 23);
		expBox.TabIndex = 0;
		expBox.TextChanged += expBox_TextChanged;
		// 
		// plotBox
		// 
		plotBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		plotBox.Location = new Point(12, 76);
		plotBox.Name = "plotBox";
		plotBox.Size = new Size(381, 284);
		plotBox.TabIndex = 3;
		plotBox.TabStop = false;
		// 
		// blBox
		// 
		blBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		blBox.Location = new Point(3, 4);
		blBox.Name = "blBox";
		blBox.Size = new Size(184, 23);
		blBox.TabIndex = 4;
		blBox.TextChanged += blBox_TextChanged;
		// 
		// brBox
		// 
		brBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		brBox.Location = new Point(3, 4);
		brBox.Name = "brBox";
		brBox.Size = new Size(181, 23);
		brBox.TabIndex = 5;
		brBox.TextChanged += brBox_TextChanged;
		// 
		// splitContainer1
		// 
		splitContainer1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		splitContainer1.Location = new Point(12, 366);
		splitContainer1.Name = "splitContainer1";
		// 
		// splitContainer1.Panel1
		// 
		splitContainer1.Panel1.Controls.Add(blBox);
		splitContainer1.Panel1MinSize = 50;
		// 
		// splitContainer1.Panel2
		// 
		splitContainer1.Panel2.Controls.Add(brBox);
		splitContainer1.Panel2MinSize = 50;
		splitContainer1.Size = new Size(381, 30);
		splitContainer1.SplitterDistance = 190;
		splitContainer1.TabIndex = 6;
		// 
		// trBox
		// 
		trBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		trBox.Location = new Point(3, 3);
		trBox.Name = "trBox";
		trBox.Size = new Size(181, 23);
		trBox.TabIndex = 2;
		trBox.TextChanged += trBox_TextChanged;
		// 
		// tlBox
		// 
		tlBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		tlBox.Location = new Point(3, 3);
		tlBox.Name = "tlBox";
		tlBox.Size = new Size(184, 23);
		tlBox.TabIndex = 1;
		tlBox.TextChanged += tlBox_TextChanged;
		// 
		// splitContainer2
		// 
		splitContainer2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		splitContainer2.Location = new Point(12, 41);
		splitContainer2.Name = "splitContainer2";
		// 
		// splitContainer2.Panel1
		// 
		splitContainer2.Panel1.Controls.Add(tlBox);
		splitContainer2.Panel1MinSize = 50;
		// 
		// splitContainer2.Panel2
		// 
		splitContainer2.Panel2.Controls.Add(trBox);
		splitContainer2.Panel2MinSize = 50;
		splitContainer2.Size = new Size(381, 29);
		splitContainer2.SplitterDistance = 190;
		splitContainer2.TabIndex = 7;
		// 
		// Plot
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		BackColor = Color.FromArgb(64, 64, 64);
		ClientSize = new Size(405, 408);
		Controls.Add(splitContainer2);
		Controls.Add(splitContainer1);
		Controls.Add(plotBox);
		Controls.Add(expBox);
		Name = "Plot";
		Text = "Comparser - Plot";
		((System.ComponentModel.ISupportInitialize)plotBox).EndInit();
		splitContainer1.Panel1.ResumeLayout(false);
		splitContainer1.Panel1.PerformLayout();
		splitContainer1.Panel2.ResumeLayout(false);
		splitContainer1.Panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
		splitContainer1.ResumeLayout(false);
		splitContainer2.Panel1.ResumeLayout(false);
		splitContainer2.Panel1.PerformLayout();
		splitContainer2.Panel2.ResumeLayout(false);
		splitContainer2.Panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
		splitContainer2.ResumeLayout(false);
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private TextBox expBox;
	private PictureBox plotBox;
	private TextBox textBox4;
	private TextBox blBox;
	private TextBox brBox;
	private SplitContainer splitContainer1;
	private TextBox trBox;
	private TextBox tlBox;
	private SplitContainer splitContainer2;
}