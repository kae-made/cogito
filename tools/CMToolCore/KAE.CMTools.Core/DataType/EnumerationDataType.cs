// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core.DataType
{
    public class EnumerationDataType : DataType
    {
        public IReadOnlyList<string> Enumarations { get => enumarations.AsReadOnly();  }

        public EnumerationDataType(string name, IList<string> enumerationsDef)  : base(name)
        {
            this.enumarations=new List<string>(enumerationsDef.ToArray());
            
        }

        protected List<string> enumarations;

        public override Type GetType()
        {
            return typeof(string);
        }
    }
}
