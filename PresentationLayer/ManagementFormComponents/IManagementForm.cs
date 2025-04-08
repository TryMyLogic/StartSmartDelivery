using System.Data;
using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.PresentationLayer.ManagementFormComponents
{
    public interface IManagementForm : ISearchableView
    {
        DataGridView DgvMain { get; }
        DataTable DataSource { get; set; }

        event EventHandler FormLoadOccurred;
        event EventHandler AddClicked;
        event EventHandler<int> EditClicked;
        event EventHandler<int> DeleteClicked;
        event EventHandler ReloadClicked;
        event EventHandler RollbackClicked;
        event EventHandler PrintAllPagesByRowCountClicked;
        event EventHandler FirstPageClicked;
        event EventHandler PreviousPageClicked;
        event EventHandler NextPageClicked;
        event EventHandler LastPageClicked;
        event EventHandler<int> GoToPageClicked;
        event EventHandler PrintClicked;

        void AddEditDeleteButtons(Func<string, Image>? imageLoader = null);
        void ShowMessageBox(string Text, string Caption, MessageBoxButtons Buttons, MessageBoxIcon Icon);
        void UpdatePaginationDisplay(int CurrentPage, int TotalPages);
        void HideExcludedColumns();
        void ConfigureDataGridViewColumns();
        string StartPageText { get; set; }
        string EndPageText { get; set; }
    }
}
