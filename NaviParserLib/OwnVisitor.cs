using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace NaviParserLib
{    
    public class OwnVisitor : TSqlFragmentVisitor
    {
        public List<QualifiedJoin> QualifiedJoins = new List<QualifiedJoin>();
        public List<UnqualifiedJoin> UnqualifiedJoins = new List<UnqualifiedJoin>();
        public List<SearchedCaseExpression> CaseExpressions = new List<SearchedCaseExpression>();

        public override void ExplicitVisit(QualifiedJoin node)
        {
            base.ExplicitVisit(node);

            QualifiedJoins.Add(node);

            // This is the source code from the Microsoft dll
            //GenerateFragmentIfNotNull(node.FirstTableReference);

            //GenerateNewLineOrSpace(_options.NewLineBeforeJoinClause);

            //GenerateQualifiedJoinType(node.QualifiedJoinType);

            //if (node.JoinHint != JoinHint.None)
            //{
            //    GenerateSpace();
            //    JoinHintHelper.Instance.GenerateSourceForOption(_writer, node.JoinHint);
            //}

            //GenerateSpaceAndKeyword(TSqlTokenType.Join);

            ////MarkClauseBodyAlignmentWhenNecessary(_options.NewlineBeforeJoinClause);

            //NewLine(); 
            //GenerateFragmentIfNotNull(node.SecondTableReference);

            //NewLine();
            //GenerateKeyword(TSqlTokenType.On);

            //GenerateSpaceAndFragmentIfNotNull(node.SearchCondition);

        }

        public override void ExplicitVisit(UnqualifiedJoin node)
        {
            base.ExplicitVisit(node);

            UnqualifiedJoins.Add(node);

            // This is the source code from the Microsoft dll
            //GenerateFragmentIfNotNull(node.FirstTableReference);

            //List<TokenGenerator> generators = GetValueForEnumKey(_unqualifiedJoinTypeGenerators, node.UnqualifiedJoinType);
            //if (generators != null)
            //{
            //    GenerateSpace();
            //    GenerateTokenList(generators);
            //}

            //GenerateSpaceAndFragmentIfNotNull(node.SecondTableReference);
        }

        public override void ExplicitVisit(SearchedCaseExpression node)
        {
            base.ExplicitVisit(node);

            CaseExpressions.Add(node);

            // This is the source code from the Microsoft dll
            //GenerateKeyword(TSqlTokenType.Case);

            //GenerateSpaceAndFragmentIfNotNull(node.InputExpression);

            //foreach (SimpleWhenClause when in node.WhenClauses)
            //{
            //    GenerateSpaceAndFragmentIfNotNull(when);
            //}

            //if (node.ElseExpression != null)
            //{
            //    GenerateSpaceAndKeyword(TSqlTokenType.Else);
            //    GenerateSpaceAndFragmentIfNotNull(node.ElseExpression);
            //}

            //GenerateSpaceAndKeyword(TSqlTokenType.End);

            //GenerateSpaceAndCollation(node.Collation);
        }

    }
}
