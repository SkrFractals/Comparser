using System.ComponentModel;
namespace Comparser;
partial class MenuForm {
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

			#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			components = new System.ComponentModel.Container();
			expBox = new Button();
			screen = new PictureBox();
			fps = new System.Windows.Forms.Timer(components);
			SuspendLayout();
			// 
			// expBox
			// 
			expBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			expBox.Location = new Point(3, 32);
			expBox.Name = "expBox";
			expBox.Size = new Size(428, 23);
			expBox.TabIndex = 0;
			expBox.Text = "ADD EXPRESSION";
			expBox.UseVisualStyleBackColor = true;
			// 
			// expBox
			// 
			screen.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			screen.Location = new Point(0, 0);
			screen.Name = "screen";
			screen.Size = new Size(1600, 1600);
			screen.TabIndex = 1;
			// 
			// fps
			// 
			fps.Enabled = true;
			fps.Interval = 50;
			// 
			// ComparserForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(64, 64, 64);
			ClientSize = new Size(1600, 1600);
			Controls.Add(expBox);
			Controls.Add(screen);
			Name = "MenuForm";
			Text = "Comparser - Complex Computer Parser";
			ResumeLayout(false);
		}

		#endregion
		private Button expBox;
		private PictureBox screen;
		private System.Windows.Forms.Timer fps;
}