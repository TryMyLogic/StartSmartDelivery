using System;
using System.Windows.Forms;

namespace StartSmartDeliveryForm.BusinessLogicLayer
{
    public static class DataFormValidator
    {
        public static bool IsValidString(string input, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show($"{fieldName} cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public static bool IsValidEnumValue<TEnum>(string input, string fieldName) where TEnum : struct
        {
            if (!Enum.TryParse(input, out TEnum _))
            {
                MessageBox.Show($"{fieldName} is not a valid enum value.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public static bool IsValidBoolValue(string input, string fieldName)
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
