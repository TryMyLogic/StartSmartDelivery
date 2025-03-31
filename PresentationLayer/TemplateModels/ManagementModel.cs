using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.SharedLayer.EventDelegates;

namespace StartSmartDeliveryForm.PresentationLayer.TemplateModels
{
    public class ManagementModel : IManagementModel
    {

        protected DataTable _dgvTable;
        private readonly ILogger<ManagementModel> _logger;

        public ManagementModel(ILogger<ManagementModel>? logger = null)
        {
            _dgvTable = new DataTable();
            _logger = logger ?? NullLogger<ManagementModel>.Instance;
        }

        public DataTable DgvTable => _dgvTable;
        public event MessageBoxEventDelegate? DisplayErrorMessage;

        public virtual async Task InitializeAsync() { await Task.Delay(100); }

        public void ApplyFilter(object? sender, SearchRequestEventArgs e)
        {
            _logger.LogInformation("Applying Filter");
            string? searchTerm = e.SearchTerm;
            DataTable dataTable = e.DataTable;
            string selectedOption = e.SelectedOption;
            bool isCaseSensitive = e.IsCaseSensitive;

            if (searchTerm != null)
            {
                _logger.LogWarning(searchTerm);
            }

            if (dataTable == null || string.IsNullOrEmpty(selectedOption))
            {
                // Refresh dgv or show an error message
                _logger.LogError("Invalid parameters for filtering.");
                return;
            }

            List<DataRow> filteredRows = FilterRows(dataTable, selectedOption, searchTerm, isCaseSensitive);

            DataTable filteredData = dataTable.Clone(); //Does not clone data, only the schema
            foreach (DataRow row in filteredRows)
            {
                filteredData.Rows.Add(row.ItemArray);
            }

            _dgvTable = filteredData;

            _logger.LogInformation($"Filtered {filteredRows.Count} rows for '{selectedOption}' with search term '{searchTerm}' (CaseSensitive: {isCaseSensitive}).");
        }

        public static List<DataRow> FilterRows(DataTable DataTable, string SelectedOption, string? SearchTerm, bool IsCaseSensitive)
        {
            // Return an empty list if the search term is null/empty or if the column does not exist
            // null is a possible value for certain database tables. IsNullOrEmpty wont filter out "null"
            if (string.IsNullOrEmpty(SearchTerm) || !DataTable.Columns.Contains(SelectedOption))
                return [];

            Type columnType = DataTable.Columns[SelectedOption]!.DataType;

            if (columnType == typeof(int))
            {
                //Handle enum with case sensitivity
                if (Enum.TryParse(typeof(LicenseType), SearchTerm, !IsCaseSensitive, out object? enumValue) && enumValue != null)
                {
                    int enumIntValue = (int)enumValue;
                    return [.. DataTable.AsEnumerable().Where(row => row.Field<int?>(SelectedOption) == enumIntValue)];
                }
                return [];
            }

            // Handle string columns with case sensitivity and partial matching
            if (columnType == typeof(string))
            {
                return [.. DataTable.AsEnumerable()
                                .Where(row =>
                                {
                                    string fieldValue = row.Field<string>(SelectedOption)!;
                                    if (fieldValue == null) return false;

                                    return IsCaseSensitive
                                        ? fieldValue.Contains(SearchTerm)
                                        : fieldValue.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase);
                                })];
            }

            if (columnType == typeof(bool))
            {
                bool? boolSearchTerm = null;

                // Need to explicity look for accepted values else any unknown value resolves as true.
                if (SearchTerm.Equals("1", StringComparison.OrdinalIgnoreCase))
                {
                    boolSearchTerm = true;
                }
                else if (SearchTerm.Equals("0", StringComparison.OrdinalIgnoreCase))
                {
                    boolSearchTerm = false;
                }
                else if (SearchTerm.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    boolSearchTerm = true;
                }
                else if (SearchTerm.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    boolSearchTerm = false;
                }

                if (boolSearchTerm == null)
                {
                    return [];
                }

                return [.. DataTable.AsEnumerable().Where(row => row.Field<bool>(SelectedOption) == boolSearchTerm)];
            }

            return [];
        }
    }
}
