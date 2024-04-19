using NaviParserLib;
using NaviSqlSsms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test_DaviSqlSsms
{
    public class CustomParse_InperfectSample
    {
        private NaviTextPoint caretPoint = new NaviTextPoint();
        private NaviTextPoint startPoint = new NaviTextPoint();
        private NaviTextPoint endPoint = new NaviTextPoint();

        private DaviParser daviParser = new DaviParser();

        [Fact]
        public void BlankLine_ReturnFalse()
        {
            string sqlSample = @"select * from Realm
 
여기서부터 에러줄일꺼 임";


            NaviTextPoint curCaretPoint = new NaviTextPoint(2, 1);

            bool result = daviParser.CustomParse(sqlSample, curCaretPoint, ref startPoint, ref endPoint);

            Assert.False(result);
        }


        [Theory]
        [InlineData(3, 5, 3, 1, 4, 11)]
        [InlineData(4, 11, 3, 1, 4, 11)]
        public void SecondSqlOfSql3_2times(int carPosLine, int carPosOffset, int startPointLine, int startPointOffset, int endPointLine, int endPointOffset)
        {
            string sqlSample = @"select * from Ahouse

select *
from Realm

tt 영어에서
";

            NaviTextPoint curCaretPoint = new NaviTextPoint(carPosLine, carPosOffset);

            bool result = daviParser.CustomParse(sqlSample, curCaretPoint, ref startPoint, ref endPoint);

            Assert.Equal(startPointLine, startPoint.Line);
            Assert.Equal(startPointOffset, startPoint.LineCharOffset);
            Assert.Equal(endPointLine, endPoint.Line);
            Assert.Equal(endPointOffset, endPoint.LineCharOffset);
        }

        [Fact]
        public void OneLineSql()
        {
            string sqlSample = @"select * from Realm
 
여기서부터 에러줄일꺼 임";

            NaviTextPoint curCaretPoint = new NaviTextPoint(1, 5);

            bool result = daviParser.CustomParse(sqlSample, curCaretPoint, ref startPoint, ref endPoint);

            Assert.Equal(1, startPoint.Line);
            Assert.Equal(1, startPoint.LineCharOffset);
            Assert.Equal(1, endPoint.Line);
            Assert.Equal(20, endPoint.LineCharOffset);
        }

        [Fact]
        public void TwoLineSql_CarPos2()
        {
            string sqlSample = @"select *
from Realm
 
여기서부터 에러줄일꺼 임";

            NaviTextPoint curCaretPoint = new NaviTextPoint(2, 5);

            bool result = daviParser.CustomParse(sqlSample, curCaretPoint, ref startPoint, ref endPoint);

            Assert.Equal(1, startPoint.Line);
            Assert.Equal(1, startPoint.LineCharOffset);
            Assert.Equal(2, endPoint.Line);
            Assert.Equal(11, endPoint.LineCharOffset);
        }        

        [Theory]
        [InlineData(3, 9, 3, 1, 5, 48)]
        [InlineData(4, 16, 3, 1, 5, 48)]
        [InlineData(5, 48, 3, 1, 5, 48)]
        public void LastThreeLineSql_3TimesTest(int carPosLine, int carPosOffset, int startPointLine, int startPointOffset, int endPointLine, int endPointOffset)
        {
            //마지막 sql의 첫번째라인, 두번째라인, 세번째라인에 포커스를 주고 결과값테스트
            string sqlSample = @"여기서 에러가 날꺼임

select *
from Realm AS R
    join Ahouse AS AH on R.RealmId = AH.RealmId";

            NaviTextPoint curCaretPoint = new NaviTextPoint(carPosLine, carPosOffset);

            bool result = daviParser.CustomParse(sqlSample, curCaretPoint, ref startPoint, ref endPoint);

            Assert.Equal(startPointLine, startPoint.Line);
            Assert.Equal(startPointOffset, startPoint.LineCharOffset);
            Assert.Equal(endPointLine, endPoint.Line);
            Assert.Equal(endPointOffset, endPoint.LineCharOffset);
        }
    }
}
