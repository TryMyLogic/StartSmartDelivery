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
        [InlineData("DriverID", "1", "DriverID = 1")]
        [InlineData("Name", "John", "Name LIKE '%John%'")]
        [InlineData("LicenseType", "Code8",     "LicenseType = 1")]
        [InlineData("Availability", "True", "Availability = true")]

        //Invalid returns empty string
        [InlineData("DriverID", "", "")]
        [InlineData("Name", "", "")]
        [InlineData("LicenseType", "", "")]
        [InlineData("Availability", "", "")]

        //Boolean as numbers and weird cases
        [InlineData("Availability", "TrUe", "Availability = true")]
        [InlineData("Availability", "False", "Availability = false")]
        [InlineData("Availability", "1", "Availability = true")]
        [InlineData("Availability", "0", "Availability = false")]

        //Case sensitive string matching
        [InlineData("Name", "JoHn", "Name LIKE '%JoHn%'")]

        //Invalid column
        [InlineData("InvalidColumn", "Test", "")]

        //Special characters
        [InlineData("Name", "O'Reilly", "Name LIKE '%O''Reilly%'")]
        [InlineData("Name", "O'Riley", "Name LIKE '%O''Riley%'")]

        //Null input
        [InlineData("Name", null,  "")]
        [InlineData("Name", "null",  "Name LIKE '%null%'")]

        public void BuildFilterExpression_ShouldReturnCorrectExpression(string selectedOption, string searchTerm, string expectedExpression)
        {
            // Arrange
            DataTable dataTable = new();
            dataTable.Columns.Add("DriverID", typeof(int));         // DriverID as integer
            dataTable.Columns.Add("Name", typeof(string));          // Name as string
            dataTable.Columns.Add("LicenseType", typeof(int));      // LicenseType as int (enum values stored as integers)
            dataTable.Columns.Add("Availability", typeof(bool));    // Availability as boolean

            dataTable.Rows.Add(1, "John", (int)LicenseType.Code8, true);

            // Act
            string filterExpression = ManagementTemplateForm.BuildFilterExpression(dataTable, selectedOption, searchTerm);

            // Assert
            Assert.Equal(expectedExpression, filterExpression);
        }

        [Theory]
        //Invalid Enum Name
        [InlineData("LicenseType", "NonExistentName", "")]

        //Invalid Enum Type
        [InlineData("NonExistentType", "Code8",  "")]

        //Empty Search Term
        [InlineData("LicenseType", "", "")]

        //Null Search Term
        [InlineData("LicenseType", null,  "")]

        //Whitespace Search Term
        [InlineData("LicenseType", "   ", "")]

        //Partial Match (not allowed for Enums)
        [InlineData("LicenseType", "Code", "")]

        //Special Characters
        [InlineData("LicenseType", "@Code8!", "")]

        // Numeric String (assuming it doesn't match any enum)
        [InlineData("LicenseType", "1234",  "")]

        //Valid Enum Name and Type (for LicenseType)
        [InlineData("LicenseType", "Code8", "LicenseType = 1")]
        [InlineData("LicenseType","Code10",  "LicenseType = 2")]
        [InlineData("LicenseType", "Code14",  "LicenseType = 3")]

        public void SearchEnumColumn_ShouldReturnEnumValueIfExists(string selectedOption,string searchTerm, string expectedResult)
        {
            // Act
            string result = ManagementTemplateForm.SearchEnumColumn(selectedOption,searchTerm);

            //Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
