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

using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using PreTran.Q_Part_Structures;
using PreTran.Q_Structures;

namespace PreTran.Listeners
{
    class MyMySQLListener : MySqlParserBaseListener
    {
        public IVocabulary voc;
        public List<string> TableNames = new List<string>();
        public List<string> ColumnNames = new List<string>();
        public List<string> ExprColumnNames = new List<string>();
        public List<string> SelectColumns = new List<string>();
        public List<string> GroupByColumnsNames = new List<string>();
        public List<JoinStructure> JoinStructures = new List<JoinStructure>();
        public List<WhereStructure> WhereList = new List<WhereStructure>();
        public List<AsStructure> AsList = new List<AsStructure>();
        public List<OrderByStructure> OrderByList = new List<OrderByStructure>();

        public List<MyMySQLListener> SubQueryListeners = new List<MyMySQLListener>();
        
        private bool _triggerEnterSelectFunctionElemenAsExist = false;
        private int _depth;
        private int _tmpDepth;

        public string _return = "Return:\r\n";

        public MyMySQLListener(int depth)
        {
            _tmpDepth =_depth = depth;

        }

        public int Depth
        {
            get { return _depth; }
        }

        public override void EnterFullColumnName([NotNull] MySqlParser.FullColumnNameContext context)
        {
            if(_depth == _tmpDepth)
            ColumnNames.Add(context.GetText());
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
                        context.uid().GetText(), asl._functionName));
                    ExprColumnNames.AddRange(asl.AsColumnList);
                }
            }
        }

        public override void EnterBinaryComparasionPredicate([NotNull] MySqlParser.BinaryComparasionPredicateContext context)
        {//обернуть в два листенера

            if (_depth == _tmpDepth)
            {
                if (!context.GetChild(2).GetChild(0).GetType().ToString().Contains("SubqueryExpessionAtom"))
                {
                    if (context.Stop.Type != 968)
                    {
                        ExprColumnNames.Add(context.left.GetText());
                        WhereList.Add(new WhereStructure(context.GetText(), context.left.GetText()));
                    }
                    else
                    {
                        if (context.Stop.Type == 968)
                        {
                            JoinListener tmpJoinListener = new JoinListener();
                            ParseTreeWalker wlk = new ParseTreeWalker();
                            wlk.Walk(tmpJoinListener, context);
                            JoinStructures.Add(new JoinStructure(context.Start.Text, context.Stop.Text,
                                tmpJoinListener.Output));
                        }
                    }
                }
            }
        }

        public override void EnterSelectColumnElement([NotNull] MySqlParser.SelectColumnElementContext context)
        {
            if(_depth == _tmpDepth)
            SelectColumns.Add(context.GetText());
        }

        public override void EnterGroupByItem([NotNull] MySqlParser.GroupByItemContext context)
        {
            if(_depth == _tmpDepth)
            GroupByColumnsNames.Add(context.GetText());
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

        public override void EnterSubqueryExpessionAtom([NotNull] MySqlParser.SubqueryExpessionAtomContext context)
        {
            _depth++;
            MyMySQLListener tmpSubListener = new MyMySQLListener(_depth);
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(tmpSubListener, context.selectStatement());
            SubQueryListeners.Add(tmpSubListener);
            
        }

        public override void ExitSubqueryExpessionAtom([NotNull] MySqlParser.SubqueryExpessionAtomContext context)
        {
            _depth--;
        }
    }
}
