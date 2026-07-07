// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace KAE.CMTools.Core
{
    public class InstanceRepository
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
        public InstanceRepository()
        {
            cDomains = new Dictionary<string, ConceptualDomain>();
            fieldsOfSense = new Dictionary<string, Dictionary<string, FieldOfSense>>();
        }

        protected Dictionary<string, ConceptualDomain> cDomains;
        protected Dictionary<string, Dictionary<string, FieldOfSense>> fieldsOfSense;
        
    }
}
