﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using static StartSmartDeliveryForm.Generics.TableDefinition;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.PresentationLayer;

namespace StartSmartDeliveryForm.Generics
{
    public class GenericManagementPresenter<T> where T : class, new()
    {
        private readonly IGenericManagementForm _managementForm;
        private readonly IGenericManagementModel<T> _managementModel;
        private readonly ILogger<GenericManagementPresenter<T>> _logger;
        private readonly ILogger<GenericDataFormTemplate> _dataFormLogger;

        private readonly ILogger<GenericPrintDataForm> _printDataFormLogger;
        private readonly GenericDataFormValidator _validator;
        private GenericDataFormTemplate? _dataForm;
        private readonly ILogger<GenericDataFormPresenter<T>> _dataFormPresenterLogger;
        ILogger<GenericPrintDataPresenter<T>> _printDataPresenterLogger;

        private readonly TableConfig _tableConfig;
        private readonly IRepository<T> _repository;

        public GenericManagementPresenter(
        IGenericManagementForm managementForm,
        IGenericManagementModel<T> managementModel,
        TableConfig tableConfig,
        IRepository<T> repository,
        ILogger<GenericManagementPresenter<T>>? logger = null,
        ILogger<GenericDataFormTemplate>? dataFormLogger = null,
        ILogger<GenericDataFormPresenter<T>>? dataFormPresenterLogger = null,
        ILogger<GenericPrintDataForm>? printDataFormLogger = null,
        ILogger<GenericPrintDataPresenter<T>>? printDataPresenterLogger = null
        )
        {
            _managementForm = managementForm ?? throw new ArgumentNullException(nameof(managementForm));
            _managementModel = managementModel ?? throw new ArgumentNullException(nameof(managementModel));
            _tableConfig = tableConfig ?? throw new ArgumentNullException(nameof(tableConfig));
            _logger = logger ?? NullLogger<GenericManagementPresenter<T>>.Instance;
            _dataFormLogger = dataFormLogger ?? NullLogger<GenericDataFormTemplate>.Instance;
            _dataFormPresenterLogger = dataFormPresenterLogger ?? NullLogger<GenericDataFormPresenter<T>>.Instance;
            _validator = new GenericDataFormValidator();
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _printDataFormLogger = printDataFormLogger ?? NullLogger<GenericPrintDataForm>.Instance;
            _printDataPresenterLogger = printDataPresenterLogger ?? NullLogger<GenericPrintDataPresenter<T>>.Instance;

            WireUpEvents();
        }

        private void WireUpEvents()
        {
            _managementForm.FormLoadOccurred += HandleFormLoadOccurred;
            _managementForm.SearchClicked += HandleSearchClicked;
            _managementForm.AddClicked += HandleAddClicked;
            _managementForm.EditClicked += HandleEditClicked;
            _managementForm.DeleteClicked += HandleDeleteClicked;
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
            _managementModel.ApplyFilter(sender, e);
            _managementForm.DataSource = _managementModel.DgvTable;
        }

