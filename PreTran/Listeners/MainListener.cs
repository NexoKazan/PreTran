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
using System.Runtime.InteropServices;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PreTran.Q_Part_Structures;
using PreTran.TestClasses;
using PreTran.TestClasses.Listeners;

namespace PreTran.Listeners
{
    class MainListener : MySqlParserBaseListener
    {
        
        private int _depth;
        private int _tmpDepth;
        private int _id;
        private bool _triggerEnterSelectFunctionElemenAsExist = false;
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
       
        //заменить после тестов
        public string[] ruleNames;
        public List<Interval> planeRulesInterval = new List<Interval>();
        public List<BaseRule> allRules = new List<BaseRule>();
        public List<BaseRule> readyRules = new List<BaseRule>();
        public List<ITerminalNode> AllTerminalNodes = new List<ITerminalNode>();
        public List<ITerminalNode> ReadyTerminalNodes = new List<ITerminalNode>();

        public MainListener(int depth)
        {
            _tmpDepth = _depth = depth;
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
            readyRules.Add(new EnterFullCollumnName(context.SourceInterval, context, context.GetText()));
            _columnNames.Add(context.GetText());
            TmpListenerFortTerminalNodes tmpListener = new TmpListenerFortTerminalNodes();
            ParseTreeWalker tempWalker = new ParseTreeWalker();
            tempWalker.Walk(tmpListener, context);
            ReadyTerminalNodes.AddRange(tmpListener.TerminalNodes);

        }

        public override void EnterTableName([NotNull] MySqlParser.TableNameContext context)
        {
            _tableNames.Add(context.GetText());
            TmpListenerFortTerminalNodes tmpListener = new TmpListenerFortTerminalNodes();
            ParseTreeWalker tempWalker = new ParseTreeWalker();
            tempWalker.Walk(tmpListener, context);
            ReadyTerminalNodes.AddRange(tmpListener.TerminalNodes);
        }

        public override void EnterSelectColumnElement([NotNull] MySqlParser.SelectColumnElementContext context)
        {
            if(_depth == _tmpDepth)
                _selectColumnNames.Add(context.GetText());

            TmpListenerFortTerminalNodes tmpListener = new TmpListenerFortTerminalNodes();
            ParseTreeWalker tempWalker = new ParseTreeWalker();
            tempWalker.Walk(tmpListener, context);
            ReadyTerminalNodes.AddRange(tmpListener.TerminalNodes);
        }
        
        public override void EnterTableSourceBase([NotNull] MySqlParser.TableSourceBaseContext context)
        { 
            if(_depth == _tmpDepth)
            TableNames.Add(context.GetText());
            TmpListenerFortTerminalNodes tmpListener = new TmpListenerFortTerminalNodes();
            ParseTreeWalker tempWalker = new ParseTreeWalker();
            tempWalker.Walk(tmpListener, context);
            ReadyTerminalNodes.AddRange(tmpListener.TerminalNodes);
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
                        context.uid().GetText(), asl._functionName));
                }
                else
                {
                    _subSelectFunction = context.GetText();
                }
            }
            TmpListenerFortTerminalNodes tmpListener = new TmpListenerFortTerminalNodes();
            ParseTreeWalker tempWalker = new ParseTreeWalker();
            tempWalker.Walk(tmpListener, context);
            ReadyTerminalNodes.AddRange(tmpListener.TerminalNodes);
        }

        public override void EnterBinaryComparasionPredicate([NotNull] MySqlParser.BinaryComparasionPredicateContext context)
        {
            //Console.WriteLine(ruleNames[context.RuleIndex]);
            if (_depth == _tmpDepth)
            {
                BinaryComparisionPredicateStructure tmpBinary = new BinaryComparisionPredicateStructure(context.left.GetText(), context.comparisonOperator().GetText(), context.right.GetText());
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
            readyRules.Add(new BinaryComp(context.SourceInterval, context, context.GetText()));
            TmpListenerFortTerminalNodes tmpListener = new TmpListenerFortTerminalNodes();
            ParseTreeWalker tempWalker = new ParseTreeWalker();
            tempWalker.Walk(tmpListener, context);
            ReadyTerminalNodes.AddRange(tmpListener.TerminalNodes);
        }
        
        public override void EnterGroupByItem([NotNull] MySqlParser.GroupByItemContext context)
        {
            if(_depth == _tmpDepth)
            GroupByColumnsNames.Add(context.GetText());
            TmpListenerFortTerminalNodes tmpListener = new TmpListenerFortTerminalNodes();
            ParseTreeWalker tempWalker = new ParseTreeWalker();
            tempWalker.Walk(tmpListener, context);
            ReadyTerminalNodes.AddRange(tmpListener.TerminalNodes);
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
            TmpListenerFortTerminalNodes tmpListener = new TmpListenerFortTerminalNodes();
            ParseTreeWalker tempWalker = new ParseTreeWalker();
            tempWalker.Walk(tmpListener, context);
            ReadyTerminalNodes.AddRange(tmpListener.TerminalNodes);
        }

        public override void EnterSubqueryExpessionAtom([NotNull] MySqlParser.SubqueryExpessionAtomContext context)
        {
            _depth++;
            MainListener tmpSubListener = new MainListener(_depth);
            tmpSubListener.ID = _id;
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(tmpSubListener, context.selectStatement());
            SubQueryListeners.Add(tmpSubListener);
            TmpListenerFortTerminalNodes tmpListener = new TmpListenerFortTerminalNodes();
            ParseTreeWalker tempWalker = new ParseTreeWalker();
            tempWalker.Walk(tmpListener, context);
            ReadyTerminalNodes.AddRange(tmpListener.TerminalNodes);
            
        }

        public override void ExitSubqueryExpessionAtom([NotNull] MySqlParser.SubqueryExpessionAtomContext context)
        {
            _depth--;
        }

        public override void EnterLikePredicate([NotNull] MySqlParser.LikePredicateContext context)
        {
            if (_depth == _tmpDepth)
            {
                LikeStructure tmpLike = new LikeStructure(context.Stop.Text, context.Start.Text);
                if (context.NOT()!=null)
                {
                    tmpLike.IsNot = true;
                }
                _likeList.Add(tmpLike);
            }
            TmpListenerFortTerminalNodes tmpListener = new TmpListenerFortTerminalNodes();
            ParseTreeWalker tempWalker = new ParseTreeWalker();
            tempWalker.Walk(tmpListener, context);
            ReadyTerminalNodes.AddRange(tmpListener.TerminalNodes);
        }

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            if (context.ChildCount > 1)
            {
                _baseRules.Add(new BaseRule(context.SourceInterval, context, context.GetText()));
                //_return += ruleNames[context.RuleIndex] + " {" + context.SourceInterval + "} " + Environment.NewLine;
                planeRulesInterval.Add(context.SourceInterval);
            }
            allRules.Add(new BaseRule(context.SourceInterval, context, context.GetText()));
        }

        public override void VisitTerminal([NotNull] ITerminalNode node)
        {
           // _return += "TERMINAL: " + Vocabulary.GetDisplayName(node.Symbol.Type) + " {" + node.SourceInterval + "} " + ": " + node.Symbol.Text + Environment.NewLine;
            AllTerminalNodes.Add(node);
        }

        public override void VisitErrorNode([NotNull] IErrorNode node)
        {
            _return += "ERROR: " + Vocabulary.GetDisplayName(node.Symbol.Type) + " {" + node.SourceInterval + "} " + ": " + node.Symbol.Text + Environment.NewLine;
        }
    }
}
