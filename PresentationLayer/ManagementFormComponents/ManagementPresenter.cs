using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.PresentationLayer.DataFormComponents;
using StartSmartDeliveryForm.PresentationLayer.PrintDataFormComponents;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;
using StartSmartDeliveryForm.DataLayer.Repositories;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Diagnostics;

namespace StartSmartDeliveryForm.PresentationLayer.ManagementFormComponents
{
    public class ManagementPresenter<T> : IDisposable where T : class, new()
    {
        private readonly IManagementForm _managementForm;
        private readonly IManagementModel<T> _managementModel;
        private readonly ILogger<ManagementPresenter<T>> _logger;
        private readonly ILogger<DataForm> _dataFormLogger;

        private readonly ILogger<PrintDataForm> _printDataFormLogger;
        private readonly DataFormValidator _validator;
        private DataForm? _dataForm;
        private readonly ILogger<DataFormPresenter<T>> _dataFormPresenterLogger;
        private readonly ILogger<PrintDataPresenter<T>> _printDataPresenterLogger;

        private readonly TableConfig _tableConfig;
        private readonly IRepository<T> _repository;

        private DataTable? _unfilteredDgvTable;
        private bool _disposedValue;

        public ManagementPresenter(
        IManagementForm managementForm,
        IManagementModel<T> managementModel,
        IRepository<T> repository,
        ILogger<ManagementPresenter<T>>? logger = null,
        ILogger<DataForm>? dataFormLogger = null,
        ILogger<DataFormPresenter<T>>? dataFormPresenterLogger = null,
        ILogger<PrintDataForm>? printDataFormLogger = null,
        ILogger<PrintDataPresenter<T>>? printDataPresenterLogger = null
        )
        {
            _managementForm = managementForm ?? throw new ArgumentNullException(nameof(managementForm));
            _managementModel = managementModel ?? throw new ArgumentNullException(nameof(managementModel));
            _tableConfig = TableConfigResolver.Resolve<T>();
            _logger = logger ?? NullLogger<ManagementPresenter<T>>.Instance;
            _dataFormLogger = dataFormLogger ?? NullLogger<DataForm>.Instance;
            _dataFormPresenterLogger = dataFormPresenterLogger ?? NullLogger<DataFormPresenter<T>>.Instance;
            _validator = new DataFormValidator();
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _printDataFormLogger = printDataFormLogger ?? NullLogger<PrintDataForm>.Instance;
            _printDataPresenterLogger = printDataPresenterLogger ?? NullLogger<PrintDataPresenter<T>>.Instance;

            _logger.LogInformation("Created ManagementPresenter, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
            WireUpEvents();
        }

        private void WireUpEvents()
        {
            _logger.LogDebug("Subscribing to ManagementForm and ManagementModel events, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
            _managementForm.FormLoadOccurred += HandleFormLoadOccurred;
            _managementForm.SearchClicked += HandleSearchClicked;
            _managementForm.AddClicked += HandleAddClicked;
            _managementForm.EditClicked += HandleEditClicked;
            _managementForm.DeleteClicked += HandleDeleteClicked;
            _managementForm.RefreshedClicked += HandleRefreshClicked;
            _managementForm.ReloadClicked += HandleReloadClicked;
            _managementForm.RollbackClicked += HandleRollbackClicked;
            _managementForm.PrintAllPagesByRowCountClicked += HandlePrintAllPagesByRowCount;
            _managementForm.FirstPageClicked += HandleFirstPageClicked;
            _managementForm.PreviousPageClicked += HandlePreviousPageClicked;
            _managementForm.NextPageClicked += HandleNextPageClicked;
            _managementForm.LastPageClicked += HandleLastPageClicked;
            _managementForm.GoToPageClicked += HandleGoToPageClicked;
            _managementForm.PrintClicked += HandlePrintClicked;

            _managementModel.DisplayErrorMessage += _managementForm.ShowMessageBox;
            _managementModel.PageChanged += HandlePageChange;
        }

        private void HandleSearchClicked(object? sender, SearchRequestEventArgs e)
        {
            _logger.LogInformation("Handling SearchClicked, DTO: {DtoType}, Table: {TableName}, Column: {Column}, Term: {SearchTerm}, CaseSensitive: {IsCaseSensitive}", typeof(T).Name, _tableConfig.TableName, e.SelectedOption, e.SearchTerm, e.IsCaseSensitive);
            _unfilteredDgvTable = e.DataTable;
            _managementModel.ApplyFilter(sender, e);
            _managementForm.DataSource = _managementModel.DgvTable;
            _logger.LogInformation("Search completed, DTO: {DtoType}, Table: {TableName}, Rows: {RowCount}", typeof(T).Name, _tableConfig.TableName, _managementModel.DgvTable.Rows.Count);
        }

        private async void HandleFormLoadOccurred(object? sender, EventArgs e)
        {
            _logger.LogInformation("Handling FormLoadOccurred, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
            try
            {
                _managementForm.SetTableConfig(_tableConfig);
                await _managementModel.InitializeAsync();
                _managementForm.DataSource = _managementModel.DgvTable;
                _unfilteredDgvTable = _managementModel.DgvTable;
                _managementForm.UpdatePaginationDisplay(_managementModel.PaginationManager.CurrentPage, _managementModel.PaginationManager.TotalPages);
                _managementForm.ConfigureDataGridViewColumns();
                _managementForm.SetSearchOptions();
                _managementForm.HideExcludedColumns();
                _logger.LogInformation("Form initialized, DTO: {DtoType}, Table: {TableName}, Rows: {RowCount}", typeof(T).Name, _tableConfig.TableName, _managementModel.DgvTable.Rows.Count);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Initialization failed, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
                _managementForm.ShowMessageBox("Initialization failed. This may result in errors such as Pagination not functioning", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void HandlePageChange(object? sender, EventArgs e)
        {
            _logger.LogDebug("Handling PageChange, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
            UpdateView();
        }

        public void UpdateView()
        {
            _logger.LogDebug("Updating view, DTO: {DtoType}, Table: {TableName}, Rows: {RowCount}", typeof(T).Name, _tableConfig.TableName, _managementModel.DgvTable.Rows.Count);
            _managementForm.DataSource = _managementModel.DgvTable;
            _managementForm.UpdatePaginationDisplay(_managementModel.PaginationManager.CurrentPage, _managementModel.PaginationManager.TotalPages);
        }

        private void HandleAddClicked(object? sender, EventArgs e)
        {
            _logger.LogDebug("Handling AddClicked, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
            _dataForm = new(typeof(T), _tableConfig, _dataFormLogger);
            DataFormPresenter<T> presenter = new(_dataForm, _repository, _tableConfig, _validator, _dataFormPresenterLogger);
            presenter.SubmissionCompleted += DataForm_SubmitClicked;

            _dataForm.FormClosed += (_, _) =>
            {
                _logger.LogDebug("DataForm closed for Add, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
                presenter.SubmissionCompleted -= DataForm_SubmitClicked;
                _dataForm.Dispose();
                _dataForm = null;
            };

            _dataForm.Show();
        }

        private void HandleEditClicked(object? sender, int rowIndex)
        {
            _logger.LogDebug("Handling EditClicked for row {RowIndex}, DTO: {DtoType}, Table: {TableName}", rowIndex, typeof(T).Name, _tableConfig.TableName);

            DataGridViewRow selectedRow = _managementForm.DgvMain.Rows[rowIndex];
            T entity = _managementModel.GetEntityFromRow(selectedRow);

            _dataForm = new DataForm(typeof(T), _tableConfig, _dataFormLogger) { Mode = FormMode.Edit };
            _dataForm.InitializeEditing(entity);
            var presenter = new DataFormPresenter<T>(_dataForm, _repository, _tableConfig, _validator, _dataFormPresenterLogger);
            presenter.SubmissionCompleted += DataForm_SubmitClicked;

            _dataForm.FormClosed += (_, _) =>
            {
                _logger.LogDebug("DataForm closed for Edit, DTO: {DtoType}, Table: {TableName}, RowIndex: {RowIndex}", typeof(T).Name, _tableConfig.TableName, rowIndex);
                presenter.SubmissionCompleted -= DataForm_SubmitClicked;
                _dataForm.Dispose();
                _dataForm = null;
            };

            _dataForm.Show();
        }

        private async void HandleDeleteClicked(object? sender, int RowIndex)
        {
            _logger.LogInformation("Delete Clicked for row {RowIndex}", RowIndex);

            try
            {
                await HandleDelete(RowIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete failed for row {RowIndex}, DTO: {DtoType}, Table: {TableName}", RowIndex, typeof(T).Name, _tableConfig.TableName);
                _managementForm.ShowMessageBox("Failed to delete record", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task HandleDelete(int RowIndex)
        {
            DataGridViewRow selectedRow = _managementForm.DgvMain.Rows[RowIndex];
            object? PkValue = selectedRow.Cells[_tableConfig.PrimaryKey].Value;

            DialogResult result = MessageBox.Show("Are you sure?", "Delete Row", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes && int.TryParse(PkValue?.ToString(), out int id))
            {
                _logger.LogInformation("Deleting record from {TableName} with ID: {ID}", _tableConfig.TableName, id);
                await _managementModel.DeleteRecordAsync(id);
            }
        }

        private void HandleRefreshClicked(object? sender, EventArgs e)
        {
            _logger.LogDebug("Handling RefreshClicked, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
            if (_unfilteredDgvTable != null)
            {
                _managementForm.DataSource = _unfilteredDgvTable;
                _managementForm.ConfigureDataGridViewColumns();
                _managementForm.HideExcludedColumns();
                _logger.LogInformation("Refresh completed, DTO: {DtoType}, Table: {TableName}, Rows: {RowCount}", typeof(T).Name, _tableConfig.TableName, _unfilteredDgvTable.Rows.Count);
                _managementForm.ShowMessageBox("Successfully Refreshed", "Refresh Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                _logger.LogWarning("No unfiltered table to refresh, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
                _managementForm.ShowMessageBox("No data to refresh", "Refresh Status", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void HandleReloadClicked(object? sender, EventArgs e)
        {
            _logger.LogDebug("Handling ReloadClicked, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);

            //Refetch data and rebind
            await _managementModel.FetchAndBindRecordsAtPageAsync();
            _managementForm.DgvMain.DataSource = null;
            _managementForm.DgvMain.DataSource = _managementModel.DgvTable;
            _managementForm.ConfigureDataGridViewColumns();
            _managementForm.HideExcludedColumns();
            _logger.LogInformation("Reload completed, DTO: {DtoType}, Table: {TableName}, Rows: {RowCount}", typeof(T).Name, _tableConfig.TableName, _managementModel.DgvTable.Rows.Count);
            _managementForm.ShowMessageBox("Successfully Reloaded", "Reload Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void HandleRollbackClicked(object? sender, EventArgs e)
        {
            _logger.LogDebug("Handling RollbackClicked, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName); ;
        }

        private void HandlePrintAllPagesByRowCount(object? sender, EventArgs e)
        {
            _logger.LogDebug("Handling PrintAllPagesByRowCountClicked, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
        }

        private async void HandleFirstPageClicked(object? sender, EventArgs e)
        {
            _logger.LogDebug("Handling FirstPageClicked, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
            await _managementModel.PaginationManager.GoToFirstPageAsync();
        }

        private async void HandlePreviousPageClicked(object? sender, EventArgs e)
        {
            _logger.LogDebug("Handling PreviousPageClicked, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
            await _managementModel.PaginationManager.GoToPreviousPageAsync();
        }

        private async void HandleNextPageClicked(object? sender, EventArgs e)
        {
            _logger.LogDebug("Handling NextPageClicked, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
            await _managementModel.PaginationManager.GoToNextPageAsync();
        }

        private async void HandleLastPageClicked(object? sender, EventArgs e)
        {
            _logger.LogDebug("Handling LastPageClicked, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
            await _managementModel.PaginationManager.GoToLastPageAsync();
        }

        private async void HandleGoToPageClicked(object? sender, int pageNumber)
        {
            _logger.LogDebug("Handling GoToPageClicked for page {PageNumber}, DTO: {DtoType}, Table: {TableName}", pageNumber, typeof(T).Name, _tableConfig.TableName);
            await _managementModel.PaginationManager.GoToPageAsync(pageNumber);
        }

        private async void HandlePrintClicked(object? sender, EventArgs e)
        {
            _logger.LogDebug("Handling PrintClicked, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);

            PrintDataForm preview = new(_printDataFormLogger);
            _ = await PrintDataPresenter<T>.CreateAsync(preview, _repository, _managementModel.DgvTable, _printDataPresenterLogger);

            //Unlike Show, it blocks execution on main form till complete
            preview.ShowDialog();
        }

        // DataForm submit button event handlers
        private async void DataForm_SubmitClicked(object? sender, EventArgs e) { await DataForm_SubmitClickedAsync(sender, e); }
        private async Task DataForm_SubmitClickedAsync(object? sender, EventArgs e)
        {
            if (_dataForm == null)
            {
                _logger.LogWarning("DataForm is null during submission, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);
                return;
            }

            _logger.LogDebug("Submission completed event received, DTO: {DtoType}, Table: {TableName}, Sender: {Sender}", typeof(T).Name, _tableConfig.TableName, sender);

            if (e is SubmissionCompletedEventArgs args)
            {
                _logger.LogDebug("Submitted data, DTO: {DtoType}, Table: {TableName}, Data: {Data}", typeof(T).Name, _tableConfig.TableName, args.Data);
                if (args.Data is T entity)
                {
                    try
                    {
                        if (args.Mode == FormMode.Add)
                        {
                            await _managementModel.AddRecordAsync(entity);
                            _logger.LogInformation("Added new record: {Entity}", entity);
                        }
                        else if (args.Mode == FormMode.Edit)
                        {
                            await _managementModel.UpdateRecordAsync(entity);
                            _logger.LogInformation("Updated record: {Entity}", entity);
                        }

                        UpdateView();
                        _dataForm.Close();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to process submission,  DTO: {DtoType}, Table: {TableName}, Error: {Error}", typeof(T).Name, _tableConfig.TableName, ex.Message);
                        _managementForm.ShowMessageBox("Failed to save record", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    _logger.LogWarning("Submitted data is not of type {Type}, DTO: {DtoType}, Table: {TableName}, Data: {Data}", typeof(T).Name, typeof(T).Name, _tableConfig.TableName, args.Data);
                }

                _dataForm.ClearData();
            }
        }

        private void UnwireEvents()
        {
            _managementForm.FormLoadOccurred -= HandleFormLoadOccurred;
            _managementForm.SearchClicked -= HandleSearchClicked;
            _managementForm.AddClicked -= HandleAddClicked;
            _managementForm.EditClicked -= HandleEditClicked;
            _managementForm.DeleteClicked -= HandleDeleteClicked;
            _managementForm.RefreshedClicked -= HandleRefreshClicked;
            _managementForm.ReloadClicked -= HandleReloadClicked;
            _managementForm.RollbackClicked -= HandleRollbackClicked;
            _managementForm.PrintAllPagesByRowCountClicked -= HandlePrintAllPagesByRowCount;
            _managementForm.FirstPageClicked -= HandleFirstPageClicked;
            _managementForm.PreviousPageClicked -= HandlePreviousPageClicked;
            _managementForm.NextPageClicked -= HandleNextPageClicked;
            _managementForm.LastPageClicked -= HandleLastPageClicked;
            _managementForm.GoToPageClicked -= HandleGoToPageClicked;
            _managementForm.PrintClicked -= HandlePrintClicked;

            _managementModel.DisplayErrorMessage -= _managementForm.ShowMessageBox;
            _managementModel.PageChanged -= HandlePageChange;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _logger.LogInformation("Disposing ManagementPresenter, DTO: {DtoType}, Table: {TableName}", typeof(T).Name, _tableConfig.TableName);

                    if (_managementModel is IDisposable disposableModel)
                    {
                        disposableModel.Dispose();
                    }

                    _dataForm?.Dispose();
                    _dataForm = null;

                    _unfilteredDgvTable?.Dispose();
                    _unfilteredDgvTable = null;

                    UnwireEvents();
                }

                _disposedValue = true;
            }
        }

        ~ManagementPresenter()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
