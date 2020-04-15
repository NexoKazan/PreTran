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
    class FromClause : BaseRule
    {
        private bool _isRealised = false;
        private string _text;
        private Interval _whereInterval;
        private Interval _groupInterval;
        private BaseRule _whereRule;
        private FromClauseListener _listener = new FromClauseListener();
        public FromClause(Interval ruleInterval, MySqlParser.FromClauseContext context, string text) : base(ruleInterval, context, text)
        {
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(_listener, context);
            _text = text;
            Rules = _listener.Rules;
            _whereRule = new BaseRule(new Interval(0, 0), context, "ERROR" );
            foreach (var rule in Rules)
            {
                switch (rule.Text)
                {
                    case "FROM" :
                        rule.Text = rule.Text + Environment.NewLine; break;
                    case "WHERE" : rule.Text = Environment.NewLine + rule.Text + Environment.NewLine;
                        _whereRule = rule;
                        _whereInterval = rule.SourceInterval;
                        break;
                    case "GROUP" : rule.Text = Environment.NewLine + rule.Text;
                        _groupInterval = rule.SourceInterval;
                        break;
                    case "BY" : rule.Text += Environment.NewLine; break; 
                    default: break;
                }
            }
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
                            bool whereStart = false;
                            List<BaseRule> rulesForCheck = new List<BaseRule>(); 
                            foreach (var baseRule in Rules)
                            {
                                if (whereStart && (baseRule.SourceInterval.a != _groupInterval.a && baseRule.SourceInterval.b != _groupInterval.b))
                                {
                                    rulesForCheck.Add(baseRule);
                                }

                                if (baseRule.SourceInterval.a == _whereInterval.a && baseRule.SourceInterval.b == _whereInterval.b)
                                {
                                    whereStart = true;
                                }
                                if ((baseRule.SourceInterval.a == _groupInterval.a && baseRule.SourceInterval.b == _groupInterval.b) || baseRule == Rules.Last())
                                {
                                    whereStart = false;
                                }
                            }
                            if (CheckRule(rulesForCheck))
                            {
                                _whereRule.Text = "";
                                _whereRule.IsRealised = true;
                            }

                            foreach (var baseRule in Rules)
                            {
                                if (baseRule != Rules.Last())
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

        private bool CheckRule(List<BaseRule> rulesForCheck)
        {
            bool result = true;
            foreach (BaseRule rule in rulesForCheck)
            {
                if (rule.Text != Environment.NewLine && rule.Text != "" && rule.Text != "\t" && rule.Text != " " && rule.Text != "\r")
                {
                    result = false;
                }
            }

            return result;
        }
    }
}
