namespace WorkPackageApplication
{
    partial class ECApiExampleAddSchemaToElm
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
            WorkPackageAddin.SetForm(null);
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbSchemasCB = new System.Windows.Forms.CheckedListBox();
            this.cmdBtnAddToElm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbSchemasCB
            // 
            this.lbSchemasCB.FormattingEnabled = true;
            this.lbSchemasCB.Location = new System.Drawing.Point(46, 47);
            this.lbSchemasCB.Name = "lbSchemasCB";
            this.lbSchemasCB.Size = new System.Drawing.Size(200, 89);
            this.lbSchemasCB.TabIndex = 0;
            // 
            // cmdBtnAddToElm
            // 
            this.cmdBtnAddToElm.Location = new System.Drawing.Point(69, 177);
            this.cmdBtnAddToElm.Name = "cmdBtnAddToElm";
            this.cmdBtnAddToElm.Size = new System.Drawing.Size(153, 23);
            this.cmdBtnAddToElm.TabIndex = 1;
            this.cmdBtnAddToElm.Text = "AddToElementCmd";
            this.cmdBtnAddToElm.UseVisualStyleBackColor = true;
            this.cmdBtnAddToElm.Click += new System.EventHandler(this.cmdBtnAddToElm_Click);
            // 
            // ECApiExampleAddSchemaToElm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 255);
            this.Controls.Add(this.cmdBtnAddToElm);
            this.Controls.Add(this.lbSchemasCB);
            this.Name = "ECApiExampleAddSchemaToElm";
            this.Text = "ECApiExampleAddSchemaToElm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox lbSchemasCB;
        private System.Windows.Forms.Button cmdBtnAddToElm;
    }
}