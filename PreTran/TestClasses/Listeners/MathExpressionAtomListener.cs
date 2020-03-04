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
    class MathExpressionAtomListener : MySqlParserBaseListener
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

        public override void EnterNestedExpressionAtom(MySqlParser.NestedExpressionAtomContext context)
        {
            NestedExpressionAtom nestedExpressionAtom = new NestedExpressionAtom(context.SourceInterval, context,context.GetText());
            Rules.Remove(Rules[Rules.Count - 1]);
            Rules.Add(nestedExpressionAtom);
            _isOtherListener ++;
        }

        public override void ExitNestedExpressionAtom(MySqlParser.NestedExpressionAtomContext context)
        {
            _isOtherListener--;
        }
        
        public override void EnterMathExpressionAtom(MySqlParser.MathExpressionAtomContext context)
        {
            //if (_isOtherListener == 1 && _isFirst == false)
            //{
            //    MathExpressionAtom mathExpressionAtom = new MathExpressionAtom(context.SourceInterval, context, context.GetText());
            //    Rules.Remove(Rules[Rules.Count - 1]);
            //    Rules.Add(mathExpressionAtom);
            //    _isOtherListener++;
            //}
            if (_isOtherListener == 1 && Rules.Count > 0 && _isFirst)
            {
                Rules.Remove(Rules[Rules.Count - 1]);
                _isFirst = false;
            }

        }

        public override void EnterAggregateWindowedFunction(MySqlParser.AggregateWindowedFunctionContext context)
        {
            
            AggregateWindowedFunction aggregateWindowedFunction  = new AggregateWindowedFunction(context.SourceInterval, context, context.GetText());
            Rules.Remove(Rules[Rules.Count - 1]);
            Rules.Add(aggregateWindowedFunction);
            _isOtherListener ++;
        }

        public override void ExitAggregateWindowedFunction(MySqlParser.AggregateWindowedFunctionContext context)
        {
            _isOtherListener --;
        }

        //public override void ExitMathExpressionAtom(MySqlParser.MathExpressionAtomContext context)
        //{
        //    if (_isFirst == false)
        //    {
        //        _isOtherListener--;
        //    }
        //}
    }
}
