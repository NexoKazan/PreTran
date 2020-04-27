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
    class TableSources :BaseRule
    {
        private TableSourcesListener _listener = new TableSourcesListener();

        public TableSources(Interval ruleInterval, ParserRuleContext context, string text) : base(ruleInterval, context,
            text)
        {
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(_listener, context);
            Rules = _listener.Rules;
            for (var i = 0; i < Rules.Count; i++)
            {
                if (Rules[i].Text == ",")
                {
                    if (Rules[i-1].Text == Environment.NewLine && Rules[i-1].Text != "" && Rules[i-1].Text != "\t" && Rules[i-1].Text != " " && Rules[i-1].Text != "\r")
                    {//костыль
                    }
                    else
                    {
                        Rules[i].Text = "";
                        Rules[i].IsRealised = true;
                    }

                    Rules[i].Text += Environment.NewLine;
                }
            }
        }
    }
}
