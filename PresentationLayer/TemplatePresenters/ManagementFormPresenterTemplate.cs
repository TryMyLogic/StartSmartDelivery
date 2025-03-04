using System.Data;
using StartSmartDeliveryForm.SharedLayer.Enums;
using Serilog;
using StartSmartDeliveryForm.PresentationLayer.TemplateModels;
using StartSmartDeliveryForm.SharedLayer.Interfaces;
using StartSmartDeliveryForm.SharedLayer.EventArgs;

namespace StartSmartDeliveryForm.PresentationLayer.TemplatePresenters
{
    internal class ManagementFormPresenterTemplate
    {

        private readonly IManagementForm _managementForm;
        public ManagementFormPresenterTemplate(IManagementForm managementForm)
        {
            _managementForm = managementForm;

            if (managementForm is ISearchableView view) {
                Log.Information("ManagementForm is searchable");
                view.SearchClicked += ApplyFilter;
            }
        }

        private void ApplyFilter(object? sender, CustomEventArgs.SearchRequestEventArgs e)
        {
            Log.Information("Applying Filter");
            string? searchTerm = e.SearchTerm;
            DataTable dataTable = e.DataTable;
            string selectedOption = e.SelectedOption;
            
            if (_managementForm is not ISearchableView searchableView)
            {
                Log.Error("View is not searchable");
                return;
            }

            if (searchTerm != null)
            {
                Log.Warning(searchTerm);
            }

            if (dataTable == null || string.IsNullOrEmpty(selectedOption))
            {
                // Refresh dgv or show an error message
                Log.Error("Invalid parameters for filtering.");
                return;
            }

            List<DataRow> filteredRows = FilterRows(dataTable, selectedOption, searchTerm, searchableView.IsCaseSensitive);

            DataTable filteredData = dataTable.Clone(); //Does not clone data, only the schema
            foreach (DataRow row in filteredRows)
            {
                filteredData.Rows.Add(row.ItemArray); 
            }

            searchableView.UpdateDataGrid(filteredData);

            Log.Information($"Filtered {filteredRows.Count} rows for '{selectedOption}' with search term '{searchTerm}' (CaseSensitive: {searchableView.IsCaseSensitive}).");
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
                                    string fieldValue = row.Field<string>(selectedOption)!;
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
                if (searchTerm.Equals("1", StringComparison.OrdinalIgnoreCase))
                {
                    boolSearchTerm = true;
                }
                else if (searchTerm.Equals("0", StringComparison.OrdinalIgnoreCase))
                {
                    boolSearchTerm = false;
                }
                else if (searchTerm.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    boolSearchTerm = true;
                }
                else if (searchTerm.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    boolSearchTerm = false;
                }

                if (boolSearchTerm == null)
                {
                    return [];
                }

                return [.. dataTable.AsEnumerable().Where(row => row.Field<bool>(selectedOption) == boolSearchTerm)];
            }

            return [];
        }
    }
}
