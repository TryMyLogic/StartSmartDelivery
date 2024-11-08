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
        [InlineData("LicenseType", "NonExistentName",false, "")]

        //Invalid Enum Type
        [InlineData("NonExistentType", "Code8", false, "")]

        //Empty Search Term
        [InlineData("LicenseType", "", false, "")]

        //Null Search Term
        [InlineData("LicenseType", null, false, "")]

        //Whitespace Search Term
        [InlineData("LicenseType", "   ", false, "")]

        //Partial Match (not allowed for Enums)
        [InlineData("LicenseType", "Code", false, "")]

        //Special Characters
        [InlineData("LicenseType", "@Code8!", false,"")]

        // Numeric String (assuming it doesn't match any enum)
        [InlineData("LicenseType", "1234", false, "")]

        //Case sensitivity check (is case insensitive by default)
        [InlineData("LicenseType", "CoDe8", false,"LicenseType = 1")]

        //Valid Enum Name and Type (for LicenseType)
        [InlineData("LicenseType", "Code8", false,"LicenseType = 1")]
        [InlineData("LicenseType","Code10", false, "LicenseType = 2")]
        [InlineData("LicenseType", "Code14", false, "LicenseType = 3")]

        public void SearchEnumColumn_ShouldReturnEnumValueIfExists(string selectedOption,string searchTerm,bool isCaseSensitive, string expectedResult)
        {
            // Act
            string result = ManagementTemplateForm.SearchEnumColumn(selectedOption,searchTerm,isCaseSensitive);

            //Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
