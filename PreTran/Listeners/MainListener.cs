#region Copyright
/*
 * Copyright 2019 Igor Kazantsev
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy
 * of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PreTran.DataBaseSchemeStructure;
using PreTran.Q_Part_Structures;
using PreTran.TestClasses;
using PreTran.TestClasses.Listeners;
using PreTran.TestClasses.Rules;

namespace PreTran.Listeners
{
    class MainListener : MySqlParserBaseListener
    {

        #region Переменные

        private int _depth;
        private int _tmpDepth;
        private int _id;
        private bool _triggerEnterSelectFunctionElemenAsExist = false;
        private bool _isFirst = true;
        private bool _caseBlock = false; //только для бинарок в кейзе и лайке
        private bool _isOuterJoin = false;
        public string _return = "Return:\r\n";
        private string _subSelectFunction;
        private IVocabulary _vocabulary;
        private Interval _outerJoinInterval;

        //private List<string> _columnNames = new List<string>();
        //private List<TableStructure> _tableNames = new List<TableStructure>();
        private List<string> _selectColumnNames = new List<string>();
        //private List<string> _groupByColumnsNames = new List<string>();
        //private List<string> _removeCounterColumsNames = new List<string>();
        //private List<ColumnStructure> _columns = new List<ColumnStructure>();
        
        private List<ColumnStructure> _mainColumns = new List<ColumnStructure>();
        private List<ColumnStructure> _subColumns = new List<ColumnStructure>();
        private List<TableStructure> _mainTables = new List<TableStructure>();
        private List<TableStructure> _subTables = new List<TableStructure>();

        private List<LikeStructure> _likeList = new List<LikeStructure>();
        private List<InStructure> _inStructureList = new List<InStructure>();
        private List<BetweenStructure> _betweenList = new List<BetweenStructure>();
        private List<AsStructure> _asList = new List<AsStructure>();
        private List<OrderByStructure> _orderByList = new List<OrderByStructure>();   
        private List<MainListener> _subQueryListeners = new List<MainListener>();
        private List<BaseRule> _baseRules = new List<BaseRule>();
        private List<BinaryComparisionPredicateStructure> _binaries = new List<BinaryComparisionPredicateStructure>();


        #endregion

        public MainListener(int depth)
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

        public List<ColumnStructure> MainColumns
        {
            get { return _mainColumns; }
        }

        public List<ColumnStructure> SubColumns
        {
            get { return _subColumns; }
        }

        public List<TableStructure> MainTables
        {
            get { return _mainTables; }
        }

        public List<TableStructure> SubTables
        {
            get { return _subTables; }
        }

        public List<string> SelectColumnNames
        {
            get { return _selectColumnNames; }
        }

        public List<BinaryComparisionPredicateStructure> Binaries
        {
            get { return _binaries; }
        }

        //public List<string> GroupByColumnsNames
        //{
        //    get { return _groupByColumnsNames; }
        //    set { _groupByColumnsNames = value; }
        //}

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

        public List<BetweenStructure> BetweenList => _betweenList;

        public List<InStructure> InStructureList => _inStructureList;

        //public List<string> RemoveCounterColumsNames
        //{
        //    get => _removeCounterColumsNames;
        //    set => _removeCounterColumsNames = value;
        //}

        #endregion

        public override void EnterFullColumnName([NotNull] MySqlParser.FullColumnNameContext context)
        {
            if (_tmpDepth == _depth)
            {
                if (context.ChildCount < 2)
                {
                    //_columnNames.Add(context.GetText());
                    //возможно не нужно.
                    List<Interval> tmp = new List<Interval>();
                    tmp.Add(context.SourceInterval);
                    _mainColumns.Add(new ColumnStructure(context.GetText(), tmp));
                }
                else
                {
                    string columnName = context.children[1].GetText();
                    columnName = columnName.Remove(0, 1);
                    List<Interval> tmp = new List<Interval>();
                    tmp.Add(context.SourceInterval);
                    ColumnStructure tmpColumn = new ColumnStructure(columnName, tmp);
                    tmpColumn.DotTableId = context.children[0].GetText();
                    //_columnNames.Add(columnName);
                    //возможно не нужно.
                    _mainColumns.Add(tmpColumn);
                }
            }
            else
            {
                if (context.ChildCount < 2)
                {
                    List<Interval> tmp = new List<Interval>();
                    tmp.Add(context.SourceInterval);
                    _subColumns.Add(new ColumnStructure(context.GetText(), tmp));
                }
                else
                {
                    string columnName = context.children[1].GetText();
                    columnName = columnName.Remove(0, 1);
                    List<Interval> tmp = new List<Interval>();
                    tmp.Add(context.SourceInterval);
                    ColumnStructure tmpColumn = new ColumnStructure(columnName, tmp);
                    tmpColumn.DotTableId = context.children[0].GetText();
                    //_columnNames.Add(columnName);
                    //возможно не нужно.
                    _subColumns.Add(tmpColumn);
                }
            }
        }

        public override void EnterSelectColumnElement([NotNull] MySqlParser.SelectColumnElementContext context)
        {
            if(_depth == _tmpDepth)
                _selectColumnNames.Add(context.GetText());
        }
        
        public override void EnterAtomTableItem(MySqlParser.AtomTableItemContext context)
        {
            if (_depth == _tmpDepth)
            {
                if (context.ChildCount < 2)
                {
                    MainTables.Add(new TableStructure(context.GetText(), context.SourceInterval));
                }
                else
                {
                    TableStructure tmpTable = new TableStructure(context.children[0].GetText(), context.SourceInterval);
                    tmpTable.DotedId = context.children.Last().GetText();
                    MainTables.Add(tmpTable);
                }
            }
            else
            {
                if (context.ChildCount < 2)
                {
                    SubTables.Add(new TableStructure(context.GetText(), context.SourceInterval));
                }
                else
                {
                    TableStructure tmpTable = new TableStructure(context.children[0].GetText(), context.children[0].SourceInterval);
                    tmpTable.DotedId = context.children.Last().GetText();
                    SubTables.Add(tmpTable);
                }
            }

            //if (context.ChildCount < 2)
            //{
            //    _tableNames.Add(new TableStructure(context.GetText(), context.SourceInterval));
            //}
            //else
            //{
            //    _tableNames.Add(new TableStructure(context.children[0].GetText(), context.children[0].SourceInterval));
            //}

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

        public override void EnterSelectExpressionElement(MySqlParser.SelectExpressionElementContext context)
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
            if (!_caseBlock)
            {
                if (_depth == _tmpDepth)
                {
                    BinaryComparisionPredicateStructure tmpBinary;
                    if (!_isOuterJoin)
                    {
                         tmpBinary =
                            new BinaryComparisionPredicateStructure(context.left.GetText(),
                                context.comparisonOperator().GetText(), context.right.GetText(),
                                context.SourceInterval, _isOuterJoin);
                    }
                    else
                    {
                         tmpBinary =
                            new BinaryComparisionPredicateStructure(context.left.GetText(),
                                context.comparisonOperator().GetText(), context.right.GetText(),
                                _outerJoinInterval, _isOuterJoin);
                    }
                    if (context.GetChild(2).GetChild(0).GetType().ToString()
                            .Contains("ConstantExpressionAtomContext") || context.GetChild(2).GetChild(0).GetType()
                            .ToString().Contains("MathExpressionAtomContext"))
                    {
                        tmpBinary.Type = (int) PredicateType.simple;
                    }

                    if (context.GetChild(2).GetChild(0).GetType().ToString()
                            .Contains("FullColumnNameExpressionAtomContext"))
                        //&&
                        //context.GetChild(2).GetChild(0).GetChild(0).ChildCount < 2)
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

        public override void EnterQuerySpecification(MySqlParser.QuerySpecificationContext context)
        {
            if (!_isFirst)
            {
                _tmpDepth++;
                MainListener tmpSubListener = new MainListener(_tmpDepth);
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
                _tmpDepth--;
            }
        }

        public override void EnterBetweenPredicate(MySqlParser.BetweenPredicateContext context)
        {
            if (_depth == _tmpDepth)
            {
                _betweenList.Add(new BetweenStructure(context.GetText(), context.Start.Text, context.SourceInterval, context));
            }
        }
        
        public override void EnterLikePredicate([NotNull] MySqlParser.LikePredicateContext context)
        {
            if (!_caseBlock)
            {
                if (_depth == _tmpDepth)
                {
                    LikeStructure tmpLike =
                        new LikeStructure(context.Stop.Text, context.Start.Text, context.SourceInterval);
                    if (context.NOT() != null)
                    {
                        tmpLike.IsNot = true;
                    }

                    _likeList.Add(tmpLike);
                }
            }
        }

        public override void EnterCaseFunctionCall(MySqlParser.CaseFunctionCallContext context)
        {
            _caseBlock = true;
        }

        public override void ExitCaseFunctionCall(MySqlParser.CaseFunctionCallContext context)
        {
            _caseBlock = false;
        }

        public override void EnterOuterJoin(MySqlParser.OuterJoinContext context)
        {
            //ПРЕДПОЛАГАЕМ ЧТО ТОЛЬКО LEFT
            _outerJoinInterval = context.SourceInterval;
            _isOuterJoin = true;
        }

        public override void ExitOuterJoin(MySqlParser.OuterJoinContext context)
        {
            _outerJoinInterval = new Interval();
            _isOuterJoin = false;
        }

        public override void EnterInPredicate(MySqlParser.InPredicateContext context)
        {
            if (!_caseBlock)
            {
                if (_depth == _tmpDepth)
                {
                    InPredicate inPredicateRule = new InPredicate(context.SourceInterval, context, context.GetText());
                    InStructure tmpInStructure = new InStructure(inPredicateRule.Text, context.children[0].GetText(), context.SourceInterval);
                    InStructureList.Add(tmpInStructure);
                }
            }
        }
    }
}
