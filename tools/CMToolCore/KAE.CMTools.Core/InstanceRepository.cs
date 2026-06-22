// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace KAE.CMTools.Core
{
    public class InstanceRepository
    {
        public Dictionary<string, ConceptualDomain> ConceptualDomains { get => cDomains; }
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

        public InstanceRepository()
        {
            cDomains = new Dictionary<string, ConceptualDomain>();
        }

        protected Dictionary<string, ConceptualDomain> cDomains;
        
    }
}
