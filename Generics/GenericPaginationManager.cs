using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.Generics
{
    public class GenericPaginationManager<T>(IRepository<T> repository, ILogger<GenericPaginationManager<T>>? logger = null) where T : class
    {
        private readonly IRepository<T> _repository = repository;
        private readonly int _recordsPerPage = GlobalConstants.s_recordLimit;
        private readonly ILogger<GenericPaginationManager<T>> _logger = logger ?? NullLogger<GenericPaginationManager<T>>.Instance;

        public int CurrentPage { get; private set; } = 1;
        public int TotalPages { get; private set; } = 1;
        public int RecordCount { get; private set; } = 0;

        public static async Task<GenericPaginationManager<T>> CreateAsync(IRepository<T> repository, ILogger<GenericPaginationManager<T>>? logger = null)
        {
            GenericPaginationManager<T> paginationManager = new(repository, logger);
            await paginationManager.InitializeAsync();
            return paginationManager;
        }

        public async Task InitializeAsync()
        {
            try
            {
                RecordCount = await GetTotalRecordCountAsync();
                TotalPages = Math.Max(1, (int)Math.Ceiling((double)RecordCount / _recordsPerPage));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("DynamicPaginationManager Initialization failed", ex);
            }
        }

        public event Func<int, Task>? PageChanged;
        public async Task EmitPageChangedAsync()
        {
            _logger.LogInformation("Changing Page to {CurrentPage} of {TotalPages}", CurrentPage, TotalPages);
            // `?` checks for subscribers, `??` ensures Task completion, preventing "Object reference not set" errors.
            await (PageChanged?.Invoke(CurrentPage) ?? Task.CompletedTask);
        }

        private async Task<int> GetTotalRecordCountAsync(CancellationToken cancellationToken = default)
        {
            int count = await _repository.GetRecordCountAsync(cancellationToken);
            _logger.LogInformation("Record count: {RecordCount}", count);
            return count;
        }

        public async Task GoToFirstPageAsync()
        {
            CurrentPage = 1;
            await EmitPageChangedAsync();
        }

        public async Task GoToPreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await EmitPageChangedAsync();
            }
        }

        public async Task GoToNextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await EmitPageChangedAsync();
            }
        }

        public async Task GoToLastPageAsync()
        {
            CurrentPage = TotalPages;
            await EmitPageChangedAsync();
        }

        public async Task GoToPageAsync(int page)
        {
            CurrentPage = Math.Clamp(page, 1, TotalPages);
            await EmitPageChangedAsync();
        }

        public void UpdateRecordCountAsync(int recordCount)
        {
            RecordCount = recordCount;
            TotalPages = Math.Max(1, (int)Math.Ceiling((double)RecordCount / _recordsPerPage));
        }

        public async Task EnsureValidPageAsync()
        {
            if (CurrentPage > TotalPages)
            {
                await GoToLastPageAsync();
            }
            else
            {
                await GoToPageAsync(CurrentPage);
            }
        }
    }
}
