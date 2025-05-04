using System.Data;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.Registry;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.SharedLayer;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;

namespace StartSmartDeliveryForm.DataLayer.Repositories
{
    public class DeliveriesRepository(
            ResiliencePipelineProvider<string> pipelineProvider,
            IConfiguration configuration,
            ILogger<Repository<DeliveriesDTO>>? logger = null,
            string? connectionString = null,
            RetryEventService? retryEventService = null) : IRepository<DeliveriesDTO>
    {
        private readonly string _connectionString = connectionString ?? configuration["ConnectionStrings:StartSmartDB"]
                ?? throw new InvalidOperationException("Connection string not found.");
        private readonly ILogger<Repository<DeliveriesDTO>> _logger = logger ?? NullLogger<Repository<DeliveriesDTO>>.Instance;
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("sql-retry-pipeline");
        private readonly RetryEventService _retryEventService = retryEventService ?? new RetryEventService();
        private readonly TableConfig _tableConfig = TableConfigResolver.Resolve<DeliveriesDTO>();

        public string TableName => _tableConfig.TableName;

        public async Task<DataTable?> GetRecordsAtPageAsync(int Page, CancellationToken cancellationToken = default)
        {
            string Query = @"
            SELECT 
            dt.TaskID, dt.OrderNumber, dt.CustomerCode, dt.Name, dt.Telephone, dt.Cellphone, dt.Email, 
            dt.Address, dt.Product, dt.Amount, dt.PaymentMethod, dt.Notes, dt.ReceivedTimestamp, 
            d.DispatchTimestamp, CONCAT(dr.Name, ' ', dr.Surname) AS AssignedDriver
            FROM DeliveryTask dt
            INNER JOIN Delivery d ON dt.TaskID = d.TaskID
            INNER JOIN Drivers dr ON d.DriverID = dr.DriverID
            ORDER BY dt.TaskID
            OFFSET @Offset ROWS
            FETCH NEXT @PageLimit ROWS ONLY";

            int Offset = (Page - 1) * GlobalConstants.s_recordLimit;
            DataTable Dt = new();

            try
            {
                await _pipeline.ExecuteAsync(async _cancellationToken =>
                {
                    await using (SqlConnection Connection = new(_connectionString))
                    {
                        await Connection.OpenAsync(_cancellationToken);
                        using (SqlCommand Command = new(Query, Connection))
                        {
                            Command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = Offset });
                            Command.Parameters.Add(new SqlParameter("@PageLimit", SqlDbType.Int) { Value = GlobalConstants.s_recordLimit });

                            using (SqlDataReader reader = await Command.ExecuteReaderAsync(_cancellationToken))
                            {
                                Dt.Load(reader);
                            }

                            DataColumn[] primaryKeyColumns = [Dt.Columns[_tableConfig.PrimaryKey]!];
                            Dt.PrimaryKey = primaryKeyColumns;
                        }
                    }
                    _retryEventService.OnRetrySuccessOccurred();
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
            string Query = @"
            SELECT 
            dt.TaskID, dt.OrderNumber, dt.CustomerCode, dt.Name, dt.Telephone, dt.Cellphone, dt.Email, 
            dt.Address, dt.Product, dt.Amount, dt.PaymentMethod, dt.Notes, dt.ReceivedTimestamp, 
            d.DispatchTimestamp, CONCAT(dr.Name, ' ', dr.Surname) AS AssignedDriver
            FROM DeliveryTask dt
            INNER JOIN Delivery d ON dt.TaskID = d.TaskID
            INNER JOIN Drivers dr ON d.DriverID = dr.DriverID
            ORDER BY dt.TaskID";

            DataTable Dt = new();

            try
            {
                await _pipeline.ExecuteAsync(async _cancellationToken =>
                {
                    await using (SqlConnection Connection = new(_connectionString))
                    {
                        await Connection.OpenAsync(_cancellationToken);
                        using (SqlCommand Command = new(Query, Connection))
                        {
                            using (SqlDataReader reader = await Command.ExecuteReaderAsync(_cancellationToken))
                            {
                                Dt.Load(reader);

                                if (Dt.Columns.Contains(_tableConfig.PrimaryKey))
                                {
                                    DataColumn[] primaryKeyColumns = [Dt.Columns[_tableConfig.PrimaryKey]!];
                                    Dt.PrimaryKey = primaryKeyColumns;
                                }
                                else
                                {
                                    throw new InvalidOperationException($"The DataTable must contain the '{_tableConfig.PrimaryKey}' column.");
                                }
                            }
                        }
                        _retryEventService.OnRetrySuccessOccurred();
                    }
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
            string Query = $"SELECT COUNT(*) FROM DeliveryTask";
            int recordCount = 0;

            try
            {
                await _pipeline.ExecuteAsync(async _cancellationToken =>
                {
                    using (SqlConnection connection = new(_connectionString))
                    {
                        await connection.OpenAsync(_cancellationToken);
                        using (SqlCommand Command = new(Query, connection))
                        {
                            object? result = await Command.ExecuteScalarAsync(_cancellationToken);
                            recordCount = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                        }
                    }
                    _retryEventService.OnRetrySuccessOccurred();
                }, cancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
            }

            return recordCount;
        }

        public async Task<DataTable> GetRecordByPKAsync(int pkId, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default)
        {
            string columns = string.Join(", ", _tableConfig.Columns.Select(c => c.Name));
            string Query = @"
            SELECT 
            dt.TaskID, dt.OrderNumber, dt.CustomerCode, dt.Name, dt.Telephone, dt.Cellphone, dt.Email, 
            dt.Address, dt.Product, dt.Amount, dt.PaymentMethod, dt.Notes, dt.ReceivedTimestamp, 
            d.DispatchTimestamp, CONCAT(dr.Name, ' ', dr.Surname) AS AssignedDriver
            FROM DeliveryTask dt
            INNER JOIN Delivery d ON dt.TaskID = d.TaskID
            INNER JOIN Drivers dr ON d.DriverID = dr.DriverID
            WHERE dt.TaskID = @TaskID";

            DataTable dt = new();

            bool shouldCloseConnection = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(_connectionString);
                shouldCloseConnection = true;
            }

            try
            {
                await _pipeline.ExecuteAsync(async _cancellationToken =>
                {
                    await Connection.OpenAsync(_cancellationToken);
                    using (SqlCommand Command = Transaction != null ? new(Query, Connection, Transaction) : new(Query, Connection))
                    {
                        Command.Parameters.AddWithValue("@PkId", pkId);

                        using (SqlDataReader reader = await Command.ExecuteReaderAsync(_cancellationToken))
                        {
                            dt.Load(reader);
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
                if (shouldCloseConnection) await Connection.CloseAsync();
            }

            return dt;
        }

        public async Task<int> InsertRecordAsync(DeliveriesDTO Entity, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default)
        {
            PropertyInfo[] properties = typeof(DeliveriesDTO).GetProperties();
            IEnumerable<string> insertColumns = _tableConfig.Columns.Where(c => !c.IsIdentity).Select(c => c.Name);
            string columns = string.Join(", ", insertColumns);
            string parameters = string.Join(", ", insertColumns.Select(c => $"@{c}"));
            string Query = @"
            INSERT INTO DeliveryTask (
            OrderNumber, CustomerCode, Name, Telephone, Cellphone, Email, Address, Product, 
            Amount, PaymentMethod, Notes, ReceivedTimestamp
            )
            OUTPUT INSERTED.TaskID
            VALUES (
            @OrderNumber, @CustomerCode, @Name, @Telephone, @Cellphone, @Email, @Address, @Product, 
            @Amount, @PaymentMethod, @Notes, @ReceivedTimestamp
            )";
            int newId = -1;

            bool shouldCloseConnection = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(_connectionString);
                shouldCloseConnection = true;
            }

            SqlTransaction localTransaction = Transaction ?? Connection.BeginTransaction();
            try
            {
                await _pipeline.ExecuteAsync(async _cancellationToken =>
                {
                    await Connection.OpenAsync(_cancellationToken);
                    using (SqlCommand Command = localTransaction != null ? new(Query, Connection, localTransaction) : new(Query, Connection))
                    {
                        if (_tableConfig.MapInsertParameters == null)
                            throw new InvalidOperationException("Insert mapping not defined for this table.");

                        _tableConfig.MapInsertParameters(Entity, Command);

                        newId = (int)(await Command.ExecuteScalarAsync(_cancellationToken))!;
                        _logger.LogInformation("Record added successfully with ID: {Id}", newId);

                        string[] driverNames = Entity.AssignedDriver.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        string driverQuery = @"
                        SELECT DriverID
                        FROM Drivers
                        WHERE Name = @Name AND Surname = @Surname";
                        using (SqlCommand driverCommand = new(driverQuery, Connection, localTransaction))
                        {
                            driverCommand.Parameters.AddWithValue("@Name", driverNames[0]);
                            driverCommand.Parameters.AddWithValue("@Surname", driverNames.Length > 1 ? driverNames[1] : "");
                            object? driverIdResult = await driverCommand.ExecuteScalarAsync(_cancellationToken);
                            if (driverIdResult == null)
                            {
                                _logger.LogError("Driver not found for AssignedDriver: {AssignedDriver}", Entity.AssignedDriver);
                                throw new InvalidOperationException($"Driver not found for {Entity.AssignedDriver}");
                            }
                            int driverId = (int)driverIdResult;

                            string deliveryQuery = @"
                                INSERT INTO Delivery (TaskID, DriverID, DispatchTimestamp)
                                VALUES (@TaskID, @DriverID, @VehicleID, @DispatchTimestamp)";
                            using (SqlCommand deliveryCommand = new(deliveryQuery, Connection, localTransaction))
                            {
                                deliveryCommand.Parameters.AddWithValue("@TaskID", newId);
                                deliveryCommand.Parameters.AddWithValue("@DriverID", driverId);
                                deliveryCommand.Parameters.AddWithValue("@DispatchTimestamp", Entity.DispatchTimestamp.HasValue ? Entity.DispatchTimestamp.Value : DBNull.Value);
                                await deliveryCommand.ExecuteNonQueryAsync(_cancellationToken);
                                _logger.LogInformation("Delivery record added successfully for TaskID: {TaskID}", newId);
                            }

                        }
                        localTransaction!.Commit();
                        _retryEventService.OnRetrySuccessOccurred();
                    }
                }, cancellationToken);
            }
            catch (SqlException ex)
            {
                localTransaction?.Rollback();
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return -1;
            }
            catch (Exception ex)
            {
                localTransaction?.Rollback();
                _logger.LogError(ex, "Unexpected error occurred while inserting delivery record: {ErrorMessage}", ex.Message);
                return -1;
            }
            finally
            {
                if (shouldCloseConnection) await Connection.CloseAsync();
            }

            return newId;
        }

        public async Task<bool> UpdateRecordAsync(DeliveriesDTO Entity, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default)
        {
            PropertyInfo[] properties = typeof(DeliveriesDTO).GetProperties();
            IEnumerable<string> updateColumns = _tableConfig.Columns.Where(c => !c.IsIdentity).Select(c => $"{c.Name} = @{c.Name}");
            string setClause = string.Join(", ", updateColumns);
            string Query = @"
            UPDATE DeliveryTask
            SET 
            OrderNumber = @OrderNumber, CustomerCode = @CustomerCode, Name = @Name, 
            Telephone = @Telephone, Cellphone = @Cellphone, Email = @Email, Address = @Address, 
            Product = @Product, Amount = @Amount, PaymentMethod = @PaymentMethod, Notes = @Notes, 
            ReceivedTimestamp = @ReceivedTimestamp
            WHERE TaskID = @TaskID";

            bool shouldCloseConnection = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(_connectionString);
                shouldCloseConnection = true;
            }

            SqlTransaction? localTransaction = Transaction;
            bool success = false;
            try
            {
                await _pipeline.ExecuteAsync(async _cancellationToken =>
                {
                    await Connection.OpenAsync(_cancellationToken);

                    localTransaction ??= Connection.BeginTransaction();

                    using (SqlCommand Command = localTransaction != null ? new(Query, Connection, localTransaction) : new(Query, Connection))
                    {
                        if (_tableConfig.MapUpdateParameters == null)
                            throw new InvalidOperationException("Update mapping not defined for this table.");

                        _tableConfig.MapUpdateParameters(Entity, Command);

                        int RowsAffected = await Command.ExecuteNonQueryAsync(_cancellationToken);
                        if (RowsAffected > 0)
                        {
                            _logger.LogInformation("Record updated successfully with ID: {Id}", properties.First(p => p.Name == _tableConfig.PrimaryKey).GetValue(Entity));

                            string[] driverNames = Entity.AssignedDriver.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            string driverQuery = @"
                            SELECT DriverID
                            FROM Drivers
                            WHERE Name = @Name AND Surname = @Surname";
                            using (SqlCommand driverCommand = new(driverQuery, Connection, localTransaction))
                            {
                                driverCommand.Parameters.AddWithValue("@Name", driverNames[0]);
                                driverCommand.Parameters.AddWithValue("@Surname", driverNames.Length > 1 ? driverNames[1] : "");
                                object? driverIdResult = await driverCommand.ExecuteScalarAsync(_cancellationToken);
                                if (driverIdResult == null)
                                {
                                    _logger.LogError("Driver not found for AssignedDriver: {AssignedDriver}", Entity.AssignedDriver);
                                    throw new InvalidOperationException($"Driver not found for {Entity.AssignedDriver}");
                                }
                                int driverId = (int)driverIdResult;

                                string deliveryQuery = @"
                                    UPDATE Delivery
                                    SET DriverID = @DriverID, DispatchTimestamp = @DispatchTimestamp
                                    WHERE TaskID = @TaskID";
                                using (SqlCommand deliveryCommand = new(deliveryQuery, Connection, localTransaction))
                                {
                                    deliveryCommand.Parameters.AddWithValue("@TaskID", Entity.TaskID);
                                    deliveryCommand.Parameters.AddWithValue("@DriverID", driverId);
                                    deliveryCommand.Parameters.AddWithValue("@DispatchTimestamp", Entity.DispatchTimestamp.HasValue ? Entity.DispatchTimestamp.Value : DBNull.Value);
                                    int deliveryRowsAffected = await deliveryCommand.ExecuteNonQueryAsync(_cancellationToken);
                                    if (deliveryRowsAffected > 0)
                                    {
                                        _logger.LogInformation("Delivery record updated successfully for TaskID: {TaskID}", Entity.TaskID);
                                        success = true;
                                    }
                                    else
                                    {
                                        _logger.LogWarning("No Delivery record found for TaskID: {TaskID}", Entity.TaskID);
                                        success = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning("No record was found with ID: {Id}", properties.First(p => p.Name == _tableConfig.PrimaryKey).GetValue(Entity));
                            success &= false;
                        }

                        localTransaction!.Commit();
                        _retryEventService.OnRetrySuccessOccurred();
                        return success;
                    }
                }, cancellationToken);
                return success;
            }
            catch (SqlException ex)
            {
                localTransaction?.Rollback();
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                localTransaction?.Rollback();
                _logger.LogError(ex, "Unexpected error occurred while updating delivery record: {ErrorMessage}", ex.Message);
                return false;
            }
            finally
            {
                if (shouldCloseConnection) await Connection.CloseAsync();
            }
        }

        public async Task<bool> DeleteRecordAsync(int PkId, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default)
        {
            string Query = $"DELETE FROM {_tableConfig.TableName} WHERE {_tableConfig.PrimaryKey} = @PkId";

            bool shouldCloseConnection = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(_connectionString);
                shouldCloseConnection = true;
            }

            try
            {
                await _pipeline.ExecuteAsync(async _cancellationToken =>
                {
                    await Connection.OpenAsync(_cancellationToken);
                    using (SqlCommand Command = Transaction != null ? new(Query, Connection, Transaction) : new(Query, Connection))
                    {
                        Command.Parameters.AddWithValue("@PkId", PkId);

                        int rowsAffected = await Command.ExecuteNonQueryAsync(_cancellationToken);
                        if (rowsAffected > 0)
                        {
                            _logger.LogInformation("{RowsAffected} row(s) deleted with ID: {Id}", rowsAffected, PkId);
                        }
                        else
                        {
                            _logger.LogWarning("No record found with ID: {Id}", PkId);
                        }
                        _retryEventService.OnRetrySuccessOccurred();
                        return rowsAffected > 0;
                    }
                }, cancellationToken);
                return true;
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return false;
            }
            finally
            {
                if (shouldCloseConnection) await Connection.CloseAsync();
            }
        }

        public async Task<int> GetFieldCountAsync(string FieldName, string Value, CancellationToken cancellationToken = default)
        {
            string query = $"SELECT COUNT(*) FROM {_tableConfig.TableName} WHERE {FieldName} = @Value";
            int count = 0;

            try
            {
                await _pipeline.ExecuteAsync(async _cancellationToken =>
                {
                    using (SqlConnection connection = new(_connectionString))
                    {
                        await connection.OpenAsync(_cancellationToken);
                        using (SqlCommand Command = new(query, connection))
                        {
                            Command.Parameters.AddWithValue("@Value", Value);
                            count = (int)(await Command.ExecuteScalarAsync(_cancellationToken))!;
                        }
                    }
                    _retryEventService.OnRetrySuccessOccurred();
                }, cancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("Error checking uniqueness for {FieldName}: {ErrorMessage}", FieldName, ex.Message);
                return 1;
            }

            return count;
        }

#if DEBUG
        public async Task ReseedTableAsync(int SeedValue)
        {
            string query = $"DBCC CHECKIDENT ('{_tableConfig.TableName}', RESEED, {SeedValue})";

            using (SqlConnection connection = new(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand Command = new(query, connection))
                {
                    await Command.ExecuteNonQueryAsync();
                }
            }
        }
#endif
    }
}
