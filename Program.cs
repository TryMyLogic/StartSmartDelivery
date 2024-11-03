using StartSmartDeliveryForm.PresentationLayer.DriverManagement;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            FormConsole.Instance.Show();
            FormConsole.Instance.Log($"Page limit is: {GlobalConstants.PageLimit}");

            Application.Run(new DriverManagementForm());
        }
    }
}
