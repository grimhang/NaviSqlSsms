using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviSqlSsms
{
    public class VirtualPointNotuse2
    {
        public int Line { get; set; }
        public int LineCharOffset { get; set; }

        public VirtualPointNotuse2()
        {
            Line = 1;
            LineCharOffset = 0;
        }

        /*
        public VirtualPoint(EnvDTE.TextPoint point)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Line = point.Line;
            LineCharOffset = point.LineCharOffset;
        }
        */

        public VirtualPointNotuse2(int line, int lineCharOffset)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Line = line;
            LineCharOffset = lineCharOffset;
        }
    }

    public class TextBlockNotUse2
    {
        public VirtualPointNotuse2 StartPoint { get; set; }
        public VirtualPointNotuse2 EndPoint { get; set; }
    }
}
