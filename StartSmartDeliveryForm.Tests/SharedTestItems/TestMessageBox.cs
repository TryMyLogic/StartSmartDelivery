using System.Windows.Forms;
using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.Tests.SharedTestItems
{
    public class TestMessageBox : IMessageBox
    {
        public bool WasShowCalled { get; private set; }
        public string? LastMessage { get; private set; }
        public string? LastCaption { get; private set; }
        public MessageBoxButtons LastButton { get; private set; }
        public MessageBoxIcon LastIcon { get; private set; }

        public void Show(string text, string caption, MessageBoxButtons button, MessageBoxIcon icon)
        {
            WasShowCalled = true;
            LastMessage = text;
            LastCaption = caption;
            LastButton = button;
            LastIcon = icon;
        }
    }

}
