using System.Data;
using Microsoft.Data.SqlClient;

namespace StartSmartDeliveryForm.DataLayer.Repositories
{
    public interface IRepository
    {
        string TableName { get; }
        Task<DataTable?> GetRecordsAtPageAsync(int PageNumber, CancellationToken cancellationToken = default);
        Task<DataTable?> GetAllRecordsAsync(CancellationToken cancellationToken = default);
        Task<int> GetRecordCountAsync(CancellationToken cancellationToken = default);
        Task<DataTable> GetRecordByPKAsync(int PkId, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default);
    }

    public interface IRepository<T> : IRepository where T : class
    {
        Task<int> InsertRecordAsync(T Entity, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default);
        Task<bool> UpdateRecordAsync(T Entity, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteRecordAsync(int PkId, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default);
        Task<int> GetFieldCountAsync(string FieldName, string Value, CancellationToken cancellationToken = default);

#if DEBUG
        Task ReseedTableAsync(int SeedValue);
#endif
    }
}
