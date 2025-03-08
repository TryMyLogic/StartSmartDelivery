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
            catch (InvalidOperationException ex)
            {
                _logger.LogError("Fatal error during initialization. Pagination will not function - Error: {Error}", ex);
                DisplayErrorMessage?.Invoke("Fatal error during initialization. Pagination will not function.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public event EventHandler? PageChanged;
        public async Task OnPageChanged(int CurrentPage)
        {
            _dgvTable = await _driversDAO.GetDriversAtPageAsync(CurrentPage) ?? new DataTable();
            PageChanged?.Invoke(this, new EventArgs());
        }

        public new event MessageBoxEventDelegate? DisplayErrorMessage;
        public async Task AddDriverAsync(DriversDTO Driver)
        {
            int newDriverId = await _driversDAO.InsertDriverAsync(Driver);
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
            DataRow? rowToUpdate = _dgvTable.Rows.Find(Driver.DriverID);
            if (rowToUpdate == null)
            {
                _logger.LogWarning("Driver with ID {DriverID} was not found for update.", Driver.DriverID);
                return;
            }

            bool success = await _driversDAO.UpdateDriverAsync(Driver);
            if (success)
            {
                PopulateDataRow(rowToUpdate, Driver);
                _logger.LogInformation("Driver with ID {DriverId} was updated successfully.", Driver.DriverID);
            }
            else
            {
                DisplayErrorMessage?.Invoke("Failed to update driver in database", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        public async Task DeleteDriverAsync(int DriverID)
        {
            DataRow? rowToDelete = _dgvTable.Rows.Find(DriverID);
            if (rowToDelete == null)
            {
                _logger.LogWarning("Driver with ID {DriverID} was not found for delete.", DriverID);
                return;
            }

            bool success = await _driversDAO.DeleteDriverAsync(DriverID);
            if (success)
            {
                _dgvTable.Rows.Remove(rowToDelete);
                _paginationManager.UpdateRecordCount(_paginationManager.RecordCount - 1);
                await _paginationManager.EnsureValidPage();
                _logger.LogInformation("Driver with ID {DriverId} deleted successfully.", DriverID);
            }
            else
            {
                DisplayErrorMessage?.Invoke("Failed to delete driver from database", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private static void PopulateDataRow(DataRow Row, DriversDTO DriverDTO)
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
            _dgvTable = await _driversDAO.GetDriversAtPageAsync(PaginationManager.CurrentPage) ?? new DataTable();
        }
    }
}
