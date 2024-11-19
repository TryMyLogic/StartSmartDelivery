using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement
{
    public partial class PrintDriverDataForm : Form
    {
        private int _currentPage = 1;
        private readonly int _totalPages;
        private readonly DataGridView? _dataGridView;

        // TODO - Print all databases pages, ensuring each page contains as many records (within reason)
        public PrintDriverDataForm()
        {
            InitializeComponent();
            printPreviewControl.Document = printDocument;

            // _totalPages = (int)Math.Ceiling((double)_recordsCount / GlobalConstants.s_recordLimit);
        }

        // Used for printing all the database pages, according to the appsettings row count
        public PrintDriverDataForm(int totalPages)
        {
            InitializeComponent();
            printPreviewControl.Document = printDocument;

            _totalPages = totalPages;

        }

        // Used for printing a specific page
        public PrintDriverDataForm(DataGridView dgvMain)
        {
            InitializeComponent();
            printPreviewControl.Document = printDocument;

            _dataGridView = dgvMain;
            _totalPages = 1;
        }

        private void PrintDriverDataForm_Load(object sender, EventArgs e)
        {
            if (_totalPages == 1)
            {
                btnPrevious.Hide();
                btnNext.Hide();
            }
        }

        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            FormConsole.Instance.Log("Hello");
            if (e.Graphics == null)
            {
                FormConsole.Instance.Log("e.Graphics was null"); // Shouldnt get here in most cases.
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
            if (_dataGridView != null && _dataGridView.DataSource is DataTable dt)
            {
                dataTable = dt;
            }
            else
            {
                dataTable = DriversDAO.GetDriversAtPage(_currentPage);
                if (dataTable == null)
                {
                    FormConsole.Instance.Log("GetDriversAtPage returned null datatable");
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
                    float availableWidth = columnWidth - (2 * padding);
                    SizeF cellSize = e.Graphics.MeasureString(cellText, font, (int)availableWidth);
                    maxRowHeight = Math.Max(maxRowHeight, cellSize.Height);
                }

                maxRowHeight += padding * 2; // Padding on top and bottom of each cell

                foreach (object? cell in row.ItemArray)
                {
                    string cellText = cell?.ToString() ?? "";
                    float availableWidth = columnWidth - (2 * padding); // width for wrapping text without padding

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

            if (_totalPages != 1)
            {
                // Footer text with page number
                Font footerFont = new("Arial", 10, FontStyle.Italic);
                string footerText = $"Page {_currentPage}";
                SizeF footerSize = e.Graphics.MeasureString(footerText, footerFont);
                float footerY = e.MarginBounds.Bottom + 20; // Place below content
                e.Graphics.DrawString(footerText, footerFont, Brushes.Black,
                e.MarginBounds.Right - footerSize.Width, footerY);
            }

            if (_currentPage < _totalPages)
            {
                _currentPage++;
                e.HasMorePages = true;
            }
            else
            {
                _currentPage = 1;
                printPreviewControl.StartPage = _currentPage - 1; // 0 based index
                e.HasMorePages = false;
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                printPreviewControl.StartPage = _currentPage - 1;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                printPreviewControl.StartPage = _currentPage - 1;
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            printDocument.Print();
            Close();
        }
    }
}
