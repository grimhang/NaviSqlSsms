using DaviSqlSsms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DaviSqlSsms_Test
{
    public class DaviParserTest_InvalidSample
    {
        private VirtualPoint caretPoint = new VirtualPoint();
        private VirtualPoint startPoint = new VirtualPoint();
        private VirtualPoint endPoint = new VirtualPoint();

        private DaviParser daviParser = new DaviParser();

        private string sampleData = File.ReadAllText($"{System.Environment.CurrentDirectory}/SampleData/{"InvalidSample01.txt"}");

        [Fact]
        public void CustomParse_BlankLine_ReturnFalse()
        {            
            caretPoint.Line = 2;
            caretPoint.LineCharOffset = 1;            

            bool result = daviParser.CustomParse(sampleData, caretPoint, ref startPoint, ref endPoint);

            Assert.False(result);
        }

        [Fact]
        public void CustomParse_FirstLine_CheckStartEnd()
        {
            caretPoint.Line = 1;
            caretPoint.LineCharOffset = 1;

            bool result = daviParser.CustomParse(sampleData, caretPoint, ref startPoint, ref endPoint);

            Assert.Equal(1, startPoint.Line);
            Assert.Equal(1, startPoint.LineCharOffset);
            Assert.Equal(1, endPoint.Line);
            Assert.Equal(3, endPoint.LineCharOffset);
        }

        [Fact]
        public void CustomParse_Select2LineStatement_CheckStartEnd()
        {
            caretPoint.Line = 3;
            caretPoint.LineCharOffset = 7;

            bool result = daviParser.CustomParse(sampleData, caretPoint, ref startPoint, ref endPoint);

            Assert.Equal(3, startPoint.Line);
            Assert.Equal(1, startPoint.LineCharOffset);
            Assert.Equal(4, endPoint.Line);
            Assert.Equal(12, endPoint.LineCharOffset);
        }

        [Fact]
        public void CustomParse_SelectLineStatement_CheckStartEnd()
        {
            caretPoint.Line = 8;
            caretPoint.LineCharOffset = 20;

            bool result = daviParser.CustomParse(sampleData, caretPoint, ref startPoint, ref endPoint);

            Assert.Equal(8, startPoint.Line);
            Assert.Equal(1, startPoint.LineCharOffset);
            Assert.Equal(8, endPoint.Line);
            Assert.Equal(20, endPoint.LineCharOffset);
        }
    }
}
