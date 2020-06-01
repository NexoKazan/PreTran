using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using PreTran.DataBaseSchemeStructure;
using PreTran.SchemeCreator;

namespace PreTran.Listeners
{
    class SortRuleListener : MySqlParserBaseListener
    {
        private int _selectElementsFlag = 0;
        private bool _selectColumnElementFlag = false;

        private List<string> _selectColumnNames = new List<string>();
        private List<string> _selectAsRightColumns = new List<string>();
        private List<string> _selectColumnsOrded = new List<string>();

        List<BaseRule> _asRules = new List<BaseRule>();
        private List<SchemeAsStructure> _asStructures = new List<SchemeAsStructure>();

        #region Свойства

        public List<BaseRule> AsRules
        {
            get => _asRules;
        }

        public List<string> SelectColumnNames
        {
            get => _selectColumnNames;
        }

        public List<SchemeAsStructure> AsStructures
        {
            get => _asStructures;
        }

        public List<string> SelectAsRightColumns
        {
            get => _selectAsRightColumns;
        }

        public List<string> SelectColumnsOrded => _selectColumnsOrded;

        #endregion

        public override void EnterEveryRule(ParserRuleContext context)
        {
            if (context.ChildCount > 1)
            {
                if (context.children[1].GetText().ToLower() == "as" && context.GetType().ToString().ToLower() != "mysqlparser+subquerytableitemcontext") 
                {
                    _asStructures.Add(new SchemeAsStructure(context));
                    _asRules.Add(new BaseRule(context.SourceInterval, context, context.GetText()));

                    if (_selectElementsFlag == 1)
                    {
                        _selectAsRightColumns.Add(context.children.Last().GetText());
                        _selectColumnsOrded.Add(context.children.Last().GetText());
                    }
                }
            }
        }

        public override void EnterFullColumnName(MySqlParser.FullColumnNameContext context)
        {
            if (_selectElementsFlag == 1 && _selectColumnElementFlag)
            {
                if (context.ChildCount == 1)
                {
                    _selectColumnNames.Add(context.GetText());
                    _selectColumnsOrded.Add(context.GetText());
                }
                else
                {
                    string tmp = context.children[1].GetText();
                    tmp = tmp.Remove(0, 1);
                    _selectColumnNames.Add(tmp);
                    _selectColumnsOrded.Add(tmp);
                }
            }
        }

        public override void EnterSelectElements(MySqlParser.SelectElementsContext context)
        {
            _selectElementsFlag++;
        }

        public override void EnterSelectColumnElement(MySqlParser.SelectColumnElementContext context)
        {
            _selectColumnElementFlag = true;
        }

        public override void ExitSelectColumnElement(MySqlParser.SelectColumnElementContext context)
        {
            _selectColumnElementFlag = false;
        }
    }
}
