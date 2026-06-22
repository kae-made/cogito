// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using KAE.CMTools.Core.DataType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KAE.CMTools.Core.DataType.DataType;
using static KAE.CMTools.Core.Relationship;

namespace KAE.CMTools.Core
{
    public class RelationshipEdge<T> : Validater
        where T: ConceptualClass
    {
        public T EdgeInstance { get => edgeInstance; }
        public Multipricity Multiplicity { get => multiplicity; }
        public string Phrase { get => phrase; }
        public SemanticRole EdgeRole { get => edgeRole; }

        public List<string> Properties { get => properties; }

        public bool Validate()
        {
            bool result = true;

            Action<string, string> ShowProblem = (propertyName, message) =>
            {
                Console.WriteLine("Invalid Relationship description");
                Console.WriteLine($" - Relationship Index {rIndex}");
                Console.WriteLine($" - Property '{propertyName}' of '{edgeInstance.KeyLetter}' : {message}.");
            };

            var referentProperties = new List<Property>();
            // check property name
            foreach (var propertyName in Properties)
            {
                if (!edgeInstance.Properties.ContainsKey(propertyName))
                {
                    ShowProblem(propertyName, "has not defined");
                    result = false;
                    break;
                }
                else
                {
                    if (edgeRole == SemanticRole.Participant)
                    {
                        var property = edgeInstance.Properties[propertyName];
                        if (!property.IsReferenDataType())
                        {
                            ShowProblem(propertyName, $" Data Type should be {DataTypeKind.REFERENCE.ToString()}");
                            result = false;
                            break;
                        }
                    }
                    else if (edgeRole == SemanticRole.Referent)
                    {
                        referentProperties.Add(edgeInstance.Properties[propertyName]);
                    }
                }
            }

            if (edgeRole== SemanticRole.Referent)
            {
                bool same = false;
                foreach(var identityLevel in edgeInstance.Identities.Keys)
                {
                    var identity = edgeInstance.Identities[identityLevel];
                    var propNames = identity.Keys.ToArray();
                    same = propNames.OrderBy(x => x).SequenceEqual(this.properties.OrderBy(x => x));
                    if (same)
                    {
                        break;
                    }
                }
                if (!same)
                {
                    string propList = "";
                    foreach (var propName in properties)
                    {
                        if (!string.IsNullOrEmpty(propList))
                        {
                            propList += ", ";
                        }
                        propList += $"'{propName}'";
                    }
                    Console.WriteLine("Invalid Relationship description");
                    Console.WriteLine($" - Relationship Index {rIndex}");
                    Console.WriteLine($" - Properties : {propList} of {edgeInstance.KeyLetter} should be same level identity.");
                    result = false;
                }
            }

            return result;
        }

        public RelationshipEdge(string rIndex, T edgeInstance, SemanticRole role, Multipricity multiplicity, string phrase, List<string> properties)
        {
            this.rIndex = rIndex;
            this.edgeInstance = edgeInstance;
            this.multiplicity = multiplicity;
            this.phrase = phrase;
            this.properties = new List<string>(properties);
            this.edgeRole = role;
        }

        protected string rIndex;
        protected T edgeInstance;
        protected Multipricity multiplicity;
        protected string phrase;
        protected List<string> properties;

        protected SemanticRole edgeRole;
    }
}
