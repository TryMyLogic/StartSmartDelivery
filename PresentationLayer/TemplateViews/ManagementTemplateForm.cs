using System.Data;
using System.Reflection;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;
using Serilog;
using StartSmartDeliveryForm.PresentationLayer.TemplatePresenters;
using StartSmartDeliveryForm.SharedLayer.Interfaces;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;

namespace StartSmartDeliveryForm.PresentationLayer
{
    public partial class ManagementTemplateForm : Form, IManagementForm, ISearchableView
    {
        public ManagementTemplateForm()
        {
            InitializeComponent();
        }

        private void ManagementTemplateForm_Load(object sender, EventArgs e)
        {
            SetTheme();
            dgvMain.RowHeadersVisible = false; // Hides Row Number Column
        }

        public event EventHandler<SearchRequestEventArgs>? SearchClicked;
        private bool _isCaseSensitive = false;
        DataTable IManagementForm.DgvTable
        {
            get => dgvMain.DataSource as DataTable ?? throw new InvalidOperationException("DataSource is not a DataTable.");
            set => dgvMain.DataSource = value;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string? SelectedOption = cboSearchOptions.SelectedItem?.ToString();
            string SearchTerm = txtSearchBox.Text.Trim();

            if (SelectedOption != null)
            {
                SearchClicked?.Invoke(this, new SearchRequestEventArgs((DataTable)dgvMain.DataSource, SelectedOption, SearchTerm, _isCaseSensitive));
            }
            else
            {
                Log.Error("Search coloumn was not set properly");
            }
        }

        private void btnMatchCase_Click(object sender, EventArgs e)
        {
            _isCaseSensitive = !_isCaseSensitive;

            if (_isCaseSensitive)
            {
                btnMatchCase.BackColor = Color.White;
            }
            else
            {
                btnMatchCase.BackColor = GlobalConstants.SoftBeige;
            }
        }

        private void txtSearchBox_Enter(object sender, EventArgs e)
        {
            if (txtSearchBox.Text == "Value for search")
            {
                txtSearchBox.Text = "";
                txtSearchBox.ForeColor = Color.Black;
            }
        }

