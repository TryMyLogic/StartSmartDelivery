using System.Data;
using Microsoft.Data.SqlClient;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.SharedLayer
{
    public class TableDefinition
    {
        public class ColumnConfig(string name, SqlDbType sqlType, bool isIdentity = false, bool isUnique = false, int? size = null)
        {
            public string Name { get; } = name;
            public SqlDbType SqlType { get; } = sqlType;
            public bool IsIdentity { get; } = isIdentity;
            public bool IsUnique { get; } = isUnique;
            public int? Size { get; } = size;
        }

        public class TableConfig
        {
            public string TableName { get; set; }
            public string PrimaryKey { get; set; }
            public Type EntityType { get; }
            public List<ColumnConfig> Columns { get; set; } = [];

            public Action<object, SqlCommand> MapInsertParameters { get; set; }
            public Action<object, SqlCommand> MapUpdateParameters { get; set; }
            public Action<DataRow, object> MapToRow { get; set; }
            public Func<DataGridViewRow, object> CreateFromRow { get; set; }
            public Action<object, Dictionary<string, Control>> MapToForm { get; set; }
            public Func<Dictionary<string, Control>, object> CreateFromForm { get; set; }
            public Func<ColumnConfig, object?> GetDefaultValue { get; set; }

            public TableConfig(string tableName, string primaryKey, Type entityType)
            {
                TableName = tableName;
                PrimaryKey = primaryKey;
                EntityType = entityType;
                Columns = [];

                MapInsertParameters = (entity, cmd) =>
                {
                    foreach (ColumnConfig? col in Columns.Where(c => !c.IsIdentity))
                    {
                        System.Reflection.PropertyInfo? prop = entity.GetType().GetProperty(col.Name);
                        object? value = prop?.GetValue(entity);
                        if (prop?.PropertyType.IsEnum == true) value = (int)value!;
                        cmd.Parameters.Add(new SqlParameter($"@{col.Name}", col.SqlType, col.Size ?? 0) { Value = value ?? DBNull.Value });
                    }
                };

                MapUpdateParameters = (entity, cmd) =>
                {
                    foreach (ColumnConfig col in Columns)
                    {
                        System.Reflection.PropertyInfo? prop = entity.GetType().GetProperty(col.Name);
                        object? value = prop?.GetValue(entity);
                        if (prop?.PropertyType.IsEnum == true) value = (int)value!;
                        cmd.Parameters.Add(new SqlParameter($"@{col.Name}", col.SqlType, col.Size ?? 0) { Value = value ?? DBNull.Value });
                    }
                };

                MapToRow = (row, entity) =>
                {
                    foreach (ColumnConfig col in Columns.Where(c => !c.IsIdentity))
                    {
                        System.Reflection.PropertyInfo? prop = entity.GetType().GetProperty(col.Name);
                        object? value = prop?.GetValue(entity);
                        if (prop?.PropertyType.IsEnum == true) value = (int)value!;
                        row[col.Name] = value ?? DBNull.Value;
                    }
                };

                CreateFromRow = row =>
                {
                    object entity = Activator.CreateInstance(entityType)!; // Ensure DTO has empty constructor
                    foreach (ColumnConfig col in Columns)
                    {
                        System.Reflection.PropertyInfo? prop = entityType.GetProperty(col.Name);
                        if (prop != null && row.Cells[col.Name].Value != null)
                        {
                            object value = row.Cells[col.Name].Value;
                            if (prop.PropertyType.IsEnum) value = Enum.ToObject(prop.PropertyType, value);
                            else value = Convert.ChangeType(value, prop.PropertyType);
                            prop.SetValue(entity, value);
                        }
                    }
                    return entity!;
                };

                MapToForm = (entity, controls) =>
                {
                    foreach (ColumnConfig col in Columns)
                    {
                        if (controls.TryGetValue(col.Name, out Control? control))
                        {
                            System.Reflection.PropertyInfo? prop = entity.GetType().GetProperty(col.Name);
                            object? value = prop?.GetValue(entity);
                            if (control is TextBox textBox) textBox.Text = value?.ToString() ?? "";
                            else if (control is ComboBox comboBox)
                            {
                                if (prop?.PropertyType.IsEnum == true)
                                {
                                    comboBox.SelectedItem = Enum.GetName(prop.PropertyType, value!) ?? Enum.GetNames(prop.PropertyType)[0];
                                }
                                else comboBox.SelectedItem = value?.ToString();
                            }
                        }
                    }
                };

                CreateFromForm = controls =>
                {
                    object? entity = Activator.CreateInstance(entityType, [0, "", "", "", default(LicenseType), false]);
                    foreach (ColumnConfig col in Columns)
                    {
                        if (controls.TryGetValue(col.Name, out Control? control))
                        {
                            System.Reflection.PropertyInfo? prop = entityType.GetProperty(col.Name);
                            if (prop != null)
                            {
                                object? value = control switch
                                {
                                    TextBox textBox => string.IsNullOrEmpty(textBox.Text) ? "" : Convert.ChangeType(textBox.Text, prop.PropertyType),
                                    ComboBox comboBox when col.SqlType == SqlDbType.Bit => bool.Parse(comboBox.SelectedItem?.ToString() ?? "False"),
                                    ComboBox comboBox when prop.PropertyType.IsEnum => Enum.Parse(prop.PropertyType, comboBox.SelectedItem?.ToString() ?? Enum.GetNames(prop.PropertyType)[0]),
                                    _ => null
                                };
                                if (value != null) prop.SetValue(entity, value);
                            }
                        }
                    }
                    return entity!;
                };

                GetDefaultValue = col => col.SqlType == SqlDbType.Bit ? false : null;
            }

            // Fluent API
            public TableConfig AddColumn(string name, SqlDbType sqlType, bool isIdentity = false, bool isUnique = false, int? size = null)
            {
                Columns.Add(new ColumnConfig(name, sqlType, isIdentity, isUnique, size));
                return this;
            }

            public TableConfig WithDefaults(Func<ColumnConfig, object?> getDefaultValue)
            {
                GetDefaultValue = getDefaultValue;
                return this;
            }
        }

        public static class TableConfigs
        {
            public static readonly TableConfig Empty = new TableConfig("EmptyTable", "Id", typeof(object))
                .AddColumn("Id", SqlDbType.Int, isIdentity: true);

            public static readonly TableConfig Drivers = new TableConfig("Drivers", "DriverID", typeof(DriversDTO))
                .AddColumn("DriverID", SqlDbType.Int, isIdentity: true)
                .AddColumn("Name", SqlDbType.NVarChar, size: 100)
                .AddColumn("Surname", SqlDbType.NVarChar, size: 100)
                .AddColumn("EmployeeNo", SqlDbType.NVarChar, isUnique: true, size: 50)
                .AddColumn("LicenseType", SqlDbType.Int)
                .AddColumn("Availability", SqlDbType.Bit)
                .WithDefaults(col => col.Name switch
                {
                    "Availability" => true,
                    _ => col.SqlType == SqlDbType.Bit ? false : null
                });
        }
    }
}


