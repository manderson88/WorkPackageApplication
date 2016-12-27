namespace WorkPackageApplication
{
    partial class QueryForm
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
            this.cbxSchemas = new System.Windows.Forms.ComboBox();
            this.cbxClasses = new System.Windows.Forms.ComboBox();
            this.cbxProperty = new System.Windows.Forms.ComboBox();
            this.txtValue = new System.Windows.Forms.TextBox();
            this.cbAllClasses = new System.Windows.Forms.CheckBox();
            this.OKBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cbLike = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cbxSchemas
            // 
            this.cbxSchemas.FormattingEnabled = true;
            this.cbxSchemas.Location = new System.Drawing.Point(91, 21);
            this.cbxSchemas.Name = "cbxSchemas";
            this.cbxSchemas.Size = new System.Drawing.Size(148, 21);
            this.cbxSchemas.TabIndex = 0;
            this.cbxSchemas.SelectedIndexChanged += new System.EventHandler(this.cbxSchemas_SelectedIndexChanged);
            // 
            // cbxClasses
            // 
            this.cbxClasses.FormattingEnabled = true;
            this.cbxClasses.Location = new System.Drawing.Point(91, 87);
            this.cbxClasses.Name = "cbxClasses";
            this.cbxClasses.Size = new System.Drawing.Size(148, 21);
            this.cbxClasses.TabIndex = 1;
            this.cbxClasses.SelectedIndexChanged += new System.EventHandler(this.cbxClasses_SelectedIndexChanged);
            // 
            // cbxProperty
            // 
            this.cbxProperty.FormattingEnabled = true;
            this.cbxProperty.Location = new System.Drawing.Point(91, 150);
            this.cbxProperty.Name = "cbxProperty";
            this.cbxProperty.Size = new System.Drawing.Size(148, 21);
            this.cbxProperty.TabIndex = 2;
            // 
            // txtValue
            // 
            this.txtValue.Location = new System.Drawing.Point(91, 210);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(148, 20);
            this.txtValue.TabIndex = 3;
            // 
            // cbAllClasses
            // 
            this.cbAllClasses.AutoSize = true;
            this.cbAllClasses.Checked = true;
            this.cbAllClasses.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAllClasses.Location = new System.Drawing.Point(28, 114);
            this.cbAllClasses.Name = "cbAllClasses";
            this.cbAllClasses.Size = new System.Drawing.Size(76, 17);
            this.cbAllClasses.TabIndex = 4;
            this.cbAllClasses.Text = "All Classes";
            this.cbAllClasses.UseVisualStyleBackColor = true;
            // 
            // OKBtn
            // 
            this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKBtn.Location = new System.Drawing.Point(43, 260);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 5;
            this.OKBtn.Text = "OK";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(164, 259);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 6;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Schema";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Class";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 158);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Property";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 217);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Value";
            // 
            // cbLike
            // 
            this.cbLike.AutoSize = true;
            this.cbLike.Location = new System.Drawing.Point(157, 114);
            this.cbLike.Name = "cbLike";
            this.cbLike.Size = new System.Drawing.Size(68, 17);
            this.cbLike.TabIndex = 11;
            this.cbLike.Text = "Use Like";
            this.cbLike.UseVisualStyleBackColor = true;
            // 
            // QueryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 296);
            this.Controls.Add(this.cbLike);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OKBtn);
            this.Controls.Add(this.cbAllClasses);
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.cbxProperty);
            this.Controls.Add(this.cbxClasses);
            this.Controls.Add(this.cbxSchemas);
            this.Name = "QueryForm";
            this.Text = "QueryForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbxSchemas;
        private System.Windows.Forms.ComboBox cbxClasses;
        private System.Windows.Forms.ComboBox cbxProperty;
        private System.Windows.Forms.TextBox txtValue;
        private System.Windows.Forms.CheckBox cbAllClasses;
        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbLike;
    }
}