namespace WorkPackageApplication
{
    partial class plcWorkTaskSettings
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
            this.cbWTType = new System.Windows.Forms.ComboBox();
            this.cbWTOrientation = new System.Windows.Forms.ComboBox();
            this.txtWTDimension = new System.Windows.Forms.TextBox();
            this.lblWTType = new System.Windows.Forms.Label();
            this.lblDimension = new System.Windows.Forms.Label();
            this.lblWTOrientation = new System.Windows.Forms.Label();
            this.lblTaskDescription = new System.Windows.Forms.Label();
            this.cbtDescription = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cbWTType
            // 
            this.cbWTType.FormattingEnabled = true;
            this.cbWTType.Location = new System.Drawing.Point(138, 35);
            this.cbWTType.Name = "cbWTType";
            this.cbWTType.Size = new System.Drawing.Size(121, 21);
            this.cbWTType.TabIndex = 0;
            this.cbWTType.SelectedValueChanged += new System.EventHandler(this.WTTChanged);
            // 
            // cbWTOrientation
            // 
            this.cbWTOrientation.FormattingEnabled = true;
            this.cbWTOrientation.Location = new System.Drawing.Point(138, 73);
            this.cbWTOrientation.Name = "cbWTOrientation";
            this.cbWTOrientation.Size = new System.Drawing.Size(121, 21);
            this.cbWTOrientation.TabIndex = 1;
            this.cbWTOrientation.SelectedValueChanged += new System.EventHandler(this.WTOChanged);
            // 
            // txtWTDimension
            // 
            this.txtWTDimension.Enabled = false;
            this.txtWTDimension.Location = new System.Drawing.Point(136, 201);
            this.txtWTDimension.Name = "txtWTDimension";
            this.txtWTDimension.Size = new System.Drawing.Size(100, 20);
            this.txtWTDimension.TabIndex = 2;
            this.txtWTDimension.Visible = false;
            this.txtWTDimension.Leave += new System.EventHandler(this.txtWTDimension_Leave);
            // 
            // lblWTType
            // 
            this.lblWTType.AutoSize = true;
            this.lblWTType.Location = new System.Drawing.Point(45, 38);
            this.lblWTType.Name = "lblWTType";
            this.lblWTType.Size = new System.Drawing.Size(58, 13);
            this.lblWTType.TabIndex = 3;
            this.lblWTType.Text = "Task Type";
            // 
            // lblDimension
            // 
            this.lblDimension.AutoSize = true;
            this.lblDimension.Enabled = false;
            this.lblDimension.Location = new System.Drawing.Point(43, 204);
            this.lblDimension.Name = "lblDimension";
            this.lblDimension.Size = new System.Drawing.Size(56, 13);
            this.lblDimension.TabIndex = 4;
            this.lblDimension.Text = "Dimension";
            this.lblDimension.Visible = false;
            // 
            // lblWTOrientation
            // 
            this.lblWTOrientation.AutoSize = true;
            this.lblWTOrientation.Location = new System.Drawing.Point(45, 76);
            this.lblWTOrientation.Name = "lblWTOrientation";
            this.lblWTOrientation.Size = new System.Drawing.Size(58, 13);
            this.lblWTOrientation.TabIndex = 5;
            this.lblWTOrientation.Text = "Orientation";
            // 
            // lblTaskDescription
            // 
            this.lblTaskDescription.AutoSize = true;
            this.lblTaskDescription.Location = new System.Drawing.Point(45, 124);
            this.lblTaskDescription.Name = "lblTaskDescription";
            this.lblTaskDescription.Size = new System.Drawing.Size(87, 13);
            this.lblTaskDescription.TabIndex = 7;
            this.lblTaskDescription.Text = "Task Description";
            // 
            // cbtDescription
            // 
            this.cbtDescription.FormattingEnabled = true;
            this.cbtDescription.Location = new System.Drawing.Point(138, 121);
            this.cbtDescription.Name = "cbtDescription";
            this.cbtDescription.Size = new System.Drawing.Size(121, 21);
            this.cbtDescription.TabIndex = 6;
            this.cbtDescription.SelectedValueChanged += new System.EventHandler(this.WTDChanged);
            // 
            // plcWorkTaskSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.lblTaskDescription);
            this.Controls.Add(this.cbtDescription);
            this.Controls.Add(this.lblWTOrientation);
            this.Controls.Add(this.lblDimension);
            this.Controls.Add(this.lblWTType);
            this.Controls.Add(this.txtWTDimension);
            this.Controls.Add(this.cbWTOrientation);
            this.Controls.Add(this.cbWTType);
            this.Name = "plcWorkTaskSettings";
            this.Text = "Work Task Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbWTType;
        private System.Windows.Forms.ComboBox cbWTOrientation;
        private System.Windows.Forms.TextBox txtWTDimension;
        private System.Windows.Forms.Label lblWTType;
        private System.Windows.Forms.Label lblDimension;
        private System.Windows.Forms.Label lblWTOrientation;
        private System.Windows.Forms.Label lblTaskDescription;
        private System.Windows.Forms.ComboBox cbtDescription;
    }
}