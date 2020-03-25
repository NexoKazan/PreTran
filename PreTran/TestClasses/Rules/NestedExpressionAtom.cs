using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PreTran.TestClasses.Listeners;

namespace PreTran.TestClasses.Rules
{
    class NestedExpressionAtom : BaseRule
    {
        NestedExpressionAtomListener _listener = new NestedExpressionAtomListener();

        public NestedExpressionAtom(Interval ruleInterval, ParserRuleContext context, string text) : base(ruleInterval, context, text)
        {
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(_listener, context);
            Rules = _listener.Rules;
            foreach (BaseRule rule in Rules)
            {
                rule.Text = rule.Text + "";
            }
        }
    }
}
