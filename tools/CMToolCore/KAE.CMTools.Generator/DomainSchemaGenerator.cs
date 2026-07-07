using KAE.CMTools.Core;
using KAE.CMTools.Generator.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Generator
{
    public class DomainSchemaGenerator : KAE.CMTools.Generator.Core.Generator
    {
        public ILogger Logger { get => logger; set => logger = value; }

        public void Generate(string domainName, InstanceRepository repository, TextWriter output)
        {
            var domain = repository.ConceptualDomains[domainName];
            var generator = new templates.InstanceSchema(domain);

            string resultText = generator.TransformText();

            output.WriteLine(resultText);
        }

        public DomainSchemaGenerator()
        {
            ;
        }

        protected ILogger logger;
    }
}
