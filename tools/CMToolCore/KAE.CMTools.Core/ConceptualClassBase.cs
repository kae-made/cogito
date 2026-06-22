// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class ConceptualClassBase : ConceptualClass
    {
        public override ConceptualDomain CDomain { get => conceptualDomain; }

        public ConceptualClassBase(ConceptualDomain cDomain, string cClassName, string keyLetter, string number, string description = null) : base(cClassName, keyLetter, number, description)
        {
            conceptualDomain = cDomain;
        }

        protected ConceptualDomain conceptualDomain;
    }
}
