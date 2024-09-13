namespace SmartStartDeliveryForm
{
    partial class DriverManagement : ManagementTemplate
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
            this.label4 = new System.Windows.Forms.Label();
            this.NameTXTBox = new System.Windows.Forms.TextBox();
            this.SurnameTXTBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.EmployeeNumberTXTBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.LicenseTypeCombobox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(281, 344);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 16);
            this.label4.TabIndex = 1;
            this.label4.Text = "Name";
            // 
            // NameTXTBox
            // 
            this.NameTXTBox.Location = new System.Drawing.Point(281, 363);
            this.NameTXTBox.Name = "NameTXTBox";
            this.NameTXTBox.Size = new System.Drawing.Size(144, 22);
            this.NameTXTBox.TabIndex = 0;
            // 
            // SurnameTXTBox
            // 
            this.SurnameTXTBox.Location = new System.Drawing.Point(434, 363);
            this.SurnameTXTBox.Name = "SurnameTXTBox";
            this.SurnameTXTBox.Size = new System.Drawing.Size(144, 22);
            this.SurnameTXTBox.TabIndex = 22;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(431, 345);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 16);
            this.label5.TabIndex = 23;
            this.label5.Text = "Surname";
            // 
            // EmployeeNumberTXTBox
            // 
            this.EmployeeNumberTXTBox.Location = new System.Drawing.Point(284, 416);
            this.EmployeeNumberTXTBox.Name = "EmployeeNumberTXTBox";
            this.EmployeeNumberTXTBox.Size = new System.Drawing.Size(144, 22);
            this.EmployeeNumberTXTBox.TabIndex = 24;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(284, 397);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(120, 16);
            this.label6.TabIndex = 25;
            this.label6.Text = "Employee Number";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(434, 397);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(86, 16);
            this.label7.TabIndex = 27;
            this.label7.Text = "LicenseType";
            // 
            // LicenseTypeCombobox
            // 
            this.LicenseTypeCombobox.FormattingEnabled = true;
            this.LicenseTypeCombobox.Items.AddRange(new object[] {
            "Code8",
            "Code10",
            "Code14"});
            this.LicenseTypeCombobox.Location = new System.Drawing.Point(437, 416);
            this.LicenseTypeCombobox.Name = "LicenseTypeCombobox";
            this.LicenseTypeCombobox.Size = new System.Drawing.Size(141, 24);
            this.LicenseTypeCombobox.TabIndex = 28;
            // 
            // DriverManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(881, 595);
            this.Controls.Add(this.LicenseTypeCombobox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.EmployeeNumberTXTBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.SurnameTXTBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.NameTXTBox);
            this.Controls.Add(this.label4);
            this.Name = "DriverManagement";
            this.Text = "DriverManagement";
            this.Load += new System.EventHandler(this.DriverManagement_Load);
            this.Controls.SetChildIndex(this.label4, 0);
            this.Controls.SetChildIndex(this.NameTXTBox, 0);
            this.Controls.SetChildIndex(this.label5, 0);
            this.Controls.SetChildIndex(this.SurnameTXTBox, 0);
            this.Controls.SetChildIndex(this.label6, 0);
            this.Controls.SetChildIndex(this.EmployeeNumberTXTBox, 0);
            this.Controls.SetChildIndex(this.label7, 0);
            this.Controls.SetChildIndex(this.LicenseTypeCombobox, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox NameTXTBox;
        private System.Windows.Forms.TextBox SurnameTXTBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox EmployeeNumberTXTBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox LicenseTypeCombobox;
    }
}