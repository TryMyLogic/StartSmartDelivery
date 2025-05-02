using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.PresentationLayer.DataFormComponents
{
    public interface IDataModel<T> where T : class, new()
    {
        FormMode Mode { get; set; }
        Dictionary<string, (Label Label, Control Control)> GenerateControls();
        T CreateFromForm(Dictionary<string, Control> controls);
        Dictionary<string, object> MapToForm(T entity);
        void ClearData(Dictionary<string, Control> controls);
        Task<(bool IsValid, string? ErrorMessage)> ValidateFormAsync(Dictionary<string, Control> controls);
    }
}
