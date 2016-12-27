namespace WorkPackageApplication
{
    partial class ElementList
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
            this.dgvElements = new Bentley.EC.Controls.DataGridViewEx();
            this.Hilite = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvElements)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvElements
            // 
            this.dgvElements.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvElements.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Hilite});
            this.dgvElements.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvElements.ErrorMessage = "";
            this.dgvElements.IsError = false;
            this.dgvElements.Location = new System.Drawing.Point(0, 0);
            this.dgvElements.Name = "dgvElements";
            this.dgvElements.Size = new System.Drawing.Size(284, 262);
            this.dgvElements.TabIndex = 0;
            // 
            // Hilite
            // 
            this.Hilite.FalseValue = "false";
            this.Hilite.HeaderText = "Hilite";
            this.Hilite.Name = "Hilite";
            this.Hilite.TrueValue = "true";
            // 
            // ElementList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.dgvElements);
            this.Name = "ElementList";
            this.Text = "ElementList";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.closingElementList);
            ((System.ComponentModel.ISupportInitialize)(this.dgvElements)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Bentley.EC.Controls.DataGridViewEx dgvElements;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Hilite;

    }
}