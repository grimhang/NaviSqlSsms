using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace NaviSqlSsms
{
    static class Helpers
    {
        public static bool HasActiveDocument(this DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (dte != null && dte.ActiveDocument != null)
            {
                var doc = (dte.ActiveDocument.DTE)?.ActiveDocument;
                return doc != null;
            }

            return false;
        }

        public static EnvDTE.Document GetDocument(this DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (dte.HasActiveDocument())
            {
                return (dte.ActiveDocument.DTE)?.ActiveDocument;
            }

            return null;
        }
    }
}
