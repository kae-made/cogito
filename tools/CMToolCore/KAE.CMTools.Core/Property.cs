// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using KAE.CMTools.Core.DataType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KAE.CMTools.Core.DataType.DataType;

namespace KAE.CMTools.Core
{
    public class Property
    {
        public string Name { get => name; }
        public DataType.DataType DataType { get => dataType; }
        public string Description { get => description; }

        public bool IsDenote { get => isDenote; }
        public bool IsMethematical { get=> isMethematical; }
        public bool IsNullable {  get => isNullable; set => isNullable = value; }
        public string Grammar { get => grammar; }

        public DataType.DataType BaseDataType { get=> baseDataType; set => baseDataType = value; }

        public bool IsParticipantProperty()
        {
            if (this.dataType is PrimitiveDataType)
            {
                if (this.dataType.Name== DataTypeKind.REFERENCE.ToString())
                {
                    return true;
                }
            }
            return false;
        }


        public Property(string name, DataType.DataType dataType, string description = null)
        {
            this.name = name;
            this.dataType = dataType;
            this.description = description;
            this.isNullable= false;
        }

        public Property(string name, DataType.DataType dataType, bool isDenote, string description=null) : this(name, dataType, description)
        {
            this.isMethematical = isDenote;
        }

        public Property(string name, DataType.DataType dataType, bool isDenote,  bool isMathematical, string grammar=null, string description=null):this(name, dataType, description)
        {
            this.isMethematical = isMathematical;
            this.grammar = grammar;
            this.isDenote = isDenote;
        }

        public void AddReferentProperty(string cclassKeyLett, Property referentProperty)
        {
            referentProperties.Add(new KeyValuePair<string, Property>(cclassKeyLett, referentProperty));
        }

        public DataType.DataType FixBaseDataType()
        {

            if (this.IsParticipantProperty())
            {
                foreach(var classKeyProp in referentProperties)
                {
                    var referentDataType = classKeyProp.Value.FixBaseDataType();
                    if (referentDataType != null)
                    {
                        if (referentDataType.Name != DataTypeKind.REFERENCE.ToString())
                        {
                            BaseDataType = referentDataType;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (this.baseDataType == null)
                {
                    if (this.dataType.Name== DataTypeKind.UNIQUE_ID.ToString())
                    {
                        this.baseDataType = PrimitiveDataType.GetPrimitiveDataTypes()[DataTypeKind.STRING];
                    }
                    else if (this.dataType.Name == DataTypeKind.OTHER.ToString())
                    {
                        this.baseDataType = PrimitiveDataType.GetPrimitiveDataTypes()[DataTypeKind.STRING];
                    }
                    else
                    {
                        this.baseDataType = this.dataType;
                    }
                }
            }

            return baseDataType;
        }

        protected string name;
        protected DataType.DataType dataType;
        protected string description = null;
        protected bool isMethematical = false;
        protected string grammar = null;
        protected bool isDenote = false;
        protected bool isNullable = false;
        protected DataType.DataType baseDataType = null;
        protected List<KeyValuePair<string, Property>> referentProperties = new List<KeyValuePair<string, Property>>();
    }

    public class PropertyRef<T, TProp>
        where T : ConceptualClass
        where TProp : Property
    {
        public Func<T, TProp> Getter { get; }
        public Action<T, TProp> Setter { get; }
        public PropertyRef(Func<T, TProp> getter, Action<T, TProp> setter)
        {
            Getter = getter;
            Setter = setter;
        }
    }

    public class PropertyPair<TA, TB, TProp>
        where TA : ConceptualClass
        where TB : ConceptualClass
        where TProp : Property
    {
        public PropertyRef<TA, TProp> A { get; }
        public PropertyRef<TB, TProp> B { get; }
        public PropertyPair(PropertyRef<TA, TProp> a, PropertyRef<TB, TProp> b)
        {
            A = a;
            B = b;
        }

    }
}
