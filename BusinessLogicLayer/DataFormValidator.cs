using System;
using System.Windows.Forms;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.BusinessLogicLayer
{
    public class DataFormValidator()
    {
        public event Action<string, string, MessageBoxButtons, MessageBoxIcon>? RequestMessageBox;

        public bool IsValidString(string input, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                RequestMessageBox?.Invoke($"{fieldName} cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public bool IsValidEnumValue<TEnum>(string input) where TEnum : struct
        {
            if (!Enum.TryParse(input, out TEnum value) || !Enum.IsDefined(typeof(TEnum), value))
            {
                RequestMessageBox?.Invoke($"' {input} ' is not a valid enum value.","Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public bool IsValidBoolValue(string input)
        {
            if (!bool.TryParse(input, out bool _))
            {
                RequestMessageBox?.Invoke($"' {input} ' is not true or false.", input, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

    }
}
