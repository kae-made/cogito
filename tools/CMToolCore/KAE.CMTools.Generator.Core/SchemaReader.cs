// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using KAE.CMTools.Core;
using Microsoft.Extensions.Logging;

namespace KAE.CMTools.Generator.Core
{
    public interface SchemaReader
    {
        ILogger Logger { get; set; }

        bool Read(Stream formatStream, Stream schemaStream); 
        void Parse(InstanceRepository repository);
        bool Validate();
    }

}
