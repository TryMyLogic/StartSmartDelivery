namespace StartSmartDeliveryForm.BusinessLogicLayer
{
    public class DataFormValidator()
    {
        public delegate void MessageBoxEventDelegate(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
        public event MessageBoxEventDelegate? RequestMessageBox;

        public bool IsValidString(string Input, string FieldName)
        {
            if (string.IsNullOrWhiteSpace(Input))
            {
                RequestMessageBox?.Invoke($"{FieldName} cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public bool IsValidEnumValue(Type enumType, string? input)
        {
            if (!enumType.IsEnum)
            {
                RequestMessageBox?.Invoke($"{enumType.Name} is not an enum type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrEmpty(input) || !Enum.TryParse(enumType, input, out object? value) || !Enum.IsDefined(enumType, value))
            {
                RequestMessageBox?.Invoke($"'{input ?? "null"}' is not a valid {enumType.Name} value.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public bool IsValidBoolValue(string Input)
        {
            if (!bool.TryParse(Input, out bool _))
            {
                RequestMessageBox?.Invoke($"' {Input} ' is not true or false.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public bool IsValidIntValue(string? Input, string FieldName)
        {
            if (!int.TryParse(Input, out _))
            {
                RequestMessageBox?.Invoke($"' {Input} ' is not a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public bool IsValidDecimalValue(string? input, string fieldName)
        {
            if (!decimal.TryParse(input, out _))
            {
                RequestMessageBox?.Invoke($"'{input}' is not a valid decimal value for {fieldName}.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public bool IsValidDateTimeValue(string? input, string fieldName)
        {
            if (!DateTime.TryParse(input, out _))
            {
                RequestMessageBox?.Invoke($"'{input}' is not a valid date/time value for {fieldName}.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

    }
}
