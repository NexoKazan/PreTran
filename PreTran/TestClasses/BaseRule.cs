using System;
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
        private string _divideSym;
        private string _ruleType;
        private bool _isRealised = false;
        private List<BaseRule> _rules = new List<BaseRule>();

        public BaseRule(Interval ruleInterval, ParserRuleContext context, string text)
        {
            _sourceInterval = ruleInterval;
            _context = context;
            _text = text;
            _divideSym = " ";
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
                    if (Rules.Count > 0)
                    {
                        _text = "";
                        if (Rules.Count > 1)
                        {
                            foreach (var baseRule in Rules)
                            {
                                if (baseRule != Rules.Last())
                                {
                                    _text += baseRule.Text + _divideSym;
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

        public Interval SourceInterval => _sourceInterval;

        public string RuleType
        {
            get => _ruleType;
        }

        public string DivideSym
        {
            get => _divideSym;
            set => _divideSym = value;
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
            foreach (var rule in Rules)
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

        public BaseRule GetRule(Interval sourceInterval, string ruleType)
        {
            BaseRule outRule = new BaseRule(sourceInterval, Context, "ERROR");
            if (_sourceInterval.a == sourceInterval.a && _sourceInterval.b == sourceInterval.b && _ruleType == ruleType)
            {
                outRule = this;
            }
            else
            {
                foreach (BaseRule rule in Rules)
                {
                    if (rule.SourceInterval.a <= sourceInterval.a && rule.SourceInterval.b >= sourceInterval.b)
                    {
                        outRule = rule.GetRule(sourceInterval, ruleType);
                    }
                }
            }

            return outRule;
        }

        public virtual bool CheckRealize()
        {
            bool output = true;
            if (Rules !=null && Rules.Count > 0)
            {
                foreach (BaseRule rule in Rules)
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
