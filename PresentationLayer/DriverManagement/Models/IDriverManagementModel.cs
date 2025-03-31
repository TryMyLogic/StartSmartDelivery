using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.TemplateModels;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement.Models
{
    public interface IDriverManagementModel : IManagementModel<DriversDTO>
    {
        public event EventHandler? PageChanged;
        Task AddDriverAsync(DriversDTO Driver);
        Task UpdateDriverAsync(DriversDTO Driver);
        Task DeleteDriverAsync(int DriverId);
        DriversDTO GetDriverFromRow(DataGridViewRow SelectedRow);
        Task FetchAndBindDriversAtPage();
    }
}
