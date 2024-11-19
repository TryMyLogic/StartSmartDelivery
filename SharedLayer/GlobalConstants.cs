using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace StartSmartDeliveryForm.SharedLayer
{
    public class GlobalConstants
    {
        private static readonly IConfiguration s_configuration;

        // Static constructor to initialize configuration
        static GlobalConstants()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../")) // Move up three levels from bin
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            s_configuration = builder.Build();
        }

        public static int s_recordLimit => int.TryParse(s_configuration["RecordLimit"], out int pageLimit)
            ? pageLimit: 20; //Default to 20 records per page

        public static string s_connectionString => s_configuration["ConnectionStrings:StartSmartDB"]
            ?? throw new InvalidOperationException("Connection string not found in the configuration file.");
    }
}
