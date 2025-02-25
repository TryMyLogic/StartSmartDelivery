using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.Registry;
using StartSmartDeliveryForm.BusinessLogicLayer;
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
    public class DriversDAO(ResiliencePipelineProvider<string> pipelineProvider, IConfiguration configuration, ILogger<DriversDAO>? logger = null, string? connectionString = null, RetryEventService? retryEventService = null)
    {

        private readonly string _connectionString = connectionString ?? configuration["ConnectionStrings:StartSmartDB"]
                               ?? throw new InvalidOperationException("Connection string not found.");
        private readonly ILogger<DriversDAO> _logger = logger ?? NullLogger<DriversDAO>.Instance;
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("sql-retry-pipeline");
        private readonly RetryEventService _retryEventService = retryEventService ?? new RetryEventService(); // New retry service ensures no "success" events are emitted.
        public async Task<DataTable?> GetDriversAtPageAsync(int Page, CancellationToken CancellationToken)
        {
            string Query = @"
            SELECT * FROM Drivers
            ORDER BY DriverID
            OFFSET @Offset ROWS 
            FETCH NEXT @Pagelimit ROWS ONLY;";

            int Offset = (Page - 1) * GlobalConstants.s_recordLimit;
            DataTable Dt = new();

            try
            {
                // The inner token in the lambda will/should be different if you're using strategies like timeout and hedging
                await _pipeline.ExecuteAsync(async (_cancellationToken) =>
                {
                    await using (SqlConnection Connection = new(_connectionString))
                    {
                        await Connection.OpenAsync(_cancellationToken);
                        using (SqlCommand Command = new(Query, Connection))
                        {
                            Command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = Offset });
                            Command.Parameters.Add(new SqlParameter("@Pagelimit", SqlDbType.Int) { Value = GlobalConstants.s_recordLimit });

                            using (SqlDataReader Reader = await Command.ExecuteReaderAsync(_cancellationToken))
                            {
                                Dt.Load(Reader);
                            }

                            DataColumn[] PrimaryKeyColumns = [Dt.Columns["DriverID"]!];
                            Dt.PrimaryKey = PrimaryKeyColumns;
                        }
                    }
                    _retryEventService.OnRetrySuccessOccurred(); // Does internally check if a retry has occurred. Else its skipped
                }, CancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex.Message);
                return null;
            }

            return Dt;
        }

        public async Task<DataTable?> GetAllDriversAsync(CancellationToken CancellationToken)
        {
            string Query = @"SELECT * FROM Drivers;";
            DataTable Dt = new();

            try
            {
                await _pipeline.ExecuteAsync(async (_cancellationToken) =>
                {
                    await using (SqlConnection Connection = new(_connectionString))
                    {
                        await Connection.OpenAsync(_cancellationToken);
                        using (SqlCommand Command = new(Query, Connection))
                        {
                            using (SqlDataReader Reader = await Command.ExecuteReaderAsync(_cancellationToken))
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
                    _retryEventService.OnRetrySuccessOccurred(); // Does internally check if a retry has occurred. Else its skipped
                }, CancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex.Message);
                return null;
            }

            return Dt;
        }

        public async Task<int> InsertDriverAsync(DriversDTO Driver, CancellationToken CancellationToken, SqlConnection? Connection = null, SqlTransaction? Transaction = null)
        {
            string Query = @"
            INSERT INTO Drivers (Name, Surname, EmployeeNo, LicenseType, Availability) 
            VALUES (@Name, @Surname, @EmployeeNo, @LicenseType, @Availability);
            SELECT CAST(SCOPE_IDENTITY() AS int);"; // Get the new DriverID
            int newDriverId = -1;

            bool ShouldCloseCon = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(connectionString);
                ShouldCloseCon = true;
            }

            try
            {
                await _pipeline.ExecuteAsync(async (_cancellationToken) =>
                {
                    await Connection.OpenAsync(_cancellationToken);
                    using (SqlCommand Command = Transaction != null ? new SqlCommand(Query, Connection, Transaction) : new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = Driver.Name });
                        Command.Parameters.Add(new SqlParameter("@Surname", SqlDbType.NVarChar, 100) { Value = Driver.Surname });
                        Command.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = Driver.EmployeeNo });
                        Command.Parameters.Add(new SqlParameter("@LicenseType", SqlDbType.Int) { Value = (int)Driver.LicenseType });
                        Command.Parameters.Add(new SqlParameter("@Availability", SqlDbType.Bit) { Value = Driver.Availability });

                        newDriverId = (int)(await Command.ExecuteScalarAsync(_cancellationToken))!;

                        _logger.LogInformation("Driver added successfully with ID: {DriverID}", newDriverId);
                        _retryEventService.OnRetrySuccessOccurred(); // Does internally check if a retry has occurred. Else its skipped
                    }
                }, CancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return -1; // Indicates an error occurred
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex.Message);
                return -1;
            }
            finally
            {
                if (ShouldCloseCon) await Connection.CloseAsync();
            }
            return newDriverId;
        }

        public async Task UpdateDriverAsync(DriversDTO driver, CancellationToken CancellationToken, SqlConnection? Connection = null, SqlTransaction? Transaction = null)
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
                await _pipeline.ExecuteAsync(async (_cancellationToken) =>
                {
                    await Connection.OpenAsync(_cancellationToken);
                    using (SqlCommand Command = Transaction != null ? new SqlCommand(Query, Connection, Transaction) : new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add(new SqlParameter("@DriverID", SqlDbType.Int) { Value = driver.DriverID });
                        Command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = driver.Name });
                        Command.Parameters.Add(new SqlParameter("@Surname", SqlDbType.NVarChar, 100) { Value = driver.Surname });
                        Command.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = driver.EmployeeNo });
                        Command.Parameters.Add(new SqlParameter("@LicenseType", SqlDbType.Int) { Value = (int)driver.LicenseType });
                        Command.Parameters.Add(new SqlParameter("@Availability", SqlDbType.Bit) { Value = driver.Availability });

                        int RowsAffected = await Command.ExecuteNonQueryAsync(_cancellationToken);
                        _logger.LogInformation("Driver updated successfully with ID: {DriverID}", driver.DriverID);
                    }
                }, CancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex.Message);
            }
            finally
            {
                if (ShouldCloseCon) await Connection.CloseAsync();
            }
        }

        public async Task DeleteDriverAsync(int DriverID, CancellationToken CancellationToken, SqlTransaction? Transaction = null, SqlConnection? Connection = null)
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
                await _pipeline.ExecuteAsync(async (_cancellationToken) =>
                {
                    await Connection.OpenAsync(_cancellationToken);
                    using (SqlCommand Command = Transaction != null ? new SqlCommand(Query, Connection, Transaction) : new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.Add(new SqlParameter("@DriverID", SqlDbType.Int) { Value = DriverID });

                        int RowsAffected = await Command.ExecuteNonQueryAsync(_cancellationToken);

                        if (RowsAffected > 0)
                        {
                            _logger.LogInformation("{RowsAffected} row(s) were deleted with the DriverID: {DriverID}", RowsAffected, DriverID);
                        }
                        else
                        {
                            _logger.LogWarning("No rows were deleted. Employee may not exist with DriverID: {DriverID}", DriverID);
                        }
                    }
                }, CancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex.Message);
            }
            finally
            {
                if (ShouldCloseCon) await Connection.CloseAsync();
            }
        }

        public async Task<int> GetEmployeeNoCountAsync(string EmployeeNo, CancellationToken CancellationToken)
        {
            string Query = "SELECT TOP 1 * FROM Drivers WHERE EmployeeNo = @EmployeeNo";
            int Result = 1;
            using (SqlConnection Connection = new(_connectionString))
            {
                try
                {
                    await _pipeline.ExecuteAsync(async (_cancellationToken) =>
                    {
                        await Connection.OpenAsync(_cancellationToken);
                        using (var Command = new SqlCommand(Query, Connection))
                        {
                            Command.CommandType = CommandType.Text; // Since it's a direct SQL query
                            Command.Parameters.Add(new SqlParameter("@EmployeeNo", SqlDbType.NVarChar, 50) { Value = EmployeeNo });

                            object? FoundRow = await Command.ExecuteScalarAsync(_cancellationToken);
                            Result = (FoundRow != null) ? (int)FoundRow : 0;

                            return Result; // 1 if the EmployeeNo exists, 0 if not
                        }
                    }, CancellationToken);
                }
                catch (SqlException ex)
                {
                    _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                    return 1; // Assume it's not unique on error
                }
                catch (Exception ex)
                {
                    _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex.Message);
                    return 1;
                }
                return Result;
            }
        }

        internal async Task<int> GetRecordCountAsync(CancellationToken CancellationToken)
        {
            string Query = "SELECT COUNT(DriverID) FROM Drivers";
            int recordsCount = 0;

            try
            {
                await _pipeline.ExecuteAsync(async (_cancellationToken) =>
                {
                    using (SqlConnection Connection = new(_connectionString))
                    {
                        await Connection.OpenAsync(_cancellationToken);
                        using (SqlCommand Command = new(Query, Connection))
                        {
                            object? result = await Command.ExecuteScalarAsync(_cancellationToken);
                            recordsCount = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                        }
                    }
                    _retryEventService.OnRetrySuccessOccurred(); // Does internally check if a retry has occurred. Else its skipped
                }, CancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return recordsCount; // Return 0 pages if an error occurs
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex.Message);
                return recordsCount;
            }

            return recordsCount;
        }

        public async Task<DataTable> GetDriverByIDAsync(int DriverID, CancellationToken CancellationToken, SqlConnection? Connection = null, SqlTransaction? Transaction = null)
        {
            string Query = "SELECT DriverID, Name, Surname, EmployeeNo, LicenseType, Availability FROM Drivers WHERE DriverID = @DriverID";
            DataTable driverDataTable = new();

            bool ShouldCloseCon = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(connectionString);
                ShouldCloseCon = true;
            }

            try
            {
                await _pipeline.ExecuteAsync(async (_cancellationToken) =>
                {
                    await Connection.OpenAsync(_cancellationToken);
                    using (SqlCommand Command = Transaction != null ? new SqlCommand(Query, Connection, Transaction) : new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.AddWithValue("@DriverID", DriverID);

                        using (SqlDataReader reader = await Command.ExecuteReaderAsync(_cancellationToken))
                        {
                            driverDataTable.Load(reader);
                        }
                    }
                }, CancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred: {ErrorMessage}", ex.Message);
            }
            finally
            {
                if (ShouldCloseCon) await Connection.CloseAsync();
            }

            return driverDataTable;
        }

        // Used only for testing
        #if DEBUG
        public async Task ReseedTable(string TableName, int SeedValue)
        {
            string Query = $"DBCC CHECKIDENT ('{TableName}', RESEED, {SeedValue})";

            using (SqlConnection Connection = new(_connectionString))
            {
                await Connection.OpenAsync();
                using (var Command = new SqlCommand(Query, Connection))
                {
                    await Command.ExecuteNonQueryAsync();
                }
            }
        }
        #endif

    }
}
