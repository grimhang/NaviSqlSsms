using System;

namespace DaviParserLib
{
    public class DaviParser
    {
        public bool CustomParse(string script, DaviTextPoint caretPoint, ref DaviTextPoint startPoint, ref DaviTextPoint endPoint)
        {
            string[] strLineArr = script.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            // 현재 라인이 공백이 아닌경우에만
            if (!string.IsNullOrWhiteSpace(strLineArr[caretPoint.Line - 1]))
            {
                // 01. startPoint 구하기
                if (caretPoint.Line > 1)
                {
                    // 상위라인이 공백인 경우 찾을때까지 루핑해 startPoint 채움
                    for (int curLine = caretPoint.Line; curLine > 1; curLine--)
                    {
                        if (string.IsNullOrWhiteSpace(strLineArr[curLine - 2])) // 윗줄이 공백이면
                        {
                            startPoint.Line = curLine;
                            startPoint.LineCharOffset = 1;
                            break;
                        }
                    }

                    /*
                    for (int curLine = (caretPoint.Line - 1); curLine > 0; curLine--)
                    {
                        if (!string.IsNullOrWhiteSpace(strLineArr[curLine]) && string.IsNullOrWhiteSpace(strLineArr[curLine - 1]))
                        {
                            startPoint.Line = curLine + 1;
                            startPoint.LineCharOffset = 1;
                            break;
                        }
                    }
                    */
                }

                // 02. endPoint 구하기
                if (caretPoint.Line == strLineArr.Length) // 현재 위치가 마지막 라인이라면
                {
                    endPoint.Line = caretPoint.Line;
                    endPoint.LineCharOffset = strLineArr[caretPoint.Line - 1].Length + 1;
                }
                else
                {
                    // 현재 위치가 끝에서 두번째 라인이거나 또는 다음 라인이 공백이면 endPoint에 저장
                    for (int curLine = caretPoint.Line; curLine < strLineArr.Length; curLine++)
                    {
                        if (string.IsNullOrWhiteSpace(strLineArr[curLine]))
                        {
                            endPoint.Line = curLine;
                            endPoint.LineCharOffset = strLineArr[curLine - 1].Length + 1;
                            break;
                        }

                        if (curLine == strLineArr.Length - 1)
                        {
                            endPoint.Line = (curLine + 1);
                            endPoint.LineCharOffset = strLineArr[curLine].Length + 1;
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
