using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.PerformanceData;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using Antlr4;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Drawing.Imaging;
using System.Globalization;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Schema;
using Antlr4.Runtime.Misc;
using ClusterixN.Common.Data;
using ClusterixN.Common.Data.Query;
using ClusterixN.Common.Data.Query.Relation;
using ClusterixN.Common.Utils;
using ClusterixN.Network.Packets;
using MySQL_Clear_standart.Properties;
using PreTran;
using PreTran.DataBaseSchemeStructure;
using PreTran.Listeners;
using PreTran.Network;
using PreTran.Q_Part_Structures;
using PreTran.Q_Structures;
using PreTran.SchemeCreator;
using PreTran.TestClasses;
using PreTran.TestClasses.Listeners;
using PreTran.TestClasses.Rules;
using PreTran.Visual;


namespace MySQL_Clear_standart
{
    public partial class Form1 : Form
    {
        
        #region Чек сериализации

        protected void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            MessageBox.Show("Unknown Node:" + e.Name + "\t" + e.Text);
        }

        protected void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            MessageBox.Show("Unknown attribute " + attr.Name + "='" + attr.Value + "'");
        }

        #endregion

        #region Глобальные переменные

        private DataBaseStructure _dbName;
        //private DataBaseStructure _queryDB;
        // объявляются переменный для построения дерева
        private string _output = " ";
        private string _inputString;
        private ICharStream _inputStream;
        private ITokenSource _mySqlLexer;
        private CommonTokenStream _commonTokenStream;
        private MySqlParser _mySqlParser;
        private IParseTree _tree;
        private CommonNode _treeNodeDrawable;
        private TreeVisitor _vTree;
        private ParseTreeWalker _walker;
        private MainListener _listener;

        private BaseRule _sortRule;
       
       
        private SelectStructure[] _selectQuery;
        private List<JoinStructure> _joinQuery;
        private SortStructure _sortQuery;

        private SelectStructure[] _subSelectQuery;
        private List<JoinStructure> _subJoinQuery;
        private SortStructure _subSortQuery;
        
        private bool _toDoTextFlag;
        bool pictureSize = false;
        private string _connectionIP;
        

        #endregion

        #region СлужебныеМетоды

        #region Общие методы

        private void GetTree()
        {
            string oldInput = _inputString;
            FillScheme(); //странный баг.
            textBox_tab2_Query.Text = textBox_tab1_Query.Text;
            _inputString = textBox_tab1_Query.Text;
            _inputStream = new AntlrInputStream(_inputString);
            _mySqlLexer = new MySqlLexer(_inputStream);
            _commonTokenStream = new CommonTokenStream(_mySqlLexer);
            _mySqlParser = new MySqlParser(_commonTokenStream);
            _mySqlParser.BuildParseTree = true;
            _tree = _mySqlParser.root();
            _treeNodeDrawable = new CommonNode(_tree);
            _vTree = new TreeVisitor(_treeNodeDrawable);
            _walker = new ParseTreeWalker();
            _listener = new MainListener(0); 
            _listener.Vocabulary = _mySqlParser.Vocabulary;
            _walker.Walk(_listener, _tree);
            if (_sortRule == null || oldInput != _inputString)
            {
                _sortRule = GetMainRule(_inputString);
            }
            //_sortRule = new BaseRule(new Interval(1, 1), new ParserRuleContext(), "ERROR");
            //_sortRule = GetMainRule(_inputString);
            //_queryDB = CreateSubDatabase(_dbName, _listener.TableNames.ToArray(), _listener.ColumnNames.ToArray(), _listener.RemoveCounterColumsNames.ToArray());
            
        }

        private void GetTree(string queryText)
        {
            string oldInput = _inputString;
            FillScheme(); //странный баг.
            _inputString = queryText;
            _inputStream = new AntlrInputStream(_inputString);
            _mySqlLexer = new MySqlLexer(_inputStream);
            _commonTokenStream = new CommonTokenStream(_mySqlLexer);
            _mySqlParser = new MySqlParser(_commonTokenStream);
            _mySqlParser.BuildParseTree = true;
            _tree = _mySqlParser.root();
            _treeNodeDrawable = new CommonNode(_tree);
            _vTree = new TreeVisitor(_treeNodeDrawable);
            _walker = new ParseTreeWalker();
            _listener = new MainListener(0);
            _listener.Vocabulary = _mySqlParser.Vocabulary;
            _walker.Walk(_listener, _tree);
            if (_sortRule == null || oldInput != _inputString)
            {
                _sortRule = GetMainRule(_inputString);
            }
        }

        private void GetTree(string queryText, MySqlParserBaseListener listener)
        {
            string oldInput = _inputString;
            FillScheme(); //странный баг.
            _inputString = queryText;
            _inputStream = new AntlrInputStream(_inputString);
            _mySqlLexer = new MySqlLexer(_inputStream);
            _commonTokenStream = new CommonTokenStream(_mySqlLexer);
            _mySqlParser = new MySqlParser(_commonTokenStream);
            _mySqlParser.BuildParseTree = true;
            _tree = _mySqlParser.root();
            _treeNodeDrawable = new CommonNode(_tree);
            _vTree = new TreeVisitor(_treeNodeDrawable);
            _walker = new ParseTreeWalker();
            _walker.Walk(listener, _tree);
            if (_sortRule == null || oldInput != _inputString)
            {
                _sortRule = GetMainRule(_inputString);
            }

            //_queryDB = CreateSubDatabase(_dbName, _listener.TableNames.ToArray(), _listener.ColumnNames.ToArray(), _listener.RemoveCounterColumsNames.ToArray());

        }

        private BaseRule GetMainRule(string inputQuery)
        {
            AntlrInputStream inputStream = new AntlrInputStream(inputQuery);
            MySqlLexer mySqlLexer = new MySqlLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(mySqlLexer);
            MySqlParser mySqlParser = new MySqlParser(commonTokenStream);
            mySqlParser.BuildParseTree = true;
            IParseTree tree = mySqlParser.sqlStatements();
            ParseTreeWalker walker = new ParseTreeWalker();
            SqlStatsmentsListener qListener = new SqlStatsmentsListener();
            walker.Walk(qListener, tree);
            QuerySpecification mainRule = qListener.queries[0];
            
            return mainRule;
        }

        private void FillScheme()
        {
            using (FileStream dbCreateFileStream = new FileStream("res//db.xml", FileMode.Open, FileAccess.ReadWrite)
            ) // Заполнение БД из XML
            {
                XmlSerializer dbCreateSerializer = new XmlSerializer(typeof(DataBaseStructure));
                _dbName = (DataBaseStructure) dbCreateSerializer.Deserialize(dbCreateFileStream);
            }

            SetID(_dbName);
            SetTables(_dbName);
        }
        
        private void GetQuerryTreesScreens(string path, int start, int end)
        {
            for (int i = start; i < end + 1; i++)
            {
                string pt = path + i.ToString() + ".bmp";
                comboBox_tab1_QueryNumber.Text = i.ToString();
                btn_tab1_SelectQuerry.PerformClick();
                btn_tab1_CreateTree.PerformClick();
                pictureBox_tab1_Tree.Image.Save(pt, ImageFormat.Bmp);
            }
        }
        
        //Сопоставление типов столбцов с типами из базы данных.
        private void SetID(DataBaseStructure inDb)
        {
            foreach (TableStructure dbTable in inDb.Tables)
            {
                foreach (ColumnStructure dbColumn in dbTable.Columns)
                {
                    foreach (S_Type dbType in inDb.Types)
                    {
                        if (dbColumn.TypeID == dbType.ID)
                        {
                            dbColumn.Type = dbType;
                            dbColumn.Size = dbType.Size;
                            break;
                        }
                    }
                }
            }
        }

        //обратная связь столбцов и колонок.
        private void SetTables(DataBaseStructure database)
        {
            foreach (TableStructure table in database.Tables)
            {
                foreach (ColumnStructure column in table.Columns)
                {
                    column.Table = table;
                }
            }
        }

        //Присвоение типов полученной схемы после запроса(саб т) известным столбцам.
        private void MatchColumns(DataBaseStructure mainDataBase, DataBaseStructure subDataBase)
        {
            foreach (TableStructure subTable in subDataBase.Tables)
            {
                foreach (ColumnStructure subColumn in subTable.Columns)
                {
                    foreach (TableStructure mainTable in mainDataBase.Tables)
                    {
                        foreach (ColumnStructure mainColumn in mainTable.Columns)
                        {
                            if (subColumn.Name == mainColumn.Name)
                            {
                                subColumn.Type = mainColumn.Type;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private string GetQuery(int queryNumber, bool state, ComboBox box)
        {
            string query;
            if (!state)
            {
                switch (queryNumber)
                {

                    case 1:
                        query =
                            "SELECT\r\n\tL_RETURNFLAG,\r\n\tL_LINESTATUS,\r\n\tSUM(L_QUANTITY) AS SUM_QTY,\r\n\tSUM(L_EXTENDEDPRICE) AS SUM_BASE_PRICE,\r\n\tSUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS SUM_DISC_PRICE,\r\n\tSUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT) * (1 + L_TAX)) AS SUM_CHARGE,\r\n\tAVG(L_QUANTITY) AS AVG_QTY,\r\n\tAVG(L_EXTENDEDPRICE) AS AVG_PRICE,\r\n\tAVG(L_DISCOUNT) AS AVG_DISC,\r\n\tCOUNT(*) AS COUNT_ORDER\r\nFROM\r\n\tLINEITEM\r\nWHERE\r\n\tL_SHIPDATE <='1998-12-01' - INTERVAL '90' DAY\r\nGROUP BY\r\n\tL_RETURNFLAG,\r\n\tL_LINESTATUS\r\nORDER BY\r\n\tL_RETURNFLAG,\r\n\tL_LINESTATUS;\r\n";
                        break;
                    case 2:
                        query =
                            "SELECT\r\n\tS_ACCTBAL,\r\n\tS_NAME,\r\n\tN_NAME,\r\n\tP_PARTKEY,\r\n\tP_MFGR,\r\n\tS_ADDRESS,\r\n\tS_PHONE,\r\n\tS_COMMENT\r\nFROM\r\n\tPART,\r\n\tSUPPLIER,\r\n\tPARTSUPP,\r\n\tNATION,\r\n\tREGION\r\nWHERE\r\n\tP_PARTKEY = PS_PARTKEY\r\n\tAND S_SUPPKEY = PS_SUPPKEY\r\n\tAND P_SIZE = 48\r\n\tAND P_TYPE LIKE '%NICKEL'\r\n\tAND S_NATIONKEY = N_NATIONKEY\r\n\tAND N_REGIONKEY = R_REGIONKEY\r\n\tAND R_NAME = 'AMERICA'\r\n\tAND PS_SUPPLYCOST = (\r\n\t\tSELECT\r\n\t\t\tMIN(PS_SUPPLYCOST)\r\n\t\tFROM\r\n\t\t\tPARTSUPP,\r\n\t\t\tSUPPLIER,\r\n\t\t\tNATION,\r\n\t\t\tREGION\r\n\t\tWHERE\r\n\t\t\tP_PARTKEY = PS_PARTKEY\r\n\t\t\tAND S_SUPPKEY = PS_SUPPKEY\r\n\t\t\tAND S_NATIONKEY = N_NATIONKEY\r\n\t\t\tAND N_REGIONKEY = R_REGIONKEY\r\n\t\t\tAND R_NAME = 'AMERICA'\r\n\t)\r\nORDER BY\r\n\tS_ACCTBAL DESC,\r\n\tN_NAME,\r\n\tS_NAME,\r\n\tP_PARTKEY;\r\n";
                        break;
                    case 3:
                        query =
                            "SELECT\r\n\tL_ORDERKEY,\r\n\tSUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS REVENUE,\r\n\tO_ORDERDATE,\r\n\tO_SHIPPRIORITY\r\nFROM\r\n\tCUSTOMER,\r\n\tORDERS,\r\n\tLINEITEM\r\nWHERE\r\n\tC_MKTSEGMENT = 'HOUSEHOLD'\r\n\tAND C_CUSTKEY = O_CUSTKEY\r\n\tAND L_ORDERKEY = O_ORDERKEY\r\n\tAND O_ORDERDATE < '1995-03-31'\r\n\tAND L_SHIPDATE > '1995-03-31'\r\nGROUP BY\r\n\tL_ORDERKEY,\r\n\tO_ORDERDATE,\r\n\tO_SHIPPRIORITY\r\nORDER BY\r\n\tREVENUE DESC,\r\n\tO_ORDERDATE;\r\n";
                        break;
                    case 4:
                        query =
                            "SELECT\r\n\tO_ORDERPRIORITY,\r\n\tCOUNT(*) AS ORDER_COUNT\r\nFROM\r\n\tORDERS\r\nWHERE\r\n\tO_ORDERDATE >= '1996-02-01'\r\n\tAND O_ORDERDATE < '1996-02-01' + INTERVAL '3' MONTH\r\n\tAND EXISTS (\r\n\t\tSELECT\r\n\t\t\t*\r\n\t\tFROM\r\n\t\t\tLINEITEM\r\n\t\tWHERE\r\n\t\t\tL_ORDERKEY = O_ORDERKEY\r\n\t\t\tAND L_COMMITDATE < L_RECEIPTDATE\r\n\t)\r\nGROUP BY\r\n\tO_ORDERPRIORITY\r\nORDER BY\r\n\tO_ORDERPRIORITY;\r\n";
                        break;
                    case 5:
                        query =
                            "SELECT\r\n\tN_NAME,\r\n\tSUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS REVENUE\r\nFROM\r\n\tCUSTOMER,\r\n\tORDERS,\r\n\tLINEITEM,\r\n\tSUPPLIER,\r\n\tNATION,\r\n\tREGION\r\nWHERE\r\n\tC_CUSTKEY = O_CUSTKEY\r\n\tAND L_ORDERKEY = O_ORDERKEY\r\n\tAND L_SUPPKEY = S_SUPPKEY\r\n\tAND C_NATIONKEY = S_NATIONKEY\r\n\tAND S_NATIONKEY = N_NATIONKEY\r\n\tAND N_REGIONKEY = R_REGIONKEY\r\n\tAND R_NAME = 'MIDDLE EAST'\r\n\tAND O_ORDERDATE >= '1995-01-01'\r\n\tAND O_ORDERDATE < '1995-01-01' + INTERVAL '1' YEAR\r\nGROUP BY\r\n\tN_NAME\r\nORDER BY\r\n\tREVENUE DESC;\r\n";
                        break;
                    case 6:
                        query =
                            "SELECT\r\n\tSUM(L_EXTENDEDPRICE * L_DISCOUNT) AS REVENUE\r\nFROM\r\n\tLINEITEM\r\nWHERE\r\n\tL_SHIPDATE >= '1997-01-01'\r\n\tAND L_SHIPDATE < '1997-01-01' + INTERVAL '1' YEAR\r\n\tAND L_DISCOUNT BETWEEN 0.07 - 0.01 AND 0.07 + 0.01\r\n\tAND L_QUANTITY < 24;\r\n";
                        break;
                    case 7:
                        query =
                            "SELECT\r\n\tSUPP_NATION,\r\n\tCUST_NATION,\r\n\tL_YEAR,\r\n\tSUM(VOLUME) AS REVENUE\r\nFROM\r\n\t(\r\n\t\tSELECT\r\n\t\t\tN1.N_NAME AS SUPP_NATION,\r\n\t\t\tN2.N_NAME AS CUST_NATION,\r\n\t\t\tEXTRACT(YEAR FROM L_SHIPDATE) AS L_YEAR,\r\n\t\t\tL_EXTENDEDPRICE * (1 - L_DISCOUNT) AS VOLUME\r\n\t\tFROM\r\n\t\t\tSUPPLIER,\r\n\t\t\tLINEITEM,\r\n\t\t\tORDERS,\r\n\t\t\tCUSTOMER,\r\n\t\t\tNATION N1,\r\n\t\t\tNATION N2\r\n\t\tWHERE\r\n\t\t\tS_SUPPKEY = L_SUPPKEY\r\n\t\t\tAND O_ORDERKEY = L_ORDERKEY\r\n\t\t\tAND C_CUSTKEY = O_CUSTKEY\r\n\t\t\tAND S_NATIONKEY = N1.N_NATIONKEY\r\n\t\t\tAND C_NATIONKEY = N2.N_NATIONKEY\r\n\t\t\tAND (\r\n\t\t\t\t(N1.N_NAME = 'IRAQ' AND N2.N_NAME = 'ALGERIA')\r\n\t\t\t\tOR (N1.N_NAME = 'ALGERIA' AND N2.N_NAME = 'IRAQ')\r\n\t\t\t)\r\n\t\t\tAND L_SHIPDATE BETWEEN '1995-01-01' AND '1996-12-31'\r\n\t) AS SHIPPING\r\nGROUP BY\r\n\tSUPP_NATION,\r\n\tCUST_NATION,\r\n\tL_YEAR\r\nORDER BY\r\n\tSUPP_NATION,\r\n\tCUST_NATION,\r\n\tL_YEAR;\r\n";
                        break;
                    case 8:
                        query =
                            "SELECT\r\n\tO_YEAR,\r\n\tSUM(CASE\r\n\t\tWHEN NATION = 'IRAN' THEN VOLUME\r\n\t\tELSE 0\r\n\tEND) / SUM(VOLUME) AS MKT_SHARE\r\nFROM\r\n\t(\r\n\t\tSELECT\r\n\t\t\tEXTRACT(YEAR FROM O_ORDERDATE) AS O_YEAR,\r\n\t\t\tL_EXTENDEDPRICE * (1 - L_DISCOUNT) AS VOLUME,\r\n\t\t\tN2.N_NAME AS NATION\r\n\t\tFROM\r\n\t\t\tPART,\r\n\t\t\tSUPPLIER,\r\n\t\t\tLINEITEM,\r\n\t\t\tORDERS,\r\n\t\t\tCUSTOMER,\r\n\t\t\tNATION N1,\r\n\t\t\tNATION N2,\r\n\t\t\tREGION\r\n\t\tWHERE\r\n\t\t\tP_PARTKEY = L_PARTKEY\r\n\t\t\tAND S_SUPPKEY = L_SUPPKEY\r\n\t\t\tAND L_ORDERKEY = O_ORDERKEY\r\n\t\t\tAND O_CUSTKEY = C_CUSTKEY\r\n\t\t\tAND C_NATIONKEY = N1.N_NATIONKEY\r\n\t\t\tAND N1.N_REGIONKEY = R_REGIONKEY\r\n\t\t\tAND R_NAME = 'MIDDLE EAST'\r\n\t\t\tAND S_NATIONKEY = N2.N_NATIONKEY\r\n\t\t\tAND O_ORDERDATE BETWEEN '1995-01-01' AND '1996-12-31'\r\n\t\t\tAND P_TYPE = 'STANDARD BRUSHED BRASS'\r\n\t) AS ALL_NATIONS\r\nGROUP BY\r\n\tO_YEAR\r\nORDER BY\r\n\tO_YEAR;\r\n";
                        break;
                    case 9:
                        query =
                            "SELECT\r\n\tNATION,\r\n\tO_YEAR,\r\n\tSUM(AMOUNT) AS SUM_PROFIT\r\nFROM\r\n\t(\r\n\t\tSELECT\r\n\t\t\tN_NAME AS NATION,\r\n\t\t\tEXTRACT(YEAR FROM O_ORDERDATE) AS O_YEAR,\r\n\t\t\tL_EXTENDEDPRICE * (1 - L_DISCOUNT) - PS_SUPPLYCOST * L_QUANTITY AS AMOUNT\r\n\t\tFROM\r\n\t\t\tPART,\r\n\t\t\tSUPPLIER,\r\n\t\t\tLINEITEM,\r\n\t\t\tPARTSUPP,\r\n\t\t\tORDERS,\r\n\t\t\tNATION\r\n\t\tWHERE\r\n\t\t\tS_SUPPKEY = L_SUPPKEY\r\n\t\t\tAND PS_SUPPKEY = L_SUPPKEY\r\n\t\t\tAND PS_PARTKEY = L_PARTKEY\r\n\t\t\tAND P_PARTKEY = L_PARTKEY\r\n\t\t\tAND O_ORDERKEY = L_ORDERKEY\r\n\t\t\tAND S_NATIONKEY = N_NATIONKEY\r\n\t\t\tAND P_NAME LIKE '%SNOW%'\r\n\t) AS PROFIT\r\nGROUP BY\r\n\tNATION,\r\n\tO_YEAR\r\nORDER BY\r\n\tNATION,\r\n\tO_YEAR DESC;\r\n";
                        break;
                    case 10:
                        query =
                            "SELECT\r\n\tC_CUSTKEY,\r\n\tC_NAME,\r\n\tSUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS REVENUE,\r\n\tC_ACCTBAL,\r\n\tN_NAME,\r\n\tC_ADDRESS,\r\n\tC_PHONE,\r\n\tC_COMMENT\r\nFROM\r\n\tCUSTOMER,\r\n\tORDERS,\r\n\tLINEITEM,\r\n\tNATION\r\nWHERE\r\n\tC_CUSTKEY = O_CUSTKEY\r\n\tAND L_ORDERKEY = O_ORDERKEY\r\n\tAND O_ORDERDATE >= '1994-04-01'\r\n\tAND O_ORDERDATE < '1994-04-01' + INTERVAL '3' MONTH\r\n\tAND L_RETURNFLAG = 'R'\r\n\tAND C_NATIONKEY = N_NATIONKEY\r\nGROUP BY\r\n\tC_CUSTKEY,\r\n\tC_NAME,\r\n\tC_ACCTBAL,\r\n\tC_PHONE,\r\n\tN_NAME,\r\n\tC_ADDRESS,\r\n\tC_COMMENT\r\nORDER BY\r\n\tREVENUE DESC;\r\n";
                        break;
                    case 11:
                        query =
                            "SELECT\r\n\tPS_PARTKEY,\r\n\tSUM(PS_SUPPLYCOST * PS_AVAILQTY) AS VALUE\r\nFROM\r\n\tPARTSUPP,\r\n\tSUPPLIER,\r\n\tNATION\r\nWHERE\r\n\tPS_SUPPKEY = S_SUPPKEY\r\n\tAND S_NATIONKEY = N_NATIONKEY\r\n\tAND N_NAME = 'ALGERIA'\r\nGROUP BY\r\n\tPS_PARTKEY HAVING\r\n\t\tSUM(PS_SUPPLYCOST * PS_AVAILQTY) > (\r\n\t\t\tSELECT\r\n\t\t\t\tSUM(PS_SUPPLYCOST * PS_AVAILQTY) * 0.0001000000\r\n\t\t\tFROM\r\n\t\t\t\tPARTSUPP,\r\n\t\t\t\tSUPPLIER,\r\n\t\t\t\tNATION\r\n\t\t\tWHERE\r\n\t\t\t\tPS_SUPPKEY = S_SUPPKEY\r\n\t\t\t\tAND S_NATIONKEY = N_NATIONKEY\r\n\t\t\t\tAND N_NAME = 'ALGERIA'\r\n\t\t)\r\nORDER BY\r\n\tVALUE DESC;\r\n";
                        break;
                    case 12:
                        query =
                            "SELECT\r\n\tL_SHIPMODE,\r\n\tSUM(CASE\r\n\t\tWHEN O_ORDERPRIORITY = '1-URGENT'\r\n\t\t\tOR O_ORDERPRIORITY = '2-HIGH'\r\n\t\t\tTHEN 1\r\n\t\tELSE 0\r\n\tEND) AS HIGH_LINE_COUNT,\r\n\tSUM(CASE\r\n\t\tWHEN O_ORDERPRIORITY <> '1-URGENT'\r\n\t\t\tAND O_ORDERPRIORITY <> '2-HIGH'\r\n\t\t\tTHEN 1\r\n\t\tELSE 0\r\n\tEND) AS LOW_LINE_COUNT\r\nFROM\r\n\tORDERS,\r\n\tLINEITEM\r\nWHERE\r\n\tO_ORDERKEY = L_ORDERKEY\r\n\tAND L_SHIPMODE IN ('AIR', 'SHIP')\r\n\tAND L_COMMITDATE < L_RECEIPTDATE\r\n\tAND L_SHIPDATE < L_COMMITDATE\r\n\tAND L_RECEIPTDATE >= '1994-01-01'\r\n\tAND L_RECEIPTDATE < '1994-01-01' + INTERVAL '1' YEAR\r\nGROUP BY\r\n\tL_SHIPMODE\r\nORDER BY\r\n\tL_SHIPMODE;\r\n";
                        break;
                    case 13:
                        query =
                            "SELECT\r\n\tC_COUNT,\r\n\tCOUNT(*) AS CUSTDIST\r\nFROM\r\n\t(\r\n\t\tSELECT\r\n\t\t\tC_CUSTKEY,\r\n\t\t\tCOUNT(O_ORDERKEY) AS C_COUNT\r\n\t\tFROM\r\n\t\t\tCUSTOMER LEFT OUTER JOIN ORDERS ON\r\n\t\t\t\tC_CUSTKEY = O_CUSTKEY\r\n\t\t\t\tAND O_COMMENT NOT LIKE '%SPECIAL%REQUESTS%'\r\n\t\tGROUP BY\r\n\t\t\tC_CUSTKEY\r\n\t) AS C_ORDERS\r\nGROUP BY\r\n\tC_COUNT\r\nORDER BY\r\n\tCUSTDIST DESC,\r\n\tC_COUNT DESC;\r\n";
                        break;
                    case 14:
                        query =
                            "SELECT\r\n\t100.00 * SUM(CASE\r\n\t\tWHEN P_TYPE LIKE 'PROMO%'\r\n\t\t\tTHEN L_EXTENDEDPRICE * (1 - L_DISCOUNT)\r\n\t\tELSE 0\r\n\tEND) / SUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS PROMO_REVENUE\r\nFROM\r\n\tLINEITEM,\r\n\tPART\r\nWHERE\r\n\tL_PARTKEY = P_PARTKEY\r\n\tAND L_SHIPDATE >= '1995-01-01'\r\n\tAND L_SHIPDATE < '1995-01-01' + INTERVAL '1' MONTH;\r\n";
                        break;
                    default:
                        query =
                            "SELECT\r\n\tL_ORDERKEY,\r\n\tSUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS REVENUE,\r\n\tSUM(C_MKTSEGMENT * (1 - L_DISCOUNT)) AS REVENUE2,\r\n\tO_ORDERDATE,\r\n\tO_SHIPPRIORITY\r\nFROM\r\n\tCUSTOMER,\r\n\tORDERS,\r\n\tLINEITEM\r\nWHERE\r\n\tC_MKTSEGMENT = \'HOUSEHOLD\'\r\n\tAND C_CUSTKEY = O_CUSTKEY\r\n\tAND L_ORDERKEY = O_ORDERKEY\r\n\tAND O_ORDERDATE < \'1995-03-31\'\r\n\tAND L_SHIPDATE  > \'1995-03-31\'\r\nGROUP BY\r\n\tL_ORDERKEY,\r\n\tO_ORDERDATE,\r\n\tO_SHIPPRIORITY\r\nORDER BY\r\n\tREVENUE DESC,\r\n\tO_ORDERDATE;\r\n";
                        break;
                }
            }
            else
            {
                switch (queryNumber)
                {
                    case 1:
                        query =
                            "SELECT\r\n\tL_RETURNFLAG,\r\n\tL_LINESTATUS,\r\n\tSUM(L_QUANTITY) AS SUM_QTY,\r\n\tSUM(L_EXTENDEDPRICE) AS SUM_BASE_PRICE,\r\n\tSUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS SUM_DISC_PRICE,\r\n\tSUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT) * (1 + L_TAX)) AS SUM_CHARGE,\r\n\tAVG(L_QUANTITY) AS AVG_QTY,\r\n\tAVG(L_EXTENDEDPRICE) AS AVG_PRICE,\r\n\tAVG(L_DISCOUNT) AS AVG_DISC,\r\n\tCOUNT(*) AS COUNT_ORDER\r\nFROM\r\n\tLINEITEM\r\nWHERE\r\n\tL_SHIPDATE <='1998-12-01' - INTERVAL '90' DAY\r\nGROUP BY\r\n\tL_RETURNFLAG,\r\n\tL_LINESTATUS\r\nORDER BY\r\n\tL_RETURNFLAG,\r\n\tL_LINESTATUS;\r\n";
                        box.Text = "3";
                        break;
                    case 3:
                        query =
                            "SELECT\r\n\tL_ORDERKEY,\r\n\tSUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS REVENUE,\r\n\tO_ORDERDATE,\r\n\tO_SHIPPRIORITY\r\nFROM\r\n\tCUSTOMER,\r\n\tORDERS,\r\n\tLINEITEM\r\nWHERE\r\n\tC_MKTSEGMENT = 'HOUSEHOLD'\r\n\tAND C_CUSTKEY = O_CUSTKEY\r\n\tAND L_ORDERKEY = O_ORDERKEY\r\n\tAND O_ORDERDATE < '1995-03-31'\r\n\tAND L_SHIPDATE > '1995-03-31'\r\nGROUP BY\r\n\tL_ORDERKEY,\r\n\tO_ORDERDATE,\r\n\tO_SHIPPRIORITY\r\nORDER BY\r\n\tREVENUE DESC,\r\n\tO_ORDERDATE;\r\n";
                        box.Text = "5";
                        break;
                    case 5:
                        query =
                            "SELECT\r\n\tN_NAME,\r\n\tSUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS REVENUE\r\nFROM\r\n\tCUSTOMER,\r\n\tORDERS,\r\n\tLINEITEM,\r\n\tSUPPLIER,\r\n\tNATION,\r\n\tREGION\r\nWHERE\r\n\tC_CUSTKEY = O_CUSTKEY\r\n\tAND L_ORDERKEY = O_ORDERKEY\r\n\tAND L_SUPPKEY = S_SUPPKEY\r\n\tAND C_NATIONKEY = S_NATIONKEY\r\n\tAND S_NATIONKEY = N_NATIONKEY\r\n\tAND N_REGIONKEY = R_REGIONKEY\r\n\tAND R_NAME = 'MIDDLE EAST'\r\n\tAND O_ORDERDATE >= '1995-01-01'\r\n\tAND O_ORDERDATE < '1995-01-01' + INTERVAL '1' YEAR\r\nGROUP BY\r\n\tN_NAME\r\nORDER BY\r\n\tREVENUE DESC;\r\n";
                        box.Text = "6";
                        break;
                    case 6:
                        query =
                            "SELECT\r\n\tSUM(L_EXTENDEDPRICE * L_DISCOUNT) AS REVENUE\r\nFROM\r\n\tLINEITEM\r\nWHERE\r\n\tL_SHIPDATE >= '1997-01-01'\r\n\tAND L_SHIPDATE < '1997-01-01' + INTERVAL '1' YEAR\r\n\tAND L_DISCOUNT BETWEEN 0.07 - 0.01 AND 0.07 + 0.01\r\n\tAND L_QUANTITY < 24;\r\n";
                        box.Text = "10";
                        break;
                    case 10:
                        query =
                            "SELECT\r\n\tC_CUSTKEY,\r\n\tC_NAME,\r\n\tSUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS REVENUE,\r\n\tC_ACCTBAL,\r\n\tN_NAME,\r\n\tC_ADDRESS,\r\n\tC_PHONE,\r\n\tC_COMMENT\r\nFROM\r\n\tCUSTOMER,\r\n\tORDERS,\r\n\tLINEITEM,\r\n\tNATION\r\nWHERE\r\n\tC_CUSTKEY = O_CUSTKEY\r\n\tAND L_ORDERKEY = O_ORDERKEY\r\n\tAND O_ORDERDATE >= '1994-04-01'\r\n\tAND O_ORDERDATE < '1994-04-01' + INTERVAL '3' MONTH\r\n\tAND L_RETURNFLAG = 'R'\r\n\tAND C_NATIONKEY = N_NATIONKEY\r\nGROUP BY\r\n\tC_CUSTKEY,\r\n\tC_NAME,\r\n\tC_ACCTBAL,\r\n\tC_PHONE,\r\n\tN_NAME,\r\n\tC_ADDRESS,\r\n\tC_COMMENT\r\nORDER BY\r\n\tREVENUE DESC;\r\n";
                        box.Text = "12";
                        break;
                    case 12:
                        query =
                            "SELECT\r\n\tL_SHIPMODE,\r\n\tSUM(CASE\r\n\t\tWHEN O_ORDERPRIORITY = '1-URGENT'\r\n\t\t\tOR O_ORDERPRIORITY = '2-HIGH'\r\n\t\t\tTHEN 1\r\n\t\tELSE 0\r\n\tEND) AS HIGH_LINE_COUNT,\r\n\tSUM(CASE\r\n\t\tWHEN O_ORDERPRIORITY <> '1-URGENT'\r\n\t\t\tAND O_ORDERPRIORITY <> '2-HIGH'\r\n\t\t\tTHEN 1\r\n\t\tELSE 0\r\n\tEND) AS LOW_LINE_COUNT\r\nFROM\r\n\tORDERS,\r\n\tLINEITEM\r\nWHERE\r\n\tO_ORDERKEY = L_ORDERKEY\r\n\tAND L_SHIPMODE IN ('AIR', 'SHIP')\r\n\tAND L_COMMITDATE < L_RECEIPTDATE\r\n\tAND L_SHIPDATE < L_COMMITDATE\r\n\tAND L_RECEIPTDATE >= '1994-01-01'\r\n\tAND L_RECEIPTDATE < '1994-01-01' + INTERVAL '1' YEAR\r\nGROUP BY\r\n\tL_SHIPMODE\r\nORDER BY\r\n\tL_SHIPMODE;\r\n";
                        box.Text = "14";
                        break;
                    case 14:
                        query =
                            "SELECT\r\n\t100.00 * SUM(CASE\r\n\t\tWHEN P_TYPE LIKE 'PROMO%'\r\n\t\t\tTHEN L_EXTENDEDPRICE * (1 - L_DISCOUNT)\r\n\t\tELSE 0\r\n\tEND) / SUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS PROMO_REVENUE\r\nFROM\r\n\tLINEITEM,\r\n\tPART\r\nWHERE\r\n\tL_PARTKEY = P_PARTKEY\r\n\tAND L_SHIPDATE >= '1995-01-01'\r\n\tAND L_SHIPDATE < '1995-01-01' + INTERVAL '1' MONTH;\r\n";
                        break;
                    default:
                        query =
                            "SELECT\r\n\tL_ORDERKEY,\r\n\tSUM(L_EXTENDEDPRICE * (1 - L_DISCOUNT)) AS REVENUE,\r\n\tSUM(C_MKTSEGMENT * (1 - L_DISCOUNT)) AS REVENUE2,\r\n\tO_ORDERDATE,\r\n\tO_SHIPPRIORITY\r\nFROM\r\n\tCUSTOMER,\r\n\tORDERS,\r\n\tLINEITEM\r\nWHERE\r\n\tC_MKTSEGMENT = \'HOUSEHOLD\'\r\n\tAND C_CUSTKEY = O_CUSTKEY\r\n\tAND L_ORDERKEY = O_ORDERKEY\r\n\tAND O_ORDERDATE < \'1995-03-31\'\r\n\tAND L_SHIPDATE  > \'1995-03-31\'\r\nGROUP BY\r\n\tL_ORDERKEY,\r\n\tO_ORDERDATE,\r\n\tO_SHIPPRIORITY\r\nORDER BY\r\n\tREVENUE DESC,\r\n\tO_ORDERDATE;\r\n";
                        box.Text = "1";
                        break;
                }
            }

            return query;
        }
        
        private string ShowDataBase(DataBaseStructure dataBase)
        {
            string output = "";
            foreach (TableStructure table in dataBase.Tables)
            {
                output += table.Name + Environment.NewLine;
                foreach (ColumnStructure column in table.Columns)
                {
                    output += "\t" + column.Name + "\t\t" + column.Size + "\t\t" + column.Type.Name + Environment.NewLine;
                }
            }
            return output;
        }

        private DataBaseStructure CreateSubDatabase(DataBaseStructure fullDataBase, MainListener listener)
        {
            DataBaseStructure subDataBase;
            List<TableStructure> inputMainTables = new List<TableStructure>();
            List<ColumnStructure> notSubColumns = new List<ColumnStructure>();

            foreach (ColumnStructure subColumn in listener.SubColumns)
            {
                foreach (TableStructure subTable in listener.SubTables) 
                {
                    foreach (TableStructure fullTable in fullDataBase.Tables)
                    {
                        if (fullTable.Name == subTable.Name)
                        {
                            foreach (ColumnStructure fullColumn in fullTable.Columns)
                            {
                                if (fullColumn.Name == subColumn.Name && subTable.DotedId == subColumn.DotTableId)
                                {
                                    subColumn.UsageCounter++;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (subColumn.UsageCounter > 0)
                {
                    subColumn.UsageCounter = 0;
                }
                else
                {
                    notSubColumns.Add(subColumn);
                }
            }

            foreach (TableStructure mainTable in listener.MainTables)
            {
                List<ColumnStructure> tmpColumns = new List<ColumnStructure>();
                TableStructure tmpTable;
                foreach (TableStructure fullTable in fullDataBase.Tables)
                {
                    if (mainTable.Name == fullTable.Name)
                    {
                        
                        foreach (ColumnStructure fullColumn in fullTable.Columns)
                        {
                            ColumnStructure tmpColumn = new ColumnStructure(fullColumn);
                            tmpColumn.SoureInterval = new List<Interval>();

                            foreach (ColumnStructure mainColumn in listener.MainColumns)
                            {
                                if (fullColumn.Name == mainColumn.Name && mainColumn.DotTableId == mainTable.DotedId)
                                {
                                    tmpColumn.DotTableId = mainColumn.DotTableId;
                                    tmpColumn.SoureInterval.AddRange(mainColumn.SoureInterval);
                                    tmpColumn.UsageCounter++;
                                }
                            }

                            foreach (ColumnStructure notSubColumn in notSubColumns)
                            {
                                if (fullColumn.Name == notSubColumn.Name && notSubColumn.DotTableId == mainTable.DotedId)
                                {
                                    tmpColumn.DotTableId = notSubColumn.DotTableId;
                                    tmpColumn.SoureInterval.AddRange(notSubColumn.SoureInterval);
                                    tmpColumn.UsageCounter++;
                                }
                            }

                            if (tmpColumn.UsageCounter > 0)
                            {
                                tmpColumns.Add(tmpColumn);
                            }
                        }

                        if (tmpColumns.Count > 0)
                        {
                            tmpTable = new TableStructure(fullTable);
                            tmpTable.DotedId = mainTable.DotedId;
                            tmpTable.SourceInterval = mainTable.SourceInterval;
                            tmpTable.Columns = tmpColumns.ToArray();
                            inputMainTables.Add(tmpTable);
                        }
                    }
                }
            }


            subDataBase = new DataBaseStructure("sub_" + listener.Depth + "_" + fullDataBase.Name, inputMainTables.ToArray(), fullDataBase.Types);
            return subDataBase;
        }


        #endregion

        #region Методы Для Select

        private void SetIsForSelectFlags(DataBaseStructure dataBase, List<string> isForSelectColumnNames)
        {
            foreach (TableStructure table in dataBase.Tables)
            {
                foreach (ColumnStructure column in table.Columns)
                {
                    foreach (string columnName in isForSelectColumnNames)
                    {
                        if (columnName == column.Name)
                        {
                            column.IsForSelect = true;
                        }
                    }
                }
            }
        }

        private void FillAsStructures(DataBaseStructure dataBase, List<AsStructure> asStructures)
        {            
            List<TableStructure> asTables = new List<TableStructure>();
            foreach (AsStructure asStructure in asStructures)
            {
                asTables = new List<TableStructure>();
                ColumnStructure rightColumn = new ColumnStructure(asStructure.GetAsRightName);
                rightColumn.Size = -1;
                List<ColumnStructure> tmpColumns = new List<ColumnStructure>();
                foreach (var table in dataBase.Tables)
                {
                    foreach (ColumnStructure column in table.Columns)
                    {
                        foreach (string columnName in asStructure.ColumnNames)
                        {
                            if (column.Name == columnName)
                            {
                                tmpColumns.Add(column);
                                if (column.Size > rightColumn.Size)
                                {
                                    rightColumn.Size = column.Size;
                                    rightColumn.Type = column.Type;
                                    rightColumn.TypeID = column.TypeID;
                                }
                                asTables.Add(table);
                            }
                        }
                    }
                }
                asStructure.AsColumns = tmpColumns.ToArray();
                asStructure.AsRightColumn = rightColumn;
                
                asTables = asTables.Distinct().ToList();
                if (asTables.Count == 1)
                {
                    asStructure.IsSelectPart = true;
                    asStructure.Tables = asTables;
                    asStructure.AsRightColumn.OldName = asStructure.AsRightColumn.Name;
                    asStructure.AsRightColumn.Name =
                        asStructure.Tables[0].ShortName + asStructure.AsRightColumn.Name;
                    asStructure.AsRightColumn.IsRenamed = true;
                }
                else
                {
                    asStructure.IsSelectPart = false;
                    asStructure.Tables = asTables;
                }
                if(asStructure.AsString == "*")
                {
                    foreach (S_Type type in dataBase.Types)
                    {
                        if (type.Name == "INT")
                        {
                            asStructure.AsRightColumn.Type = type;
                            asStructure.AsRightColumn.Size = type.Size;
                            asStructure.AsRightColumn.TypeID = type.ID;
                        }
                    }
                }

                if (asStructure.AggregateFunctionName != null)
                {
                    if (asStructure.AggregateFunctionName.ToLower() == "extract")
                    {
                        S_Type tmpType = null;
                        foreach (S_Type type in dataBase.Types)
                        {
                            if (type.Name == "INT")
                            {
                                tmpType = type;
                                break;

                            }
                        }

                        if (tmpType == null)
                        {
                            tmpType = new S_Type("INT", 6, (dataBase.Types.Length + 1).ToString());
                            List<S_Type> tmpTypes = new List<S_Type>();
                            foreach (S_Type type in dataBase.Types)
                            {
                                tmpTypes.Add(type);
                            }

                            tmpTypes.Add(tmpType);
                            dataBase.Types = tmpTypes.ToArray();
                        }

                        asStructure.AsRightColumn.Type = tmpType;
                        asStructure.AsRightColumn.Size = tmpType.Size;
                        asStructure.AsRightColumn.TypeID = tmpType.ID;
                    }
                }
            }
        }

        private void FillLikeStructures(DataBaseStructure dataBase, List<LikeStructure> likeStructures)
        {
            foreach (LikeStructure like in likeStructures)
            {
                foreach (TableStructure table in dataBase.Tables)
                {
                    foreach (ColumnStructure column in table.Columns)
                    {
                        if (column.Name == like.ColumnName)
                        {
                            like.LeftColumn = column;
                            like.Table = table;
                        }
                    }
                }
            }
        }
        
        private void FindeWhereStructureTable(List<WhereStructure> whereList, DataBaseStructure dataBase)
        {
            foreach (WhereStructure ws in whereList)
            {
                foreach (TableStructure dataBaseTable in dataBase.Tables)
                {
                    foreach (ColumnStructure column in dataBaseTable.Columns)
                    {
                        if (column.Name == ws.LeftColumn)
                        {
                            ws.Column = column;
                            ws.Table = dataBaseTable.Name;
                        }
                        if(column.Name == ws.RightExpr)
                        {
                            ws.RightColumn = column;
                        }
                    }

                }
            }
        }

        private List<WhereStructure> GetCorrectWhereStructure(List<WhereStructure> whereList, string tableName)
        {
            List<WhereStructure> outList = new List<WhereStructure>();
            foreach (var ws in whereList)
            {
                if (ws.Table == tableName)
                {
                    outList.Add(ws);
                }
            }

            return outList;
        }

        private List<AsStructure> GetCorrectAsStructures(List<AsStructure> asStructures, TableStructure table)
        {
            List<AsStructure> outList = new List<AsStructure>();
            foreach (var asStructure in asStructures)
            {
                if (asStructure.Tables.Count==1)
                {
                    if (asStructure.Tables[0] == table && asStructure.IsSelectPart)
                    {
                        outList.Add(asStructure);
                    }
                }
            }

            return outList;
        }
        
        private List<BetweenStructure> GetCorrectBetweenList(List<BetweenStructure> listenerBetweenList, TableStructure queryDbTable)
        {
            List<BetweenStructure> tmpList = new List<BetweenStructure>();
            foreach (ColumnStructure column in queryDbTable.Columns)
            {
                foreach (BetweenStructure betweenStructure in listenerBetweenList)
                {
                    if (column.Name == betweenStructure.ColumnName)
                    {
                        betweenStructure.Column = column;
                        tmpList.Add(betweenStructure);
                    }
                }
            }

            return tmpList;
        }

        private void FillInStructures(DataBaseStructure queryDb, List<InStructure> listenerInStructureList)
        {
            foreach (InStructure inStructure in listenerInStructureList)
            {
                foreach (TableStructure table in queryDb.Tables)
                {
                    foreach (ColumnStructure column in table.Columns)
                    {
                        if (column.Name == inStructure.LeftColumnName)
                        {
                            inStructure.LeftColumn = column;
                            inStructure.Table = table;
                            break;
                        }
                    }
                }
            }

        }


        #endregion

        #region Методы для Join

        //"Филл Джонс"
        private List<JoinStructure> FillJoins(List<JoinStructure> joinList, DataBaseStructure dataBase,
            List<SelectStructure> selectQueries)
        {
            int i = 1;
            foreach (JoinStructure join in joinList)
            {
                i++;
                foreach (TableStructure table in dataBase.Tables)
                {
                    foreach (ColumnStructure column in table.Columns)
                    {
                        if (join.LeftColumnString == column.Name  || join.LeftColumnString.Replace(".","") == column.DotTableId + column.Name)
                        {
                            join.LeftColumn = column;
                            foreach (SelectStructure select in selectQueries)
                            {
                                if (select.TableName == table.Name && select.InputTable.DotedId == table.DotedId)
                                {
                                    join.LeftSelect = select;
                                    break;
                                }
                            }

                            break;
                        }
                    }
                }

                foreach (TableStructure table in dataBase.Tables)
                {
                    foreach (ColumnStructure column in table.Columns)
                    {
                        if (join.RightColumnString == column.Name || join.RightColumnString.Replace(".", "") == column.DotTableId + column.Name)
                        {
                            join.RightColumn = column;
                            foreach (SelectStructure select in selectQueries)
                            {
                                if (select.TableName == table.Name && select.InputTable.DotedId == table.DotedId)
                                {
                                    join.RightSelect = select;
                                    break;
                                }
                            }

                            break;
                        }
                    }
                }
            }
            for (i = 0; i < joinList.Count - 1; i++)
            {
                for (int j = i + 1; j < joinList.Count - 1; j++)
                {
                    if (joinList[i].LeftSelect != null && joinList[j].LeftSelect != null &&
                        joinList[i].RightSelect != null && joinList[j].RightSelect != null)
                    {
                        if (joinList[i].LeftSelect.TableName == joinList[j].LeftSelect.TableName &&
                         joinList[i].RightSelect.TableName == joinList[j].RightSelect.TableName ||
                         joinList[i].LeftSelect.TableName == joinList[j].RightSelect.TableName &&
                         joinList[i].RightSelect.TableName == joinList[j].LeftSelect.TableName
                        )
                        {
                            joinList[i].AdditionalJoins.Add(joinList[j]);
                            joinList[j].IsAdditional = true;
                        }

                    }
                }
            }
            List<JoinStructure> tmpJoins = new List<JoinStructure>();
            foreach (JoinStructure join in joinList)
            {
                if(!join.IsAdditional)
                {tmpJoins.Add(join);}
            }

            return tmpJoins;
        }
        
        private List<JoinStructure> GetJoinSequence(List<JoinStructure> joinStructures, int joinDepth)
        {
            List<JoinStructure> outJoin = joinStructures;
            if (outJoin.Count != 0)
            {
                List<Pares> j_list_Pares = new List<Pares>();

                #region Magic

                foreach (JoinStructure joinStructure in joinStructures)
                {
                    if (joinStructure.LeftColumn != null && joinStructure.RightColumn != null)
                    {
                        if (joinStructure.LeftSelect.Name[2] == joinStructure.RightSelect.Name[2])
                        {
                            Pares pr = new Pares(joinStructure.LeftSelect.Name, joinStructure.RightSelect.Name);
                            j_list_Pares.Add(pr);
                        }
                    }
                }

                bool razriv = true;
                List<List<string>> containers = new List<List<string>>();
                List<string> cont = new List<string>()
                {
                    j_list_Pares[0].Left,
                };
                //f1
                for (int j = 0; j < cont.Count;)
                {
                    razriv = true;
                    //f1.1
                    for (int i = 0; i < j_list_Pares.Count; i++)
                    {
                        //нахождение пар, слева или справа в которых находится сont[j]. Под парой здесь понимается структура с двумя строками P("A","B").
                        //В начале цикла в cont[0] содержится ЛЕВЫЙ аргумент нулевой(первой по счёту) пары.
                        if (cont[j] == j_list_Pares[i].Left)
                        {
                            cont.Add(j_list_Pares[i].Right);
                            j_list_Pares[i].IsForDelete = true;
                            razriv = false;
                        }

                        if (cont[j] == j_list_Pares[i].Right)
                        {
                            cont.Add(j_list_Pares[i].Left);
                            j_list_Pares[i].IsForDelete = true;
                            razriv = false;
                        }
                    }
                    //fo1.1
                    foreach (Pares pares in j_list_Pares)
                    {
                        //поиск пар, аргументы которых уже находятся в контейнере.
                        bool haveLeft = false;
                        bool haveRight = false;
                        //fo1.1.1
                        foreach (string s in cont)
                        {
                            if (pares.Left == s)
                            {
                                haveLeft = true;
                            }

                            if (pares.Right == s)
                            {
                                haveRight = true;
                            }
                        }

                        if (haveRight && haveLeft)
                        {
                            pares.IsForDelete = true;
                        }
                    }

                    List<Pares> tmp = new List<Pares>();
                    //f1.2
                    for (int i = 0; i < j_list_Pares.Count; i++)
                    {
                        if (!j_list_Pares[i].IsForDelete)
                        {
                            tmp.Add(j_list_Pares[i]);
                        }
                    }

                    j_list_Pares = tmp;
                    j++;
                    if (razriv && j_list_Pares.Count > 0 && j == cont.Count)
                    {
                        containers.Add(cont);
                        cont = new List<string>();
                        cont.Add(j_list_Pares[0].Left);
                        j = 0;
                    }
                }

                if (j_list_Pares.Count == 0)
                {
                    containers.Add(cont);
                }

                #endregion //создан контейнер селектов(s1,s3,s2,s5)

                //в containers поледовательности конвейров джойн, даже если они разделяются.
                //алгоритм помог придумать выпускник КАИ Алексей Казнаецев.
                foreach (List<string> container in containers)
                {
                    int i = 0;
                    List<JoinStructure> j_sequence = new List<JoinStructure>();
                    for (; i < container.Count;)
                    {
                        if (i == 0)
                        {
                            JoinStructure tmp = FindeJoin(container[0], container[1], outJoin);
                            tmp.IsFirst = true;
                            j_sequence.Add(tmp);
                            i = 2;
                        }
                        else
                        {
                            int stopper = j_sequence.Count;
                            for (int j = 0; j < i; j++)
                            {
                                JoinStructure tmp = FindeJoin(container[j], container[i], outJoin);
                                if (tmp.Name != "ERROR")
                                {
                                    j_sequence.Add(tmp);
                                }

                                //если в последовательность джойнов добавился новый 
                                if (j_sequence.Count == stopper + 1)
                                {
                                    break;
                                }
                            }

                            i++;
                        }

                        for (int j = 1; j < j_sequence.Count; j++)
                        {
                            j_sequence[j].LeftJoin = j_sequence[j - 1];
                            for (int k = 0; k < j; k++)
                            {
                                if (j_sequence[j].RightSelect == j_sequence[k].LeftSelect ||
                                    j_sequence[j].RightSelect == j_sequence[k].RightSelect)
                                {
                                    j_sequence[j].Switched = true;
                                }
                            }
                        }
                    }

                    for (int j = 0; j < j_sequence.Count; j++)
                    {
                        j_sequence[j].Name = "J_" + joinDepth + "_" + j;
                    }
                }
            }

            return outJoin;
        }

        private List<JoinStructure> SortJoin(List<JoinStructure> joinStructures, int joinDepth)
        {
            List<JoinStructure> tmp = new List<JoinStructure>();
            int notJoinedCount = 0;
            for (int i = 0; i < joinStructures.Count; i++)
            {
                string s = "J_" + joinDepth + "_"  + i;
                foreach (JoinStructure joinStructure in joinStructures)
                {
                    if (joinStructure.Name == s)
                    {
                        tmp.Add(joinStructure);
                    }
                }
            }

            foreach (JoinStructure joinStructure in joinStructures)
            {
                if (joinStructure.Name == null)
                {
                    joinStructure.IsAdditional = true;
                    tmp.LastOrDefault().AdditionalJoins.Add(joinStructure);
                    //joinStructure.Name = "J_" + joinDepth + "_"  + tmp.Count.ToString();
                    //joinStructure.LeftJoin = tmp.Last();
                    //joinStructure.LeftSelect = null;
                    //joinStructure.RightSelect = null;
                    //tmp.Add(joinStructure);
                }
            }
            return tmp;
        }

        private JoinStructure FindeJoin(string j1, string j2, List<JoinStructure> joinList)
        {
            //не должно быть ошибок
            JoinStructure output = new JoinStructure("ERROR", "ERROR", "ERROR", new Interval(0,0), _sortRule, false);
            output.Name = "ERROR";
            foreach (JoinStructure structure in joinList)
            {
                if (structure.LeftSelect != null && structure.RightSelect != null)
                {
                    if ((structure.LeftSelect.Name == j1 && structure.RightSelect.Name == j2) ||
                        (structure.LeftSelect.Name == j2 && structure.RightSelect.Name == j1))
                    {
                        output = structure;
                        break;
                    }
                }
            }

            return output;
        }

        private void CheckBinaryType(BinaryComparisionPredicateStructure binary, DataBaseStructure queryDb)
        {
            bool isLeftCollumn = false;
            bool isRightCollumn = false;
            foreach (TableStructure table in queryDb.Tables)
            {
                foreach (var columnStructure in table.Columns)
                {
                    if (columnStructure.Name == binary.LeftString)
                    {
                        isLeftCollumn = true;
                    }
                    if (columnStructure.Name == binary.RightString)
                    {
                        isRightCollumn = true;
                    }
                }

                if (isRightCollumn && isLeftCollumn)
                {
                    binary.Type = 4;
                    break;
                }
                else
                {
                    isRightCollumn = false;
                    isLeftCollumn = false;
                }
            }

        }

        #endregion

        #region Методы для Sort

        private ColumnStructure GetCorrectOrderByColumn(List<ColumnStructure> columns, string columnName)
        {
            ColumnStructure correctColumn = new ColumnStructure();
            foreach (ColumnStructure column in columns)
            {
                if (column.Name == columnName || column.OldName == columnName)
                {
                    correctColumn = column;
                }
            }
            return correctColumn;
        }
        
        #endregion

        private void CreateScheme(SelectStructure[] selectQuery)
        {
            List<TableStructure> outTablesList = new List<TableStructure>();
            foreach (var selectStructure in selectQuery)
            {
                outTablesList.Add(selectStructure.OutTable);
            }

            DataBaseStructure outDB = new DataBaseStructure("SELECT_OUT_DB", outTablesList.ToArray());
            MatchColumns(_dbName, outDB);
            outDB.Name = _dbName.Name + "_Select";
            outDB.Types = _dbName.Types;
            using (FileStream fs = new FileStream(@"res\SelectOutDB.xml", FileMode.Create, FileAccess.ReadWrite))
            {
                XmlSerializer dbSerializer = new XmlSerializer(typeof(DataBaseStructure));
                dbSerializer.Serialize(fs, outDB);
            }
        }
        
        private void CreateScheme(List<JoinStructure> joinQuerry)
        {
            List<TableStructure> outTablesList = new List<TableStructure>();
            foreach (var joinStructure in joinQuerry)
            {
                outTablesList.Add(joinStructure.OutTable);
            }

            DataBaseStructure outDB = new DataBaseStructure("JOIN_OUT_DB", outTablesList.ToArray());
            MatchColumns(_dbName, outDB);
            outDB.Name = _dbName.Name + "_Join";
            outDB.Types = _dbName.Types;
            using (FileStream fs = new FileStream(@"res\JoinOutDB.xml", FileMode.Create, FileAccess.ReadWrite))
            {
                XmlSerializer dbSerializer = new XmlSerializer(typeof(DataBaseStructure));
                dbSerializer.Serialize(fs, outDB);
            }
        }

        private void CreateScheme(NewSortStructure sortQuerry)
        {
            List<TableStructure> outTables = new List<TableStructure>();
            outTables.Add(sortQuerry.OutDataBase.Tables[0]);
            DataBaseStructure outDB = sortQuerry.OutDataBase;
            MatchColumns(_dbName, outDB);
            outDB.Name = _dbName.Name + "_Sort";
            outDB.Types = _dbName.Types;
            using (FileStream fs = new FileStream(@"res\SortOutDB.xml", FileMode.Create, FileAccess.ReadWrite))
            {
                XmlSerializer dbSerializer = new XmlSerializer(typeof(DataBaseStructure));
                dbSerializer.Serialize(fs, outDB);
            }
        }
       
        #region Make-методы
        
        private SelectStructure[] MakeSelect(DataBaseStructure dataBase, MainListener listener)
        {
            DataBaseStructure queryDB = CreateSubDatabase(dataBase, listener);
            SetIsForSelectFlags(queryDB, listener.SelectColumnNames);
            SelectStructure[] selectQueries = new SelectStructure[queryDB.Tables.Length];
            List<WhereStructure> tmpWhere = new List<WhereStructure>();
            
            foreach (BinaryComparisionPredicateStructure binary in listener.Binaries)
            {
                CheckBinaryType(binary, queryDB);
                if (binary.Type == 1)
                {
                    WhereStructure tmp = new WhereStructure(binary.LeftString, binary.ComparisionSymphol, binary.RightString, binary.SourceInterval);
                    tmpWhere.Add(tmp);
                }

                if (binary.Type == 4)
                {
                    WhereStructure tmp = new WhereStructure(binary.LeftString, binary.ComparisionSymphol, binary.RightString, binary.SourceInterval);
                    tmpWhere.Add(tmp);
                }
            }

            FindeWhereStructureTable(tmpWhere, queryDB);
            FillAsStructures(queryDB, listener.AsList);
            FillLikeStructures(queryDB, listener.LkeList);
            FillInStructures(queryDB, listener.InStructureList);

            for (var i = 0; i < queryDB.Tables.Length; i++)
            {
                selectQueries[i] = new SelectStructure("S_" + listener.Depth + "_" + i, queryDB.Tables[i],
                    GetCorrectWhereStructure(tmpWhere, queryDB.Tables[i].Name),
                    GetCorrectAsStructures(listener.AsList, queryDB.Tables[i]), _sortRule, GetCorrectBetweenList(listener.BetweenList, queryDB.Tables[i])
                    );
            }
            foreach (SelectStructure select in selectQueries)
            {
                foreach (LikeStructure like in listener.LkeList)
                { 
                    if (select.TableName == like.Table.Name)
                    {
                        select.LikeList= new List<LikeStructure>();
                        select.LikeList.Add(like);
                    }
                }

                foreach (InStructure inStructure in listener.InStructureList)
                {
                    if (select.TableName == inStructure.Table.Name)
                    {
                        select.InStructureList = new List<InStructure>();
                        select.InStructureList.Add(inStructure);
                    }
                }
                select.CreateQuerry();
                select.SetIndexes();
            }

            
            CreateScheme(selectQueries);
            return selectQueries;
        }
                
        private JoinStructure[] MakeJoin(DataBaseStructure dataBase, MainListener listener, SelectStructure[] selects)
        {
            DataBaseStructure queryDB = CreateSubDatabase(dataBase, listener);
            List<JoinStructure> tmpJoins = new List<JoinStructure>();
            //List<JoinStructure> excludedJoin = new List<JoinStructure>();
            List<JoinStructure> tmpList = new List<JoinStructure>();
            JoinStructure[] joinQueries = new JoinStructure[0];

            foreach (var binary in listener.Binaries) 
            {
                if (binary.Type == 2 && binary.ComparisionSymphol == "=")
                {
                    JoinStructure tmp = new JoinStructure(binary.LeftString, binary.RightString, binary.ComparisionSymphol, binary.SourceInterval, _sortRule, binary.IsOuterJoin);
                    tmpJoins.Add(tmp);
                }
            }

            if (tmpJoins.Count > 0)
            {
                joinQueries = FillJoins(tmpJoins.ToList(), queryDB, selects.ToList()).ToArray();

                foreach (JoinStructure join in joinQueries)
                {
                    join.CheckIsFilled();
                    if (join.IsFilled)
                    {
                        tmpList.Add(join);
                        //if (join.LeftColumn.IsPrimary == 1 || join.RightColumn.IsPrimary == 1)
                        //{
                        //    tmpList.Add(join);
                        //}
                        //else
                        //{
                        //    join.IsAdditional = true;
                        //    excludedJoin.Add(join);
                        //}
                    }
                }

                if (tmpList.Count > 0)
                {
                    List<List<JoinStructure>> shuffledJoins = ShuffleJoin(tmpList);

                    List<JoinStructure> unitedJoin = GetJoinSequence(shuffledJoins[0], listener.Depth);
                    //unitedJoin.AddRange(excludedJoin);



                    joinQueries = SortJoin(unitedJoin.ToList(), listener.Depth).ToArray();

                    foreach (var join in joinQueries)
                    {
                        join.CreateQuerry();
                    }

                    for (int i = joinQueries.Length - 1; i >= 0; i--)
                    {
                        joinQueries[i].SetIndex();
                    }

                    foreach (JoinStructure joinQuery in joinQueries)
                    {
                        joinQuery.CheckIsDistinct();
                    }

                    CreateScheme(joinQueries.ToList());
                }
                else
                {
                    joinQueries = tmpList.ToArray();
                }
            }

            return joinQueries;
        }

        private List<List<JoinStructure>> ShuffleJoin(List<JoinStructure> inputJoins)
        {
            List<List<JoinStructure>> outSequence = new List<List<JoinStructure>>();

            for (int i = 0; i < inputJoins.Count; i++)
            {
                List<JoinStructure> tmpSeq = new List<JoinStructure>();
                tmpSeq.Add(inputJoins[i]);
                for (int j = 0; j < inputJoins.Count; j++)
                {
                    if (i != j)
                    {
                        tmpSeq.Add(inputJoins[j]);
                    }
                }

                outSequence.Add(tmpSeq);
            }

            return outSequence;
        }

        private NewSortStructure MakeSort(DataBaseStructure dataBase, MainListener listener, BaseRule sortRule)
        {
            SelectStructure[] selects = MakeSelect(dataBase, listener);
            SelectStructure[] subSelects = new SelectStructure[] { };
            JoinStructure[] joins = MakeJoin(dataBase, listener, selects);
            JoinStructure[] subJoins = new JoinStructure[] { };
            if (_listener.SubQueryListeners.Count != 0)
            {
                foreach (var subQlistener in _listener.SubQueryListeners)
                {
                    subSelects = MakeSelect(dataBase, subQlistener);
                    subJoins = MakeJoin(dataBase, subQlistener, subSelects);
                }

            }

            NewSortStructure sortQuery = new NewSortStructure("So_1", sortRule, dataBase);
            sortQuery.SetIndexes();
            #region OLD

            //SelectStructure[] select = selects;
            //JoinStructure[] join = joins;
            //SortStructure sortQuery = new SortStructure("So_1");
            //List<OrderByStructure> orderByStructures = listener.OrderByList;
            //List<ColumnStructure> inputColumns;

            //if (join.Length != 0)
            //{
            //    inputColumns = join.LastOrDefault().Columns;
            //}
            //else
            //{
            //    inputColumns = select.LastOrDefault().OutColumn.ToList();
            //}

            //if (orderByStructures != null)
            //    foreach (OrderByStructure orderByStructure in orderByStructures)
            //    {
            //        orderByStructure.Column =
            //            GetCorrectOrderByColumn(inputColumns, orderByStructure.ColumnName);
            //    }

            //sortQuery.Select = select.LastOrDefault();
            //sortQuery.Join = join.LastOrDefault();
            //FillAsStructures(dataBase, listener.AsList);
            //sortQuery.AsSortList = listener.AsList;
            //sortQuery.GroupByColumnList = listener.GroupByColumnsNames;
            //sortQuery.OrderByStructures = orderByStructures;

            //if (listener.SubQueryListeners.Count>0)
            //{
            //    JoinStructure notFilledJoinForSort;
            //    DataBaseStructure subDB = CreateSubDatabase(_queryDB,
            //        listener.SubQueryListeners[0].TableNames.ToArray(),
            //        listener.SubQueryListeners[0].ColumnNames.ToArray(),
            //        listener.SubQueryListeners[0].RemoveCounterColumsNames.ToArray());
            //    SelectStructure[] subSelects = MakeSelect(subDB, listener.SubQueryListeners[0]);
            //    JoinStructure[] subJoins = MakeJoin(subDB, listener.SubQueryListeners[0], subSelects, out notFilledJoinForSort);
            //    foreach (BinaryComparisionPredicateStructure binary in listener.Binaries)
            //    {
            //        if (binary.Type == 3)
            //        {
            //            sortQuery.ConnectBinary = binary;
            //        }
            //    }

            //    sortQuery.NotFilledJoin = notFilledJoinForSort;
            //    sortQuery.SubJoin = subJoins.LastOrDefault();
            //    sortQuery.SubSelect = subSelects.LastOrDefault();
            //    sortQuery.SelectString = _listener.SubQueryListeners[0].SubSelectFunction;

            //}



            //sortQuery.CreateQuerry();
            //CreateScheme(sortQuery);

            #endregion
            CreateScheme(sortQuery);
            return sortQuery;
        }
        
        #endregion

        #region Make-методы со string

        
        private JoinStructure[] MakeJoin(DataBaseStructure dataBase, MainListener listener, SelectStructure[] selects, string left, string right)
        {
            DataBaseStructure queryDB = CreateSubDatabase(dataBase, listener);
            List<JoinStructure> tmpJoins = new List<JoinStructure>();
            JoinStructure[] joinQueries = new JoinStructure[0];

            foreach (var binary in listener.Binaries) 
            {
                if (binary.Type == 2 && binary.ComparisionSymphol == "=")
                {
                    JoinStructure tmp = new JoinStructure(binary.LeftString, binary.RightString, binary.ComparisionSymphol, binary.SourceInterval, _sortRule, binary.IsOuterJoin);
                    tmpJoins.Add(tmp);
                }
            }

            if (tmpJoins.Count > 0)
            {
                joinQueries = FillJoins(tmpJoins.ToList(), queryDB, selects.ToList()).ToArray();

                List<JoinStructure> tmpList = new List<JoinStructure>();
                List<JoinStructure> excludedJoin = new List<JoinStructure>();
                foreach (JoinStructure join in joinQueries)
                {
                    join.CheckIsFilled();
                    if (join.IsFilled)
                    {
                        tmpList.Add(join);
                        //if (join.LeftColumn.IsPrimary == 1 || join.RightColumn.IsPrimary == 1)
                        //{
                        //    tmpList.Add(join);
                        //}
                        //else
                        //{
                        //    join.IsAdditional = true;
                        //    excludedJoin.Add(join);
                        //}
                    }



                }

                if (tmpList.Count > 0)
                {
                    List<List<JoinStructure>> shuffledJoins = ShuffleJoin(tmpList);

                    List<JoinStructure> unitedJoin = GetJoinSequence(shuffledJoins[0], listener.Depth);
                    //unitedJoin.AddRange(excludedJoin);

                    joinQueries = SortJoin(unitedJoin.ToList(), listener.Depth).ToArray();
                    foreach (var join in joinQueries)
                    {
                        join.CreateQuerry(left, right);
                    }

                    for (int i = joinQueries.Length - 1; i >= 0; i--)
                    {
                        joinQueries[i].SetIndex();
                    }

                    foreach (JoinStructure joinQuery in joinQueries)
                    {
                        joinQuery.CheckIsDistinct();
                    }

                    CreateScheme(joinQueries.ToList());
                }
                else
                {
                    joinQueries = tmpList.ToArray();
                }
            }

            return joinQueries;
        }

        private NewSortStructure MakeSort(DataBaseStructure dataBase, MainListener listener, BaseRule sortRule,  string tag)
        {
            #region OLD

            //SelectStructure[] select = selects;
            //JoinStructure[] join = joins;
            //SortStructure sortQuery = new SortStructure("So_1");
            //List<OrderByStructure> orderByStructures = listener.OrderByList;
            //List<ColumnStructure> inputColumns;

            //if (join.Length != 0)
            //{
            //    inputColumns = join.LastOrDefault().Columns;
            //}
            //else
            //{
            //    inputColumns = select.LastOrDefault().OutColumn.ToList();
            //}

            //if (orderByStructures != null)
            //    foreach (OrderByStructure orderByStructure in orderByStructures)
            //    {
            //        orderByStructure.Column =
            //            GetCorrectOrderByColumn(inputColumns, orderByStructure.ColumnName);
            //    }

            //sortQuery.Select = select.LastOrDefault();
            //sortQuery.Join = join.LastOrDefault();
            //FillAsStructures(dataBase, listener.AsList);
            //sortQuery.AsSortList = listener.AsList;
            //sortQuery.GroupByColumnList = listener.GroupByColumnsNames;
            //sortQuery.OrderByStructures = orderByStructures;

            //if (listener.SubQueryListeners.Count>0)
            //{
            //    JoinStructure notFilledJoinForSort;
            //    DataBaseStructure subDB = CreateSubDatabase(_queryDB,
            //        listener.SubQueryListeners[0].TableNames.ToArray(),
            //        listener.SubQueryListeners[0].ColumnNames.ToArray(),
            //        listener.SubQueryListeners[0].RemoveCounterColumsNames.ToArray());
            //    SelectStructure[] subSelects = MakeSelect(subDB, listener.SubQueryListeners[0]);
            //    JoinStructure[] subJoins = MakeJoin(subDB, listener.SubQueryListeners[0], subSelects, out notFilledJoinForSort);
            //    foreach (BinaryComparisionPredicateStructure binary in listener.Binaries)
            //    {
            //        if (binary.Type == 3)
            //        {
            //            sortQuery.ConnectBinary = binary;
            //        }
            //    }

            //    sortQuery.NotFilledJoin = notFilledJoinForSort;
            //    sortQuery.SubJoin = subJoins.LastOrDefault();
            //    sortQuery.SubSelect = subSelects.LastOrDefault();
            //    sortQuery.SelectString = _listener.SubQueryListeners[0].SubSelectFunction;

            //}



            //sortQuery.CreateQuerry(tag);
            //CreateScheme(sortQuery);

            #endregion
            DataBaseStructure qDB = CreateSubDatabase(dataBase, listener);
            SelectStructure[] selects = MakeSelect(qDB, listener);
            SelectStructure[] subSelects = new SelectStructure []{};
            JoinStructure[] joins = MakeJoin(qDB, listener, selects);
            JoinStructure[] subJoins = new JoinStructure[] { };
            if (_listener.SubQueryListeners.Count != 0)
            {
                DataBaseStructure subQDB = CreateSubDatabase(dataBase,listener.SubQueryListeners[0]);
                foreach (var subQlistener in _listener.SubQueryListeners)
                {
                    subSelects =
                        MakeSelect(subQDB, subQlistener);

                    subJoins = MakeJoin(subQDB, subQlistener, subSelects);
                }
            }

           
            //List<BaseRule> fromList = sortRule.GetRulesByType("tablesourcebase");
            //foreach (BaseRule rule in sortRule.GetRulesByType("tablesourcebase"))
            //{
            //    if (rule.Text != "")
            //    {
            //        rule.IsRealised = false;
            //        rule.Text = tag;
            //        rule.IsRealised = true;
            //    }
            //}
           
            NewSortStructure sortQuery = new NewSortStructure("So_1", sortRule, dataBase, tag);
            sortQuery.SetIndexes();
            CreateScheme(sortQuery);
            return sortQuery;
        }

        #endregion

        private void FillTextBoxWithHandQ(Query query)
        {
            textBox_tab2_SelectResult.Clear();
            textBox_tab2_JoinResult.Clear();
            textBox_tab2_SortResult.Clear();
            for (var index = 0; index < query.SelectQueries.Count; index++)
            {
                var selectQuery = query.SelectQueries[index];
                textBox_tab2_SelectResult.Text += "\r\n -- ======== S_" + index + "=========\r\n";
                textBox_tab2_SelectResult.Text += selectQuery.Query;
            }

            for (var index = 0; index < query.JoinQueries.Count; index++)
            {
                var joinQuery = query.JoinQueries[index];
                textBox_tab2_JoinResult.Text += "\r\n -- ======== J_" + index + "=========\r\n";
                textBox_tab2_JoinResult.Text += joinQuery.Query;
            }

            textBox_tab2_SortResult.Text += query.SortQuery.Query;
        }

        private List<Index> CreateRelationIndex(List<ColumnStructure> indexColumns, bool isLast)
        {
            List<Index> outIndex = new List<Index>();
            if (!isLast)
            {
                Index primaryIndex = new Index();
                Index anotherIndex = new Index();
                List<string> notPrimaryColumns = new List<string>();
                List<ColumnStructure> possPrimaryColumns = new List<ColumnStructure>();
                foreach (ColumnStructure column in indexColumns)
                {
                    if (column.IsPrimary == 1)
                    {
                        primaryIndex = new Index()
                        {
                            Name = "TotalyPrim_" + column.Name,
                            FieldNames = new List<string>() {column.Name},
                            IsPrimary = true
                        };
                        outIndex.Add(primaryIndex);
                    }
                    else
                    {
                        if (column.IsPrimary == 0)
                        {
                            notPrimaryColumns.Add(column.Name);
                        }
                        else
                        {
                            possPrimaryColumns.Add(column);
                        }
                    }
                }

                if (notPrimaryColumns.Count > 0)
                {
                    anotherIndex = new Index()
                    {
                        Name = "TotalyNoPrim_" + indexColumns[0].Name,
                        FieldNames = notPrimaryColumns,
                        IsPrimary = false

                    };
                    outIndex.Add(anotherIndex);
                }

                //тут будет куча проблем, потому что нужно проверять таблицы изначальные, а по ним информации нет, надо сделать привязку столбца к таблице column.TableName
                if (possPrimaryColumns.Count > 0)
                {
                    int primaryKeyCount = -1;
                    List<string> notPrimaryColumnNames = new List<string>();
                    List<string> possPrimaryIndexColumnNames = new List<string>();

                    foreach (ColumnStructure column in possPrimaryColumns)
                    {
                        if (column.IsPrimary > 1)
                        {
                            primaryKeyCount = column.IsPrimary;
                            break;
                        }
                    }

                    foreach (ColumnStructure column in possPrimaryColumns)
                    {
                        if (column.IsPrimary > 1 && primaryKeyCount > 0)
                        {
                            possPrimaryIndexColumnNames.Add(column.Name);
                            primaryKeyCount--;
                        }
                        else
                        {
                            notPrimaryColumnNames.Add(column.Name);
                        }
                    }

                    if (primaryKeyCount == 0)
                    {
                        Index possPrimaryIndex = new Index()
                        {
                            Name = "PossPrim" + possPrimaryIndexColumnNames[0],
                            FieldNames = possPrimaryIndexColumnNames,
                            IsPrimary = true
                        };
                        outIndex.Add(possPrimaryIndex);
                    }
                    else
                    {
                        Index errPossPrimaryIndex = new Index()
                        {
                            Name = "PossPrim" + possPrimaryIndexColumnNames[0],
                            FieldNames = possPrimaryIndexColumnNames,
                            IsPrimary = false
                        };
                        outIndex.Add(errPossPrimaryIndex);
                    }

                    if (notPrimaryColumnNames.Count > 0)
                    {
                        Index notPrimaryIndex = new Index()
                        {
                            Name = "PossNot" + notPrimaryColumnNames[0],
                            FieldNames = notPrimaryColumnNames,
                            IsPrimary = false
                        };
                        outIndex.Add(notPrimaryIndex);
                    }
                }

                int outIndexCount = 0;

                foreach (Index index in outIndex)
                {
                    foreach (string name in index.FieldNames)
                    {
                        outIndexCount++;
                    }
                }

                if (indexColumns.Count != outIndexCount)
                {
                    MessageBox.Show("ERROR in INDEX_COUNT");
                }

                return outIndex;
            }
            else
            {
                List<string> fields = new List<string>();
                foreach (ColumnStructure column in indexColumns)
                {
                    fields.Add(column.Name);
                }
                outIndex.Add(new Index()
                    {
                        Name = "LastIndex",
                        FieldNames =  fields,
                        IsPrimary = false
                    }
            );
                return outIndex;
            }
        }

        #endregion

        #region FormMethods

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            ReSize();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox_tab2_Query.Text = textBox_tab1_Query.Text;
            FillScheme();
            ReSize();
            _toDoTextFlag = true;
            StreamReader sr = new StreamReader(@"res\ToDo.txt", System.Text.Encoding.Default);
            textBox4.Text = sr.ReadToEnd();
            sr.Close();
            _toDoTextFlag = false;
            using (FileStream fs = new FileStream(@"res\db_result.xml", FileMode.Create, FileAccess.ReadWrite))
            {
                XmlSerializer dbSerializer = new XmlSerializer(typeof(DataBaseStructure));
                dbSerializer.Serialize(fs, _dbName);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox_tab2_Query.Text = textBox_tab1_Query.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox_tab1_Query.Text = textBox_tab2_Query.Text;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (!_toDoTextFlag)
            {
                using (StreamWriter sw = new StreamWriter(@"res\ToDo.txt", false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(textBox4.Text);
                    sw.Close();
                }
            }
        }

        private void checkBox_tab2_DisableHeavyQuerry_CheckedChanged(object sender, EventArgs e)
        {
            checkBox_tab1_DisableHeavyQuerry.CheckState = checkBox_tab2_DisableHeavyQuerry.CheckState;
        }
        
        private void allow_SelectAl(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                if (sender != null)
                    ((TextBox)sender).SelectAll();
            }
        }

        private void ReSize()
        {
            textBox_tab2_Query.Width = textBox_tab2_SelectResult.Width = textBox_tab2_JoinResult.Width = textBox_tab2_SortResult.Width = textBox_tab2_AllResult.Width = (tabPage2.Width - 100) / 5;
            textBox_tab2_SelectResult.Location = new Point(textBox_tab2_Query.Location.X + 40 + textBox_tab2_Query.Width, textBox_tab2_SelectResult.Location.Y);
            textBox_tab2_JoinResult.Location = new Point(textBox_tab2_SelectResult.Location.X + 20 + textBox_tab2_SelectResult.Width, textBox_tab2_SelectResult.Location.Y);
            textBox_tab2_SortResult.Location = new Point(textBox_tab2_JoinResult.Location.X + 20 + textBox_tab2_SelectResult.Width, textBox_tab2_SelectResult.Location.Y);
            textBox_tab2_AllResult.Location = new Point(textBox_tab2_SortResult.Location.X + 20 + textBox_tab2_SelectResult.Width, textBox_tab2_SelectResult.Location.Y);
        }

        private void comboBox_tab2_IP_TextChanged(object sender, EventArgs e)
        {
            _connectionIP = comboBox_tab2_IP.Text;
        }

        #endregion
        
        #region TAB_1

        private void btn_CreateTree_Click(object sender, EventArgs e)
        {
            //Кнопка для картинки(построить дерево)
            GetTree();
            richTextBox_tab1_Query.Visible = false;
            Image image = _vTree.Draw();
            pictureBox_tab1_Tree.Image = image;

        }

        private void btn_SaveTree_Click(object sender, EventArgs e)
        {
            //кнопка для сохранения картинки
            saveFileDialog1.Filter = "Images|*.png;*.bmp;*.jpg";
            ImageFormat format = ImageFormat.Png;
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName);
                switch (ext)
                {
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                }

                pictureBox_tab1_Tree.Image.Save(saveFileDialog1.FileName, format);
            }
        }

        private void btn_SelectQuerry_tab1_Click(object sender, EventArgs e)
        {
            //выбрать запрос
            pictureBox_tab1_Tree.Visible = true;
            richTextBox_tab1_Query.Visible = false;
            textBox_tab1_Query.Width = 283;
            richTextBox_tab1_Query.Text = "ЗАПРОС " + comboBox_tab1_QueryNumber.Text + Environment.NewLine;
            //запросы TPC-H (убраны Date)
            if (!checkBox_tab1_DisableHeavyQuerry.Checked)
            {
                textBox_tab1_Query.Text = GetQuery(Convert.ToInt16(comboBox_tab1_QueryNumber.Text), checkBox_tab1_DisableHeavyQuerry.Checked, comboBox_tab1_QueryNumber);
                if (Convert.ToInt16(comboBox_tab1_QueryNumber.Text) < 14)
                {
                    comboBox_tab1_QueryNumber.Text = (Convert.ToInt16(comboBox_tab1_QueryNumber.Text) + 1).ToString();
                }
                else
                {
                    comboBox_tab1_QueryNumber.Text = "0";
                }
            }
            else
            {
                textBox_tab1_Query.Text = GetQuery(Convert.ToInt16(comboBox_tab1_QueryNumber.Text), checkBox_tab1_DisableHeavyQuerry.Checked, comboBox_tab1_QueryNumber);
                
                if (Convert.ToInt16(comboBox_tab1_QueryNumber.Text) == 14)
                {
                    comboBox_tab1_QueryNumber.Text = "0";
                }
            }

        }

        private void btn_Debug_Click(object sender, EventArgs e)
        {
            Console.Clear();
            //GetQuerryTreesScreens(@"D:\!Studing\Скриншоты деревьев\Cцифрами\",1,14);
            //GetQuerryTreesScreens(@"E:\Магистратура\!MainProject\Скриншоты деревьев\Сцифрами\", 1,14);
            //отладка
            pictureBox_tab1_Tree.Visible = false;
            richTextBox_tab1_Query.Visible = true;
            richTextBox_tab1_Query.Text = textBox_tab1_Query.Text;
            textBox_tab1_Query.Width = Width - richTextBox_tab1_Query.Width - 50;
            richTextBox_tab1_Query.Height = textBox_tab1_Query.Height;
            richTextBox_tab1_Query.Location = new Point(textBox_tab1_Query.Location.X + textBox_tab1_Query.Width + 10,
                textBox_tab1_Query.Location.Y);
            GetTree();
            _output = "";
            _output += "\r\n========Return================\r\n";
            
            

            textBox_tab1_Query.Text = _output;
        }
        
        #endregion

        #region TAB_2

        private void btn_CreateSelect_Click(object sender, EventArgs e)
        {
            //составление запросов SELECT
            
            textBox_tab2_SelectResult.Clear();
            GetTree( textBox_tab2_Query.Text);
            _selectQuery = MakeSelect(_dbName, _listener);
            for (int i = 0; i < _selectQuery.Length; i++)
            {
                textBox_tab2_SelectResult.Text += "\r\n=======" + _selectQuery[i].Name + "=========\r\n";
                textBox_tab2_SelectResult.Text += _selectQuery[i].Output + "\r\n";
            }

            if (_listener.SubQueryListeners.Count != 0)
            {
                textBox_tab2_SelectResult.Text += "\r\n========SUB_Q==========================\r\n";
                foreach (var subQlistener in _listener.SubQueryListeners)
                {
                    _subSelectQuery =
                        MakeSelect(_dbName, subQlistener);
                }

                foreach (SelectStructure subSelect in _subSelectQuery)
                {
                    textBox_tab2_SelectResult.Text += "\r\n========" + subSelect.Name + "=========\r\n";
                    textBox_tab2_SelectResult.Text += subSelect.Output + "\r\n";
                }
            }
        }

        private void btn_CreateJoin_Click(object sender, EventArgs e)
        {
            GetTree(textBox_tab2_Query.Text);
            _joinQuery =
                MakeJoin(_dbName, _listener, MakeSelect(_dbName, _listener)).ToList();
            List<JoinStructure> subJoin = new List<JoinStructure>();
            textBox_tab2_JoinResult.Clear();
            foreach (var join in _joinQuery)
            {
                textBox_tab2_JoinResult.Text += "\r\n========" + join.Name + "========\r\n" + join.Output + "\r\n";
            }
            if (_listener.SubQueryListeners.Count != 0)
            {
                textBox_tab2_JoinResult.Text += "\r\n========SUB_Q==========================\r\n";
                foreach (var subQlistener in _listener.SubQueryListeners)
                {
                    subJoin = MakeJoin(_dbName, subQlistener, MakeSelect(_dbName, subQlistener)).ToList();
                }

                foreach (var join in subJoin)
                {
                    textBox_tab2_JoinResult.Text += "\r\n========" + join.Name + "========\r\n" + join.Output + "\r\n";
                }
            }
        }
        
        private void btn_CreateSort_Click(object sender, EventArgs e)
        {
            GetTree(textBox_tab2_Query.Text);
            NewSortStructure sortQ = MakeSort(_dbName, _listener, _sortRule);
            textBox_tab2_SortResult.Text = sortQ.Output;
        }
        
        private void btn_CreateTest_Click(object sender, EventArgs e)
        {
            GetTree(textBox_tab2_Query.Text);
            SelectStructure[] selectQ, subSelectQ;
            JoinStructure[] joinQ, subJoinQ;
            NewSortStructure sortQ;
            if (_connectionIP == null)
            {
                _connectionIP = comboBox_tab2_IP.Text;
            }
            if (checkBox_Tab2_ClusterXNEnable.Checked)
            {
                selectQ = MakeSelect(_dbName, _listener);
                joinQ = MakeJoin(_dbName, _listener, selectQ, Constants.LeftRelationNameTag,
                    Constants.RightRelationNameTag);

                if (_listener.SubQueryListeners.Count > 0)
                {
                    subSelectQ = MakeSelect(_dbName, _listener.SubQueryListeners[0]); //Добавить foreach
                        subJoinQ = MakeJoin(_dbName, _listener.SubQueryListeners[0], subSelectQ, Constants.LeftRelationNameTag, Constants.RightRelationNameTag);
                }
                else
                {
                    subSelectQ = null;
                    subJoinQ = null;
                }

                sortQ = MakeSort(_dbName, _listener, _sortRule, Constants.RelationNameTag);
            }
            else
            {
                selectQ = MakeSelect(_dbName, _listener);
                joinQ = MakeJoin(_dbName, _listener, selectQ);
                if (_listener.SubQueryListeners.Count > 0)
                {
                    subSelectQ = MakeSelect(_dbName, _listener.SubQueryListeners[0]); //Добавить foreach
                    subJoinQ = MakeJoin(_dbName, _listener.SubQueryListeners[0], subSelectQ);
                    sortQ = MakeSort(_dbName, _listener, _sortRule);
                }
                else
                {
                    sortQ = MakeSort(_dbName, _listener, _sortRule);
                    subSelectQ = null;
                    subJoinQ = null;
                }
            }

            string dropSyntax = Environment.NewLine + "DROP TABLE {0};" + Environment.NewLine;
            string createTableSyntax = "CREATE TABLE {0} (\r\n{1} {2} ) ENGINE=MEMORY\r\n\r\n";
            string createIndexSyntax = ",\r\n INDEX {0} ( {2} ) \r\n\r\n";
            string querSyntax = "{0}\r\n";
            var dropBuilder = new StringBuilder();
            var testQuery = new StringBuilder();

            foreach (var select in selectQ)
            {
                testQuery.Append("\r\n -- ========" + select.Name + "=========\r\n");
                //testQuery.Append(string.Format(dropSyntax, select.Name));

                string index = "";
                if (select.IndexColumnNames.Count > 0)
                {
                    foreach (string joinIndexColumnName in select.IndexColumnNames)
                    {
                        if (joinIndexColumnName != select.IndexColumnNames.Last())
                        {
                            index += joinIndexColumnName + ", ";
                        }
                        else
                        {
                            index += joinIndexColumnName;
                        }
                    }
                }

                testQuery.Append(string.Format(createTableSyntax, select.Name, select.CreateTableColumnNames,
                    select.IndexColumnNames.Count > 0 ? 
                    string.Format(createIndexSyntax, select.Name + "_INDEX", select.Name, index)
                    : ""));
                testQuery.Append(string.Format(querSyntax, select.Output));
                //testQuery.Append(string.Format(createIndexSyntax, select.Name + "_INDEX", select.Name, select.IndexColumnName));
                dropBuilder.Append(string.Format(dropSyntax, select.Name));
            }
            foreach (var join in joinQ)
            {
                testQuery.Append("\r\n -- ========" + join.Name + "=========\r\n");
               // testQuery.Append(string.Format(dropSyntax, join.Name));
               string index = "";
               if (join.IndexColumnNames.Count > 0)
               {
                   foreach (string joinIndexColumnName in join.IndexColumnNames)
                   {
                       if (joinIndexColumnName != join.IndexColumnNames.Last())
                       {
                           index += joinIndexColumnName + ", ";
                       }
                       else
                       {
                           index += joinIndexColumnName;
                       }
                   }
               }

               testQuery.Append(string.Format(createTableSyntax, join.Name, join.CreateTableColumnNames,
                   join.IndexColumnNames.Count > 0 ?
                   string.Format(createIndexSyntax, join.Name + "_INDEX", join.Name,
                       index)
                   :""));
                testQuery.Append(string.Format(querSyntax, join.Output));

                dropBuilder.Append(string.Format(dropSyntax, join.Name));
            }
            
            if (_listener.SubQueryListeners.Count > 0)
            {
                foreach (var select in subSelectQ)
                {
                    testQuery.Append("\r\n -- ========" + select.Name + "=========\r\n");
                    //testQuery.Append(string.Format(dropSyntax, select.Name));
                    string index = "";
                    if (select.IndexColumnNames.Count > 0)
                    {
                        foreach (string joinIndexColumnName in select.IndexColumnNames)
                        {
                            if (joinIndexColumnName != select.IndexColumnNames.Last())
                            {
                                index += joinIndexColumnName + ", ";
                            }
                            else
                            {
                                index += joinIndexColumnName;
                            }
                        }
                    }

                    testQuery.Append(string.Format(createTableSyntax, select.Name, select.CreateTableColumnNames,
                        select.IndexColumnNames.Count > 0 ? 
                        string.Format(createIndexSyntax, select.Name + "_INDEX", select.Name, index)
                        : ""));
                    testQuery.Append(string.Format(querSyntax, select.Output));
                    //testQuery.Append(string.Format(createIndexSyntax, select.Name + "_INDEX", select.Name, select.IndexColumnName));
                    dropBuilder.Append(string.Format(dropSyntax, select.Name));
                }
                foreach (var join in subJoinQ)
                {
                    testQuery.Append("\r\n -- ========" + join.Name + "=========\r\n");
                    // testQuery.Append(string.Format(dropSyntax, join.Name));
                    string index = "";
                    if (join.IndexColumnNames.Count > 0)
                    {
                        foreach (string joinIndexColumnName in join.IndexColumnNames)
                        {
                            if (joinIndexColumnName != join.IndexColumnNames.Last())
                            {
                                index += joinIndexColumnName + ", ";
                            }
                            else
                            {
                                index += joinIndexColumnName;
                            }
                        }
                    }
                    testQuery.Append(string.Format(createTableSyntax, join.Name, join.CreateTableColumnNames,
                        string.Format(createIndexSyntax, join.Name + "_INDEX", join.Name,
                                index)));
                    testQuery.Append(string.Format(querSyntax, join.Output));

                    dropBuilder.Append(string.Format(dropSyntax, join.Name));
                }
            }
            
            testQuery.Append("\r\n -- ========" + sortQ.Name + "=========\r\n");
            //testQuery.Append(string.Format(dropSyntax, sortQ.Name));
            testQuery.Append(string.Format(createTableSyntax, sortQ.Name, sortQ.CreateTableColumnNames, ""));
            testQuery.Append(string.Format(querSyntax, sortQ.Output));
            dropBuilder.Append(string.Format(dropSyntax, sortQ.Name));

            textBox_tab2_AllResult.Text = testQuery.ToString();
            textBox_tab2_AllResult.Text += "SELECT * FROM So_1;";
            textBox_tab2_AllResult.Text += dropBuilder.ToString();

            JoinStructure[] connectJoins;
            if (_listener.SubQueryListeners.Count > 0)
            {
                connectJoins = new JoinStructure[subJoinQ.Length + joinQ.Length];
                for (int i = 0; i < subJoinQ.Length; i++)
                {
                    connectJoins[i] = subJoinQ[i];
                }

                for (int i = subJoinQ.Length, j = 0; i < joinQ.Length + subJoinQ.Length; i++, j++)
                {
                    connectJoins[i] = joinQ[j];
                }
            }
            else
            {
                connectJoins = new JoinStructure[joinQ.Length];
                for (int i = 0; i < joinQ.Length; i++)
                {
                    connectJoins[i] = joinQ[i];
                }
            }

            if (checkBox_Tab2_ClusterXNEnable.Checked)
            {
                if (connectJoins.Length > 0)
                {
                    if (subJoinQ != null)
                    {
                        TryConnect(connectJoins, sortQ, subJoinQ.Length - 1, _connectionIP);
                    }
                    else
                    {
                        TryConnect(connectJoins, sortQ, -1, _connectionIP);
                    }
                }
                else
                {
                    List<SelectStructure> cSelects = new List<SelectStructure>();
                    
                    cSelects.AddRange(selectQ);
                    if (subSelectQ != null)
                    {
                        cSelects.AddRange(subSelectQ);
                    }


                    TryConnect(cSelects.ToArray(), sortQ, _connectionIP);
                }
            }
        }

        private void btn_SelectQuerry_tab2_Click(object sender, EventArgs e)
        {
            //Выбрать запрос на 2й вкладке
            
            if (!checkBox_tab2_DisableHeavyQuerry.Checked)
            {
                textBox_tab2_Query.Text = GetQuery(Convert.ToInt16(comboBox_tab2_QueryNumber.Text), checkBox_tab2_DisableHeavyQuerry.Checked, comboBox_tab2_QueryNumber);
                if (Convert.ToInt16(comboBox_tab2_QueryNumber.Text) < 14)
                {
                    comboBox_tab2_QueryNumber.Text = (Convert.ToInt16(comboBox_tab2_QueryNumber.Text) + 1).ToString();
                }
                else
                {
                    comboBox_tab2_QueryNumber.Text = "0";
                }
            }
            else
            {
                textBox_tab2_Query.Text = GetQuery(Convert.ToInt16(comboBox_tab2_QueryNumber.Text), checkBox_tab2_DisableHeavyQuerry.Checked, comboBox_tab2_QueryNumber);
                
                if (Convert.ToInt16(comboBox_tab2_QueryNumber.Text) == 14)
                {
                    comboBox_tab2_QueryNumber.Text = "0";
                }
            }
        }

        #endregion

        #region TAB_4

        private void btn_tab4_SendToClusterix_Click(object sender, EventArgs e)
        {
            var clinet = new ClusterixClient(comboBox_tab4_connetionIP.Text, 1234); //10.114.20.200"
            clinet.Send(new XmlQueryPacket() { XmlQuery = richTextBox_tab4_XML.Text });
        }

        #endregion

        #region Актуальные методы(в разработке)

        private void TryConnect(JoinStructure[] joinQ, NewSortStructure sortQ, int subJoinIndex, string address)
        {
            int qNumber = 1;
            if (comboBox_tab2_QueryNumber.Text != "0"  )
            {
                qNumber = int.Parse(comboBox_tab2_QueryNumber.Text) - 1;
            }
            else
            {
                qNumber = 14;
            }
            QueryBuilder qb = new QueryBuilder(qNumber);
            bool isNoMainjoin = joinQ.Length == subJoinIndex + 1;

            var c_join = new JoinQuery[joinQ.Length];

            for (var index = 0; index < joinQ.Length; index++)
            {
                var select = new SelectQuery();
                if (!joinQ[index].Switched)
                {
                    if (joinQ[index].RightSelect != null)
                    {
                        select = qb.CreateSelectQuery(joinQ[index].RightSelect.Output, 0);
                    }
                    else
                    {
                       // select.;

                    }
                }
                else
                {
                    select = qb.CreateSelectQuery(joinQ[index].LeftSelect.Output, 0);
                }

                qb.AddSelectQuery(select);

                var leftRelation = new Relation();
                if (joinQ[index].IsFirst)
                {
                    var select2 = qb.CreateSelectQuery(joinQ[index].LeftSelect.Output, 0);
                    qb.AddSelectQuery(select2);

                    leftRelation = qb.CreateRelation(
                        select2,"a", //joinQ[index].LeftSelect.Name,
                        qb.CreateRelationSchema(joinQ[index].LeftSelect.OutTable.Columns
                                .Select(j => new Field() {Name = j.Name, Params = j.Type.Name})
                                .ToList(), joinQ[index].LeftSelect.IndexColumnNames.Count > 0 ?
                            joinQ[index] != joinQ.Last() ?
                            //new List<Index>()
                            //{
                            //    new Index()
                            //    {
                            //        FieldNames = joinQ[index].LeftSelect.IndexColumnNames,
                            //        Name = $"INDEX_{joinQ[index].LeftSelect.Name}"
                            //    }
                            //}
                            CreateRelationIndex(joinQ[index].LeftSelect.IndexColumns, false)
                                : CreateRelationIndex(joinQ[index].LeftSelect.IndexColumns, true)
                            : new List<Index>()));

                }
                else
                {
                    leftRelation = qb.CreateRelation(c_join[index - 1]);
                }
                var rightRelation = new Relation();
                if (!joinQ[index].Switched)
                {
                    if (joinQ[index].RightSelect != null)
                    {
                        rightRelation =
                            qb.CreateRelation(
                                select, "b", //joinQ[index].RightSelect.Name,
                                qb.CreateRelationSchema(joinQ[index].RightSelect.OutTable.Columns
                                    .Select(j => new Field() {Name = j.Name, Params = j.Type.Name})
                                    .ToList(), joinQ[index].RightSelect.IndexColumnNames.Count > 0 ? 
                                    joinQ[index] != joinQ.Last( )?
                                    //new List<Index>()
                                    //{
                                    //    new Index()
                                    //    {
                                    //        FieldNames = joinQ[index].RightSelect.IndexColumnNames,
                                    //        Name = $"INDEX_{joinQ[index].RightSelect.Name}"
                                    //    }
                                    //}
                                    CreateRelationIndex(joinQ[index].RightSelect.IndexColumns, false) 
                                        : CreateRelationIndex(joinQ[index].RightSelect.IndexColumns, true)
                                    : new List<Index>()));
                    }
                    else
                    {
                        rightRelation.IsEmpty = true;
                        rightRelation = qb.CreateEmptyRelation();
                    }
                }
                else
                {
                    rightRelation =
                        qb.CreateRelation(
                            select, "c", //joinQ[index].LeftSelect.Name,
                            qb.CreateRelationSchema(joinQ[index].LeftSelect.OutTable.Columns
                                    .Select(j => new Field() {Name = j.Name, Params = j.Type.Name})
                                    .ToList(), joinQ[index].LeftSelect.IndexColumnNames.Count > 0 ?
                                joinQ[index] != joinQ.Last() ?
                                //new List<Index>()
                                //{
                                //    new Index()
                                //    {
                                //        FieldNames = joinQ[index].LeftSelect.IndexColumnNames,
                                //        Name = $"INDEX_{joinQ[index].LeftSelect.Name}"
                                //    }
                                //}
                                //
                                
                                CreateRelationIndex(joinQ[index].LeftSelect.IndexColumns, false)
                                    : CreateRelationIndex(joinQ[index].LeftSelect.IndexColumns, true)
                                : new List<Index>()));
                }
                

                c_join[index] = qb.AddJoinQuery(
                    qb.CreateJoinQuery(joinQ[index].Output,
                        qb.CreateRelationSchema(
                            joinQ[index].OutTable.Columns
                                .Select(j => new Field() {Name = j.Name, Params = j.Type.Name}).ToList(),
                            joinQ[index].IndexColumnNames.Count > 0 ?
                                joinQ[index] != joinQ.Last() ?
                                //new List<Index>()
                                //{
                                //    new Index()
                                //    {
                                //        FieldNames = joinQ[index].IndexColumnNames,
                                //        Name = $"INDEX_{joinQ[index].IndexColumnNames[0]}"
                                //    }
                                //}
                                CreateRelationIndex(joinQ[index].IndexColumn, false)
                                    :CreateRelationIndex(joinQ[index].IndexColumn, true)
                                : new List<Index>()), 0,
                        leftRelation, rightRelation));
            }

            string resultSelect = "SELECT ";
            if (sortQ.OutDataBase.Tables[0].Columns.Length > 1)
            {
                foreach (ColumnStructure column in sortQ.OutDataBase.Tables[0].Columns)
                {
                    if (column != sortQ.OutDataBase.Tables[0].Columns.Last())
                    {
                        resultSelect += column.Name + ", ";
                    }
                    else
                    {
                        resultSelect += column.Name;
                    }
                }
            }
            else
            {
                resultSelect += sortQ.OutDataBase.Tables[0].Columns[0].Name;
            }

            resultSelect += " FROM " + Constants.RelationNameTag + ";";
            //resultSelect = "SELECT * FROM " + Constants.RelationNameTag + ";";
            if (subJoinIndex > -1) //ПАЧИНИТЬ
            {

                if (!isNoMainjoin)
                {
                    
                    qb.SetSortQuery(qb.CreateSortQuery(sortQ.Output, qb.CreateRelationSchema(sortQ.OutDataBase.Tables[0]
                                .Columns
                                .Select(j => new Field() {Name = j.Name, Params = j.Type.Name})
                                .ToList(),
                            new List<Index>()
                            {
                            }), 0, resultSelect, qb.CreateRelation(c_join.Last()),
                        qb.CreateRelation(c_join[subJoinIndex])));
                }
                else
                {
                    qb.SetSortQuery(qb.CreateSortQuery(sortQ.Output, qb.CreateRelationSchema(sortQ.OutDataBase.Tables[0]
                            .Columns
                            .Select(j => new Field() { Name = j.Name, Params = j.Type.Name })
                            .ToList(),
                        new List<Index>()
                        {
                        }), 0, resultSelect , qb.CreateRelation(c_join[subJoinIndex])));
                }
            }
            else
            {
                qb.SetSortQuery(qb.CreateSortQuery(sortQ.Output, qb.CreateRelationSchema(sortQ.OutDataBase.Tables[0].Columns
                        .Select(j => new Field() {Name = j.Name, Params = j.Type.Name})
                        .ToList(),
                    new List<Index>()
                    {
                    }), 0, resultSelect, qb.CreateRelation(c_join.Last())));
            
            }


            var query = qb.GetQuery();
            query.Save(query.Number + ".xml");

            #region Вывод ручных запросов

            FillTextBoxWithHandQ(query);

            #endregion

            if (checkBox_Tab2_ClusterixN_Online.Checked)
            {
                var clinet = new ClusterixClient(address, 1234); //10.114.20.200"
                clinet.Send(new XmlQueryPacket() {XmlQuery = query.SaveToString()});
            }


            string debug = "==========DEBUG==========";
            foreach (JoinQuery joinQuery in c_join)
            {
                debug += Environment.NewLine + joinQuery.QueryId;
                debug += Environment.NewLine + "\t" + joinQuery.LeftRelation.Name + " = " +
                         joinQuery.RightRelation.Name;

            }
            
            Console.WriteLine(debug);
        }

        private void TryConnect(SelectStructure[] selectQ, NewSortStructure sortQ, string address)
        {
            QueryBuilder qb = new QueryBuilder(int.Parse(comboBox_tab2_QueryNumber.Text) - 1);
            List<Relation> relations = new List<Relation>();
            foreach (SelectStructure select in selectQ)
            {
                var cSelect = new SelectQuery();
                cSelect = qb.CreateSelectQuery(select.Output, 0);
                qb.AddSelectQuery(cSelect);

                var cRelation = qb.CreateRelation(cSelect, select.Name,
                    qb.CreateRelationSchema(
                        select.OutColumn.Select(j => new Field() {Name = j.Name, Params = j.Type.Name}).ToList(),
                        select.IndexColumnNames.Count > 0 ?
                            select != selectQ.Last() ?
                                CreateRelationIndex(select.IndexColumns, false)
                            : CreateRelationIndex(select.IndexColumns, true) 
                        : new List<Index>()
                    ));

                relations.Add(cRelation);
            }

            string resultSelect = "SELECT ";
            if (sortQ.OutDataBase.Tables[0].Columns.Length > 1)
            {
                foreach (ColumnStructure column in sortQ.OutDataBase.Tables[0].Columns)
                {
                    if (column != sortQ.OutDataBase.Tables[0].Columns.Last())
                    {
                        resultSelect += column.Name + ", ";
                    }
                    else
                    {
                        resultSelect += column.Name;
                    }
                }
            }
            else
            {
                resultSelect += sortQ.OutDataBase.Tables[0].Columns[0].Name;
            }

            resultSelect += " FROM " + Constants.RelationNameTag + ";";
            //resultSelect = "SELECT * FROM " + Constants.RelationNameTag + ";";

            qb.SetSortQuery(qb.CreateSortQuery(sortQ.Output, qb.CreateRelationSchema(sortQ.OutDataBase.Tables[0]
                        .Columns
                        .Select(j => new Field() { Name = j.Name, Params = j.Type.Name })
                        .ToList(),
                    new List<Index>()
                    {
                    }), 0, resultSelect, relations.ToArray() ));

            //qb.SetSortQuery(qb.CreateSortQuery(sortQ.Output, qb.CreateRelationSchema(sortQ.OutDataBase.Tables[0]
            //            .Columns
            //            .Select(j => new Field() { Name = j.Name, Params = j.Type.Name })
            //            .ToList(),
            //        new List<Index>()
            //        {
            //        }), 0, "SELECT * FROM " + Constants.RelationNameTag + ";", qb.CreateRelation(c_join.Last()),
            //    qb.CreateRelation(c_join[subJoinIndex])));

            var query = qb.GetQuery();
            query.Save(query.Number + ".xml");
            #region Вывод ручных запросов

            FillTextBoxWithHandQ(query);

            #endregion
            if (checkBox_Tab2_ClusterixN_Online.Checked)
            {
                var clinet = new ClusterixClient(address, 1234); //10.114.20.200"
                clinet.Send(new XmlQueryPacket() { XmlQuery = query.SaveToString() });
            }
        }

        private JoinStructure[] TransformToJoin(List<JoinStructure> inputJoins)
        {
            JoinStructure[] outputJoins = new JoinStructure[inputJoins.Count];


            return outputJoins;
        }



        #endregion

        
    }
}