using KAE.CMTools.Core;

namespace KAE.CMTools.Repository.OnMemory
{
    public class InstanceRepositoryImpl : InstanceRepository
    {
        public Dictionary<string, ConceptualDomain> ConceptualDomains { get => cDomains; }

        public Dictionary<string, Dictionary<string, FieldOfSense>> FieldsOfSense { get => fieldsOfSense; }
        public ConceptualDomain? AddConceptualDomain(string domainName)
        {
            ConceptualDomain? cDomain = null;
            if (!cDomains.ContainsKey(domainName))
            {
                cDomain = new ConceptualDomain(domainName);
                cDomains.Add(domainName, cDomain);
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
        public InstanceRepositoryImpl()
        {
            cDomains = new Dictionary<string, ConceptualDomain>();
            fieldsOfSense = new Dictionary<string, Dictionary<string, FieldOfSense>>();
        }

        protected Dictionary<string, ConceptualDomain> cDomains;
        protected Dictionary<string, Dictionary<string, FieldOfSense>> fieldsOfSense;

    }
}
