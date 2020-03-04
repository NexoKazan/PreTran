using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PreTran.TestClasses.Listeners;

namespace PreTran.TestClasses.Rules
{
    class SelectElements : BaseRule
    {
        private SelectElementsListener _listener = new SelectElementsListener();

        public SelectElements(Interval ruleInterval, MySqlParser.SelectElementsContext context, string text) : base(ruleInterval, context, text)
        {
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(_listener, context);
            Rules = _listener.Rules;
            foreach (var rule in Rules)
            {
                rule.Text += "";
            }
        }
        
        public override string Text {
            get
            {
                string text = "";
                if (this.Rules.Count > 0)
                {
                    
                    foreach (var baseRule in Rules)
                    {
                        if (baseRule.Text != ",")
                        {
                            text += Environment.NewLine + baseRule.Text;
                        }
                        else
                        {
                            text += baseRule.Text;
                        }
                    }

                    return text;
                }
                else
                {
                    return text;
                }
            }
            set => Text = value; }
    }
}
