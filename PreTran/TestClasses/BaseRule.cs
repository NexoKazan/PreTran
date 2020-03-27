﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace PreTran
{
    class BaseRule
    {
        private Interval _sourceInterval;
        private ParserRuleContext _context;
        private string _text;
        private string _ruleType;
        private bool _isRealised = false;
        private List<BaseRule> _rules = new List<BaseRule>();

        public BaseRule(Interval ruleInterval, ParserRuleContext context, string text)
        {
            _sourceInterval = ruleInterval;
            _context = context;
            _text = text;
            if (context != null)
            {
                _ruleType = context.GetType().ToString().ToLower().Replace("context", "").Replace("mysqlparser+", "");
            }
            else
            {
                _ruleType = "terminal";
            }
        }


        #region Свойства

        public List<BaseRule> Rules
        {
            get { return _rules; }
            set { _rules = value; }
        }

        public ParserRuleContext Context
        {
            get => _context;
            set => _context = value;
        }

        public bool IsRealised
        {
            get
            {
                return _isRealised;
            }
            set
            {
                _isRealised = value;
            }
        }

        public virtual string Text {
            get
            {
                if (!_isRealised)
                {
                    if (_rules.Count > 0)
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

        public Interval SourceInterval => _sourceInterval;

        public string RuleType
        {
            get => _ruleType;
        }

        #endregion

        public List<BaseRule> GetRulesByType(string ruleType)
        {
            List<BaseRule> outList = new List<BaseRule>();
            if (_ruleType == ruleType)
            {
                outList.Add(this);
                return outList;
            }
            foreach (var rule in _rules)
            {
                if (rule.RuleType == ruleType)
                {
                    //могут ли правила содержать в себе правила такого же типа как они сами? Стоит ли их добавлять?
                    outList.Add(rule);
                }
                else
                {
                    outList.AddRange( rule.GetRulesByType(ruleType));
                }
            }

            return outList;
        }

        public BaseRule GetRuleBySourceInterval(Interval sourceInterval)
        {
            BaseRule outRule = new BaseRule(sourceInterval, Context, "ERROR");
            if (_sourceInterval.a == sourceInterval.a && _sourceInterval.b == sourceInterval.b)
            {
                outRule = this;
            }
            else
            {
                foreach (BaseRule rule in Rules)
                {
                    if (rule.SourceInterval.a <= sourceInterval.a && rule.SourceInterval.b >= sourceInterval.b)
                    {
                        outRule = rule.GetRuleBySourceInterval(sourceInterval);
                    }
                }
            }

            return outRule;
        }

        public virtual bool CheckRealize()
        {
            bool output = true;
            if (_rules !=null && _rules.Count > 0)
            {
                foreach (BaseRule rule in _rules)
                {
                    if (!rule.IsRealised)
                    {
                        output = false;
                        break;
                    }
                }
            }
            return output;
        }
    }
}
