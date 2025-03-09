using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement.Views
{
    public interface IDriverDataForm : IDataForm
    {
        int DriverID { get; set; }
        string DriverName { get; set; }
        string DriverSurname { get; set; }
        string DriverEmployeeNo { get; set; }
        LicenseType DriverLicenseType { get; set; }
        bool DriverAvailability { get; set; }
    }
}
