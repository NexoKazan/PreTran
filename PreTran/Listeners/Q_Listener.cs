using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PreTran.Q_Part_Structures;

namespace PreTran.Listeners
{
    class Q_Listener : MySqlParserBaseListener
    {
        
        
        private int _depth;
        private int _tmpDepth;
        private int _id;
        private bool _triggerEnterSelectFunctionElemenAsExist = false;
        private bool _isFirst = true;
        public string _return = "Return:\r\n";
        private string _subSelectFunction;
        private IVocabulary _vocabulary;
        private List<string> _columnNames = new List<string>();
        private List<string> _tableNames = new List<string>();
        private List<string> _selectColumnNames = new List<string>();
        private List<string> _groupByColumnsNames = new List<string>();
        private List<LikeStructure> _likeList = new List<LikeStructure>();
        private List<AsStructure> _asList = new List<AsStructure>();
        private List<OrderByStructure> _orderByList = new List<OrderByStructure>();   
        private List<MainListener> _subQueryListeners = new List<MainListener>();
        private List<BaseRule> _baseRules = new List<BaseRule>();
        private List<BinaryComparisionPredicateStructure> _binaries = new List<BinaryComparisionPredicateStructure>();
       
        public Q_Listener(int depth)
        {
            _tmpDepth = depth;
            _depth = depth;
        }

        #region Свойства

        public int Depth
        {
            get { return _depth; }
        }

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }
        
        public IVocabulary Vocabulary
        {
            get { return _vocabulary; }
            set { _vocabulary = value; }
        }

        public List<string> ColumnNames
        {
            get { return _columnNames; }
        }

        public List<string> TableNames
        {
            get { return _tableNames; }
        }

        public List<string> SelectColumnNames
        {
            get { return _selectColumnNames; }
        }

        public List<BinaryComparisionPredicateStructure> Binaries
        {
            get { return _binaries; }
        }

        public List<string> GroupByColumnsNames
        {
            get { return _groupByColumnsNames; }
            set { _groupByColumnsNames = value; }
        }

        public List<AsStructure> AsList
        {
            get { return _asList; }
        }

        public List<OrderByStructure> OrderByList
        {
            get { return _orderByList; }
        }

        public List<MainListener> SubQueryListeners
        {
            get { return _subQueryListeners; }
        }

        public List<LikeStructure> LkeList
        {
            get { return _likeList; }
        }
        
        public string SubSelectFunction
        {
            get { return _subSelectFunction; }
        }

        internal List<BaseRule> BaseRules { get => _baseRules; set => _baseRules = value; }

        #endregion

        public override void EnterFullColumnName([NotNull] MySqlParser.FullColumnNameContext context)
        {
            _columnNames.Add(context.GetText());
        }

        public override void EnterTableName([NotNull] MySqlParser.TableNameContext context)
        {
            _tableNames.Add(context.GetText());
        }

        public override void EnterSelectColumnElement([NotNull] MySqlParser.SelectColumnElementContext context)
        {
            if(_depth == _tmpDepth)
                _selectColumnNames.Add(context.GetText());
        }
        
        public override void EnterTableSourceBase([NotNull] MySqlParser.TableSourceBaseContext context)
        { 
            if(_depth == _tmpDepth)
            TableNames.Add(context.GetText());
            
        }

        public override void EnterSelectFunctionElement([NotNull] MySqlParser.SelectFunctionElementContext context)
        {
            if (_depth == _tmpDepth)
            {
                if (context.AS() != null)
                {
                    AsListener asl = new AsListener();
                    ParseTreeWalker wlk = new ParseTreeWalker();
                    wlk.Walk(asl, context);
                    AsList.Add(new AsStructure(asl.AsColumnList, asl._output, asl._functionOutput,
                        context.uid().GetText(), asl._functionName, context.SourceInterval));
                }
                else
                {
                    _subSelectFunction = context.GetText();
                }
            }
           
        }

        public override void EnterBinaryComparasionPredicate([NotNull] MySqlParser.BinaryComparasionPredicateContext context)
        {
            //Console.WriteLine(ruleNames[context.RuleIndex]);
            if (_depth == _tmpDepth)
            {
                BinaryComparisionPredicateStructure tmpBinary = new BinaryComparisionPredicateStructure(context.left.GetText(), context.comparisonOperator().GetText(), context.right.GetText(), context.SourceInterval);
                if (context.GetChild(2).GetChild(0).GetType().ToString().Contains("ConstantExpressionAtomContext") || context.GetChild(2).GetChild(0).GetType().ToString().Contains("MathExpressionAtomContext"))
                {
                    tmpBinary.Type = (int)PredicateType.simple;}

                if (context.GetChild(2).GetChild(0).GetType().ToString()
                    .Contains("FullColumnNameExpressionAtomContext"))
                {
                    tmpBinary.Type = 2;
                }

                if (context.GetChild(2).GetChild(0).GetType().ToString().Contains("SubqueryExpessionAtomContext"))
                {
                    tmpBinary.Type = 3;
                    _id++;
                    tmpBinary.SubQid = _id;

                }
                _binaries.Add(tmpBinary);
            }
            
        }
        
        public override void EnterGroupByItem([NotNull] MySqlParser.GroupByItemContext context)
        {
            if (_depth == _tmpDepth)
            {
                GroupByColumnsNames.Add(context.GetText());
            }
        }

        public override void EnterOrderByExpression([NotNull] MySqlParser.OrderByExpressionContext context)
        {
            if (_depth == _tmpDepth)
            {
                OrderByStructure tmpOrder = new OrderByStructure();
                tmpOrder.ColumnName = context.expression().GetText();
                if (context.order != null)
                {
                    if (context.order.Text == "DESC")
                    {
                        tmpOrder.IsDESC = true;
                    }
                }

                OrderByList.Add(tmpOrder);
            }
            
        }

        //public override void EnterSubqueryExpessionAtom([NotNull] MySqlParser.SubqueryExpessionAtomContext context)
        //{
        //    _depth++;
        //    MainListener tmpSubListener = new MainListener(_depth);
        //    tmpSubListener.ID = _id;
        //    ParseTreeWalker walker = new ParseTreeWalker();
        //    walker.Walk(tmpSubListener, context.selectStatement());
        //    SubQueryListeners.Add(tmpSubListener);
        //}

        //public override void ExitSubqueryExpessionAtom([NotNull] MySqlParser.SubqueryExpessionAtomContext context)
        //{
        //    _depth--;
        //}

        public override void EnterQuerySpecification(MySqlParser.QuerySpecificationContext context)
        {
            if (!_isFirst)
            {
                _depth++;
                MainListener tmpSubListener = new MainListener(_depth);
                tmpSubListener.ID = _id;
                ParseTreeWalker walker = new ParseTreeWalker();
                walker.Walk(tmpSubListener, context.Payload);
                SubQueryListeners.Add(tmpSubListener);
            }
            else
            {
                _isFirst = false;
            }
        }

        public override void ExitQuerySpecification(MySqlParser.QuerySpecificationContext context)
        {
            if (!_isFirst)
            {
                _isFirst = true;
            }
        }

        public override void EnterLikePredicate([NotNull] MySqlParser.LikePredicateContext context)
        {
            if (_depth == _tmpDepth)
            {
                LikeStructure tmpLike = new LikeStructure(context.Stop.Text, context.Start.Text, context.SourceInterval);
                if (context.NOT()!=null)
                {
                    tmpLike.IsNot = true;
                }
                _likeList.Add(tmpLike);
            }
           
        }
    }
}
