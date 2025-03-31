using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using StartSmartDeliveryForm.DataLayer.DTOs;

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
