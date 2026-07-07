using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class AssociativeLink : Link
    {
        public Instance OneSideInstance { get => oneSideInstance; }
        public Instance OtherSideInstance { get => otherSideInstance; }
        public Instance AssociativeInstance { get => associativeInstance; }
        public List<DeterminedPropertyPair> OneSidePropertyPairs { get => oneSidePropertyPairs; }
        public List<DeterminedPropertyPair> OtherSidePropertyPairs { get => otherSidePropertyPairs; }

        public AssociativeLink(FieldOfSense fos, Relationship relationship, Instance oneSide, Instance otherSide, Instance associative):base(relationship)
        {
            this.fieldOfSense = fos;
            this.oneSideInstance = oneSide;
            this.otherSideInstance = otherSide;
            this.associativeInstance = associative;
            this.oneSidePropertyPairs = new List<DeterminedPropertyPair>();
            this.otherSidePropertyPairs = new List<DeterminedPropertyPair>();

            // Build property pairs
        }

        protected FieldOfSense fieldOfSense;
        protected Instance associativeInstance;
        protected Instance oneSideInstance;
        protected Instance otherSideInstance;
        protected List<DeterminedPropertyPair> oneSidePropertyPairs;
        protected List<DeterminedPropertyPair> otherSidePropertyPairs;
    }
}
