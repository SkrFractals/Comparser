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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			expBox = new System.Windows.Forms.Button();
			innerPanel = new System.Windows.Forms.Panel();
			logPanel = new System.Windows.Forms.Panel();
			logBox = new System.Windows.Forms.RichTextBox();
			codeBox = new System.Windows.Forms.TextBox();
			algebraBox = new System.Windows.Forms.ComboBox();
			decLabel = new System.Windows.Forms.Label();
			decimalBox = new System.Windows.Forms.TextBox();
			outerPanel = new System.Windows.Forms.Panel();
			innerPanel.SuspendLayout();
			logPanel.SuspendLayout();
			outerPanel.SuspendLayout();
			SuspendLayout();
			// 
			// expBox
			// 
			expBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			expBox.Location = new System.Drawing.Point(3, 32);
			expBox.Name = "expBox";
			expBox.Size = new System.Drawing.Size(428, 23);
			expBox.TabIndex = 2;
			expBox.Text = "ADD EXPRESSION";
			expBox.UseVisualStyleBackColor = true;
			expBox.Click += ExpAdd;
			// 
			// innerPanel
			// 
			innerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			innerPanel.BackColor = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)64)), ((int)((byte)64)));
			innerPanel.Controls.Add(logPanel);
			innerPanel.Controls.Add(codeBox);
			innerPanel.Controls.Add(algebraBox);
			innerPanel.Controls.Add(decLabel);
			innerPanel.Controls.Add(decimalBox);
			innerPanel.Controls.Add(expBox);
			innerPanel.Location = new System.Drawing.Point(3, 3);
			innerPanel.Name = "innerPanel";
			innerPanel.Size = new System.Drawing.Size(434, 244);
			innerPanel.TabIndex = 2;
			// 
			// logPanel
			// 
			logPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			logPanel.AutoScroll = true;
			logPanel.Controls.Add(logBox);
			logPanel.Location = new System.Drawing.Point(3, 61);
			logPanel.Name = "logPanel";
			logPanel.Size = new System.Drawing.Size(428, 52);
			logPanel.TabIndex = 6;
			// 
			// logBox
			// 
			logBox.AutoSize = true;
			logBox.ForeColor = System.Drawing.Color.White;
			logBox.Location = new System.Drawing.Point(3, 3);
			logBox.Name = "logBox";
			logBox.ReadOnly = true;
			logBox.Size = new System.Drawing.Size(422, 46);
			logBox.TabIndex = 0;
			logBox.Text = "[logs]";
			// 
			// codeBox
			// 
			codeBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			codeBox.Location = new System.Drawing.Point(3, 119);
			codeBox.MinimumSize = new System.Drawing.Size(0, 32);
			codeBox.Multiline = true;
			codeBox.Name = "codeBox";
			codeBox.Size = new System.Drawing.Size(428, 122);
			codeBox.TabIndex = 5;
			codeBox.TextChanged += CodeBox_TextChanged;
			// 
			// algebraBox
			// 
			algebraBox.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
			algebraBox.FormattingEnabled = true;
			algebraBox.Items.AddRange(new object[] { "REAL", "COMPLEX", "QUATERNION" });
			algebraBox.Location = new System.Drawing.Point(322, 3);
			algebraBox.Name = "algebraBox";
			algebraBox.Size = new System.Drawing.Size(109, 23);
			algebraBox.TabIndex = 1;
			algebraBox.Text = "COMPLEX";
			algebraBox.SelectedIndexChanged += AlgebraBox_SelectedIndexChanged;
			// 
			// decLabel
			// 
			decLabel.AutoSize = true;
			decLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)238));
			decLabel.ForeColor = System.Drawing.Color.White;
			decLabel.Location = new System.Drawing.Point(3, 6);
			decLabel.Name = "decLabel";
			decLabel.Size = new System.Drawing.Size(60, 15);
			decLabel.TabIndex = 3;
			decLabel.Text = "Decimals:";
			// 
			// decimalBox
			// 
			decimalBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			decimalBox.Location = new System.Drawing.Point(77, 3);
			decimalBox.Name = "decimalBox";
			decimalBox.Size = new System.Drawing.Size(239, 23);
			decimalBox.TabIndex = 0;
			decimalBox.Text = "3";
			decimalBox.TextChanged += DecimalBox_TextChanged;
			// 
			// outerPanel
			// 
			outerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
			outerPanel.AutoScroll = true;
			outerPanel.BackColor = System.Drawing.Color.White;
			outerPanel.Controls.Add(innerPanel);
			outerPanel.Location = new System.Drawing.Point(12, 12);
			outerPanel.Name = "outerPanel";
			outerPanel.Size = new System.Drawing.Size(440, 250);
			outerPanel.TabIndex = 3;
			// 
			// ComparserForm
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.Color.FromArgb(((int)((byte)64)), ((int)((byte)64)), ((int)((byte)64)));
			ClientSize = new System.Drawing.Size(464, 274);
			Controls.Add(outerPanel);
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
		private System.Windows.Forms.Panel innerPanel;
		private System.Windows.Forms.Panel outerPanel;
		private TextBox decimalBox;
		private Label decLabel;
		private ComboBox algebraBox;
		private System.Windows.Forms.Panel logPanel;
		private System.Windows.Forms.TextBox codeBox;
		private System.Windows.Forms.RichTextBox logBox;
	}
}
