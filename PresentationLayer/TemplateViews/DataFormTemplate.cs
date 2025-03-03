using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.PresentationLayer.TemplateModels;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Interfaces;
using static StartSmartDeliveryForm.SharedLayer.EventArgs.CustomEventArgs;
using static StartSmartDeliveryForm.SharedLayer.EventDelegates.CustomEventDelegates;

namespace StartSmartDeliveryForm.PresentationLayer.TemplateViews
{
    public enum FormMode
    {
        Add,
        Edit
    }
    internal partial class DataFormTemplate : Form, IDataForm
    {
        public FormMode Mode { get; set; }
        public readonly ILogger<DataFormTemplate> _logger;
        private readonly IMessageBox _messageBox;

        public DataFormTemplate() : this(NullLogger<DataFormTemplate>.Instance, new MessageBoxWrapper()) { }
        public DataFormTemplate(ILogger<DataFormTemplate>? logger = null, IMessageBox? messageBox = null)
        {
            InitializeComponent();
            _logger = logger ?? NullLogger<DataFormTemplate>.Instance;
            _messageBox = messageBox ?? new MessageBoxWrapper();
        }

        private void DataFormTemplate_Load(object sender, EventArgs e)
        {
            btnSubmit.BackColor = GlobalConstants.SoftBeige;
            btnSubmit.FlatAppearance.BorderSize = 0;
        }

        private event SubmitEventDelegate<SubmissionCompletedEventArgs>? _submitClicked;

        event SubmitEventDelegate<SubmissionCompletedEventArgs>? IDataForm.SubmitClicked
        {
            add { _submitClicked += value; }
            remove { _submitClicked -= value; }
        }

        public void btnSubmit_Click(object sender, EventArgs e)
        {
            _logger.LogInformation("btnSubmit clicked");
            _submitClicked?.Invoke(this, SubmissionCompletedEventArgs.Empty);
        }

        public virtual void OnSubmissionComplete(object sender, SubmissionCompletedEventArgs e) { }
        public virtual void InitializeEditing(object data) { }
        public virtual void ClearData() { }
        public virtual object GetData() { return -1; }

        public void ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            _messageBox.Show(text, caption, buttons, icon);
        }
    }
}
