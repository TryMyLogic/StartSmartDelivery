namespace StartSmartDeliveryForm.PresentationLayer.TemplateViews
{
    partial class GenericDataFormTemplate
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
            tableLayoutPanel1 = new TableLayoutPanel();
            btnSubmit = new Button();
            tlpDynamicFields = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(btnSubmit, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Bottom;
            tableLayoutPanel1.Location = new Point(0, 423);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(800, 27);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // btnSubmit
            // 
            btnSubmit.Anchor = AnchorStyles.None;
            tableLayoutPanel1.SetColumnSpan(btnSubmit, 2);
            btnSubmit.FlatStyle = FlatStyle.Flat;
            btnSubmit.Location = new Point(359, 0);
            btnSubmit.Margin = new Padding(0, 0, 0, 5);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(82, 22);
            btnSubmit.TabIndex = 11;
            btnSubmit.Text = "Submit";
            btnSubmit.UseVisualStyleBackColor = true;
            btnSubmit.Click += btnSubmit_Click;
            // 
            // tlpDynamicFields
            // 
            tlpDynamicFields.AutoScroll = true;
            tlpDynamicFields.ColumnCount = 1;
            tlpDynamicFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpDynamicFields.Dock = DockStyle.Fill;
            tlpDynamicFields.Location = new Point(0, 0);
            tlpDynamicFields.Name = "tlpDynamicFields";
            tlpDynamicFields.RowCount = 1;
            tlpDynamicFields.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpDynamicFields.Size = new Size(800, 423);
            tlpDynamicFields.TabIndex = 1;
            // 
            // GenericDataFormTemplate
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tlpDynamicFields);
            Controls.Add(tableLayoutPanel1);
            Name = "GenericDataFormTemplate";
            Text = "DataFormTemplate";
            Load += GenericDataForm_Load;
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Button btnSubmit;
        private TableLayoutPanel tlpDynamicFields;
    }
}
