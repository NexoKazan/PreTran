using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace PreTran.TestClasses.Rules
{
    class OrderByClause :BaseRule
    {
        public OrderByClause(Interval ruleInterval, ParserRuleContext context, string text) : base(ruleInterval, context, text)
        {
        }
    }
}
