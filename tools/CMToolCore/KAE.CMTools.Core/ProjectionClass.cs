using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class ProjectionClass : ConceptualClass
    {
        public string PKeyLetter { get => pKeyLetter; }

        public override ConceptualDomain CDomain { get => this.CDomain; }

        public ProjectionClass(ConceptualClass baseClass) : base(baseClass.Name, baseClass.KeyLetter, baseClass.Number, baseClass.Description)
        {
            this.pKeyLetter = baseClass.CDomain.KeyLetter + ":" + baseClass.KeyLetter;
        }

        protected string pKeyLetter;
    }
}
