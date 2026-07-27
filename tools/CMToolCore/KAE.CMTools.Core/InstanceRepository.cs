// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace KAE.CMTools.Core
{
    public interface InstanceRepository
    {
        Dictionary<string, ConceptualDomain> ConceptualDomains { get; }

        Dictionary<string, Dictionary<string, FieldOfSense>> FieldsOfSense { get; }
        ConceptualDomain? AddConceptualDomain(string domainName);

        void Clear();

        FieldOfSense? AddFieldOfSense(string domainName, string fosId, string describDate);
        
    }
}
