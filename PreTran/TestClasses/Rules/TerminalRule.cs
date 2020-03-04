using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace PreTran.TestClasses.Rules
{
    class TerminalRule : BaseRule
    {
        private List<string> _branchRulesTypes = new List<string>();
        public TerminalRule(Interval ruleInterval, string text, IRuleNode parent) : base(ruleInterval, null, text)
        {
            IRuleNode tmpParent = parent;
            while (tmpParent.RuleContext.ChildCount < 2)
            {
                _branchRulesTypes.Add(tmpParent.RuleContext.GetType().ToString().ToLower().Replace("context", "").Replace("mysqlparser+", ""));
                tmpParent = tmpParent.Parent;
            }
        }

        public List<string> GetBranchRulesTypes
        {
            get {return _branchRulesTypes;}
        }

    }
}
