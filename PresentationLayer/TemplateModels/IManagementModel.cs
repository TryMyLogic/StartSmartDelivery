using System.Data;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.SharedLayer.EventDelegates;

namespace StartSmartDeliveryForm.PresentationLayer.TemplateModels
{
    public interface IManagementModel<T> where T : class
    {
        public DataTable DgvTable { get; }
        public PaginationManager<T> PaginationManager { get; }
        Task InitializeAsync();
        void ApplyFilter(object? sender, SearchRequestEventArgs e);
        public event MessageBoxEventDelegate? DisplayErrorMessage;
    }
}
