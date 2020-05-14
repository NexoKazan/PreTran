﻿using System;
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
    class OuterJoinListener : MySqlParserBaseListener
    {

        private int _tmpDepth;
        private int _depth;
        private bool _isMainQ = false;
        private bool _isFirst = true;
        private int _isOtherListener = 1;

        public List<BaseRule> Rules = new List<BaseRule>();

        public override void VisitTerminal(ITerminalNode node)
        {
            if (_isOtherListener == 1)
            {
                TerminalRule terminal = new TerminalRule(node.SourceInterval, node.GetText(), node.Parent);
                Rules.Add(terminal);
            }
        }

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            if (context.ChildCount > 1)
            {
                if (_isOtherListener == 1)
                    Rules.Add(new BaseRule(context.SourceInterval, context, context.GetText()));
            }
        }

        public override void EnterTableSources(MySqlParser.TableSourcesContext context)
        {
            if (_isOtherListener == 1 && Rules.Count > 0 && _isFirst)
            {
                Rules.Remove(Rules[Rules.Count - 1]);
                _isFirst = false;
            }
        }

        public override void EnterOuterJoin(MySqlParser.OuterJoinContext context)
        {
            if (_isOtherListener == 1 && Rules.Count > 0 && _isFirst)
            {
                Rules.Remove(Rules[Rules.Count - 1]);
                _isFirst = false;
            }
        }

        public override void EnterAtomTableItem(MySqlParser.AtomTableItemContext context)
        {
            if (_isOtherListener == 1)
            {
                if (context.ChildCount > 1)
                {
                    Rules.Remove(Rules[Rules.Count - 1]);
                }

                AtomTableItem atomTableItem =
                    new AtomTableItem(context.SourceInterval, context, context.GetText());

                Rules.Add(atomTableItem);

            }
            _isOtherListener++;
        }

        public override void ExitAtomTableItem(MySqlParser.AtomTableItemContext context)
        {
            if (_isOtherListener == 1)
            {
                if (context.ChildCount > 1)
                {
                    Rules.Remove(Rules[Rules.Count - 1]);
                }

                OuterJoin outerJoin =
                    new OuterJoin(context.SourceInterval, context, context.GetText());

                Rules.Add(outerJoin);

            }
            _isOtherListener--;
        }

        public override void EnterLogicalExpression(MySqlParser.LogicalExpressionContext context)
        {
            if (_isOtherListener == 1)
            {
                LogicalExpression logicalExpression =
                    new LogicalExpression(context.SourceInterval, context, context.GetText());
                Rules.Remove(Rules[Rules.Count - 1]);
                Rules.Add(logicalExpression);
            }
            _isOtherListener++;
        }

        public override void ExitLogicalExpression(MySqlParser.LogicalExpressionContext context)
        {
            _isOtherListener--;
        }
    }
}