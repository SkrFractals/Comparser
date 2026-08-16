namespace Comparser
{
    partial class ComparserForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			expBox = new Button();
			innerPanel = new Panel();
			logPanel = new Panel();
			logLabel = new Label();
			codeBox = new TextBox();
			algebraBox = new ComboBox();
			decLabel = new Label();
			decimalBox = new TextBox();
			outerPanel = new Panel();
			innerPanel.SuspendLayout();
			logPanel.SuspendLayout();
			outerPanel.SuspendLayout();
			SuspendLayout();
			// 
			// expBox
			// 
			expBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			expBox.Location = new Point(3, 32);
			expBox.Name = "expBox";
			expBox.Size = new Size(428, 23);
			expBox.TabIndex = 2;
			expBox.Text = "ADD EXPRESSION";
			expBox.UseVisualStyleBackColor = true;
			expBox.Click += ExpAdd;
			// 
			// innerPanel
			// 
			innerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			innerPanel.BackColor = Color.FromArgb(64, 64, 64);
			innerPanel.Controls.Add(logPanel);
			innerPanel.Controls.Add(codeBox);
			innerPanel.Controls.Add(algebraBox);
			innerPanel.Controls.Add(decLabel);
			innerPanel.Controls.Add(decimalBox);
			innerPanel.Controls.Add(expBox);
			innerPanel.Location = new Point(3, 3);
			innerPanel.Name = "innerPanel";
			innerPanel.Size = new Size(434, 149);
			innerPanel.TabIndex = 2;
			// 
			// logPanel
			// 
			logPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
			logPanel.AutoScroll = true;
			logPanel.Controls.Add(logLabel);
			logPanel.Location = new Point(175, 61);
			logPanel.Name = "logPanel";
			logPanel.Size = new Size(256, 85);
			logPanel.TabIndex = 6;
			// 
			// logLabel
			// 
			logLabel.AutoSize = true;
			logLabel.ForeColor = Color.White;
			logLabel.Location = new Point(3, 3);
			logLabel.Name = "logLabel";
			logLabel.Size = new Size(37, 15);
			logLabel.TabIndex = 0;
			logLabel.Text = "[logs]";
			// 
			// codeBox
			// 
			codeBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			codeBox.Location = new Point(3, 61);
			codeBox.Multiline = true;
			codeBox.Name = "codeBox";
			codeBox.Size = new Size(166, 85);
			codeBox.TabIndex = 5;
			codeBox.TextChanged += CodeBox_TextChanged;
			// 
			// algebraBox
			// 
			algebraBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			algebraBox.FormattingEnabled = true;
			algebraBox.Items.AddRange(new object[] { "REAL", "COMPLEX", "QUATERNION" });
			algebraBox.Location = new Point(322, 3);
			algebraBox.Name = "algebraBox";
			algebraBox.Size = new Size(109, 23);
			algebraBox.TabIndex = 1;
			algebraBox.Text = "COMPLEX";
			algebraBox.SelectedIndexChanged += AlgebraBox_SelectedIndexChanged;
			// 
			// decLabel
			// 
			decLabel.AutoSize = true;
			decLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 238);
			decLabel.ForeColor = Color.White;
			decLabel.Location = new Point(3, 6);
			decLabel.Name = "decLabel";
			decLabel.Size = new Size(60, 15);
			decLabel.TabIndex = 3;
			decLabel.Text = "Decimals:";
			// 
			// decimalBox
			// 
			decimalBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			decimalBox.Location = new Point(77, 3);
			decimalBox.Name = "decimalBox";
			decimalBox.Size = new Size(239, 23);
			decimalBox.TabIndex = 0;
			decimalBox.Text = "3";
			decimalBox.TextChanged += DecimalBox_TextChanged;
			// 
			// outerPanel
			// 
			outerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			outerPanel.AutoScroll = true;
			outerPanel.BackColor = Color.White;
			outerPanel.Controls.Add(innerPanel);
			outerPanel.Location = new Point(12, 12);
			outerPanel.Name = "outerPanel";
			outerPanel.Size = new Size(440, 155);
			outerPanel.TabIndex = 3;
			// 
			// ComparserForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(64, 64, 64);
			ClientSize = new Size(464, 179);
			Controls.Add(outerPanel);
			Name = "ComparserForm";
			Text = "Comparser - Complex Computer Parser";
			innerPanel.ResumeLayout(false);
			innerPanel.PerformLayout();
			logPanel.ResumeLayout(false);
			logPanel.PerformLayout();
			outerPanel.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion
		private Button expBox;
		private Panel innerPanel;
		private Panel outerPanel;
		private TextBox decimalBox;
		private Label decLabel;
		private ComboBox algebraBox;
		private Panel logPanel;
		private TextBox codeBox;
		private Label logLabel;
	}
}
