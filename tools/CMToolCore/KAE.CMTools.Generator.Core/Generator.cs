using KAE.CMTools.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Generator.Core
{
    public interface Generator
    {
        ILogger Logger { get; set; }
        void Generate(string domainName, InstanceRepository repository, TextWriter output);
    }
}
