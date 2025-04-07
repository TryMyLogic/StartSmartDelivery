using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;

namespace StartSmartDeliveryForm.Generics
{
    public interface IGenericDataForm
    {
        Dictionary<string, Control> GetControls();
        public FormMode Mode { get; set; }
        public event EventHandler<SubmissionCompletedEventArgs>? SubmitClicked;
        void OnSubmissionComplete(object sender, SubmissionCompletedEventArgs e);
        void InitializeEditing(object data);
        void ClearData();
        object GetData();
        void ShowMessageBox(string Text, string Caption, MessageBoxButtons Buttons, MessageBoxIcon Icon);
    }
}
