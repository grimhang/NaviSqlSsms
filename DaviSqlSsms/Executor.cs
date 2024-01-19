using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;

using EnvDTE;
using EnvDTE80;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DaviSqlSsms
{
    sealed class Executor
    {
        public readonly string CMD_QUERY_EXECUTE = "Query.Execute";

        private Document document;

        private EditPoint oldAnchor;
        private EditPoint oldActivePoint;

        public Executor(DTE2 dte)
        {
            if (dte == null) throw new ArgumentNullException(nameof(dte));

            document = dte.GetDocument();

            SaveActiveAndAnchorPoints();
        }

        private VirtualPoint GetCaretPoint()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var p = ((TextSelection)document.Selection).ActivePoint;

            return new VirtualPoint(p.Line, p.LineCharOffset);
        }

        private string GetDocumentText()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var content = string.Empty;
            var selection = (TextSelection)document.Selection;

            if (!selection.IsEmpty)
            {
                content = selection.Text;
            }
            else
            {
                if (document.Object("TextDocument") is TextDocument doc)
                {
                    content = doc.StartPoint.CreateEditPoint().GetText(doc.EndPoint);
                }
            }

            return content;
        }

        private void SaveActiveAndAnchorPoints()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var selection = (TextSelection)document.Selection;

            oldAnchor = selection.AnchorPoint.CreateEditPoint();
            oldActivePoint = selection.ActivePoint.CreateEditPoint();
        }

        private void RestoreActiveAndAnchorPoints()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var startPoint = new VirtualPoint(oldAnchor.Line, oldAnchor.LineCharOffset);
            var endPoint = new VirtualPoint(oldActivePoint.Line, oldActivePoint.LineCharOffset);

            MakeSelection(startPoint, endPoint);
        }

        private void MakeSelection(VirtualPoint startPoint, VirtualPoint endPoint)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var selection = (TextSelection)document.Selection;

            selection.MoveToLineAndOffset(startPoint.Line, startPoint.LineCharOffset);
            selection.SwapAnchor();
            selection.MoveToLineAndOffset(endPoint.Line, endPoint.LineCharOffset, true);
        }

        private bool ParseSqlFragments(string script, out TSqlScript sqlFragments)
        {
            IList<ParseError> errors;
            TSql140Parser parser = new TSql140Parser(true);
            //TSql160Parser parser = new TSql160Parser(true);

            using (System.IO.StringReader reader = new System.IO.StringReader(script))
            {
                sqlFragments = parser.Parse(reader, out errors) as TSqlScript;
            }

            return errors.Count == 0;
        }

        private IList<TSqlStatement> GetInnerStatements(TSqlStatement statement)
        {
            List<TSqlStatement> list = new List<TSqlStatement>();

            if (statement is BeginEndBlockStatement block)
            {
                list.AddRange(block.StatementList.Statements);
            }
            else if (statement is IfStatement ifBlock)
            {
                if (ifBlock.ThenStatement != null)
                {
                    list.Add(ifBlock.ThenStatement);
                }
                if (ifBlock.ElseStatement != null)
                {
                    list.Add(ifBlock.ElseStatement);
                }
            }
            else if (statement is WhileStatement whileBlock)
            {
                list.Add(whileBlock.Statement);
            }

            return list;
        }

        private bool IsCaretInsideStatement(TSqlStatement statement, VirtualPoint caret)
        {
            var ft = statement.ScriptTokenStream[statement.FirstTokenIndex];
            var lt = statement.ScriptTokenStream[statement.LastTokenIndex];

            if (caret.Line >= ft.Line && caret.Line <= lt.Line)
            {
                var isBeforeFirstToken = caret.Line == ft.Line && caret.LineCharOffset < ft.Column;
                var isAfterLastToken = caret.Line == lt.Line && caret.LineCharOffset > lt.Column + lt.Text.Length;

                if (!(isBeforeFirstToken || isAfterLastToken))
                {
                    return true;
                }
            }

            return false;
        }

        private TextBlock GetTextBlockFromStatement(TSqlStatement statement)
        {
            var ft = statement.ScriptTokenStream[statement.FirstTokenIndex];
            var lt = statement.ScriptTokenStream[statement.LastTokenIndex];

            return new TextBlock()
            {
                StartPoint = new VirtualPoint
                {
                    Line = ft.Line,
                    LineCharOffset = ft.Column
                },

                EndPoint = new VirtualPoint
                {
                    Line = lt.Line,
                    LineCharOffset = lt.Column + lt.Text.Length
                }
            };
        }

        private TextBlock FindCurrentStatement(IList<TSqlStatement> statements, VirtualPoint caret, ExecScope scope)
        {
            if (statements == null || statements.Count == 0)
            {
                return null;
            }

            foreach (var statement in statements)
            {
                if (scope == ExecScope.Inner)
                {
                    IList<TSqlStatement> statementList = GetInnerStatements(statement);

                    TextBlock currentStatement = FindCurrentStatement(statementList, caret, scope);

                    if (currentStatement != null)
                    {
                        return currentStatement;
                    }
                }

                if (IsCaretInsideStatement(statement, caret))
                {
                    return GetTextBlockFromStatement(statement);
                }
            }

            return null;
        }

        private void Exec()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            document.DTE.ExecuteCommand(CMD_QUERY_EXECUTE);
        }

        private bool CanExecute()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var cmd = document.DTE.Commands.Item(CMD_QUERY_EXECUTE, -1);
                return cmd.IsAvailable;
            }
            catch
            { }

            return false;
        }

        public void ExecuteStatement(ExecScope scope = ExecScope.Block)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!CanExecute())
            {
                return;
            }

            SaveActiveAndAnchorPoints();

            var selection = (TextSelection)document.Selection;

            if (!selection.IsEmpty)
            {
                Exec();
            }
            else
            {
                var script = GetDocumentText();
                var caretPoint = GetCaretPoint();

                bool success = ParseSqlFragments(script, out TSqlScript sqlScript);

                if (success)
                {
                    TextBlock currentStatement = null;

                    foreach (var batch in sqlScript?.Batches)
                    {
                        currentStatement = FindCurrentStatement(batch.Statements, caretPoint, scope);

                        if (currentStatement != null)
                        {
                            break;
                        }
                    }

                    if (currentStatement != null)
                    {
                        // select the statement to be executed
                        MakeSelection(currentStatement.StartPoint, currentStatement.EndPoint);

                        // execute the statement
                        //Exec();

                        // restore selection
                        //RestoreActiveAndAnchorPoints();       //여기가 핵심
                    }
                }
                else
                {
                    // 편집기 전체내용중에 오류가 있는 문장이 한개라도 있다면 tsqlparser가 작동이 안되기 때문에 
                    // 강제로 현재 문장의 위/아래쪽 공백까지 선택하게

                    VirtualPoint startPoint = new VirtualPoint();
                    VirtualPoint endPoint = new VirtualPoint();


                    //string[] lines = script.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                    var daviParser = new DaviParser();

                    bool result = daviParser.CustomParse(script, caretPoint, ref startPoint, ref endPoint);

                    if (result)
                    {
                        MakeSelection(startPoint, endPoint);
                    }

                    /*
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
                    */

                    



                    /*
                    if (currentStatement != null)
                    {
                        // select the statement to be executed
                        MakeSelection(currentStatement.StartPoint, currentStatement.EndPoint);
                    }
                    */
                }
            }
        }

        //public void ExecuteStatement(ExecScope scope = ExecScope.Block)
        //{
        //    ThreadHelper.ThrowIfNotOnUIThread();
        //    if (!CanExecute())
        //    {
        //        return;
        //    }

        //    //SaveActiveAndAnchorPoints();

        //    if (!(document.Selection as TextSelection).IsEmpty)
        //    {
        //        Exec();
        //    }            
        //}

        /*public void SelectStatement(ExecScope scope = ExecScope.Block)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!CanExecute())
            {
                return;
            }

            SaveActiveAndAnchorPoints();


            var script = GetDocumentText();
            var caretPoint = GetCaretPoint();

            bool success = ParseSqlFragments(script, out TSqlScript sqlScript);

            if (success)
            {
                TextBlock currentStatement = null;

                foreach (var batch in sqlScript?.Batches)
                {
                    currentStatement = FindCurrentStatement(batch.Statements, caretPoint, scope);

                    if (currentStatement != null)
                    {
                        break;
                    }
                }

                if (currentStatement != null)
                {
                    // select the statement to be executed
                    MakeSelection(currentStatement.StartPoint, currentStatement.EndPoint);

                    // execute the statement
                    //Exec();

                    // restore selection
                    //RestoreActiveAndAnchorPoints();       //여기가 핵심
                }
            }

        }
        */

        internal enum ExecScope
        {
            Block,
            Inner
        }
    }
}
