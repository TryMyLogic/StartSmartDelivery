using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.DataLayer.DAOs
{
    /*
    DAO:
    Encapsulates the data access operations (like querying, inserting, updating, and deleting data).
    Provides an interface to interact with the data source (such as a database).
    Decouples the data access code from the rest of the application
    */
    public class DriversDAO(IConfiguration configuration, ILogger<DriversDAO> logger, string? connectionString = null)
    {
        private readonly string _connectionString = connectionString ?? configuration["ConnectionStrings:StartSmartDB"]
                               ?? throw new InvalidOperationException("Connection string not found.");
        private readonly ILogger<DriversDAO> _logger = logger;

        public async Task<DataTable?> GetDriversAtPageAsync(int Page)
        {
            string Query = @"
                    SELECT * FROM Drivers
                    ORDER BY DriverID
                    OFFSET @Offset ROWS 
                    FETCH NEXT @Pagelimit ROWS ONLY;";

            int Offset = (Page - 1) * GlobalConstants.s_recordLimit;
            DataTable Dt = new();

            using (SqlConnection Connection = new(_connectionString))
            {
                try
                {
                    await Connection.OpenAsync();
                    using (SqlCommand Command = new(Query, Connection))
                    {
                        Command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = Offset });
                        Command.Parameters.Add(new SqlParameter("@Pagelimit", SqlDbType.Int) { Value = GlobalConstants.s_recordLimit });

                        using (SqlDataReader Reader = await Command.ExecuteReaderAsync())
                        {
                            Dt.Load(Reader);
                        }

                        DataColumn[] PrimaryKeyColumns = [Dt.Columns["DriverID"]!];
                        Dt.PrimaryKey = PrimaryKeyColumns;
                    }
                }
                catch (SqlException ex)
                {
                    _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                    return null;
                }
            }

            return Dt;
        }

        public async Task<DataTable?> GetAllDriversAsync()
        {
            string Query = @"SELECT * FROM Drivers;";
            DataTable Dt = new();

            using (SqlConnection Connection = new(_connectionString))
            {
                try
                {
                    await Connection.OpenAsync();

                    using (SqlCommand Command = new(Query, Connection))
                    {
                        using (SqlDataReader Reader = await Command.ExecuteReaderAsync())
                        {
                            Dt.Load(Reader);

                            if (Dt.Columns.Contains("DriverID"))
                            {
                                DataColumn[] PrimaryKeyColumns = [Dt.Columns["DriverID"]!];
                                Dt.PrimaryKey = PrimaryKeyColumns;
                            }
                            else
                            {
                                throw new InvalidOperationException("The DataTable must contain the 'DriverID' column.");
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                    return null;
                }
            }

            return Dt;
        }

        public async Task<int> InsertDriverAsync(DriversDTO Driver, SqlConnection? Connection = null, SqlTransaction? Transaction = null)
        {
            string Query = @"
            INSERT INTO Drivers (Name, Surname, EmployeeNo, LicenseType, Availability) 
            VALUES (@Name, @Surname, @EmployeeNo, @LicenseType, @Availability);
            SELECT CAST(SCOPE_IDENTITY() AS int);"; // Get the new DriverID

            bool ShouldCloseCon = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(connectionString);
                ShouldCloseCon = true;
            }

            try
            {
                using (SqlCommand Command = Transaction != null ? new SqlCommand(Query, Connection, Transaction) : new SqlCommand(Query, Connection))
                {
                    await Connection.OpenAsync();

                    Command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = Driver.Name });
                    Command.Parameters.Add(new SqlParameter("@Surname", SqlDbType.NVarChar, 100) { Value = Driver.Surname });
                    Command.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = Driver.EmployeeNo });
                    Command.Parameters.Add(new SqlParameter("@LicenseType", SqlDbType.Int) { Value = (int)Driver.LicenseType });
                    Command.Parameters.Add(new SqlParameter("@Availability", SqlDbType.Bit) { Value = Driver.Availability });

                    int newDriverId = (int)(await Command.ExecuteScalarAsync())!;

                    _logger.LogInformation("Driver added successfully with ID: {DriverID}", newDriverId);
                    return newDriverId;
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return -1; //Indicates an error occurred
            }
            finally
            {
                if (ShouldCloseCon) Connection.Close();
            }
        }

        public async Task UpdateDriverAsync(DriversDTO driver, SqlConnection? Connection = null, SqlTransaction? Transaction = null)
        {
            string Query = "UPDATE Drivers SET Name = @Name, Surname = @Surname, EmployeeNo = @EmployeeNo, LicenseType = @LicenseType, Availability = @Availability WHERE DriverID = @DriverID;";

            bool ShouldCloseCon = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(connectionString);
                ShouldCloseCon = true;
            }

            try
            {
                await Connection.OpenAsync();
                using (SqlCommand Command = Transaction != null ? new SqlCommand(Query, Connection, Transaction) : new SqlCommand(Query, Connection))
                {
                    Command.Parameters.Add(new SqlParameter("@DriverID", SqlDbType.Int) { Value = driver.DriverID });
                    Command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = driver.Name });
                    Command.Parameters.Add(new SqlParameter("@Surname", SqlDbType.NVarChar, 100) { Value = driver.Surname });
                    Command.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = driver.EmployeeNo });
                    Command.Parameters.Add(new SqlParameter("@LicenseType", SqlDbType.Int) { Value = (int)driver.LicenseType });
                    Command.Parameters.Add(new SqlParameter("@Availability", SqlDbType.Bit) { Value = driver.Availability });

                    int RowsAffected = await Command.ExecuteNonQueryAsync();
                    _logger.LogInformation("Driver updated successfully with ID: {DriverID}", driver.DriverID);
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
            }
            finally
            {
                if (ShouldCloseCon) await Connection.CloseAsync();
            }
        }

        public async Task DeleteDriverAsync(int DriverID, SqlTransaction? Transaction = null, SqlConnection? Connection = null)
        {
            string Query = "DELETE FROM Drivers WHERE DriverID = @DriverID";
            bool ShouldCloseCon = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(connectionString);
                ShouldCloseCon = true;
            }

            try
            {
                await Connection.OpenAsync();
                using (SqlCommand Command = Transaction != null ? new SqlCommand(Query, Connection, Transaction) : new SqlCommand(Query, Connection))
                {
                    Command.Parameters.Add(new SqlParameter("@DriverID", SqlDbType.Int) { Value = DriverID });

                    int RowsAffected = await Command.ExecuteNonQueryAsync();

                    if (RowsAffected > 0)
                    {
                        _logger.LogInformation("{RowsAffected} row(s) were deleted with the DriverID: {DriverID}", RowsAffected, DriverID);
                    }
                    else
                    {
                        _logger.LogWarning("No rows were deleted. Employee may not exist with DriverID: {DriverID}", DriverID);
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
            }
            finally
            {
                if (ShouldCloseCon) await Connection.CloseAsync();
            }
        }

        public async Task<int> GetEmployeeNoCountAsync(string EmployeeNo)
        {
            string Query = "SELECT TOP 1 * FROM Drivers WHERE EmployeeNo = @EmployeeNo";
            using (SqlConnection Connection = new(_connectionString))
            {
                try
                {
                    await Connection.OpenAsync();
                    using (var Command = new SqlCommand(Query, Connection))
                    {
                        Command.CommandType = CommandType.Text; // Since it's a direct SQL query
                        Command.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = EmployeeNo });

                        object? FoundRow = await Command.ExecuteScalarAsync();
                        int Result = (FoundRow != null) ? (int)FoundRow : 0;

                        return Result; // 1 if the EmployeeNo exists, 0 if not
                    }
                }
                catch (SqlException ex)
                {
                    _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                    return 1; // Assume it's not unique on error
                }
            }
        }

        internal int GetRecordCount()
        {
            int recordsCount = 0;

            string query = "SELECT COUNT(DriverID) FROM Drivers";

            using (SqlConnection connection = new(_connectionString)) // Ensure s_connectionString is defined
            {
                try
                {
                    using (SqlCommand command = new(query, connection))
                    {
                        connection.Open();
                        recordsCount = (int)command.ExecuteScalar(); // Execute the query and get the total count of records
                    }
                }
                catch (SqlException ex)
                {
                    FormConsole.Instance.Log("Error counting records: " + ex.Message);
                    return recordsCount; // Return 0 pages if an error occurs
                }
            }

            return recordsCount;
        }

        public DataTable GetDriverByID(int driverID, SqlConnection? Connection = null, SqlTransaction? Transaction = null)
        {
            DataTable driverDataTable = new();
            string query = "SELECT DriverID, Name, Surname, EmployeeNo, LicenseType, Availability FROM Drivers WHERE DriverID = @DriverID";

            bool ShouldCloseCon = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(connectionString);
                Connection.Open();
                ShouldCloseCon = true;
            }

            try
            {
                using (SqlCommand Command = Transaction != null ? new SqlCommand(query, Connection, Transaction) : new SqlCommand(query, Connection))
                {
                    Command.Parameters.AddWithValue("@DriverID", driverID);

                    using (SqlDataAdapter adapter = new(Command)) // Pass command with/without transaction
                    {
                        adapter.Fill(driverDataTable);
                    }
                }
            }
            catch (SqlException ex)
            {
                FormConsole.Instance.Log("Error retrieving driver by ID: " + ex.Message);
            }
            finally
            {
                if (ShouldCloseCon) Connection.Close();
            }

            return driverDataTable;
        }
    }
}
