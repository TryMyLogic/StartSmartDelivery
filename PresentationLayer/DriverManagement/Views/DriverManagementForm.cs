using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.DataLayer;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Views;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement
{
    public partial class DriverManagementForm : ManagementFormTemplate, IDriverManagementForm
    {
        private readonly ILogger<DriverManagementForm> _logger;
        private readonly RetryEventService _retryEventService;

        public DriverManagementForm(ILogger<DriverManagementForm>? logger = null, RetryEventService? retryEventService = null)
        {
            InitializeComponent();
            _logger = logger ?? NullLogger<DriverManagementForm>.Instance;

            _retryEventService = retryEventService ?? new RetryEventService(); // New RetryEvent should never display anything
            _retryEventService.RetryOccurred += OnRetryOccurred;
            _retryEventService.RetrySucceeded += OnRetrySuccessOccurred;
        }

        private void DriverManagementForm_Load(object? sender, EventArgs e)
        {
            AdjustDataGridViewHeight(DgvMain);
            SetSearchOptions(typeof(DriversDTO));
            OnLoad();
        }

        protected override void OnLoad() => base.OnLoad();

        private void OnRetrySuccessOccurred() { MessageBox.Show("Success"); }
        private void OnRetryOccurred(int AttemptNumber, int MaxRetries, TimeSpan RetryDelay, string ExceptionMessage)
        {
            //TODO: Will replace this with a custom form for live updates since multiple MessageBoxes stack, which is a bad user experience
            MessageBox.Show($"Retry attempt {AttemptNumber}/{MaxRetries}. Retrying in {RetryDelay.Seconds} seconds. Exception: {ExceptionMessage}");
        }

        public void UpdatePaginationDisplay(int CurrentPage, int TotalPages)
        {
            txtStartPage.Text = $"{CurrentPage}";
            lblEndPage.Text = $"/{TotalPages}";
        }

        protected override HashSet<string> GetExcludedColumns()
        {
            return [DriverColumns.DriverID]; // By default, exclude nothing.
        }

        protected override void btnAdd_Click(object sender, EventArgs e)
        {
            _logger.LogInformation("btnAdd Clicked");
            base.btnAdd_Click(sender, e);
        }

        protected override void btnEdit_Click(int RowIndex)
        {
            _logger.LogInformation("btnEdit Clicked");
            base.btnEdit_Click(RowIndex);
        }

        protected override void btnDelete_Click(int RowIndex)
        {
            _logger.LogInformation("btnEdit Clicked");
            base.btnDelete_Click(RowIndex);
        }

        public void SetDataGridViewColumns()
        {
            DataGridViewColumn? nameCol = DgvMain.Columns[DriverColumns.Name];
            DataGridViewColumn? surnameCol = DgvMain.Columns[DriverColumns.Surname];
            DataGridViewColumn? employeeNoCol = DgvMain.Columns[DriverColumns.EmployeeNo];
            DataGridViewColumn? licenseTypeCol = DgvMain.Columns[DriverColumns.LicenseType];
            DataGridViewColumn? availabilityCol = DgvMain.Columns[DriverColumns.Availability];
            DataGridViewColumn? editCol = DgvMain.Columns[DriverColumns.Edit];
            DataGridViewColumn? deleteCol = DgvMain.Columns[DriverColumns.Delete];
            DataGridViewColumn? driverIDCol = DgvMain.Columns[DriverColumns.DriverID];

            if (nameCol != null && surnameCol != null && employeeNoCol != null &&
                licenseTypeCol != null && availabilityCol != null &&
                editCol != null && deleteCol != null && driverIDCol != null)
            {
                nameCol.DisplayIndex = 0;
                surnameCol.DisplayIndex = 1;
                employeeNoCol.DisplayIndex = 2;
                licenseTypeCol.DisplayIndex = 3;
                availabilityCol.DisplayIndex = 4;
                editCol.DisplayIndex = 5;
                deleteCol.DisplayIndex = 6;
                driverIDCol.Visible = false;
            }
        }

        protected override void btnRefresh_Click(object sender, EventArgs e)
        {
            if (DgvMain.DataSource is DataTable dataTable)
            {
                dataTable.DefaultView.RowFilter = string.Empty;
                DgvMain.DataSource = null;
                DgvMain.DataSource = dataTable;
                SetDataGridViewColumns();
                MessageBox.Show("Successfully Refreshed", "Refresh Status");
            }
        }

        protected override void reloadToolStripMenuItem_Click(object sender, EventArgs e) { base.reloadToolStripMenuItem_Click(sender, e); }

        protected override void rollbackToolStripMenuItem_Click(object sender, EventArgs e) { base.rollbackToolStripMenuItem_Click(sender, e); }

        protected override void dgvMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (DgvMain.Columns[e.ColumnIndex].Name == DriverColumns.LicenseType && e.Value != null)
            {
                try
                {
                    e.Value = ((LicenseType)e.Value).ToString();
                    e.FormattingApplied = true;
                }
                catch
                {
                    // Invalid Values are left as is (integers)
                    e.FormattingApplied = true;
                }
            }
        }

        protected override void btnFirst_Click(object sender, EventArgs e) => base.btnFirst_Click(sender, e);
        protected override void btnPrevious_Click(object sender, EventArgs e) => base.btnPrevious_Click(sender, e);
        protected override void btnNext_Click(object sender, EventArgs e) => base.btnNext_Click(sender, e);
        protected override void btnLast_Click(object sender, EventArgs e) => base.btnLast_Click(sender, e);

        private int _gotoPageValue = 1;
        protected override void btnGotoPage_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtStartPage.Text, out _gotoPageValue)) // Use the existing field
            {
                base.btnGotoPage_Click(sender, e);
            }
            else
            {
                MessageBox.Show("Page number is out of range", "Invalid Number", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        protected override int GoToPageValue() { return _gotoPageValue; }

        protected override void btnPrint_Click(object sender, EventArgs e) { base.btnPrint_Click(sender, e); }
    }
}
