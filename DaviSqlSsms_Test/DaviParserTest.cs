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
    public class DaviParserTest
    {
        private VirtualPoint caretPoint = new VirtualPoint();
        private VirtualPoint startPoint = new VirtualPoint();
        private VirtualPoint endPoint = new VirtualPoint();

        private DaviParser daviParser = new DaviParser();

        [Fact]
        public void CustomParse_IncorrectSample_BlankLine_ReturnFalse()
        {            
            caretPoint.Line = 2;
            caretPoint.LineCharOffset = 1;            
            string script = File.ReadAllText($"{System.Environment.CurrentDirectory}/SampleData/{"IncorrectSample01.txt"}");

            bool result = daviParser.CustomParse(script, caretPoint, ref startPoint, ref endPoint);

            Assert.False(result);
        }

        [Fact]
        public void CustomParse_IncorrectSample_FirstLineIncorrectSample_CheckStartEnd()
        {
            caretPoint.Line = 1;
            caretPoint.LineCharOffset = 1;
            string script = File.ReadAllText($"{System.Environment.CurrentDirectory}/SampleData/{"IncorrectSample01.txt"}");

            bool result = daviParser.CustomParse(script, caretPoint, ref startPoint, ref endPoint);

            Assert.Equal(1, startPoint.Line);
            Assert.Equal(1, startPoint.LineCharOffset);
            Assert.Equal(1, endPoint.Line);
            Assert.Equal(3, endPoint.LineCharOffset);
        }
    }
}
