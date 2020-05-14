using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PreTran.DataBaseSchemeStructure;

namespace PreTran.SchemeCreator
{
    class SchemeAsStructure
    {
        private string _asRightColumnName;
        private List<string> _columnNames = new List<string>();

        //private DataBaseStructure _database;
        private ColumnStructure _asRightColumn;
        private ParserRuleContext _context;
        private SchemeAsListener _listener = new SchemeAsListener();

        private List<ColumnStructure> _columns;

        public SchemeAsStructure(ParserRuleContext context)
        {
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(_listener, context);
            _context = context;
            _columnNames = _listener.ColumnNames;
            _asRightColumnName = context.children.Last().GetText();
        }

        public ColumnStructure AsRightColumn
        {
            get => _asRightColumn;
            set => _asRightColumn = value;
        }

        public List<string> ColumnNames
        {
            get => _columnNames;
        }

        public void FillAsStructure(DataBaseStructure dataBase)
        {
            _columns = new List<ColumnStructure>();
            foreach (string columnName in _columnNames)
            {
                foreach (TableStructure table in dataBase.Tables)
                {
                    foreach (ColumnStructure column in table.Columns)
                    {
                        if (column.Name == columnName)
                        {
                            _columns.Add(column);
                        }
                    }
                }
            }

            if (_columns.Count > 0)
            {
                ColumnStructure biggestColumn = _columns[0];
                foreach (ColumnStructure column in _columns)
                {
                    if (column.Size > biggestColumn.Size)
                    {
                        biggestColumn = column;
                    }
                }

                _asRightColumn = new ColumnStructure(_asRightColumnName, biggestColumn.Type);
                _asRightColumn.Size = _asRightColumn.Type.Size;
            }
            else
            {
                _asRightColumn = new ColumnStructure(_asRightColumnName, FindeByName(dataBase, "INT"));
            }
        }

        private S_Type FindeByName(DataBaseStructure inDb, string typeName)
        {
            S_Type output = new S_Type();
            foreach (S_Type type in inDb.Types)
            {
                if (type.Name == typeName)
                {
                    output = type;
                    break;
                }
            }
            return output;
        }

        public void FillCrossAsStructutre(List<SchemeAsStructure> listenerAsStructures, DataBaseStructure dataBase)
        {
            foreach (SchemeAsStructure asStructure in listenerAsStructures)
            {
                asStructure.FillAsStructure(dataBase);
                foreach (string columnName in ColumnNames)
                {
                    if (columnName == asStructure.AsRightColumn.Name)
                    {
                        _columns.Add(asStructure.AsRightColumn);
                    }
                }
            }
            if (_columns.Count > 0)
            {
                ColumnStructure biggestColumn = _columns[0];
                foreach (ColumnStructure column in _columns)
                {
                    if (column.Size > biggestColumn.Size)
                    {
                        biggestColumn = column;
                    }
                }

                _asRightColumn = new ColumnStructure(_asRightColumnName, biggestColumn.Type);
                _asRightColumn.Size = _asRightColumn.Type.Size;
            }
            else
            {
                _asRightColumn = new ColumnStructure(_asRightColumnName, FindeByName(dataBase, "INT"));
            }
        }
    }
}
