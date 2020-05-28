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
    class AtomTableItem : BaseRule
    {
        private AtomTableItemListener _listener = new AtomTableItemListener();
        private string _text;
        public AtomTableItem(Interval ruleInterval, ParserRuleContext context, string text) : base(ruleInterval, context, text)
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
                if (!IsRealised)
                {
                    if (Rules.Count > 0)
                    {
                        _text = "";
                        if (Rules.Count > 1)
                        {
                            foreach (var baseRule in Rules)
                            {
                                if (baseRule != Rules.Last())
                                {
                                    _text += baseRule.Text + DivideSym;
                                }
                                else
                                {
                                    _text += baseRule.Text;
                                }
                            }
                        }
                        else
                        {
                            _text += Rules[0].Text;
                        }

                        return _text;
                    }
                    else
                    {
                        return _text;
                    }
                }
                else
                {
                    return _text;
                }
            }
            set
            {
                if (!IsRealised)
                {
                    _text = value;
                }
            }

        }
    }
}
