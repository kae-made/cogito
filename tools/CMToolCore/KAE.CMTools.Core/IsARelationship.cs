// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
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
            this.subEdges.Add(subEdge.KeyLetter, new SubEdgeOnIsARelationship(relationshipIndex, superEdge, referentProps, subEdge, propertiesOnSub));

           
        }

        public override bool Validate(ILogger logger)
        {
            bool result = true;
            Action<string, string> ShowProblem = (propertyName, message) =>
            {
            };

            foreach (var propertyName in referentProps)
            {
                if (!superEdge.Properties.ContainsKey(propertyName))
                {
                    logger.LogInformation("Invalid Relationship description");
                    logger.LogInformation($" - Relationship Index {relationshipIndex}");
                    logger.LogInformation($" - Property '{propertyName}' of '{superEdge.KeyLetter}' : has not defined.");
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
                    logger.LogInformation("Invalid Relationship description");
                    logger.LogInformation($" - Relationship Index {relationshipIndex}");
                    logger.LogInformation($" - Properties : {propList} of {superEdge.KeyLetter} should be same level identity.");
                    result = false;
                }

                foreach (var keyLett in subEdges.Keys)
                {
                    result = subEdges[keyLett].Validate(logger);
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

    public class SubEdgeOnIsARelationship : Validator
    {
        public ConceptualClass SubEdge { get => subEdge; }
        public List<string> Properties { get => propertiesOnSub; }

        public SubEdgeOnIsARelationship(string rIndex, ConceptualClass superEdge, List<string> referentProps, ConceptualClass edgeInstance, List<string> properties)
        {
            this.rIndex = rIndex;
            this.superEdge = superEdge;
            this.subEdge = edgeInstance;
            this.referentProps = referentProps;
            subEdge = edgeInstance;
            propertiesOnSub = new List<string>(properties);
        }

        private string rIndex;
        private ConceptualClass superEdge;
        private List<string> referentProps;
        private ConceptualClass subEdge;
        private List<string> propertiesOnSub;

        public bool Validate(ILogger logger)
        {
            bool result = true;
            if (referentProps.Count == propertiesOnSub.Count)
            {
                bool isValid = true;
                foreach(var propertyName in referentProps)
                {
                    if (!superEdge.Properties.ContainsKey(propertyName))
                    {
                        isValid = false;
                        logger.LogInformation("Invalid Relationship description");
                        logger.LogInformation($" - Relationship Index {rIndex}");
                        logger.LogInformation($" - Property '{propertyName}' of '{superEdge.KeyLetter}' : has not defined.");
                        break;
                    }
                }
                if (isValid)
                {
                    foreach (var (propertyName, index) in propertiesOnSub.Select((item, index) => (item, index)))
                    {
                        if (!subEdge.Properties.ContainsKey(propertyName))
                        {
                            logger.LogInformation("Invalid Relationship description");
                            logger.LogInformation($" - Relationship Index {rIndex}");
                            logger.LogInformation($" - Property '{propertyName}' of '{subEdge.KeyLetter}' : has not defined.");
                            result = false;
                            break;
                        }
                        else
                        {
                            var property = subEdge.Properties[propertyName];
                            if (!property.IsParticipantProperty())
                            {
                                logger.LogInformation("Invalid Relationship description");
                                logger.LogInformation($" - Relationship Index {rIndex}");
                                logger.LogInformation($" - Property '{propertyName}' of '{subEdge.KeyLetter}' : Data Type should be 'REFERENCE'.");
                                result = false;
                                break;
                            }
                            property.AddReferentProperty(superEdge.KeyLetter, superEdge.Properties[referentProps[index]]);
                        }



                    }
                }
            }
            else
            {
                logger.LogInformation("Invalid Relationship description");
                logger.LogInformation($" - Relationship Index {rIndex}");
                logger.LogInformation($" - Number of Participant properties  of '{subEdge.KeyLetter}' : should be same.");

                result = false;
            }

            return result;
        }
    }
}
