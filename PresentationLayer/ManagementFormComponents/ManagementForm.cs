using System.Data;
using System.Diagnostics;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.SharedLayer.Interfaces;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;

namespace StartSmartDeliveryForm.PresentationLayer.ManagementFormComponents
{
    public partial class ManagementForm : Form, IManagementForm
    {
        private IFileSystem _fileSystem;
        public IFileSystem FileSystem
        {
            get => _fileSystem;
            set => _fileSystem = value ?? throw new ArgumentNullException(nameof(value)); // Prevent null assignment
        }

        private readonly IMessageBox _messageBox;
        private readonly ILogger<ManagementForm> _logger;
        private TableConfig _tableConfig;

        internal ManagementForm() : this(NullLogger<ManagementForm>.Instance, new MessageBoxWrapper(), new FileSystem())
        {
            Log.Warning("MangementForm's empty constructor was used.");
        }
        public ManagementForm(ILogger<ManagementForm>? logger = null, IMessageBox? messageBox = null, IFileSystem? fileSystem = null)
        {
            InitializeComponent();
            _fileSystem = fileSystem ?? new FileSystem();
            _messageBox = messageBox ?? new MessageBoxWrapper();
            _logger = logger ?? NullLogger<ManagementForm>.Instance;
            _tableConfig = TableConfigs.Empty;
        }

        public bool FirstLoad { get; set; }
        private void ManagementForm_Load(object sender, EventArgs e)
        {
            DisableWIP();
            _logger.LogInformation("ManagementForm loaded, FirstLoad: {FirstLoad}, Table: {TableName}", FirstLoad, _tableConfig.TableName);
            SetTheme();
            dgvMain.RowHeadersVisible = false; // Hides Row Number Column
            AdjustDataGridViewHeight();

            if (FirstLoad == false)
            {
                FormLoadOccurred?.Invoke(this, e);
                FirstLoad = true;
            }
        }

        private void DisableWIP()
        {
            fileToolStripMenuItem.Visible = false;
            dashboardToolStripMenuItem.Visible = false;
            printAllPagesByRowCountToolStripMenuItem.Visible = false;
        }

        public event EventHandler<SearchRequestEventArgs>? SearchClicked;
        private bool _isCaseSensitive = false;
        public DataGridView DgvMain { get => dgvMain; }
        public DataTable DataSource
        {
            get => dgvMain.DataSource as DataTable ?? throw new InvalidOperationException("DataSource is not a DataTable.");
            set
            {
                _logger.LogInformation("Setting DataSource for Table: {TableName}, Columns: {Columns}, Rows: {RowCount}", _tableConfig.TableName, string.Join(", ", value.Columns.Cast<DataColumn>().Select(c => c.ColumnName)), value.Rows.Count);
                dgvMain.DataSource = value;
            }
        }

        public void SetTableConfig(TableConfig config)
        {
            _tableConfig = config ?? throw new ArgumentNullException(nameof(config));
            _logger.LogInformation("TableConfig set, Table: {TableName}, Columns: {Columns}", config.TableName, string.Join(", ", config.Columns.Select(c => c.Name)));
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string? SelectedOption = cboSearchOptions.SelectedItem?.ToString();
            string SearchTerm = txtSearchBox.Text.Trim();

            if (SelectedOption != null)
            {
                _logger.LogInformation("Search initiated, Table: {TableName}, Column: {Column}, Term: {SearchTerm}, CaseSensitive: {IsCaseSensitive}", _tableConfig.TableName, SelectedOption, SearchTerm, _isCaseSensitive);
                SearchClicked?.Invoke(this, new SearchRequestEventArgs((DataTable)dgvMain.DataSource, SelectedOption, SearchTerm, _isCaseSensitive));
            }
            else
            {
                _logger.LogError("Search column not set, Table: {TableName}, SearchTerm: {SearchTerm}", _tableConfig.TableName, SearchTerm);
            }
        }
        private void btnMatchCase_Click(object sender, EventArgs e)
        {
            _isCaseSensitive = !_isCaseSensitive;
            _logger.LogDebug("MatchCase toggled, IsCaseSensitive: {IsCaseSensitive}, Table: {TableName}", _isCaseSensitive, _tableConfig.TableName);
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
            int requiredHeight = rowHeight * (records) + dgvMain.ColumnHeadersHeight;

            if (dgvMain.Parent != null)
            {
                int formHeightWithoutDataGridView = dgvMain.Parent.Height - dgvMain.Height;
                int newHeight = formHeightWithoutDataGridView + requiredHeight;
                dgvMain.Parent.Height = newHeight;
                _logger.LogInformation("Adjusted DataGridView height, Table: {TableName}, RowHeight: {RowHeight}, ColumnHeadersHeight: {ColumnHeadersHeight}, NewHeight: {NewHeight}", _tableConfig.TableName, rowHeight, dgvMain.ColumnHeadersHeight, newHeight);
            }
            else
            {
                _logger.LogWarning("DataGridView parent is null, Table: {TableName}", _tableConfig.TableName);
            }
        }

