using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.SharedLayer;

namespace StartSmartDeliveryForm.Tests
{
    public class ManagementTemplateTests
    {
        [Theory]
        //Invalid Enum Name
        [InlineData("LicenseType", "NonExistentName", "")]

        //Invalid Enum Type
        [InlineData("NonExistentType", "Code8", "")]

        //Empty Search Term
        [InlineData("LicenseType", "", "")]

        //Null Search Term
        [InlineData("LicenseType", null, "")]

        //Whitespace Search Term
        [InlineData("LicenseType", "   ", "")]

        //Partial Match (not allowed for Enums)
        [InlineData("LicenseType", "Code", "")]

        //Special Characters
        [InlineData("LicenseType", "@Code8!", "")]

        // Numeric String (assuming it doesn't match any enum)
        [InlineData("LicenseType", "1234", "")]

        //Case sensitivity check (must be case insensitive when match case button is deselected)
        [InlineData("LicenseType", "CoDe8", "LicenseType = 1")]

        //Valid Enum Name and Type (for LicenseType)
        [InlineData("LicenseType", "Code8", "LicenseType = 1")]
        [InlineData("LicenseType","Code10", "LicenseType = 2")]
        [InlineData("LicenseType", "Code14", "LicenseType = 3")]

        public void SearchEnumColumn_ShouldReturnEnumValueIfExists(string selectedOption,string searchTerm,string expectedResult)
        {
            // Act
            string result = ManagementTemplateForm.SearchEnumColumn(selectedOption,searchTerm);

            //Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
