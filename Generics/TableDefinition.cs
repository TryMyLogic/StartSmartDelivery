using System.Data;
using Microsoft.Data.SqlClient;
using StartSmartDeliveryForm.DataLayer.DTOs;

namespace StartSmartDeliveryForm.Generics
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

        public class TableConfig(string tableName, string primaryKey)
        {
            public string TableName { get; set; } = tableName;
            public string PrimaryKey { get; set; } = primaryKey;
            public List<ColumnConfig> Columns { get; set; } = [];

            public Action<object, SqlCommand>? MapInsertParameters { get; set; }
            public Action<object, SqlCommand>? MapUpdateParameters { get; set; }

            public TableConfig AddColumn(string name, SqlDbType sqlType, bool isIdentity = false, bool isUnique = false, int? size = null)
            {
                Columns.Add(new ColumnConfig(name, sqlType, isIdentity, isUnique, size));
                return this; // Fluent API for chaining
            }
        }

        public static class TableConfigs
        {
            public static readonly TableConfig Empty = new TableConfig("EmptyTable", "Id")
            .AddColumn("Id", SqlDbType.Int, isIdentity: true);

            public static readonly TableConfig Drivers = new("Drivers", "DriverID")
            {
                Columns =
            [
                new("DriverID", SqlDbType.Int, isIdentity: true),
                new("Name", SqlDbType.NVarChar, size: 100),
                new("Surname", SqlDbType.NVarChar, size: 100),
                new("EmployeeNo", SqlDbType.NVarChar, isUnique: true, size: 50),
                new("LicenseType", SqlDbType.Int),
                new("Availability", SqlDbType.Bit)
            ],
                MapInsertParameters = (entity, cmd) =>
                {
                    var driver = (DriversDTO)entity;
                    cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = driver.Name });
                    cmd.Parameters.Add(new SqlParameter("@Surname", SqlDbType.NVarChar, 100) { Value = driver.Surname });
                    cmd.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = driver.EmployeeNo });
                    cmd.Parameters.Add(new SqlParameter("@LicenseType", SqlDbType.Int) { Value = (int)driver.LicenseType });
                    cmd.Parameters.Add(new SqlParameter("@Availability", SqlDbType.Bit) { Value = driver.Availability });
                },
                MapUpdateParameters = (entity, cmd) =>
                {
                    var driver = (DriversDTO)entity;
                    cmd.Parameters.Add(new SqlParameter("@DriverID", SqlDbType.Int) { Value = driver.DriverID });
                    cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = driver.Name });
                    cmd.Parameters.Add(new SqlParameter("@Surname", SqlDbType.NVarChar, 100) { Value = driver.Surname });
                    cmd.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = driver.EmployeeNo });
                    cmd.Parameters.Add(new SqlParameter("@LicenseType", SqlDbType.Int) { Value = (int)driver.LicenseType });
                    cmd.Parameters.Add(new SqlParameter("@Availability", SqlDbType.Bit) { Value = driver.Availability });
                }
            };
        }
    }

}
