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

        protected virtual HashSet<string> GetExcludedColumns()
        {
            return new HashSet<string>(); // By default, exclude nothing.
        }
        protected void SetSearchOptions(Type Dto)
        {
            cboSearchOptions.Items.Clear();

            //Makes comparison case insensitive
            var ExcludedColumns = GetExcludedColumns().Select(c => c.ToLower()).ToHashSet();
            var Properties = Dto.GetProperties();
            foreach (var Property in Properties)
            {
                if (!ExcludedColumns.Contains(Property.Name.ToLower()))
                    cboSearchOptions.Items.Add(Property.Name);
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

        }
        protected virtual void DeleteBTN_Click(int RowIndex)
        {

        }

        protected virtual void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        protected virtual void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        protected virtual void rollbackToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        protected virtual void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

        }

        protected virtual void SearchBTN_Click(object sender, EventArgs e)
        {
            var SelectedOption = cboSearchOptions.SelectedItem.ToString();
            var SearchTerm = txtSearchBox.Text.Trim();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var dataTable = (DataTable)dataGridView1.DataSource;

                if (dataTable != null)
                {
                    // Escape single quotes in the search term
                    SearchTerm = SearchTerm.Replace("'", "''");

                    var ColumnType = dataTable.Columns[SelectedOption].DataType;

                    string filterExpression;

                    if (ColumnType == typeof(string))
                    {
                        filterExpression = $"{SelectedOption} LIKE '%{SearchTerm}%'";
                    }
                    else if (ColumnType == typeof(int))
                    {
                        filterExpression = SearchEnumColumn(SelectedOption, SearchTerm);
                        //If it isnt an enum. handle as a normal int
                        
                        //TODO

                    }
                    else
                    {
                        FormConsole.Instance.Log("Unsupported data type for filtering.");
                        return; 
                    }

                    // Apply the filter
                    dataTable.DefaultView.RowFilter = filterExpression;
                    FormConsole.Instance.Log($"Filter applied: {filterExpression}");
                }
                else
                {
                    FormConsole.Instance.Log("No data source found for DataGridView.");
                }
            }
            else
            {
                FormConsole.Instance.Log("Search term is empty, removing filter.");

                var dataTable = (DataTable)dataGridView1.DataSource;
                if (dataTable != null)
                {
                    dataTable.DefaultView.RowFilter = string.Empty;  // Reset the filter
                }
            }
        }

        /*
        Note: The SearchEnum function works on the assumption that
        1. The namespace for Enums is the same across the board
        2. The column name for any Enum column have the same name as the 
           Enum itself.
         */
        protected string SearchEnumColumn(string selectedOption, string searchTerm)
        {
            var EnumType = Type.GetType($"SmartStartDeliveryForm.Enums.{selectedOption}", false);

            FormConsole.Instance.Log("enumType" + EnumType);
            if (EnumType != null && EnumType.IsEnum)
            {
                // Check if the SearchTerm is found within the Enum 
                if (Enum.IsDefined(EnumType, searchTerm))
                {
                    var enumValue = Enum.Parse(EnumType, searchTerm);
                    return $"{selectedOption} = {(int)enumValue}";
                }
                else
                {
                    FormConsole.Instance.Log($"Invalid {selectedOption} search term.");
                    return string.Empty;
                }
            }

            // Return empty if not an enum
            return string.Empty;
        }



    }
}
