using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.TemplateModels;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventDelegates;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement.Models
{
    internal class DriverManagementModel : ManagementModel, IDriverManagementModel
    {
        private readonly DriversDAO _driversDAO;
        protected readonly PaginationManager _paginationManager;
        private readonly ILogger<DriverManagementModel> _logger;
        private CancellationTokenSource? _cts;
        public PaginationManager PaginationManager { get => _paginationManager; }
        public DriverManagementModel(DriversDAO driversDAO, PaginationManager paginationManager, ILogger<DriverManagementModel>? logger = null)
        {
            _driversDAO = driversDAO;
            _paginationManager = paginationManager;
            _logger = logger ?? NullLogger<DriverManagementModel>.Instance;

            _paginationManager.PageChanged += OnPageChanged;
        }

        public override async Task InitializeAsync()
        {
            try
            {
                await _paginationManager.InitializeAsync();
                await _paginationManager.GoToFirstPage();
                _logger.LogInformation("CurrentPage: {CurrentPage}, TotalPages: {TotalPages}", PaginationManager.CurrentPage, PaginationManager.TotalPages);
            }
            catch (Exception ex)
            {
                _logger.LogError("Fatal error during initialization. Pagination will not function - Error: {Error}", ex);
                MessageBox.Show("Fatal error during initialization. Pagination will not function", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public event EventHandler? PageChanged;
        public async Task OnPageChanged(int CurrentPage)
        {
            _cts = new CancellationTokenSource();

            try
            {
                _dgvTable = await _driversDAO.GetDriversAtPageAsync(CurrentPage, _cts.Token) ?? new DataTable();
                PageChanged?.Invoke(this, new EventArgs());
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("The operation was canceled.");
            }
        }

        public new event MessageBoxEventDelegate? DisplayErrorMessage;
        public async Task AddDriverAsync(DriversDTO Driver)
        {
            _cts = new CancellationTokenSource();

            int newDriverId = await _driversDAO.InsertDriverAsync(Driver, _cts.Token);
            if (newDriverId != -1) // Check for success
            {
                DataRow newRow = _dgvTable.NewRow();
                PopulateDataRow(newRow, Driver);
                newRow[DriverColumns.DriverID] = newDriverId;
                _dgvTable.Rows.Add(newRow);
                _paginationManager.UpdateRecordCount(_paginationManager.RecordCount + 1);
                await _paginationManager.GoToLastPage(); // Allows user to see successful insert
            }
        }

        public async Task UpdateDriverAsync(DriversDTO Driver)
        {
            _cts = new CancellationTokenSource();

            DataRow? rowToUpdate = _dgvTable.Rows.Find(Driver.DriverID);
            if (rowToUpdate == null)
            {
                _logger.LogWarning("Driver with ID {DriverID} was not found for update.", Driver.DriverID);
                return;
            }

            bool success = await _driversDAO.UpdateDriverAsync(Driver, _cts.Token);
            if (success)
            {
                _logger.LogInformation("Driver with ID {DriverId} was updated successfully.", Driver.DriverID);
            }
            else
            {
                DisplayErrorMessage?.Invoke("Failed to update driver in database", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            PopulateDataRow(rowToUpdate, Driver);
        }

        public async Task DeleteDriverAsync(int DriverID)
        {
            _cts = new CancellationTokenSource();

            DataRow? rowToDelete = _dgvTable.Rows.Find(DriverID);
            if (rowToDelete == null)
            {
                _logger.LogWarning("Driver with ID {DriverID} was not found for delete.", DriverID);
                return;
            }

            bool success = await _driversDAO.DeleteDriverAsync(DriverID, _cts.Token);
            if (success)
            {
                _paginationManager.UpdateRecordCount(_paginationManager.RecordCount - 1);
                await _paginationManager.EnsureValidPage();
                _logger.LogInformation("Driver with ID {DriverId} deleted successfully.", DriverID);
            }
            else
            {
                DisplayErrorMessage?.Invoke("Failed to delete driver from database", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _dgvTable.Rows.Remove(rowToDelete);
        }

        public void CancelOperations()
        {
            _cts?.Cancel();
        }

        private void PopulateDataRow(DataRow Row, DriversDTO DriverDTO)
        {
            Row[DriverColumns.Name] = DriverDTO.Name;
            Row[DriverColumns.Surname] = DriverDTO.Surname;
            Row[DriverColumns.EmployeeNo] = DriverDTO.EmployeeNo;
            Row[DriverColumns.LicenseType] = DriverDTO.LicenseType;
            Row[DriverColumns.Availability] = DriverDTO.Availability;
        }

        public DriversDTO GetDriverFromRow(DataGridViewRow selectedRow)
        {
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
                return new DriversDTO(
                    driverID,
                    Name.ToString()!,
                    Surname.ToString()!,
                    EmployeeNo.ToString()!,
                    license,
                    availability
                );
            };

            return DriversDTO.Empty;
        }

        public async Task FetchAndBindDriversAtPage()
        {
            _cts = new CancellationTokenSource();
            _dgvTable = await _driversDAO.GetDriversAtPageAsync(PaginationManager.CurrentPage, _cts.Token) ?? new DataTable();
        }
    }
}
