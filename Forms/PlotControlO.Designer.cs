using System.ComponentModel;
namespace Comparser.Forms;
partial class PlotControlO {
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
		plotBox = new PictureBox();
		fps = new System.Windows.Forms.Timer(components);
		outputsLabel = new Label();
		outputSelect = new ComboBox();
		modeLabel =  new Label();
		modeSelect = new();
		addButton = new();
		fyLabel = new();
		fyInputBox = new();
		fyDivLabel = new();
		widthLabel = new();
		widthBox = new();
		heightLabel = new();
		heightBox = new();
		ixsInputLabel = new();
		ixsInputBox = new();
		ixcInputLabel = new();
		ixcInputBox = new();
		ixeInputLabel = new();
		ixeInputBox = new();
		iysInputLabel = new();
		iysInputBox = new();
		iycInputLabel = new();
		iycInputBox = new();
		iyeInputLabel = new();
		iyeInputBox = new();
		oysInputLabel = new();
		oysInputBox = new();
		oycInputLabel = new();
		oycInputBox = new();
		oyeInputLabel = new();
		oyeInputBox = new();
		itsInputLabel = new();
		itsInputBox = new();
		itcInputLabel = new();
		itcInputBox = new();
		iteInputLabel = new();
		iteInputBox = new();
		itlInputLabel = new();
		itlInputBox = new();
		itfInputLabel = new();
		prevButton = new();
		nextButton = new();
		animatedButton = new();
		itfInputBox = new();
		outputLabel = new();
		outputBox = new();
		rgbLabel = new();
		rgbBox = new();
		splitContainer1 = new SplitContainer();
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
		// fps
		// 
		fps.Enabled = true;
		fps.Tick += Fps_Tick;
		// 
		// outputSelect
		// 
		outputSelect.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
		outputSelect.FormattingEnabled = true;
		outputSelect.Items.AddRange(new object[] { });
		outputSelect.Location = new System.Drawing.Point(163, 4);
		outputSelect.Name = "outputSelect";
		outputSelect.Size = new System.Drawing.Size(118, 23);
		outputSelect.TabIndex = 1;
		outputSelect.Text = "Select Output";
		// 
		// modeSelect
		// 
		modeSelect.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
		modeSelect.FormattingEnabled = true;
		modeSelect.Items.AddRange(new object[] { "X Outline", "X Fill", "XY" });
		modeSelect.Location = new System.Drawing.Point(163, 4);
		modeSelect.Name = "modeSelect";
		modeSelect.Size = new System.Drawing.Size(118, 23);
		modeSelect.TabIndex = 1;
		modeSelect.Text = "Select Mode";
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
		// splitContainer1
		// 
		splitContainer1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		splitContainer1.Orientation = Orientation.Horizontal;
		splitContainer1.Location = new Point(12, 366);
		splitContainer1.Name = "splitContainer1";
		// 
		// splitContainer1.Panel1
		// 
		splitContainer1.Panel1MinSize = 50;
		splitContainer1.Panel1.Controls.Add(iysInputBox);
		splitContainer1.Panel1.Controls.Add(oycInputBox);
		// 
		// splitContainer1.Panel2
		// 
		splitContainer1.Panel2MinSize = 50;
		splitContainer1.Panel1.Controls.Add(iyeInputBox);
		splitContainer1.Panel1.Controls.Add(oyeInputBox);
		splitContainer1.Size = new Size(381, 30);
		splitContainer1.SplitterDistance = 190;
		splitContainer1.TabIndex = 6;
		// 
		// fyInputBox
		// 
		fyInputBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		fyInputBox.Location = new System.Drawing.Point(3, 3);
		fyInputBox.MinimumSize = new System.Drawing.Size(0, 32);
		fyInputBox.Name = "fyInputBox";
		fyInputBox.Size = new System.Drawing.Size(308, 205);
		fyInputBox.TabIndex = 0;
		fyInputBox.Text = "";
		// 
		// ixsInputBox
		// 
		ixsInputBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		ixsInputBox.Location = new System.Drawing.Point(3, 3);
		ixsInputBox.MinimumSize = new System.Drawing.Size(0, 32);
		ixsInputBox.Name = "ixsInputBox";
		ixsInputBox.Size = new System.Drawing.Size(308, 205);
		ixsInputBox.TabIndex = 0;
		ixsInputBox.Text = "";
		// 
		// ixcInputBox
		// 
		ixcInputBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		ixcInputBox.Location = new System.Drawing.Point(3, 3);
		ixcInputBox.MinimumSize = new System.Drawing.Size(0, 32);
		ixcInputBox.Name = "ixcInputBox";
		ixcInputBox.Size = new System.Drawing.Size(308, 205);
		ixcInputBox.TabIndex = 0;
		ixcInputBox.Text = "";
		// 
		// ixeInputBox
		// 
		ixeInputBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		ixeInputBox.Location = new System.Drawing.Point(3, 3);
		ixeInputBox.MinimumSize = new System.Drawing.Size(0, 32);
		ixeInputBox.Name = "ixeInputBox";
		ixeInputBox.Size = new System.Drawing.Size(308, 205);
		ixeInputBox.TabIndex = 0;
		ixeInputBox.Text = "";
		// 
		// iysInputBox
		// 
		iysInputBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		iysInputBox.Location = new System.Drawing.Point(3, 3);
		iysInputBox.MinimumSize = new System.Drawing.Size(0, 32);
		iysInputBox.Name = "iysInputBox";
		iysInputBox.Size = new System.Drawing.Size(308, 205);
		iysInputBox.TabIndex = 0;
		iysInputBox.Text = "";
		// 
		// iycInputBox
		// 
		iycInputBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		iycInputBox.Location = new System.Drawing.Point(3, 3);
		iycInputBox.MinimumSize = new System.Drawing.Size(0, 32);
		iycInputBox.Name = "iycInputBox";
		iycInputBox.Size = new System.Drawing.Size(308, 205);
		iycInputBox.TabIndex = 0;
		iycInputBox.Text = "";
		// 
		// iyeInputBox
		// 
		iyeInputBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		iyeInputBox.Location = new System.Drawing.Point(3, 3);
		iyeInputBox.MinimumSize = new System.Drawing.Size(0, 32);
		iyeInputBox.Name = "iyeInputBox";
		iyeInputBox.Size = new System.Drawing.Size(308, 205);
		iyeInputBox.TabIndex = 0;
		iyeInputBox.Text = "";
		// 
		// oysInputBox
		// 
		oysInputBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		oysInputBox.Location = new System.Drawing.Point(3, 3);
		oysInputBox.MinimumSize = new System.Drawing.Size(0, 32);
		oysInputBox.Name = "oysInputBox";
		oysInputBox.Size = new System.Drawing.Size(308, 205);
		oysInputBox.TabIndex = 0;
		oysInputBox.Text = "";
		// 
		// oycInputBox
		// 
		oycInputBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		oycInputBox.Location = new System.Drawing.Point(3, 3);
		oycInputBox.MinimumSize = new System.Drawing.Size(0, 32);
		oycInputBox.Name = "oycInputBox";
		oycInputBox.Size = new System.Drawing.Size(308, 205);
		oycInputBox.TabIndex = 0;
		oycInputBox.Text = "";
		// 
		// oyeInputBox
		// 
		oyeInputBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		oyeInputBox.Location = new System.Drawing.Point(3, 3);
		oyeInputBox.MinimumSize = new System.Drawing.Size(0, 32);
		oyeInputBox.Name = "oyeInputBox";
		oyeInputBox.Size = new System.Drawing.Size(308, 205);
		oyeInputBox.TabIndex = 0;
		oyeInputBox.Text = "";
		// 
		// outputBox
		// 
		outputBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		outputBox.Location = new System.Drawing.Point(3, 3);
		outputBox.MinimumSize = new System.Drawing.Size(0, 32);
		outputBox.Name = "outputBox";
		outputBox.Size = new System.Drawing.Size(308, 205);
		outputBox.TabIndex = 0;
		outputBox.Text = "";
		// 
		// rgbBox
		// 
		rgbBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		rgbBox.Location = new System.Drawing.Point(3, 3);
		rgbBox.MinimumSize = new System.Drawing.Size(0, 32);
		rgbBox.Name = "rgbBox";
		rgbBox.Size = new System.Drawing.Size(308, 205);
		rgbBox.TabIndex = 0;
		rgbBox.Text = "";
		// 
		// addButton
		// 
		addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
		addButton.Location = new System.Drawing.Point(3, 3);
		addButton.Name = "addButton";
		addButton.Size = new System.Drawing.Size(314, 32);
		addButton.TabIndex = 3;
		addButton.Text = "ADD OUTPUT";
		addButton.UseVisualStyleBackColor = true;
		addButton.UseMnemonic = false;
		// 
		// PlotControl
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		BackColor = Color.FromArgb(64, 64, 64);
		ClientSize = new Size(405, 408);
		Controls.Add(splitContainer2);
		Controls.Add(splitContainer1);
		Controls.Add(widthBox);
		Controls.Add(heightBox);
		Controls.Add(plotBox);
		Controls.Add(outputsLabel);
		Controls.Add(outputSelect);
		Controls.Add(modeLabel);
		Controls.Add(modeSelect);
		Controls.Add(addButton);
		Controls.Add(fyLabel);
		Controls.Add(fyDivLabel);
		Controls.Add(ixsInputLabel);
		Controls.Add(ixsInputBox);
		Controls.Add(ixcInputLabel);
		Controls.Add(ixcInputBox);
		Controls.Add(ixeInputLabel);
		Controls.Add(ixeInputBox);
		Controls.Add(iysInputLabel);
		Controls.Add(iysInputBox);
		Controls.Add(iycInputLabel);
		Controls.Add(iycInputBox);
		Controls.Add(iyeInputLabel);
		Controls.Add(iyeInputBox);
		Controls.Add(oysInputLabel);
		Controls.Add(oysInputBox);
		Controls.Add(oycInputLabel);
		Controls.Add(oycInputBox);
		Controls.Add(oyeInputLabel);
		Controls.Add(oyeInputBox);
		Controls.Add(itsInputLabel);
		Controls.Add(itsInputLabel);
		Controls.Add(itsInputBox);
		Controls.Add(itcInputLabel);
		Controls.Add(itcInputBox);
		Controls.Add(iteInputLabel);
		Controls.Add(iteInputBox);
		Controls.Add(itlInputLabel);
		Controls.Add(itlInputBox);
		Controls.Add(itfInputLabel);
		Controls.Add(prevButton);
		Controls.Add(nextButton);
		Controls.Add(animatedButton);
		Controls.Add(itfInputBox);
		Controls.Add(outputLabel);
		Controls.Add(outputBox);
		Controls.Add(rgbLabel);
		Controls.Add(rgbBox);
		Text = "Comparser - Plotter";
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
	private System.Windows.Forms.Timer fps;
	private PictureBox plotBox;
	private Label widthLabel;
	private RichTextBox widthBox;
	private Label heightLabel;
	private RichTextBox heightBox;
	private Label outputsLabel;
	private ComboBox outputSelect;
	private Label modeLabel;
	private ComboBox modeSelect;
	private Button addButton;
	private Label fyLabel;
	private RichTextBox fyInputBox;
	private Label fyDivLabel;
	private Label ixsInputLabel;
	private RichTextBox ixsInputBox;
	private Label ixcInputLabel;
	private RichTextBox ixcInputBox;
	private Label ixeInputLabel;
	private RichTextBox ixeInputBox;
	private Label iysInputLabel;
	private RichTextBox iysInputBox;
	private Label iycInputLabel;
	private RichTextBox iycInputBox;
	private Label iyeInputLabel;
	private RichTextBox iyeInputBox;
	private Label oysInputLabel;
	private RichTextBox oysInputBox;
	private Label oycInputLabel;
	private RichTextBox oycInputBox;
	private Label oyeInputLabel;
	private RichTextBox oyeInputBox;
	private Label itsInputLabel;
	private RichTextBox itsInputBox;
	private Label itcInputLabel;
	private RichTextBox itcInputBox;
	private Label iteInputLabel;
	private RichTextBox iteInputBox;
	private Label itlInputLabel;
	private RichTextBox itlInputBox;
	private Label itfInputLabel;
	private Button prevButton;
	private Button nextButton;
	private Button animatedButton;
	private RichTextBox itfInputBox;
	private Label outputLabel;
	private RichTextBox outputBox;
	private Label rgbLabel;
	private RichTextBox rgbBox;
	private SplitContainer splitContainer1;
	private SplitContainer splitContainer2;
}