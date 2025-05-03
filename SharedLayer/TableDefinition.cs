using System.Data;
using Microsoft.Data.SqlClient;
using StartSmartDeliveryForm.DataLayer.DTOs;

namespace StartSmartDeliveryForm.SharedLayer
{
    public class TableDefinition
    {
        public record ColumnConfig(string name, SqlDbType sqlType, bool isIdentity = false, bool isUnique = false, bool isNullable = false, int? size = null, int? precision = null, int? scale = null)
        {
            public string Name { get; } = name;
            public SqlDbType SqlType { get; } = sqlType;
            public bool IsIdentity { get; } = isIdentity;
            public bool IsUnique { get; } = isUnique;
            public bool IsNullable { get; } = isNullable;
            public int? Size { get; } = size;
            public int? Precision { get; } = precision;
            public int? Scale { get; } = scale;
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

                        if (!col.IsNullable && value == null) throw new InvalidOperationException($"Non-nullable column {col.Name} cannot be null.");

                        SqlParameter parameter = new($"@{col.Name}", col.SqlType, col.Size ?? 0) { Value = value ?? DBNull.Value };
                        if (col.SqlType == SqlDbType.Decimal && col.Precision.HasValue && col.Scale.HasValue)
                        {
                            parameter.Precision = (byte)col.Precision.Value;
                            parameter.Scale = (byte)col.Scale.Value;
                        }

                        cmd.Parameters.Add(parameter);
                    }
                };

                MapUpdateParameters = (entity, cmd) =>
                {
                    foreach (ColumnConfig col in Columns)
                    {
                        System.Reflection.PropertyInfo? prop = entity.GetType().GetProperty(col.Name);
                        object? value = prop?.GetValue(entity);

                        if (!col.IsNullable && value == null && !col.IsIdentity) throw new InvalidOperationException($"Non-nullable column {col.Name} cannot be null.");

                        if (prop?.PropertyType.IsEnum == true) value = (int)value!;

                        SqlParameter parameter = new($"@{col.Name}", col.SqlType, col.Size ?? 0) { Value = value ?? DBNull.Value };
                        if (col.SqlType == SqlDbType.Decimal && col.Precision.HasValue && col.Scale.HasValue)
                        {
                            parameter.Precision = (byte)col.Precision.Value;
                            parameter.Scale = (byte)col.Scale.Value;
                        }

                        cmd.Parameters.Add(parameter);
                    }
                };

                MapToRow = (row, entity) =>
                {
                    foreach (ColumnConfig col in Columns.Where(c => !c.IsIdentity))
                    {
                        System.Reflection.PropertyInfo? prop = entity.GetType().GetProperty(col.Name);
                        object? value = prop?.GetValue(entity);

                        if (!col.IsNullable && value == null && !col.IsIdentity) throw new InvalidOperationException($"Non-nullable column {col.Name} cannot be null.");

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
                        if (prop != null && row.Cells[col.Name].Value != null && row.Cells[col.Name].Value != DBNull.Value)
                        {
                            object value = row.Cells[col.Name].Value;
                            if (prop.PropertyType.IsEnum) value = Enum.ToObject(prop.PropertyType, value);
                            else value = Convert.ChangeType(value, prop.PropertyType);
                            prop.SetValue(entity, value);
                        }
                    }
                    return entity!;
                };

                GetDefaultValue = col => col.SqlType == SqlDbType.Bit ? false : null;
            }

            // Fluent API
            public TableConfig AddColumn(string name, SqlDbType sqlType, bool isIdentity = false, bool isUnique = false, bool isNullable = false, int? size = null, int? precision = null, int? scale = null)
            {
                Columns.Add(new ColumnConfig(name, sqlType, isIdentity, isUnique, isNullable, size, precision, scale));
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

            public static readonly TableConfig Vehicles = new TableConfig("Vehicles", "VehicleID", typeof(VehiclesDTO))
                .AddColumn("VehicleID", SqlDbType.Int, isIdentity: true)
                .AddColumn("Make", SqlDbType.NVarChar, size: 50)
                .AddColumn("Model", SqlDbType.NVarChar, size: 50)
                .AddColumn("Year", SqlDbType.Int)
                .AddColumn("NumberPlate", SqlDbType.NVarChar, size: 20)
                .AddColumn("Availability", SqlDbType.Int)
                .WithDefaults(col => col.Name switch
                {
                    "Availability" => true,
                    _ => col.SqlType == SqlDbType.Bit ? false : null
                });

            public static readonly TableConfig Deliveries = new TableConfig("DeliveryTask", "TaskID", typeof(DeliveriesDTO))
                .AddColumn("TaskID", SqlDbType.Int, isIdentity: true)
                .AddColumn("OrderNumber", SqlDbType.NVarChar, size: 50)
                .AddColumn("CustomerCode", SqlDbType.NVarChar, size: 50)
                .AddColumn("Name", SqlDbType.NVarChar, size: 100)
                .AddColumn("Telephone", SqlDbType.NVarChar, size: 20, isNullable: true)
                .AddColumn("Cellphone", SqlDbType.NVarChar, size: 20)
                .AddColumn("Email", SqlDbType.NVarChar, size: 100, isNullable: true)
                .AddColumn("Address", SqlDbType.NVarChar, size: 200)
                .AddColumn("Product", SqlDbType.NVarChar, size: 100)
                .AddColumn("Amount", SqlDbType.Decimal, precision: 18, scale: 2)
                .AddColumn("PaymentMethod", SqlDbType.NVarChar, size: 50)
                .AddColumn("Notes", SqlDbType.NVarChar, size: 500, isNullable: true)
                .AddColumn("ReceivedTimestamp", SqlDbType.DateTime)
                .AddColumn("DispatchTimestamp", SqlDbType.DateTime, isNullable: true)
                .AddColumn("AssignedDriver", SqlDbType.NVarChar, size: 200)
                .WithDefaults(col => col.Name switch
                {
                    _ => col.SqlType switch
                    {
                        SqlDbType.NVarChar => string.Empty,
                        SqlDbType.Decimal => 0m,
                        SqlDbType.DateTime => DateTime.MinValue,
                        _ => null
                    }
                });
        }

        public class TableConfigResolver
        {
            public static TableConfig Resolve<T>()
            {
                if (typeof(T) == typeof(DriversDTO))
                {
                    return TableConfigs.Drivers;
                }
                else if (typeof(T) == typeof(VehiclesDTO))
                {
                    return TableConfigs.Vehicles;
                }
                else if (typeof(T) == typeof(DeliveriesDTO))
                    return TableConfigs.Deliveries;
                else
                    throw new InvalidOperationException($"No TableConfig found for type {typeof(T).Name}");
            }
        }
    }
}


