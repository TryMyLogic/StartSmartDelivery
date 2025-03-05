namespace StartSmartDeliveryForm.SharedLayer.EventDelegates
{
    public static class CustomEventDelegates
    {
        public delegate void MessageBoxEventDelegate(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
    }
}
