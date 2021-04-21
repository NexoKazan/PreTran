using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PreTran.DataBaseSchemeStructure;
using PreTran.Q_Structures;

namespace PreTran.Services
{
    class JoinOptimizer
    {
        private List<List<JoinStructure>> _inputJoinSequences;
        private List<List<JoinStructure>> _outputJoinSequences;
        private DataBaseStructure _inDataBase;

        #region Constructors

        public JoinOptimizer(List<List<JoinStructure>> inputJoins, DataBaseStructure inDatabase)
        {
            _inputJoinSequences = inputJoins;
            _inDataBase = inDatabase;
        }

        #endregion

        #region Properties

        public List<List<JoinStructure>> OutputJoinSequences => _outputJoinSequences;

        #endregion

    }
}
