using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PreTran.TestClasses.Rules;

namespace PreTran.TestClasses.Listeners
{
    class SqlStatsmentsListener : MySqlParserBaseListener
    {
        public List<QuerySpecification> queries = new List<QuerySpecification>();
        public override void EnterQuerySpecification(MySqlParser.QuerySpecificationContext context)
        {
           queries.Add(new QuerySpecification(context.SourceInterval, context, context.GetText()));
        }
    }
}
