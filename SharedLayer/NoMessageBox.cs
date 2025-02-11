using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.SharedLayer
{
    public class NoMessageBox : IMessageBox
    {
        public void Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) { }
    }
}
