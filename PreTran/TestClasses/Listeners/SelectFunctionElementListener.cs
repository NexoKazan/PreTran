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
    class SelectFunctionElementListener : MySqlParserBaseListener
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

        public override void EnterSelectFunctionElement(MySqlParser.SelectFunctionElementContext context)
        {
            if (_isOtherListener == 1 && Rules.Count > 0 && _isFirst)
            {
                Rules.Remove(Rules[Rules.Count - 1]);
                _isFirst = false;
            }

        }

        public override void EnterAggregateWindowedFunction(MySqlParser.AggregateWindowedFunctionContext context)
        {
            if (_isOtherListener == 1)
            {
                AggregateWindowedFunction aggregateWindowedFunction =
                    new AggregateWindowedFunction(context.SourceInterval, context, context.GetText());
                if (context.ChildCount > 1)
                {
                    Rules.Remove(Rules[Rules.Count - 1]);
                }
                Rules.Add(aggregateWindowedFunction);
            }

            _isOtherListener++;
        }

        public override void ExitAggregateWindowedFunction(MySqlParser.AggregateWindowedFunctionContext context)
        {
            _isOtherListener --;
        }

        public override void EnterExtractFunctionCall(MySqlParser.ExtractFunctionCallContext context)
        {
            if (_isOtherListener == 1)
            {
                if (context.ChildCount > 1)
                {
                    Rules.Remove(Rules[Rules.Count - 1]);
                }

                ExtractFunctionCall extractFunctionCall =
                    new ExtractFunctionCall(context.SourceInterval, context, context.GetText());

                Rules.Add(extractFunctionCall);

            }
            _isOtherListener++;

        }

        public override void ExitExtractFunctionCall(MySqlParser.ExtractFunctionCallContext context)
        {
            _isOtherListener--;
        }
    }
}
