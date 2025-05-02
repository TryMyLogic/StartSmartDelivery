using StartSmartDeliveryForm.SharedLayer.EventArgs;

namespace StartSmartDeliveryForm.PresentationLayer.DataFormComponents
{
    public interface IDataForm
    {
        Dictionary<string, Control> GetControls();
        event EventHandler<SubmissionCompletedEventArgs> SubmitClicked;
        void InitializeEditing(Dictionary<string, object> values);
        void ClearData();
        void ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
        void RenderControls(Dictionary<string, (Label Label, Control Control)> controlsLayout);
    }
}
