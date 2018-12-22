using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace LinkedDataProcessor
{
    public static class JsonLdProcessor
    {
        public static JObject Frame(Context context, IGraph graph, string root)
        {
            var writeContext = new WriteContext(context);
            var obj = ProcessSubject(writeContext, graph, root);
            obj.Add("@context", context.ToJson());
            return obj;
        }

        public static IGraph CreateGraph(JObject obj)
        {
            var context = obj["@context"] as JObject;
            var readContext = new ReadContext(context);
            var result = new Graph();
            FlattenJObject(readContext, obj, result);
            return result;
        }

        // implementation of framing/compact processing

        private static JToken ProcessObject(WriteContext context, IGraph graph, GraphObject o)
        {
            if (o.IsID)
            {
                return ProcessSubject(context, graph, o.Id);
            }
            else
            {
                return JToken.Parse(o.ToJSON());
            }
        }

        private static JToken ProcessPredicate(WriteContext context, IGraph graph, IEnumerable<GraphObject> p)
        {
            if (!p.Skip(1).Any())
            {
                return ProcessObject(context, graph, p.First());
            }
            else
            {
                return new JArray(p.Select(v => ProcessObject(context, graph, v)));
            }
        }

        private static JObject ProcessSubject(WriteContext context, IGraph graph, string root)
        {
            var obj = new JObject
            {
                { "@id", context.GetName(root) }
            };

            if (!context.Subjects.Contains(root))
            {
                context.Subjects.Add(root);
                var subjectGrouping = graph.GetSubjectGroupings(root);
                if (subjectGrouping.Any())
                {
                    foreach (var predicateGrouping in subjectGrouping)
                    {
                        var propertyName = context.GetName(predicateGrouping.Key);
                        obj.Add(propertyName, ProcessPredicate(context, graph, predicateGrouping));
                    }
                }
            }
            return obj;
        }

        // implementation of flattening processing

        private static string FlattenJObject(ReadContext context, JObject obj, IGraph result)
        {
            var id = obj["@id"];
            if (id != null)
            {
                var expandedId = context.GetFullName(id.Value<string>());

                foreach (var property in obj)
                {
                    if (property.Key == "@id")
                    {
                        continue;
                    }

                    var expandedPropertyName = context.GetFullName(property.Key);

                    if (property.Value is JValue value)
                    {
                        result.Assert(expandedId, expandedPropertyName, new GraphObject(value));
                    }
                    else if (property.Value is JArray array)
                    {
                        foreach (var item in array)
                        {
                            if (item is JValue itemValue)
                            {
                                result.Assert(expandedId, expandedPropertyName, new GraphObject(itemValue));
                            }
                            else
                            {
                                // objects in arrays
                            }
                        }
                    }
                    else if (property.Value is JObject nestedObj)
                    {
                        var nestedObjId = FlattenJObject(context, nestedObj, result);
                        if (nestedObjId != null)
                        {
                            result.Assert(expandedId, expandedPropertyName, nestedObjId);
                        }
                    }
                }

                return expandedId;
            }

            return null;
        }
    }
}
