namespace StartSmartDeliveryForm.SharedLayer.EventArgs
{
    internal class MessageBoxEventArgs(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        public string Text { get; } = text;
        public string Caption { get; } = caption;
        public MessageBoxButtons Buttons { get; } = buttons;
        public MessageBoxIcon Icon { get; } = icon;

        public static MessageBoxEventArgs Empty => new(string.Empty, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);
    }
}
