// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public abstract class ConceptualClass
    {
        public abstract ConceptualDomain CDomain { get;  }
        public string Name { get => cname; }
        public string KeyLetter { get => keyLetter;  }
        public string Number { get => number; }
        public string Description { get => description; }
        public Dictionary<int, Dictionary<string, Property>> Identities { get => identities; }

        public Dictionary<string, Property> Properties { get => properties; }

        public void AddProperty(Property property)
        {
            if (properties.ContainsKey(property.Name))
            {
                Console.WriteLine($"Invalid definition : Property of {this.keyLetter} in Domain : {this.Name}");
                Console.WriteLine($"Property : '{property.Name}' has been used!");
            }
            properties.Add(property.Name, property);
        }

        public void AddIdentity(int level, Property property)
        {
            if (!identities.ContainsKey(level))
            {
                identities.Add(level, new Dictionary<string, Property>());
            }
            identities[level].Add(property.Name, property);
        }


        protected ConceptualClass(string cClassName, string keyLetter, string number, string descriptionl)
        {
            this.cname = cClassName;
            this.keyLetter = keyLetter;
            this.number = number;
            this.description = description;

            identities = new Dictionary<int, Dictionary<string,Property>>();
            properties = new Dictionary<string, Property>();
        }

        protected string cname;
        protected string keyLetter;
        protected string number;
        protected string description;
        protected Dictionary<int, Dictionary<string, Property>> identities;
        protected Dictionary<string, Property> properties;
    }
}
