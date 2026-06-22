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
        public string Grammer { get => Grammer; }

        public bool IsReferenDataType()
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
        }

        public Property(string name, DataType.DataType dataType, bool isDenote, string description=null) : this(name, dataType, description)
        {
            this.isMethematical = isDenote;
        }

        public Property(string name, DataType.DataType dataType, bool isDenote,  bool isMathematical, string grammer=null, string description=null):this(name, dataType, description)
        {
            this.isMethematical = isMathematical;
            this.grammer = grammer;
            this.isDenote = isDenote;
        }


        protected string name;
        protected DataType.DataType dataType;
        protected string description = null;
        protected bool isMethematical = false;
        protected string grammer = null;
        protected bool isDenote = false;
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
