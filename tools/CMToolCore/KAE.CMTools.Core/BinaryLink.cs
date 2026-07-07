using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class BinaryLink : Link
    {
        public bool IsIsA { get { return (relationship is IsARelationship<ConceptualClass>); } } 
        public Instance Referent { get => referent; }
        public Instance Participant { get => participant; }

        public DeterminedPropertyPair AddFormedProperties(DeterminedProperty referent, DeterminedProperty participant)
        {
            var pair = new DeterminedPropertyPair(referent, participant);
            propertyPairs.Add(pair);
            return pair;
        }

        public List<DeterminedPropertyPair> PropertyPaires { get => propertyPairs; }
        public BinaryLink(FieldOfSense fos, Relationship relationship, Instance referent, Instance participant) : base(relationship)
        {
            this.fieldOfSense = fos;
            // build determined property pair
            propertyPairs = new List<DeterminedPropertyPair>();
            this.referent = referent;
            this.participant = participant;
        }


        protected FieldOfSense fieldOfSense;
        protected List<DeterminedPropertyPair> propertyPairs;
        protected Instance referent;
        protected Instance participant;

    }
}