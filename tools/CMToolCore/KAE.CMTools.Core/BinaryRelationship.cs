// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
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

        public bool IsPartOfAssociative { get => isPartOfAssociative; }

        public BinaryRelationship(string rIndex,
            TRef refEdgeInstance, Multipricity refMult, string refPhrase, List<string> refProperties,
            TPart partEdgeInstance, Multipricity partMult, string partPhrase, List<string> partProperteis,
            bool partOfAssociative = false) : base(rIndex)
        {
            this.referentEdge = new RelationshipEdge<TRef>(rIndex, refEdgeInstance, SemanticRole.Referent, refMult, refPhrase, refProperties);
            this.participantEdge = new RelationshipEdge<TPart>(rIndex, partEdgeInstance, SemanticRole.Participant, partMult, partPhrase, partProperteis);
            this.isPartOfAssociative = partOfAssociative;
        }

        protected RelationshipEdge<TRef> referentEdge;
        protected RelationshipEdge<TPart> participantEdge;
        protected bool isPartOfAssociative;

        public override bool Validate(ILogger logger)
        {
            bool result = referentEdge.Validate(logger) && participantEdge.Validate(logger);

            if (result)
            {
                FixParticipantProperties(participantEdge, referentEdge);
            }

            return result;
        }

        static public void FixParticipantProperties(RelationshipEdge<TPart> participantEdge, RelationshipEdge<TRef> referentEdge)
        {
            var participantProperties = participantEdge.Properties;
            var referentProperties = referentEdge.Properties;

            foreach (var (propertyName, index) in participantProperties.Select((item, index) => (item, index)))
            {
                var participantProp = participantEdge.EdgeInstance.Properties[propertyName];
                var referentPropName = referentProperties[index];
                participantProp.AddReferentProperty(referentEdge.EdgeInstance.KeyLetter, referentEdge.EdgeInstance.Properties[referentPropName]);
            }
        }
    }
}
