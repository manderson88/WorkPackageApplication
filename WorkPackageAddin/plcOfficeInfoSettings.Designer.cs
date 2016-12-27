namespace WorkPackageApplication
{
    partial class plcOfficeInfoSettings
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
            this.txtOccupant = new System.Windows.Forms.TextBox();
            this.txtOfficeID = new System.Windows.Forms.TextBox();
            this.lblOccupant = new System.Windows.Forms.Label();
            this.lblID = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtOccupant
            // 
            this.txtOccupant.Location = new System.Drawing.Point(150, 21);
            this.txtOccupant.Name = "txtOccupant";
            this.txtOccupant.Size = new System.Drawing.Size(100, 20);
            this.txtOccupant.TabIndex = 0;
            // 
            // txtOfficeID
            // 
            this.txtOfficeID.Location = new System.Drawing.Point(150, 58);
            this.txtOfficeID.Name = "txtOfficeID";
            this.txtOfficeID.Size = new System.Drawing.Size(100, 20);
            this.txtOfficeID.TabIndex = 1;
            // 
            // lblOccupant
            // 
            this.lblOccupant.AutoSize = true;
            this.lblOccupant.Location = new System.Drawing.Point(53, 21);
            this.lblOccupant.Name = "lblOccupant";
            this.lblOccupant.Size = new System.Drawing.Size(54, 13);
            this.lblOccupant.TabIndex = 2;
            this.lblOccupant.Text = "Occupant";
            this.lblOccupant.Click += new System.EventHandler(this.lblOccupant_Click);
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.Location = new System.Drawing.Point(53, 58);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(49, 13);
            this.lblID.TabIndex = 3;
            this.lblID.Text = "Office ID";
            // 
            // plcOfficeInfoSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 101);
            this.Controls.Add(this.lblID);
            this.Controls.Add(this.lblOccupant);
            this.Controls.Add(this.txtOfficeID);
            this.Controls.Add(this.txtOccupant);
            this.Name = "plcOfficeInfoSettings";
            this.Text = "plcOfficeInfoSettings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblOccupant;
        private System.Windows.Forms.Label lblID;
        public System.Windows.Forms.TextBox txtOccupant;
        public System.Windows.Forms.TextBox txtOfficeID;
    }
}