        private async void HandleFormLoadOccurred(object? sender, EventArgs e)
        {
            _logger.LogInformation("Form Load Occurred");
            try
            {
                await _managementModel.InitializeAsync();
                _managementForm.DataSource = _managementModel.DgvTable;
                _managementForm.UpdatePaginationDisplay(_managementModel.PaginationManager.CurrentPage, _managementModel.PaginationManager.TotalPages);
                _managementForm.HideExcludedColumns();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError("Initialization failed: {Error}", ex);
                _managementForm.ShowMessageBox("Initialization failed. This may result in errors such as Pagination not functioning", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void HandlePageChange(object? sender, EventArgs e)
        {
            UpdateView();
        }

        public void UpdateView()
        {
            _managementForm.DataSource = _managementModel.DgvTable;
            _managementForm.UpdatePaginationDisplay(_managementModel.PaginationManager.CurrentPage, _managementModel.PaginationManager.TotalPages);
        }

        private void HandleAddClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("Add Clicked");
            _dataForm = new(typeof(T), _tableConfig, _dataFormLogger);
            GenericDataFormPresenter<T> presenter = new(_dataForm, _repository, _tableConfig, _validator, _dataFormPresenterLogger);
            presenter.SubmissionCompleted += DataForm_SubmitClicked;

            _dataForm.Show();
        }

        private void HandleEditClicked(object? sender, int rowIndex)
        {
            _logger.LogInformation("Edit Clicked for row {RowIndex}", rowIndex);

            DataGridViewRow selectedRow = _managementForm.DgvMain.Rows[rowIndex];
            T entity = _managementModel.GetEntityFromRow(selectedRow);

            object? primaryKeyValue = selectedRow.Cells[_tableConfig.PrimaryKey].Value;
            if (primaryKeyValue == null || (int.TryParse(primaryKeyValue.ToString(), out int id) && id == 0))
            {
                _logger.LogError("Selected row returned no valid entity data");
                _managementForm.ShowMessageBox("Cannot open edit form", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _dataForm = new GenericDataFormTemplate(typeof(T), _tableConfig, _dataFormLogger) { Mode = FormMode.Edit };
            _dataForm.InitializeEditing(entity);
            var presenter = new GenericDataFormPresenter<T>(_dataForm, _repository, _tableConfig, _validator, _dataFormPresenterLogger);
            presenter.SubmissionCompleted += DataForm_SubmitClicked;
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
                _logger.LogError("Delete failed: {Error}", ex.Message);
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

        private async void HandleReloadClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("Reload Clicked");

            //Refetch data and rebind
            await _managementModel.FetchAndBindRecordsAtPageAsync();
            _managementForm.DgvMain.DataSource = null;
            _managementForm.DgvMain.DataSource = _managementModel.DgvTable;
            _managementForm.ConfigureDataGridViewColumns();
            _managementForm.HideExcludedColumns();

            _managementForm.ShowMessageBox("Succesfully Reloaded", "Reload Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void HandleRollbackClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("Rollback Clicked");
        }

        private void HandlePrintAllPagesByRowCount(object? sender, EventArgs e)
        {
            _logger.LogInformation("Print All Pages By Row Count Clicked");
        }

        private async void HandleFirstPageClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("First Page Clicked");
            await _managementModel.PaginationManager.GoToFirstPageAsync();
        }

        private async void HandlePreviousPageClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("Previous Page Clicked");
            await _managementModel.PaginationManager.GoToPreviousPageAsync();
        }

        private async void HandleNextPageClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("Next Page Clicked");
            await _managementModel.PaginationManager.GoToNextPageAsync();
        }

        private async void HandleLastPageClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("Last Page Clicked");
            await _managementModel.PaginationManager.GoToLastPageAsync();
        }

        private async void HandleGoToPageClicked(object? sender, int pageNumber)
        {
            _logger.LogInformation("Go To Page Clicked");
            await _managementModel.PaginationManager.GoToPageAsync(pageNumber);
        }

        private async void HandlePrintClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation("Print Clicked");

            GenericPrintDataForm preview = new(_printDataFormLogger);
            _ = await GenericPrintDataPresenter<T>.CreateAsync(preview, _repository, _managementModel.DgvTable, _printDataPresenterLogger);

            //Unlike Show, it blocks execution on main form till complete
            preview.ShowDialog();
        }

        // DataForm submit button event handlers
        private async void DataForm_SubmitClicked(object? sender, EventArgs e) { await DataForm_SubmitClickedAsync(sender, e); }
        private async Task DataForm_SubmitClickedAsync(object? sender, EventArgs e)
        {
            if (_dataForm == null) return;

            _logger.LogInformation("Submission completed event received from {Sender}", sender);

            if (e is SubmissionCompletedEventArgs args)
            {
                _logger.LogInformation("Submitted data: {Data}", args.Data);
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
                        _logger.LogError("Failed to process submission: {Error}", ex.Message);
                        _managementForm.ShowMessageBox("Failed to save record", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    _logger.LogWarning("Submitted data is not of type {Type}: {Data}", typeof(T).Name, args.Data);
                }

                _dataForm.ClearData();
            }
        }
    }

}
