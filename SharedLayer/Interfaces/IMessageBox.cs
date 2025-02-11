using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartSmartDeliveryForm.SharedLayer.Interfaces
{
    public interface IMessageBox
    {
        void Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
    }
}
