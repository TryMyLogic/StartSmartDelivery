using StartSmartDeliveryForm.PresentationLayer;
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
            Application.Run(new DriverManagementForm());
        }
    }
}
