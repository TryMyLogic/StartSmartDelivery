namespace StartSmartDeliveryForm.SharedLayer.Interfaces
{
    public interface IMessageBox
    {
        void Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
    }
}
