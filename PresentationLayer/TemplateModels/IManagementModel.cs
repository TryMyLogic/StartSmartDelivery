using System.Data;
using StartSmartDeliveryForm.SharedLayer.EventArgs;

namespace StartSmartDeliveryForm.PresentationLayer.TemplateModels
{
    public interface IManagementModel
    {
        public DataTable DgvTable { get; }
        Task InitializeAsync();
        void ApplyFilter(object? sender, SearchRequestEventArgs e);
    }
}
