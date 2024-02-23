using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaviParserLib
{
    public class DaviTextPoint
    {
        public int Line { get; set; }
        public int LineCharOffset { get; set; }

        public DaviTextPoint()
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

        public DaviTextPoint(int line, int lineCharOffset)
        {
            Line = line;
            LineCharOffset = lineCharOffset;
        }
    }

    public class DaviTextBlock
    {
        public DaviTextPoint StartPoint { get; set; }
        public DaviTextPoint EndPoint { get; set; }
    }
}
