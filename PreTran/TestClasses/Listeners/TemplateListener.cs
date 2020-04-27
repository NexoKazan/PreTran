using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PreTran.TestClasses.Rules;

namespace PreTran.TestClasses.Listeners
{
    class TemplateListener : MySqlParserBaseListener
    {
        private int _tmpDepth;
        private int _depth;
        private bool _isMainQ = false;
        private bool _isOtherListener = false;

        public List<BaseRule> RemoveRules = new List<BaseRule>();
        public List<BaseRule> AllRules = new List<BaseRule>();

        public TemplateListener(int depth)
        {
            _tmpDepth = _depth = depth;
        }

        public override void VisitTerminal(ITerminalNode node)
        {
            if (!_isOtherListener)
            {
                TerminalRule terminal = new TerminalRule(node.SourceInterval, node.GetText(), node.Parent);
                AllRules.Add(terminal);
            }
        }

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            if (context.ChildCount > 1)
            {
                if(!_isOtherListener)
                   AllRules.Add(new BaseRule(context.SourceInterval, context, context.GetText()));
            }
        }

        public override void EnterSelectElements([NotNull] MySqlParser.SelectElementsContext context)
        {
            if (_tmpDepth == _depth)
            {
                SelectElements selectElements = new SelectElements(context.SourceInterval, context, Environment.NewLine + "SELECT-----"+ context.GetText()+"----SELECT" + Environment.NewLine);
                if (AllRules[AllRules.Count - 1].RuleType == "selectelements")
                {
                    AllRules.Remove(AllRules[AllRules.Count - 1]);
                }

                AllRules.Add(selectElements);
                _isOtherListener = true;
            }
        }

        public override void ExitSelectElements(MySqlParser.SelectElementsContext context)
        {
            if (_tmpDepth == _depth)
            {
                _isOtherListener = false;
            }
        }

        public override void EnterFromClause(MySqlParser.FromClauseContext context)
        {
            if (_tmpDepth == _depth)
            {
                FromClause fromClause = new FromClause(context.SourceInterval, context, Environment.NewLine +  context.GetText() + Environment.NewLine);
                AllRules.Remove(AllRules[AllRules.Count-1]);
                AllRules.Add(fromClause);
                _isOtherListener = true;
            }
        }

        public override void ExitFromClause(MySqlParser.FromClauseContext context)
        {
            if (_tmpDepth == _depth)
            {
                _isOtherListener = false;
            }
        }

        public override void EnterOrderByClause(MySqlParser.OrderByClauseContext context)
        {
            if (_tmpDepth == _depth)
            {
                OrderByClause fromClause = new OrderByClause(context.SourceInterval, context, Environment.NewLine + "ORDER-----"+ context.GetText()+"----ORDER" + Environment.NewLine);
                AllRules.Remove(AllRules[AllRules.Count-1]);
                AllRules.Add(fromClause);
                _isOtherListener = true;
            }
        }

        public override void ExitOrderByClause(MySqlParser.OrderByClauseContext context)
        {
            if (_tmpDepth == _depth)
            {
                _isOtherListener = false;
            }
        }

        public override void EnterRoot([NotNull] MySqlParser.RootContext context)
        {
            RemoveRules.Add(new BaseRule(context.SourceInterval, context, "ROOT "));
            AllRules.Remove(AllRules[AllRules.Count - 1]);
        }

        public override void EnterSqlStatements([NotNull] MySqlParser.SqlStatementsContext context)
        {
            RemoveRules.Add(new BaseRule(context.SourceInterval, context, "SQL_STATEMENTS "));
            AllRules.Remove(AllRules[AllRules.Count - 1]);
        }
        
        public override void EnterQuerySpecification([NotNull] MySqlParser.QuerySpecificationContext context)
        {
            if (!_isMainQ)
            {
                RemoveRules.Add(new BaseRule(context.SourceInterval, context, "QUERY_SPEC "));
                _isMainQ = true;
            }
            else
            {
                _depth++;
            }
        }

        public override void ExitQuerySpecification([NotNull] MySqlParser.QuerySpecificationContext context)
        {
            if (_depth != _tmpDepth)
            {
                _depth--;
            }
        }

    }
}
