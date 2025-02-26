﻿using StartSmartDeliveryForm.SharedLayer;

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

        protected async void btnSubmit_Click(object sender, EventArgs e)
        {
            await btnSubmit_ClickAsync(sender, e);
        }

        protected async Task btnSubmit_ClickAsync(object sender, EventArgs e)
        {
            if (await ValidFormAsync())
            {
                SubmitClicked?.Invoke(this, EventArgs.Empty);
            }
        }

        protected virtual async Task<bool> ValidFormAsync()
        {
            await Task.Delay(500);
            return false;
        }
        internal virtual void InitializeEditing(object data) { }
        internal virtual void ClearData() { }
        internal virtual object GetData() { return new { }; }
    }
}
