using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.Enums;

namespace StartSmartDeliveryForm.Tests
{
    public class ManagementTemplateTests
    {

        [Theory]
        //Three Point BVA
        [InlineData("0")] //F - out of range
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("99")]
        [InlineData("100")]
        [InlineData("101")] //F - out of range
        public void btnGotoPage_Click_HandlesRangeCorrectly(string txtGotoPage)
        {
            int _currentPage = 5;
            int _totalPages = 100;
            bool ParsedGoto = int.TryParse(txtGotoPage, out int GotoPage);
            if (ParsedGoto)
            {
                if (GotoPage == _currentPage)
                {
                    Assert.True(GotoPage == _currentPage, "Already on that page, no action expected");
                    return;
                } 

                if (GotoPage >= 1 && GotoPage <= _totalPages)
                {
                    Assert.InRange(GotoPage, 1, _totalPages);
                }
                else
                {
                    Assert.Fail("GotoPage is out of range");
                    return;
                }
            }
            Assert.True(ParsedGoto, "String did not parse to int");
        }

        [Fact]
        public void btnGotoPage_Click_SetsLabelCorrectly()
        {
            int _currentPage = 5;
            int _totalPages = 100;
            string lblStartEndPages = $"{_currentPage}/{_totalPages}";

            Assert.Equal("5/100", lblStartEndPages);
        }

        //Example
        // Page 1: Records 1 to 11
        // Page 2: Records 12 to 22
        // Page 3: Records 23 to 33
        // Page 4: Records 34 to 44
        // Page 5: Records 45 to 50 (This page will have 6 records)
        [Theory]
        [InlineData(10, 0, 0)]
        [InlineData(10, 50, 5)]
        [InlineData(11, 50, 5)]

        public void GetTotalPages(int pagesize, int recordscount, int expectedTotal)
        {
            if (pagesize < 0) Assert.Fail("pagesize cannot be negative");

            int Total = (int)Math.Ceiling((double)recordscount / pagesize);
            Assert.Equal(expectedTotal, Total);
        }
    }


}
