using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement
{
    public partial class DriverManagementForm : ManagementTemplateForm
    {
        private readonly DriversDAO _driversDAO;
        private DataTable _driverData;
        private readonly PaginationManager _paginationManager;
        private readonly ILogger<DriverManagementForm> _logger;
        private CancellationTokenSource? _cts;
        private readonly RetryEventService _retryEventService;
        public DriverManagementForm(DriversDAO driversDAO, ILogger<DriverManagementForm>? logger = null, RetryEventService? retryEventService = null)
        {
            InitializeComponent();
            _driversDAO = driversDAO;
            _driverData = new DataTable(); //Empty table by default
            _logger = logger ?? NullLogger<DriverManagementForm>.Instance;

            _retryEventService = retryEventService ?? new RetryEventService(); // New RetryEvent should never display anything
            _retryEventService.RetryOccurred += OnRetryOccurred;
            _retryEventService.RetrySucceeded += OnRetrySuccessOccurred;

            _paginationManager = new PaginationManager("Drivers", driversDAO);
            _paginationManager.PageChanged += OnPageChanged;

        }

        private async Task InitializeAsync()
        {
            try
            {
                await _paginationManager.InitializeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Fatal error during initialization. Pagination will not function - Error: {Error}", ex);
                MessageBox.Show("Fatal error during initialization. Pagination will not function", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnRetrySuccessOccurred() { MessageBox.Show("Success"); }
        private void OnRetryOccurred(int attemptNumber, int maxRetries, TimeSpan retryDelay, string exceptionMessage)
        {
            //TODO: Will replace this with a custom form for live updates since multiple MessageBoxes stack, which is a bad user experience
            MessageBox.Show($"Retry attempt {attemptNumber}/{maxRetries}. Retrying in {retryDelay.Seconds} seconds. Exception: {exceptionMessage}");
        }

        public async Task OnPageChanged(int currentPage)
        {
            _cts = new CancellationTokenSource();

            try
            {
                _driverData = await _driversDAO.GetDriversAtPageAsync(currentPage, _cts.Token) ?? new DataTable();
                dgvMain.DataSource = _driverData;
                txtStartPage.Text = $"{_paginationManager.CurrentPage}";
                lblEndPage.Text = $"/{_paginationManager.TotalPages}";
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("The operation was canceled.");
            }

        }

        private async void DriverManagementForm_Load(object sender, EventArgs e)
        {
            AdjustDataGridViewHeight(dgvMain);
            SetSearchOptions(typeof(DriversDTO));
            await DriverManagementForm_LoadAsync(sender, e);
        }

        private async Task DriverManagementForm_LoadAsync(object sender, EventArgs e)
        {
            await InitializeAsync();
            await _paginationManager.GoToFirstPage();

            if (_driverData == null || _driverData.Rows.Count == 0)
            {
                MessageBox.Show("Failed to load driver data. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                dgvMain.DataSource = _driverData;
                AddEditDeleteButtons();

                // Hide the Primary Key Column from User
                DataGridViewColumn? driverColumn = dgvMain.Columns[DriverColumns.DriverID];
                if (driverColumn != null)
                {
                    driverColumn.Visible = false;
                }
            }
        }

        protected override HashSet<string> GetExcludedColumns()
        {
            return [DriverColumns.DriverID]; // By default, exclude nothing.
        }

        protected override void btnAdd_Click(object sender, EventArgs e)
        {
            DriverDataForm driverDataForm = new(_driversDAO);
            driverDataForm.SubmitClicked += DriverDataForm_SubmitClicked;
            driverDataForm.Show();
        }

        protected override void btnEdit_Click(int rowIndex)
        {
            DataGridViewRow selectedRow = dgvMain.Rows[rowIndex];

            object? DriverID = selectedRow.Cells[DriverColumns.DriverID].Value;
            object? Name = selectedRow.Cells[DriverColumns.Name].Value;
            object? Surname = selectedRow.Cells[DriverColumns.Surname].Value;
            object? EmployeeNo = selectedRow.Cells[DriverColumns.EmployeeNo].Value;
            object? LicenseType = selectedRow.Cells[DriverColumns.LicenseType].Value;
            object? Availability = selectedRow.Cells[DriverColumns.Availability].Value;

            if (DriverID != null &&
                Name != null &&
                Surname != null &&
                EmployeeNo != null &&
                LicenseType != null &&
                Availability != null &&
                int.TryParse(DriverID.ToString(), out int driverID) &&
                Enum.TryParse(LicenseType.ToString(), out LicenseType license) &&
                bool.TryParse(Availability.ToString(), out bool availability))
            {
                DriversDTO driverData = new(
                    driverID,
                    Name.ToString()!,
                    Surname.ToString()!,
                    EmployeeNo.ToString()!,
                    license,
                    availability
                );

                DriverDataForm driverDataForm = new(_driversDAO)
                {
                    Mode = FormMode.Edit
                };

                driverDataForm.InitializeEditing(driverData);
                driverDataForm.SubmitClicked += DriverDataForm_SubmitClicked;
                driverDataForm.Show();
            }
            else
            {
                MessageBox.Show("An error occurred. Edit form could not be initialized due to invalid data.");
            }
        }

        protected override async Task btnDelete_ClickAsync(int rowIndex)
        {
            _cts = new CancellationTokenSource();

            try
            {
                DataGridViewRow selectedRow = dgvMain.Rows[rowIndex];

                object? DriverID = selectedRow.Cells[DriverColumns.DriverID].Value;

                DialogResult result = MessageBox.Show("Are you sure?", "Delete Row", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes && int.TryParse(DriverID?.ToString(), out int driverID))
                {
                    _driverData.Rows.RemoveAt(rowIndex);
                    await _driversDAO.DeleteDriverAsync(driverID,_cts.Token);

                    _paginationManager.UpdateRecordCount(_paginationManager.RecordCount - 1);
                    await _paginationManager.EnsureValidPage();
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("The operation was canceled.");
            }
           
        }

        // DriverDataForm submit button event handlers
        private async void DriverDataForm_SubmitClicked(object sender, EventArgs e) { await DriverDataForm_SubmitClickedAsync(sender, e); }
        private async Task DriverDataForm_SubmitClickedAsync(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();

            try
            {
                if (sender is DriverDataForm form)
                {
                    DriversDTO driverDTO = form.GetData();

                    if (driverDTO != null)
                    {
                        if (form.Mode == FormMode.Add)
                        {
                            int newDriverId = await _driversDAO.InsertDriverAsync(driverDTO, _cts.Token);
                            _logger.LogInformation("NewDriverID: {newDriverID}", newDriverId);
                            if (newDriverId != -1) // Check for success
                            {
                                DataRow newRow = _driverData.NewRow();
                                PopulateDataRow(newRow, driverDTO);
                                newRow[DriverColumns.DriverID] = newDriverId;

                                _driverData.Rows.Add(newRow);
                                _paginationManager.UpdateRecordCount(_paginationManager.RecordCount + 1);
                                await _paginationManager.GoToLastPage(); // Allows user to see successful insert
                            }
                        }
                        else if (form.Mode == FormMode.Edit)
                        {
                            DataRow? rowToUpdate = _driverData.Rows.Find(driverDTO.DriverID); // Assuming EmployeeNo is the primary key
                            if (rowToUpdate == null)
                            {
                                FormConsole.Instance.Log("Row not found for update.");
                                return;
                            }
                            PopulateDataRow(rowToUpdate, driverDTO);

                            await _driversDAO.UpdateDriverAsync(driverDTO, _cts.Token);
                            form.Close();
                        }
                    }

                    form.ClearData(); //Clear form for next batch of data
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("The operation was canceled.");
            }
        }

        private static void PopulateDataRow(DataRow row, DriversDTO driverDTO)
        {
            row[DriverColumns.Name] = driverDTO.Name;
            row[DriverColumns.Surname] = driverDTO.Surname;
            row[DriverColumns.EmployeeNo] = driverDTO.EmployeeNo;
            row[DriverColumns.LicenseType] = driverDTO.LicenseType;
            row[DriverColumns.Availability] = driverDTO.Availability;
        }

        private void SetDataGridViewColumns()
        {
            DataGridViewColumn? nameCol = dgvMain.Columns[DriverColumns.Name];
            DataGridViewColumn? surnameCol = dgvMain.Columns[DriverColumns.Surname];
            DataGridViewColumn? employeeNoCol = dgvMain.Columns[DriverColumns.EmployeeNo];
            DataGridViewColumn? licenseTypeCol = dgvMain.Columns[DriverColumns.LicenseType];
            DataGridViewColumn? availabilityCol = dgvMain.Columns[DriverColumns.Availability];
            DataGridViewColumn? editCol = dgvMain.Columns[DriverColumns.Edit];
            DataGridViewColumn? deleteCol = dgvMain.Columns[DriverColumns.Delete];
            DataGridViewColumn? driverIDCol = dgvMain.Columns[DriverColumns.DriverID];

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
            DataTable? dataTable = null;
            if (dgvMain.DataSource is DataTable dt)
            {
                dataTable = dt;
            }

            // Remove any filters that were applied
            if (dataTable != null)
            {
                dataTable.DefaultView.RowFilter = string.Empty;  // Clear any applied filters
            }

            // Rebind
            dgvMain.DataSource = null;
            dgvMain.DataSource = _driverData;
            SetDataGridViewColumns();

            MessageBox.Show("Succesfully Refreshed", "Refresh Status");
        }

        protected override async Task reloadToolStripMenuItem_ClickAsync(object sender, EventArgs e)
        {
            //Refetch data and rebind
            _cts = new CancellationTokenSource();

            try
            {
                _driverData = await _driversDAO.GetAllDriversAsync(_cts.Token) ?? new DataTable();
                dgvMain.DataSource = null;
                dgvMain.DataSource = _driverData;
                SetDataGridViewColumns();

                MessageBox.Show("Succesfully Reloaded", "Reload Status");
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("The operation was canceled.");
            }
        }

        protected override void rollbackToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        protected override void dgvMain_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvMain.Columns[e.ColumnIndex].Name == DriverColumns.LicenseType && e.Value != null)
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

        protected override async Task btnFirst_ClickAsync(object sender, EventArgs e)
        {
            await _paginationManager.GoToFirstPage();
        }

        protected override async Task btnPrevious_ClickAsync(object sender, EventArgs e)
        {
            await _paginationManager.GoToPreviousPage();
        }

        protected override async Task btnNext_ClickAsync(object sender, EventArgs e)
        {
            await _paginationManager.GoToNextPage();
        }

        protected override async Task btnLast_ClickAsync(object sender, EventArgs e)
        {
            await _paginationManager.GoToLastPage();
        }

        protected override async Task btnGotoPage_ClickAsync(object sender, EventArgs e)
        {
            bool ParsedGoto = int.TryParse(txtStartPage.Text, out int GotoPage);
            if (ParsedGoto)
            {

                if (GotoPage == _paginationManager.CurrentPage) return;

                if (GotoPage >= 1 && GotoPage <= _paginationManager.TotalPages)
                {
                    await _paginationManager.GoToPage(GotoPage);
                }
                else
                {
                    MessageBox.Show("Page number is out of range", "Invalid Number");
                    return;
                }
            }
        }

        protected override void btnPrint_Click(object sender, EventArgs e)
        {
            PrintDriverDataForm preview = new(dgvMain, _driversDAO); // Prints only 1 page. 

            //Unlike Show, it blocks execution on main form till complete
            preview.ShowDialog();
        }
    }
}
