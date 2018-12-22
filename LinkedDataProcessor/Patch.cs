
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LinkedDataProcessor
{
    public class Patch
    {
        private IGraph _retract;
        private IGraph _assert;

        public Patch()
        {
            _retract = new Graph();
            _assert = new Graph();
        }

        public Patch(IGraph assert, IGraph retract)
        {
            _assert = assert;
            _retract = retract;
        }

        public IGraph Assert => _assert;

        public IGraph Retract => _retract;

        public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>();

        public static Patch operator +(Patch c1, Patch c2)
        {
            IGraph assert = new Graph();
            IGraph retract = new Graph();

            assert.Merge(c1.Assert);
            assert.Merge(c2.Assert);

            retract.Merge(c1.Retract);
            retract.Merge(c2.Retract);

            var cross = assert.Intersection(retract);
            retract.Minus(cross);
            assert.Minus(cross);

            return new Patch(assert, retract);
        }

        public static Patch Create(IGraph from, IGraph to)
        {
            var newPatch = new Patch();
            Create(newPatch._retract, from, to);
            Create(newPatch._assert, to, from);
            return newPatch;
        }

        /*
        public static Patch Parse(string input)
        {
            using (var textReader = new StringReader(input))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                jsonReader.DateParseHandling = DateParseHandling.None;
                return Load(JObject.Load(jsonReader));
            }
        }
        
        public static IEnumerable<Patch> ParseJsonArray(string input)
        {
            var patchArray = JArray.Parse(input);
            return patchArray.Select(obj => Load((JObject)obj));
        }

        public static string ToJson(IEnumerable<Patch> patches)
        {
            using (var textWriter = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                jsonWriter.WriteStartArray();
                foreach (var patch in patches)
                {
                    patch.WriteTo(jsonWriter);
                }
                jsonWriter.WriteEndArray();
                jsonWriter.Flush();
                return textWriter.ToString();
            }
        }
        */

        public void Apply(IGraph graph)
        {
            graph.Minus(_retract);
            graph.Merge(_assert);
        }

        public void Add(Patch patch)
        {
            var temp = this + patch;
            _assert = temp.Assert;
            _retract = temp.Retract;
        }

        public Patch CreateReverse()
        {
            var reversePatch = new Patch(Retract, Assert);
            foreach (var property in Properties)
            {
                reversePatch.Properties.Add(property);
            }
            return reversePatch;
        }

        /*
        public string ToJson()
        {
            using (var textWriter = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                WriteTo(jsonWriter);
                jsonWriter.Flush();
                return textWriter.ToString();
            }
        }

        // TODO: move this serialization code out of this class
        public void WriteTo(JsonWriter jsonWriter)
        {
            jsonWriter.WriteStartObject();

            // patch properties
            jsonWriter.WritePropertyName("properties");
            jsonWriter.WriteStartObject();
            foreach (var property in Properties)
            {
                jsonWriter.WritePropertyName(property.Key);
                jsonWriter.WriteValue(property.Value);
            }
            jsonWriter.WriteEndObject();

            // the retract graph
            jsonWriter.WritePropertyName("retract");
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("@graph");
            jsonWriter.WriteStartArray();
            var retractWriter = new JsonLdFlattened.FlattenedGraphWriter(string.Empty) { Writer = jsonWriter };
            _retract.WriteSubjectTo(retractWriter);
            jsonWriter.WriteEndArray();
            jsonWriter.WriteEndObject();

            // the assert graph
            jsonWriter.WritePropertyName("assert");
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("@graph");
            jsonWriter.WriteStartArray();
            var assertWriter = new JsonLdFlattened.FlattenedGraphWriter(string.Empty) { Writer = jsonWriter };
            _assert.WriteSubjectTo(assertWriter);
            jsonWriter.WriteEndArray();
            jsonWriter.WriteEndObject();

            jsonWriter.WriteEndObject();
        }
        */

        public override bool Equals(object obj)
        {
            var other = obj as Patch;
            if (other != null)
            {
                bool propertiesEquals = Properties.Intersect(other.Properties).Count() == Properties.Union(other.Properties).Count();
                return propertiesEquals && _retract.Equals(other._retract) && _assert.Equals(other._assert);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 2;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "R:{0} A:{1}", Retract, Assert);
        }

        /*
        private static Patch Load(JObject obj)
        {
            var patch = new Patch();
            LoadProperties(patch.Properties, obj["properties"]);
            FlattenedGraphConverter.Load(obj["retract"], patch.Retract.Assert);
            FlattenedGraphConverter.Load(obj["assert"], patch.Assert.Assert);
            return patch;
        }
        */

        private static void Create(IGraph target, IGraph lhs, IGraph rhs)
        {
            foreach (var lhsTriple in lhs.GetTriples().Where(t => !rhs.Exists(t)))
            {
                target.Assert(lhsTriple);
            }
        }

        private static void LoadProperties(IDictionary<string, string> properties, JToken obj)
        {
            foreach (var prop in (JObject)obj)
            {
                properties.Add(prop.Key, SafeToString(prop.Value));
            }
        }

        private static string SafeToString(JToken v)
        {
            return v.Type == JTokenType.Date ? v.Value<DateTime>().ToString("O") : v.ToString();
        }
    }
}
