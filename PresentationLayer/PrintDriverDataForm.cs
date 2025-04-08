using System.Drawing.Printing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement
{
    public partial class GenericPrintDataForm : Form, IGenericPrintDataForm
    {
        private readonly ILogger _logger;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;

        public GenericPrintDataForm(ILogger<GenericPrintDataForm>? logger = null)
        {
            InitializeComponent();
            printPreviewControl.Document = printDocument;
            _logger = logger ?? NullLogger<GenericPrintDataForm>.Instance;
        }

        private void GenericPrintDataForm_Load(object sender, EventArgs e) { }

        public void SetPrintDocument(PrintDocument document)
        {
            printPreviewControl.Document = document;
        }

        public void HideNavigationButtons()
        {
            _logger.LogInformation("HideNavigationButtons was ran");
            btnPrevious.Hide();
            btnNext.Hide();
        }

        public void UpdatePreviewPage(int pageIndex)
        {
            printPreviewControl.StartPage = pageIndex;
        }

        public event EventHandler? PreviousClicked;
        public event EventHandler? NextClicked;
        public event EventHandler? SubmitClicked;

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            PreviousClicked?.Invoke(this, e);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            NextClicked?.Invoke(this, e);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            SubmitClicked?.Invoke(this, e);
        }

        public void CloseForm()
        {
            Close();
        }

    }
}