        public HashSet<string> GetExcludedColumns()
        {
            return [_tableConfig.PrimaryKey];
        }

        public void HideExcludedColumns()
        {
            HashSet<string> excludedColumns = GetExcludedColumns();
            _logger.LogDebug("Hiding excluded columns: {ExcludedColumns}, Table: {TableName}", string.Join(", ", excludedColumns), _tableConfig.TableName);

            foreach (string columnName in excludedColumns)
            {
                DataGridViewColumn? column = dgvMain.Columns[columnName];
                if (column != null)
                {
                    column.Visible = false;
                }
                else
                {
                    _logger.LogWarning("Column {ColumnName} not found in DataGridView, Table: {TableName}", columnName, _tableConfig.TableName);
                }
            }
        }

        public void SetSearchOptions()
        {
            cboSearchOptions.Items.Clear();

            HashSet<string> excludedColumns = GetExcludedColumns().Select(c => c.ToLower()).ToHashSet();
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
                _logger.LogDebug("Set search options, Table: {TableName}, Options: {Options}", _tableConfig.TableName, string.Join(", ", cboSearchOptions.Items.Cast<string>()));
            }
            else
            {
                _logger.LogWarning("No search options available, Table: {TableName}", _tableConfig.TableName);
            }
        }

        private void AddEditDeleteButtons(Func<string, Image>? imageLoader = null)
        {
            imageLoader ??= (path) =>
            {
                using Image original = Image.FromStream(new MemoryStream(_fileSystem.File.ReadAllBytes(path)));
                return new Bitmap(original, new Size(20, 20));
            };

            string editIconPath = GetIconPath("EditIcon.png");
            string deleteIconPath = GetIconPath("DeleteIcon.png");

            DataGridViewImageColumn editButtonColumn = CreateImageColumn("Edit", editIconPath, imageLoader);
            DataGridViewImageColumn deleteButtonColumn = CreateImageColumn("Delete", deleteIconPath, imageLoader);

            dgvMain.Columns.Add(editButtonColumn);
            dgvMain.Columns.Add(deleteButtonColumn);
            _logger.LogDebug("Added Edit and Delete buttons, Table: {TableName}", _tableConfig.TableName);
        }

        private string GetIconPath(string FileName)
        {
            string path = _fileSystem.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "PresentationLayer", "Images", FileName);
            string fullPath = _fileSystem.Path.GetFullPath(path);
            if (!_fileSystem.File.Exists(fullPath))
            {
                _logger.LogError("Icon file not found: {FullPath}, Table: {TableName}", fullPath, _tableConfig.TableName);
            }
            else
            {
                _logger.LogDebug("Resolved icon path: {FullPath}, Table: {TableName}", fullPath, _tableConfig.TableName);
            }
            return fullPath;
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
        public event EventHandler? ReloadClicked;
        public event EventHandler? RefreshedClicked;
        public event EventHandler? RollbackClicked;
        public event EventHandler? PrintAllPagesByRowCountClicked;
        public event EventHandler? FirstPageClicked;
        public event EventHandler? PreviousPageClicked;
        public event EventHandler? NextPageClicked;
        public event EventHandler? LastPageClicked;
        public event EventHandler<int>? GoToPageClicked;
        public event EventHandler? PrintClicked;

