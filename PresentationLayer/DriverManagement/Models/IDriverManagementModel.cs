using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.TemplateModels;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement.Models
{
    internal interface IDriverManagementModel : IManagementModel
    {
        public PaginationManager PaginationManager { get; }
        public event EventHandler PageChanged;
        Task AddDriverAsync(DriversDTO Driver);
        Task UpdateDriverAsync(DriversDTO Driver);
        Task DeleteDriverAsync(int DriverId);
        DriversDTO GetDriverFromRow(DataGridViewRow SelectedRow);
        void CancelOperations();
        Task FetchAndBindDriversAtPage();
    }
}
