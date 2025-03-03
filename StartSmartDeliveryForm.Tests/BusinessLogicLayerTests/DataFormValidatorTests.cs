using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DAOs;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;

namespace StartSmartDeliveryForm.Tests.BusinessLogicLayerTests
{
    public class DataFormValidatorTests
    {
        //private readonly DataFormValidator _noMsgValidator = new(new SharedTestItems.NoMessageBox());
        //private DataFormValidator? _testMsgValidator;

        //[Theory]
        //[InlineData(" ", false)]
        //[InlineData("Valid", true)]
        //public void IsValidString_ShouldValidateCorrectly(string input, bool expectedResult)
        //{
        //    // Arrange
        //    string Fieldname = "Name";

        //    // Act
        //    bool Result = _noMsgValidator.IsValidString(input, Fieldname);

        //    // Assert
        //    Assert.Equal(expectedResult, Result);
        //}

        //[Fact]
        //public void IsValidString_ShouldThrowErrorMessage()
        //{
        //    // Arrange
        //    string input = " ";
        //    string Fieldname = "Name";

        //    TestMessageBox testMsgBox = new();
        //    _testMsgValidator = new(testMsgBox);

        //    // Act
        //    _testMsgValidator.IsValidString(input, Fieldname);

        //    // Assert
        //    Assert.True(testMsgBox.WasShowCalled, "MessageBox was not called");
        //    Assert.Equal("Name cannot be empty.", testMsgBox.LastMessage);
        //}

        //[Theory]
        //[InlineData(" ", false)]
        //[InlineData("Code8", true)]
        //[InlineData("Code10", true)]
        //[InlineData("Code14", true)]
        //public void IsValidEnumValue_ShouldValidateCorrectly(string input, bool expectedResult)
        //{
        //    // Arrange

        //    // Act
        //    bool Result = _noMsgValidator.IsValidEnumValue<LicenseType>(input);

        //    // Assert
        //    Assert.Equal(expectedResult, Result);
        //}

        //[Fact]
        //public void IsValidEnumValue_ShouldThrowErrorMessage()
        //{
        //    // Arrange
        //    string input = " ";
        //    TestMessageBox testMsgBox = new();
        //    _testMsgValidator = new(testMsgBox);

        //    // Act
        //    _testMsgValidator.IsValidEnumValue<LicenseType>(input);

        //    // Assert
        //    Assert.True(testMsgBox.WasShowCalled, "MessageBox was not called");
        //    Assert.Equal($"' {input} ' is not a valid enum value.", testMsgBox.LastMessage);
        //}

        //[Theory]
        //[InlineData(" ", false)]
        //[InlineData("RandomWord", false)]
        //[InlineData("0", false)]
        //[InlineData("1", false)]
        //[InlineData("True", true)]
        //[InlineData("False", true)]
        //[InlineData("true", true)]
        //[InlineData("false", true)]
        //public void IsValidBoolValue_ShouldValidateCorrectly(string input, bool expectedResult)
        //{
        //    // Arrange

        //    // Act
        //    bool Result = _noMsgValidator.IsValidBoolValue(input);

        //    // Assert
        //    Assert.Equal(expectedResult, Result);
        //}

        //[Fact]
        //public void IsValidBoolValue_ShouldThrowErrorMessage()
        //{
        //    // Arrange
        //    string input = " ";
        //    TestMessageBox testMsgBox = new();
        //    _testMsgValidator = new(testMsgBox);

        //    // Act
        //    _testMsgValidator.IsValidBoolValue(input);

        //    // Assert
        //    Assert.True(testMsgBox.WasShowCalled, "MessageBox was not called");
        //    Assert.Equal($"' {input} ' is not true or false.", testMsgBox.LastMessage);
        //}
    }
}
