namespace Expressions
{
    partial class MainForm
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
			funcBox = new Button();
			expBox = new Button();
			innerPanel = new Panel();
			decLabel = new Label();
			decimalBox = new TextBox();
			outerPanel = new Panel();
			innerPanel.SuspendLayout();
			outerPanel.SuspendLayout();
			SuspendLayout();
			// 
			// funcBox
			// 
			funcBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			funcBox.Location = new Point(3, 32);
			funcBox.Name = "funcBox";
			funcBox.Size = new Size(315, 23);
			funcBox.TabIndex = 1;
			funcBox.Text = "ADD FUNCTION";
			funcBox.UseVisualStyleBackColor = true;
			funcBox.Click += FuncAdd;
			// 
			// expBox
			// 
			expBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			expBox.Location = new Point(3, 61);
			expBox.Name = "expBox";
			expBox.Size = new Size(315, 23);
			expBox.TabIndex = 2;
			expBox.Text = "ADD EXPRESSION";
			expBox.UseVisualStyleBackColor = true;
			expBox.Click += ExpAdd;
			// 
			// innerPanel
			// 
			innerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			innerPanel.BackColor = Color.FromArgb(64, 64, 64);
			innerPanel.Controls.Add(decLabel);
			innerPanel.Controls.Add(decimalBox);
			innerPanel.Controls.Add(funcBox);
			innerPanel.Controls.Add(expBox);
			innerPanel.Location = new Point(3, 3);
			innerPanel.Name = "innerPanel";
			innerPanel.Size = new Size(321, 128);
			innerPanel.TabIndex = 2;
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
			decimalBox.Size = new Size(241, 23);
			decimalBox.TabIndex = 0;
			decimalBox.Text = "3";
			decimalBox.TextChanged += DecimalBox_TextChanged;
			// 
			// outerPanel
			// 
			outerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			outerPanel.AutoScroll = true;
			outerPanel.BackColor = Color.Black;
			outerPanel.Controls.Add(innerPanel);
			outerPanel.Location = new Point(12, 12);
			outerPanel.Name = "outerPanel";
			outerPanel.Size = new Size(327, 134);
			outerPanel.TabIndex = 3;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(224, 224, 224);
			ClientSize = new Size(351, 158);
			Controls.Add(outerPanel);
			Name = "MainForm";
			Text = "Comparser - Complex Computer Parser";
			innerPanel.ResumeLayout(false);
			innerPanel.PerformLayout();
			outerPanel.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private Button funcBox;
		private Button expBox;
		private Panel innerPanel;
		private Panel outerPanel;
		private TextBox decimalBox;
		private Label decLabel;
	}
}
