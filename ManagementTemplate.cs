﻿using SmartStartDeliveryForm.Classes;
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
using SmartStartDeliveryForm.DataForms;
using SmartStartDeliveryForm.DTOs;
using System.IO;

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

        //protected void SetTitle(string NewTitle = "SetMe")
        //{
        //    TitleLabel.Text = NewTitle;
        //}

        protected void SetSearchOptions(Type Dto)
        {
            cboSearchOptions.Items.Clear();

            var properties = Dto.GetProperties();
            foreach (var property in properties)
            {
                cboSearchOptions.Items.Add(property.Name);
            }

            cboSearchOptions.SelectedIndex = 0;
        }



        protected virtual void InsertBTN_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {


                if (dataGridView1.Columns[e.ColumnIndex].Name == "Edit")
                {
                    EditBTN_Click(e.RowIndex);
                }
                else if (dataGridView1.Columns[e.ColumnIndex].Name == "Delete")
                {
                    DeleteBTN_Click(e.RowIndex);
                }

            }
        }

        protected virtual void EditBTN_Click(int RowIndex)
        {
            // Retrieve the data for the selected row
            var SelectedRow = dataGridView1.Rows[RowIndex];
            // Implement your edit logic here{rowIndex}");
            MessageBox.Show($"Edit button clicked for row {RowIndex}");
        }
        protected virtual void DeleteBTN_Click(int RowIndex)
        {
            // Retrieve the data for the selected row
            var SelectedRow = dataGridView1.Rows[RowIndex];
            // Implement your delete logic here

            DialogResult Result = MessageBox.Show("Are you sure", "Delete Row", MessageBoxButtons.YesNo);
            if (Result == DialogResult.Yes)
            {

                MessageBox.Show($"Delete button clicked for row {RowIndex}");
            }
        }

        protected virtual void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
             
        }

        protected virtual void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
