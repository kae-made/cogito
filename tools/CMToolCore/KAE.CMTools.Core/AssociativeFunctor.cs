using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class AssociativeFunctor<TOne, TOther, TAssoc> : AssociativeRelationship<TOne, TOther, TAssoc>, Functor
        where TOne : ConceptualClass
        where TOther : ConceptualClass
        where TAssoc : ConceptualClass
    {
        public AssociativeFunctor(
            string fIndex,
            TOne oneEdgeInstance, Multipricity oneMult, List<string> oneProperties, List<string> assocOnOneProperties,
            TOther otherEdgeInstance, Multipricity otherMult, List<string> otherProperties, TAssoc assocEdgeInstance, List<string> assocOnOtherProperties)
            : base(fIndex, oneEdgeInstance, oneMult, "", oneProperties, assocOnOneProperties, otherEdgeInstance, otherMult, "", otherProperties, assocEdgeInstance, assocOnOtherProperties)
        {
        }

        public string FIndex { get => RIndex; }
    }
}
