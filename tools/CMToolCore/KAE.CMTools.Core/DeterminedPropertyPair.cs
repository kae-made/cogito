using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class DeterminedPropertyPair
    {
        public DeterminedProperty Referent { get=>referent; }
        public DeterminedProperty Participant { get=>participant; }

        public DeterminedPropertyPair(DeterminedProperty referent, DeterminedProperty participant)
        {
            this.referent = referent;
            this.participant = participant;
        }

        protected DeterminedProperty referent;
        protected DeterminedProperty participant;
    }
}
