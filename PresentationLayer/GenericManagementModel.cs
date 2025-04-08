using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.Generics;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.SharedLayer.EventDelegates;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;

namespace StartSmartDeliveryForm.PresentationLayer
{
    public class GenericManagementModel<T> : IGenericManagementModel<T> where T : class
    {
        private readonly ILogger<GenericManagementModel<T>> _logger;
        private readonly IRepository<T> _repository;
        private readonly TableConfig _tableConfig;

        public DataTable DgvTable { get; private set; }
        public GenericPaginationManager<T> PaginationManager { get; }

        public event MessageBoxEventDelegate? DisplayErrorMessage;
        protected void InvokeDisplayErrorMessage(string text, string caption, MessageBoxButtons button, MessageBoxIcon icon)
        {
            DisplayErrorMessage?.Invoke(text, caption, button, icon);
        }

        public GenericManagementModel(
            IRepository<T> repository,
            TableConfig tableConfig,
            GenericPaginationManager<T> paginationManager,
            ILogger<GenericManagementModel<T>>? logger = null,
            ILogger<GenericPaginationManager<T>>? paginationLogger = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _tableConfig = tableConfig ?? throw new ArgumentNullException(nameof(tableConfig));
            DgvTable = new DataTable();
            _logger = logger ?? NullLogger<GenericManagementModel<T>>.Instance;

            PaginationManager = paginationManager;
            PaginationManager.PageChanged += OnPageChanged;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await PaginationManager.InitializeAsync();
                await PaginationManager.GoToFirstPageAsync();
                _logger.LogInformation("CurrentPage: {CurrentPage}, TotalPages: {TotalPages}",
                    PaginationManager.CurrentPage, PaginationManager.TotalPages);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Fatal error during initialization. Pagination will not function.", ex);
            }
        }

        public void ApplyFilter(object? sender, SearchRequestEventArgs e)
        {
            _logger.LogInformation("Applying Filter");
            string? searchTerm = e.SearchTerm;
            DataTable dataTable = e.DataTable;
            string selectedOption = e.SelectedOption;
            bool isCaseSensitive = e.IsCaseSensitive;

            if (searchTerm == null)
            {
                _logger.LogError("SearchTerm is null");
                InvokeDisplayErrorMessage("Search Term is null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (dataTable == null || string.IsNullOrEmpty(selectedOption))
            {
                _logger.LogError("Invalid parameters for filtering.");
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

            _logger.LogInformation("Filtered {X} rows for '{Option}' with search term '{Term}' (CaseSensitive: {Sensitivity}).", filteredRows.Count, selectedOption, searchTerm, isCaseSensitive);
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
            _logger.LogInformation("Page changed to {CurrentPage}", currentPage);
            DataTable? result = await _repository.GetRecordsAtPageAsync(currentPage);
            if (result == null)
            {
                _logger.LogWarning("No data returned for page {CurrentPage}", currentPage);
            }
            DgvTable = result ?? new DataTable();
            PageChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task AddRecordAsync(T entity)
        {
            int newPk = await _repository.InsertRecordAsync(entity);
            if (newPk != -1)
            {
                DataRow newRow = DgvTable.NewRow();
                _tableConfig.MapToRow(newRow, entity);
                newRow[_tableConfig.PrimaryKey] = newPk;
                DgvTable.Rows.Add(newRow);
                PaginationManager.UpdateRecordCountAsync(PaginationManager.RecordCount + 1);
                await PaginationManager.GoToLastPageAsync();
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
            string table = _tableConfig.TableName;
            DataRow? rowToDelete = DgvTable.Rows.Find(PkID);
            if (rowToDelete == null)
            {
                _logger.LogWarning("{Table} with PK {Pk} was not found for deletion", table, PkID);
                return;
            }

            bool success = await _repository.DeleteRecordAsync(PkID);
            if (success)
            {
                DgvTable.Rows.Remove(rowToDelete);
                PaginationManager.UpdateRecordCountAsync(PaginationManager.RecordCount - 1);
                await PaginationManager.EnsureValidPageAsync();
                _logger.LogInformation("{Table} with PK {Pk} deleted successfully", table, PkID);
            }
            else
            {
                InvokeDisplayErrorMessage($"Failed to delete {PkID} from database", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public T GetEntityFromRow(DataGridViewRow SelectedRow)
        {
            return (T)_tableConfig.CreateFromRow(SelectedRow);
        }

        public async Task FetchAndBindRecordsAtPageAsync()
        {
            DgvTable = await _repository.GetRecordsAtPageAsync(PaginationManager.CurrentPage) ?? new DataTable();
        }

        private int GetPrimaryKeyFromEntity(T Entity)
        {
            using SqlCommand DummyCommand = new();
            _tableConfig.MapUpdateParameters(Entity, DummyCommand);
            SqlParameter? param = DummyCommand.Parameters.Cast<SqlParameter>().FirstOrDefault(p => p.ParameterName == "@" + _tableConfig.PrimaryKey);
            if (param == null || param.Value == null)
                throw new InvalidOperationException($"Primary key {_tableConfig.PrimaryKey} not found in mapped parameters.");
            return (int)param.Value;
        }
    }
}
