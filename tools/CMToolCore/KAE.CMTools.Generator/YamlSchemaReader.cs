// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using KAE.CMTools.Core;
using KAE.CMTools.Core.DataType;
using KAE.CMTools.Generator.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Reflection.Metadata.Ecma335;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static KAE.CMTools.Core.Relationship;

namespace KAE.CMTools.Generator
{
    public class YamlSchemaReader : SchemaReader
    {
        public void Parse(InstanceRepository repository)
        {
            if (validDescripJson != null)
            {
                foreach (var prop in validDescripJson)
                {
                    if (prop.Key == "domain")
                    {
                        currentRepository = repository;
                        var domainValue = prop.Value;
                        ConceptualDomain parsedDomain = null;

                        foreach (var domainProp in (JObject)domainValue)
                        {
                            if (domainProp.Key == "name")
                            {
                                string domainName = (string)domainProp.Value;
                                parsedDomain = repository.AddConceptualDomain(domainName);
                            }
                            else if (domainProp.Key == "datatypes")
                            {
                                JObject datatypes=(JObject)domainProp.Value;
                                foreach (var datatypesProp in datatypes.Properties())
                                {
                                    if (datatypesProp.Value.Type == JTokenType.Array)
                                    {
                                        foreach(var datatype in (JArray)datatypesProp.Value)
                                        {
                                            var parsedDataType = ParseDatatype(parsedDomain, (JObject)datatype);
                                            parsedDomain.AddDataType(parsedDataType);
                                        }
                                    }
                                }
                            }
                            else if (domainProp.Key == "cclasses")
                            {
                                JObject cclasses = (JObject)domainProp.Value;
                                foreach (var cclassesProp in cclasses.Properties())
                                {
                                    if (cclassesProp.Value.Type == JTokenType.Array)
                                    {
                                        foreach (var cclass in (JArray)cclassesProp.Value)
                                        {
                                            var parsedCClass = ParseConceptualClass(parsedDomain, (JObject)cclass);
                                        }
                                    }
                                }

                            }
                            else if (domainProp.Key == "relationships")
                            {
                                JObject relationships = (JObject)domainProp.Value;
                                foreach (var relationshipProp in relationships.Properties())
                                {
                                    if (relationshipProp.Value.Type == JTokenType.Array)
                                    {
                                        foreach (var relationship in (JArray)relationshipProp.Value)
                                        {
                                            var parsedRelationship = ParseRelationship(parsedDomain, (JObject)relationship);
                                        }
                                    }
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                }
            }
        }

        private DataType ParseDatatype(ConceptualDomain domain, JObject dataTypeDef)
        {
            string dataTypeName = "";
            DataType parsedDataType = null;
            foreach (var dtProp in dataTypeDef)
            {
                if (dtProp.Key == "name")
                {
                    dataTypeName = (string)dtProp.Value;
                    break;
                }
            }
            string dataTypeKind = "";
            foreach (var dtProp in dataTypeDef)
            {
                if (dtProp.Key == "kind")
                {
                    dataTypeKind = (string)dtProp.Value;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(dataTypeName) && !string.IsNullOrEmpty(dataTypeKind))
            {
                if (dataTypeKind == "primitive")
                {
                    parsedDataType = ParsePrimitiveDataType(dataTypeName, dataTypeDef);
                }
                else if (dataTypeKind == "enumeration")
                {
                    foreach(var dtProp in dataTypeDef)
                    {
                        if (dtProp.Key == "items")
                        {
                            parsedDataType = ParseEnumerationDataType(dataTypeName, (JArray)dtProp.Value);
                            break;
                        }
                    }
                }
                else if (dataTypeKind == "complex")
                {
                    foreach(var dtProp in dataTypeDef)
                    {
                        if (dtProp.Key == "items")
                        {
                            parsedDataType = ParseComplexDataType(domain, dataTypeName, (JArray)dtProp.Value);
                        }
                    }
                }
            }
            return parsedDataType;
        }

        private PrimitiveDataType ParsePrimitiveDataType(string pdtName, JObject pdtDef)
        {
            PrimitiveDataType parsedPDT= null;
            
            foreach(var dtProp in pdtDef)
            {
                if (dtProp.Key == "basetype")
                {
                    DataType.DataTypeKind baseKind = (DataType.DataTypeKind)Enum.Parse(typeof(DataType.DataTypeKind), (string)dtProp.Value);
                    parsedPDT = new PrimitiveDataType(pdtName, baseKind);
                    break;
                }
            }
            if (parsedPDT != null)
            {
                foreach (var dtProp in pdtDef)
                {
                    if (dtProp.Key == "pattern")
                    {
                        parsedPDT.Pattern = (string)dtProp.Value;
                    }
                    else if (dtProp.Key == "unit")
                    {
                        parsedPDT.Unit = (string)dtProp.Value;
                    }
                }
            }
            return parsedPDT;
        }
        private EnumerationDataType ParseEnumerationDataType(string edtName, JArray itemsDef)
        {
            EnumerationDataType parsedEDT= null;
            var items = new List<string>();
            foreach (var itemDef in itemsDef)
            {
                string itemName = (string)itemDef;
                items.Add(itemName);
            }
            parsedEDT = new EnumerationDataType(edtName, items);

            return parsedEDT;
        }

        private ComplexDataType ParseComplexDataType(ConceptualDomain domain, string cdtName, JArray cdtDef)
        {
            ComplexDataType parsedCDT = null;
            var children = new Dictionary<string, DataType>();
            foreach (var itemDef in cdtDef)
            {
                string childName = "";
                string typeName = "";
                foreach (var childDef in itemDef)
                {
                    var child = (JProperty)childDef;
                    if (child.Name == "name")
                    {
                        childName = (string)child.Value;
                    }
                    else if (child.Name == "type")
                    {
                        typeName = (string)child.Value;
                    }
                    if (!string.IsNullOrEmpty(childName) && !string.IsNullOrEmpty(typeName))
                    {
                        var dataType = domain.DataTypes[typeName];
                        children.Add(childName, dataType);
                        break;
                    }
                }
            }
            parsedCDT = new ComplexDataType(cdtName, children);

            return parsedCDT;
        }

        private ConceptualClass ParseConceptualClass(ConceptualDomain domain, JObject conceptualClassDef)
        {
            string cclassName = "";
            string keyLetter = "";
            string number = "";
            string description = null;
            foreach (var cclassDef in conceptualClassDef)
            {
                if (cclassDef.Key == "name")
                {
                    cclassName = (string)cclassDef.Value;
                }
                else if (cclassDef.Key == "key_letter")
                {
                    keyLetter = (string)cclassDef.Value;
                }
                else if (cclassDef.Key == "number")
                {
                    number = (string)cclassDef.Value;
                }
                else if (cclassDef.Key == "description")
                {
                    description = (string)cclassDef.Value;
                }
            }

            if (string.IsNullOrEmpty(cclassName) || string.IsNullOrEmpty(keyLetter) || string.IsNullOrEmpty(number))
            {

            }

            logger.LogInformation($"Parsing Conceptual Class - '{cclassName}" + "{" + $"{keyLetter}, {number}" + "}'");
            var parsedCClass = new ConceptualClassBase(domain, cclassName, keyLetter, number, description);

            var identities = new Dictionary<int, Dictionary<string, Property>>();

            foreach (var cclassDef in conceptualClassDef)
            {
                if (cclassDef.Key == "properties")
                {
                    foreach (var propsDef in ((JObject)cclassDef.Value).Properties())
                    {
                        if (propsDef.Value.Type == JTokenType.Array)
                        {
                            foreach (var propDef in (JArray)propsDef.Value)
                            {
                                string name = "";
                                string dataTypeName = "";
                                description = null;
                                bool isMath = false;
                                string grammar = null;
                                bool isDenote = false;
                                bool nullable = false;
                                // parsing property description
                                foreach (var aPropDef in (JObject)propDef)
                                {
                                    if (aPropDef.Key == "name")
                                    {
                                        name = (string)aPropDef.Value;
                                    }
                                    else if (aPropDef.Key == "description")
                                    {
                                        description = (string)aPropDef.Value;
                                    }
                                    else if (aPropDef.Key == "type")
                                    {
                                        dataTypeName = (string)aPropDef.Value;
                                    }
                                    else if (aPropDef.Key == "mathematical")
                                    {
                                        isMath = (bool)aPropDef.Value;
                                    }
                                    else if (aPropDef.Key == "grammar")
                                    {
                                        grammar = (string)aPropDef.Value;
                                    }
                                    else if (aPropDef.Key == "denote")
                                    {
                                        isDenote = (bool)aPropDef.Value;
                                    }
                                    else if (aPropDef.Key == "nullable")
                                    {
                                        nullable = (bool)aPropDef.Value;
                                    }
                                }
                                DataType dataType = null;
                                if (!string.IsNullOrEmpty(dataTypeName))
                                {
                                    // dataType = PrimitiveDataType.GetPrimitiveDataTypes().Where(kv => kv.Key.ToString() == dataTypeName).FirstOrDefault().Value;
                                    dataType = domain.DataTypes[dataTypeName];
                                }
                                var parsedProperty = new Property(name, dataType, isDenote, isMath, grammar, description);
                                if (nullable)
                                {
                                    parsedProperty.IsNullable = true;
                                }

                                parsedCClass.AddProperty(parsedProperty);

                                foreach (var aPropDef in (JObject)propDef)
                                {
                                    if (aPropDef.Key == "identity")
                                    {
                                        foreach (var idDef in (JArray)aPropDef.Value)
                                        {
                                            int idLevel = (int)idDef;
                                            parsedCClass.AddIdentity(idLevel, parsedProperty);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            domain.AddConceptualClass(parsedCClass);

            return parsedCClass;
        }

        private Relationship ParseRelationship(ConceptualDomain domain, JObject relationshipDef)
        {
            Relationship parsedRelationship = null;
            int relKind = 0; // 1->binary,2->is-a,3->binary-associative
            string rIndex = "";

            foreach (var relDef in relationshipDef)
            {
                if (relDef.Key == "index")
                {
                    rIndex = (string)relDef.Value;
                }
                else if (relDef.Key == "kind")
                {
                    string relKindDef = (string)relDef.Value;
                    if (relKindDef == "binary")
                    {
                        relKind = 1;
                    }
                    else if (relKindDef == "is-a")
                    {
                        relKind = 2;
                    }
                    else if (relKindDef == "binary-associative")
                    {
                        relKind = 3;
                    }

                }
                if ((!string.IsNullOrEmpty(rIndex)) && relKind > 0)
                {
                    break;
                }
            }

            logger.LogInformation($"Parsing Relationship - {rIndex}...");

            Func<JObject, (string, string, string)> ParseEdge = token =>
            {
                string keyLetter = "";
                string mult = "";
                string phrase = "";
                foreach (var edgeDef in token)
                {
                    if (edgeDef.Key == "cclass")
                    {
                        keyLetter = (string)edgeDef.Value;
                    }
                    else if (edgeDef.Key == "multiplicity")
                    {
                        mult = (string)edgeDef.Value;
                    }
                    else if (edgeDef.Key == "phrase")
                    {
                        phrase = (string)edgeDef.Value;
                    }
                }
                return (keyLetter, mult, phrase);
            };

            Action<string, string, string> ShowRelationshipError = (rIndex, keyLett, reason) =>
            {
                logger.LogInformation("Description Error :");
                logger.LogInformation($" - in Relationship : {rIndex}");
                logger.LogInformation($"   '{keyLett}' {reason}.");

            };

            if (relKind == 1)
            {
                ConceptualClass refClass = null;
                ConceptualClass partClass = null;

                string refKeyLetter = "";
                string refMult = "";
                string refPhrase = "";
                string partKeyLetter = "";
                string partMult = "";
                string partPhrase = "";
                foreach (var relDef in relationshipDef)
                {
                    if (relDef.Key == "referent")
                    {
                        (refKeyLetter, refMult, refPhrase) = ParseEdge((JObject)relDef.Value);
                    }
                    else if (relDef.Key == "participant")
                    {
                        (partKeyLetter, partMult, partPhrase) = ParseEdge((JObject)relDef.Value);
                    }
                }
                if (!domain.ConceptualClasses.ContainsKey(refKeyLetter))
                {
                    ShowRelationshipError(rIndex, refKeyLetter, "undefined");
                }
                if (!domain.ConceptualClasses.ContainsKey(partKeyLetter))
                {
                    ShowRelationshipError(rIndex, partKeyLetter, "undefined");
                }
                var refClassDef = domain.ConceptualClasses[refKeyLetter];
                var partClassDef = domain.ConceptualClasses[partKeyLetter];
                Multipricity refMulti = Relationship.ToMultiplicity(refMult);
                Multipricity partMulti = Relationship.ToMultiplicity(partMult);

                var relProps = new List<string>();
                var partProps = new List<string>();

                foreach (var relDef in relationshipDef)
                {
                    if (relDef.Key == "referencing")
                    {
                        foreach (var propsDef in (JArray)relDef.Value)
                        {
                            foreach (var propDef in propsDef)
                            {
                                var propPair = (JProperty)propDef;
                                relProps.Add(propPair.Name);
                                partProps.Add((string)((JValue)propPair.Value).Value);
                            }
                        }
                        break;
                    }
                }

                var binaryRelationship = new BinaryRelationship<ConceptualClass, ConceptualClass>(rIndex, refClassDef, refMulti, refPhrase, relProps, partClassDef, partMulti, partPhrase, partProps);
                domain.AddRelationship(binaryRelationship);
                parsedRelationship = binaryRelationship;
            }
            else if (relKind == 2)
            {
                ConceptualClass superClass = null;
                List<string> superProps = new List<string>();
                List<ConceptualClass> subClasses = new List<ConceptualClass>();
                Dictionary<string, List<string>> subsProps = new Dictionary<string, List<string>>();
                foreach (var relDef in relationshipDef)
                {
                    if (relDef.Key == "referent")
                    {
                        foreach (var superDef in (JObject)relDef.Value)
                        {
                            if (superDef.Key == "cclass")
                            {
                                string keyLett = (string)superDef.Value;
                                if (!domain.ConceptualClasses.ContainsKey(keyLett))
                                {
                                    ShowRelationshipError(rIndex, keyLett, "undefined");
                                }
                                superClass = domain.ConceptualClasses[keyLett];
                                break;
                            }
                        }
                        foreach (var superDef in (JObject)relDef.Value)
                        {
                            if (superDef.Key == "properties")
                            {
                                foreach (var propDef in (JArray)superDef.Value)
                                {
                                    superProps.Add((string)propDef);
                                }
                            }
                        }
                    }
                    else if (relDef.Key == "participants")
                    {
                        foreach (var subsDef in (JArray)relDef.Value)
                        {
                            ConceptualClass subClass = null;
                            List<string> subProps = new List<string>();
                            foreach (var subDef in (JObject)subsDef)
                            {
                                if (subDef.Key == "cclass")
                                {
                                    string keyLett = (string)subDef.Value;
                                    if (!domain.ConceptualClasses.ContainsKey(keyLett))
                                    {
                                        ShowRelationshipError(rIndex, keyLett, "undefined");
                                    }
                                    subClass = domain.ConceptualClasses[keyLett];
                                }
                                else if (subDef.Key == "properties")
                                {
                                    foreach (var propDef in (JArray)subDef.Value)
                                    {
                                        subProps.Add((string)propDef);
                                    }
                                }
                            }
                            if (subsProps.ContainsKey(subClass.KeyLetter))
                            {
                                ShowRelationshipError(rIndex, subClass.KeyLetter, "already defined");
                            }
                            subClasses.Add(subClass);
                            subsProps.Add(subClass.KeyLetter, subProps);
                        }
                    }
                }
                var isARelationship = new IsARelationship<ConceptualClass>(rIndex, superClass, superProps);
                foreach (var subClass in subClasses)
                {
                    isARelationship.AddSubEdge(subClass, subsProps[subClass.KeyLetter]);
                }
                domain.AddRelationship(isARelationship);
                parsedRelationship = isARelationship;
            }
            else if (relKind == 3)
            {
                ConceptualClass oneClass = null;
                ConceptualClass otherClass = null;
                ConceptualClass assocClass = null;
                string oneMult = "";
                string onePhrase = "";
                string otherMult = "";
                string otherPhrase = "";
                List<string> oneProps = new List<string>();
                List<string> otherProps = new List<string>();
                List<string> assocOnOneProps = new List<string>();
                List<string> assocOnOtherProps = new List<string>();
                foreach (var relDef in relationshipDef)
                {
                    if (relDef.Key == "one")
                    {
                        string keyLett;
                        (keyLett, oneMult, onePhrase) = ParseEdge((JObject)relDef.Value);
                        if (!domain.ConceptualClasses.ContainsKey(keyLett))
                        {
                            ShowRelationshipError(rIndex, keyLett, "has not defined.");
                        }
                        oneClass = domain.ConceptualClasses[keyLett];
                    }
                    else if (relDef.Key == "other")
                    {
                        string keyLett;
                        (keyLett, otherMult, otherPhrase) = ParseEdge((JObject)relDef.Value);
                        if (!domain.ConceptualClasses.ContainsKey(keyLett))
                        {
                            ShowRelationshipError(rIndex, keyLett, "has not defined.");
                        }
                        otherClass = domain.ConceptualClasses[keyLett];
                    }
                    else if (relDef.Key == "associative")
                    {
                        foreach (var assocDef in (JObject)relDef.Value)
                        {
                            if (assocDef.Key == "cclass")
                            {
                                string keyLett = (string)assocDef.Value;
                                if (!domain.ConceptualClasses.ContainsKey(keyLett))
                                {
                                    ShowRelationshipError(rIndex, keyLett, "has not defined.");
                                }
                                assocClass = domain.ConceptualClasses[keyLett];
                            }
                        }
                    }
                    else if (relDef.Key == "referencing")
                    {
                        foreach (var refDef in (JObject)relDef.Value)
                        {
                            if (refDef.Key == "one")
                            {
                                foreach (var pairDef in (JArray)refDef.Value)
                                {
                                    foreach (var propDef in pairDef)
                                    {
                                        var propPair = (JProperty)propDef;
                                        oneProps.Add(propPair.Name);
                                        assocOnOneProps.Add((string)((JValue)propPair.Value).Value);
                                    }
                                }
                            }
                            else if (refDef.Key == "other")
                            {
                                foreach (var pairDef in (JArray)refDef.Value)
                                {
                                    foreach (var propDef in pairDef)
                                    {
                                        var propPair = (JProperty)propDef;
                                        otherProps.Add(propPair.Name);
                                        assocOnOtherProps.Add((string)((JValue)propPair.Value).Value);
                                    }
                                }
                            }
                        }
                    }
                }

                if (oneClass != null && otherClass != null && assocClass != null)
                {
                    var binAssocRelationship = new AssociativeRelationship<ConceptualClass, ConceptualClass, ConceptualClass>(
                        rIndex, oneClass, Relationship.ToMultiplicity(oneMult), onePhrase, oneProps, assocOnOneProps,
                        otherClass, Relationship.ToMultiplicity(otherMult), otherPhrase, otherProps, assocClass, assocOnOtherProps);
                    domain.AddRelationship(binAssocRelationship);
                    parsedRelationship = binAssocRelationship;
                }
            }

            return parsedRelationship;
        }

        protected InstanceRepository currentRepository = null;

        protected void TraverseShcemaDefinition(JToken token)
        {

        }

        public bool Read(Stream schemaStream, Stream descripStream)
        {
            var validator = new YamlValidator(schemaStream, descripStream) { Logger = logger };
            logger.LogInformation("Validating...");
            if (validator.Validate())
            {
                logger.LogInformation("Validated.");
                validDescripJson = validator.ValidatedDescripJson;
                return true;
            }
            else
            {
                validator.ShowErrors();
            }
            return false;
        }

        public void ReadOld(Stream schemaStream, Stream descripStream)
        {
            string schemaText = "";
            string descripYamlText = "";
            using (var reader = new StreamReader(schemaStream))
            {
                schemaText = reader.ReadToEnd();
            }
            using (var reader = new StreamReader(descripStream))
            {
                descripYamlText = reader.ReadToEnd();
            }

            IList<string> erros = new List<string>();

            var deserializer = new DeserializerBuilder().
                WithNamingConvention(CamelCaseNamingConvention.Instance).
                WithAttemptingUnquotedStringTypeDeserialization().
                Build();

            try
            {
                var descripYamlObject = deserializer.Deserialize<object>(descripYamlText);
                string descripJsonYaml = JsonConvert.SerializeObject(descripYamlObject);
                try
                {
                    var schemaObject = deserializer.Deserialize<object>(schemaText);
                    var schemaJsonText = JsonConvert.SerializeObject(schemaObject);

                    try
                    {
                        JSchema schema = JSchema.Parse(schemaJsonText);

                        JObject parsedDescripYamlObject = JObject.Parse(descripJsonYaml);

                        var validated = parsedDescripYamlObject.IsValid(schema, out erros);
                        if (!validated)
                        {
                            var yamlFormatErrors = new List<YamlFormatError>();

                            foreach (var error in erros)
                            {
                                string posStr = "position ";
                                var pos = error.LastIndexOf(posStr);
                                var periodPos = error.LastIndexOf('.');
                                var posPos = error.Substring(pos + posStr.Length, periodPos - pos - posStr.Length);
                                int errorPosition = int.Parse(posPos);
                                yamlFormatErrors.Add(new YamlFormatError() { Message = error, JsonText = descripJsonYaml.Substring(errorPosition, 50) });
                            }
                            throw new YamlSchemaValidationException(yamlFormatErrors);
                        }
                        validDescripJson = parsedDescripYamlObject;
                    }
                    catch (Newtonsoft.Json.Schema.JSchemaReaderException ex)
                    {
                        string missingDescrip = "..." + schemaJsonText.Substring(ex.LinePosition, 100) + "...";
                        throw new YamlFormatSyntaxValidationException(ex, missingDescrip);
                    }
                }
                catch (YamlDotNet.Core.SyntaxErrorException ex)
                {
                    throw new YamlSyntaxValidationException(ex, YamlSyntaxValidationException.Kind.Format);
                }
            }
            catch (YamlDotNet.Core.SyntaxErrorException ex)
            {
                throw new YamlSyntaxValidationException(ex, YamlSyntaxValidationException.Kind.Schema);
            }

        }

        public bool Validate()
        {
            bool result = true;

            foreach (var domainName in currentRepository.ConceptualDomains.Keys)
            {
                var domain = currentRepository.ConceptualDomains[domainName];
                logger.LogInformation($"Validating Domain : '{domain.Name}' ...");
                foreach (var relIndex in domain.Relationships.Keys)
                {
                    var relationship = domain.Relationships[relIndex];
                    logger.LogInformation($" Validating Relationship - {relationship.RIndex}...");
                    result = relationship.Validate(logger);
                    if (result == false)
                    {
                        logger.LogInformation("Validation Failed.");
                        break;
                    }
                }

                foreach (var cclassKeyLett in domain.ConceptualClasses.Keys)
                {
                    var cclass = domain.ConceptualClasses[cclassKeyLett];
                    foreach (var propName in cclass.Properties.Keys)
                    {
                        var property = cclass.Properties[propName];
                        property.FixBaseDataType();
                    }
                }
            }

            if (result)
            {

                logger.LogInformation("Validation Done without any problem!");
            }

            return result;
        }

        protected JObject validDescripJson = null;

        public ILogger Logger { get => logger; set => logger = value; }
        protected ILogger logger;
    }

    public class YamlFormatError
    {
        public string Message { get; set; }
        public string JsonText { get; set; }
    }

    public class YamlSyntaxValidationException : Exception
    {
        public enum Kind
        {
            Schema,
            Format
        }

        public Exception innerException { get; set; }

        public Kind YamlKind { get; set; }

        public YamlSyntaxValidationException(Exception ex, Kind kind) : base(ex.Message)
        {
            this.YamlKind = kind;
            this.innerException = ex;
        }
    }

    public class YamlSchemaValidationException : Exception
    {
        public IList<YamlFormatError> Errors { get => erros; }

        public YamlSchemaValidationException(IList<YamlFormatError> Errors) : base("This YAML file is not compliant with the schema")
        {
            erros = new List<YamlFormatError>(Errors);
        }

        private IList<YamlFormatError> erros;
    }

    public class YamlFormatSyntaxValidationException: Exception
    {
        public string InvalidDefiniton { get; set; }

        public YamlFormatSyntaxValidationException(Exception ex, string invalidDefinition) : base(ex.Message)
        {
            this.InvalidDefiniton = invalidDefinition;
        }
    }
}
