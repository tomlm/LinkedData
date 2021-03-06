﻿
using Schema.NET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkedDataProcessor
{
    public class Graph : IGraph
    {
        private readonly IDictionary<string, IDictionary<string, ISet<GraphObject>>> _spo;
        private readonly IDictionary<string, IDictionary<GraphObject, ISet<string>>> _pos;
        private readonly IDictionary<GraphObject, IDictionary<string, ISet<string>>> _osp;

        public Graph(string name = null)
        {
            Name = name ?? string.Empty;
            _spo = new Dictionary<string, IDictionary<string, ISet<GraphObject>>>();
            _pos = new Dictionary<string, IDictionary<GraphObject, ISet<string>>>();
            _osp = new Dictionary<GraphObject, IDictionary<string, ISet<string>>>();
        }

        public string Name
        {
            get;
            private set;
        }

        public int Count
        {
            get;
            private set;
        }

        public bool Assert(Triple t)
        {
            return Assert(t.Subject, t.Predicate, t.Object);
        }

        public bool Assert(JsonLdObject source, string p, JsonLdObject target)
        {
            if (source.GetType().GetProperty(p) == null)
            {
                throw new MissingMemberException(p);
            }

            if (!Uri.IsWellFormedUriString(p, UriKind.Absolute))
            {
                var name = char.ToLower(p[0]) + p.Substring(1); 
                p = new Uri(new Uri(source.Context), new Uri(name, UriKind.Relative)).ToString();
            }
            return Assert(source.Id.ToString(), p, target.Id.ToString());
        }

        public bool Assert(string s, string p, GraphObject o)
        {
            if (s == null || p == null || o == null || o.IsNull)
            {
                return false;
            }

            if (Assert(_spo, s, p, o))
            {
                if (Assert(_pos, p, o, s))
                {
                    if (Assert(_osp, o, s, p))
                    {
                        Count++;
                        return true;
                    }
                }
            }
            return false;
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
            return Retract(t.Subject, t.Predicate, t.Object);
        }

        public bool Retract(string s, string p, GraphObject o)
        {
            if (s == null || p == null || o == null || o.IsNull)
            {
                return false;
            }

            if (Retract(_spo, s, p, o))
            {
                if (Retract(_pos, p, o, s))
                {
                    if (Retract(_osp, o, s, p))
                    {
                        Count--;
                        return true;
                    }
                }
            }
            return false;
        }

        public IEnumerable<Triple> GetByObject(GraphObject to)
        {
            IDictionary<string, ISet<string>> sp;
            if (_osp.TryGetValue(to, out sp))
            {
                foreach (var s in sp)
                {
                    foreach (var p in s.Value)
                    {
                        yield return new Triple(s.Key, p, to);
                    }
                }
            }
        }

        public IEnumerable<Triple> GetByObjectSubject(GraphObject to, string ts)
        {
            IDictionary<string, ISet<string>> sp;
            if (_osp.TryGetValue(to, out sp))
            {
                ISet<string> p;
                if (sp.TryGetValue(ts, out p))
                {
                    foreach (var tp in p)
                    {
                        yield return new Triple(ts, tp, to);
                    }
                }
            }
        }

        public IEnumerable<Triple> GetByPredicate(string tp)
        {
            IDictionary<GraphObject, ISet<string>> os;
            if (_pos.TryGetValue(tp, out os))
            {
                foreach (var o in os)
                {
                    foreach (var s in o.Value)
                    {
                        yield return new Triple(s, tp, o.Key);
                    }
                }
            }
        }

        public IEnumerable<Triple> GetByPredicateObject(string tp, GraphObject to)
        {
            IDictionary<GraphObject, ISet<string>> os;
            if (_pos.TryGetValue(tp, out os))
            {
                ISet<string> s;
                if (os.TryGetValue(to, out s))
                {
                    foreach (var ts in s)
                    {
                        yield return new Triple(ts, tp, to);
                    }
                }
            }
        }

        public IEnumerable<Triple> GetBySubject(string ts)
        {
            IDictionary<string, ISet<GraphObject>> po;
            if (_spo.TryGetValue(ts, out po))
            {
                foreach (var o in po)
                {
                    foreach (var qo in o.Value)
                    {
                        yield return new Triple(ts, o.Key, qo);
                    }
                }
            }
        }

        public IEnumerable<Triple> GetBySubjectPredicate(string ts, string tp)
        {
            IDictionary<string, ISet<GraphObject>> po;
            if (_spo.TryGetValue(ts, out po))
            {
                ISet<GraphObject> o;
                if (po.TryGetValue(tp, out o))
                {
                    foreach (var qo in o)
                    {
                        yield return new Triple(ts, tp, qo);
                    }
                }
            }
        }

        public IEnumerable<Triple> GetTriples()
        {
            foreach (var s in _spo)
            {
                foreach (var po in s.Value)
                {
                    foreach (var o in po.Value)
                    {
                        yield return new Triple(s.Key, po.Key, o);
                    }
                }
            }
        }

        public bool Exists(Triple t)
        {
            return Exists(t.Subject, t.Predicate, t.Object);
        }

        public bool Exists(string ts, string tp, GraphObject to)
        {
            IDictionary<string, ISet<GraphObject>> po;
            if (_spo.TryGetValue(ts, out po))
            {
                ISet<GraphObject> o;
                if (po.TryGetValue(tp, out o))
                {
                    return o.Contains(to);
                }
            }
            return false;
        }

        public IEnumerable<IGrouping<string, IGrouping<string, GraphObject>>> GetGroupings()
        {
            foreach (var s in _spo)
            {
                yield return new SubjectGrouping(s);
            }
        }

        public IEnumerable<IGrouping<string, GraphObject>> GetSubjectGroupings(string s)
        {
            IDictionary<string, ISet<GraphObject>> po;
            if (_spo.TryGetValue(s, out po))
            {
                foreach (var p in po)
                {
                    yield return new PredicateGrouping(p);
                }
            }
        }

        public object Clone()
        {
            var result = new Graph();
            foreach (var t in GetTriples())
            {
                result.Assert(t);
            }
            return result;
        }

#if DEBUG
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var t in GetTriples())
            {
                sb.AppendLine(t.ToString());
            }
            return sb.ToString();
        }
