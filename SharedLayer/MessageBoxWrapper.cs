using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.SharedLayer
{
    // Used to DI the static class MessageBox 
    public class MessageBoxWrapper : IMessageBox
    {
        public void Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            MessageBox.Show(text, caption, buttons, icon);
        }
    }

}
