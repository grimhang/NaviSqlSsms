using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviParserLib
{
    public class NaviTextPoint
    {
        public int Line { get; set; }
        public int LineCharOffset { get; set; }

        public NaviTextPoint()
            : this(1, 1)
        {            
        }

        /*
        public VirtualPoint(EnvDTE.TextPoint point)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Line = point.Line;
            LineCharOffset = point.LineCharOffset;
        }
        */

        public NaviTextPoint(int line, int lineCharOffset)
        {
            Line = line;
            LineCharOffset = lineCharOffset;
        }
    }

    public class DaviTextBlock
    {
        public NaviTextPoint StartPoint { get; set; }
        public NaviTextPoint EndPoint { get; set; }
    }
}
