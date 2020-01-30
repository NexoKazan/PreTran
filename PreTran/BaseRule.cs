using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace PreTran
{
    class BaseRule
    {
        private int _startNodeNumber;
        private int _endNodeNumber;
        private ParserRuleContext _context;
        private string _text;

        public BaseRule(int startNodeNumber, int endNodeNumber, ParserRuleContext context, string text)
        {
            _startNodeNumber = startNodeNumber;
            _endNodeNumber = endNodeNumber;
            _context = context;
            _text = text;
        }

        public int StartNodeNumber { get => _startNodeNumber; set => _startNodeNumber = value; }
        public int EndNodeNumber { get => _endNodeNumber; set => _endNodeNumber = value; }

        public ParserRuleContext Context
        {
            get => _context;
            set => _context = value;
        }
        public string Text { get => _text; set => _text = value; }
    }
}
