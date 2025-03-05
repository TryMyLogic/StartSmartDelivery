using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartSmartDeliveryForm.SharedLayer.EventArgs
{
    public class SearchRequestEventArgs(DataTable dataTable, string selectedOption, string? searchTerm, bool isCaseSensitive = false) : System.EventArgs
    {
        public DataTable DataTable { get; } = dataTable;
        public string SelectedOption { get; } = selectedOption;
        public string? SearchTerm { get; } = searchTerm;
        public bool IsCaseSensitive { get; } = isCaseSensitive;
        public static new SearchRequestEventArgs Empty => new(new DataTable(), string.Empty, null, false);
    }
}
