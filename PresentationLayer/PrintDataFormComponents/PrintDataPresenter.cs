using System.Data;
using System.Drawing.Printing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.DataLayer.Repositories;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.PresentationLayer.PrintDataFormComponents
{
    public class PrintDataPresenter<T> where T : class
    {
        private readonly IPrintDataForm _printDataForm;
        private readonly IRepository<T> _repository;
        private readonly ILogger<PrintDataPresenter<T>> _logger;
        private readonly PrintDocument _printDocument;
        private readonly DataTable? _dataTable;
        private int _recordCount; // Made mutable since initialized in InitializeAsync
        private readonly int _recordsPerPage = GlobalConstants.s_recordLimit;

        private PrintDataPresenter(
            IPrintDataForm printDataForm,
            IRepository<T> repository,
            DataTable? dataTable,
            ILogger<PrintDataPresenter<T>>? logger = null)
        {
            _printDataForm = printDataForm ?? throw new ArgumentNullException(nameof(printDataForm));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? NullLogger<PrintDataPresenter<T>>.Instance;
            _dataTable = dataTable;

            _printDocument = new PrintDocument();
            _printDocument.PrintPage += async (s, e) => await PrintDocument_PrintPageAsync(s, e);
            printDataForm.SetPrintDocument(_printDocument);
        }

        public static async Task<PrintDataPresenter<T>> CreateAsync(
            IPrintDataForm printDataForm,
            IRepository<T> repository,
            DataTable? dataTable,
            ILogger<PrintDataPresenter<T>>? logger = null)
        {
            var presenter = new PrintDataPresenter<T>(printDataForm, repository, dataTable, logger);
            await presenter.InitializeAsync();
            return presenter;
        }

        private async Task InitializeAsync()
        {
            try
            {
                if (_printDataForm.TotalPages == 1)
                {
                    _printDataForm.HideNavigationButtons();
                }

                _printDataForm.PreviousClicked += HandlePreviousClicked;
                _printDataForm.NextClicked += HandleNextClicked;
                _printDataForm.SubmitClicked += HandleSubmitClicked;

                _recordCount = await GetTotalRecordCountAsync();
                _printDataForm.TotalPages = Math.Max(1, (int)Math.Ceiling((double)_recordCount / _recordsPerPage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrintDataPresenter initialization failed.");
                throw new InvalidOperationException("PrintDataPresenter initialization failed", ex);
            }
        }

        private async Task<int> GetTotalRecordCountAsync(CancellationToken cancellationToken = default)
        {
            int count = await _repository.GetRecordCountAsync(cancellationToken);
            _logger.LogInformation("Record count: {RecordCount}", count);
            return count;
        }

        private async Task PrintDocument_PrintPageAsync(object sender, PrintPageEventArgs e, CancellationToken cancellationToken = default)
        {
            if (e.Graphics == null)
            {
                _logger.LogWarning("e.Graphics was null");
                return;
            }

            // Header text
            string pageHeaderText = "Data Report";
            var pageHeaderFont = new Font("Arial", 12, FontStyle.Bold);
            SizeF pageHeaderSize = e.Graphics.MeasureString(pageHeaderText, pageHeaderFont);
            float headerY = e.MarginBounds.Top - pageHeaderSize.Height - 20;
            e.Graphics.DrawString(pageHeaderText, pageHeaderFont, Brushes.Black, e.MarginBounds.Left, headerY);

            DataTable? dataTable;
            if (_dataTable != null)
            {
                dataTable = _dataTable;
            }
            else
            {
                dataTable = await _repository.GetRecordsAtPageAsync(_printDataForm.CurrentPage, cancellationToken);
                if (dataTable == null)
                {
                    _logger.LogError("GetRecordsAtPageAsync returned null datatable");
                    return;
                }
            }

            var font = new Font("Arial", 10);
            var headerFont = new Font("Arial", 10, FontStyle.Bold);
            float x = e.MarginBounds.Left;
            float y = e.MarginBounds.Top;
            float columnWidth = e.MarginBounds.Width / dataTable.Columns.Count;
            float padding = 5f;

            foreach (DataColumn column in dataTable.Columns)
            {
                string headerText = column.ColumnName;
                SizeF headerSize = e.Graphics.MeasureString(headerText, headerFont, (int)columnWidth);
                float headerHeight = headerSize.Height;

                e.Graphics.DrawString(headerText, headerFont, Brushes.Black, x + padding, y + padding);
                e.Graphics.DrawRectangle(Pens.Black, x, y, columnWidth, headerHeight + padding);

                x += columnWidth;
            }

            y += e.Graphics.MeasureString(dataTable.Columns[0].ColumnName, headerFont, (int)columnWidth).Height + padding;

            foreach (DataRow row in dataTable.Rows)
            {
                x = e.MarginBounds.Left;

                float maxRowHeight = 0;
                foreach (object? cell in row.ItemArray)
                {
                    string cellText = cell?.ToString() ?? "";
                    float availableWidth = columnWidth - 2 * padding;
                    SizeF cellSize = e.Graphics.MeasureString(cellText, font, (int)availableWidth);
                    maxRowHeight = Math.Max(maxRowHeight, cellSize.Height);
                }

                maxRowHeight += padding * 2;

                foreach (object? cell in row.ItemArray)
                {
                    string cellText = cell?.ToString() ?? "";
                    float availableWidth = columnWidth - 2 * padding;

                    StringFormat stringFormat = new()
                    {
                        FormatFlags = StringFormatFlags.LineLimit,
                        Trimming = StringTrimming.EllipsisCharacter
                    };

                    e.Graphics.DrawString(cellText, font, Brushes.Black, new RectangleF(x + padding, y + padding, availableWidth, maxRowHeight), stringFormat);
                    e.Graphics.DrawRectangle(Pens.Black, x, y, columnWidth, maxRowHeight);

                    x += columnWidth;
                }

                y += maxRowHeight;
            }

            if (_printDataForm.TotalPages != 1)
            {
                Font footerFont = new("Arial", 10, FontStyle.Italic);
                string footerText = $"Page {_printDataForm.CurrentPage}";
                SizeF footerSize = e.Graphics.MeasureString(footerText, footerFont);
                float footerY = e.MarginBounds.Bottom + 20;
                e.Graphics.DrawString(footerText, footerFont, Brushes.Black, e.MarginBounds.Right - footerSize.Width, footerY);
            }

            if (_printDataForm.CurrentPage < _printDataForm.TotalPages)
            {
                _printDataForm.CurrentPage++;
                e.HasMorePages = true;
            }
            else
            {
                _printDataForm.CurrentPage = 1;
                _printDataForm.UpdatePreviewPage(_printDataForm.CurrentPage - 1);
                e.HasMorePages = false;
            }
        }

        private void HandlePreviousClicked(object? sender, EventArgs e)
        {
            if (_printDataForm.CurrentPage > 1)
            {
                _printDataForm.CurrentPage--;
                _printDataForm.UpdatePreviewPage(_printDataForm.CurrentPage - 1);
            }
        }

        private void HandleNextClicked(object? sender, EventArgs e)
        {
            if (_printDataForm.CurrentPage < _printDataForm.TotalPages)
            {
                _printDataForm.CurrentPage++;
                _printDataForm.UpdatePreviewPage(_printDataForm.CurrentPage - 1);
            }
        }

        private void HandleSubmitClicked(object? sender, EventArgs e)
        {
            _printDocument.Print();
            _printDataForm.CloseForm();
        }
    }
}
