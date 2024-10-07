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
        //LIMIT 10 OFFSET 20;

        //TODO - Update this test to match btnGotoPage_Click logic
        [Theory]
        //Three Point BVA
        [InlineData(0)] //F - out of range
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(99)]
        [InlineData(100)]
        [InlineData(101)] //F - out of range

        //Additional Test
        [InlineData(-1)] //F - is negative

        public void GotoSubmit(int GotoPage)
        {
            if (GotoPage < 0) Assert.Fail("GotoPage is negative");
            int start = 1;
            int end = 100;

            // Page must be between start and end (inclusive)
            if (GotoPage <= end && GotoPage >= start)
            {
                Assert.InRange(GotoPage, start, end);
                //GotoPageX()
            }
            else
            {
                Assert.Fail("GotoPage is out of range");
            }
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
