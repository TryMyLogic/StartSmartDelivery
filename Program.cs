using StartSmartDeliveryForm.Classes;
using StartSmartDeliveryForm.SharedLayer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StartSmartDeliveryForm
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Initialize application styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FormConsole.Instance.Show();

            try
            {
                FormConsole.Instance.Log("Initializing database connection...");

                using (SqlConnection connection = new SqlConnection(GlobalConstants.s_connectionString))
                {
                    connection.Open();
                    FormConsole.Instance.Log("Database Initialized");
                }

                // Only run the form if the Database connects
                Application.Run(new DriverManagement());
            }
            catch (InvalidOperationException ex)
            {
                FormConsole.Instance.Log(ex.Message);
                MessageBox.Show(ex.ToString());
            }
            catch (SqlException ex) 
            {
                FormConsole.Instance.Log($"Database connection failed: {ex.Message}");
                MessageBox.Show(ex.ToString());
            }

        }   
    }
}
