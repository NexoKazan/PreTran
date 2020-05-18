using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace PreTran.SchemeCreator
{
    class SchemeAsListener : MySqlParserBaseListener
    {
        private bool _caseBlock = false;
        private List<string> _columnNames = new List<string>();
        
        public List<string> ColumnNames
        {
            get => _columnNames;
            set => _columnNames = value;
        }
        
        public override void VisitTerminal(ITerminalNode node)
        {
            if (_caseBlock)
            {
                if (node.GetText().ToLower() == "then")
                {
                    _caseBlock = false;
                }
            }
        }

        public override void EnterFullColumnName(MySqlParser.FullColumnNameContext context)
        {
            if (!_caseBlock)
            {
                if (context.ChildCount == 1)
                {
                    _columnNames.Add(context.GetText());
                }
                else
                {
                    string tmp = context.children[1].GetText();
                    tmp = tmp.Remove(0, 1);
                    _columnNames.Add(tmp);
                }
            }
        }

        public override void EnterCaseFuncAlternative(MySqlParser.CaseFuncAlternativeContext context)
        {
            _caseBlock = true;
        }

        public override void EnterCaseAlternative(MySqlParser.CaseAlternativeContext context)
        {

        }

        public override void EnterCaseStatement(MySqlParser.CaseStatementContext context)
        {
        }
    }
}
