using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.PresentationLayer
{
    public enum FormMode
    {
        Add,
        Edit
    }
    public partial class DataFormTemplate : Form
    {
        public FormMode Mode { get; set; }
        public DataFormTemplate()
        {
            InitializeComponent();
        }

        private void DataFormTemplate_Load(object sender, EventArgs e)
        {
            btnSubmit.BackColor = GlobalConstants.SoftBeige;
            btnSubmit.FlatAppearance.BorderSize = 0;
        }

        public delegate void SubmitEventHandler(object sender, EventArgs e);
        public event SubmitEventHandler? SubmitClicked;

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (ValidForm())
            {
                SubmitClicked?.Invoke(this, EventArgs.Empty);
            }
        }

        protected virtual bool ValidForm() { return true; }
        internal virtual void InitializeEditing(object data) { }
        internal virtual void ClearData() { }
        internal virtual object GetData() { return new { }; }
    }
}
