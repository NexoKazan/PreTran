using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace PreTran.TestClasses.Listeners
{
    class TmpListenerFortTerminalNodes : MySqlParserBaseListener
    {
        public List<ITerminalNode> TerminalNodes = new List<ITerminalNode>();
        public override void VisitTerminal([NotNull] ITerminalNode node)
        {
            TerminalNodes.Add(node);
        }
    }
}
