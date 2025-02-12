using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartSmartDeliveryForm
{
    public class DummyClass
    {
        internal string InternalProperty { get; set; } = "Hello from Internal Property!";

        internal void InternalMethod()
        {
            // Simple internal method
            Console.WriteLine("Hello from Internal Method!");
        }
    }
}
