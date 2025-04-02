using System.Data;
using System.Drawing.Printing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.PresentationLayer
{
    public class PrintDriverDataFormPresenter<T> where T : class
    {
        private readonly IPrintDriverDataForm _printDriverDataForm;
        private readonly IDAO<T> _dao;
        private readonly ILogger<PrintDriverDataFormPresenter<T>> _logger;
        private readonly PrintDocument _printDocument;
        private readonly DataTable? _dataTable;
        private readonly int _recordCount;
        private readonly int _recordsPerPage = GlobalConstants.s_recordLimit;

        public PrintDriverDataFormPresenter(IPrintDriverDataForm printDriverDataForm, IDAO<T> dao, DataTable? dataTable, ILogger<PrintDriverDataFormPresenter<T>>? logger = null)
        {
            _printDriverDataForm = printDriverDataForm;
            _dao = dao;
            _logger = logger ?? NullLogger<PrintDriverDataFormPresenter<T>>.Instance;

            _dataTable = dataTable;

            _printDocument = new PrintDocument();
            _printDocument.PrintPage += async (s, e) => await PrintDocument_PrintPageAsync(s, e);
            printDriverDataForm.SetPrintDocument(_printDocument);
        }

        public static async Task<PrintDriverDataFormPresenter<T>> CreateAsync(IPrintDriverDataForm printDriverDataForm, IDAO<T> dao, DataTable dataTable, ILogger<PrintDriverDataFormPresenter<T>>? logger = null)
        {
            var presenter = new PrintDriverDataFormPresenter<T>(printDriverDataForm, dao, dataTable, logger);
            await presenter.InitializeAsync();
            return presenter;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (_printDriverDataForm.TotalPages == 1)
                {
                    _printDriverDataForm.HideNavigationButtons();
                }

                _printDriverDataForm.PreviousClicked += HandlePreviousClicked;
                _printDriverDataForm.NextClicked += HandleNextClicked;
                _printDriverDataForm.SubmitClicked += HandleSubmitClicked;

                await GetTotalRecordCountAsync();
                _printDriverDataForm.TotalPages = Math.Max(1, (int)Math.Ceiling((double)_recordCount / _recordsPerPage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PrintDriverDataFormPresenter initialization failed.");
                throw new InvalidOperationException("PrintDriverDataFormPresenter initialization failed", ex);
            }
        }

        private async Task<int> GetTotalRecordCountAsync(CancellationToken cancellationToken = default)
        {
            int count = await _dao.GetRecordCountAsync(cancellationToken);
            _logger.LogInformation("Record count: {RecordCount}", count);
            return count;
        }

        private async Task PrintDocument_PrintPageAsync(object sender, PrintPageEventArgs e, CancellationToken cancellationToken = default)
        {
            if (e.Graphics == null)
            {
                _logger.LogWarning("e.Graphics was null"); // Shouldnt get here in most cases.
                return;
            }

            // Header text
            string pageHeaderText = "Driver Data Report";
            var pageHeaderFont = new Font("Arial", 12, FontStyle.Bold);
            SizeF pageHeaderSize = e.Graphics.MeasureString(pageHeaderText, pageHeaderFont);
            float headerY = e.MarginBounds.Top - pageHeaderSize.Height - 20; // Place above content
            e.Graphics.DrawString(pageHeaderText, pageHeaderFont, Brushes.Black,
                                   e.MarginBounds.Left, headerY);

            DataTable? dataTable;
            if (_dataTable != null)
            {
                dataTable = _dataTable;
            }
            else
            {
                dataTable = await _dao.GetRecordsAtPageAsync(_printDriverDataForm.CurrentPage, cancellationToken);
                if (dataTable == null)
                {
                    _logger.LogError("GetDriversAtPage returned null datatable");
                    // Notify view
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
                e.Graphics.DrawRectangle(Pens.Black, x, y, columnWidth, headerHeight + padding); // Border with padding

                x += columnWidth;
            }

            // Move to line after the headers
            y += e.Graphics.MeasureString(dataTable.Columns[0].ColumnName, headerFont, (int)columnWidth).Height + padding;

            foreach (DataRow row in dataTable.Rows)
            {
                x = e.MarginBounds.Left; // Reset x for each row

                float maxRowHeight = 0; // Ensures height consistency
                foreach (object? cell in row.ItemArray)
                {
                    string cellText = cell?.ToString() ?? "";
                    float availableWidth = columnWidth - 2 * padding;
                    SizeF cellSize = e.Graphics.MeasureString(cellText, font, (int)availableWidth);
                    maxRowHeight = Math.Max(maxRowHeight, cellSize.Height);
                }

                maxRowHeight += padding * 2; // Padding on top and bottom of each cell

                foreach (object? cell in row.ItemArray)
                {
                    string cellText = cell?.ToString() ?? "";
                    float availableWidth = columnWidth - 2 * padding; // width for wrapping text without padding

                    // Wraps text if needed
                    StringFormat stringFormat = new()
                    {
                        FormatFlags = StringFormatFlags.LineLimit,
                        Trimming = StringTrimming.EllipsisCharacter
                    };

                    e.Graphics.DrawString(cellText, font, Brushes.Black, new RectangleF(x + padding, y + padding, availableWidth, maxRowHeight), stringFormat);
                    e.Graphics.DrawRectangle(Pens.Black, x, y, columnWidth, maxRowHeight);

                    x += columnWidth;
                }

                y += maxRowHeight; // Move down for the next row
            }

            if (_printDriverDataForm.TotalPages != 1)
            {
                // Footer text with page number
                Font footerFont = new("Arial", 10, FontStyle.Italic);
                string footerText = $"Page {_printDriverDataForm.CurrentPage}";
                SizeF footerSize = e.Graphics.MeasureString(footerText, footerFont);
                float footerY = e.MarginBounds.Bottom + 20; // Place below content
                e.Graphics.DrawString(footerText, footerFont, Brushes.Black,
                e.MarginBounds.Right - footerSize.Width, footerY);
            }

            if (_printDriverDataForm.CurrentPage < _printDriverDataForm.TotalPages)
            {
                _printDriverDataForm.CurrentPage++;
                e.HasMorePages = true;
            }
            else
            {
                _printDriverDataForm.CurrentPage = 1;
                _printDriverDataForm.UpdatePreviewPage(_printDriverDataForm.CurrentPage - 1); // 0 based index
                e.HasMorePages = false;
            }
        }

        private void HandlePreviousClicked(object? sender, EventArgs e)
        {
            if (_printDriverDataForm.CurrentPage > 1)
            {
                _printDriverDataForm.CurrentPage--;
                _printDriverDataForm.UpdatePreviewPage(_printDriverDataForm.CurrentPage - 1);
            }
        }

        private void HandleNextClicked(object? sender, EventArgs e)
        {
            if (_printDriverDataForm.CurrentPage < _printDriverDataForm.TotalPages)
            {
                _printDriverDataForm.CurrentPage++;
                _printDriverDataForm.UpdatePreviewPage(_printDriverDataForm.CurrentPage - 1);
            }
        }

        private void HandleSubmitClicked(object? sender, EventArgs e)
        {
            _printDocument.Print();
            _printDriverDataForm.CloseForm();
        }

    }
}
