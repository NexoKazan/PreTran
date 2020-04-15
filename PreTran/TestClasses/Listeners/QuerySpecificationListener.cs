using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PreTran.TestClasses.Rules;

namespace PreTran.TestClasses.Listeners
{
    class QuerySpecificationListener : MySqlParserBaseListener
    {
        
        private int _tmpDepth;
        private int _depth;
        private bool _isMainQ = false;
        private bool _isFirst = true;
        private int _isOtherListener = 1;

        public QuerySpecification ownRule;
        public List<BaseRule> Rules = new List<BaseRule>();

        public override void VisitTerminal(ITerminalNode node)
        {
            if (_isOtherListener==1)
            {
                TerminalRule terminal = new TerminalRule(node.SourceInterval, node.GetText(), node.Parent);
                Rules.Add(terminal);
            }
        }

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            if (context.ChildCount > 1)
            {
                if(_isOtherListener == 1)
                    Rules.Add(new BaseRule(context.SourceInterval, context, context.GetText()));
            }
        }

        public override void EnterQuerySpecification(MySqlParser.QuerySpecificationContext context)
        {
            if (!_isFirst)
            {
                if (_isOtherListener == 1)
                {
                    QuerySpecification querySpecification =
                        new QuerySpecification(context.SourceInterval, context, context.GetText());
                    if (Rules.Count > 0)
                    {
                        Rules.Remove(Rules[Rules.Count - 1]);
                    }

                    Rules.Add(querySpecification);
                }

                _isOtherListener++;
            }
            if (_isOtherListener == 1 && Rules.Count > 0 && _isFirst)
            {
                //ownRule = new QuerySpecification(context.SourceInterval, context, context.GetText());
                Rules.Remove(Rules[Rules.Count - 1]);
                _isFirst = false;
            }
        }

        public override void EnterSelectElements([NotNull] MySqlParser.SelectElementsContext context)
        {
            if (_tmpDepth == _depth)
            {
                SelectElements selectElements = new SelectElements(context.SourceInterval, context, Environment.NewLine + "SELECT-----" + context.GetText() + "----SELECT" + Environment.NewLine);
                if (Rules[Rules.Count - 1].RuleType == "selectelements")
                {
                    Rules.Remove(Rules[Rules.Count - 1]);
                }

                Rules.Add(selectElements);
                _isOtherListener ++;
            }
        }

        public override void ExitSelectElements(MySqlParser.SelectElementsContext context)
        {
            if (_tmpDepth == _depth)
            {
                _isOtherListener--;
            }
        }

        public override void EnterFromClause(MySqlParser.FromClauseContext context)
        {
            if (_tmpDepth == _depth)
            {
                FromClause fromClause = new FromClause(context.SourceInterval, context, Environment.NewLine + "FROM-----"+ context.GetText()+"----FROM "+ Environment.NewLine);
                Rules.Remove(Rules[Rules.Count-1]);
                Rules.Add(fromClause);
                _isOtherListener ++;
            }
        }

        public override void ExitFromClause(MySqlParser.FromClauseContext context)
        {
            if (_tmpDepth == _depth)
            {
                _isOtherListener--;
            }
        }

        public override void EnterOrderByClause(MySqlParser.OrderByClauseContext context)
        {
            if (_tmpDepth == _depth)
            {
                OrderByClause fromClause = new OrderByClause(context.SourceInterval, context, Environment.NewLine + "ORDER-----"+ context.GetText()+"----ORDER" + Environment.NewLine);
                Rules.Remove(Rules[Rules.Count-1]);
                Rules.Add(fromClause);
                _isOtherListener ++;
            }
        }

        public override void ExitOrderByClause(MySqlParser.OrderByClauseContext context)
        {
            if (_tmpDepth == _depth)
            {
                _isOtherListener --;
            }
        }
    }
}
