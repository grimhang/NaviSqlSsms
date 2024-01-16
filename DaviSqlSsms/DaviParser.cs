using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DaviSqlSsms.Executor;

namespace DaviSqlSsms
{
    public class DaviParser
    {
        public bool CustomParse(string script, VirtualPoint caretPoint, ref VirtualPoint startPoint, ref VirtualPoint endPoint)
        {
            string[] lines = script.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            //VirtualPoint startPoint = new VirtualPoint();
            //VirtualPoint endPoint = new VirtualPoint();

            if (!string.IsNullOrEmpty(lines[caretPoint.Line - 1].Trim()))
            {
                if (caretPoint.Line == 1) // 첫번째 라인이라면
                {
                    startPoint.Line = 1;
                    startPoint.LineCharOffset = 1;
                }
                else
                {
                    // 현재 라인 공백이 아닌경우에 상위라인이 공백인 경우 찾을때까지 루핑해 startPoint 채움
                    for (int currentLine = (caretPoint.Line - 1); currentLine > 0; currentLine--)
                    {
                        if (!string.IsNullOrEmpty(lines[currentLine].Trim()) && string.IsNullOrEmpty(lines[currentLine - 1].Trim()))
                        {
                            startPoint.Line = currentLine + 1;
                            startPoint.LineCharOffset = 1;
                            break;
                        }
                    }
                }

                if (caretPoint.Line == lines.Length) // 마지막 라인이라면
                {
                    endPoint.Line = caretPoint.Line;
                    endPoint.LineCharOffset = lines[caretPoint.Line - 1].Length + 1;
                }
                else
                {
                    // 내 라인은 공백이 아니고 아래라인은 공백일때까지 루핑해 endPoint를 채움
                    for (int currentLine = (caretPoint.Line - 1); currentLine < (lines.Length - 1); currentLine++)
                    {
                        if (!string.IsNullOrEmpty(lines[currentLine].Trim()) && string.IsNullOrEmpty(lines[currentLine + 1].Trim()))
                        {
                            endPoint.Line = (currentLine + 1);
                            endPoint.LineCharOffset = lines[currentLine].Length + 1;
                            break;
                        }
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
