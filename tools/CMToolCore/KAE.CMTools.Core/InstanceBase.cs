using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class InstanceBase : Instance
    {
        public InstanceBase(FieldOfSense fos, ConceptualClass conceptualClass, string instanceId) : base(conceptualClass, instanceId)
        {
            this.fieldOfSense = fos;
        }

        protected FieldOfSense fieldOfSense;

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
