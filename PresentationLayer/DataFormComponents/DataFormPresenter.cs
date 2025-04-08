using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;
using StartSmartDeliveryForm.DataLayer.Repositories;

namespace StartSmartDeliveryForm.PresentationLayer.DataFormComponents
{
    public class DataFormPresenter<T> where T : class, new()
    {
        private readonly IDataForm _dataForm;
        private readonly IRepository<T> _repository;
        private readonly DataFormValidator _dataFormValidator;
        private readonly ILogger<DataFormPresenter<T>> _logger;
        private readonly TableConfig _tableConfig;

        public DataFormPresenter(
            IDataForm dataForm,
            IRepository<T> repository,
            TableConfig tableConfig,
            DataFormValidator? dataFormValidator = null,
            ILogger<DataFormPresenter<T>>? logger = null)
        {
            _dataForm = dataForm ?? throw new ArgumentNullException(nameof(dataForm));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _dataFormValidator = dataFormValidator ?? new DataFormValidator();
            _logger = logger ?? NullLogger<DataFormPresenter<T>>.Instance;
            _tableConfig = tableConfig;

            _dataForm.SubmitClicked += HandleSubmit_Clicked;
            _dataFormValidator.RequestMessageBox += _dataForm.ShowMessageBox;
        }

        public event EventHandler<SubmissionCompletedEventArgs>? SubmissionCompleted;

        private async void HandleSubmit_Clicked(object? sender, EventArgs e)
        {
            try
            {
                bool isValid = await ValidFormAsync();
                if (isValid)
                {
                    _logger.LogInformation("Form is valid");
                    object data = _dataForm.GetData();

                    _dataForm.OnSubmissionComplete(this, new SubmissionCompletedEventArgs(data, _dataForm.Mode));
                    SubmissionCompleted?.Invoke(this, new SubmissionCompletedEventArgs(data, _dataForm.Mode));
                }
                else
                {
                    _logger.LogInformation("Form is not valid");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Submission failed due to: {Exception}", ex.Message);
                _dataForm.ShowMessageBox("Submission Failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task<bool> ValidFormAsync()
        {
            _logger.LogInformation("Validating Form");

            Dictionary<string, Control> controls = _dataForm.GetControls();

            foreach (ColumnConfig column in _tableConfig.Columns)
            {
                if (!controls.TryGetValue(column.Name, out Control? control))
                {
                    _logger.LogWarning("Control not found for column: {ColumnName}", column.Name);
                    continue;
                }

                string? stringValue = control switch
                {
                    TextBox textBox => textBox.Text,
                    ComboBox comboBox => comboBox.SelectedItem?.ToString(),
                    _ => null // Unexpected control type
                };

                if (stringValue == null)
                {
                    _logger.LogInformation("Value is null for column: {ColumnName}", column.Name);
                    return false;
                }

                switch (column.SqlType)
                {
                    case SqlDbType.NVarChar:
                    case SqlDbType.VarChar:
                        if (!_dataFormValidator.IsValidString(stringValue, column.Name))
                        {
                            _logger.LogWarning("Validation failed for string column: {ColumnName}, Value: '{StringValue}'", column.Name, stringValue);
                            return false;
                        }
                        break;
                    case SqlDbType.Int:
                        if (column.Name == "LicenseType")
                        {
                            if (!Enum.TryParse(typeof(LicenseType), stringValue, out _))
                            {
                                _logger.LogWarning("Validation failed for enum column: {ColumnName}, Value: '{StringValue}'", column.Name, stringValue);
                                return false;
                            }
                            stringValue = ((int)Enum.Parse(typeof(LicenseType), stringValue)).ToString();
                        }
                        else if (!_dataFormValidator.IsValidIntValue(stringValue, column.Name))
                        {
                            _logger.LogWarning("Validation failed for int column: {ColumnName}, Value: '{StringValue}'", column.Name, stringValue);
                            return false;
                        }
                        break;
                    case SqlDbType.Bit:
                        if (!_dataFormValidator.IsValidBoolValue(stringValue))
                        {
                            _logger.LogWarning("Validation failed for bool column: {ColumnName}, Value: '{StringValue}'", column.Name, stringValue);
                            return false;
                        }
                        break;
                }

                if (_dataForm.Mode == FormMode.Add && column.IsUnique && !string.IsNullOrEmpty(stringValue))
                {
                    int fieldCount = await _repository.GetFieldCountAsync(column.Name, stringValue);
                    if (fieldCount > 0)
                    {
                        _dataForm.ShowMessageBox($"{column.Name} '{stringValue}' is not unique.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
