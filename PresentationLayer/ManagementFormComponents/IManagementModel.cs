﻿using System.Data;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.SharedLayer.EventDelegates;

namespace StartSmartDeliveryForm.PresentationLayer.ManagementFormComponents
{
    public interface IManagementModel<T> where T : class
    {

        public event EventHandler? PageChanged;
        DataTable DgvTable { get; }
        PaginationManager<T> PaginationManager { get; }
        Task InitializeAsync();
        void ApplyFilter(object? sender, SearchRequestEventArgs e);
        Task AddRecordAsync(T entity);
        Task UpdateRecordAsync(T entity);
        Task DeleteRecordAsync(int rowIndex);
        Task FetchAndBindRecordsAtPageAsync();
        event MessageBoxEventDelegate? DisplayErrorMessage;
        T GetEntityFromRow(DataGridViewRow selectedRow);
    }
}
