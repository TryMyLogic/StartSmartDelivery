using System.Data;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.Registry;
using StartSmartDeliveryForm.SharedLayer;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;

namespace StartSmartDeliveryForm.DataLayer.Repositories
{
    public class Repository<T>(
           ResiliencePipelineProvider<string> pipelineProvider,
           IConfiguration configuration,
           TableConfig tableConfig,
           ILogger<Repository<T>>? logger = null,
           string? connectionString = null,
           RetryEventService? retryEventService = null) : IRepository<T> where T : class
    {
        private readonly string _connectionString = connectionString ?? configuration["ConnectionStrings:StartSmartDB"]
                ?? throw new InvalidOperationException("Connection string not found.");
        private readonly ILogger<Repository<T>> _logger = logger ?? NullLogger<Repository<T>>.Instance;
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("sql-retry-pipeline");
        private readonly RetryEventService _retryEventService = retryEventService ?? new RetryEventService();
        private readonly TableConfig _tableConfig = tableConfig ?? throw new ArgumentNullException(nameof(tableConfig));

        public string TableName => _tableConfig.TableName;

        public async Task<DataTable?> GetRecordsAtPageAsync(int Page, CancellationToken cancellationToken = default)
        {
            string Query = $@"
            SELECT * FROM {_tableConfig.TableName}
            ORDER BY {_tableConfig.PrimaryKey}
            OFFSET @Offset ROWS 
            FETCH NEXT @PageLimit ROWS ONLY;";

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
            string Query = $"SELECT * FROM {_tableConfig.TableName};";
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
            string Query = $"SELECT COUNT({_tableConfig.PrimaryKey}) FROM {_tableConfig.TableName}";
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
            string Query = $"SELECT {columns} FROM {_tableConfig.TableName} WHERE {_tableConfig.PrimaryKey} = @PkId";
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

        public async Task<int> InsertRecordAsync(T Entity, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            IEnumerable<string> insertColumns = _tableConfig.Columns.Where(c => !c.IsIdentity).Select(c => c.Name);
            string columns = string.Join(", ", insertColumns);
            string parameters = string.Join(", ", insertColumns.Select(c => $"@{c}"));
            string Query = $@"
            INSERT INTO {_tableConfig.TableName} ({columns})
            VALUES ({parameters});
            SELECT CAST(SCOPE_IDENTITY() AS int);";
            int newId = -1;

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
                        if (_tableConfig.MapInsertParameters == null)
                            throw new InvalidOperationException("Insert mapping not defined for this table.");

                        _tableConfig.MapInsertParameters(Entity, Command);

                        newId = (int)(await Command.ExecuteScalarAsync(_cancellationToken))!;
                        _logger.LogInformation("Record added successfully with ID: {Id}", newId);
                        _retryEventService.OnRetrySuccessOccurred();
                    }
                }, cancellationToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError("An error occurred while accessing the database: {ErrorMessage}", ex.Message);
                return -1;
            }
            finally
            {
                if (shouldCloseConnection) await Connection.CloseAsync();
            }

            return newId;
        }

        public async Task<bool> UpdateRecordAsync(T Entity, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            IEnumerable<string> updateColumns = _tableConfig.Columns.Where(c => !c.IsIdentity).Select(c => $"{c.Name} = @{c.Name}");
            string setClause = string.Join(", ", updateColumns);
            string Query = $"UPDATE {_tableConfig.TableName} SET {setClause} WHERE {_tableConfig.PrimaryKey} = @{_tableConfig.PrimaryKey}";

            bool shouldCloseConnection = false;
            if (Connection == null)
            {
                Connection = new SqlConnection(_connectionString);
                shouldCloseConnection = true;
            }

            bool success = false;
            try
            {
                await _pipeline.ExecuteAsync(async _cancellationToken =>
                {
                    await Connection.OpenAsync(_cancellationToken);
                    using (SqlCommand Command = Transaction != null ? new(Query, Connection, Transaction) : new(Query, Connection))
                    {
                        if (_tableConfig.MapUpdateParameters == null)
                            throw new InvalidOperationException("Update mapping not defined for this table.");

                        _tableConfig.MapUpdateParameters(Entity, Command);

                        int RowsAffected = await Command.ExecuteNonQueryAsync(_cancellationToken);
                        if (RowsAffected > 0)
                        {
                            _logger.LogInformation("Record updated successfully with ID: {Id}", properties.First(p => p.Name == _tableConfig.PrimaryKey).GetValue(Entity));
                            success = true;
                        }
                        else
                        {
                            _logger.LogWarning("No record was found with ID: {Id}", properties.First(p => p.Name == _tableConfig.PrimaryKey).GetValue(Entity));
                            success &= false;
                        }
                        _retryEventService.OnRetrySuccessOccurred();
                        return success;
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
