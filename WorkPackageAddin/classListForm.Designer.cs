namespace WorkPackageApplication
{
    partial class classListForm
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
            this.lbClassListCB = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // lbClassListCB
            // 
            this.lbClassListCB.FormattingEnabled = true;
            this.lbClassListCB.Location = new System.Drawing.Point(27, 41);
            this.lbClassListCB.Name = "lbClassListCB";
            this.lbClassListCB.Size = new System.Drawing.Size(226, 174);
            this.lbClassListCB.TabIndex = 0;
            // 
            // classListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 255);
            this.Controls.Add(this.lbClassListCB);
            this.Name = "classListForm";
            this.Text = "classListForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox lbClassListCB;
    }
}