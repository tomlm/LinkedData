
using Newtonsoft.Json;

namespace LinkedDataProcessor
{
    /// <summary>
    /// Part of the implementation of the flattened graph
    /// </summary>
    public class Triple
    {
        public Triple(string s, string p, GraphObject o)
        {
            Subject = s;
            Predicate = p;
            Object = o;
        }

        public string Subject { get; private set; }

        public string Predicate { get; private set; }

        public GraphObject Object { get; private set; }

        public override string ToString()
        {
            return string.Format("<{0}> <{1}> {2} .", Subject, Predicate, Object);
        }

        public override bool Equals(object obj)
        {
            var t = obj as Triple;
            if (t == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, t))
            {
                return true;
            }
            return t.Subject.Equals(Subject) && t.Predicate.Equals(Predicate) && t.Object.Equals(Object);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
