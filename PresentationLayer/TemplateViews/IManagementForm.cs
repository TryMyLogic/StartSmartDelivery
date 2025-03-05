using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.PresentationLayer.TemplateViews
{
    internal interface IManagementForm : ISearchableView
    {
        public DataTable? DgvTable { get; set; }
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
        event EventHandler GoToPageClicked;
        event EventHandler PrintClicked;
    }
}
