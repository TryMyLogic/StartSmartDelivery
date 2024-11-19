using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace StartSmartDeliveryForm.Tests
{
    internal class TestConstants
    {
            private static readonly IConfiguration s_configuration;

            // Static constructor to initialize configuration
            static TestConstants()
            {
                IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../")) // Move up three levels from bin
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                s_configuration = builder.Build();
            }

            public static string s_testconnectionString => s_configuration["ConnectionStrings:StartSmartDB"]
                ?? throw new InvalidOperationException("Connection string not found in the configuration file.");
        
    }
}
