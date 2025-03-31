using System.Data;
using Microsoft.Data.SqlClient;

namespace StartSmartDeliveryForm.DataLayer.DAOs
{
    public interface IDAO<T> where T : class
    {
        string TableName { get; }
        Task<DataTable?> GetRecordsAtPageAsync(int PageNumber, CancellationToken cancellationToken = default);
        Task<DataTable?> GetAllRecordsAsync(CancellationToken cancellationToken = default);
        Task<int> GetRecordCountAsync(CancellationToken cancellationToken = default);
        Task<DataTable> GetRecordByPKAsync(int PkID, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default);
        Task<int> InsertRecordAsync(T Entity, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default);
        Task<bool> UpdateRecordAsync(T Entity, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteRecordAsync(int PkID, SqlConnection? Connection = null, SqlTransaction? Transaction = null, CancellationToken cancellationToken = default);

#if DEBUG
        Task ReseedTable(string TableName, int SeedValue);
#endif
    }
}
