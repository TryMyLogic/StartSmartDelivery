using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.Repositories;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.SharedLayer.EventDelegates;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;

namespace StartSmartDeliveryForm.PresentationLayer.ManagementFormComponents
{
    public class ManagementModel<T> : IDisposable, IManagementModel<T> where T : class
    {
        private readonly ILogger<ManagementModel<T>> _logger;
        private readonly IRepository<T> _repository;
        private readonly TableConfig _tableConfig;
        private bool _disposedValue;

        public DataTable DgvTable { get; private set; }
        public PaginationManager<T> PaginationManager { get; }

        public event MessageBoxEventDelegate? DisplayErrorMessage;
        protected void InvokeDisplayErrorMessage(string text, string caption, MessageBoxButtons button, MessageBoxIcon icon)
        {
            _logger.LogDebug("Invoking DisplayErrorMessage with text: {Text}, caption: {Caption}", text, caption);
            DisplayErrorMessage?.Invoke(text, caption, button, icon);
        }

        public ManagementModel(
            IRepository<T> repository,
            PaginationManager<T> paginationManager,
            ILogger<ManagementModel<T>>? logger = null,
            ILogger<PaginationManager<T>>? paginationLogger = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _tableConfig = TableConfigResolver.Resolve<T>();
            DgvTable = new DataTable();
            _logger = logger ?? NullLogger<ManagementModel<T>>.Instance;

            PaginationManager = paginationManager;
            PaginationManager.PageChanged += OnPageChanged;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await PaginationManager.InitializeAsync();
                await PaginationManager.GoToFirstPageAsync();
                _logger.LogInformation("Initialized model for {TableName} with {CurrentPage}/{TotalPages} pages", _tableConfig.TableName, PaginationManager.CurrentPage, PaginationManager.TotalPages);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Initialization failed for {TableName}: {ErrorMessage}", _tableConfig.TableName, ex.Message);
                throw new InvalidOperationException("Fatal error during initialization. Pagination will not function.", ex);
            }
        }

