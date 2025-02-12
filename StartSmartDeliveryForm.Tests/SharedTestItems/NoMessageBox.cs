using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.SharedLayer.Interfaces;
using System.Windows.Forms;

namespace StartSmartDeliveryForm.Tests.SharedTestItems
{
    public class NoMessageBox : IMessageBox
    {
        public void Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) { }
    }
}
