namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement
{
    partial class DriverManagementForm
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
            SuspendLayout();
            // 
            // DriverManagementForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(761, 479);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(779, 500);
            Name = "DriverManagementForm";
            Text = "DriverManagementForm";
            Load += DriverManagementForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}