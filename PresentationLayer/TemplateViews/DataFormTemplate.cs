﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.PresentationLayer.TemplateViews
{
    public partial class DataFormTemplate : Form, IDataForm
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

        private event EventHandler<SubmissionCompletedEventArgs>? _submitClicked;

        public event EventHandler<SubmissionCompletedEventArgs>? SubmitClicked
        {
            add { _submitClicked += value; }
            remove { _submitClicked -= value; }
        }

        public void btnSubmit_Click(object? sender, EventArgs e)
        {
            _logger.LogInformation("btnSubmit clicked");
            _submitClicked?.Invoke(this, SubmissionCompletedEventArgs.Empty);
        }

        public virtual void OnSubmissionComplete(object? sender, SubmissionCompletedEventArgs e) { }
        public virtual void InitializeEditing(object data) { }
        public virtual void ClearData() { }
        public virtual object GetData() { return -1; }

        public void ShowMessageBox(string Text, string Caption, MessageBoxButtons Buttons, MessageBoxIcon Icon)
        {
            _messageBox.Show(Text, Caption, Buttons, Icon);
        }
    }
}
