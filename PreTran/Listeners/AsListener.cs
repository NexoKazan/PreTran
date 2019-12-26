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

namespace PreTran.Listeners
{
    class AsListener: MySqlParserBaseListener
    {
        public string _output;
        public string _functionOutput;
        public string _functionName;
        public List<string> _selectFunctions;

        public List<string> AsColumnList= new List<string>();

        public override void EnterFunctionArg([NotNull] MySqlParser.FunctionArgContext context)
        {
            _output = context.GetText();
        }

        public override void EnterFullColumnName([NotNull] MySqlParser.FullColumnNameContext context)
        {
            AsColumnList.Add(context.GetText());
        }

        public override void EnterAggregateFunctionCall([NotNull] MySqlParser.AggregateFunctionCallContext context)
        {
            _output = context.GetText();
            _functionOutput = context.GetText();
            _functionName = context.Start.Text;
        }

        public override void EnterAggregateWindowedFunction([NotNull] MySqlParser.AggregateWindowedFunctionContext context)
        {
            if (context.starArg != null)
            {
                _output = "*";
            }
        }

    }
}
