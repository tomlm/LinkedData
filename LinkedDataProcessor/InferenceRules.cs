
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkedDataProcessor
{
    /// <summary>
    /// Implementation of RDFS schema rules and a small number of relevant OWL schema rules
    /// This code processes the provided schema and produces a set of functions that can be applied to the actual data
    /// applying these functions (actually "SPIN"-ing them) materializes the inferred relationships and types
    /// </summary>
    public static class InferenceRules
    {
        static InferenceRules()
        {
            var schemaSchema = new Graph();

            // subClassOf
            schemaSchema.Assert("subClassOf", "@type", "TransitiveProperty");

            // subPropertyOf
            schemaSchema.Assert("subPropertyOf", "@type", "TransitiveProperty");

            // equivalentClass
            schemaSchema.Assert("equivalentClass", "@type", "SymmetricProperty");
            schemaSchema.Assert("equivalentClass", "subPropertyOf", "subClassOf");

            // equivalentProperty
            schemaSchema.Assert("equivalentProperty", "@type", "SymmetricProperty");
            schemaSchema.Assert("equivalentProperty", "subPropertyOf", "subPropertyOf");

            // inverseOf
            schemaSchema.Assert("inverseOf", "@type", "SymmetricProperty");

            // sameAs
            schemaSchema.Assert("sameAs", "@type", "SymmetricProperty");
            schemaSchema.Assert("sameAs", "@type", "TransitiveProperty");

            SchemaSchemaRules = InferenceRules.InnerCreateFromSchema(schemaSchema);
        }

        private static List<Action<IGraph>> SchemaSchemaRules { get; set; }

        public static List<Action<IGraph>> CreateFromSchema(IGraph schema)
        {
            Reasoner.Spin(schema, SchemaSchemaRules);
            return InnerCreateFromSchema(schema);
        }

        /// <summary>
        /// This functions implements the schema constructs described as "RDFS-Plus"
        /// in the text "Semantic Web for the Working Ontologist" by Dean Allenmang et al.
        /// </summary>
        private static List<Action<IGraph>> InnerCreateFromSchema(IGraph schema)
        {
            var result = new List<Action<IGraph>>();

            // RDFS refer to https://www.w3.org/TR/rdf-schema/
            result.AddRange(Range(schema));
            result.AddRange(Domain(schema));
            result.AddRange(SubClassOf(schema));
            result.AddRange(SubPropertyOf(schema));

            // OWL refer to https://www.w3.org/TR/owl-features/
            result.AddRange(InverseOf(schema));
            result.AddRange(SymmetricProperty(schema));
            result.AddRange(TransitiveProperty(schema));
            result.AddRange(SameAs(schema));
            result.AddRange(FunctionalProperty(schema));
            result.AddRange(InverseFunctionalProperty(schema));
            return result;
        }

        /// <summary>
        /// p rdfs:range c
        /// </summary>
        private static IEnumerable<Action<IGraph>> Range(IGraph schema)
        {
            var result = new List<Action<IGraph>>();
            foreach (var t in schema.GetByPredicate("range").Where(t => t.Object.IsID))
            {
                result.Add((g) => ApplyRange(g, t.Subject, t.Object.Id));
            }
            return result;
        }

        private static void ApplyRange(IGraph g, string p, string c)
        {
            foreach (var t in g.GetByPredicate(p).Where(t => t.Object.IsID).ToList())
            {
                g.Assert(t.Object.Id, "@type", c);
            }
        }

        /// <summary>
        /// p rdfs:domain c
        /// </summary>
        private static IEnumerable<Action<IGraph>> Domain(IGraph schema)
        {
            var result = new List<Action<IGraph>>();
            foreach (var t in schema.GetByPredicate("domain").Where(t => t.Object.IsID))
            {
                result.Add((g) => ApplyDomain(g, t.Subject, t.Object.Id));
            }
            return result;
        }

        private static void ApplyDomain(IGraph g, string p, string c)
        {
            foreach (var t in g.GetByPredicate(p).ToList())
            {
                g.Assert(t.Subject, "@type", c);
            }
        }

        /// <summary>
        /// p rdfs:subClassOf c
        /// </summary>
        private static IEnumerable<Action<IGraph>> SubClassOf(IGraph schema)
        {
            var result = new List<Action<IGraph>>();
            foreach (var t in schema.GetByPredicate("subClassOf").Where(t => t.Object.IsID))
            {
                result.Add((g) => ApplySubClassOf(g, t.Subject, t.Object.Id));
            }
            return result;
        }

        private static void ApplySubClassOf(IGraph g, string a, string b)
        {
            foreach (var t in g.GetByPredicateObject("@type", a).ToList())
            {
                g.Assert(t.Subject, "@type", b);
            }
        }

        /// <summary>
        /// p rdfs:subPropertyOf c
        /// </summary>
        private static IEnumerable<Action<IGraph>> SubPropertyOf(IGraph schema)
        {
            var result = new List<Action<IGraph>>();
            foreach (var t in schema.GetByPredicate("subPropertyOf").Where(t => t.Object.IsID))
            {
                result.Add((g) => ApplySubPropertyOf(g, t.Subject, t.Object.Id));
            }
            return result;
        }

        private static void ApplySubPropertyOf(IGraph g, string a, string b)
        {
            foreach (var t in g.GetByPredicate(a).ToList())
            {
                g.Assert(t.Subject, b, t.Object);
            }
        }

        /// <summary>
        /// x owl:inverseOf y
        /// </summary>
        private static IEnumerable<Action<IGraph>> InverseOf(IGraph schema)
        {
            var result = new List<Action<IGraph>>();
            foreach (var t in schema.GetByPredicate("inverseOf").Where(t => t.Object.IsID))
            {
                result.Add((g) => ApplyInverseOf(g, t.Subject, t.Object.Id));
            }
            return result;
        }

        private static void ApplyInverseOf(IGraph g, string x, string y)
        {
            foreach (var t in g.GetByPredicate(x).Where(t => t.Object.IsID).ToList())
            {
                g.Assert(t.Object.Id, y, t.Subject);
            }
        }

        /// <summary>
        /// x a owl:SymmetricProperty
        /// </summary>
        private static IEnumerable<Action<IGraph>> SymmetricProperty(IGraph schema)
        {
            var result = new List<Action<IGraph>>();
            foreach (var t in schema.GetByPredicateObject("@type", "SymmetricProperty"))
            {
                result.Add((g) => ApplySymmetricProperty(g, t.Subject));
            }
            return result;
        }

        private static void ApplySymmetricProperty(IGraph g, string p)
        {
            foreach (var t in g.GetByPredicate(p).Where(t => t.Object.IsID).ToList())
            {
                g.Assert(t.Object.Id, p, t.Subject);
            }
        }

        /// <summary>
        /// x a owl:TransitiveProperty
        /// </summary>
        private static IEnumerable<Action<IGraph>> TransitiveProperty(IGraph schema)
        {
            var result = new List<Action<IGraph>>();
            foreach (var t in schema.GetByPredicateObject("@type", "TransitiveProperty"))
            {
                result.Add((g) => ApplyTransitiveProperty(g, t.Subject));
            }
            return result;
        }

        private static void ApplyTransitiveProperty(IGraph g, string p)
        {
            var triples = new List<Triple>();
            foreach (var t1 in g.GetByPredicate(p).Where(t => t.Object.IsID))
            {
                foreach (var t2 in g.GetBySubjectPredicate(t1.Object.Id, p))
                {
                    triples.Add(new Triple(t1.Subject, p, t2.Object));
                }
            }
            foreach (var t in triples)
            {
                g.Assert(t);
            }
        }

        /// <summary>
        /// x owl:sameAs y
        /// </summary>
        private static IEnumerable<Action<IGraph>> SameAs(IGraph schema)
        {
            var result = new List<Action<IGraph>>();
            foreach (var t in schema.GetByPredicate("sameAs").Where(t => t.Object.IsID))
            {
                result.Add((g) => ApplySameAs(g, t.Subject, t.Object.Id));
            }
            return result;
        }

        private static void ApplySameAs(IGraph g, string x, string y)
        {
            var triples = new List<Triple>();
            InnerApplySameAs(g, x, y, triples);
            foreach (var t in triples)
            {
                g.Assert(t);
            }
        }

        private static void InnerApplySameAs(IGraph g, string x, string y, List<Triple> triples)
        {
            if (!x.Equals(y))
            {
                triples.Add(new Triple(x, "sameAs", y));
            }
            foreach (var t in g.GetBySubject(x))
            {
                if (!t.Predicate.Equals("sameAs"))
                {
                    triples.Add(new Triple(y, t.Predicate, t.Object));
                }
            }
            foreach (var t in g.GetByPredicate(x))
            {
                triples.Add(new Triple(t.Subject, y, t.Object));
            }
            foreach (var t in g.GetByObject(x))
            {
                if (!t.Predicate.Equals("sameAs"))
                {
                    triples.Add(new Triple(t.Subject, t.Predicate, y));
                }
            }
        }

        /// <summary>
        /// x a owl:FunctionalProperty
        /// </summary>
        private static IEnumerable<Action<IGraph>> FunctionalProperty(IGraph schema)
        {
            var result = new List<Action<IGraph>>();
            foreach (var t in schema.GetByPredicateObject("@type", "FunctionalProperty"))
            {
                result.Add((g) => ApplyFunctionalProperty(g, t.Subject));
            }
            return result;
        }

        private static void ApplyFunctionalProperty(IGraph g, string x)
        {
            var triples = new List<Triple>();
            foreach (var gp in g.GetByPredicate(x).Where(t => t.Object.IsID).GroupBy(k => k.Subject))
            {
                foreach (var mx in gp)
                {
                    foreach (var my in gp)
                    {
                        if (mx.Equals(my))
                        {
                            continue;
                        }
                        InnerApplySameAs(g, mx.Object.Id, my.Object.Id, triples);
                    }
                }
            }
            foreach (var t in triples)
            {
                g.Assert(t);
            }
        }

        /// <summary>
        /// x a owl:InverseFunctionalProperty
        /// </summary>
        private static IEnumerable<Action<IGraph>> InverseFunctionalProperty(IGraph schema)
        {
            var result = new List<Action<IGraph>>();
            foreach (var t in schema.GetByPredicateObject("@type", "InverseFunctionalProperty"))
            {
                result.Add((g) => ApplyInverseFunctionalProperty(g, t.Subject));
            }
            return result;
        }

        private static void ApplyInverseFunctionalProperty(IGraph g, string x)
        {
            var triples = new List<Triple>();
            foreach (var gp in g.GetByPredicate(x).GroupBy(k => k.Object))
            {
                foreach (var mx in gp)
                {
                    foreach (var my in gp)
                    {
                        if (mx.Equals(my))
                        {
                            continue;
                        }
                        InnerApplySameAs(g, mx.Subject, my.Subject, triples);
                    }
                }
            }
            foreach (var t in triples)
            {
                g.Assert(t);
            }
        }
    }
}
