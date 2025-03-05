using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using static StartSmartDeliveryForm.SharedLayer.EventDelegates.CustomEventDelegates;

namespace StartSmartDeliveryForm.PresentationLayer.TemplateViews
{
    public interface IDataForm
    {
        public FormMode Mode { get; set; }
        public event EventHandler<SubmissionCompletedEventArgs>? SubmitClicked;

        void OnSubmissionComplete(object sender, SubmissionCompletedEventArgs e);
        void InitializeEditing(object data);
        void ClearData();
        object GetData();
        void ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
    }
}
