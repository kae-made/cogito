// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core.DataType
{
    public class ComplexDataType : DataType
    {
        public Dictionary<string, DataType> Children { get => children; }

        public ComplexDataType(string name, Dictionary<string, DataType> children) : base(name)
        {
            this.children = new Dictionary<string, DataType>();
            foreach(var child in children)
            {
                this.children.Add(child.Key, child.Value);
            }
        }

        protected Dictionary<string, DataType> children;

        public override Type GetType()
        {
            return typeof(ComplexDataType);
        }
    }
}
