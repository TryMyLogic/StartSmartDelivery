namespace StartSmartDeliveryForm.BusinessLogicLayer
{
    public class DataFormValidator()
    {
        public event Action<string, string, MessageBoxButtons, MessageBoxIcon>? RequestMessageBox;

        public bool IsValidString(string Input, string FieldName)
        {
            if (string.IsNullOrWhiteSpace(Input))
            {
                RequestMessageBox?.Invoke($"{FieldName} cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public bool IsValidEnumValue<TEnum>(string Input) where TEnum : struct
        {
            if (!Enum.TryParse(Input, out TEnum value) || !Enum.IsDefined(typeof(TEnum), value))
            {
                RequestMessageBox?.Invoke($"' {Input} ' is not a valid enum value.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public bool IsValidBoolValue(string Input)
        {
            if (!bool.TryParse(Input, out bool _))
            {
                RequestMessageBox?.Invoke($"' {Input} ' is not true or false.", Input, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

    }
}
