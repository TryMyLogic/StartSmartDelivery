using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StartSmartDeliveryForm.SharedLayer.EventArgs.CustomEventArgs;

namespace StartSmartDeliveryForm.SharedLayer.Interfaces
{
    public interface ISearchableView
    {
        event EventHandler<SearchRequestEventArgs> SearchClicked;
        bool IsCaseSensitive { get; }
        void UpdateDataGrid(DataTable UpdatedDataTable);
    }
}
