// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAE.CMTools.Core
{
    public class ConceptualDomain
    {
        public string Name { get => cDomainName; }
        public IReadOnlyDictionary<string, ConceptualClass> ConceptualClasses { get => cClasses; }

        public IReadOnlyDictionary<string, Relationship> Relationships { get => relationships; }


        public ConceptualDomain(string Name)
        {
            this.cDomainName = Name;
            this.cClasses = new Dictionary<string, ConceptualClass>();
            this.relationships = new Dictionary<string, Relationship>();
        }

        protected void ShowProblem(string defType)
        {
            Console.WriteLine($"Invalid definition : {defType} in Domain : {this.Name}");
        }
        public void AddConceptualClass(ConceptualClass conceptualClass)
        {
            if (cClasses.ContainsKey(conceptualClass.KeyLetter))
            {
                ShowProblem("Conceptual Class");
                Console.WriteLine($"Key Letter '{conceptualClass.KeyLetter}' has been used!");
            }
            var validateName = cClasses.Where(kv=>kv.Value.Name== conceptualClass.Name).ToList();
            if (validateName.Any())
            {
                ShowProblem("Conceptual Class");
                Console.WriteLine($"Name '{conceptualClass.Name}' has been used!");
            }
            var validateNumber = cClasses.Where(kv=>kv.Value.Number== conceptualClass.Number).ToList();
            if (validateNumber.Any())
            {
                ShowProblem("Conceptual Class");
                Console.WriteLine($"Number '{conceptualClass.Number}' has been used!");
            }
            cClasses.Add(conceptualClass.KeyLetter, conceptualClass);
        }

        public void AddRelationship(Relationship relationship)
        {
            if (relationships.ContainsKey(relationship.RIndex))
            {
                ShowProblem("Relationship");
                Console.WriteLine($"Relationship Index : {relationship.RIndex} has been used!");
            }
            relationships.Add(relationship.RIndex, relationship);
        }

        protected string cDomainName;
        protected Dictionary<string, ConceptualClass> cClasses;
        protected Dictionary<string, Relationship> relationships;
    }
}
