using Microsoft.Extensions.DependencyInjection;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm
{
    internal static class Program
    {

        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            IServiceProvider serviceRegistry = ServiceRegistry.RegisterServices();
            ApplicationCoordinator applicationCoordinator = serviceRegistry.GetRequiredService<ApplicationCoordinator>();
            applicationCoordinator.Start();
        }
    }
}
