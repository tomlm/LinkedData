
using System.Linq;

namespace LinkedDataProcessor
{
    public static class GraphExtensions
    {
        public static IGraph Intersection(this IGraph first, IGraph second)
        {
            var cross = new Graph();
            if (second == null)
            {
                return cross;
            }

            IGraph smaller;
            IGraph larger;
            if (first.Count > second.Count)
            {
                larger = first;
                smaller = second;
            }
            else
            {
                larger = second;
                smaller = first;
            }

            foreach (var t in smaller.GetTriples())
            {
                if (larger.Exists(t))
                {
                    cross.Assert(t);
                }
            }

            return cross;
        }

        public static void RetractBySubject(this IGraph graph, string s)
        {
            foreach (var t in graph.GetBySubject(s).ToList())
            {
                graph.Retract(t);
            }
        }

        public static void RetractByPredicate(this IGraph graph, string p)
        {
            foreach (var t in graph.GetByPredicate(p).ToList())
            {
                graph.Retract(t);
            }
        }

        public static void RetractByObject(this IGraph graph, GraphObject o)
        {
            foreach (var t in graph.GetByObject(o).ToList())
            {
                graph.Retract(t);
            }
        }

        public static void RetractAll(this IGraph graph)
        {
            foreach (var t in graph.GetTriples().ToList())
            {
                graph.Retract(t);
            }
        }

        public static void WriteTo(this IGraph graph, IGraphWriter writer)
        {
            writer.WriteStart();
            writer.WriteName(graph.Name);
            foreach (var s in graph.GetGroupings())
            {
                writer.WriteStartSubject(s.Key);
                foreach (var p in s)
                {
                    writer.WriteStartPredicate(p.Key);
                    foreach (var o in p)
                    {
                        writer.WriteObject(o);
                    }
                    writer.WriteEndPredicate();
                }
                writer.WriteEndSubject();
            }
            writer.WriteEnd();
        }

        public static void WriteSubjectTo(this IGraph graph, IGraphWriter writer)
        {
            foreach (var s in graph.GetGroupings())
            {
                writer.WriteStartSubject(s.Key);
                foreach (var p in s)
                {
                    writer.WriteStartPredicate(p.Key);
                    foreach (var o in p)
                    {
                        writer.WriteObject(o);
                    }
                    writer.WriteEndPredicate();
                }

                writer.WriteEndSubject();
            }
        }
    }
}
