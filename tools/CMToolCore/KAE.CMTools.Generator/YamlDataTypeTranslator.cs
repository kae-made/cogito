using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KAE.CMTools.Core.DataType.DataType;

namespace KAE.CMTools.Generator
{
    internal class YamlDataTypeTranslator
    {
        public static (string typeName, string format) ToYamlTypeName(DataTypeKind dataTypeKind)
        {
            var yamlType = yamlTypes[dataTypeKind];
            return (yamlType.Key, yamlType.Value);
        }

        public static object ConvertJValueToObject(JValue jv)
        {
            switch (jv.Type)
            {
                case JTokenType.String:
                    return (string)jv;

                case JTokenType.Integer:
                    return (long)jv;   // JSON の整数は long になる

                case JTokenType.Float:
                    return (double)jv;

                case JTokenType.Boolean:
                    return (bool)jv;

                case JTokenType.Null:
                    return null;

                default:
                    return jv.Value;
            }
        }

        private static Dictionary<DataTypeKind, KeyValuePair<string, string>> yamlTypes = new Dictionary<DataTypeKind, KeyValuePair<string, string>>(){
            {DataTypeKind.UNIQUE_ID, new KeyValuePair<string, string>( "string","uuid" )  },
            {DataTypeKind.STRING, new KeyValuePair<string,string>( "string","") },
            {DataTypeKind.BOOLEAN, new KeyValuePair<string, string>("boolean", "") },
            {DataTypeKind.INTEGER, new KeyValuePair<string, string>( "integer", "") },
            {DataTypeKind.REAL, new KeyValuePair<string, string>( "number","") },
            {DataTypeKind.TIMESTAMP, new KeyValuePair<string,string>("string", "date-time") },
            {DataTypeKind.TIMESPAN, new KeyValuePair<string, string>("string", "duration") },
            {DataTypeKind.OTHER, new KeyValuePair<string, string>("object", "") },
            {DataTypeKind.UNKNOWN, new KeyValuePair<string, string>("string","") }
        };
    }
}