        private void txtSearchBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchBox.Text))
            {
                txtSearchBox.Text = "Value for search";
                txtSearchBox.ForeColor = Color.Gray;
            }
        }

        private void txtStartPage_Enter(object sender, EventArgs e)
        {
            if (sender is TextBox textBox)
            {
                BeginInvoke(new Action(() => textBox.SelectAll()));
            }
        }

        private static Bitmap ResizeImage(Image img, int width, int height)
        {
            Bitmap resizedImage = new(img, new Size(width, height));
            return resizedImage;
        }

        private void SetTheme()
        {
            tsSearchbar.BackColor = GlobalConstants.SoftBeige;
            pnlGap.BackColor = GlobalConstants.SoftBeige;

            btnFirst.BackColor = GlobalConstants.MintGreen;
            btnPrevious.BackColor = GlobalConstants.MintGreen;
            btnNext.BackColor = GlobalConstants.MintGreen;
            btnLast.BackColor = GlobalConstants.MintGreen;
            btnPrint.BackColor = GlobalConstants.SoftBeige;
            btnAdd.BackColor = GlobalConstants.SoftBeige;

            btnFirst.FlatAppearance.BorderSize = 0;
            btnPrevious.FlatAppearance.BorderSize = 0;
            btnNext.FlatAppearance.BorderSize = 0;
            btnLast.FlatAppearance.BorderSize = 0;
            btnPrint.FlatAppearance.BorderSize = 0;
            btnAdd.FlatAppearance.BorderSize = 0;

            //Prevents image from touching borders
            if (btnFirst.Image != null)
            {
                btnFirst.Image = ResizeImage(btnFirst.Image, 20, 20);
            }

            if (btnLast.Image != null)
            {
                btnLast.Image = ResizeImage(btnLast.Image, 20, 20);
            }
        }

        //DO NOT use this in the ManagementTemplateForm_Load.It interferes with children initialization, breaking the child designer. 
        protected static void AdjustDataGridViewHeight(DataGridView dataGridView)
        {
            int records = Math.Min(GlobalConstants.s_recordLimit, 30);

            int rowHeight = dataGridView.RowTemplate.Height;

            int requiredHeight = rowHeight * (records + 1) + dataGridView.ColumnHeadersHeight + 2;

            if (dataGridView.Parent != null)
            {
                int formHeightWithoutDataGridView = dataGridView.Parent.Height - dataGridView.Height;
                int newHeight = formHeightWithoutDataGridView + requiredHeight;
                dataGridView.Parent.Height = newHeight;
                Log.Information($"Row Height: {rowHeight}, Column Headers Height: {dataGridView.ColumnHeadersHeight}, New Height: {newHeight}");
            }
            else
            {
                Log.Warning("DataGridView parent is null.");
            }
        }



        protected virtual HashSet<string> GetExcludedColumns()
        {
            return []; // By default, exclude nothing.
        }

        protected void SetSearchOptions(Type Dto)
        {
            cboSearchOptions.Items.Clear();

            //Makes comparison case insensitive
            var ExcludedColumns = GetExcludedColumns().Select(c => c.ToLower()).ToHashSet();
            PropertyInfo[] properties = Dto.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!ExcludedColumns.Contains(property.Name.ToLower()))
                    cboSearchOptions.Items.Add(property.Name);
            }

            cboSearchOptions.SelectedIndex = 0;
        }

        protected void AddEditDeleteButtons()
        {
            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "PresentationLayer", "Images", "EditIcon.png");
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "PresentationLayer", "Images", "DeleteIcon.png");

            //Resolve filepaths
            string editIconPath = Path.GetFullPath(filepath);
            string deleteIconPath = Path.GetFullPath(path);


            DataGridViewImageColumn editButtonColumn = new()
            {
                Name = "Edit",
                HeaderText = "",
                Image = Image.FromFile(editIconPath),
                Width = 30,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };

            DataGridViewImageColumn deleteButtonColumn = new()
            {
                Name = "Delete",
                HeaderText = "",
                Image = Image.FromFile(deleteIconPath),
                Width = 30,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };

            dgvMain.Columns.Add(editButtonColumn);
            dgvMain.Columns.Add(deleteButtonColumn);
        }

        private void dgvMain_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (dgvMain.Columns[e.ColumnIndex].Name == "Edit")
                {
                    btnEdit_Click(e.RowIndex);
                }
                else if (dgvMain.Columns[e.ColumnIndex].Name == "Delete")
                {
                    btnDelete_Click(e.RowIndex);
                }
            }
        }

        //Required by Children:
        public event EventHandler? AddClicked;
        public event EventHandler<int>? EditClicked;
        public event EventHandler<int>? DeleteClicked;
        public event EventHandler? RefreshClicked;
        public event EventHandler? ReloadClicked;
        public event EventHandler? RollbackClicked;
        public event EventHandler? PrintAllPagesByRowCountClicked;
        public event EventHandler? FirstPageClicked;
        public event EventHandler? PreviousPageClicked;
        public event EventHandler? NextPageClicked;
        public event EventHandler? LastPageClicked;
        public event EventHandler? GoToPageClicked;
        public event EventHandler? PrintClicked;

        // Presenter
        protected virtual void btnAdd_Click(object sender, EventArgs e) { AddClicked?.Invoke(sender, e); }
        protected virtual void btnEdit_Click(int RowIndex) { EditClicked?.Invoke(this, RowIndex); }
        private async void btnDelete_Click(int RowIndex)
        {
            await btnDelete_ClickAsync(RowIndex);
            DeleteClicked?.Invoke(this, RowIndex);
        }
        protected virtual async Task btnDelete_ClickAsync(int RowIndex) { await Task.Delay(500); }
        protected virtual void btnRefresh_Click(object sender, EventArgs e) { RefreshClicked?.Invoke(sender, e); }

        private async void reloadToolStripMenuItem_Click(object sender, EventArgs e) { await reloadToolStripMenuItem_ClickAsync(sender, e); }
        protected virtual async Task reloadToolStripMenuItem_ClickAsync(object sender, EventArgs e)
        {
            await Task.Delay(500);
            ReloadClicked?.Invoke(sender, e);
        }

        protected virtual void rollbackToolStripMenuItem_Click(object sender, EventArgs e) { RollbackClicked?.Invoke(sender, e); }
        protected virtual void printAllPagesByRowCountToolStripMenuItem_Click(object sender, EventArgs e) { PrintAllPagesByRowCountClicked?.Invoke(sender, e); }


        private async void btnFirst_Click(object sender, EventArgs e) { await btnFirst_ClickAsync(sender, e); }
        protected virtual async Task btnFirst_ClickAsync(object sender, EventArgs e)
        {
            await Task.Delay(500);
            FirstPageClicked?.Invoke(sender, e);
        }

        private async void btnPrevious_Click(object sender, EventArgs e) { await btnPrevious_ClickAsync(sender, e); }
        protected virtual async Task btnPrevious_ClickAsync(object sender, EventArgs e)
        {
            await Task.Delay(500);
            PreviousPageClicked?.Invoke(sender, e);
        }

        private async void btnNext_Click(object sender, EventArgs e) { await btnNext_ClickAsync(sender, e); }
        protected virtual async Task btnNext_ClickAsync(object sender, EventArgs e) { await Task.Delay(500);
            NextPageClicked?.Invoke(sender, e);
        }

        private async void btnLast_Click(object sender, EventArgs e) { await btnLast_ClickAsync(sender, e); }
        protected virtual async Task btnLast_ClickAsync(object sender, EventArgs e) { await Task.Delay(500);
            LastPageClicked?.Invoke(sender, e);
        }

        private async void btnGotoPage_Click(object sender, EventArgs e) { await btnGotoPage_ClickAsync(sender, e); }
        protected virtual async Task btnGotoPage_ClickAsync(object sender, EventArgs e) { await Task.Delay(500);
            GoToPageClicked?.Invoke(sender, e);
        }

        protected virtual void btnPrint_Click(object sender, EventArgs e) { PrintClicked?.Invoke(sender, e); }

        // View specific

        protected virtual void dgvMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) { }
    }
}
