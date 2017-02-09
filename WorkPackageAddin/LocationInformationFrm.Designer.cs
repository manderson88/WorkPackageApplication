namespace WorkPackageApplication
{
    partial class LocationInformationFrm
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
            this.dgLocationInfo = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgLocationInfo)).BeginInit();
            this.SuspendLayout();
            // 
            // dgLocationInfo
            // 
            this.dgLocationInfo.AllowUserToAddRows = false;
            this.dgLocationInfo.AllowUserToDeleteRows = false;
            this.dgLocationInfo.AllowUserToOrderColumns = true;
            this.dgLocationInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgLocationInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgLocationInfo.Location = new System.Drawing.Point(0, 0);
            this.dgLocationInfo.Name = "dgLocationInfo";
            this.dgLocationInfo.ReadOnly = true;
            this.dgLocationInfo.Size = new System.Drawing.Size(284, 262);
            this.dgLocationInfo.TabIndex = 0;
            // 
            // LocationInformationFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.dgLocationInfo);
            this.Name = "LocationInformationFrm";
            this.Text = "LocationInformationFrm";
            ((System.ComponentModel.ISupportInitialize)(this.dgLocationInfo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgLocationInfo;
    }
}