using System.Windows.Forms;
using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.Tests.SharedTestItems
{
    public class TestMessageBox : IMessageBox
    {
        public bool WasShowCalled { get; private set; }
        public string LastMessage { get; private set; }

        public void Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            WasShowCalled = true;
            LastMessage = text;
        }
    }

}
