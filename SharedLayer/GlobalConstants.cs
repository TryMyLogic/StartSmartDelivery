using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace StartSmartDeliveryForm.SharedLayer
{
    internal class GlobalConstants
    {
        private static readonly IConfiguration _configuration;

        // Static constructor to initialize configuration
        static GlobalConstants()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../")) // Move up two levels from bin
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public static int PageLimit => int.Parse(_configuration["Pagelimit"]);

        public static string ConnectionString => _configuration["StartSmartDB:ConnectionString"]
            ?? throw new InvalidOperationException("Connection string not found in the configuration file.");
    }
}