        public void ApplyFilter(object? sender, SearchRequestEventArgs e)
        {
            _logger.LogDebug("Applying filter with search term {SearchTerm}, option {SelectedOption}, case sensitive {IsCaseSensitive}", e.SearchTerm, e.SelectedOption, e.IsCaseSensitive);

            string? searchTerm = e.SearchTerm;
            DataTable dataTable = e.DataTable;
            string selectedOption = e.SelectedOption;
            bool isCaseSensitive = e.IsCaseSensitive;

            if (searchTerm == null)
            {
                _logger.LogWarning("Search term is null for {TableName}", _tableConfig.TableName);
                InvokeDisplayErrorMessage("Search Term is null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (dataTable == null || string.IsNullOrEmpty(selectedOption))
            {
                _logger.LogWarning("Invalid filter parameters: DataTable is {DataTableStatus}, SelectedOption is {SelectedOptionStatus}", dataTable == null ? "null" : "not null", string.IsNullOrEmpty(selectedOption) ? "empty" : "not empty");
                InvokeDisplayErrorMessage("Datatable and SelectedOption is null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<DataRow> filteredRows = FilterRows(dataTable, selectedOption, searchTerm, isCaseSensitive);

            DataTable filteredData = dataTable.Clone();
            foreach (DataRow row in filteredRows)
            {
                filteredData.Rows.Add(row.ItemArray);
            }

            DgvTable = filteredData;

            _logger.LogInformation("Filtered {X} rows for '{Option}' with search term '{Term}' (CaseSensitive: {Sensitivity} for {TableName}).", filteredRows.Count, selectedOption, searchTerm, isCaseSensitive, _tableConfig.TableName);
        }

        public static List<DataRow> FilterRows(DataTable dataTable, string selectedOption, string? searchTerm, bool isCaseSensitive)
        {
            // Return an empty list if the search term is null/empty or if the column does not exist
            // null is a possible value for certain database tables. IsNullOrEmpty wont filter out "null"
            if (string.IsNullOrEmpty(searchTerm) || !dataTable.Columns.Contains(selectedOption))
                return [];

            Type columnType = dataTable.Columns[selectedOption]!.DataType;

            if (columnType == typeof(int))
            {
                //Handle enum with case sensitivity
                if (Enum.TryParse(typeof(LicenseType), searchTerm, !isCaseSensitive, out object? enumValue) && enumValue != null)
                {
                    int enumIntValue = (int)enumValue;
                    return [.. dataTable.AsEnumerable().Where(row => row.Field<int?>(selectedOption) == enumIntValue)];
                }
                return [];
            }

            // Handle string columns with case sensitivity and partial matching
            if (columnType == typeof(string))
            {
                return [.. dataTable.AsEnumerable()
                .Where(row =>
                {
                    string? fieldValue = row.Field<string>(selectedOption);
                    if (fieldValue == null) return false;
                    return isCaseSensitive
                        ? fieldValue.Contains(searchTerm)
                        : fieldValue.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                })];
            }

            if (columnType == typeof(bool))
            {
                bool? boolSearchTerm = null;

                // Need to explicity look for accepted values else any unknown value resolves as true.
                if (searchTerm.Equals("1", StringComparison.OrdinalIgnoreCase)) boolSearchTerm = true;
                else if (searchTerm.Equals("0", StringComparison.OrdinalIgnoreCase)) boolSearchTerm = false;
                else if (searchTerm.Equals("true", StringComparison.OrdinalIgnoreCase)) boolSearchTerm = true;
                else if (searchTerm.Equals("false", StringComparison.OrdinalIgnoreCase)) boolSearchTerm = false;

                if (boolSearchTerm == null) return [];
                return [.. dataTable.AsEnumerable().Where(row => row.Field<bool>(selectedOption) == boolSearchTerm)];
            }

            return [];
        }

        public event EventHandler? PageChanged;
        public async Task OnPageChanged(int currentPage)
        {
            try
            {
                DataTable? result = await _repository.GetRecordsAtPageAsync(currentPage);
                if (result == null)
                {
                    DgvTable = new DataTable();
                    _logger.LogWarning("No data returned for page {CurrentPage} in {TableName}", currentPage, _tableConfig.TableName);
                }
                else
                {
                    DgvTable = result;
                    _logger.LogInformation("Fetched {RowCount} rows for page {CurrentPage} in {TableName}", DgvTable.Rows.Count, currentPage, _tableConfig.TableName);
                }
                PageChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch records for page {CurrentPage} in {TableName}: {ErrorMessage}",
                    currentPage, _tableConfig.TableName, ex.Message);
                DgvTable = new DataTable();
                PageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task AddRecordAsync(T entity)
        {
            try
            {
                int newPk = await _repository.InsertRecordAsync(entity);
                if (newPk == -1)
                {
                    _logger.LogWarning("Failed to insert record into {TableName}: No primary key returned", _tableConfig.TableName);
                    InvokeDisplayErrorMessage("Failed to add record to database", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DataRow newRow = DgvTable.NewRow();
                _tableConfig.MapToRow(newRow, entity);
                newRow[_tableConfig.PrimaryKey] = newPk;
                DgvTable.Rows.Add(newRow);
                PaginationManager.UpdateRecordCountAsync(PaginationManager.RecordCount + 1);
                await PaginationManager.GoToLastPageAsync();
                _logger.LogInformation("Added record with PK {PrimaryKey} to {TableName}", newPk, _tableConfig.TableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add record to {TableName}: {ErrorMessage}", _tableConfig.TableName, ex.Message);
                InvokeDisplayErrorMessage("Failed to add record to database", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task UpdateRecordAsync(T entity)
        {
            string table = _tableConfig.TableName;
            int pkValue = GetPrimaryKeyFromEntity(entity);

            DataRow? rowToUpdate = DgvTable.Rows.Find(pkValue);
            if (rowToUpdate == null)
            {
                _logger.LogWarning("{Table} with PK {Pk} was not found for update", table, pkValue);
                return;
            }

            bool success = await _repository.UpdateRecordAsync(entity);
            if (success)
            {
                _tableConfig.MapToRow(rowToUpdate, entity);
                _logger.LogInformation("{Table} with PK {Pk} was updated successfully", table, pkValue);
            }
            else
            {
                InvokeDisplayErrorMessage($"Failed to update {entity} in database", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task DeleteRecordAsync(int PkID)
        {
            try
            {
                DataRow? rowToDelete = DgvTable.Rows.Find(PkID);
                if (rowToDelete == null)
                {
                    _logger.LogWarning("Row with PK {PrimaryKey} not found in {TableName} for deletion", PkID, _tableConfig.TableName);
                    InvokeDisplayErrorMessage($"Record with ID {PkID} not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool success = await _repository.DeleteRecordAsync(PkID);
                if (success)
                {
                    DgvTable.Rows.Remove(rowToDelete);
                    PaginationManager.UpdateRecordCountAsync(PaginationManager.RecordCount - 1);
                    await PaginationManager.EnsureValidPageAsync();
                    _logger.LogInformation("Deleted record with PK {PrimaryKey} from {TableName}", PkID, _tableConfig.TableName);
                }
                else
                {
                    _logger.LogWarning("Failed to delete record with PK {PrimaryKey} from {TableName}", PkID, _tableConfig.TableName);
                    InvokeDisplayErrorMessage($"Failed to delete record with ID {PkID}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete record with PK {PrimaryKey} from {TableName}: {ErrorMessage}", PkID, _tableConfig.TableName, ex.Message);
                InvokeDisplayErrorMessage($"Failed to delete record with ID {PkID}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public T GetEntityFromRow(DataGridViewRow SelectedRow)
        {
            try
            {
                T entity = (T)_tableConfig.CreateFromRow(SelectedRow);
                _logger.LogDebug("Created entity from row for {TableName}", _tableConfig.TableName);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create entity from row for {TableName}: {ErrorMessage}", _tableConfig.TableName, ex.Message);
                throw;
            }
        }

        public async Task FetchAndBindRecordsAtPageAsync()
        {
            try
            {
                DgvTable = await _repository.GetRecordsAtPageAsync(PaginationManager.CurrentPage) ?? new DataTable();
                _logger.LogInformation("Fetched {RowCount} rows for page {CurrentPage} in {TableName}", DgvTable.Rows.Count, PaginationManager.CurrentPage, _tableConfig.TableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch records for page {CurrentPage} in {TableName}: {ErrorMessage}", PaginationManager.CurrentPage, _tableConfig.TableName, ex.Message);
                DgvTable = new DataTable();
            }
        }

        private int GetPrimaryKeyFromEntity(T Entity)
        {
            try
            {
                using SqlCommand DummyCommand = new();
                _tableConfig.MapUpdateParameters(Entity, DummyCommand);
                SqlParameter? param = DummyCommand.Parameters.Cast<SqlParameter>().FirstOrDefault(p => p.ParameterName == "@" + _tableConfig.PrimaryKey);
                if (param == null || param.Value == null)
                {
                    _logger.LogError("Primary key {PrimaryKey} not found in mapped parameters for {TableName}", _tableConfig.PrimaryKey, _tableConfig.TableName);
                    throw new InvalidOperationException($"Primary key {_tableConfig.PrimaryKey} not found in mapped parameters.");
                }
                return (int)param.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract primary key for {TableName}: {ErrorMessage}", _tableConfig.TableName, ex.Message);
                throw;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _logger.LogInformation("Disposing ManagementModel for type {Type}", typeof(T).Name);
                    DgvTable?.Dispose();

                    PaginationManager.PageChanged -= OnPageChanged;

                    DisplayErrorMessage = null;
                    PageChanged = null;

                    // IRepository does not currently need IDisposable. If it does, cleanup here
                }

                _disposedValue = true;
            }
        }

        ~ManagementModel()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
