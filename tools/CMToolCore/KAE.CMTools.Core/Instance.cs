using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public abstract class Instance : IDisposable
    {
        public string InstanceId { get => instanceId; }
        public ConceptualClass ConceptualClass { get => conceptualClass; }
        public Dictionary<string, DeterminedProperty> DeterminedProperties { get => determinedProperties; }

        public Dictionary<string, List<Link>> ParticipantLinks { get => participantLinks; }

        public void AddDeterminedProperty(DeterminedProperty property)
        {
            if (!determinedProperties.ContainsKey(property.Name))
            {
                determinedProperties.Add(property.Name, property);
            }
        }
        public void AddLink(Link link)
        {
            if (!participantLinks.ContainsKey(link.Relationship.RIndex))
            {
                participantLinks.Add(link.Relationship.RIndex, new List<Link>());
            }
            participantLinks[link.Relationship.RIndex].Add(link);
        }

        public abstract void Dispose();

        protected Instance(ConceptualClass conceptualClass, string instanceId)
        {
            this.conceptualClass = conceptualClass;
            this.instanceId = instanceId;
            this.determinedProperties = new Dictionary<string, DeterminedProperty>();
            this.participantLinks = new Dictionary<string, List<Link>>();
        }

        protected ConceptualClass conceptualClass;
        protected string instanceId;
        protected Dictionary<string, DeterminedProperty> determinedProperties;
        protected Dictionary<string, List<Link>> participantLinks;
    }
}
