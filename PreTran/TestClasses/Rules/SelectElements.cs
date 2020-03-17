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

        private string _text;
        public SelectElements(Interval ruleInterval, MySqlParser.SelectElementsContext context, string text) : base(ruleInterval, context, text)
        {
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(_listener, context);
            Rules = _listener.Rules;
            _text = text;
        }

        public override string Text
        {
            get
            {
                _text = "";
                if (this.Rules.Count > 0)
                {

                    foreach (var baseRule in Rules)
                    {
                        if (baseRule.Text != ",")
                        {
                            _text += Environment.NewLine + baseRule.Text;
                        }
                        else
                        {
                            _text += baseRule.Text;
                        }
                    }

                    return _text;
                }
                else
                {
                    return _text;
                }
            }
            set { _text = value; } 
        }
    }
}
