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
    class OrderByClause : BaseRule
    {
        List<string> _columnNames = new List<string>();
        OrderByClauseListener _listener = new OrderByClauseListener();
        public OrderByClause(Interval ruleInterval, ParserRuleContext context, string text) : base(ruleInterval, context, text)
        {
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(_listener, context);
            Rules = _listener.Rules;
            foreach (var rule in Rules)
            {
                switch (rule.Text)
                {
                    case "ORDER" : rule.Text = Environment.NewLine + rule.Text; break;
                    case "BY" : rule.Text += Environment.NewLine; break; 
                    default: break;
                }
            }
        }

        public List<string> ColumnNames
        {

            get
            {
                List<BaseRule> columnRules = this.GetRulesByType("fullcolumnname");
                foreach (BaseRule rule in columnRules)
                {
                    _columnNames.Add(rule.Text);
                }
                return _columnNames;
            }
            set => _columnNames = value;
        }
    }
}
