// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class BinaryRelationship<TRef, TPart> : Relationship
        where TRef : ConceptualClass
        where TPart : ConceptualClass
    {
        public RelationshipEdge<TRef> ReferentEdge { get => referentEdge; }
        public RelationshipEdge<TPart> ParticipantEdge { get => participantEdge; }
        public BinaryRelationship(string rIndex,
            TRef refEdgeInstance, Multipricity refMult, string refPhrase, List<string> refProperties,
            TPart partEdgeInstance, Multipricity partMult, string partPhrase, List<string> partProperteis) : base(rIndex)
        {
            this.referentEdge = new RelationshipEdge<TRef>(rIndex, refEdgeInstance, SemanticRole.Referent, refMult, refPhrase, refProperties);
            this.participantEdge = new RelationshipEdge<TPart>(rIndex, partEdgeInstance, SemanticRole.Participant, partMult, partPhrase, partProperteis);
        }

        protected RelationshipEdge<TRef> referentEdge;
        protected RelationshipEdge<TPart> participantEdge;

        public override bool Validate()
        {
            return referentEdge.Validate() && participantEdge.Validate();
        }
    }
}