#endif

        public override bool Equals(object obj)
        {
            if (obj is IGraph)
            {
                return Equals(this, (IGraph)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 2;
        }

        private static bool Equals(IGraph x, IGraph y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null || x.Count != y.Count)
            {
                return false;
            }

            foreach (var tx in x.GetTriples())
            {
                if (!y.Exists(tx))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool Assert<TX, TY, TZ>(IDictionary<TX, IDictionary<TY, ISet<TZ>>> xyz, TX tx, TY ty, TZ tz)
        {
            IDictionary<TY, ISet<TZ>> yz;
            if (!xyz.TryGetValue(tx, out yz))
            {
                yz = new Dictionary<TY, ISet<TZ>>();
                xyz[tx] = yz;
            }
            ISet<TZ> z;
            if (!yz.TryGetValue(ty, out z))
            {
                z = new HashSet<TZ>();
                yz[ty] = z;
            }
            return z.Add(tz);
        }

        private static bool Retract<TX, TY, TZ>(IDictionary<TX, IDictionary<TY, ISet<TZ>>> xyz, TX tx, TY ty, TZ tz)
        {
            bool f = false;
            IDictionary<TY, ISet<TZ>> yz;
            if (xyz.TryGetValue(tx, out yz))
            {
                ISet<TZ> z;
                if (yz.TryGetValue(ty, out z))
                {
                    f = z.Remove(tz);
                    if (z.Count == 0)
                    {
                        yz.Remove(ty);
                        if (yz.Count == 0)
                        {
                            xyz.Remove(tx);
                        }
                    }
                }
            }
            return f;
        }

        private class SubjectGrouping : IGrouping<string, IGrouping<string, GraphObject>>
        {
            private KeyValuePair<string, IDictionary<string, ISet<GraphObject>>> _kv;

            public SubjectGrouping(KeyValuePair<string, IDictionary<string, ISet<GraphObject>>> kv)
            {
                _kv = kv;
            }

            public string Key
            {
                get { return _kv.Key; }
            }

            public IEnumerator<IGrouping<string, GraphObject>> GetEnumerator()
            {
                foreach (var p in _kv.Value)
                {
                    yield return new PredicateGrouping(p);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class PredicateGrouping : IGrouping<string, GraphObject>
        {
            private KeyValuePair<string, ISet<GraphObject>> _kv;

            public PredicateGrouping(KeyValuePair<string, ISet<GraphObject>> kv)
            {
                _kv = kv;
            }

            public string Key
            {
                get { return _kv.Key; }
            }

            public IEnumerator<GraphObject> GetEnumerator()
            {
                foreach (var o in _kv.Value)
                {
                    yield return o;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
