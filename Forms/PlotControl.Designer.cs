using System.ComponentModel;
namespace Comparser.Forms;
partial class PlotControl {
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
		components = new Container();
		fps = new System.Windows.Forms.Timer(components);
		addButton = new Button();
		widthBox = new RichTextBox();
		animatedButton = new Button();
		splitContainer = new SplitContainer();
		modeLabel = new Label();
		iteLabel = new Label();
		itcLabel = new Label();
		iteBox = new RichTextBox();
		itcBox = new RichTextBox();
		itsBox = new RichTextBox();
		prevButton = new Button();
		itBox = new RichTextBox();
		nextButton = new Button();
		itlBox = new RichTextBox();
		itsLabel = new Label();
		modeSelect = new ComboBox();
		heightLabel = new Label();
		heightBox = new RichTextBox();
		widthLabel = new Label();
		plotBox = new PictureBox();
		ixsBox = new RichTextBox();
		ixcBox = new RichTextBox();
		ixeBox = new RichTextBox();
		ixeLabel = new Label();
		ixcLabel = new Label();
		ixsLabel = new Label();
		richTextBox1 = new RichTextBox();
		richTextBox2 = new RichTextBox();
		richTextBox3 = new RichTextBox();
		iyeLabel = new Label();
		iycLabel = new Label();
		iysLabel = new Label();
		oyeBox = new RichTextBox();
		oycBox = new RichTextBox();
		oysBox = new RichTextBox();
		oyeLabel = new Label();
		oycLabel = new Label();
		oysLabel = new Label();
		OutputsBox = new ComboBox();
		fyBox = new RichTextBox();
		fyLabel = new Label();
		codeBox = new RichTextBox();
		((ISupportInitialize)splitContainer).BeginInit();
		splitContainer.Panel1.SuspendLayout();
		splitContainer.Panel2.SuspendLayout();
		splitContainer.SuspendLayout();
		((ISupportInitialize)plotBox).BeginInit();
		SuspendLayout();
		// 
		// fps
		// 
		fps.Enabled = true;
		fps.Tick += Fps_Tick;
		// 
		// addButton
		// 
		addButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		addButton.Location = new Point(181, 564);
		addButton.Name = "addButton";
		addButton.Size = new Size(55, 27);
		addButton.TabIndex = 1;
		addButton.Text = "[add]";
		addButton.UseVisualStyleBackColor = true;
		// 
		// widthBox
		// 
		widthBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		widthBox.Location = new Point(181, 3);
		widthBox.MinimumSize = new Size(32, 27);
		widthBox.Name = "widthBox";
		widthBox.Size = new Size(55, 27);
		widthBox.TabIndex = 0;
		widthBox.Text = "";
		// 
		// animatedButton
		// 
		animatedButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		animatedButton.Location = new Point(181, 69);
		animatedButton.Name = "animatedButton";
		animatedButton.Size = new Size(55, 27);
		animatedButton.TabIndex = 2;
		animatedButton.Text = "[ani]";
		animatedButton.UseMnemonic = false;
		animatedButton.UseVisualStyleBackColor = true;
		// 
		// splitContainer
		// 
		splitContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		splitContainer.Location = new Point(3, 3);
		splitContainer.Name = "splitContainer";
		// 
		// splitContainer.Panel1
		// 
		splitContainer.Panel1.AutoScroll = true;
		splitContainer.Panel1.Controls.Add(codeBox);
		splitContainer.Panel1.Controls.Add(fyLabel);
		splitContainer.Panel1.Controls.Add(fyBox);
		splitContainer.Panel1.Controls.Add(OutputsBox);
		splitContainer.Panel1.Controls.Add(oyeLabel);
		splitContainer.Panel1.Controls.Add(oycLabel);
		splitContainer.Panel1.Controls.Add(oysLabel);
		splitContainer.Panel1.Controls.Add(oyeBox);
		splitContainer.Panel1.Controls.Add(oycBox);
		splitContainer.Panel1.Controls.Add(oysBox);
		splitContainer.Panel1.Controls.Add(iyeLabel);
		splitContainer.Panel1.Controls.Add(iycLabel);
		splitContainer.Panel1.Controls.Add(iysLabel);
		splitContainer.Panel1.Controls.Add(richTextBox1);
		splitContainer.Panel1.Controls.Add(richTextBox2);
		splitContainer.Panel1.Controls.Add(richTextBox3);
		splitContainer.Panel1.Controls.Add(ixeLabel);
		splitContainer.Panel1.Controls.Add(ixcLabel);
		splitContainer.Panel1.Controls.Add(ixsLabel);
		splitContainer.Panel1.Controls.Add(ixeBox);
		splitContainer.Panel1.Controls.Add(ixcBox);
		splitContainer.Panel1.Controls.Add(ixsBox);
		splitContainer.Panel1.Controls.Add(modeLabel);
		splitContainer.Panel1.Controls.Add(iteLabel);
		splitContainer.Panel1.Controls.Add(itcLabel);
		splitContainer.Panel1.Controls.Add(iteBox);
		splitContainer.Panel1.Controls.Add(itcBox);
		splitContainer.Panel1.Controls.Add(itsBox);
		splitContainer.Panel1.Controls.Add(prevButton);
		splitContainer.Panel1.Controls.Add(itBox);
		splitContainer.Panel1.Controls.Add(nextButton);
		splitContainer.Panel1.Controls.Add(itlBox);
		splitContainer.Panel1.Controls.Add(itsLabel);
		splitContainer.Panel1.Controls.Add(modeSelect);
		splitContainer.Panel1.Controls.Add(heightLabel);
		splitContainer.Panel1.Controls.Add(heightBox);
		splitContainer.Panel1.Controls.Add(widthLabel);
		splitContainer.Panel1.Controls.Add(animatedButton);
		splitContainer.Panel1.Controls.Add(widthBox);
		splitContainer.Panel1.Controls.Add(addButton);
		// 
		// splitContainer.Panel2
		// 
		splitContainer.Panel2.Controls.Add(plotBox);
		splitContainer.Size = new Size(314, 314);
		splitContainer.SplitterDistance = 256;
		splitContainer.SplitterWidth = 12;
		splitContainer.TabIndex = 3;
		// 
		// modeLabel
		// 
		modeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		modeLabel.AutoSize = true;
		modeLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		modeLabel.Location = new Point(11, 204);
		modeLabel.Name = "modeLabel";
		modeLabel.Size = new Size(90, 19);
		modeLabel.TabIndex = 17;
		modeLabel.Text = "PlotMode:";
		// 
		// iteLabel
		// 
		iteLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		iteLabel.AutoSize = true;
		iteLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		iteLabel.Location = new Point(9, 168);
		iteLabel.Name = "iteLabel";
		iteLabel.Size = new Size(81, 19);
		iteLabel.TabIndex = 16;
		iteLabel.Text = "TimeEnd:";
		// 
		// itcLabel
		// 
		itcLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		itcLabel.AutoSize = true;
		itcLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		itcLabel.Location = new Point(10, 135);
		itcLabel.Name = "itcLabel";
		itcLabel.Size = new Size(108, 19);
		itcLabel.TabIndex = 15;
		itcLabel.Text = "TimeCenter:";
		// 
		// iteBox
		// 
		iteBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		iteBox.Location = new Point(181, 168);
		iteBox.MinimumSize = new Size(32, 27);
		iteBox.Name = "iteBox";
		iteBox.Size = new Size(55, 27);
		iteBox.TabIndex = 14;
		iteBox.Text = "";
		// 
		// itcBox
		// 
		itcBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		itcBox.Location = new Point(181, 135);
		itcBox.MinimumSize = new Size(32, 27);
		itcBox.Name = "itcBox";
		itcBox.Size = new Size(55, 27);
		itcBox.TabIndex = 13;
		itcBox.Text = "";
		// 
		// itsBox
		// 
		itsBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		itsBox.Location = new Point(181, 102);
		itsBox.MinimumSize = new Size(32, 27);
		itsBox.Name = "itsBox";
		itsBox.Size = new Size(55, 27);
		itsBox.TabIndex = 12;
		itsBox.Text = "";
		// 
		// prevButton
		// 
		prevButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		prevButton.Location = new Point(65, 69);
		prevButton.Name = "prevButton";
		prevButton.Size = new Size(25, 27);
		prevButton.TabIndex = 11;
		prevButton.Text = "<";
		prevButton.UseMnemonic = false;
		prevButton.UseVisualStyleBackColor = true;
		// 
		// itBox
		// 
		itBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		itBox.Location = new Point(6, 69);
		itBox.MaximumSize = new Size(0, 27);
		itBox.MinimumSize = new Size(32, 27);
		itBox.Name = "itBox";
		itBox.Size = new Size(53, 27);
		itBox.TabIndex = 10;
		itBox.Text = "";
		// 
		// nextButton
		// 
		nextButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		nextButton.Location = new Point(149, 69);
		nextButton.Name = "nextButton";
		nextButton.Size = new Size(25, 27);
		nextButton.TabIndex = 9;
		nextButton.Text = ">";
		nextButton.UseMnemonic = false;
		nextButton.UseVisualStyleBackColor = true;
		// 
		// itlBox
		// 
		itlBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		itlBox.Location = new Point(94, 69);
		itlBox.MaximumSize = new Size(0, 27);
		itlBox.MinimumSize = new Size(32, 27);
		itlBox.Name = "itlBox";
		itlBox.Size = new Size(49, 27);
		itlBox.TabIndex = 8;
		itlBox.Text = "";
		// 
		// itsLabel
		// 
		itsLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		itsLabel.AutoSize = true;
		itsLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		itsLabel.Location = new Point(9, 102);
		itsLabel.Name = "itsLabel";
		itsLabel.Size = new Size(99, 19);
		itsLabel.TabIndex = 7;
		itsLabel.Text = "TimeStart:";
		// 
		// modeSelect
		// 
		modeSelect.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		modeSelect.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		modeSelect.FormattingEnabled = true;
		modeSelect.Items.AddRange(new object[] { "Outline X", "Fill X", "RGB XY" });
		modeSelect.Location = new Point(181, 201);
		modeSelect.Name = "modeSelect";
		modeSelect.Size = new Size(55, 27);
		modeSelect.TabIndex = 6;
		// 
		// heightLabel
		// 
		heightLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		heightLabel.AutoSize = true;
		heightLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		heightLabel.Location = new Point(7, 36);
		heightLabel.Name = "heightLabel";
		heightLabel.Size = new Size(126, 19);
		heightLabel.TabIndex = 5;
		heightLabel.Text = "ScreenHeight:";
		// 
		// heightBox
		// 
		heightBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		heightBox.Location = new Point(181, 36);
		heightBox.MinimumSize = new Size(32, 27);
		heightBox.Name = "heightBox";
		heightBox.Size = new Size(55, 27);
		heightBox.TabIndex = 4;
		heightBox.Text = "";
		// 
		// widthLabel
		// 
		widthLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		widthLabel.AutoSize = true;
		widthLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		widthLabel.Location = new Point(6, 3);
		widthLabel.Name = "widthLabel";
		widthLabel.Size = new Size(117, 19);
		widthLabel.TabIndex = 3;
		widthLabel.Text = "ScreenWidth:";
		// 
		// plotBox
		// 
		plotBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		plotBox.Location = new Point(3, 3);
		plotBox.Name = "plotBox";
		plotBox.Size = new Size(8, 308);
		plotBox.TabIndex = 1;
		plotBox.TabStop = false;
		// 
		// ixsBox
		// 
		ixsBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		ixsBox.Location = new Point(181, 234);
		ixsBox.MinimumSize = new Size(32, 27);
		ixsBox.Name = "ixsBox";
		ixsBox.Size = new Size(55, 27);
		ixsBox.TabIndex = 18;
		ixsBox.Text = "";
		// 
		// ixcBox
		// 
		ixcBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		ixcBox.Location = new Point(181, 267);
		ixcBox.MinimumSize = new Size(32, 27);
		ixcBox.Name = "ixcBox";
		ixcBox.Size = new Size(55, 27);
		ixcBox.TabIndex = 19;
		ixcBox.Text = "";
		// 
		// ixeBox
		// 
		ixeBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		ixeBox.Location = new Point(181, 300);
		ixeBox.MinimumSize = new Size(32, 27);
		ixeBox.Name = "ixeBox";
		ixeBox.Size = new Size(55, 27);
		ixeBox.TabIndex = 20;
		ixeBox.Text = "";
		// 
		// ixeLabel
		// 
		ixeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		ixeLabel.AutoSize = true;
		ixeLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		ixeLabel.Location = new Point(10, 300);
		ixeLabel.Name = "ixeLabel";
		ixeLabel.Size = new Size(63, 19);
		ixeLabel.TabIndex = 23;
		ixeLabel.Text = "X End:";
		// 
		// ixcLabel
		// 
		ixcLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		ixcLabel.AutoSize = true;
		ixcLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		ixcLabel.Location = new Point(11, 267);
		ixcLabel.Name = "ixcLabel";
		ixcLabel.Size = new Size(90, 19);
		ixcLabel.TabIndex = 22;
		ixcLabel.Text = "X Center:";
		// 
		// ixsLabel
		// 
		ixsLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		ixsLabel.AutoSize = true;
		ixsLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		ixsLabel.Location = new Point(10, 234);
		ixsLabel.Name = "ixsLabel";
		ixsLabel.Size = new Size(81, 19);
		ixsLabel.TabIndex = 21;
		ixsLabel.Text = "X Start:";
		// 
		// richTextBox1
		// 
		richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		richTextBox1.Location = new Point(181, 399);
		richTextBox1.MinimumSize = new Size(32, 27);
		richTextBox1.Name = "richTextBox1";
		richTextBox1.Size = new Size(55, 27);
		richTextBox1.TabIndex = 26;
		richTextBox1.Text = "";
		// 
		// richTextBox2
		// 
		richTextBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		richTextBox2.Location = new Point(181, 366);
		richTextBox2.MinimumSize = new Size(32, 27);
		richTextBox2.Name = "richTextBox2";
		richTextBox2.Size = new Size(55, 27);
		richTextBox2.TabIndex = 25;
		richTextBox2.Text = "";
		// 
		// richTextBox3
		// 
		richTextBox3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		richTextBox3.Location = new Point(181, 333);
		richTextBox3.MinimumSize = new Size(32, 27);
		richTextBox3.Name = "richTextBox3";
		richTextBox3.Size = new Size(55, 27);
		richTextBox3.TabIndex = 24;
		richTextBox3.Text = "";
		// 
		// iyeLabel
		// 
		iyeLabel.AutoSize = true;
		iyeLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		iyeLabel.Location = new Point(11, 399);
		iyeLabel.Name = "iyeLabel";
		iyeLabel.Size = new Size(63, 19);
		iyeLabel.TabIndex = 29;
		iyeLabel.Text = "Y End:";
		// 
		// iycLabel
		// 
		iycLabel.AutoSize = true;
		iycLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		iycLabel.Location = new Point(12, 366);
		iycLabel.Name = "iycLabel";
		iycLabel.Size = new Size(90, 19);
		iycLabel.TabIndex = 28;
		iycLabel.Text = "Y Center:";
		// 
		// iysLabel
		// 
		iysLabel.AutoSize = true;
		iysLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		iysLabel.Location = new Point(11, 333);
		iysLabel.Name = "iysLabel";
		iysLabel.Size = new Size(81, 19);
		iysLabel.TabIndex = 27;
		iysLabel.Text = "Y Start:";
		// 
		// oyeBox
		// 
		oyeBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		oyeBox.Location = new Point(181, 531);
		oyeBox.MinimumSize = new Size(32, 27);
		oyeBox.Name = "oyeBox";
		oyeBox.Size = new Size(55, 27);
		oyeBox.TabIndex = 32;
		oyeBox.Text = "";
		// 
		// oycBox
		// 
		oycBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		oycBox.Location = new Point(181, 498);
		oycBox.MinimumSize = new Size(32, 27);
		oycBox.Name = "oycBox";
		oycBox.Size = new Size(55, 27);
		oycBox.TabIndex = 31;
		oycBox.Text = "";
		// 
		// oysBox
		// 
		oysBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		oysBox.Location = new Point(181, 465);
		oysBox.MinimumSize = new Size(32, 27);
		oysBox.Name = "oysBox";
		oysBox.Size = new Size(55, 27);
		oysBox.TabIndex = 30;
		oysBox.Text = "";
		// 
		// oyeLabel
		// 
		oyeLabel.AutoSize = true;
		oyeLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		oyeLabel.Location = new Point(10, 531);
		oyeLabel.Name = "oyeLabel";
		oyeLabel.Size = new Size(63, 19);
		oyeLabel.TabIndex = 35;
		oyeLabel.Text = "V End:";
		// 
		// oycLabel
		// 
		oycLabel.AutoSize = true;
		oycLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		oycLabel.Location = new Point(11, 498);
		oycLabel.Name = "oycLabel";
		oycLabel.Size = new Size(90, 19);
		oycLabel.TabIndex = 34;
		oycLabel.Text = "V Center:";
		// 
		// oysLabel
		// 
		oysLabel.AutoSize = true;
		oysLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		oysLabel.Location = new Point(10, 465);
		oysLabel.Name = "oysLabel";
		oysLabel.Size = new Size(81, 19);
		oysLabel.TabIndex = 33;
		oysLabel.Text = "V Start:";
		// 
		// OutputsBox
		// 
		OutputsBox.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		OutputsBox.FormattingEnabled = true;
		OutputsBox.Items.AddRange(new object[] { "Outline X", "Fill X", "RGB XY" });
		OutputsBox.Location = new Point(3, 564);
		OutputsBox.Name = "OutputsBox";
		OutputsBox.Size = new Size(172, 27);
		OutputsBox.TabIndex = 36;
		// 
		// fyBox
		// 
		fyBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		fyBox.Location = new Point(181, 432);
		fyBox.MinimumSize = new Size(32, 27);
		fyBox.Name = "fyBox";
		fyBox.Size = new Size(55, 27);
		fyBox.TabIndex = 37;
		fyBox.Text = "";
		// 
		// fyLabel
		// 
		fyLabel.AutoSize = true;
		fyLabel.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
		fyLabel.Location = new Point(12, 432);
		fyLabel.Name = "fyLabel";
		fyLabel.Size = new Size(81, 19);
		fyLabel.TabIndex = 38;
		fyLabel.Text = "Y Fixed:";
		// 
		// codeBox
		// 
		codeBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		codeBox.Location = new Point(0, 597);
		codeBox.MinimumSize = new Size(32, 27);
		codeBox.Name = "codeBox";
		codeBox.Size = new Size(236, 27);
		codeBox.TabIndex = 39;
		codeBox.Text = "";
		// 
		// PlotControl
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		BackColor = Color.FromArgb(64, 64, 64);
		Controls.Add(splitContainer);
		Name = "PlotControl";
		splitContainer.Panel1.ResumeLayout(false);
		splitContainer.Panel1.PerformLayout();
		splitContainer.Panel2.ResumeLayout(false);
		((ISupportInitialize)splitContainer).EndInit();
		splitContainer.ResumeLayout(false);
		((ISupportInitialize)plotBox).EndInit();
		ResumeLayout(false);
	}
	private System.Windows.Forms.Button animatedButton;
	#endregion
	private System.Windows.Forms.Timer fps;
	private System.Windows.Forms.Button addButton;
	private System.Windows.Forms.RichTextBox widthBox;
	private System.Windows.Forms.SplitContainer splitContainer;
	private Label widthLabel;
	private PictureBox plotBox;
	private RichTextBox itlBox;
	private Label itsLabel;
	private ComboBox modeSelect;
	private Label heightLabel;
	private RichTextBox heightBox;
	private Button nextButton;
	private Button prevButton;
	private RichTextBox itBox;
	private RichTextBox iteBox;
	private RichTextBox itcBox;
	private RichTextBox itsBox;
	private Label modeLabel;
	private Label iteLabel;
	private Label itcLabel;
	private Button button2;
	private RichTextBox richTextBox2;
	private Label ixeLabel;
	private Label ixcLabel;
	private Label ixsLabel;
	private RichTextBox ixeBox;
	private RichTextBox ixcBox;
	private RichTextBox ixsBox;
	private Label oyeLabel;
	private Label oycLabel;
	private Label oysLabel;
	private RichTextBox oyeBox;
	private RichTextBox oycBox;
	private RichTextBox oysBox;
	private Label iyeLabel;
	private Label iycLabel;
	private Label iysLabel;
	private RichTextBox richTextBox1;
	private RichTextBox richTextBox3;
	private ComboBox OutputsBox;
	private Label fyLabel;
	private RichTextBox fyBox;
	private RichTextBox codeBox;
}