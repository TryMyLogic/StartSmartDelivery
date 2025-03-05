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
        public DataTable DgvTable { get; set; }
    }
}
