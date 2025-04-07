using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.SharedLayer.EventArgs
{
    public class SubmissionCompletedEventArgs(object? data, FormMode mode) : System.EventArgs
    {
        public object? Data { get; } = data;
        public FormMode Mode { get; } = mode;

        public static new SubmissionCompletedEventArgs Empty => new(null, FormMode.Add);
    }
}
