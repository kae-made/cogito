using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class Bridge : ConceptualDomain
    {
        IReadOnlyDictionary<string, ProjectionClass> ProjectionClasses { get => pClasses; }
        IReadOnlyDictionary<string, Functor> Functors { get => functors; }

        public void AddProjectionlClass(ProjectionClass projectionClass)
        {
            if (pClasses.ContainsKey(projectionClass.KeyLetter))
            {
                ShowProblem("Projection Class");
                Console.WriteLine($"Key Letter '{projectionClass.KeyLetter}' has been used!");
            }
            var validateName = pClasses.Where(kv => kv.Value.Name == projectionClass.Name).ToList();
            if (validateName.Any())
            {
                ShowProblem("Projection Class");
                Console.WriteLine($"Name '{projectionClass.Name}' has been used!");
            }
            var validateNumber = pClasses.Where(kv => kv.Value.Number == projectionClass.Number).ToList();
            if (validateNumber.Any())
            {
                ShowProblem("Projection Class");
                Console.WriteLine($"Number '{projectionClass.Number}' has been used!");
            }
            pClasses.Add(projectionClass.KeyLetter, projectionClass);
        }

        public void AddFunctor(Functor functor)
        {
            if (functors.ContainsKey(functor.FIndex))
            {
                ShowProblem("Functor");
                Console.WriteLine($"Functor Index : {functor.FIndex} has been used!");
            }
            functors.Add(functor.FIndex, functor);
        }

        public Bridge(string Name, string KeyLetter) : base(Name, KeyLetter)
        {
        }

        protected Dictionary<string, ProjectionClass> pClasses = new Dictionary<string, ProjectionClass>();
        protected Dictionary<string, Functor> functors = new Dictionary<string, Functor>();

    }
}
