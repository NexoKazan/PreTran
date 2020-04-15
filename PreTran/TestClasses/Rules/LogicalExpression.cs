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
    class LogicalExpression : BaseRule
    {
        private LogicalExpressionListener _listener = new LogicalExpressionListener();
        
        private List<BaseRule> _rules = new List<BaseRule>();
        private string _text = "";

        public LogicalExpression(Interval ruleInterval, ParserRuleContext context, string text) : base(ruleInterval, context, text)
        {
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(_listener, context);
            _rules = _listener.Rules;
            Rules = _rules;
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
                        if (_rules.Count > 1)
                        {

                            foreach (var baseRule in _rules)
                            {
                                if (baseRule != _rules.Last())
                                {
                                    _text += baseRule.Text + " ";
                                }
                                else
                                {
                                    _text += baseRule.Text;
                                }
                            }
                        }
                        else
                        {
                            _text += _rules[0].Text;
                        }
                    }
                }
                
                // не забыть заменить на "" при оканчательном варианте
                if (_rules[0].Text == "\r\n" || _rules[0].Text == "")
                {
                    _text = _rules[2].Text;
                }

                if (_rules[2].Text == "\r\n" || _rules[2].Text == "")
                {
                    _text = _rules[0].Text;
                }

                if ((_rules[2].Text == "\r\n" || _rules[2].Text == "" ) && (_rules[0].Text == "\r\n" || _rules[0].Text == ""))
                {
                    _text = "";
                }
                return _text;
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
