﻿using System;
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
    class BinaryComparasionPredicate : BaseRule
    {
        private BinaryComparasionPredicateListener _listener = new BinaryComparasionPredicateListener();
        private List<BaseRule> _rules = new List<BaseRule>();
        private string _text = "";

        public BinaryComparasionPredicate(Interval ruleInterval, ParserRuleContext context, string text) : base(ruleInterval, context, text)
        {
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(_listener, context);
            _rules = _listener.Rules;
            Rules = _rules;
            foreach (var rule in _rules)
            {
                rule.Text += "";
            }
        }

        public override string Text {
            get
            {
                if (_rules.Count > 0)
                {
                    _text = "";
                    foreach (var baseRule in _rules)
                    {
                        _text += baseRule.Text + " ";
                    }

                    return _text + Environment.NewLine;
                }
                else
                {
                    return _text + Environment.NewLine;
                }
            }
            set => _text = value; }
    }
}
