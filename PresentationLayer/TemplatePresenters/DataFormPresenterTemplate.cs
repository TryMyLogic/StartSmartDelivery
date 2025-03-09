using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer.EventArgs;

namespace StartSmartDeliveryForm.PresentationLayer.TemplatePresenters
{
    internal class DataFormPresenterTemplate
    {
        protected readonly IDataForm _dataForm;
        private readonly ILogger<DataFormPresenterTemplate> _logger;

        public DataFormPresenterTemplate(IDataForm dataForm, ILogger<DataFormPresenterTemplate>? logger = null)
        {
            _dataForm = dataForm;
            _dataForm.SubmitClicked += HandleSubmit_Clicked;
            _logger = logger ?? NullLogger<DataFormPresenterTemplate>.Instance;
        }

        public event EventHandler<SubmissionCompletedEventArgs>? SubmissionCompleted;
        internal async void HandleSubmit_Clicked(object? sender, EventArgs e)
        {
            try
            {     
                bool Valid = await ValidFormAsync();
                if (Valid)
                {
                    _logger.LogInformation("Is valid");
                    _dataForm.OnSubmissionComplete(this, new SubmissionCompletedEventArgs(_dataForm.GetData(), _dataForm.Mode));
                    SubmissionCompleted?.Invoke(this, new SubmissionCompletedEventArgs(_dataForm.GetData(), _dataForm.Mode));
                }
                else
                {
                    _logger.LogInformation("Is not valid");
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
            _logger.LogCritical("ValidFormAsync is running in parent. Child must override it.");
            await Task.Delay(100);
            return true;
        }
    }
}
