// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public abstract class Relationship : Validater
    {
        public enum SemanticRole
        {
            Referent,
            Participant
        }

        public string RIndex { get => relationshipIndex; }

        protected Relationship(string rIndex)
        {
            this.relationshipIndex = rIndex;
        }

        protected string relationshipIndex;

        public enum Multipricity
        {
            JustOne,
            ZeroOrOne,
            MoreThanOne,
            MoreThanZero
        }

        public static Multipricity ToMultiplicity(string mult)
        {
            switch (mult)
            {
                case "1":
                    return Multipricity.JustOne;
                case "1C":
                    return Multipricity.ZeroOrOne;
                case "M":
                    return Multipricity.MoreThanZero;
                case "MC":
                    return Multipricity.MoreThanOne;
            }
            throw new ArgumentException();
        }

        public abstract bool Validate();
    }
}
