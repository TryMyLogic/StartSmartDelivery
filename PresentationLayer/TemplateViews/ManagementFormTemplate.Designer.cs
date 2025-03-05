namespace StartSmartDeliveryForm.PresentationLayer
{
    partial class ManagementFormTemplate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManagementFormTemplate));
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            changeUserToolStripMenuItem = new ToolStripMenuItem();
            navigateToolStripMenuItem = new ToolStripMenuItem();
            dashboardToolStripMenuItem = new ToolStripMenuItem();
            deliveryManagementToolStripMenuItem = new ToolStripMenuItem();
            vehicleManagementToolStripMenuItem = new ToolStripMenuItem();
            driverManagementToolStripMenuItem = new ToolStripMenuItem();
            dataManagementToolStripMenuItem = new ToolStripMenuItem();
            rollbackToolStripMenuItem = new ToolStripMenuItem();
            reloadToolStripMenuItem = new ToolStripMenuItem();
            printAllPagesByRowCountToolStripMenuItem = new ToolStripMenuItem();
            pnlGap = new Panel();
            tsSearchbar = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            cboSearchOptions = new ToolStripComboBox();
            txtSearchBox = new ToolStripTextBox();
            btnRefresh = new ToolStripButton();
            btnMatchCase = new ToolStripButton();
            btnSearch = new ToolStripButton();
            tableLayoutPanelBottom = new TableLayoutPanel();
            tableLayoutPanelBottomInnerRight = new TableLayoutPanel();
            btnAdd = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnNext = new Button();
            btnLast = new Button();
            flowLayoutPanel2 = new FlowLayoutPanel();
            btnPrevious = new Button();
            btnFirst = new Button();
            tableLayoutPanelBottomInnerLeft = new TableLayoutPanel();
            btnPrint = new Button();
            flowLayoutPanel3 = new FlowLayoutPanel();
            label1 = new Label();
            txtStartPage = new TextBox();
            lblEndPage = new Label();
            btnGotoPage = new Button();
            dgvMain = new DataGridView();
            menuStrip1.SuspendLayout();
            tsSearchbar.SuspendLayout();
            tableLayoutPanelBottom.SuspendLayout();
            tableLayoutPanelBottomInnerRight.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            tableLayoutPanelBottomInnerLeft.SuspendLayout();
            flowLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMain).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, navigateToolStripMenuItem, dataManagementToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(6, 3, 0, 3);
            menuStrip1.Size = new Size(795, 30);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { changeUserToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(52, 24);
            fileToolStripMenuItem.Text = "User";
            // 
            // changeUserToolStripMenuItem
            // 
            changeUserToolStripMenuItem.Name = "changeUserToolStripMenuItem";
            changeUserToolStripMenuItem.Size = new Size(224, 26);
            changeUserToolStripMenuItem.Text = "Change User";
            // 
            // navigateToolStripMenuItem
            // 
            navigateToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { dashboardToolStripMenuItem, deliveryManagementToolStripMenuItem, vehicleManagementToolStripMenuItem, driverManagementToolStripMenuItem });
            navigateToolStripMenuItem.Name = "navigateToolStripMenuItem";
            navigateToolStripMenuItem.Size = new Size(83, 24);
            navigateToolStripMenuItem.Text = "Navigate";
            // 
            // dashboardToolStripMenuItem
            // 
            dashboardToolStripMenuItem.Name = "dashboardToolStripMenuItem";
            dashboardToolStripMenuItem.Size = new Size(238, 26);
            dashboardToolStripMenuItem.Text = "Dashboard";
            // 
            // deliveryManagementToolStripMenuItem
            // 
            deliveryManagementToolStripMenuItem.Name = "deliveryManagementToolStripMenuItem";
            deliveryManagementToolStripMenuItem.Size = new Size(238, 26);
            deliveryManagementToolStripMenuItem.Text = "Delivery Management";
            // 
            // vehicleManagementToolStripMenuItem
            // 
            vehicleManagementToolStripMenuItem.Name = "vehicleManagementToolStripMenuItem";
            vehicleManagementToolStripMenuItem.Size = new Size(238, 26);
            vehicleManagementToolStripMenuItem.Text = "Vehicle Management";
            // 
            // driverManagementToolStripMenuItem
            // 
            driverManagementToolStripMenuItem.Name = "driverManagementToolStripMenuItem";
            driverManagementToolStripMenuItem.Size = new Size(238, 26);
            driverManagementToolStripMenuItem.Text = "Driver Management";
            // 
            // dataManagementToolStripMenuItem
            // 
            dataManagementToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { rollbackToolStripMenuItem, reloadToolStripMenuItem, printAllPagesByRowCountToolStripMenuItem });
            dataManagementToolStripMenuItem.Name = "dataManagementToolStripMenuItem";
            dataManagementToolStripMenuItem.Size = new Size(147, 24);
            dataManagementToolStripMenuItem.Text = "Data Management";
            // 
            // rollbackToolStripMenuItem
            // 
            rollbackToolStripMenuItem.Name = "rollbackToolStripMenuItem";
            rollbackToolStripMenuItem.Size = new Size(292, 26);
            rollbackToolStripMenuItem.Text = "Rollback";
            rollbackToolStripMenuItem.Click += rollbackToolStripMenuItem_Click;
            // 
            // reloadToolStripMenuItem
            // 
            reloadToolStripMenuItem.Name = "reloadToolStripMenuItem";
            reloadToolStripMenuItem.Size = new Size(292, 26);
            reloadToolStripMenuItem.Text = "Reload";
            reloadToolStripMenuItem.Click += reloadToolStripMenuItem_Click;
            // 
            // printAllPagesByRowCountToolStripMenuItem
            // 
            printAllPagesByRowCountToolStripMenuItem.Name = "printAllPagesByRowCountToolStripMenuItem";
            printAllPagesByRowCountToolStripMenuItem.Size = new Size(292, 26);
            printAllPagesByRowCountToolStripMenuItem.Text = "Print all pages (By Row Count)";
            printAllPagesByRowCountToolStripMenuItem.Click += printAllPagesByRowCountToolStripMenuItem_Click;
            // 
            // pnlGap
            // 
            pnlGap.BackColor = Color.AntiqueWhite;
            pnlGap.Dock = DockStyle.Top;
            pnlGap.Location = new Point(0, 30);
            pnlGap.Margin = new Padding(0);
            pnlGap.Name = "pnlGap";
            pnlGap.Size = new Size(795, 15);
            pnlGap.TabIndex = 4;
            // 
            // tsSearchbar
            // 
            tsSearchbar.BackColor = Color.AntiqueWhite;
            tsSearchbar.ImageScalingSize = new Size(20, 20);
            tsSearchbar.Items.AddRange(new ToolStripItem[] { toolStripLabel1, cboSearchOptions, txtSearchBox, btnRefresh, btnMatchCase, btnSearch });
            tsSearchbar.LayoutStyle = ToolStripLayoutStyle.Flow;
            tsSearchbar.Location = new Point(0, 45);
            tsSearchbar.Name = "tsSearchbar";
            tsSearchbar.Padding = new Padding(0, 3, 0, 7);
            tsSearchbar.RenderMode = ToolStripRenderMode.Professional;
            tsSearchbar.Size = new Size(795, 38);
            tsSearchbar.Stretch = true;
            tsSearchbar.TabIndex = 5;
            tsSearchbar.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Margin = new Padding(15, 1, 25, 2);
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(53, 20);
            toolStripLabel1.Text = "Search";
            // 
            // cboSearchOptions
            // 
            cboSearchOptions.FlatStyle = FlatStyle.Flat;
            cboSearchOptions.Items.AddRange(new object[] { "All columns" });
            cboSearchOptions.Margin = new Padding(0, 0, 25, 0);
            cboSearchOptions.Name = "cboSearchOptions";
            cboSearchOptions.Size = new Size(121, 28);
            // 
            // txtSearchBox
            // 
            txtSearchBox.BackColor = SystemColors.Window;
            txtSearchBox.ForeColor = Color.LightGray;
            txtSearchBox.Margin = new Padding(0, 0, 25, 0);
            txtSearchBox.Name = "txtSearchBox";
            txtSearchBox.Size = new Size(430, 27);
            txtSearchBox.Tag = "";
            txtSearchBox.Text = "Value for search";
            txtSearchBox.Enter += txtSearchBox_Enter;
            // 
            // btnRefresh
            // 
            btnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnRefresh.Image = (Image)resources.GetObject("btnRefresh.Image");
            btnRefresh.ImageTransparentColor = Color.Magenta;
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(29, 24);
            btnRefresh.Text = "Refresh datagridview";
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnMatchCase
            // 
            btnMatchCase.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnMatchCase.Image = (Image)resources.GetObject("btnMatchCase.Image");
            btnMatchCase.ImageTransparentColor = Color.Magenta;
            btnMatchCase.Margin = new Padding(2);
            btnMatchCase.Name = "btnMatchCase";
            btnMatchCase.Size = new Size(29, 24);
            btnMatchCase.Text = "Match Case";
            btnMatchCase.Click += btnMatchCase_Click;
            // 
            // btnSearch
            // 
            btnSearch.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnSearch.Image = (Image)resources.GetObject("btnSearch.Image");
            btnSearch.ImageTransparentColor = Color.Magenta;
            btnSearch.Margin = new Padding(2);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(29, 24);
            btnSearch.Text = "Search";
            btnSearch.Click += btnSearch_Click;
            // 
            // tableLayoutPanelBottom
            // 
            tableLayoutPanelBottom.AutoSize = true;
            tableLayoutPanelBottom.ColumnCount = 3;
            tableLayoutPanelBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelBottom.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanelBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelBottom.Controls.Add(tableLayoutPanelBottomInnerRight, 2, 1);
            tableLayoutPanelBottom.Controls.Add(flowLayoutPanel1, 2, 0);
            tableLayoutPanelBottom.Controls.Add(flowLayoutPanel2, 0, 0);
            tableLayoutPanelBottom.Controls.Add(tableLayoutPanelBottomInnerLeft, 0, 1);
            tableLayoutPanelBottom.Controls.Add(flowLayoutPanel3, 1, 0);
            tableLayoutPanelBottom.Dock = DockStyle.Bottom;
            tableLayoutPanelBottom.Location = new Point(0, 375);
            tableLayoutPanelBottom.Name = "tableLayoutPanelBottom";
            tableLayoutPanelBottom.RowCount = 2;
            tableLayoutPanelBottom.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelBottom.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelBottom.Size = new Size(795, 106);
            tableLayoutPanelBottom.TabIndex = 6;
            // 
            // tableLayoutPanelBottomInnerRight
            // 
            tableLayoutPanelBottomInnerRight.ColumnCount = 2;
            tableLayoutPanelBottomInnerRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelBottomInnerRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelBottomInnerRight.Controls.Add(btnAdd, 0, 0);
            tableLayoutPanelBottomInnerRight.Dock = DockStyle.Fill;
            tableLayoutPanelBottomInnerRight.Location = new Point(528, 56);
            tableLayoutPanelBottomInnerRight.Name = "tableLayoutPanelBottomInnerRight";
            tableLayoutPanelBottomInnerRight.RowCount = 1;
            tableLayoutPanelBottomInnerRight.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelBottomInnerRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 47F));
            tableLayoutPanelBottomInnerRight.Size = new Size(264, 47);
            tableLayoutPanelBottomInnerRight.TabIndex = 9;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.None;
            btnAdd.BackColor = Color.White;
            tableLayoutPanelBottomInnerRight.SetColumnSpan(btnAdd, 2);
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.ForeColor = Color.Black;
            btnAdd.Location = new Point(63, 9);
            btnAdd.Margin = new Padding(3, 3, 40, 3);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(101, 29);
            btnAdd.TabIndex = 6;
            btnAdd.TabStop = false;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = false;
            btnAdd.Click += btnAdd_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btnNext);
            flowLayoutPanel1.Controls.Add(btnLast);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(528, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(264, 47);
            flowLayoutPanel1.TabIndex = 6;
            // 
            // btnNext
            // 
            btnNext.BackColor = Color.Transparent;
            btnNext.FlatStyle = FlatStyle.Flat;
            btnNext.ForeColor = Color.Black;
            btnNext.Image = (Image)resources.GetObject("btnNext.Image");
            btnNext.Location = new Point(3, 3);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(101, 29);
            btnNext.TabIndex = 9;
            btnNext.TabStop = false;
            btnNext.UseVisualStyleBackColor = false;
            btnNext.Click += btnNext_Click;
            // 
            // btnLast
            // 
            btnLast.BackColor = Color.Transparent;
            btnLast.FlatStyle = FlatStyle.Flat;
            btnLast.ForeColor = Color.Black;
            btnLast.Image = (Image)resources.GetObject("btnLast.Image");
            btnLast.Location = new Point(110, 3);
            btnLast.Name = "btnLast";
            btnLast.Size = new Size(101, 29);
            btnLast.TabIndex = 10;
            btnLast.TabStop = false;
            btnLast.UseVisualStyleBackColor = false;
            btnLast.Click += btnLast_Click;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Controls.Add(btnPrevious);
            flowLayoutPanel2.Controls.Add(btnFirst);
            flowLayoutPanel2.Dock = DockStyle.Fill;
            flowLayoutPanel2.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel2.Location = new Point(3, 3);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(263, 47);
            flowLayoutPanel2.TabIndex = 7;
            // 
            // btnPrevious
            // 
            btnPrevious.BackColor = Color.Transparent;
            btnPrevious.FlatStyle = FlatStyle.Flat;
            btnPrevious.ForeColor = Color.Black;
            btnPrevious.Image = (Image)resources.GetObject("btnPrevious.Image");
            btnPrevious.Location = new Point(159, 3);
            btnPrevious.Name = "btnPrevious";
            btnPrevious.Size = new Size(101, 29);
            btnPrevious.TabIndex = 2;
            btnPrevious.TabStop = false;
            btnPrevious.UseVisualStyleBackColor = false;
            btnPrevious.Click += btnPrevious_Click;
            // 
            // btnFirst
            // 
            btnFirst.BackColor = Color.Transparent;
            btnFirst.FlatStyle = FlatStyle.Flat;
            btnFirst.ForeColor = Color.Black;
            btnFirst.Image = (Image)resources.GetObject("btnFirst.Image");
            btnFirst.Location = new Point(52, 3);
            btnFirst.Name = "btnFirst";
            btnFirst.Size = new Size(101, 29);
            btnFirst.TabIndex = 3;
            btnFirst.TabStop = false;
            btnFirst.UseVisualStyleBackColor = false;
            btnFirst.Click += btnFirst_Click;
            // 
            // tableLayoutPanelBottomInnerLeft
            // 
            tableLayoutPanelBottomInnerLeft.ColumnCount = 2;
            tableLayoutPanelBottomInnerLeft.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelBottomInnerLeft.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelBottomInnerLeft.Controls.Add(btnPrint, 0, 0);
            tableLayoutPanelBottomInnerLeft.Dock = DockStyle.Fill;
            tableLayoutPanelBottomInnerLeft.Location = new Point(3, 56);
            tableLayoutPanelBottomInnerLeft.Name = "tableLayoutPanelBottomInnerLeft";
            tableLayoutPanelBottomInnerLeft.RowCount = 1;
            tableLayoutPanelBottomInnerLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelBottomInnerLeft.RowStyles.Add(new RowStyle(SizeType.Absolute, 47F));
            tableLayoutPanelBottomInnerLeft.Size = new Size(263, 47);
            tableLayoutPanelBottomInnerLeft.TabIndex = 8;
            // 
            // btnPrint
            // 
            btnPrint.Anchor = AnchorStyles.None;
            tableLayoutPanelBottomInnerLeft.SetColumnSpan(btnPrint, 2);
            btnPrint.FlatStyle = FlatStyle.Flat;
            btnPrint.ForeColor = Color.Black;
            btnPrint.Location = new Point(99, 9);
            btnPrint.Margin = new Padding(40, 3, 3, 3);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(101, 29);
            btnPrint.TabIndex = 6;
            btnPrint.TabStop = false;
            btnPrint.Text = "Print";
            btnPrint.UseVisualStyleBackColor = true;
            btnPrint.Click += btnPrint_Click;
            // 
            // flowLayoutPanel3
            // 
            flowLayoutPanel3.Controls.Add(label1);
            flowLayoutPanel3.Controls.Add(txtStartPage);
            flowLayoutPanel3.Controls.Add(lblEndPage);
            flowLayoutPanel3.Controls.Add(btnGotoPage);
            flowLayoutPanel3.Dock = DockStyle.Fill;
            flowLayoutPanel3.Location = new Point(272, 3);
            flowLayoutPanel3.Name = "flowLayoutPanel3";
            flowLayoutPanel3.Size = new Size(250, 47);
            flowLayoutPanel3.TabIndex = 10;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 11);
            label1.Margin = new Padding(3, 11, 3, 0);
            label1.Name = "label1";
            label1.Size = new Size(44, 20);
            label1.TabIndex = 0;
            label1.Text = "Page:";
            // 
            // txtStartPage
            // 
            txtStartPage.Location = new Point(50, 8);
            txtStartPage.Margin = new Padding(0, 8, 0, 0);
            txtStartPage.Name = "txtStartPage";
            txtStartPage.Size = new Size(79, 27);
            txtStartPage.TabIndex = 1;
            txtStartPage.TabStop = false;
            txtStartPage.TextAlign = HorizontalAlignment.Right;
            txtStartPage.Enter += txtStartPage_Enter;
            // 
            // lblEndPage
            // 
            lblEndPage.Location = new Point(132, 11);
            lblEndPage.Margin = new Padding(3, 11, 3, 0);
            lblEndPage.Name = "lblEndPage";
            lblEndPage.Size = new Size(80, 20);
            lblEndPage.TabIndex = 2;
            lblEndPage.Text = "/End";
            // 
            // btnGotoPage
            // 
            btnGotoPage.Dock = DockStyle.Right;
            btnGotoPage.FlatStyle = FlatStyle.Flat;
            btnGotoPage.ForeColor = Color.Transparent;
            btnGotoPage.Image = (Image)resources.GetObject("btnGotoPage.Image");
            btnGotoPage.Location = new Point(215, 4);
            btnGotoPage.Margin = new Padding(0, 4, 0, 0);
            btnGotoPage.Name = "btnGotoPage";
            btnGotoPage.Size = new Size(30, 31);
            btnGotoPage.TabIndex = 3;
            btnGotoPage.TabStop = false;
            btnGotoPage.UseVisualStyleBackColor = true;
            btnGotoPage.Click += btnGotoPage_Click;
            // 
            // dgvMain
            // 
            dgvMain.AllowUserToAddRows = false;
            dgvMain.AllowUserToDeleteRows = false;
            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.Dock = DockStyle.Fill;
            dgvMain.ImeMode = ImeMode.NoControl;
            dgvMain.Location = new Point(0, 83);
            dgvMain.Name = "dgvMain";
            dgvMain.ReadOnly = true;
            dgvMain.RowHeadersWidth = 51;
            dgvMain.Size = new Size(795, 292);
            dgvMain.TabIndex = 7;
            dgvMain.CellContentClick += dgvMain_CellContentClick;
            // 
            // ManagementTemplateForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(795, 481);
            Controls.Add(dgvMain);
            Controls.Add(tableLayoutPanelBottom);
            Controls.Add(tsSearchbar);
            Controls.Add(pnlGap);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(779, 495);
            Name = "ManagementTemplateForm";
            Text = "ManagementTemplateForm";
            Load += ManagementTemplateForm_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            tsSearchbar.ResumeLayout(false);
            tsSearchbar.PerformLayout();
            tableLayoutPanelBottom.ResumeLayout(false);
            tableLayoutPanelBottomInnerRight.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel2.ResumeLayout(false);
            tableLayoutPanelBottomInnerLeft.ResumeLayout(false);
            flowLayoutPanel3.ResumeLayout(false);
            flowLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMain).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem navigateToolStripMenuItem;
        private ToolStripMenuItem dataManagementToolStripMenuItem;
        private ToolStrip tsSearchbar;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox cboSearchOptions;
        private ToolStripTextBox txtSearchBox;
        private ToolStripButton btnMatchCase;
        private ToolStripButton btnSearch;
        private TableLayoutPanel tableLayoutPanelBottom;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button btnNext;
        private Button btnLast;
        private FlowLayoutPanel flowLayoutPanel2;
        private Button btnFirst;
        private Button btnPrevious;
        private TableLayoutPanel tableLayoutPanelBottomInnerLeft;
        private Button btnPrint;
        private TableLayoutPanel tableLayoutPanelBottomInnerRight;
        private Button btnAdd;
        private FlowLayoutPanel flowLayoutPanel3;
        private Label label1;
        protected TextBox txtStartPage;
        protected Label lblEndPage;
        private Button btnGotoPage;
        private Panel pnlGap;
        protected DataGridView dgvMain;
        private ToolStripMenuItem changeUserToolStripMenuItem;
        private ToolStripMenuItem dashboardToolStripMenuItem;
        private ToolStripMenuItem deliveryManagementToolStripMenuItem;
        private ToolStripMenuItem vehicleManagementToolStripMenuItem;
        private ToolStripMenuItem driverManagementToolStripMenuItem;
        private ToolStripMenuItem rollbackToolStripMenuItem;
        private ToolStripMenuItem reloadToolStripMenuItem;
        private ToolStripButton btnRefresh;
        private ToolStripMenuItem printAllPagesByRowCountToolStripMenuItem;
    }
}
