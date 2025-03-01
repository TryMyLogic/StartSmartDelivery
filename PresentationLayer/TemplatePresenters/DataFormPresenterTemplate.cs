using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Presenters;
using StartSmartDeliveryForm.PresentationLayer.TemplateModels;
using static StartSmartDeliveryForm.SharedLayer.EventHandlers.EventHandlers;

namespace StartSmartDeliveryForm.PresentationLayer.TemplatePresenters
{
    internal class DataFormPresenterTemplate
    {
        protected readonly IDataForm _dataForm;
        private readonly ILogger<DataFormPresenterTemplate> _logger;

        public DataFormPresenterTemplate(IDataForm dataForm, ILogger<DataFormPresenterTemplate>? logger = null)
        {
            _dataForm = dataForm;
            _dataForm.SubmitClicked += OnSubmit_Clicked;
            _logger = logger ?? NullLogger<DataFormPresenterTemplate>.Instance;
        }

        public event SubmitEventHandler? SubmissionCompleted;
        internal async void OnSubmit_Clicked(object sender, EventArgs e)
        {
            try
            {
                _logger.LogInformation("OnSubmit_clicked ran (Information)"); // Level 2
                bool Valid = await ValidFormAsync();
                _logger.LogInformation("Valid: {Valid}", Valid);
                if (Valid)
                {
                    _dataForm.OnSubmissionComplete(this, EventArgs.Empty);
                    SubmissionCompleted?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Submission Failed due to: {Exception}", ex.Message);
                _dataForm.ShowMessageBox("Submission Failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected virtual async Task<bool> ValidFormAsync()
        {
            await Task.Delay(100);
            return false;
        }
    }
}
