using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStartDeliveryForm.Classes
{
    internal class User
    {
        internal static bool Login(string username, string password)
        {
           if(username == "Admin" &&  password == "admin1234")
            {
                return true;
            }
            return false;
        }
    }
}
