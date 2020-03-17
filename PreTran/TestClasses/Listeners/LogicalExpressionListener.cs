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
    class LogicalExpressionListener : MySqlParserBaseListener
    {
        private int _tmpDepth;
        private int _depth;
        private bool _isMainQ = false;
        private bool _isFirst = true;
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

        public override void EnterLogicalExpression(MySqlParser.LogicalExpressionContext context)
        {
            if (_isOtherListener == 1 && _isFirst == false)
            {
                LogicalExpression logicalExpression =
                    new LogicalExpression(context.SourceInterval, context, context.GetText());
                Rules.Remove(Rules[Rules.Count - 1]);
                Rules.Add(logicalExpression);
            }

            if (_isFirst == false)
            {
                _isOtherListener++;
            }

            if (_isOtherListener == 1 && Rules.Count > 0 && _isFirst)
            {
                Rules.Remove(Rules[Rules.Count - 1]);
                _isFirst = false;
            }
        }

        public override void ExitLogicalExpression(MySqlParser.LogicalExpressionContext context)
        {
            if (_isFirst == false)
            {
                _isOtherListener--;
            }
        }

        public override void EnterBinaryComparasionPredicate(MySqlParser.BinaryComparasionPredicateContext context)
        {
            if (_isOtherListener == 1)
            {
                BinaryComparasionPredicate binaryComparasionPredicate =
                    new BinaryComparasionPredicate(context.SourceInterval, context, context.GetText());
                Rules.Remove(Rules[Rules.Count - 1]);
                Rules.Add(binaryComparasionPredicate);
                
            }
            _isOtherListener++;
        }

        public override void ExitBinaryComparasionPredicate(MySqlParser.BinaryComparasionPredicateContext context)
        {
            _isOtherListener--;
        }

        public override void EnterLikePredicate(MySqlParser.LikePredicateContext context)
        {
            if (_isOtherListener == 1)
            {
                LikePredicate likePredicate =
                    new LikePredicate(context.SourceInterval, context, context.GetText());
                Rules.Remove(Rules[Rules.Count - 1]);
                Rules.Add(likePredicate);
            }
            _isOtherListener++;
        }

        public override void ExitLikePredicate(MySqlParser.LikePredicateContext context)
        {
            _isOtherListener--;
        }

        public override void EnterExistsExpessionAtom(MySqlParser.ExistsExpessionAtomContext context)
        {
            if (_isOtherListener == 1)
            {
                if (context.ChildCount > 1)
                {
                    Rules.Remove(Rules[Rules.Count - 1]);
                }

                ExistsExpessionAtom existsExpessionAtom =
                    new ExistsExpessionAtom(context.SourceInterval, context, context.GetText());

                Rules.Add(existsExpessionAtom);

            }
            _isOtherListener++;
        }

        public override void ExitExistsExpessionAtom(MySqlParser.ExistsExpessionAtomContext context)
        {
            _isOtherListener--;
        }

        public override void EnterBetweenPredicate(MySqlParser.BetweenPredicateContext context)
        {
            if (_isOtherListener == 1)
            {
                if (context.ChildCount > 1)
                {
                    Rules.Remove(Rules[Rules.Count - 1]);
                }

                BetweenPredicate betweenPredicate =
                    new BetweenPredicate(context.SourceInterval, context, context.GetText());

                Rules.Add(betweenPredicate);

            }
            _isOtherListener++;
        }

        public override void ExitBetweenPredicate(MySqlParser.BetweenPredicateContext context)
        {
            _isOtherListener--;
        }
    }
}
