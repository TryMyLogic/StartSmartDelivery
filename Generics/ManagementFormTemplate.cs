using System.Data;
using System.IO.Abstractions;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.Generics;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.SharedLayer.Interfaces;
using static StartSmartDeliveryForm.Generics.TableDefinition;

namespace StartSmartDeliveryForm.PresentationLayer
{
    // Will rename containing file once original ManagementFormTemplate is deleted.
    public partial class GenericManagementForm : Form, IGenericManagementForm
    {
        private IFileSystem _fileSystem;
        public IFileSystem FileSystem
        {
            get => _fileSystem;
            set => _fileSystem = value ?? throw new ArgumentNullException(nameof(value)); // Prevent null assignment
        }

        private readonly IMessageBox _messageBox;
        private readonly ILogger<GenericManagementForm> _logger;
        private readonly TableConfig _tableConfig;

        public GenericManagementForm() : this(TableConfigs.Empty, NullLogger<GenericManagementForm>.Instance, new MessageBoxWrapper(), new FileSystem()) { }
        public GenericManagementForm(TableConfig config, ILogger<GenericManagementForm>? logger = null, IMessageBox? messageBox = null, IFileSystem? fileSystem = null)
        {
            InitializeComponent();
            _fileSystem = FileSystem ?? new FileSystem();
            _messageBox = messageBox ?? new MessageBoxWrapper();
            _logger = logger ?? NullLogger<GenericManagementForm>.Instance;
            _tableConfig = config;
        }

        private void GenericManagementForm_Load(object sender, EventArgs e)
        {
            SetTheme();
            dgvMain.RowHeadersVisible = false; // Hides Row Number Column
            AdjustDataGridViewHeight();
            ConfigureDataGridViewColumns();
            AddEditDeleteButtons();
            SetSearchOptions();
            FormLoadOccurred?.Invoke(this, e);
        }

