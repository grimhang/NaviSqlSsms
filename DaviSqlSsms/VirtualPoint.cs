using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaviSqlSsms
{
    public class VirtualPoint
    {
        public int Line { get; set; }
        public int LineCharOffset { get; set; }

        public VirtualPoint()
        {
            Line = 1;
            LineCharOffset = 0;
        }

        public VirtualPoint(EnvDTE.TextPoint point)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Line = point.Line;
            LineCharOffset = point.LineCharOffset;
        }
    }

    public class TextBlock
    {
        public VirtualPoint StartPoint { get; set; }
        public VirtualPoint EndPoint { get; set; }
    }
}
