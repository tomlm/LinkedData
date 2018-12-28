using System;
using System.Collections.Generic;
using System.Text;

namespace LinkedDataProcessor
{
    class Constants
    {
        public const string RDF = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        public const string RDFS = "http://www.w3.org/2000/01/rdf-schema#";
        public const string XSD = "http://www.w3.org/2001/XMLSchema#";
        public const string OWL = "http://www.w3.org/2002/07/owl#";

        public const string Range = Constants.RDFS + "range";
        public const string Domain = Constants.RDFS + "domain";
        public const string SubClassOf = Constants.RDFS + "subClassOf";
        public const string SubPropertyOf = Constants.RDFS + "subPropertyOf";

        public const string InverseOf = Constants.OWL + "inverseOf";
        public const string SymmetricProperty = Constants.OWL + "SymmetricProperty";
        public const string TransitiveProperty = Constants.OWL + "TransitiveProperty";
        public const string SameAs = Constants.OWL + "sameAs";
        public const string FunctionalProperty = Constants.OWL + "FunctionalProperty";
        public const string InverseFunctionalProperty = Constants.OWL + "InverseFunctionalProperty";

        public static void AddStandardPrefixNames(IDictionary<string, string> prefixTable)
        {
            prefixTable["rdf"] = RDF;
            prefixTable["rdfs"] = RDFS;
            prefixTable["xsd"] = XSD;
            prefixTable["owl"] = OWL;
        }

        public static string GetName(IDictionary<string, string> prefixTable, string name)
        {
            if (name == "@type")
            {
                return RDF + "type";
            }

            var index = name.IndexOf(':');
            if (index > 0 && index != name.Length - 1)
            {
                var prefixName = name.Substring(0, index);
                if (prefixTable.TryGetValue(prefixName, out var value))
                {
                    return value + name.Substring(index + 1);
                }
            }
            return name;
        }
    }
}
