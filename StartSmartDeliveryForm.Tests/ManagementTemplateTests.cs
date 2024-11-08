using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.PresentationLayer;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;

namespace StartSmartDeliveryForm.Tests
{
    public class ManagementTemplateTests
    {
        [Theory]
        //Valid 
        [InlineData("DriverID", "1", false, "DriverID = 1")]
        [InlineData("Name", "John", false, "Name LIKE '%John%'")]
        [InlineData("LicenseType", "Code8", false, "LicenseType = 1")]
        [InlineData("Availability", "True", false, "Availability = true")]

        //Invalid returns empty string
        [InlineData("DriverID", "", false, "")]
        [InlineData("Name", "", false, "")]
        [InlineData("LicenseType", "", false, "")]
        [InlineData("Availability", "", false, "")]

        //Enum case sensitivity
        [InlineData("LicenseType", "code8", true, "")]
        //False already done above

        //Boolean as numbers and weird cases
        [InlineData("Availability", "TrUe", false, "Availability = true")]
        [InlineData("Availability", "False", false, "Availability = false")]
        [InlineData("Availability", "1", false, "Availability = true")]
        [InlineData("Availability", "0", false, "Availability = false")]

        //Case sensitive string matching
        [InlineData("Name", "JoHn", true, "Name LIKE '%JoHn%'")]

        //Invalid column
        [InlineData("InvalidColumn", "Test", false, "")]

        //Special characters
        [InlineData("Name", "O'Reilly", false, "Name LIKE '%O''Reilly%'")]
        [InlineData("Name", "O'Riley", false, "Name LIKE '%O''Riley%'")]

        //Null input
        [InlineData("Name", null, false, "")]
        [InlineData("Name", "null", false, "Name LIKE '%null%'")]

        public void BuildFilterExpression_ShouldReturnCorrectExpression(string selectedOption, string searchTerm, bool isCaseSensitive, string expectedExpression)
        {
            // Arrange
            DataTable dataTable = new();
            dataTable.Columns.Add("DriverID", typeof(int));         // DriverID as integer
            dataTable.Columns.Add("Name", typeof(string));          // Name as string
            dataTable.Columns.Add("LicenseType", typeof(int));      // LicenseType as int (enum values stored as integers)
            dataTable.Columns.Add("Availability", typeof(bool));    // Availability as boolean

            dataTable.Rows.Add(1, "John", (int)LicenseType.Code8, true);

            // Act
            string filterExpression = ManagementTemplateForm.BuildFilterExpression(dataTable, selectedOption, searchTerm, isCaseSensitive);

            // Assert
            Assert.Equal(expectedExpression, filterExpression);
        }

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
