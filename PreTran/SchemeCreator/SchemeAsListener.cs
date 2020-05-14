using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreTran.SchemeCreator
{
    class SchemeAsListener : MySqlParserBaseListener
    {
        private List<string> _columnNames = new List<string>();

        public List<string> ColumnNames
        {
            get => _columnNames;
            set => _columnNames = value;
        }


        public override void EnterFullColumnName(MySqlParser.FullColumnNameContext context)
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
}
