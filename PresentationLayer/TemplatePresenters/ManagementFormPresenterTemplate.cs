using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.PresentationLayer.TemplateModels;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;

namespace StartSmartDeliveryForm.PresentationLayer.TemplatePresenters
{
    internal class ManagementFormPresenterTemplate
    {

        private readonly IManagementForm _managementForm;
        private readonly IManagementModel _managementModel;
        private readonly ILogger<ManagementFormPresenterTemplate> _logger;
        public ManagementFormPresenterTemplate(IManagementForm managementForm, IManagementModel managementModel, ILogger<ManagementFormPresenterTemplate>? logger = null)
        {
            _managementForm = managementForm;
            _managementModel = managementModel;
            _logger = logger ?? NullLogger<ManagementFormPresenterTemplate>.Instance;

            _managementForm.FormLoadOccurred += (s, e) => HandleFormLoadOccurred(s, e);
            _managementForm.SearchClicked += _managementModel.ApplyFilter;
            _managementForm.AddClicked += (s, e) => HandleAddClicked(s, e);
            _managementForm.EditClicked += (s, e) => HandleEditClicked(s, e);
            _managementForm.DeleteClicked += (s, e) => HandleDeleteClicked(s, e);
            _managementForm.ReloadClicked += (s, e) => HandleReloadClicked(s, e);
            _managementForm.RollbackClicked += (s, e) => HandleRollbackClicked(s, e);
            _managementForm.PrintAllPagesByRowCountClicked += (s, e) => HandlePrintAllPagesByRowCount(s, e);
            _managementForm.FirstPageClicked += (s, e) => HandleFirstPageClicked(s, e);
            _managementForm.PreviousPageClicked += (s, e) => HandlePreviousPageClicked(s, e);
            _managementForm.NextPageClicked += (s, e) => HandleNextPageClicked(s, e);
            _managementForm.LastPageClicked += (s, e) => HandleLastPageClicked(s, e);
            _managementForm.GoToPageClicked += (s, e) => HandleGoToPageClicked(s, e);
            _managementForm.PrintClicked += (s, e) => HandlePrintClicked(s, e);

            _managementModel.DisplayErrorMessage += _managementForm.ShowMessageBox;
        }

        protected virtual void HandleFormLoadOccurred(object? sender, EventArgs e) { _logger.LogInformation("HandleFormLoadOccurred Ran"); }
        protected virtual void HandleAddClicked(object? sender, EventArgs e) { _logger.LogInformation("HandleAddClicked Ran"); }
        protected virtual void HandleEditClicked(object? sender, int rowIndex) { _logger.LogInformation("HandleEditClicked Ran"); }
        protected virtual void HandleDeleteClicked(object? sender, int rowIndex) { _logger.LogInformation("HandleDeleteClicked Ran"); }
        protected virtual void HandleReloadClicked(object? sender, EventArgs e) { _logger.LogInformation("HandleReloadClicked Ran"); }
        protected virtual void HandleRollbackClicked(object? sender, EventArgs e) { _logger.LogInformation("HandleRollbackClicked Ran"); }
        protected virtual void HandlePrintAllPagesByRowCount(object? sender, EventArgs e) { _logger.LogInformation("HandlePrintAllPagesByRowCount Ran"); }
        protected virtual async void HandleFirstPageClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("HandleFirstPageClicked Ran");
            await Task.Delay(100);
        }
        protected virtual async void HandlePreviousPageClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("HandlePreviousPageClicked Ran");
            await Task.Delay(100);
        }
        protected virtual async void HandleNextPageClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("HandleNextPageClicked Ran");
            await Task.Delay(100);
        }
        protected virtual async void HandleLastPageClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("HandleLastPageClicked Ran");
            await Task.Delay(100);
        }
        protected virtual async void HandleGoToPageClicked(object? sender, int GotoPage)
        {
            _logger.LogInformation("HandleGoToPageClicked Ran");
            await Task.Delay(100);
        }
        protected virtual void HandlePrintClicked(object? sender, EventArgs e) { _logger.LogInformation("HandlePrintClicked Ran"); }
    }
}
