using System.Data;
using StartSmartDeliveryForm.SharedLayer.Interfaces;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;

namespace StartSmartDeliveryForm.PresentationLayer.ManagementFormComponents
{
    public interface IManagementForm : ISearchableView
    {
        DataGridView DgvMain { get; }
        DataTable DataSource { get; set; }
        bool FirstLoad { get; set; }

        event EventHandler FormLoadOccurred;
        event EventHandler AddClicked;
        event EventHandler<int> EditClicked;
        event EventHandler<int> DeleteClicked;
        event EventHandler RefreshedClicked;
        event EventHandler ReloadClicked;
        event EventHandler RollbackClicked;
        event EventHandler PrintAllPagesByRowCountClicked;
        event EventHandler FirstPageClicked;
        event EventHandler PreviousPageClicked;
        event EventHandler NextPageClicked;
        event EventHandler LastPageClicked;
        event EventHandler<int> GoToPageClicked;
        event EventHandler PrintClicked;

        event EventHandler DashboardFormRequested;
        event EventHandler DeliveryManagementFormRequested;
        event EventHandler VehicleManagementFormRequested;
        event EventHandler DriverManagementFormRequested;

        event EventHandler ChangeUserRequested;

        void ShowMessageBox(string Text, string Caption, MessageBoxButtons Buttons, MessageBoxIcon Icon);
        void UpdatePaginationDisplay(int CurrentPage, int TotalPages);
        void HideExcludedColumns();
        void ConfigureDataGridViewColumns(Func<string, Image>? imageLoader = null);
        string StartPageText { get; set; }
        string EndPageText { get; set; }

        void SetTableConfig(TableConfig config);
        void SetSearchOptions();
        void InvokeFormLoadOccurred(object? sender, EventArgs e);
        HashSet<string> GetExcludedColumns();
    }
}
