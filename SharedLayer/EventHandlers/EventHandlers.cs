using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartSmartDeliveryForm.SharedLayer.EventHandlers
{
    public class EventHandlers
    {
        public delegate void SubmitEventHandler(object sender, EventArgs e);
        public delegate void MessageBoxEventHandler(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
    }
}
