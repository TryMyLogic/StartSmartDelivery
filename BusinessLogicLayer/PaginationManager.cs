﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog.Core;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.BusinessLogicLayer
{
    public class PaginationManager
    {
        private readonly DriversDAO _driversDAO;
        private readonly int _recordsPerPage = GlobalConstants.s_recordLimit;
        private readonly string _tableName;
        private readonly ILogger<PaginationManager> _logger;

        // Uses auto property:
        public int CurrentPage { get; private set; } = 1;
        public int TotalPages { get; private set; } = 1;
        public int RecordCount { get; private set; } = 0;

        internal PaginationManager(string tableName, DriversDAO driversDAO, ILogger<PaginationManager>? logger = null)
        {
            _tableName = tableName;
            _driversDAO = driversDAO;
            _logger = logger ?? NullLogger<PaginationManager>.Instance;
        }

        // Async Factory Pattern
        public static async Task<PaginationManager> CreateAsync(string tableName, DriversDAO driversDAO, ILogger<PaginationManager>? logger = null)
        {
            PaginationManager paginationManager = new(tableName, driversDAO, logger);
            paginationManager.RecordCount = await paginationManager.GetTotalRecordCount(); // This will use _driversDAO
            return paginationManager;
        }

        // To be used in constructors which cannot be made async. E.g DriverManagementForm
        public async Task InitializeAsync()
        {
            try
            {
                RecordCount = await GetTotalRecordCount();
                TotalPages = Math.Max(1, (int)Math.Ceiling((double)RecordCount / _recordsPerPage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing pagination.");
                throw new InvalidOperationException("Initialization failed", ex);
            }
        }

        public event Func<int, Task>? PageChanged;
        public async Task EmitPageChanged()
        {
            await PageChanged?.Invoke(CurrentPage)!;
        }

        private async Task<int> GetTotalRecordCount()
        {
            try
            {
                if (_tableName == "Drivers")
                {
                    return await _driversDAO.GetRecordCountAsync();
                }
                else
                {
                    throw new InvalidOperationException("Total record count not supported for this table");
                }
            }
            catch (InvalidOperationException ex)
            {
                // Handle the specific InvalidOperationException
                _logger.LogError("Error: {ErrorMessage}", ex.Message);
                return 0; // Or any default value you prefer
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions
                _logger.LogError("Unexpected error: {ErrorMessage}", ex.Message);
                return 0; // Or any default value you prefer
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
