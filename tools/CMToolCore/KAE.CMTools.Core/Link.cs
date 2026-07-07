using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public abstract class Link
    {
        public Relationship Relationship { get => relationship; }
        protected Link(Relationship relationship)
        {
            this.relationship = relationship;
        }

        protected Relationship relationship;
    }
}
