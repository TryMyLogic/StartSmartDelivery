using StartSmartDeliveryForm.PresentationLayer.TemplateViews;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement.Views
{
    public interface IDriverManagementForm : IManagementForm
    {
        void UpdatePaginationDisplay(int CurrentPage, int TotalPages);

        void SetDataGridViewColumns();

        string StartPageText { get; set; }
        string EndPageText { get; set; }
    }
}
