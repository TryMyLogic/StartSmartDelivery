using System;
using System.Windows.Forms;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.BusinessLogicLayer
{
    public class DataFormValidator(IMessageBox messageBox)
    {
        private readonly IMessageBox _messageBox = messageBox;

        public bool IsValidString(string input, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                _messageBox.Show($"{fieldName} cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public bool IsValidEnumValue<TEnum>(string input, string fieldName) where TEnum : struct
        {
            if (!Enum.TryParse(input, out TEnum _))
            {
                MessageBox.Show($"{fieldName} is not a valid enum value.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public bool IsValidBoolValue(string input, string fieldName)
        {
            if (!bool.TryParse(input, out bool _))
            {
                MessageBox.Show($"{fieldName} must be true or false.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
    }
}