        private void btnAdd_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Invoking AddClicked, Table: {TableName}", _tableConfig.TableName);
            AddClicked?.Invoke(sender, e);
        }
        private void btnEdit_Click(int RowIndex)
        {
            _logger.LogDebug("Invoking EditClicked for row {RowIndex}, Table: {TableName}", RowIndex, _tableConfig.TableName);
            EditClicked?.Invoke(this, RowIndex);
        }
        private void btnDelete_Click(int RowIndex)
        {
            _logger.LogDebug("Invoking DeleteClicked for row {RowIndex}, Table: {TableName}", RowIndex, _tableConfig.TableName);
            if (DeleteClicked == null)
            {
                _logger.LogWarning("DeleteClicked has no subscribers for row {RowIndex}, Table: {TableName}", RowIndex, _tableConfig.TableName);
            }
            DeleteClicked?.Invoke(this, RowIndex);
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Invoking RefreshedClicked, Table: {TableName}", _tableConfig.TableName);
            RefreshedClicked?.Invoke(sender, e);
        }
        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Invoking ReloadClicked, Table: {TableName}", _tableConfig.TableName);
            ReloadClicked?.Invoke(sender, e);
        }
        private void rollbackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Invoking RollbackClicked, Table: {TableName}", _tableConfig.TableName);
            RollbackClicked?.Invoke(sender, e);
        }
        private void printAllPagesByRowCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Invoking PrintAllPagesByRowCountClicked, Table: {TableName}", _tableConfig.TableName);
            PrintAllPagesByRowCountClicked?.Invoke(sender, e);
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Invoking FirstPageClicked, Table: {TableName}", _tableConfig.TableName);
            FirstPageClicked?.Invoke(sender, e);
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Invoking PreviousPageClicked, Table: {TableName}", _tableConfig.TableName);
            PreviousPageClicked?.Invoke(sender, e);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Invoking NextPageClicked, Table: {TableName}", _tableConfig.TableName);
            NextPageClicked?.Invoke(sender, e);
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Invoking LastPageClicked, Table: {TableName}", _tableConfig.TableName);
            LastPageClicked?.Invoke(sender, e);
        }
        private void btnGotoPage_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtStartPage.Text, out int pageValue))
            {
                _logger.LogDebug("Invoking GoToPageClicked for page {PageValue}, Table: {TableName}", pageValue, _tableConfig.TableName);
                GoToPageClicked?.Invoke(this, pageValue);
            }
            else
            {
                _logger.LogError("Invalid page number entered: {InputText}, Table: {TableName}", txtStartPage.Text, _tableConfig.TableName);
                ShowMessageBox("Page number is out of range", "Invalid Number", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Invoking PrintClicked, Table: {TableName}", _tableConfig.TableName);
            PrintClicked?.Invoke(sender, e);
        }
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
                    _logger.LogWarning("Failed to format LicenseType for row {RowIndex}, column {ColumnName}", e.RowIndex, dgvMain.Columns[e.ColumnIndex].Name);
                    e.FormattingApplied = true; // Leave invalid values as-is
                }
            }
        }

        public void ShowMessageBox(string Text, string Caption, MessageBoxButtons Buttons, MessageBoxIcon Icon)
        {
            _logger.LogInformation("Showing MessageBox, Table: {TableName}, Caption: {Caption}, Text: {Text}, Icon: {Icon}", _tableConfig.TableName, Caption, Text, Icon);
            _messageBox.Show(Text, Caption, Buttons, Icon);
        }

        public string StartPageText
        {
            get => txtStartPage.Text;
            set
            {
                txtStartPage.Text = value;
                _logger.LogDebug("Updated StartPageText to {Value}, Table: {TableName}", value, _tableConfig.TableName);
            }
        }

        public string EndPageText
        {
            get => lblEndPage.Text;
            set
            {
                lblEndPage.Text = value;
                _logger.LogDebug("Updated EndPageText to {Value}, Table: {TableName}", value, _tableConfig.TableName);
            }
        }

        public void UpdatePaginationDisplay(int CurrentPage, int TotalPages)
        {
            StartPageText = $"{CurrentPage}";
            EndPageText = $"/{TotalPages}";

            _logger.LogInformation("Updated pagination display, Table: {TableName}, CurrentPage: {CurrentPage}, TotalPages: {TotalPages}", _tableConfig.TableName, CurrentPage, TotalPages);
        }

        public void ConfigureDataGridViewColumns(Func<string, Image>? imageLoader = null)
        {
            _logger.LogDebug("Configuring DataGridView columns for Table: {TableName}", _tableConfig.TableName);
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

            if (dgvMain.Rows.Count > 0)
            {
                int initialRowHeight = dgvMain.Rows[0].Height;
                AddEditDeleteButtons(imageLoader);
                int currentRowHeight = dgvMain.Rows[0].Height;

                if (initialRowHeight != currentRowHeight)
                {
                    _logger.LogError("Row height differs after adding edit and delete buttons. Initial: {InitialRowHeight}, Current: {CurrentRowHeight}. Ensure images are being scaled properly.", initialRowHeight, currentRowHeight);
                }
            }
            else
            {
                _logger.LogDebug("No rows in DataGridView; skipping row height check.");
                AddEditDeleteButtons(imageLoader);
            }

            _logger.LogInformation("Configured DataGridView columns for Table: {TableName}", _tableConfig.TableName);
        }

        public event EventHandler? DashboardFormRequested;
        public event EventHandler? DeliveryManagementFormRequested;
        public event EventHandler? VehicleManagementFormRequested;
        public event EventHandler? DriverManagementFormRequested;

        private void dashboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Dashboard ToolStripMenuItem invoked, Table: {TableName}", _tableConfig.TableName);
            DashboardFormRequested?.Invoke(sender, e);
        }

        private void deliveryManagementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Delivery Management ToolStripMenuItem invoked, Table: {TableName}", _tableConfig.TableName);
            DeliveryManagementFormRequested?.Invoke(sender, e);
        }

        public void vehicleManagementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Vehicle Management ToolStripMenuItem invoked, Table: {TableName}", _tableConfig.TableName);
            VehicleManagementFormRequested?.Invoke(sender, e);
        }

        private void driverManagementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Driver Management ToolStripMenuItem invoked, Table: {TableName}", _tableConfig.TableName);
            DriverManagementFormRequested?.Invoke(sender, e);
        }

        public event EventHandler? ChangeUserRequested;

        private void changeUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.LogInformation("Change User ToolStripMenuItem invoked");
            ChangeUserRequested?.Invoke(sender, e);
        }

        public void InvokeFormLoadOccurred(object? sender, EventArgs e)
        {
            _logger.LogDebug("Invoking FormLoadOccurred, Table: {TableName}", _tableConfig.TableName);
            FormLoadOccurred?.Invoke(sender, e);
        }
    }
}
