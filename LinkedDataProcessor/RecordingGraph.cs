
using System.Collections.Generic;
using System.Linq;

namespace LinkedDataProcessor
{
    /// <summary>
    /// A graph that keeps track of the asserts and retracts made. Otherwise delegates work to an inner graph.
    /// This structure is helpful in scenarios like keeping track of triples that have been inferred through reasoning.
    /// </summary>
    public class RecordingGraph : IGraph
    {
        private readonly IGraph _innerGraph;

        public RecordingGraph(IGraph innerGraph)
        {
            _innerGraph = innerGraph;
        }

        public ISet<Triple> Asserted { get; } = new HashSet<Triple>();

        public ISet<Triple> Retracted { get; } = new HashSet<Triple>();

        public string Name
        {
            get { return _innerGraph.Name; }
        }

        public int Count
        {
            get { return _innerGraph.Count; }
        }

        public Patch CreatePatch()
        {
            return new Patch(MakeGraph(Asserted), MakeGraph(Retracted));
        }

        public bool Assert(Triple t)
        {
            bool previouslyRetracted = Retracted.Remove(t);
            if (_innerGraph.Assert(t))
            {
                if (!previouslyRetracted)
                {
                    Asserted.Add(t);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Assert(string s, string p, GraphObject o)
        {
            return Assert(new Triple(s, p, o));
        }

        public IGraph Merge(IGraph g)
        {
            foreach (var t in g.GetTriples())
            {
                Assert(t);
            }
            return this;
        }

        public IGraph Minus(IGraph g)
        {
            foreach (var t in g.GetTriples())
            {
                Retract(t);
            }
            return this;
        }

        public bool Retract(Triple t)
        {
            bool previouslyAsserted = Asserted.Remove(t);
            if (_innerGraph.Retract(t))
            {
                if (!previouslyAsserted)
                {
                    Retracted.Add(t);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Retract(string s, string p, GraphObject o)
        {
            return Retract(new Triple(s, p, o));
        }

        public IEnumerable<Triple> GetByObject(GraphObject to)
        {
            return _innerGraph.GetByObject(to);
        }

        public IEnumerable<Triple> GetByObjectSubject(GraphObject to, string ts)
        {
            return _innerGraph.GetByObjectSubject(to, ts);
        }

        public IEnumerable<Triple> GetByPredicate(string tp)
        {
            return _innerGraph.GetByPredicate(tp);
        }

        public IEnumerable<Triple> GetByPredicateObject(string tp, GraphObject to)
        {
            return _innerGraph.GetByPredicateObject(tp, to);
        }

        public IEnumerable<Triple> GetBySubject(string ts)
        {
            return _innerGraph.GetBySubject(ts);
        }

        public IEnumerable<Triple> GetBySubjectPredicate(string ts, string tp)
        {
            return _innerGraph.GetBySubjectPredicate(ts, tp);
        }

        public IEnumerable<Triple> GetTriples()
        {
            return _innerGraph.GetTriples();
        }

        public bool Exists(Triple t)
        {
            return _innerGraph.Exists(t);
        }

        public bool Exists(string ts, string tp, GraphObject to)
        {
            return _innerGraph.Exists(ts, tp, to);
        }

        public IEnumerable<IGrouping<string, IGrouping<string, GraphObject>>> GetGroupings()
        {
            return _innerGraph.GetGroupings();
        }

        public IEnumerable<IGrouping<string, GraphObject>> GetSubjectGroupings(string s)
        {
            return _innerGraph.GetSubjectGroupings(s);
        }

        public object Clone()
        {
            return new RecordingGraph((IGraph)_innerGraph.Clone());
        }

        private IGraph MakeGraph(ISet<Triple> triples)
        {
            var result = new Graph();
            foreach (var t in triples)
            {
                result.Assert(t);
            }
            return result;
        }
    }
}
