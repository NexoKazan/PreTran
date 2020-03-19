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
    class NestedExpressionAtomListener : MySqlParserBaseListener
    {
        private int _tmpDepth;
        private int _depth;
        private bool _isMainQ = false;
        private int _isOtherListener = 1;

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

        public override void EnterNestedExpressionAtom(MySqlParser.NestedExpressionAtomContext context)
        {
            if (_isOtherListener == 1 && Rules.Count>0)
            {
                Rules.Remove(Rules[Rules.Count - 1]);
            }
        }

        public override void EnterMathExpressionAtom(MySqlParser.MathExpressionAtomContext context)
        {
            if (_isOtherListener == 1)
            {
                MathExpressionAtom mathExpressionAtom =
                    new MathExpressionAtom(context.SourceInterval, context, context.GetText());
                Rules.Remove(Rules[Rules.Count - 1]);
                Rules.Add(mathExpressionAtom);
            }
            _isOtherListener++;

        }

        public override void ExitMathExpressionAtom(MySqlParser.MathExpressionAtomContext context)
        {
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
