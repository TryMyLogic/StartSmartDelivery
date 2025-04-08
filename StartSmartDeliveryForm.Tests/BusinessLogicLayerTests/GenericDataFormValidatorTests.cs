using System.Windows.Forms;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.Tests.BusinessLogicLayerTests
{
    public class GenericDataFormValidatorTests
    {
        private readonly GenericDataFormValidator _dataFormValidator = new();
        private string _providedText = "";
        private string _providedCaption = "";
        private MessageBoxButtons _providedButton = MessageBoxButtons.OK;
        private MessageBoxIcon _providedIcon = MessageBoxIcon.None;

        public GenericDataFormValidatorTests()
        {
            _dataFormValidator.RequestMessageBox += RequestMessageBox_EventHandler;
        }

        private void RequestMessageBox_EventHandler(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            _providedText = text;
            _providedCaption = caption;
            _providedButton = buttons;
            _providedIcon = icon;
        }

        [Theory]
        [InlineData(" ", false)]
        [InlineData("Valid", true)]
        public void IsValidString_ShouldValidateCorrectly(string input, bool expectedResult)
        {
            // Arrange
            string Fieldname = "Name";

            // Act
            bool Result = _dataFormValidator.IsValidString(input, Fieldname);

            // Assert
            Assert.Equal(expectedResult, Result);
        }

        [Fact]
        public void IsValidString_ShouldRequestMessageBox_ShowingCorrectErrorDetails()
        {
            // Arrange
            string input = " ";
            string Fieldname = "Name";

            // Act
            _dataFormValidator.IsValidString(input, Fieldname);

            // Assert
            Assert.Equal($"{Fieldname} cannot be empty.", _providedText);
            Assert.Equal("Validation Error", _providedCaption);
            Assert.Equal(MessageBoxButtons.OK, _providedButton);
            Assert.Equal(MessageBoxIcon.Error, _providedIcon);
        }

        [Theory]
        [InlineData(" ", false)]
        [InlineData("Code8", true)]
        [InlineData("Code10", true)]
        [InlineData("Code14", true)]
        public void IsValidEnumValue_ShouldValidateCorrectly(string input, bool expectedResult)
        {
            // Arrange
            Type enumType = typeof(LicenseType);

            // Act
            bool Result = _dataFormValidator.IsValidEnumValue(enumType, input);

            // Assert
            Assert.Equal(expectedResult, Result);
        }

        [Fact]
        public void IsValidEnumValue_ShouldRequestMessageBox_ShowingCorrectErrorDetails()
        {
            // Arrange
            string input = " ";
            Type enumType = typeof(LicenseType);

            // Act
            _dataFormValidator.IsValidEnumValue(enumType, input);

            // Assert
            Assert.Equal($"'{input}' is not a valid {enumType.Name} value.", _providedText);
            Assert.Equal("Validation Error", _providedCaption);
            Assert.Equal(MessageBoxButtons.OK, _providedButton);
            Assert.Equal(MessageBoxIcon.Error, _providedIcon);
        }

        [Theory]
        [InlineData(" ", false)]
        [InlineData("RandomWord", false)]
        [InlineData("0", false)]
        [InlineData("1", false)]
        [InlineData("True", true)]
        [InlineData("False", true)]
        [InlineData("true", true)]
        [InlineData("false", true)]
        public void IsValidBoolValue_ShouldValidateCorrectly(string input, bool expectedResult)
        {
            // Arrange

            // Act
            bool Result = _dataFormValidator.IsValidBoolValue(input);

            // Assert
            Assert.Equal(expectedResult, Result);
        }

        [Fact]
        public void IsValidBoolValue_ShouldRequestMessageBox_ShowingCorrectErrorDetails()
        {
            // Arrange
            string input = " ";

            // Act
            _dataFormValidator.IsValidBoolValue(input);

            // Assert
            Assert.Equal($"' {input} ' is not true or false.", _providedText);
            Assert.Equal("Validation Error", _providedCaption);
            Assert.Equal(MessageBoxButtons.OK, _providedButton);
            Assert.Equal(MessageBoxIcon.Error, _providedIcon);
        }

        [Theory]
        [InlineData(" ", false)]
        [InlineData("123", true)]
        [InlineData("NotANumber", false)]
        public void IsValidIntValue_ShouldValidateCorrectly(string input, bool expectedResult)
        {
            // Arrange
            string Fieldname = "NumberColumn";

            // Act
            bool Result = _dataFormValidator.IsValidIntValue(input, Fieldname);

            // Assert
            Assert.Equal(expectedResult, Result);
        }

        [Fact]
        public void IsValidIntValue_ShouldRequestMessageBox_ShowingCorrectErrorDetails()
        {
            // Arrange
            string input = "NotANumber";
            string Fieldname = "Age";

            // Act
            _dataFormValidator.IsValidIntValue(input, Fieldname);

            // Assert
            Assert.Equal($"' {input} ' is not a valid number.", _providedText);
            Assert.Equal("Validation Error", _providedCaption);
            Assert.Equal(MessageBoxButtons.OK, _providedButton);
            Assert.Equal(MessageBoxIcon.Error, _providedIcon);
        }
    }

}
