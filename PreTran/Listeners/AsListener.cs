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
using Antlr4.Runtime.Misc;
using PreTran.TestClasses.Rules;

namespace PreTran.Listeners
{
    class AsListener: MySqlParserBaseListener
    {
        private int _blocked = 1;
        public string _output;
        public string _functionOutput;
        public string _functionName;
        public List<string> _selectFunctions;

        public List<string> AsColumnList= new List<string>();

        public override void EnterFunctionArg([NotNull] MySqlParser.FunctionArgContext context)
        {
            if (_blocked == 1)
            {
                _output = context.GetText();
            }
        }

        public override void EnterFullColumnName([NotNull] MySqlParser.FullColumnNameContext context)
        {
            AsColumnList.Add(context.GetText());
        }

        public override void EnterAggregateFunctionCall([NotNull] MySqlParser.AggregateFunctionCallContext context)
        {

            if (_blocked == 1)
            {
                _output = context.GetText();
                _functionOutput = context.GetText();
                _functionName = context.Start.Text;
            }
        }

        public override void EnterAggregateWindowedFunction([NotNull] MySqlParser.AggregateWindowedFunctionContext context)
        {
            if (_blocked == 1)
            {

                if (context.starArg != null)
                {
                    _output = "*";
                }
            }
        }

        public override void EnterExtractFunctionCall(MySqlParser.ExtractFunctionCallContext context)
        {
            if (_blocked == 1)
            {
                ExtractFunctionCall extractFunctionCall =
                    new ExtractFunctionCall(context.SourceInterval, context, context.GetText());
                _output = extractFunctionCall.Text;
            }

            _blocked++;
        }

        public override void ExitExtractFunctionCall(MySqlParser.ExtractFunctionCallContext context)
        {
            _blocked--;
        }

        public override void EnterCaseFunctionCall(MySqlParser.CaseFunctionCallContext context)
        {
            if (_blocked == 1)
            {
                CaseFunctionCall caseFunctionCall =
                    new CaseFunctionCall(context.SourceInterval, context, context.GetText());
                _output = caseFunctionCall.Text;
            }
            _blocked++;
        }

        public override void ExitCaseFunctionCall(MySqlParser.CaseFunctionCallContext context)
        {
            _blocked--;
        }
    }
}
