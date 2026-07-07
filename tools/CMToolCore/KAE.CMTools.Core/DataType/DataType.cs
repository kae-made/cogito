// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core.DataType
{
    public abstract class DataType
    {
        public abstract new Type GetType();

        public enum DataTypeKind
        {
            UNKNOWN,
            UNIQUE_ID,
            STRING,
            INTEGER,
            REAL,
            BOOLEAN,
            TIMESTAMP,
            TIMESPAN,
            REFERENCE,
            INSTANCE,
            INSTANCE_SET,
            EVENT_INSTANCE,
            OTHER
        }

        public string Name { get => name; }

        protected DataType(string name)
        {
            this.name = name;
        }

        protected string name;
    }
}
