using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using PreTran.DataBaseSchemeStructure;

namespace PreTran.Q_Part_Structures
{
    class InStructure
    {
        private string _fullString;
        private string _leftColumnName;
        private Interval _sourceInterval;
        private ColumnStructure _leftColumn;

        public InStructure(string fullString, string leftColumnName, Interval sourceInterval)
        {
            _fullString = fullString;
            _leftColumnName = leftColumnName;
            _sourceInterval = sourceInterval;
        }

        public ColumnStructure LeftColumn
        {
            get => _leftColumn;
            set => _leftColumn = value;
        }

        public string LeftColumnName => _leftColumnName;

        public string FullString => _fullString;

        public TableStructure Table { get; set; }

        public Interval SourceInterval => _sourceInterval;
    }
}
