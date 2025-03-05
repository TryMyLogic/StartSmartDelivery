using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement.Views
{
    public interface IDriverDataForm : IDataForm
    {
        int DriverID { get; set; }
        string Name { get; set; }
        string Surname { get; set; }
        string EmployeeNo { get; set; }
        LicenseType LicenseType { get; set; }
        bool Availability { get; set; }
    }
}
