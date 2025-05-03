using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.Repositories;
using StartSmartDeliveryForm.SharedLayer.Enums;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;

namespace StartSmartDeliveryForm.PresentationLayer.DataFormComponents
{
    public class DataModel<T>(
        IRepository<T> repository,
        ILogger<DataModel<T>>? logger = null) : IDataModel<T> where T : class, new()
    {
        private readonly ILogger<DataModel<T>> _logger = logger ?? NullLogger<DataModel<T>>.Instance;

        private readonly IRepository<T> _repository = repository;
        private readonly TableConfig _tableConfig = TableConfigResolver.Resolve<T>();

        public FormMode Mode { set; get; }

        public Dictionary<string, (Label Label, Control Control)> GenerateControls()
        {
            _logger.LogDebug("Generating controls for entity {EntityType}", typeof(T).Name);
            Type entityType = typeof(T);
            var controlsLayout = new Dictionary<string, (Label, Control)>();
            var entityProperties = entityType.GetProperties().ToDictionary(p => p.Name, p => p);

            foreach (ColumnConfig column in _tableConfig.Columns)
            {
                Label label = new()
                {
                    Text = column.Name,
                    AutoSize = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(3, 6, 3, 3)
                };

                Control control;
                if (column.SqlType == SqlDbType.Bit)
                {
                    control = new ComboBox
                    {
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Name = $"cbo{column.Name}",
                        Width = 150,
                        Anchor = AnchorStyles.Left | AnchorStyles.Top,
                        Margin = new Padding(3)
                    };
                    ((ComboBox)control).Items.AddRange(["False", "True"]);
                    object? defaultValue = _tableConfig.GetDefaultValue(column);
                    if (defaultValue is bool boolValue && boolValue) ((ComboBox)control).SelectedIndex = 1;
                }
                else if (entityProperties.TryGetValue(column.Name, out System.Reflection.PropertyInfo? prop) && prop.PropertyType.IsEnum)
                {
                    control = new ComboBox
                    {
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Name = $"cbo{column.Name}",
                        Width = 150,
                        Anchor = AnchorStyles.Left | AnchorStyles.Top,
                        Margin = new Padding(3)
                    };
                    ((ComboBox)control).Items.AddRange(Enum.GetNames(prop.PropertyType));
                    ((ComboBox)control).SelectedIndex = 0;
                }
                else
                {
                    control = new TextBox
                    {
                        Name = $"txt{column.Name}",
                        Width = 150,
                        Anchor = AnchorStyles.Left | AnchorStyles.Top,
                        Margin = new Padding(3)
                    };
                }

                controlsLayout[column.Name] = (label, control);
            }

            _logger.LogInformation("Generated {ControlCount} controls for entity {EntityType}", controlsLayout.Count, typeof(T).Name);
            return controlsLayout;
        }

        public T CreateFromForm(Dictionary<string, Control> controls)
        {
            _logger.LogDebug("Creating entity from form for {EntityType}", typeof(T).Name);
            T entity = new();
            System.Reflection.PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (System.Reflection.PropertyInfo prop in properties)
            {
                if (controls.TryGetValue(prop.Name, out Control? control))
                {
                    object? value = control switch
                    {
                        TextBox textBox => textBox.Text,
                        ComboBox comboBox => comboBox.SelectedItem?.ToString(),
                        _ => null
                    };

                    if (value != null)
                    {
                        try
                        {
                            if (prop.PropertyType == typeof(string))
                            {
                                prop.SetValue(entity, value.ToString());
                            }
                            else if (prop.PropertyType == typeof(int) && int.TryParse(value.ToString(), out int intValue))
                            {
                                prop.SetValue(entity, intValue);
                            }
                            else if (prop.PropertyType == typeof(bool) && bool.TryParse(value.ToString(), out bool boolValue))
                            {
                                prop.SetValue(entity, boolValue);
                            }
                            else if (prop.PropertyType.IsEnum && Enum.TryParse(prop.PropertyType, value.ToString(), out object? enumValue))
                            {
                                prop.SetValue(entity, enumValue);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Failed to set property {PropertyName} with value {Value}: {Error}", prop.Name, value, ex.Message);
                        }
                    }
                }
            }

            _logger.LogInformation("Entity created from form for {EntityType}", typeof(T).Name);
            return entity;
        }

        public Dictionary<string, object> MapToForm(T entity)
        {
            _logger.LogDebug("Mapping entity {EntityType} to form values", typeof(T).Name);
            Dictionary<string, object> values = [];
            System.Reflection.PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (System.Reflection.PropertyInfo prop in properties)
            {
                object? value = prop.GetValue(entity);
                values[prop.Name] = value ?? string.Empty;
            }

            _logger.LogInformation("Mapped {ValueCount} values for entity {EntityType}", values.Count, typeof(T).Name);
            return values;
        }

        public Dictionary<string, object?> GetDefaultValues()
        {
            _logger.LogDebug("Generating default values for entity {EntityType}", typeof(T).Name);
            Dictionary<string, object?> defaults = [];
            foreach (ColumnConfig column in _tableConfig.Columns)
            {
                object? defaultValue = _tableConfig.GetDefaultValue(column);
                defaults[column.Name] = defaultValue;
            }
            return defaults;
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateFormAsync(Dictionary<string, Control> controls)
        {
            _logger.LogDebug("Validating form for entity {EntityType} with {ControlCount} controls", typeof(T).Name, controls.Count);
            DataFormValidator validator = new();

            string? errorMessage = null;
            foreach (ColumnConfig column in _tableConfig.Columns)
            {
                if (!controls.TryGetValue(column.Name, out Control? control))
                {
                    _logger.LogError("Control not found for column: {ColumnName}", column.Name);
                    return (false, errorMessage);
                }

                string? stringValue = control switch
                {
                    TextBox textBox => textBox.Text,
                    ComboBox comboBox => comboBox.SelectedItem?.ToString(),
                    _ => null
                };

                if (stringValue == null)
                {
                    errorMessage = $"Value is null for column: {column.Name}";
                    _logger.LogWarning("Value is null for column: {ColumnName}", column.Name);
                    return (false, errorMessage);
                }

                switch (column.SqlType)
                {
                    case SqlDbType.NVarChar:
                    case SqlDbType.VarChar:
                        if (!validator.IsValidString(stringValue, column.Name))
                        {
                            errorMessage = $"Validation failed for string column: {column.Name}, Value: '{stringValue}'";
                            _logger.LogWarning("Validation failed for string column: {ColumnName}, Value: '{StringValue}'", column.Name, stringValue);
                            return (false, errorMessage);
                        }
                        break;
                    case SqlDbType.Int:
                        if (column.Name == "LicenseType")
                        {
                            if (!Enum.TryParse(typeof(LicenseType), stringValue, out _))
                            {
                                errorMessage = $"Validation failed for enum column: {column.Name}, Value: '{stringValue}'";
                                _logger.LogWarning("Validation failed for enum column: {ColumnName}, Value: '{StringValue}'", column.Name, stringValue);
                                return (false, errorMessage);
                            }
                            stringValue = ((int)Enum.Parse(typeof(LicenseType), stringValue)).ToString();
                        }
                        else if (!validator.IsValidIntValue(stringValue, column.Name))
                        {
                            errorMessage = $"Validation failed for int column: {column.Name}, Value: '{stringValue}'";
                            _logger.LogWarning("Validation failed for int column: {ColumnName}, Value: '{StringValue}'", column.Name, stringValue);
                            return (false, errorMessage);
                        }
                        break;
                    case SqlDbType.Bit:
                        if (!validator.IsValidBoolValue(stringValue))
                        {
                            errorMessage = $"Validation failed for bool column: {column.Name}, Value: '{stringValue}'";
                            _logger.LogWarning("Validation failed for bool column: {ColumnName}, Value: '{StringValue}'", column.Name, stringValue);
                            return (false, errorMessage);
                        }
                        break;
                    default:
                        _logger.LogWarning("Unhandled column.SqlType: {type}", column.SqlType);
                        break;
                }

                if (Mode == FormMode.Add && column.IsUnique && !string.IsNullOrEmpty(stringValue))
                {
                    _logger.LogDebug("Checking uniqueness for column {ColumnName} with value {Value}", column.Name, stringValue);
                    int fieldCount = await _repository.GetFieldCountAsync(column.Name, stringValue);
                    if (fieldCount > 0)
                    {
                        errorMessage = $"Unique constraint violated for column: {column.Name}, Value: '{stringValue}'";
                        _logger.LogWarning("Unique constraint violated for column: {ColumnName}, Value: '{StringValue}'", column.Name, stringValue);
                        return (false, errorMessage);
                    }
                }
            }

            _logger.LogInformation("Form validation succeeded for entity {EntityType}", typeof(T).Name);
            return (true, errorMessage);
        }
    }
}
