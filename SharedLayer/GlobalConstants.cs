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
        // Is not loaded until it is accessed for the first time
        private static readonly Lazy<IConfiguration> s_configuration = new(() =>
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            return builder.Build();
        });

        public static IConfiguration Configuration => s_configuration.Value;

        public static int s_recordLimit => int.TryParse(Configuration["RecordLimit"], out int pageLimit)
            ? pageLimit : 20;

        public static string s_connectionString => Configuration["ConnectionStrings:StartSmartDB"]
            ?? throw new InvalidOperationException("Connection string not found in the configuration file.");

        public static readonly Color MintGreen = Color.FromArgb(73, 173, 72);
        public static readonly Color SoftBeige = Color.FromArgb(240, 221, 188);
    }

}
