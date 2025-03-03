using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using static StartSmartDeliveryForm.SharedLayer.EventDelegates.CustomEventDelegates;

namespace StartSmartDeliveryForm.PresentationLayer.TemplateModels
{
    public interface IDataForm
    {
        public FormMode Mode { get; set; }
        public event SubmitEventDelegate<EventArgs>? SubmitClicked;

        void OnSubmissionComplete(object sender, EventArgs e);
        void InitializeEditing(object data);
        void ClearData();
        object GetData();
        void ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
    }
}
