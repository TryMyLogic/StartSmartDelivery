using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.Generics
{
    public interface IGenericManagementForm : ISearchableView
    {
        DataGridView DgvMain { get; }
        DataTable DataSource { get; set; }

        event EventHandler FormLoadOccurred;
        event EventHandler AddClicked;
        event EventHandler<int> EditClicked;
        event EventHandler<int> DeleteClicked;
        event EventHandler RefreshClicked;
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
