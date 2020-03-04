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
        private bool _isOtherListener = false;

        public List<BaseRule> Rules = new List<BaseRule>();

        public override void VisitTerminal(ITerminalNode node)
        {
            if (!_isOtherListener)
            {
                TerminalRule terminal = new TerminalRule(node.SourceInterval, node.GetText(), node.Parent);
                Rules.Add(terminal);
            }
        }

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            if (context.ChildCount > 1)
            {
                if(!_isOtherListener)
                    Rules.Add(new BaseRule(context.SourceInterval, context, context.GetText()));
            }
        }

        public override void EnterSelectFunctionElement(MySqlParser.SelectFunctionElementContext context)
        {
            if (!_isOtherListener && Rules.Count>0)
            {
                Rules.Remove(Rules[Rules.Count - 1]);
            }
        }

        public override void EnterAggregateWindowedFunction(MySqlParser.AggregateWindowedFunctionContext context)
        {
            
            AggregateWindowedFunction aggregateWindowedFunction  = new AggregateWindowedFunction(context.SourceInterval, context, context.GetText());
            Rules.Remove(Rules[Rules.Count - 1]);
            Rules.Add(aggregateWindowedFunction);
            _isOtherListener = true;
        }

        public override void ExitAggregateWindowedFunction(MySqlParser.AggregateWindowedFunctionContext context)
        {
            _isOtherListener = false;
        }
    }
}
