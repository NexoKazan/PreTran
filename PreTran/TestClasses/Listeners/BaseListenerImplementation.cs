using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace PreTran.TestClasses.Listeners
{
    class BaseListenerImplementation : MySqlParserBaseListener
    {
        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            if (context.ChildCount > 1)
            {
            }
        }
    }
}
