// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class IsARelationship<TSuper> : Relationship
        where TSuper : ConceptualClass
    {
        public TSuper SuperEdge { get => superEdge; }
        public List<string> ReferentProperties { get => referentProps; }
        public Dictionary<string, SubEdgeOnIsARelationship> SubEdges { get=> subEdges; }
        public IsARelationship(string rIndex, TSuper superEdgeInstance, List<string> propertiesOnSuper) : base(rIndex)
        {
            this.superEdge = superEdgeInstance;
            this.referentProps = new List<string>(propertiesOnSuper);
            this.subEdges = new Dictionary<string, SubEdgeOnIsARelationship>();
        }

        public void AddSubEdge(ConceptualClass subEdge, List<string> propertiesOnSub)
        {
            this.subEdges.Add(subEdge.KeyLetter, new SubEdgeOnIsARelationship(relationshipIndex, referentProps, subEdge, propertiesOnSub));
        }

        public override bool Validate()
        {
            bool result = true;
            Action<string, string> ShowProblem = (propertyName, message) =>
            {
            };

            foreach (var propertyName in referentProps)
            {
                if (!superEdge.Properties.ContainsKey(propertyName))
                {
                    Console.WriteLine("Invalid Relationship description");
                    Console.WriteLine($" - Relationship Index {relationshipIndex}");
                    Console.WriteLine($" - Property '{propertyName}' of '{superEdge.KeyLetter}' : has not defined.");
                    result = false;
                }
            }
            if (result)
            {
                bool same = false;
                foreach (var identityLevel in superEdge.Identities.Keys)
                {
                    var identity = superEdge.Identities[identityLevel];
                    var propNames = identity.Keys.ToArray();
                    same = propNames.OrderBy(x => x).SequenceEqual(this.referentProps.OrderBy(x => x));
                    if (same)
                    {
                        break;
                    }
                }
                if (!same)
                {
                    string propList = "";
                    foreach (var propName in referentProps)
                    {
                        if (!string.IsNullOrEmpty(propList))
                        {
                            propList += ", ";
                        }
                        propList += $"'{propName}'";
                    }
                    Console.WriteLine("Invalid Relationship description");
                    Console.WriteLine($" - Relationship Index {relationshipIndex}");
                    Console.WriteLine($" - Properties : {propList} of {superEdge.KeyLetter} should be same level identity.");
                    result = false;
                }



                foreach (var keyLett in subEdges.Keys)
                {
                    result = subEdges[keyLett].Validate();
                    if (result == false)
                    {
                        break;
                    }
                }
            }
            return result;
        }

        private TSuper superEdge;
        private List<string> referentProps = new List<string>();
        private Dictionary<string, SubEdgeOnIsARelationship> subEdges;
    }

    public class SubEdgeOnIsARelationship : Validater
    {
        public ConceptualClass SubEdge { get => subEdge; }
        public List<string> Properties { get => propertiesOnSub; }

        public SubEdgeOnIsARelationship(string rIndex, List<string> referentProps, ConceptualClass edgeInstance, List<string> properties)
        {
            this.rIndex = rIndex;
            this.referentProps = referentProps;
            subEdge = edgeInstance;
            propertiesOnSub = new List<string>(properties);
        }

        private string rIndex;
        private List<string> referentProps;
        private ConceptualClass subEdge;
        private List<string> propertiesOnSub;

        public bool Validate()
        {
            bool result = true;
            if (referentProps.Count == propertiesOnSub.Count)
            {
                foreach (var propertyName in propertiesOnSub)
                {
                    if (!subEdge.Properties.ContainsKey(propertyName))
                    {
                        Console.WriteLine("Invalid Relationship description");
                        Console.WriteLine($" - Relationship Index {rIndex}");
                        Console.WriteLine($" - Property '{propertyName}' of '{subEdge.KeyLetter}' : has not defined.");
                        result = false;
                        break;
                    }
                    else
                    {
                        var property = subEdge.Properties[propertyName];
                        if (!property.IsReferenDataType())
                        {
                            Console.WriteLine("Invalid Relationship description");
                            Console.WriteLine($" - Relationship Index {rIndex}");
                            Console.WriteLine($" - Property '{propertyName}' of '{subEdge.KeyLetter}' : Data Type should be 'REFERENCE'.");
                            result = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid Relationship description");
                Console.WriteLine($" - Relationship Index {rIndex}");
                Console.WriteLine($" - Number of Participant Properties  of '{subEdge.KeyLetter}' : should be same.");

                result = false;
            }

            return result;
        }
    }
}
