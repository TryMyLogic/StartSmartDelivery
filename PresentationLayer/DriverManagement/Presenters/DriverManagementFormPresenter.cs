using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Models;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Views;
using StartSmartDeliveryForm.PresentationLayer.TemplateModels;
using StartSmartDeliveryForm.PresentationLayer.TemplatePresenters;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.SharedLayer.EventArgs;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement.Presenters
{
    public class DriverManagementFormPresenter : ManagementFormPresenterTemplate
    {
        private readonly IDriverManagementForm _driverManagementForm;
        private readonly IDriverManagementModel _driverManagementModel;
        private readonly DriversDAO _driversDAO;
        private readonly ILogger<DriverManagementFormPresenter> _logger;

        private readonly ILogger<DriverDataForm> _dataFormLogger;
        private readonly ILogger<DriverDataFormPresenter> _dataFormPresenterLogger;
        private readonly ILogger<PrintDriverDataForm> _printDriverDataFormLogger;
        private readonly DataFormValidator _validator;
        private DriverDataForm? _driverDataForm;

        public DriverManagementFormPresenter(IDriverManagementForm driverManagementForm, IDriverManagementModel driverManagementModel, DriversDAO driversDAO, ILogger<DriverManagementFormPresenter>? logger = null, ILogger<DriverDataForm>? dataFormLogger = null, ILogger<DriverDataFormPresenter>? dataFormPresenterLogger = null, ILogger<PrintDriverDataForm>? printDriverDataFormLogger = null) : base(driverManagementForm, (IManagementModel<object>)driverManagementModel)
        {
            _driverManagementForm = driverManagementForm;
            _driverManagementModel = driverManagementModel;
            _driversDAO = driversDAO;
            _logger = logger ?? NullLogger<DriverManagementFormPresenter>.Instance;
            Initialize();

            _dataFormLogger = dataFormLogger ?? NullLogger<DriverDataForm>.Instance;
            _dataFormPresenterLogger = dataFormPresenterLogger ?? NullLogger<DriverDataFormPresenter>.Instance;
            _printDriverDataFormLogger = printDriverDataFormLogger ?? NullLogger<PrintDriverDataForm>.Instance;
            _validator = new DataFormValidator();

            _driverManagementModel.PageChanged += HandlePageChange;
        }

        public void HandlePageChange(object? sender, EventArgs e)
        {
            _driverManagementForm.DataSource = _driverManagementModel.DgvTable;
            _driverManagementForm.UpdatePaginationDisplay(_driverManagementModel.PaginationManager.CurrentPage, _driverManagementModel.PaginationManager.TotalPages);
        }

        public void HideDriverIDColumn()
        {
            DataGridViewColumn? driverColumn = _driverManagementForm.DgvMain.Columns[DriverColumns.DriverID];
            if (driverColumn != null)
            {
                driverColumn.Visible = false;
            }
        }

        private async void Initialize()
        {
            try
            {
                await _driverManagementModel.InitializeAsync();
                _driverManagementForm.DataSource = _driverManagementModel.DgvTable;
                _driverManagementForm.UpdatePaginationDisplay(_driverManagementModel.PaginationManager.CurrentPage, _driverManagementModel.PaginationManager.TotalPages);
                _driverManagementForm.AddEditDeleteButtons();
                HideDriverIDColumn();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError("Initialization failed: {Error}", ex);
                _driverManagementForm.ShowMessageBox("Initialization failed. This may result in errors such as Pagination not functioning", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void HandleAddClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("Overridden OnAddClicked Ran");
            _driverDataForm = new(_dataFormLogger);
            DriverDataFormPresenter presenter = new(_driverDataForm, _driversDAO, _validator, _dataFormPresenterLogger);
            presenter.SubmissionCompleted += DriverDataForm_SubmitClicked;
            _driverDataForm.Show();
        }

        protected override void HandleEditClicked(object? sender, int RowIndex)
        {
            _logger.LogInformation("Overridden OnEditClicked Ran");

            DataGridViewRow selectedRow = _driverManagementForm.DgvMain.Rows[RowIndex];
            DriversDTO driverData = _driverManagementModel.GetDriverFromRow(selectedRow);

            if (driverData.DriverID == 0)
            {
                _logger.LogError("Selected row returned no driverdata");
                _driverManagementForm.ShowMessageBox("Cannot open edit form", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _driverDataForm = new() { Mode = FormMode.Edit };

            _driverDataForm.InitializeEditing(driverData);
            DriverDataFormPresenter presenter = new(_driverDataForm, _driversDAO, _validator, _dataFormPresenterLogger);
            presenter.SubmissionCompleted += DriverDataForm_SubmitClicked;
            _driverDataForm.Show();
        }

        protected override async void HandleDeleteClicked(object? sender, int RowIndex)
        {
            _logger.LogInformation("Overridden OnDeleteClicked Ran");
            await HandleDelete(RowIndex);
        }

        private async Task HandleDelete(int RowIndex)
        {
            DataGridViewRow selectedRow = _driverManagementForm.DgvMain.Rows[RowIndex];
            object? DriverID = selectedRow.Cells[DriverColumns.DriverID].Value;

            DialogResult result = MessageBox.Show("Are you sure?", "Delete Row", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes && int.TryParse(DriverID?.ToString(), out int driverID))
            {
                _logger.LogInformation("Deleting Driver with DriverID: {DriverID}", driverID);
                await _driverManagementModel.DeleteDriverAsync(driverID);
            }
        }

        protected override async void HandleReloadClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("Overridden OnReloadClicked Ran");

            //Refetch data and rebind
            await _driverManagementModel.FetchAndBindDriversAtPage();
            _driverManagementForm.DgvMain.DataSource = null;
            _driverManagementForm.DgvMain.DataSource = _driverManagementModel.DgvTable;
            _driverManagementForm.SetDataGridViewColumns();

            MessageBox.Show("Succesfully Reloaded", "Reload Status");
        }

        protected override void HandleRollbackClicked(object? sender, EventArgs e) { _logger.LogInformation("Overridden OnRollbackClicked Ran"); }
        protected override void HandlePrintAllPagesByRowCount(object? sender, EventArgs e) { _logger.LogInformation("Overridden OnPrintAllPagesByRowCount Ran"); }
        protected override async void HandleFirstPageClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("Overridden OnFirstPageClicked Ran");
            await _driverManagementModel.PaginationManager.GoToFirstPage();
        }
        protected override async void HandlePreviousPageClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("Overridden OnPreviousPageClicked Ran");
            await _driverManagementModel.PaginationManager.GoToPreviousPage();
        }

        protected override async void HandleNextPageClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("Overridden OnNextPageClicked Ran");
            await _driverManagementModel.PaginationManager.GoToNextPage();
        }

        protected override async void HandleLastPageClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("Overridden OnLastPageClicked Ran");
            await _driverManagementModel.PaginationManager.GoToLastPage();
        }

        protected override async void HandleGoToPageClicked(object? sender, int GotoPage)
        {
            _logger.LogInformation("Overridden OnGoToPageClicked Ran");

            if (GotoPage == _driverManagementModel.PaginationManager.CurrentPage) return;

            if (GotoPage >= 1 && GotoPage <= _driverManagementModel.PaginationManager.TotalPages)
            {
                await _driverManagementModel.PaginationManager.GoToPage(GotoPage);
            }
        }

        protected override async void HandlePrintClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("Overridden OnPrintClicked Ran");

            PrintDriverDataForm preview = new(_printDriverDataFormLogger);
            PrintDriverDataFormPresenter<DriversDTO> printDriverDataFormPresenter = new(preview, _driversDAO, _driverManagementModel.DgvTable);
            await printDriverDataFormPresenter.InitializeAsync();

            //Unlike Show, it blocks execution on main form till complete
            preview.ShowDialog();
        }

        // DriverDataForm submit button event handlers
        private async void DriverDataForm_SubmitClicked(object? sender, EventArgs e) { await DriverDataForm_SubmitClickedAsync(sender, e); }
        private async Task DriverDataForm_SubmitClickedAsync(object? sender, EventArgs e)
        {
            if (_driverDataForm == null) return;

            _logger.LogInformation("sender is: {Sender}", sender);

            if (e is SubmissionCompletedEventArgs args)
            {
                _logger.LogInformation("object is: {Object}", args.Data);
                if (args.Data is DriversDTO driverDTO)
                {
                    {
                        if (args.Mode == FormMode.Add)
                        {
                            await _driverManagementModel.AddDriverAsync(driverDTO);
                        }
                        else if (args.Mode == FormMode.Edit)
                        {
                            await _driverManagementModel.UpdateDriverAsync(driverDTO);
                            _driverDataForm.Close();
                        }
                    }
                }
                _driverDataForm.ClearData();
            }
        }
    }
}