        public event EventHandler<SearchRequestEventArgs>? SearchClicked;
        private bool _isCaseSensitive = false;
        public DataGridView DgvMain { get => dgvMain; }
        public DataTable DataSource
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
                _logger.LogError("Search coloumn was not set properly");
            }
        }
        private void btnMatchCase_Click(object sender, EventArgs e)
        {
            _isCaseSensitive = !_isCaseSensitive;
            btnMatchCase.BackColor = _isCaseSensitive ? Color.White : GlobalConstants.SoftBeige;
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

        private static Bitmap ResizeImage(Image Img, int Width, int Height)
        {
            return new Bitmap(Img, new Size(Width, Height));
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
            if (btnFirst.Image != null) btnFirst.Image = ResizeImage(btnFirst.Image, 20, 20);
            if (btnLast.Image != null) btnLast.Image = ResizeImage(btnLast.Image, 20, 20);
        }

        private void AdjustDataGridViewHeight()
        {

            int records = Math.Min(GlobalConstants.s_recordLimit, 30);

            int rowHeight = dgvMain.RowTemplate.Height;

            int requiredHeight = rowHeight * (records + 1) + dgvMain.ColumnHeadersHeight + 2;

            if (dgvMain.Parent != null)
            {
                int formHeightWithoutDataGridView = dgvMain.Parent.Height - dgvMain.Height;
                int newHeight = formHeightWithoutDataGridView + requiredHeight;
                dgvMain.Parent.Height = newHeight;
                _logger.LogInformation("Row Height: {RowHeight}, Column Headers Height: {ColumnHeadersHeight}, New Height: {NewHeight}", rowHeight, dgvMain.ColumnHeadersHeight, newHeight);
            }
            else
            {
                _logger.LogWarning("DataGridView parent is null.");
            }
        }

        public HashSet<string> GetExcludedColumns()
        {
            return [_tableConfig.PrimaryKey];
        }

        public void HideExcludedColumns()
        {
            HashSet<string> excludedColumns = GetExcludedColumns();

            foreach (string columnName in excludedColumns)
            {
                DataGridViewColumn? column = dgvMain.Columns[columnName];
                if (column != null)
                {
                    column.Visible = false;
                }
            }
        }

        private void SetSearchOptions()
        {
            cboSearchOptions.Items.Clear();

            var excludedColumns = GetExcludedColumns().Select(c => c.ToLower()).ToHashSet();
            excludedColumns.Add("edit");
            excludedColumns.Add("delete");

            foreach (DataGridViewColumn column in dgvMain.Columns)
            {
                if (!excludedColumns.Contains(column.Name.ToLower()))
                {
                    cboSearchOptions.Items.Add(column.Name);
                }
            }

            if (cboSearchOptions.Items.Count > 0)
            {
                cboSearchOptions.SelectedIndex = 0;
            }
        }

        public void AddEditDeleteButtons(Func<string, Image>? imageLoader = null)
        {
            imageLoader ??= (path) => Image.FromStream(new MemoryStream(_fileSystem.File.ReadAllBytes(path)));

            string editIconPath = GetIconPath("EditIcon.png");
            string deleteIconPath = GetIconPath("DeleteIcon.png");

            DataGridViewImageColumn editButtonColumn = CreateImageColumn("Edit", editIconPath, imageLoader);
            DataGridViewImageColumn deleteButtonColumn = CreateImageColumn("Delete", deleteIconPath, imageLoader);

            dgvMain.Columns.Add(editButtonColumn);
            dgvMain.Columns.Add(deleteButtonColumn);
        }

        private string GetIconPath(string FileName)
        {
            string path = _fileSystem.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "PresentationLayer", "Images", FileName);
            return _fileSystem.Path.GetFullPath(path);
        }

        private static DataGridViewImageColumn CreateImageColumn(string name, string imagePath, Func<string, Image> imageLoader)
        {
            return new DataGridViewImageColumn
            {
                Name = name,
                HeaderText = "",
                Image = imageLoader(imagePath),
                Width = 30,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };
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

        public event EventHandler? FormLoadOccurred;
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
        public event EventHandler<int>? GoToPageClicked;
        public event EventHandler? PrintClicked;

        private void btnAdd_Click(object sender, EventArgs e) { AddClicked?.Invoke(sender, e); }
        private void btnEdit_Click(int RowIndex) { EditClicked?.Invoke(this, RowIndex); }
        private void btnDelete_Click(int RowIndex) { DeleteClicked?.Invoke(this, RowIndex); }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (DgvMain.DataSource is DataTable dataTable)
            {
                dataTable.DefaultView.RowFilter = string.Empty;
                DgvMain.DataSource = null;
                DgvMain.DataSource = dataTable;
                ConfigureDataGridViewColumns();
                HideExcludedColumns();
                MessageBox.Show("Successfully Refreshed", "Refresh Status");
            }
        }
        private void reloadToolStripMenuItem_Click(object sender, EventArgs e) { ReloadClicked?.Invoke(sender, e); }
        private void rollbackToolStripMenuItem_Click(object sender, EventArgs e) { RollbackClicked?.Invoke(sender, e); }
        private void printAllPagesByRowCountToolStripMenuItem_Click(object sender, EventArgs e) { PrintAllPagesByRowCountClicked?.Invoke(sender, e); }
        private void btnFirst_Click(object sender, EventArgs e) { FirstPageClicked?.Invoke(sender, e); }
        private void btnPrevious_Click(object sender, EventArgs e) { PreviousPageClicked?.Invoke(sender, e); }
        private void btnNext_Click(object sender, EventArgs e) { NextPageClicked?.Invoke(sender, e); }
        private void btnLast_Click(object sender, EventArgs e) { LastPageClicked?.Invoke(sender, e); }
        private void btnGotoPage_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtStartPage.Text, out int pageValue))
            {
                GoToPageClicked?.Invoke(this, pageValue);
            }
            else
            {
                ShowMessageBox("Page number is out of range", "Invalid Number", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrint_Click(object sender, EventArgs e) { PrintClicked?.Invoke(sender, e); }
        private void dgvMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_tableConfig.TableName == "Drivers" && dgvMain.Columns[e.ColumnIndex].Name == "LicenseType" && e.Value != null)
            {
                try
                {
                    e.Value = ((LicenseType)e.Value).ToString();
                    e.FormattingApplied = true;
                }
                catch
                {
                    e.FormattingApplied = true; // Leave invalid values as-is
                }
            }
        }

        public void ShowMessageBox(string Text, string Caption, MessageBoxButtons Buttons, MessageBoxIcon Icon)
        {
            _messageBox.Show(Text, Caption, Buttons, Icon);
        }

        public string StartPageText
        {
            get => txtStartPage.Text;
            set => txtStartPage.Text = value;
        }

        public string EndPageText
        {
            get => lblEndPage.Text;
            set => lblEndPage.Text = value;
        }

        public void UpdatePaginationDisplay(int CurrentPage, int TotalPages)
        {
            StartPageText = $"{CurrentPage}";
            EndPageText = $"/{TotalPages}";
        }

        public void ConfigureDataGridViewColumns()
        {
            dgvMain.Columns.Clear();
            foreach (ColumnConfig column in _tableConfig.Columns)
            {
                DataGridViewTextBoxColumn dgvColumn = new()
                {
                    Name = column.Name,
                    HeaderText = column.Name,
                    DataPropertyName = column.Name
                };
                dgvMain.Columns.Add(dgvColumn);
            }
        }
    }
}
