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
        public static IConfiguration? Configuration { get; set; }

        public static int s_recordLimit => int.TryParse(Configuration?["RecordLimit"], out int pageLimit)
            ? pageLimit : 20;

        public static readonly Color MintGreen = Color.FromArgb(73, 173, 72);
        public static readonly Color SoftBeige = Color.FromArgb(240, 221, 188);
    }

}
