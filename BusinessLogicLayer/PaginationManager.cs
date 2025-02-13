using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.BusinessLogicLayer
{
    public class PaginationManager
    {
        private readonly DriversDAO _driversDAO;
        private readonly int _recordsPerPage = GlobalConstants.s_recordLimit;
        private readonly string _tableName;

        // Uses auto property:
        public int CurrentPage { get; private set; } = 1;
        public int TotalPages { get; private set; } = 1;
        public int RecordCount { get; private set; } = 0;

        public PaginationManager(string tableName, DriversDAO driversDAO)
        {
            _driversDAO = driversDAO;
            _tableName = tableName;
            RecordCount = GetTotalRecordCount();  // This will use _driversDAO
            TotalPages = (int)Math.Ceiling((double)RecordCount / _recordsPerPage);
        }

        public event Func<int, Task>? PageChanged;
        public async Task EmitPageChanged()
        {
            await PageChanged?.Invoke(CurrentPage)!;
        }

        private int GetTotalRecordCount()
        {
            if (_tableName == "Drivers")
            {
                return _driversDAO.GetRecordCount();
            }
            else
            {
                throw new InvalidOperationException("Total record count not supported for this table");
            }
        }

        public async Task GoToFirstPage()
        {
            CurrentPage = 1;
            await EmitPageChanged();
        }

        public async Task GoToPreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await EmitPageChanged();
            }
        }

        public async Task GoToNextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await EmitPageChanged();
            }
        }

        public async Task GoToLastPage()
        {
            CurrentPage = TotalPages;
            await EmitPageChanged();
        }

        public async Task GoToPage(int page)
        {
            CurrentPage = Math.Clamp(page, 1, TotalPages);
            await EmitPageChanged();
        }

        public void UpdateRecordCount(int recordCount)
        {
            RecordCount = recordCount;
            TotalPages = (int)Math.Ceiling((double)RecordCount / _recordsPerPage);
        }

        public async Task EnsureValidPage()
        {
            if (CurrentPage > TotalPages)
            {
                await GoToLastPage();
            }
            else
            {
                await GoToPage(CurrentPage);
            }
        }
    }
}
