using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartSmartDeliveryForm.SharedLayer.EventDelegates
{
    public static class CustomEventDelegates
    {
        public delegate void MessageBoxEventDelegate(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
    }
}
