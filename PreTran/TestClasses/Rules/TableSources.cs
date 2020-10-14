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
        
        private string _text;

        public TableSources(Interval ruleInterval, ParserRuleContext context, string text) : base(ruleInterval, context,
            text)
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
                            List<BaseRule> tmpList = new List<BaseRule>();
                            foreach (BaseRule rule in Rules)
                            {
                                if (rule.Text != "")
                                {
                                    tmpList.Add(rule);
                                }
                            }

                            Rules = tmpList;
                            if (Rules.Count > 1)
                            {
                                if (Rules[0].Text == ",")
                                {
                                    Rules[0].IsRealised = false;
                                    Rules[0].Text = "";
                                    Rules[0].IsRealised = true;
                                }

                                for (var i = 0; i < Rules.Count; i++)
                                {
                                    if (Rules[i].Text == ",")
                                    {
                                        if (Rules[i - 1].Text == Environment.NewLine && Rules[i - 1].Text != "" &&
                                            Rules[i - 1].Text != "\t" && Rules[i - 1].Text != " " &&
                                            Rules[i - 1].Text != "\r")
                                        {
                                            //костыль
                                        }
                                        else
                                        {
                                            Rules[i].IsRealised = false;
                                            Rules[i].Text = "";
                                            Rules[i].IsRealised = true;
                                        }

                                        Rules[i].Text += Environment.NewLine;
                                    }

                                    if (Rules[i] != Rules.Last())
                                    {
                                        _text += Rules[i].Text + ", ";
                                    }
                                    else
                                    {
                                        _text += Rules[i].Text;
                                    }
                                }
                            }
                        }
                        else
                        {
                            _text = Rules[0].Text;
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
                    if (Rules.Count == 1)
                    {
                        Rules[0].Text = value;
                        Rules[0].IsRealised = true;
                    }
                }
            }

        }
    }
}
