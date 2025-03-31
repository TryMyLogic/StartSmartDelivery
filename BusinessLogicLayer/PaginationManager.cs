using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.BusinessLogicLayer
{
    public class PaginationManager<T> where T : class
    {
        private readonly IDAO<T> _dao;
        private readonly int _recordsPerPage = GlobalConstants.s_recordLimit;
        private readonly ILogger<PaginationManager<T>> _logger;

        // Uses auto property:
        public int CurrentPage { get; private set; } = 1;
        public int TotalPages { get; private set; } = 1;
        public int RecordCount { get; private set; } = 0;

        public PaginationManager()
        {
            _dao = null!;  // or set to a mock DAO or empty if needed
            _logger = NullLogger<PaginationManager<T>>.Instance;
        }

        public PaginationManager(IDAO<T> DAO, ILogger<PaginationManager<T>>? logger = null)
        {
            _dao = DAO;
            _logger = logger ?? NullLogger<PaginationManager<T>>.Instance;
        }

        // Async Factory Pattern
        public static async Task<PaginationManager<T>> CreateAsync(IDAO<T> DAO, ILogger<PaginationManager<T>>? logger = null)
        {
            PaginationManager<T> paginationManager = new(DAO, logger);
            await paginationManager.InitializeAsync();
            return paginationManager;
        }

        // Is seperate so that it can be used in constructors which cannot be made async if necessary
        public async Task InitializeAsync()
        {
            try
            {
                RecordCount = await GetTotalRecordCount();
                TotalPages = Math.Max(1, (int)Math.Ceiling((double)RecordCount / _recordsPerPage));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("PaginationManager Initialization failed", ex);
            }
        }

        public event Func<int, Task>? PageChanged;
        public async Task EmitPageChanged()
        {
            _logger.LogInformation("Changing Page to {CurrentPage}", CurrentPage);
            // `?` checks for subscribers, `??` ensures Task completion, preventing "Object reference not set" errors.
            await (PageChanged?.Invoke(CurrentPage) ?? Task.CompletedTask);
        }

        private async Task<int> GetTotalRecordCount(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Record count: {RecordCount}", RecordCount);
            return await _dao.GetRecordCountAsync(cancellationToken);
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

        public async Task GoToPage(int Page)
        {
            CurrentPage = Math.Clamp(Page, 1, TotalPages);
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
