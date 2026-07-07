using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class DeterminedProperty
    {
        public string Name { get => definition.Name; }
        public Property Definition { get => definition; }
        public object Value { get => value; set => this.value = value; }

        public DeterminedProperty(Property definition)
        {
            this.definition = definition;
        }
        public DeterminedProperty(Property definition, object value) : this(definition)
        {
            this.Value = value;
        }
        
        protected Property definition;
        protected object value;
    }
}
