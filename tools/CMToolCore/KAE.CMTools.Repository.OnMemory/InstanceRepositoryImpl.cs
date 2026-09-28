using KAE.CMTools.Core;

namespace KAE.CMTools.Repository.OnMemory
{
    public class InstanceRepositoryImpl : InstanceRepository
    {
        public Dictionary<string, ConceptualDomain> ConceptualDomains { get => cDomains; }

        public Dictionary<string, Dictionary<string, FieldOfSense>> FieldsOfSense { get => fieldsOfSense; }

        public Dictionary<string, Bridge> Bridges { get => bridges; }

        public ConceptualDomain? AddConceptualDomain(string domainName, string domainKeyLetter)
        {
            ConceptualDomain? cDomain = null;
            if (!cDomains.ContainsKey(domainKeyLetter))
            {
                cDomain = new ConceptualDomain(domainName, domainKeyLetter);
                cDomains.Add(domainKeyLetter, cDomain);
            }
            return cDomain;
        }

        public FieldOfSense? AddFieldOfSense(string domainName, string fosId, string describDate)
        {
            FieldOfSense fos = null;
            if (!fieldsOfSense.ContainsKey(domainName))
            {
                fieldsOfSense.Add(domainName, new Dictionary<string, FieldOfSense>());
            }
            if (fieldsOfSense[domainName].ContainsKey(fosId))
            {
                fos = fieldsOfSense[domainName][fosId];
            }
            else
            {
                fos = new FieldOfSense(cDomains[domainName], fosId, describDate) { };
                fieldsOfSense[domainName].Add(fosId, fos);
            }
            return fos;
        }

        public void Clear()
        {
            fieldsOfSense.Clear();
            cDomains.Clear();
        }

        public Bridge? AddBridge(string bridgeName, string bridgeKeyLetter)
        {
            Bridge bridge = null;
            if (!bridges.ContainsKey(bridgeKeyLetter))
            {
                bridge = new Bridge(bridgeName, bridgeKeyLetter);
                bridges.Add(bridgeKeyLetter, bridge);
            }
            else
            {
                bridge = bridges[bridgeKeyLetter];
            }
            return bridge;
        }

        public InstanceRepositoryImpl()
        {
            cDomains = new Dictionary<string, ConceptualDomain>();
            bridges = new Dictionary<string, Bridge>();
            fieldsOfSense = new Dictionary<string, Dictionary<string, FieldOfSense>>();
        }

        protected Dictionary<string, ConceptualDomain> cDomains;
        protected Dictionary<string, Bridge> bridges;
        protected Dictionary<string, Dictionary<string, FieldOfSense>> fieldsOfSense;

    }
}
