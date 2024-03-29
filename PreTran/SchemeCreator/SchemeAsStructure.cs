﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PreTran.DataBaseSchemeStructure;
using PreTran.Services;

namespace PreTran.SchemeCreator
{
    class SchemeAsStructure
    {
        private string _asRightColumnName;
        private string _aggregationFunctionName;
        private bool _isExtract = false;
        private List<string> _columnNames = new List<string>();

        //private DataBaseStructure _database;
        private ColumnStructure _asRightColumn;
        private ParserRuleContext _context;
        private SchemeAsListener _listener = new SchemeAsListener();

        private List<ColumnStructure> _columns;

        public SchemeAsStructure(ParserRuleContext context)
        {
            _aggregationFunctionName = context.Start.Text;
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(_listener, context);
            _context = context;
            _columnNames = _listener.ColumnNames;
            _asRightColumnName = context.children.Last().GetText();
            if (context.Start.Text.ToLower() == "extract")
            {
                _isExtract = true;
            }
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
                        //biggestColumn = column;
                    }
                }
                AsTypeCalculator asCalc = new AsTypeCalculator(_context.GetText(), dataBase, _aggregationFunctionName, _columns);
                _asRightColumn = new ColumnStructure(_asRightColumnName, asCalc.CalculateType());
                //Console.WriteLine(asCalc.CalculateType().Name);
                _asRightColumn.Size = _asRightColumn.Type.Size;
            }
            else
            {
                _asRightColumn = new ColumnStructure(_asRightColumnName, FindeByName(dataBase, "INT"));
            }

            if (_isExtract)
            {
                if (FindeByName(dataBase, "INT") == null)
                {
                    _asRightColumn.Type = new S_Type("INT", 6, (dataBase.Types.Length + 1).ToString());
                    List<S_Type> tmpTypes = new List<S_Type>();
                    foreach (S_Type type in dataBase.Types)
                    {
                        tmpTypes.Add(type);
                    }

                    tmpTypes.Add(new S_Type("INT", 6, (dataBase.Types.Length + 1).ToString()));
                    dataBase.Types = tmpTypes.ToArray();
                }
                else
                {
                    _asRightColumn.Type = FindeByName(dataBase, "INT");
                }
            }

            if (FindeColumnByName(_asRightColumnName, dataBase).Name == "ERROR")
            {
                if (FindeTableByName("ASTABLE", dataBase).Name == null)
                {
                    TableStructure asTable = new TableStructure();
                    asTable.Name = "ASTABLE";
                    List<ColumnStructure> columns = new List<ColumnStructure>();
                    columns.Add(_asRightColumn);
                    asTable.Columns = columns.ToArray();

                    List<TableStructure> tmpTables = dataBase.Tables.ToList();
                    tmpTables.Add(asTable);
                    dataBase.Tables = tmpTables.ToArray();

                }
                else
                {
                    TableStructure asTable = FindeTableByName("ASTABLE", dataBase);
                    List<ColumnStructure> columns = asTable.Columns.ToList();
                    columns.Add(_asRightColumn);
                    asTable.Columns = columns.ToArray();
                }
                
            }
        }

       

        public void FillCrossAsStructutre(List<SchemeAsStructure> listenerAsStructures, DataBaseStructure dataBase)
        {
            List<SchemeAsStructure> reversedAsStructures = listenerAsStructures;

            for (int i = listenerAsStructures.Count - 1; i >= 0; i--)
            {
               //reversedAsStructures.Add(listenerAsStructures[i]);
            }
            //reversedAsStructures.Reverse();
            

            foreach (SchemeAsStructure asStructure in reversedAsStructures)
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
                        //biggestColumn = column;
                    }
                }

                //_asRightColumn = new ColumnStructure(_asRightColumnName, biggestColumn.Type);
                //_asRightColumn.Size = _asRightColumn.Type.Size;
                AsTypeCalculator asCalc = new AsTypeCalculator(_context.GetText(), dataBase, _aggregationFunctionName, _columns);
                _asRightColumn = new ColumnStructure(_asRightColumnName, asCalc.CalculateType());
                //Console.WriteLine(asCalc.CalculateType().Name);
                _asRightColumn.Size = _asRightColumn.Type.Size;
            }
            else
            {
                _asRightColumn = new ColumnStructure(_asRightColumnName, FindeByName(dataBase, "INT"));
            }
            if (_isExtract)
            {
                if (FindeByName(dataBase, "INT") == null)
                {
                    _asRightColumn.Type = new S_Type("INT", 6, (dataBase.Types.Length + 1).ToString());
                    List<S_Type> tmpTypes = new List<S_Type>();
                    foreach (S_Type type in dataBase.Types)
                    {
                        tmpTypes.Add(type);
                    }

                    tmpTypes.Add(new S_Type("INT", 6, (dataBase.Types.Length + 1).ToString()));
                    dataBase.Types = tmpTypes.ToArray();
                }
                else
                {
                    _asRightColumn.Type = FindeByName(dataBase, "INT");
                }
            }

            if (FindeColumnByName(_asRightColumnName, dataBase).Name == "ERROR")
            {
                if (FindeTableByName("ASTABLE", dataBase).Name == null)
                {
                    TableStructure asTable = new TableStructure();
                    asTable.Name = "ASTABLE";
                    List<ColumnStructure> columns = new List<ColumnStructure>();
                    columns.Add(_asRightColumn);
                    asTable.Columns = columns.ToArray();

                    List<TableStructure> tmpTables = dataBase.Tables.ToList();
                    tmpTables.Add(asTable);
                    dataBase.Tables = tmpTables.ToArray();
                }
                else
                {
                    TableStructure asTable = FindeTableByName("ASTABLE", dataBase);
                    List<ColumnStructure> columns = asTable.Columns.ToList();
                    columns.Add(_asRightColumn);
                    asTable.Columns = columns.ToArray();
                }

            }
        }

        private ColumnStructure FindeColumnByName(string columnName, DataBaseStructure inDatabase)
        {
            ColumnStructure outColumn = new ColumnStructure("ERROR");

            foreach (TableStructure inDatabaseTable in inDatabase.Tables)
            {
                foreach (ColumnStructure column in inDatabaseTable.Columns)
                {
                    if (column.OldName == columnName)
                    {
                        outColumn = column;
                    }
                    else
                    {
                        if (column.Name == columnName)
                        {
                            outColumn = column;
                        }
                    }
                }
            }

            if (outColumn.Name == "ERROR")
            {
               // MessageBox.Show(this.GetType().Name + "FindeColumnByName");
            }

            return outColumn;
        }

        private TableStructure FindeTableByName(string tableName, DataBaseStructure inDatabase)
        {
            TableStructure table = new TableStructure();
            table.Name = null;

            foreach (TableStructure inDatabaseTable in inDatabase.Tables)
            {
                if (inDatabaseTable.Name == tableName)
                {
                    table = inDatabaseTable;
                }
            }
            return table;
        }


        private S_Type FindeByName(DataBaseStructure inDb, string typeName)
        {
            S_Type output = null;
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
    }
}
