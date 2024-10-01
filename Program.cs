using SmartStartDeliveryForm.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartStartDeliveryForm
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
            
            //Initialize connection to the database
            try
            {
                DatabaseConfig.Initialize("StartSmartDB");
                FormConsole.Instance.Log("Database Initialized");
                // Only run the form if the Database connects
                Application.Run(new Login());
            }
            catch (InvalidOperationException ex)
            {
                FormConsole.Instance.Log(ex.Message);
                MessageBox.Show(ex.ToString());
            }

          
        }   
    }
}
