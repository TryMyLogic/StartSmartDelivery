namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement
{
    partial class DriverDataForm
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
            label1 = new Label();
            txtName = new TextBox();
            txtSurname = new TextBox();
            label2 = new Label();
            label3 = new Label();
            txtEmployeeNo = new TextBox();
            label4 = new Label();
            label5 = new Label();
            cboAvailability = new ComboBox();
            cboLicenseType = new ComboBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(14, 12);
            label1.Name = "label1";
            label1.Size = new Size(39, 15);
            label1.TabIndex = 0;
            label1.Text = "Name";
            // 
            // txtName
            // 
            txtName.Location = new Point(14, 30);
            txtName.Margin = new Padding(3, 2, 3, 2);
            txtName.Name = "txtName";
            txtName.Size = new Size(132, 23);
            txtName.TabIndex = 1;
            // 
            // txtSurname
            // 
            txtSurname.Location = new Point(168, 30);
            txtSurname.Margin = new Padding(3, 2, 3, 2);
            txtSurname.Name = "txtSurname";
            txtSurname.Size = new Size(132, 23);
            txtSurname.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(168, 12);
            label2.Name = "label2";
            label2.Size = new Size(54, 15);
            label2.TabIndex = 2;
            label2.Text = "Surname";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(167, 62);
            label3.Name = "label3";
            label3.Size = new Size(71, 15);
            label3.TabIndex = 6;
            label3.Text = "LicenseType";
            // 
            // txtEmployeeNo
            // 
            txtEmployeeNo.Location = new Point(13, 80);
            txtEmployeeNo.Margin = new Padding(3, 2, 3, 2);
            txtEmployeeNo.Name = "txtEmployeeNo";
            txtEmployeeNo.Size = new Size(132, 23);
            txtEmployeeNo.TabIndex = 5;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(13, 62);
            label4.Name = "label4";
            label4.Size = new Size(75, 15);
            label4.TabIndex = 4;
            label4.Text = "EmployeeNo";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(91, 108);
            label5.Name = "label5";
            label5.Size = new Size(65, 15);
            label5.TabIndex = 8;
            label5.Text = "Availability";
            // 
            // cboAvailability
            // 
            cboAvailability.FlatStyle = FlatStyle.Flat;
            cboAvailability.FormattingEnabled = true;
            cboAvailability.Items.AddRange(new object[] { "True", "False" });
            cboAvailability.Location = new Point(91, 126);
            cboAvailability.Margin = new Padding(3, 2, 3, 2);
            cboAvailability.Name = "cboAvailability";
            cboAvailability.Size = new Size(132, 23);
            cboAvailability.TabIndex = 9;
            // 
            // cboLicenseType
            // 
            cboLicenseType.FlatStyle = FlatStyle.Flat;
            cboLicenseType.FormattingEnabled = true;
            cboLicenseType.Items.AddRange(new object[] { "Code8", "Code10", "Code14" });
            cboLicenseType.Location = new Point(168, 80);
            cboLicenseType.Margin = new Padding(3, 2, 3, 2);
            cboLicenseType.Name = "cboLicenseType";
            cboLicenseType.Size = new Size(132, 23);
            cboLicenseType.TabIndex = 11;
            // 
            // DriverDataForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(312, 188);
            Controls.Add(cboLicenseType);
            Controls.Add(cboAvailability);
            Controls.Add(label5);
            Controls.Add(label3);
            Controls.Add(txtEmployeeNo);
            Controls.Add(label4);
            Controls.Add(txtSurname);
            Controls.Add(label2);
            Controls.Add(txtName);
            Controls.Add(label1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "DriverDataForm";
            Text = "DriverDataForm";
            Load += DriverDataForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtName;
        private TextBox txtSurname;
        private Label label2;
        private Label label3;
        private TextBox txtEmployeeNo;
        private Label label4;
        private Label label5;
        private ComboBox cboAvailability;
        private ComboBox cboLicenseType;
    }
}
