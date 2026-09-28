using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class BinaryFunctor<TRef, TPart> : BinaryRelationship<TRef, TPart>, Functor
        where TRef : ConceptualClass
        where TPart : ConceptualClass
    {
        public BinaryFunctor(
            string fIndex,
            TRef refEdgeInstance, Multipricity refMult, List<string> refProperties,
            TPart partEdgeInstance, Multipricity partMult,  List<string> partProperteis, bool partOfAssociative = false)
            : base(fIndex, refEdgeInstance, refMult, refPhrase:"", refProperties, partEdgeInstance, partMult, partPhrase:"", partProperteis, partOfAssociative)
        {
        }

        public string FIndex { get => this.RIndex; }
    }
}
