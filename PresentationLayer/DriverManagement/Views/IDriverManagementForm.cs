using StartSmartDeliveryForm.PresentationLayer.TemplateViews;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement.Views
{
    internal interface IDriverManagementForm : IManagementForm
    {
        void UpdatePaginationDisplay(int CurrentPage, int TotalPages);

        void SetDataGridViewColumns();

    }
}
