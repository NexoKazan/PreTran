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
    class ExtractFunctionCall : BaseRule
    {
        private bool _isRealised = false;
        private string _text;

        ExtractFunctionCallListener _listener = new ExtractFunctionCallListener();
        public ExtractFunctionCall(Interval ruleInterval, ParserRuleContext context, string text) : base(ruleInterval, context, text)
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
                if (!_isRealised)
                {
                    if (Rules.Count > 0)
                    {
                        _text = "";
                        if (Rules.Count > 1)
                        {
                            foreach (var baseRule in Rules)
                            {
                                if (baseRule != Rules.Last() && baseRule.Text.ToLower() != "extract")
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
                if (!_isRealised)
                {
                    _text = value;
                }
            }

        }
    }
}
