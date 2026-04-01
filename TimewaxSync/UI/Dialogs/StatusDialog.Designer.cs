namespace TimewaxSync.UI
{
    partial class StatusDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        private void InitializeComponent()
        {
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ErrorLabel = new System.Windows.Forms.Label();
            this.ErrorListBox = new System.Windows.Forms.ListBox();
            this.CloseButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 276);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(366, 23);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 0;
            // 
            // StatusLabel
            // 
            this.StatusLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StatusLabel.Location = new System.Drawing.Point(12, 302);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(369, 13);
            this.StatusLabel.TabIndex = 1;
            this.StatusLabel.Text = "Crunching some coockies";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::TimewaxSync.Properties.Resources.waiting;
            this.pictureBox1.ImageLocation = "";
            this.pictureBox1.InitialImage = global::TimewaxSync.Properties.Resources.waiting;
            this.pictureBox1.Location = new System.Drawing.Point(15, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(366, 258);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // ErrorLabel
            // 
            this.ErrorLabel.AutoSize = true;
            this.ErrorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(40)))), ((int)(((byte)(54)))));
            this.ErrorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.ErrorLabel.Location = new System.Drawing.Point(12, 322);
            this.ErrorLabel.Name = "ErrorLabel";
            this.ErrorLabel.Size = new System.Drawing.Size(40, 13);
            this.ErrorLabel.TabIndex = 3;
            this.ErrorLabel.Text = "Errors:";
            this.ErrorLabel.Visible = false;
            // 
            // ErrorListBox
            // 
            this.ErrorListBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(40)))), ((int)(((byte)(54)))));
            this.ErrorListBox.FormattingEnabled = true;
            this.ErrorListBox.HorizontalScrollbar = true;
            this.ErrorListBox.Location = new System.Drawing.Point(12, 338);
            this.ErrorListBox.Name = "ErrorListBox";
            this.ErrorListBox.Size = new System.Drawing.Size(366, 95);
            this.ErrorListBox.TabIndex = 4;
            this.ErrorListBox.Visible = false;
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(303, 439);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 5;
            this.CloseButton.Text = "Sluiten";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Visible = false;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // StatusDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 324);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.ErrorListBox);
            this.Controls.Add(this.ErrorLabel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "StatusDialog";
            this.Text = "Status";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ProgressBar progressBar;
        public System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
        public System.Windows.Forms.Label ErrorLabel;
        public System.Windows.Forms.ListBox ErrorListBox;
        public System.Windows.Forms.Button CloseButton;
    }
}