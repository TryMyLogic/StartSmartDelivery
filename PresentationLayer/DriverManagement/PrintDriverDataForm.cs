using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
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
        private const int TotalPages = 2;

        public PrintDriverDataForm()
        {
            InitializeComponent();
            printPreviewControl.Document = printDocument;
        }

        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Header text
            string pageHeaderText = "Driver Data Report";
            var pageHeaderFont = new Font("Arial", 12, FontStyle.Bold);
            var pageHeaderSize = e.Graphics.MeasureString(pageHeaderText, pageHeaderFont);
            float headerY = e.MarginBounds.Top - pageHeaderSize.Height - 20; // Place above content
            e.Graphics.DrawString(pageHeaderText, pageHeaderFont, Brushes.Black,
                                   e.MarginBounds.Left, headerY);

            // Footer text with page number
            var footerFont = new Font("Arial", 10, FontStyle.Italic);
            string footerText = $"Page {_currentPage}";
            var footerSize = e.Graphics.MeasureString(footerText, footerFont);
            float footerY = e.MarginBounds.Bottom + 20; // Place below content
            e.Graphics.DrawString(footerText, footerFont, Brushes.Black,
            e.MarginBounds.Right - footerSize.Width, footerY);

            if (_currentPage < TotalPages)
            {
                _currentPage++;
                e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false;
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            printDocument.Print();
            this.Close();
        }
    }
}
