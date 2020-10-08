using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using PreTran.DataBaseSchemeStructure;
using PreTran.Listeners;
using PreTran.SchemeCreator;

namespace PreTran.Q_Structures
{
    class NewSortStructure
    {
        private string _name;
        private string _createTableColumnNames;
        private string _output = "error";
        private List<string> _indexColumnName = new List<string>();
        private DataBaseStructure _inDataBase;
        private DataBaseStructure _outDataBase;
        private BaseRule _sortRule;
        private SortRuleListener _listener = new SortRuleListener();
        private List<SchemeAsStructure> _asStructures = new List<SchemeAsStructure>();

        public NewSortStructure(string name, BaseRule sortRule, DataBaseStructure fullDataBase)
        {
            _name = name;
            _sortRule = sortRule;
            _sortRule.Text += ";";
            _sortRule.IsRealised = true;
            _inDataBase = fullDataBase;
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(_listener, sortRule.Context);
            _asStructures = _listener.AsStructures;
            List<TableStructure> tmpTableList = new List<TableStructure>();
            List<ColumnStructure> tmpColumnList = new List<ColumnStructure>();
            List<ColumnStructure> orderedColumnList = new List<ColumnStructure>();
            foreach (TableStructure inTable in _inDataBase.Tables)
            {
               
                foreach (ColumnStructure inColumn in inTable.Columns)
                {
                    foreach (string columnName in _listener.SelectColumnNames)
                    {
                        if (columnName == inColumn.Name)
                        {
                            tmpColumnList.Add(inColumn);
                        }
                    }
                }

                //if (tmpColumnList.Count > 0)
                //{
                //    tmpTableList.Add(new TableStructure(inTable.Name, tmpColumnList.ToArray()));
                //}
            }

            foreach (string columnName in _listener.SelectAsRightColumns)
            {
                foreach (SchemeAsStructure asStructure in _asStructures)
                {
                    asStructure.FillAsStructure(_inDataBase);
                    asStructure.FillCrossAsStructutre(_asStructures, _inDataBase);
                    if (asStructure.AsRightColumn.Name == columnName)
                    {
                        tmpColumnList.Add(asStructure.AsRightColumn);
                    }
                }
            }

            foreach (string columnName in _listener.SelectColumnNames)
            {
                foreach (SchemeAsStructure asStructure in _asStructures)
                {
                    asStructure.FillAsStructure(_inDataBase);
                    asStructure.FillCrossAsStructutre(_asStructures, _inDataBase);
                    if (asStructure.AsRightColumn.Name == columnName)
                    {
                        tmpColumnList.Add(asStructure.AsRightColumn);
                    }
                }
            }

            foreach (string orderColumn in _listener.SelectColumnsOrded)
            {
                foreach (ColumnStructure column in tmpColumnList)
                {
                    if (orderColumn == column.Name)
                    {
                        orderedColumnList.Add(column);
                    }
                }
            }

            tmpColumnList = orderedColumnList;

            if (tmpColumnList.Count > 0)
            {
                tmpTableList.Add(new TableStructure("SORT_OUT_TABLE", tmpColumnList.ToArray()));
            }
            if (tmpTableList.Count > 0)
            {
                _outDataBase = new DataBaseStructure("SORT_OUT_DB", tmpTableList.ToArray(), _inDataBase.Types);
            }

            for (int i = 0; i < _outDataBase.Tables[0].Columns.Length; i++)
            {
                if (_outDataBase.Tables[0].Columns[i].Type != null)
                {
                    _createTableColumnNames += _outDataBase.Tables[0].Columns[i].Name + " " + _outDataBase.Tables[0].Columns[i].Type.Name;
                }
                else
                {
                    _createTableColumnNames += _outDataBase.Tables[0].Columns[i].Name + " " + "INTEGER";
                }

                if (i < _outDataBase.Tables[0].Columns.Length - 1)
                {
                    _createTableColumnNames += ",\r\n";
                }
            }

            _sortRule.IsRealised = false;
            _output = _sortRule.Text + ";";
            //SetIndexes();
        }

