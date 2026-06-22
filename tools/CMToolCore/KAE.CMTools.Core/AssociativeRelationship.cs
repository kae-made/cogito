// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class AssociativeRelationship<TOne, TOther, TAssoc> : Relationship
        where TOne: ConceptualClass
        where TOther : ConceptualClass
        where TAssoc : ConceptualClass
    {
        public RelationshipEdge<TOne> OneEdge { get => oneSideRel.ReferentEdge; }
        public RelationshipEdge<TAssoc> AssocOnOneEdge { get => oneSideRel.ParticipantEdge; }
        public RelationshipEdge<TOther> OtherEdge { get => otherSideRel.ReferentEdge; }
        public RelationshipEdge<TAssoc> AssocOnOtherEdge { get => otherSideRel.ParticipantEdge; }

        public AssociativeRelationship( string rIndex, TOne oneEdgeInstance, Multipricity oneMult, string onePhrase, List<string>oneProperties, List<string> assocOnOneProperties,
            TOther otherEdgeInstance, Multipricity otherMult, string otherPhrase, List<string>otherProperties, TAssoc assocEdgeInstance, List<string>assocOnOtherProperties) : base(rIndex)
        {
            oneSideRel = new BinaryRelationship<TOne, TAssoc>(rIndex, oneEdgeInstance, Multipricity.JustOne, onePhrase, oneProperties, assocEdgeInstance, otherMult, "", assocOnOneProperties);
            otherSideRel = new BinaryRelationship<TOther, TAssoc>(rIndex,otherEdgeInstance, Multipricity.JustOne, otherPhrase, otherProperties, assocEdgeInstance, otherMult, "", assocOnOtherProperties);
        }

        protected BinaryRelationship<TOne, TAssoc> oneSideRel;
        protected BinaryRelationship<TOther, TAssoc> otherSideRel;

        public override bool Validate()
        {
            return oneSideRel.Validate() && otherSideRel.Validate();
        }
    }
}
