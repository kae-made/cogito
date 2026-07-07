using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class FieldOfSense
    {
        public string FoSId { get => fosId; }
        public ConceptualDomain Domain { get => domain; }

        public string Date { get => date; }
        public Dictionary<string, Dictionary<string,Instance>> Instances { get => instances; }

        public Dictionary<string, List<Link>> Links { get => links; }

        public void AddInstance(Instance instance)
        {
            if (!this.instances.ContainsKey(instance.ConceptualClass.KeyLetter))
            {
                this.instances.Add(instance.ConceptualClass.KeyLetter, new Dictionary<string, Instance>());
            }
            instances[instance.ConceptualClass.KeyLetter].Add(instance.InstanceId, instance);
        }

        public void DeleteInstance(Instance instance)
        {
            if (instances.ContainsKey(instance.ConceptualClass.KeyLetter))
            {
                if (instances[instance.ConceptualClass.KeyLetter].ContainsKey(instance.InstanceId))
                {
                    instances[instance.ConceptualClass.KeyLetter].Remove(instance.InstanceId);
                }
            }
        }
        public void AddLink(Link link)
        {
            if (!links.ContainsKey(link.Relationship.RIndex))
            {
                links.Add(link.Relationship.RIndex, new List<Link>());
            }
            links[link.Relationship.RIndex].Add(link);
        }
        public void RemoveLink(Link link)
        {
            if (links.ContainsKey(link.Relationship.RIndex))
            {
                links[link.Relationship.RIndex].Remove(link);
            }
        }

        public FieldOfSense(ConceptualDomain domain, string fosId, string date)
        {
            this.domain = domain;
            this.fosId = fosId;
            this.date = date;
            this.instances = new Dictionary<string, Dictionary<string, Instance>>();
            this.links= new Dictionary<string, List<Link>>();
        }

        protected string fosId;
        protected ConceptualDomain domain;
        protected string date;
        protected Dictionary<string, Dictionary<string, Instance>> instances;
        protected Dictionary<string, List<Link>> links; 
    }
}
