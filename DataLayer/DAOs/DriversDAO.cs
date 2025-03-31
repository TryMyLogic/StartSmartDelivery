﻿using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.Registry;
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
    public class DriversDAO(ResiliencePipelineProvider<string> pipelineProvider, IConfiguration configuration, ILogger<DriversDAO>? logger = null, string? connectionString = null, RetryEventService? retryEventService = null) : IDAO<DriversDTO>
    {
        private readonly string _connectionString = connectionString ?? configuration["ConnectionStrings:StartSmartDB"]
                           ?? throw new InvalidOperationException("Connection string not found.");
        private readonly ILogger<DriversDAO> _logger = logger ?? NullLogger<DriversDAO>.Instance;
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("sql-retry-pipeline");
        private readonly RetryEventService _retryEventService = retryEventService ?? new RetryEventService();

        public string TableName => "Drivers";

        public async Task<DataTable?> GetRecordsAtPageAsync(int Page, CancellationToken cancellationToken = default)
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
                }, cancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return null;
            }

            return Dt;
        }

        public async Task<DataTable?> GetAllRecordsAsync(CancellationToken cancellationToken = default)
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
                }, cancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return null;
            }

            return Dt;
        }

        public async Task<int> GetRecordCountAsync(CancellationToken cancellationToken = default)
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
                }, cancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return recordsCount; // Return 0 pages if an error occurs
            }

            return recordsCount;
        }

        public async Task<DataTable> GetRecordByPKAsync(int PkID, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default)
        {
            string Query = "SELECT DriverID, Name, Surname, EmployeeNo, LicenseType, Availability FROM Drivers WHERE DriverID = @DriverID";
            DataTable driverDataTable = new();

            bool ShouldCloseCon = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(_connectionString);
                ShouldCloseCon = true;
            }

            try
            {
                await _pipeline.ExecuteAsync(async (_cancellationToken) =>
                {
                    await Connection.OpenAsync(_cancellationToken);
                    using (SqlCommand Command = Transaction != null ? new SqlCommand(Query, Connection, Transaction) : new SqlCommand(Query, Connection))
                    {
                        Command.Parameters.AddWithValue("@DriverID", PkID);

                        using (SqlDataReader reader = await Command.ExecuteReaderAsync(_cancellationToken))
                        {
                            driverDataTable.Load(reader);
                        }
                    }
                }, cancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
            }
            finally
            {
                if (ShouldCloseCon) await Connection.CloseAsync();
            }

            return driverDataTable;
        }

        public async Task<int> InsertRecordAsync(DriversDTO Driver, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default)
        {
            string Query = @"
            INSERT INTO Drivers (Name, Surname, EmployeeNo, LicenseType, Availability) 
            VALUES (@Name, @Surname, @EmployeeNo, @LicenseType, @Availability);
            SELECT CAST(SCOPE_IDENTITY() AS int);"; // Get the new DriverID
            int newDriverId = -1;

            bool ShouldCloseCon = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(_connectionString);
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
                }, cancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return -1; // Indicates an error occurred
            }
            finally
            {
                if (ShouldCloseCon) await Connection.CloseAsync();
            }
            return newDriverId;
        }


        public async Task<bool> UpdateRecordAsync(DriversDTO driver, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default)
        {
            string Query = "UPDATE Drivers SET Name = @Name, Surname = @Surname, EmployeeNo = @EmployeeNo, LicenseType = @LicenseType, Availability = @Availability WHERE DriverID = @DriverID;";

            bool ShouldCloseCon = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(_connectionString);
                ShouldCloseCon = true;
            }

            bool success = false;
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

                        if (RowsAffected > 0)
                        {
                            _logger.LogInformation("Driver updated successfully with ID: {DriverID}", driver.DriverID);
                            success = true;
                        }
                        else
                        {
                            _logger.LogWarning("No driver was found with ID: {DriverID}, update not performed", driver.DriverID);
                            success &= false;
                        }
                    }
                }, cancellationToken);
                return success;
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return false;
            }
            finally
            {
                if (ShouldCloseCon) await Connection.CloseAsync();
            }
        }

        public async Task<bool> DeleteRecordAsync(int DriverID, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default)
        {
            string Query = "DELETE FROM Drivers WHERE DriverID = @DriverID";
            bool ShouldCloseCon = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(_connectionString);
                ShouldCloseCon = true;
            }

            bool success = false;
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
                            success = true;
                        }
                        else
                        {
                            _logger.LogWarning("No driver was found with ID: {DriverID}, delete not performed", DriverID);
                            success = false;
                        }
                    }
                }, cancellationToken);
                return success;
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return success = false;
            }
            finally
            {
                if (ShouldCloseCon) await Connection.CloseAsync();
            }
        }

        public async Task<int> GetEmployeeNoCountAsync(string EmployeeNo, CancellationToken cancellationToken = default)
        {
            string Query = "SELECT COUNT(*) FROM Drivers WHERE EmployeeNo = @EmployeeNo";
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
                    }, cancellationToken);
                }
                catch (SqlException ex)
                {
                    _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                    return 1; // Assume it's not unique on error
                }
                return Result;
            }
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
