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
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Initialize application styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show the logging form before running the main form
            Console console = new Console();
            console.Show();

            try
            {
                console.log(DatabaseConfig.ConnectionString);
            }
            catch (InvalidOperationException ex)
            {
                console.log(ex.Message);
            }

            // Run the main form
            Application.Run(new DriverManagement());
        }
    }
}
