
using System;
using System.Collections.Generic;

namespace LinkedDataProcessor
{
    /// <summary>
    /// The Reasoner "SPIN"s a set of rules against the graph.
    /// Executing a rule creates new triples.
    /// These triples are asserted into the underlying graph.
    /// "Asserted" and not "Inserted" because if the statement already exists it is not added.
    /// The execution continues until not more new data is created.
    /// </summary>
    public class Reasoner
    {
        private List<Action<IGraph>> _rules;

        public Reasoner(IGraph schema)
        {
            _rules = InferenceRules.CreateFromSchema(schema);
        }

        public void Apply(IGraph graph)
        {
            Spin(graph, _rules);
        }

        public static void Spin(IGraph graph, IEnumerable<Action<IGraph>> rules)
        {
            while (true)
            {
                int before = graph.Count;

                foreach (var rule in rules)
                {
                    rule(graph);
                }

                int after = graph.Count;

                if (after == before)
                {
                    break;
                }
            }
        }
    }
}
