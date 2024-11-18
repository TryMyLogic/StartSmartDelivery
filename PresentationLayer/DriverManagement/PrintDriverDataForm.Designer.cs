namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement
{
    partial class PrintDriverDataForm
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
            printPreviewControl = new PrintPreviewControl();
            tableLayoutPanel1 = new TableLayoutPanel();
            btnSubmit = new Button();
            printDocument = new System.Drawing.Printing.PrintDocument();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // printPreviewControl
            // 
            printPreviewControl.Dock = DockStyle.Fill;
            printPreviewControl.Location = new Point(0, 0);
            printPreviewControl.Name = "printPreviewControl";
            printPreviewControl.Size = new Size(800, 450);
            printPreviewControl.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(btnSubmit, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Bottom;
            tableLayoutPanel1.Location = new Point(0, 415);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(800, 35);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // btnSubmit
            // 
            btnSubmit.Anchor = AnchorStyles.None;
            tableLayoutPanel1.SetColumnSpan(btnSubmit, 2);
            btnSubmit.FlatStyle = FlatStyle.Flat;
            btnSubmit.Location = new Point(353, 3);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(94, 29);
            btnSubmit.TabIndex = 11;
            btnSubmit.Text = "Submit";
            btnSubmit.UseVisualStyleBackColor = true;
            btnSubmit.Click += btnSubmit_Click;
            // 
            // printDocument
            // 
            printDocument.PrintPage += printDocument_PrintPage;
            // 
            // PrintDriverDataForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(printPreviewControl);
            Name = "PrintDriverDataForm";
            Text = "PrintDriverDataForm";
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PrintPreviewControl printPreviewControl;
        private TableLayoutPanel tableLayoutPanel1;
        private Button btnSubmit;
        private System.Drawing.Printing.PrintDocument printDocument;
    }
}