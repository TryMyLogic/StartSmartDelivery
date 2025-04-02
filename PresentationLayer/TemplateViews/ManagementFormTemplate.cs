using System.Data;
using System.IO.Abstractions;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.PresentationLayer
{
    public partial class ManagementFormTemplate : Form, IManagementForm
    {
        private IFileSystem _fileSystem;
        public IFileSystem FileSystem
        {
            get => _fileSystem;
            set => _fileSystem = value ?? throw new ArgumentNullException(nameof(value)); // Prevent null assignment
        }

        private readonly IMessageBox _messageBox;
        private readonly ILogger<ManagementFormTemplate> _logger;

        public ManagementFormTemplate() : this(NullLogger<ManagementFormTemplate>.Instance, new MessageBoxWrapper(), new FileSystem()) { }
        public ManagementFormTemplate(ILogger<ManagementFormTemplate>? logger = null, IMessageBox? messageBox = null, IFileSystem? FileSystem = null)
        {
            InitializeComponent();
            _fileSystem = FileSystem ?? new FileSystem();
            _messageBox = messageBox ?? new MessageBoxWrapper();
            _logger = logger ?? NullLogger<ManagementFormTemplate>.Instance;
        }

        private void ManagementTemplateForm_Load(object sender, EventArgs e)
        {
            SetTheme();
            dgvMain.RowHeadersVisible = false; // Hides Row Number Column
        }

        public event EventHandler? FormLoadOccurred;
        protected virtual void OnLoad()
        {
            FormLoadOccurred?.Invoke(this, EventArgs.Empty);
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

        //DO NOT use this in the ManagementTemplateForm_Load. It interferes with children initialization, breaking the child designer. 
        protected void AdjustDataGridViewHeight(DataGridView DataGridView)
        {
            int records = Math.Min(GlobalConstants.s_recordLimit, 30);

            int rowHeight = DataGridView.RowTemplate.Height;

            int requiredHeight = rowHeight * (records + 1) + DataGridView.ColumnHeadersHeight + 2;

            if (DataGridView.Parent != null)
            {
                int formHeightWithoutDataGridView = DataGridView.Parent.Height - DataGridView.Height;
                int newHeight = formHeightWithoutDataGridView + requiredHeight;
                DataGridView.Parent.Height = newHeight;
                _logger.LogInformation("Row Height: {RowHeight}, Column Headers Height: {ColumnHeadersHeight}, New Height: {NewHeight}", rowHeight,DataGridView.ColumnHeadersHeight, newHeight);
            }
            else
            {
                _logger.LogWarning("DataGridView parent is null.");
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
            foreach (PropertyInfo property in Dto.GetProperties())
            {
                if (!ExcludedColumns.Contains(property.Name.ToLower()))
                    cboSearchOptions.Items.Add(property.Name);
            }

            cboSearchOptions.SelectedIndex = 0;
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

        //Required by Children:
        public event EventHandler? AddClicked;
        public event EventHandler<int>? EditClicked;
        public event EventHandler<int>? DeleteClicked;
        public event EventHandler? ReloadClicked;
        public event EventHandler? RollbackClicked;
        public event EventHandler? PrintAllPagesByRowCountClicked;
        public event EventHandler? FirstPageClicked;
        public event EventHandler? PreviousPageClicked;
        public event EventHandler? NextPageClicked;
        public event EventHandler? LastPageClicked;
        public event EventHandler<int>? GoToPageClicked;
        public event EventHandler? PrintClicked;

        protected virtual void btnAdd_Click(object sender, EventArgs e) { AddClicked?.Invoke(sender, e); }
        protected virtual void btnEdit_Click(int RowIndex) { EditClicked?.Invoke(this, RowIndex); }
        protected virtual void btnDelete_Click(int RowIndex) { DeleteClicked?.Invoke(this, RowIndex); }
        protected virtual void btnRefresh_Click(object sender, EventArgs e) { }
        protected virtual void reloadToolStripMenuItem_Click(object sender, EventArgs e) { ReloadClicked?.Invoke(sender, e); }
        protected virtual void rollbackToolStripMenuItem_Click(object sender, EventArgs e) { RollbackClicked?.Invoke(sender, e); }
        protected virtual void printAllPagesByRowCountToolStripMenuItem_Click(object sender, EventArgs e) { PrintAllPagesByRowCountClicked?.Invoke(sender, e); }
        protected virtual void btnFirst_Click(object sender, EventArgs e) { FirstPageClicked?.Invoke(sender, e); }
        protected virtual void btnPrevious_Click(object sender, EventArgs e) { PreviousPageClicked?.Invoke(sender, e); }
        protected virtual void btnNext_Click(object sender, EventArgs e) { NextPageClicked?.Invoke(sender, e); }
        protected virtual void btnLast_Click(object sender, EventArgs e) { LastPageClicked?.Invoke(sender, e); }
        protected virtual void btnGotoPage_Click(object sender, EventArgs e) { GoToPageClicked?.Invoke(this, GoToPageValue()); }
        protected virtual int GoToPageValue() { return 1; }
        protected virtual void btnPrint_Click(object sender, EventArgs e) { PrintClicked?.Invoke(sender, e); }
        protected virtual void dgvMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) { }
        public void ShowMessageBox(string Text, string Caption, MessageBoxButtons Buttons, MessageBoxIcon Icon)
        {
            _messageBox.Show(Text, Caption, Buttons, Icon);
        }
    }
}
