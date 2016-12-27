namespace WorkPackageApplication
{
    partial class ItemInfoForm
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
            this.dgInfoViewer = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgInfoViewer)).BeginInit();
            this.SuspendLayout();
            // 
            // dgInfoViewer
            // 
            this.dgInfoViewer.AllowUserToAddRows = false;
            this.dgInfoViewer.AllowUserToDeleteRows = false;
            this.dgInfoViewer.AllowUserToOrderColumns = true;
            this.dgInfoViewer.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgInfoViewer.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgInfoViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgInfoViewer.Location = new System.Drawing.Point(0, 0);
            this.dgInfoViewer.MultiSelect = false;
            this.dgInfoViewer.Name = "dgInfoViewer";
            this.dgInfoViewer.ReadOnly = true;
            this.dgInfoViewer.Size = new System.Drawing.Size(284, 262);
            this.dgInfoViewer.TabIndex = 0;
            // 
            // ItemInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.dgInfoViewer);
            this.Name = "ItemInfoForm";
            this.Text = "ItemInfoForm";
            ((System.ComponentModel.ISupportInitialize)(this.dgInfoViewer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgInfoViewer;
    }
}