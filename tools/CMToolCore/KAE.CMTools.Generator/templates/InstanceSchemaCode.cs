using KAE.CMTools.Core;
using KAE.CMTools.Core.DataType;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KAE.CMTools.Core.DataType.DataType;

namespace KAE.CMTools.Generator.templates
{
    public partial class InstanceSchema
    {
        ConceptualDomain domain;

        public InstanceSchema(ConceptualDomain domain)
        {
            this.domain = domain;
        }

        public string GetCClassKeyLettList()
        {
            string result = "";
            foreach (var keyLett in domain.ConceptualClasses.Keys)
            {
                var cclass = domain.ConceptualClasses[keyLett];
                if (!string.IsNullOrEmpty(result))
                {
                    result += ", ";
                }
                result += keyLett;
            }
            return result;
        }

        public string GetPropertyNameList(ConceptualClass cclass)
        {
            string result = "";
            foreach(var propertyName in  cclass.Properties.Keys)
            {
                var property = cclass.Properties[propertyName];
                if (!property.IsMethematical)
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        result += ", ";
                    }
                    result += propertyName;
                }
            }
            return result;
        }

        private void prototypeMappingDef()
        {
            foreach (var keyLett in domain.ConceptualClasses.Keys)
            {
                var cclass = domain.ConceptualClasses[keyLett];
                string name = cclass.Name;
                string number = cclass.Number;

                string propertyNameList = GetPropertyNameList(cclass);

                foreach (var propName in cclass.Properties.Keys)
                {
                    var property = (Property)cclass.Properties[propName];
                    if (!property.IsMethematical)
                    {
                        bool isDenote = property.IsDenote;
                        string grammer = property.Grammar;
                        var dataType = property.BaseDataType;
                        string dataTypeName = dataType.Name;
                        string format = "";
                        if (dataType is PrimitiveDataType)
                        {
                            (dataTypeName, format) = YamlDataTypeTranslator.ToYamlTypeName(((PrimitiveDataType)dataType).Kind);
                        }
                        if (property.DataType.Name == DataTypeKind.UNIQUE_ID.ToString()
                            || property.DataType.Name == DataTypeKind.OTHER.ToString()
                            || property.DataType.Name == DataTypeKind.REFERENCE.ToString()
                            || property.DataType.Name==DataTypeKind.UNKNOWN.ToString())
                        {
                            dataTypeName += $" # {property.DataType.Name}";
                        }
                        if (!string.IsNullOrEmpty(format))
                        {

                        }
                        if (!string.IsNullOrEmpty(grammer))
                        {

                        }
                    }

                }
            }
        }
    }
}
