using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Antlr4.Runtime.Misc;
using PreTran.DataBaseSchemeStructure;
using PreTran.TestClasses.Listeners;
using PreTran.TestClasses.Rules;

namespace PreTran.Services
{
    class AsTypeCalculator
    {
        private readonly string _mathExpression;
        private readonly DataBaseStructure _inDatabase;
        private readonly string _aggregationFunctionName;
        private readonly List<ColumnStructure> _columns;
        private S_Type _resultType;

        public AsTypeCalculator(string mathExpression, DataBaseStructure inDatabase, string aggregationFunctionName, List<ColumnStructure> columns)
        {
            _mathExpression = mathExpression;
            _inDatabase = inDatabase;
            _aggregationFunctionName = aggregationFunctionName;
            _columns = columns;
        }

        public S_Type CalculateType()
        {
            S_Type outputType = new S_Type();

            #region По умолчанию - INT

            if (FindeByName(_inDatabase, "INT") == null)
            {
                outputType = new S_Type("INT", 6, (_inDatabase.Types.Length + 1).ToString());
                List<S_Type> tmpTypes = new List<S_Type>();
                foreach (S_Type type in _inDatabase.Types)
                {
                    tmpTypes.Add(type);
                }

                tmpTypes.Add(new S_Type("INT", 6, (_inDatabase.Types.Length + 1).ToString()));
                _inDatabase.Types = tmpTypes.ToArray();
            }
            else
            {
                outputType = FindeByName(_inDatabase, "INT");
            }

            #endregion



            int resIntegerPart = -1; //целая часть + десятичная часть

            int resDecimalPart = -1; //десятичная часть

            if (_columns.Count > 1)
            {
                int multCount = _mathExpression.Count(c => c == '*');
                int sumCount = _mathExpression.Count(c => c == '+');
                int divideCount = _mathExpression.Count(c => c == '/');

               
                foreach (ColumnStructure asColumn in _columns)
                {
                    if (asColumn.Type.Name.Contains("DECIMAL"))
                    {
                        if (multCount > -1)
                        {
                            if (resIntegerPart == -1 || resDecimalPart == -1)
                            {
                                resIntegerPart = asColumn.Type.Param1;
                                resDecimalPart = asColumn.Type.Param2;
                                multCount--;
                            }
                            else
                            {
                                resIntegerPart = resIntegerPart + asColumn.Type.Param1;
                                resDecimalPart = resDecimalPart + asColumn.Type.Param2;
                                multCount--;
                            }
                        }
                    }
                }

                foreach (ColumnStructure asColumn in _columns)
                {
                    if (asColumn.Type.Name.Contains("DECIMAL"))
                    {
                        if (sumCount > 0)
                        {
                            if (resIntegerPart == -1 || resDecimalPart == -1)
                            {
                                resIntegerPart = asColumn.Type.Param1;
                                resDecimalPart = asColumn.Type.Param2;
                                sumCount--;
                            }
                            else
                            {
                                resIntegerPart = resIntegerPart + 1;
                                sumCount--;
                            }
                        }
                    }
                }

                foreach (ColumnStructure asColumn in _columns)
                {
                    if (divideCount > 0)
                    {
                        if (asColumn.Type.Name.Contains("DECIMAL"))
                        {

                            resDecimalPart = 8;
                            divideCount--;
                        }
                    }
                }

                if (resIntegerPart > 32)
                {
                    resIntegerPart = 32;
                }

                if (resDecimalPart > 8)
                {
                    resDecimalPart = 8;
                }

                if (resIntegerPart == -1 )
                {
                    //добавить логгер, может вызвать ошибочки
                    resIntegerPart = 16;
                }

                if (resDecimalPart == -1)
                {
                    //добавить логгер, может вызвать ошибочки
                    resDecimalPart = 0;
                }

                if (FindeByName(_inDatabase, "DECIMAL(" + resIntegerPart + "," + resDecimalPart + ")") == null)
                {
                    outputType = new S_Type("DECIMAL(" + resIntegerPart + ", " + resDecimalPart + ")", 77, (_inDatabase.Types.Length + 1).ToString());
                    //определить размер
                    List<S_Type> tmpTypes = new List<S_Type>();
                    foreach (S_Type type in _inDatabase.Types)
                    {
                        tmpTypes.Add(type);
                    }

                    tmpTypes.Add(outputType);
                    _inDatabase.Types = tmpTypes.ToArray();
                }
                else
                {
                    outputType = FindeByName(_inDatabase, "DECIMAL(" + resIntegerPart + "," + resDecimalPart + ")");
                }
            }
            else
            {
                if (_aggregationFunctionName.ToLower() == "avg")
                {
                    resDecimalPart = 8;
                    resIntegerPart = _columns[0].Type.Param1;
                    if (FindeByName(_inDatabase, "DECIMAL(" + resIntegerPart + "," + resDecimalPart + ")") == null)
                    {
                        outputType = new S_Type("DECIMAL(" + resIntegerPart + "," + resDecimalPart + ")", 77,
                            (_inDatabase.Types.Length + 1).ToString());
                        //определить размер
                        List<S_Type> tmpTypes = new List<S_Type>();
                        foreach (S_Type type in _inDatabase.Types)
                        {
                            tmpTypes.Add(type);
                        }

                        tmpTypes.Add(outputType);
                        _inDatabase.Types = tmpTypes.ToArray();
                    }
                    else
                    {
                        outputType = FindeByName(_inDatabase, "DECIMAL(" + resIntegerPart + "," + resDecimalPart + ")");
                    }
                }
                else
                {
                    if (_columns.Count == 1)
                    {
                        //outputType = _columns[0].Type;
                        outputType = FindeColumnByName(_columns[0].Name, _inDatabase).Type;
                    }
                }

            }

            if (outputType == null)
            {
                MessageBox.Show("AYAAYAYA");
            }
            return outputType;
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

        private ColumnStructure FindeColumnByName(string columnName, DataBaseStructure inDatabase)
        {
            ColumnStructure outColumn = new ColumnStructure("ERROR");

            foreach (TableStructure inDatabaseTable in inDatabase.Tables)
            {
                foreach (ColumnStructure column in inDatabaseTable.Columns)
                {
                    if (column.OldName == columnName )
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
    }
}
