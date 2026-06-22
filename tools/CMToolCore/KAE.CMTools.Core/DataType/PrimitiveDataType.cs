// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core.DataType
{
    public class PrimitiveDataType : DataType
    {
        public DataTypeKind Kind { get => dataTypeKind; }

        protected DataTypeKind dataTypeKind;

        public PrimitiveDataType(DataTypeKind kind) : base(kind.ToString())
        {
        }

        public static Dictionary<DataTypeKind, PrimitiveDataType> GetPrimitiveDataTypes()
        {
            if (primitiveDataTypes == null)
            {
                primitiveDataTypes = new Dictionary<DataTypeKind, PrimitiveDataType>()
                {
                    { DataTypeKind.UNIQUE_ID, new PrimitiveDataType(DataTypeKind.UNIQUE_ID) },
                    { DataTypeKind.STRING, new PrimitiveDataType(DataTypeKind.STRING) },
                    { DataTypeKind.INTEGER, new PrimitiveDataType(DataTypeKind.INTEGER) },
                    { DataTypeKind.REAL, new PrimitiveDataType(DataTypeKind.REAL) },
                    { DataTypeKind.BOOLEAN, new PrimitiveDataType(DataTypeKind.BOOLEAN) },
                    { DataTypeKind.TIMESTAMP, new PrimitiveDataType(DataTypeKind.TIMESTAMP) },
                    { DataTypeKind.TIMESPAN, new PrimitiveDataType(DataTypeKind.TIMESPAN) },
                    { DataTypeKind.REFERENCE, new PrimitiveDataType(DataTypeKind.REFERENCE) },
                    { DataTypeKind.INSTANCE, new PrimitiveDataType(DataTypeKind.INSTANCE) },
                    { DataTypeKind.INSTANCE_SET, new PrimitiveDataType(DataTypeKind.INSTANCE_SET) },
                    { DataTypeKind.EVENT_INSTANCE, new PrimitiveDataType(DataTypeKind.EVENT_INSTANCE) },
                    { DataTypeKind.OTHER, new PrimitiveDataType(DataTypeKind.OTHER) },
                    { DataTypeKind.UNKNOWN, new PrimitiveDataType(DataTypeKind.UNKNOWN) }
                };
            }
            return primitiveDataTypes;
        }

        public override Type GetType()
        {
            Type type = null;
            switch (this.Kind)
            {
                case DataTypeKind.UNKNOWN:
                case DataTypeKind.UNIQUE_ID:
                case DataTypeKind.STRING:
                    type = typeof(string); break;
                case DataTypeKind.INTEGER:
                    type = typeof(int); break;
                case DataTypeKind.REAL:
                    type = typeof(double); break;
                case DataTypeKind.BOOLEAN:
                    type = typeof(bool); break;
                case DataTypeKind.TIMESTAMP:
                    type = typeof(DateTime); break;
                case DataTypeKind.TIMESPAN:
                    type = typeof(TimeSpan); break;
                case DataTypeKind.INSTANCE:
                    type = typeof(ConceptualClass); break;
                case DataTypeKind.INSTANCE_SET:
                    type = typeof(List<ConceptualClass>); break;
            }
            return type;
        }

        protected static Dictionary<DataTypeKind, PrimitiveDataType> primitiveDataTypes;

    }
}
