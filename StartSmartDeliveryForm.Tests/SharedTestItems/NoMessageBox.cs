using System.Windows.Forms;
using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.Tests.SharedTestItems
{
    public class NoMessageBox : IMessageBox
    {
        public void Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) { }
    }
}