        public NewSortStructure(string name, BaseRule sortRule, DataBaseStructure fullDataBase, string tag)
        {
            _name = name;
            _sortRule = sortRule;
            _sortRule.Text += ";";
            _sortRule.IsRealised = true;
            _inDataBase = fullDataBase;
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(_listener, sortRule.Context);
            _asStructures = _listener.AsStructures;
            List<TableStructure> tmpTableList = new List<TableStructure>();
            List<ColumnStructure> tmpColumnList = new List<ColumnStructure>();
            List<ColumnStructure> orderedColumnList = new List<ColumnStructure>();
            foreach (TableStructure inTable in _inDataBase.Tables)
            {

                foreach (ColumnStructure inColumn in inTable.Columns)
                {
                    foreach (string columnName in _listener.SelectColumnNames)
                    {
                        if (columnName == inColumn.Name)
                        {
                            tmpColumnList.Add(inColumn);
                        }
                    }
                }
                List<BaseRule> columns = _sortRule.GetRulesByType("fullcolumnname");
                foreach (BaseRule rule in columns)
                {
                    if (rule.Rules.Count > 1)
                    {
                        string tmpName = rule.Text.Replace(".", "");
                        rule.IsRealised = false;
                        rule.Text = tmpName;
                        rule.IsRealised = true;
                        Console.WriteLine(tmpName);
                    }
                }
            }

            foreach (string columnName in _listener.SelectAsRightColumns)
            {
                foreach (SchemeAsStructure asStructure in _asStructures)
                {
                    asStructure.FillAsStructure(_inDataBase);
                    asStructure.FillCrossAsStructutre(_asStructures, _inDataBase);
                    if (asStructure.AsRightColumn.Name == columnName)
                    {
                        tmpColumnList.Add(asStructure.AsRightColumn);
                    }
                }
            }

            foreach (string columnName in _listener.SelectColumnNames)
            {
                foreach (SchemeAsStructure asStructure in _asStructures)
                {
                    asStructure.FillAsStructure(_inDataBase);
                    asStructure.FillCrossAsStructutre(_asStructures, _inDataBase);
                    if (asStructure.AsRightColumn.Name == columnName)
                    {
                        tmpColumnList.Add(asStructure.AsRightColumn);
                    }
                }
            }

            foreach (string orderColumn in _listener.SelectColumnsOrded)
            {
                foreach (ColumnStructure column in tmpColumnList)
                {
                    if (orderColumn == column.Name)
                    {
                        orderedColumnList.Add(column);
                    }
                }
            }

            tmpColumnList = orderedColumnList;

            if (tmpColumnList.Count > 0)
            {
                tmpTableList.Add(new TableStructure("SORT_OUT_TABLE", tmpColumnList.ToArray()));
            }
            if (tmpTableList.Count > 0)
            {
                _outDataBase = new DataBaseStructure("SORT_OUT_DB", tmpTableList.ToArray(), _inDataBase.Types);
            }

            for (int i = 0; i < _outDataBase.Tables[0].Columns.Length; i++)
            {
                if (_outDataBase.Tables[0].Columns[i].Type != null)
                {
                    _createTableColumnNames += _outDataBase.Tables[0].Columns[i].Name + " " + _outDataBase.Tables[0].Columns[i].Type.Name;
                }
                else
                {
                    _createTableColumnNames += _outDataBase.Tables[0].Columns[i].Name + " " + "INTEGER";
                }

                if (i < _outDataBase.Tables[0].Columns.Length - 1)
                {
                    _createTableColumnNames += ",\r\n";
                }
            }

            List<BaseRule> fromList = _sortRule.GetRulesByType("atomtableitem");
            foreach (BaseRule rule in fromList)
            {
                if (rule.Text != "")
                {
                    rule.IsRealised = false;
                    rule.Text = tag;
                    rule.IsRealised = true;
                }
            }

            _sortRule.IsRealised = false;
            _output = _sortRule.Text + ";";
            //SetIndexes();
        }

        public void SetIndexes()
        {
            foreach (TableStructure table in _outDataBase.Tables)
            {
                foreach (ColumnStructure column in table.Columns)
                {
                    if (column.IsPrimary > 0)
                    {
                        _indexColumnName.Add(column.Name);
                    }
                }
            }
        }

        public DataBaseStructure OutDataBase
        {
            get => _outDataBase;
        }

        public string Output
        {
            get { return _output; }
        }

        public string CreateTableColumnNames => _createTableColumnNames;

        public string Name => _name;

        public List<string> IndexColumnName
        {
            get => _indexColumnName;
            set => _indexColumnName = value;
        }
    }
}
