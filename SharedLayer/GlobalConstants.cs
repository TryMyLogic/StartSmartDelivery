using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartSmartDeliveryForm.SharedLayer
{
    internal class GlobalConstants
    {
        public static readonly int s_pageLimit = int.Parse(ConfigurationManager.AppSettings["Pagelimit"]);

        public static readonly string s_connectionString =
         ConfigurationManager.ConnectionStrings["StartSmartDB"]?.ConnectionString
         ?? throw new InvalidOperationException("Connection string not found in the configuration file.");
    }
}
