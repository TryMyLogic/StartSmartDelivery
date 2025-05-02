using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;

namespace StartSmartDeliveryForm.PresentationLayer.DataFormComponents
{
    public interface IDataPresenter<T>
    {
        FormMode Mode { get; set; }
        event EventHandler<SubmissionCompletedEventArgs>? SubmissionCompleted;
        void InitializeEditing(T entity);
        void SetMode(FormMode mode, T? entity = default);
    }
}
