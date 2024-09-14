using SmartStartDeliveryForm.Classes;
using SmartStartDeliveryForm.DAOs;
using SmartStartDeliveryForm.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartStartDeliveryForm
{
    public partial class ManagementTemplate : Form
    {

        public ManagementTemplate()
        {
            InitializeComponent();
        }

        private void Template_Load(object sender, EventArgs e)
        {
            dataGridView1.RowHeadersVisible = false; // Hides Row Number Column
        }    
        
        protected void SetTitle(string NewTitle="SetMe")
        {
            TitleLabel.Text = NewTitle;
        }

        protected virtual void InsertBTN_Click(object sender, EventArgs e)
        {

        }
    }
}
