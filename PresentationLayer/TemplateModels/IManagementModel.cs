using System.Data;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.SharedLayer.EventDelegates;

namespace StartSmartDeliveryForm.PresentationLayer.TemplateModels
{
    public interface IManagementModel
    {
        public DataTable DgvTable { get; }
        Task InitializeAsync();
        void ApplyFilter(object? sender, SearchRequestEventArgs e);
        public event MessageBoxEventDelegate? DisplayErrorMessage;
    }
}
