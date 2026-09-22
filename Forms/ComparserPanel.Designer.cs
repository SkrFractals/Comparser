using System.ComponentModel;
namespace Comparser.Forms;
partial class ComparserPanel {
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
		logButton = new Button();
		logBox = new RichTextBox();
		codeBox = new RichTextBox();
		buildButton = new Button();
		splitContainer = new SplitContainer();
		saveCode = new SaveFileDialog();
		openCode = new OpenFileDialog();
		((ISupportInitialize)splitContainer).BeginInit();
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
		logButton.Location = new Point(3, 3);
		logButton.Name = "logButton";
		logButton.Size = new Size(67, 32);
		logButton.TabIndex = 1;
		logButton.Text = "LOGS";
		logButton.UseVisualStyleBackColor = true;
		logButton.Click += OpenLog;
		// 
		// logBox
		// 
		logBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		logBox.DetectUrls = false;
		logBox.Font = new Font("Consolas", 12F, FontStyle.Regular, GraphicsUnit.Point, 238);
		logBox.Location = new Point(76, 3);
		logBox.Name = "logBox";
		logBox.ReadOnly = true;
		logBox.Size = new Size(241, 68);
		logBox.TabIndex = 0;
		logBox.Text = "[logs]";
		// 
		// codeBox
		// 
		codeBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		codeBox.Location = new Point(0, 0);
		codeBox.MinimumSize = new Size(0, 32);
		codeBox.Name = "codeBox";
		codeBox.Size = new Size(320, 226);
		codeBox.TabIndex = 0;
		codeBox.Text = "";
		// 
		// buildButton
		// 
		buildButton.Location = new Point(3, 41);
		buildButton.Name = "buildButton";
		buildButton.Size = new Size(67, 32);
		buildButton.TabIndex = 2;
		buildButton.Text = "OK";
		buildButton.UseMnemonic = false;
		buildButton.UseVisualStyleBackColor = true;
		buildButton.Click += CancelBuild;
		// 
		// splitContainer
		// 
		splitContainer.Dock = DockStyle.Fill;
		splitContainer.Location = new Point(0, 0);
		splitContainer.Name = "splitContainer";
		splitContainer.Orientation = Orientation.Horizontal;
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
		splitContainer.Size = new Size(320, 320);
		splitContainer.SplitterDistance = 74;
		splitContainer.SplitterWidth = 12;
		splitContainer.TabIndex = 3;
		// 
		// saveCode
		// 
		saveCode.FileName = "saveCode";
		saveCode.Filter = "COMPARSER programs (*.txt)|*.txt";
		saveCode.FileOk += saveCode_FileOk;
		// 
		// openCode
		// 
		openCode.FileName = "openCode";
		openCode.Filter = "COMPARSER programs (*.txt)|*.txt";
		openCode.FileOk += openCode_FileOk;
		// 
		// ComparserPanel
		// 
		AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(64, 64, 64);
		Controls.Add(splitContainer);
		Name = "ComparserPanel";
		Size = new Size(320, 320);
		splitContainer.Panel1.ResumeLayout(false);
		splitContainer.Panel2.ResumeLayout(false);
		((ISupportInitialize)splitContainer).EndInit();
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
	private SaveFileDialog saveCode;
	private OpenFileDialog openCode;
}