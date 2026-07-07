using KAE.CMTools.Core;
using KAE.CMTools.Generator.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace KAE.CMTools.Generator
{
    public class YamlFieldOfSenseReader : SchemaReader
    {
        public string ParsedFoSId { get => parsedFosId; }
        private string parsedFosId = "";
        public void Parse(InstanceRepository repository)
        {
            currentRepository = repository;
            FieldOfSense parsedFoS = null;
            if (validatedDescriptionJson != null)
            {
                foreach (var prop in validatedDescriptionJson)
                {
                    if (prop.Key == "fos")
                    {
                        var fosValue = prop.Value;
                        string fosId = "";
                        string domainName = "";
                        string descripDate = "";
                        foreach (var fosProp in (JObject)fosValue)
                        {
                            if (fosProp.Key == "fosId")
                            {
                                fosId = (string)fosProp.Value;
                            }
                            else if (fosProp.Key == "domain")
                            {
                                domainName = (string)fosProp.Value;
                            }
                            else if (fosProp.Key == "date")
                            {
                                descripDate = (string)fosProp.Value;
                            }
                        }
                        if (!string.IsNullOrEmpty(fosId) && !string.IsNullOrEmpty(domainName) && !string.IsNullOrEmpty(descripDate))
                        {
                            if (currentRepository.ConceptualDomains.ContainsKey(domainName))
                            {
                                parsedFoS = currentRepository.AddFieldOfSense(domainName, fosId, descripDate);
                            }
                            else
                            {
                                logger.LogInformation($"Description Error :");
                                logger.LogInformation($" '{domainName}' has not described.");
                                break;
                            }
                        }
                        foreach (var fosProp in (JObject)fosValue)
                        {
                            if (fosProp.Key == "instances")
                            {
                                foreach (var inst in (JArray)fosProp.Value)
                                {
                                    ParseInstance(currentRepository.ConceptualDomains[domainName], parsedFoS, inst);
                                }
                            }
                        }
                    }
                }
            }
            parsedFosId = parsedFoS.FoSId;
            currentFoS = parsedFoS;
        }

        protected void ParseInstance(ConceptualDomain domain, FieldOfSense fos, JToken? instanceDesc)
        {
            string instanceId = (string)instanceDesc["instance"];
            var instProps = instanceDesc["properties"] as JObject;
            logger.LogInformation($"Parsing instance : {instanceId} ...");

            ConceptualClass cclass = null;

            foreach (var instProp in instProps)
            {
                if (instProp.Key == "cclass")
                {
                    string cclassKeyLett = (string)instProp.Value;
                    cclass = currentRepository.ConceptualDomains[domain.Name].ConceptualClasses[cclassKeyLett];
                    break;
                }
            }
            if (cclass != null)
            {
                logger.LogInformation($"  is instance of '{cclass.Name}" + "{" + $"{cclass.KeyLetter}, {cclass.Number}" + "}'");
                Instance instance = null;
                if (!fos.Instances.ContainsKey(cclass.KeyLetter))
                {
                    fos.Instances.Add(cclass.KeyLetter, new Dictionary<string, Instance>());
                }
                if (fos.Instances[cclass.KeyLetter].ContainsKey(instanceId))
                {
                    logger.LogInformation("Warning :");
                    logger.LogInformation($" instance : '{instanceId}:{cclass.KeyLetter}' has been described.");
                    instance = fos.Instances[cclass.KeyLetter][instanceId];
                }
                else
                {
                    instance = new InstanceBase(fos, cclass, instanceId);
                    fos.Instances[cclass.KeyLetter].Add(instanceId, instance);
                }

                foreach (var instProp in instProps)
                {
                    if (instProp.Key != "cclass")
                    {
                        string propName = instProp.Key;
                        object propValue = YamlDataTypeTranslator.ConvertJValueToObject((JValue)instProp.Value);
                        if (cclass.Properties.ContainsKey(propName))
                        {
                            var property = cclass.Properties[propName];
                            var dProperty = new DeterminedProperty(property, propValue);
                            if (instance.DeterminedProperties.ContainsKey(propName))
                            {
                                logger.LogInformation($"  Property : '{propName}' has been described. Try to update new description.");
                                instance.DeterminedProperties[propName] = dProperty;
                            }
                            else
                            {
                                instance.AddDeterminedProperty(dProperty);
                                logger.LogInformation($"  Property Parsed - '{propName}'");
                            }
                        }
                    }
                }
            }
        }

        public bool Read(Stream schemaStream, Stream descripStream)
        {
            var validator = new YamlValidator(schemaStream, descripStream) { errorShowChars = 100, Logger = logger };
            if (!validator.Validate())
            {
                validator.ShowErrors();
                return false;
            }
            validatedDescriptionJson = validator.ValidatedDescripJson;
            return true;
        }

        public bool Validate()
        {
            bool result = true;
            if (currentFoS == null)
            {
                return false;
            }

            logger.LogInformation("Checking Identities...");
            var propValComparer = EqualityComparer<IEnumerable<object>>.Create(
                (x, y) => x.SequenceEqual(y),
                obj => obj.Aggregate(0, (h, v) => HashCode.Combine(h, v?.GetHashCode() ?? 0)));
            foreach (var cclassKeyLett in currentFoS.Domain.ConceptualClasses.Keys)
            {
                if (currentFoS.Instances.ContainsKey(cclassKeyLett))
                {
                    var cclass = currentFoS.Domain.ConceptualClasses[cclassKeyLett];
                    logger.LogInformation($" - {cclass.Name}" + "{" + $"{cclassKeyLett}, {cclass.Number}" + "}...");
                    foreach (var idLevel in cclass.Identities.Keys)
                    {
                        // identity check
                        var identitis = cclass.Identities[idLevel];
                        var allInstancesOfTheCClass = currentFoS.Instances[cclassKeyLett];
                        bool allDistinct = allInstancesOfTheCClass.Select(inst => identitis.Keys.Select(name => inst.Value.DeterminedProperties[name].Value).ToArray()).Distinct(propValComparer).Count() == allInstancesOfTheCClass.Count;
                        if (!allDistinct)
                        {
                            logger.LogInformation($"   Level : {idLevel} is missing.");
                            var duplicates = allInstancesOfTheCClass.GroupBy(inst =>
                            identitis.Keys.Select(name => inst.Value.DeterminedProperties[name].Value).ToArray(), propValComparer)
                                .Where(g => g.Count() > 1)
                                .SelectMany(g => g)
                                .ToList();
                            logger.LogInformation("     Duplicated Instances :");
                            foreach (var dupItem in duplicates)
                            {
                                var dupInstId = dupItem.Key;
                                var dupInst = dupItem.Value;
                                string propValues = "";
                                foreach (var propName in identitis.Keys)
                                {
                                    if (!string.IsNullOrEmpty(propValues))
                                    {
                                        propValues += ", ";
                                    }
                                    propValues += $"{propName}:{dupInst.DeterminedProperties[propName].Value.ToString()}";
                                }
                                logger.LogInformation($"    - instance:{dupInstId} " + "{" + $"{propValues}" + "}");
                            }
                            result = false;
                        }
                    }
                }
            }

            if (!result)
            {
                return result;
            }

            foreach (var relIndex in currentFoS.Domain.Relationships.Keys)
            {
                logger.LogInformation($"Validating Relationship link : {relIndex} ...");
                var relationship = currentFoS.Domain.Relationships[relIndex];
                if (relationship is IsARelationship<ConceptualClass>)
                {
                    result = ValidateLinkIsARelationship((IsARelationship<ConceptualClass>)relationship);
                }
                else if (relationship is BinaryRelationship<ConceptualClass, ConceptualClass>)
                {
                    result = ValidateLinkBinaryRelationship((BinaryRelationship<ConceptualClass, ConceptualClass>)relationship);
                }
                else if (relationship is AssociativeRelationship<ConceptualClass, ConceptualClass, ConceptualClass>)
                {
                    result = ValidateLinkAssociativeRelationship((AssociativeRelationship<ConceptualClass, ConceptualClass, ConceptualClass>)relationship);
                }
            }
            return result;
        }

        protected bool ValidateLinkAssociativeRelationship(AssociativeRelationship<ConceptualClass, ConceptualClass, ConceptualClass> relationship)
        {
            bool result = true;

            // One Side
            var oneSideRel = relationship.OneSideRelationship;
            var otherSideRel = relationship.OtherSideRelationship;

            logger.LogInformation($" {relationship.RIndex} is Associtive Relationship.");

            if (relationship.AssocOnOneEdge.EdgeInstance.KeyLetter != relationship.AssocOnOtherEdge.EdgeInstance.KeyLetter)
            {
                result = false;
                return result;
            }

            if (currentFoS.Instances.ContainsKey(relationship.OneEdge.EdgeInstance.KeyLetter) &&
                currentFoS.Instances.ContainsKey(relationship.OtherEdge.EdgeInstance.KeyLetter) &&
                currentFoS.Instances.ContainsKey(relationship.AssocOnOneEdge.EdgeInstance.KeyLetter))
            {
                foreach (var assocInstId in currentFoS.Instances[relationship.AssocOnOneEdge.EdgeInstance.KeyLetter].Keys)
                {
                    var assocInst = currentFoS.Instances[relationship.AssocOnOneEdge.EdgeInstance.KeyLetter][assocInstId];

                    var assocDPropsOnOneSide = new List<DeterminedProperty>();
                    foreach (var propName in relationship.AssocOnOneEdge.Properties)
                    {
                        assocDPropsOnOneSide.Add(assocInst.DeterminedProperties[propName]);
                    }
                    var assocDPropsOnOtherSide = new List<DeterminedProperty>();
                    foreach (var propName in relationship.AssocOnOtherEdge.Properties)
                    {
                        assocDPropsOnOtherSide.Add(assocInst.DeterminedProperties[propName]);
                    }

                    foreach (var candOneInstId in currentFoS.Instances[relationship.OneEdge.EdgeInstance.KeyLetter].Keys)
                    {
                        var candOneInst = currentFoS.Instances[relationship.OneEdge.EdgeInstance.KeyLetter][candOneInstId];
                        var oneDProps = new List<DeterminedProperty>();
                        foreach (var propName in relationship.OneEdge.Properties)
                        {
                            oneDProps.Add(candOneInst.DeterminedProperties[propName]);
                        }

                        bool found = false;
                        if (assocDPropsOnOneSide.Select(dp => dp.Value).SequenceEqual(oneDProps.Select(dp => dp.Value)))
                        {
                            foreach (var candOtherInstId in currentFoS.Instances[relationship.OtherEdge.EdgeInstance.KeyLetter].Keys)
                            {
                                var candOtherInst = currentFoS.Instances[relationship.OtherEdge.EdgeInstance.KeyLetter][candOtherInstId];
                                var otherDProps = new List<DeterminedProperty>();
                                foreach (var propName in relationship.OtherEdge.Properties)
                                {
                                    otherDProps.Add(candOtherInst.DeterminedProperties[propName]);
                                }
                                if (assocDPropsOnOtherSide.Select(dp => dp.Value).SequenceEqual(otherDProps.Select(dp => dp.Value)))
                                {
                                    if (found)
                                    {
                                        // error
                                        result = false;
                                    }
                                    else
                                    {
                                        found = true;
                                        var link = new AssociativeLink(currentFoS, relationship, candOneInst, candOtherInst, assocInst);
                                        foreach (var (rdp, pdp) in oneDProps.Zip(assocDPropsOnOneSide, (x, y) => (x, y)))
                                        {
                                            link.OneSidePropertyPairs.Add(new DeterminedPropertyPair(rdp, pdp));
                                        }
                                        foreach (var (rdp, pdp) in otherDProps.Zip(assocDPropsOnOtherSide, (x, y) => (x, y)))
                                        {
                                            link.OtherSidePropertyPairs.Add(new DeterminedPropertyPair(rdp, pdp));
                                        }
                                        candOneInst.AddLink(link);
                                        candOtherInst.AddLink(link);
                                        assocInst.AddLink(link);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (relationship.AssocOnOtherEdge.Multiplicity == Relationship.Multipricity.JustOne || relationship.AssocOnOtherEdge.Multiplicity == Relationship.Multipricity.MoreThanOne)
            {
                if (currentFoS.Instances.ContainsKey(relationship.OneEdge.EdgeInstance.KeyLetter))
                {
                    foreach (var oneInstId in currentFoS.Instances[relationship.OneEdge.EdgeInstance.KeyLetter].Keys)
                    {
                        var oneInst = currentFoS.Instances[relationship.OneEdge.EdgeInstance.KeyLetter][oneInstId];
                        if (oneInst.ParticipantLinks.ContainsKey(relationship.RIndex))
                        {
                            if (oneInst.ParticipantLinks[relationship.RIndex].Count > 1)
                            {
                                if (relationship.AssocOnOneEdge.Multiplicity == Relationship.Multipricity.JustOne)
                                {
                                    result = false;
                                }
                            }
                            else if (oneInst.ParticipantLinks[relationship.RIndex].Count == 0)
                            {
                                result = false;
                            }
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
            }
            else if (relationship.AssocOnOtherEdge.Multiplicity == Relationship.Multipricity.ZeroOrOne)
            {
                if (currentFoS.Instances.ContainsKey(relationship.OneEdge.EdgeInstance.KeyLetter))
                {
                    foreach (var oneInstId in currentFoS.Instances[relationship.OneEdge.EdgeInstance.KeyLetter].Keys)
                    {
                        var oneInst = currentFoS.Instances[relationship.OneEdge.EdgeInstance.KeyLetter][oneInstId];
                        if (oneInst.ParticipantLinks.ContainsKey(relationship.RIndex))
                        {
                            if (oneInst.ParticipantLinks[relationship.RIndex].Count > 1)
                            {
                                result = false;
                            }
                        }
                    }
                }
            }
            if (relationship.AssocOnOneEdge.Multiplicity == Relationship.Multipricity.JustOne || relationship.AssocOnOneEdge.Multiplicity == Relationship.Multipricity.MoreThanOne)
            {
                if (currentFoS.Instances.ContainsKey(relationship.OtherEdge.EdgeInstance.KeyLetter))
                {
                    foreach (var otherInstId in currentFoS.Instances[relationship.OtherEdge.EdgeInstance.KeyLetter].Keys)
                    {
                        var otherInst = currentFoS.Instances[relationship.OtherEdge.EdgeInstance.KeyLetter][otherInstId];
                        if (otherInst.ParticipantLinks.ContainsKey(relationship.RIndex))
                        {
                            if (otherInst.ParticipantLinks[relationship.RIndex].Count() > 1)
                            {
                                if (relationship.AssocOnOneEdge.Multiplicity == Relationship.Multipricity.JustOne)
                                {
                                    result = false;
                                }
                            }
                            else if (otherInst.ParticipantLinks[relationship.RIndex].Count == 0)
                            {
                                result = false;
                            }
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
                if (relationship.AssocOnOneEdge.Multiplicity == Relationship.Multipricity.ZeroOrOne)
                {
                    if (currentFoS.Instances.ContainsKey(relationship.OtherEdge.EdgeInstance.KeyLetter))
                    {
                        foreach (var otherInstId in currentFoS.Instances[relationship.OtherEdge.EdgeInstance.KeyLetter].Keys)
                        {
                            var otherInst = currentFoS.Instances[relationship.OtherEdge.EdgeInstance.KeyLetter][otherInstId];
                            if (otherInst.ParticipantLinks.ContainsKey(relationship.RIndex))
                            {
                                if (otherInst.ParticipantLinks[relationship.RIndex].Count > 1)
                                {
                                    result = false;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        protected bool ValidateLinkBinaryRelationship(BinaryRelationship<ConceptualClass, ConceptualClass> relationship)
        {
            bool result = true;
            if (!currentFoS.Instances.ContainsKey(relationship.ReferentEdge.EdgeInstance.KeyLetter) &&
                !currentFoS.Instances.ContainsKey(relationship.ParticipantEdge.EdgeInstance.KeyLetter))
            {
                return result;
            }
            var referentProps = new List<string>(relationship.ReferentEdge.Properties);
            if (currentFoS.Instances.ContainsKey(relationship.ReferentEdge.EdgeInstance.KeyLetter))
            {
                foreach (var refInstId in currentFoS.Instances[relationship.ReferentEdge.EdgeInstance.KeyLetter].Keys)
                {
                    logger.LogInformation($" Validating instance : {refInstId} on the referent side in binary relationship...");
                    var referentInst = currentFoS.Instances[relationship.ReferentEdge.EdgeInstance.KeyLetter][refInstId];
                    var referentDProps = new List<DeterminedProperty>();
                    Instance participantInst = null;
                    foreach (var propName in referentProps)
                    {
                        referentDProps.Add(referentInst.DeterminedProperties[propName]);
                    }
                    var participantDProps = new List<DeterminedProperty>();

                    if (currentFoS.Instances.ContainsKey(relationship.ParticipantEdge.EdgeInstance.KeyLetter))
                    {
                        foreach (var candidateInstId in currentFoS.Instances[relationship.ParticipantEdge.EdgeInstance.KeyLetter].Keys)
                        {
                            logger.LogInformation($"  Checking instance : {candidateInstId} on the participant side in binary relationship...");
                            var candidateInst = currentFoS.Instances[relationship.ParticipantEdge.EdgeInstance.KeyLetter][candidateInstId];
                            var candidateProps = new List<DeterminedProperty>();
                            foreach (var propName in relationship.ParticipantEdge.Properties)
                            {
                                candidateProps.Add(candidateInst.DeterminedProperties[propName]);
                            }
                            if (referentDProps.Select(dp => dp.Value).SequenceEqual(candidateProps.Select(dp => dp.Value)))
                            {
                                participantDProps.Clear();
                                participantInst = candidateInst;
                                participantDProps.AddRange(candidateProps);

                                var link = new BinaryLink(currentFoS, relationship, referentInst, participantInst);
                                foreach (var (rdp, pdf) in referentDProps.Zip(participantDProps, (x, y) => (x, y)))
                                {
                                    link.AddFormedProperties(rdp, pdf);
                                }
                                referentInst.AddLink(link);
                                participantInst.AddLink(link);
                            }
                        }
                    }
                    if (relationship.ParticipantEdge.Multiplicity == Relationship.Multipricity.JustOne)
                    {
                        string detail = "";
                        if (!referentInst.DeterminedProperties.ContainsKey(relationship.RIndex))
                        {
                            detail = "no";
                        }
                        else
                        {
                            if (referentInst.ParticipantLinks[relationship.RIndex].Count == 0)
                            {
                                detail = "no";
                            }
                            else if (referentInst.ParticipantLinks[relationship.RIndex].Count > 1)
                            {
                                detail = "more than one";
                            }
                        }
                        if (!string.IsNullOrEmpty(detail))
                        {
                            result = false;
                            logger.LogInformation($"  - Relationship : {relationship.RIndex}");
                            logger.LogInformation($"    Participant Multiplicity is '1' but this instance has {detail} links.");
                        }
                    }
                    if (relationship.ParticipantEdge.Multiplicity == Relationship.Multipricity.ZeroOrOne)
                    {
                        if (referentInst.ParticipantLinks.ContainsKey(relationship.RIndex))
                        {
                            if (referentInst.ParticipantLinks[relationship.RIndex].Count > 1)
                            {
                                result = false;
                                logger.LogInformation($"  - Relationship : {relationship.RIndex}");
                                logger.LogInformation($"    Participant Multiplicity is '0..1' but this instance has more than one links.");
                            }
                        }
                    }
                    if (relationship.ParticipantEdge.Multiplicity == Relationship.Multipricity.MoreThanOne)
                    {

                        if (!referentInst.ParticipantLinks.ContainsKey(relationship.RIndex) ||
                            (referentInst.ParticipantLinks.ContainsKey(relationship.RIndex) && referentInst.ParticipantLinks[relationship.RIndex].Count == 0))
                        {
                            result = false;
                            logger.LogInformation($"  - Relationship : {relationship.RIndex}");
                            logger.LogInformation($"    Participant Multiplicity is '1..*' but this instance has no links.");
                        }
                    }
                }
            }


            // Validate from Participant Side.
            if (currentFoS.Instances.ContainsKey(relationship.ParticipantEdge.EdgeInstance.KeyLetter)
                && currentFoS.Instances.ContainsKey(relationship.ReferentEdge.EdgeInstance.KeyLetter))
            {
                foreach (var partInstId in currentFoS.Instances[relationship.ParticipantEdge.EdgeInstance.KeyLetter].Keys)
                {
                    var partInst = currentFoS.Instances[relationship.ParticipantEdge.EdgeInstance.KeyLetter][partInstId];
                    var partDProps = new List<DeterminedProperty>();
                    foreach (var propName in relationship.ParticipantEdge.Properties)
                    {
                        partDProps.Add(partInst.DeterminedProperties[propName]);
                    }

                    foreach (var refCandInstId in currentFoS.Instances[relationship.ReferentEdge.EdgeInstance.KeyLetter].Keys)
                    {
                        var refCandInst = currentFoS.Instances[relationship.ReferentEdge.EdgeInstance.KeyLetter][refCandInstId];
                        var refDProps = new List<DeterminedProperty>();
                        foreach (var propName in relationship.ReferentEdge.Properties)
                        {
                            refDProps.Add(refCandInst.DeterminedProperties[propName]);
                        }
                        if (partDProps.Select(dp => dp.Value).SequenceEqual(refDProps.Select(dp => dp.Value)))
                        {
                            bool found = false;
                            if (refCandInst.ParticipantLinks.ContainsKey(relationship.RIndex))
                            {
                                foreach (var link in refCandInst.ParticipantLinks[relationship.RIndex])
                                {
                                    var bLink = (BinaryLink)link;
                                    if (bLink.Participant.InstanceId == partInstId)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                            }
                            if (!found)
                            {
                                var link = new BinaryLink(currentFoS, relationship, refCandInst, partInst);
                                foreach (var (rdp, pdp) in refDProps.Zip(partDProps, (x, y) => (x, y)))
                                {
                                    link.AddFormedProperties(rdp, pdp);
                                }
                                refCandInst.AddLink(link);
                                partInst.AddLink(link);
                            }
                        }
                    }
                }
                foreach (var partInstId in currentFoS.Instances[relationship.ParticipantEdge.EdgeInstance.KeyLetter].Keys)
                {
                    var partInst = currentFoS.Instances[relationship.ParticipantEdge.EdgeInstance.KeyLetter][partInstId];
                    if (partInst.ParticipantLinks.ContainsKey(relationship.RIndex))
                    {
                        string detail = "";
                        var pLinks = partInst.ParticipantLinks[relationship.RIndex];
                        if (relationship.ReferentEdge.Multiplicity == Relationship.Multipricity.JustOne)
                        {
                            if (pLinks.Count == 0)
                            {
                                detail = "no";
                                result = false;
                            }
                            else if (pLinks.Count > 1)
                            {
                                detail = "more than one";
                                result = false;
                            }
                        }
                        else if (relationship.ReferentEdge.Multiplicity == Relationship.Multipricity.ZeroOrOne)
                        {
                            if (pLinks.Count > 1)
                            {
                                detail = "more than one";
                                result = false;
                            }
                        }
                        else if (relationship.ReferentEdge.Multiplicity == Relationship.Multipricity.MoreThanOne)
                        {
                            if (pLinks.Count == 0)
                            {
                                detail = "no";
                                result = false;
                            }
                        }
                        if (!string.IsNullOrEmpty(detail))
                        {
                            logger.LogInformation($"  The instance : '{partInstId}' has {detail} referent instance.");
                        }
                    }
                }
            }


            return result;
        }

        protected bool ValidateLinkIsARelationship(IsARelationship<ConceptualClass> relationship)
        {
            bool result = true;
            // Validation from Super side
            if (!currentFoS.Instances.ContainsKey(relationship.SuperEdge.KeyLetter))
            {
                return result;
            }
            foreach (var instId in currentFoS.Instances[relationship.SuperEdge.KeyLetter].Keys)
            {
                if (currentFoS.Instances.ContainsKey(relationship.SuperEdge.KeyLetter))
                {
                    if (currentFoS.Instances[relationship.SuperEdge.KeyLetter].ContainsKey(instId))
                    {
                        logger.LogInformation($" Validating instance : {instId} on super side in is-a relationship...");
                        Instance participantInst = null;
                        bool found = false;

                        var instance = currentFoS.Instances[relationship.SuperEdge.KeyLetter][instId];
                        var referentDProps = new List<DeterminedProperty>();
                        var participantProps = new List<DeterminedProperty>();
                        foreach (var formPropName in relationship.ReferentProperties)
                        {
                            referentDProps.Add(instance.DeterminedProperties[formPropName]);
                        }
                        foreach (var subEdgKeyLett in relationship.SubEdges.Keys)
                        {
                            var subEdgeInRel = relationship.SubEdges[subEdgKeyLett];
                            if (currentFoS.Instances.ContainsKey(subEdgeInRel.SubEdge.KeyLetter))
                            {
                                foreach (var candidateInstId in currentFoS.Instances[subEdgeInRel.SubEdge.KeyLetter].Keys)
                                {
                                    logger.LogInformation($" checking for instance : {candidateInstId} on the sub side in is-a relationship...");
                                    var candidateInst = currentFoS.Instances[subEdgeInRel.SubEdge.KeyLetter][candidateInstId];
                                    var candidateProps = new List<DeterminedProperty>();
                                    foreach (var formPropName in subEdgeInRel.Properties)
                                    {
                                        candidateProps.Add(candidateInst.DeterminedProperties[formPropName]);
                                    }
                                    if (referentDProps.Select(dp => dp.Value).SequenceEqual(candidateProps.Select(dp => dp.Value)))
                                    {
                                        if (found)
                                        {
                                            // Validation error
                                            result = false;
                                            break;
                                        }
                                        else
                                        {
                                            found = true;
                                            participantInst = candidateInst;
                                            participantProps.Clear();
                                            participantProps.AddRange(candidateProps);
                                        }
                                    }
                                }
                            }
                        }
                        if (found && result)
                        {
                            var link = new BinaryLink(currentFoS, relationship, instance, participantInst);
                            foreach (var (rdp, pdp) in referentDProps.Zip(participantProps, (x, y) => (x, y)))
                            {
                                link.AddFormedProperties(rdp, pdp);
                            }
                            instance.AddLink(link);
                            participantInst.AddLink(link);
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
                if (!result)
                {
                    break;
                }
            }

            // Validating from Sub side
            foreach (var subEdgeRId in relationship.SubEdges.Keys)
            {
                var subEdge = relationship.SubEdges[subEdgeRId];
                if (currentFoS.Instances.ContainsKey(subEdge.SubEdge.KeyLetter))
                {
                    foreach (var instId in currentFoS.Instances[subEdge.SubEdge.KeyLetter].Keys)
                    {
                        var subInst = currentFoS.Instances[subEdge.SubEdge.KeyLetter][instId];
                        var subProps = new List<string>(subEdge.Properties);
                        var subDProps = new List<DeterminedProperty>();
                        foreach (var propName in subProps)
                        {
                            subDProps.Add(subInst.DeterminedProperties[propName]);
                        }
                        var foundInsts = new Dictionary<string, Instance>();
                        var foundInstDprops = new Dictionary<string, List<DeterminedProperty>>();
                        if (currentFoS.Instances.ContainsKey(relationship.SuperEdge.KeyLetter))
                        {
                            foreach (var candidateInstId in currentFoS.Instances[relationship.SuperEdge.KeyLetter].Keys)
                            {
                                var candidateInst = currentFoS.Instances[relationship.SuperEdge.KeyLetter][(candidateInstId)];
                                var candidateDProps = new List<DeterminedProperty>();
                                foreach (var propName in relationship.ReferentProperties)
                                {
                                    candidateDProps.Add(candidateInst.DeterminedProperties[propName]);
                                }
                                if (subDProps.Select(dp => dp.Value).SequenceEqual(candidateDProps.Select(dp => dp.Value)))
                                {
                                    foundInsts.Add(candidateInstId, candidateInst);
                                    foundInstDprops.Add(candidateInstId, candidateDProps);
                                }
                            }
                        }
                        if (foundInstDprops.Count == 0)
                        {
                            logger.LogInformation($"  Super side instance of {instId} doesn't exist.");
                            result = false;
                        }
                        else if (foundInsts.Count == 1)
                        {
                            int noInPLinks = 0;
                            if (subInst.ParticipantLinks.ContainsKey(relationship.RIndex))
                            {
                                noInPLinks = subInst.ParticipantLinks[relationship.RIndex].Count;
                            }
                            var superInstId = foundInstDprops.Keys.First();
                            if (noInPLinks == 1)
                            {
                                var link = subInst.ParticipantLinks[relationship.RIndex][0];
                                if (link is BinaryLink)
                                {
                                    var bLink = (BinaryLink)link;
                                    if (bLink.Referent.InstanceId == superInstId)
                                    {
                                        // valid
                                    }
                                    else
                                    {
                                        result = false;
                                    }
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                            else if (noInPLinks == 0)
                            {
                                var link = new BinaryLink(currentFoS, relationship, foundInsts[superInstId], subInst);
                                foreach (var (rdp, pdp) in foundInstDprops[superInstId].Zip(subDProps, (x, y) => (x, y)))
                                {
                                    link.AddFormedProperties(rdp, pdp);
                                }
                                subInst.AddLink(link);
                                foundInsts[superInstId].AddLink(link);
                            }
                        }
                        else if (foundInsts.Count > 1)
                        {
                            result = false;
                        }
                    }
                }
            }

            return result;
        }

        protected InstanceRepository currentRepository = null;
        protected JObject validatedDescriptionJson = null;
        protected FieldOfSense currentFoS = null;

        public ILogger Logger { get => logger; set => logger = value; }

        protected ILogger logger;
    }
}
