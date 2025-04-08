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

        public class TableConfig(string tableName, string primaryKey)
        {
            public string TableName { get; set; } = tableName;
            public string PrimaryKey { get; set; } = primaryKey;
            public List<ColumnConfig> Columns { get; set; } = [];

            public Action<object, SqlCommand> MapInsertParameters { get; set; } = (_, _) => throw new InvalidOperationException("Insert mapping not defined for this table.");
            public Action<object, SqlCommand> MapUpdateParameters { get; set; } = (_, _) => throw new InvalidOperationException("Update mapping not defined for this table.");
            public Action<DataRow, object> MapToRow { get; set; } = (_, _) => throw new NotImplementedException("MapToRow must be implemented.");
            public Func<DataGridViewRow, object> CreateFromRow { get; set; } = _ => throw new NotImplementedException("CreateFromRow must be implemented.");
            public Action<object, Dictionary<string, Control>> MapToForm { get; set; } = (_, _) => throw new NotImplementedException("MapToForm must be implemented.");
            public Func<Dictionary<string, Control>, object> CreateFromForm { get; set; } = _ => throw new NotImplementedException("CreateFromForm must be implemented.");
            public Func<ColumnConfig, object?> GetDefaultValue { get; set; } = column => column.SqlType == SqlDbType.Bit ? true : null;

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
                    DriversDTO driver = (DriversDTO)entity;
                    cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = driver.Name });
                    cmd.Parameters.Add(new SqlParameter("@Surname", SqlDbType.NVarChar, 100) { Value = driver.Surname });
                    cmd.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = driver.EmployeeNo });
                    cmd.Parameters.Add(new SqlParameter("@LicenseType", SqlDbType.Int) { Value = (int)driver.LicenseType });
                    cmd.Parameters.Add(new SqlParameter("@Availability", SqlDbType.Bit) { Value = driver.Availability });
                },
                MapUpdateParameters = (entity, cmd) =>
                {
                    DriversDTO driver = (DriversDTO)entity;
                    cmd.Parameters.Add(new SqlParameter("@DriverID", SqlDbType.Int) { Value = driver.DriverID });
                    cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = driver.Name });
                    cmd.Parameters.Add(new SqlParameter("@Surname", SqlDbType.NVarChar, 100) { Value = driver.Surname });
                    cmd.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = driver.EmployeeNo });
                    cmd.Parameters.Add(new SqlParameter("@LicenseType", SqlDbType.Int) { Value = (int)driver.LicenseType });
                    cmd.Parameters.Add(new SqlParameter("@Availability", SqlDbType.Bit) { Value = driver.Availability });
                },
                MapToRow = (row, entity) =>
                {
                    DriversDTO driver = (DriversDTO)entity;
                    row["Name"] = driver.Name;
                    row["Surname"] = driver.Surname;
                    row["EmployeeNo"] = driver.EmployeeNo;
                    row["LicenseType"] = (int)driver.LicenseType;
                    row["Availability"] = driver.Availability;
                },
                CreateFromRow = (row) => new DriversDTO(
                    DriverID: (int)row.Cells["DriverID"].Value,
                    Name: row.Cells["Name"].Value.ToString()!,
                    Surname: row.Cells["Surname"].Value.ToString()!,
                    EmployeeNo: row.Cells["EmployeeNo"].Value.ToString()!,
                    LicenseType: (LicenseType)row.Cells["LicenseType"].Value,
                    Availability: (bool)row.Cells["Availability"].Value
                ),
                MapToForm = (entity, controls) =>
                {
                    DriversDTO driver = (DriversDTO)entity;
                    if (controls.TryGetValue("DriverID", out Control? driverIdControl) && driverIdControl is TextBox driverIdTextBox)
                        driverIdTextBox.Text = driver.DriverID.ToString();
                    if (controls.TryGetValue("Name", out Control? nameControl) && nameControl is TextBox nameTextBox)
                        nameTextBox.Text = driver.Name;
                    if (controls.TryGetValue("Surname", out Control? surnameControl) && surnameControl is TextBox surnameTextBox)
                        surnameTextBox.Text = driver.Surname;
                    if (controls.TryGetValue("EmployeeNo", out Control? empNoControl) && empNoControl is TextBox empNoTextBox)
                        empNoTextBox.Text = driver.EmployeeNo;
                    if (controls.TryGetValue("LicenseType", out Control? licenseControl) && licenseControl is ComboBox licenseComboBox)
                        licenseComboBox.SelectedItem = driver.LicenseType.ToString();
                    if (controls.TryGetValue("Availability", out Control? availControl) && availControl is ComboBox availComboBox)
                        availComboBox.SelectedItem = driver.Availability.ToString();
                },
                CreateFromForm = (controls) =>
                {
                    int DriverID = controls.TryGetValue("DriverID", out Control? driverIdControl) && driverIdControl is TextBox driverIdTextBox && int.TryParse(driverIdTextBox.Text, out int id) ? id : 0;
                    string name = controls.TryGetValue("Name", out Control? nameControl) && nameControl is TextBox nameTextBox ? nameTextBox.Text : string.Empty;
                    string surname = controls.TryGetValue("Surname", out Control? surnameControl) && surnameControl is TextBox surnameTextBox ? surnameTextBox.Text : string.Empty;
                    string employeeNo = controls.TryGetValue("EmployeeNo", out Control? empNoControl) && empNoControl is TextBox empNoTextBox ? empNoTextBox.Text : string.Empty;
                    LicenseType licenseType = controls.TryGetValue("LicenseType", out Control? licenseControl) && licenseControl is ComboBox licenseComboBox && licenseComboBox.SelectedItem != null
                        ? (LicenseType)Enum.Parse(typeof(LicenseType), licenseComboBox.SelectedItem.ToString()!)
                        : LicenseType.Code8; // Default value
                    bool availability = controls.TryGetValue("Availability", out Control? availControl)
                    && availControl is ComboBox availComboBox && availComboBox.SelectedItem != null
                    && bool.Parse(availComboBox.SelectedItem.ToString()!); // Default value

                    return new DriversDTO(DriverID, name, surname, employeeNo, licenseType, availability);
                },
                GetDefaultValue = column => column.Name switch
                {
                    "Availability" => true, // Overriding default bool being false
                    _ => column.SqlType == SqlDbType.Bit ? false : null
                }

            };
        }
    }

}
