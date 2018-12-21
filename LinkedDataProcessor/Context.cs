using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LinkedDataProcessor
{
    public class Context
    {
        public IDictionary<string, TermDefinition> Terms { get; } = new Dictionary<string, TermDefinition>();

        public string Language { get; set; }
        
        public string Base { get; set; }
        
        public string Vocab { get; set; }
        
        public string Version { get; set; }

        public void FromJson(JObject obj)
        {
            foreach (var property in obj)
            {
                if (property.Key == "@language")
                {
                    Language = property.Value.Value<string>();
                    continue;
                }
                if (property.Key == "@base")
                {
                    Base = property.Value.Value<string>();
                    continue;
                }
                if (property.Key == "@vocab")
                {
                    Vocab = property.Value.Value<string>();
                    continue;
                }
                if (property.Key == "@version")
                {
                    Version = property.Value.Value<string>();
                    continue;
                }
                Terms.Add(property.Key, TermDefinition.FromJson(property.Value));
            }
        }

        public JObject ToJson()
        {
            var obj = new JObject();

            if (!string.IsNullOrEmpty(Language))
            {
                obj.Add("@language", Language);
            }
            if (!string.IsNullOrEmpty(Base))
            {
                obj.Add("@base", Base);
            }
            if (!string.IsNullOrEmpty(Vocab))
            {
                obj.Add("@vocab", Vocab);
            }
            if (!string.IsNullOrEmpty(Version))
            {
                obj.Add("@version", Version);
            }

            foreach (var term in Terms)
            {
                obj.Add(term.Key, term.Value.ToJson());
            }

            return obj;
        }

        public class TermDefinition
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string Container { get; set; }

            public static TermDefinition FromJson(JToken token)
            {
                var result = new TermDefinition();
                if (token is JObject obj)
                {
                    if (obj.TryGetValue("@id", out var idValue))
                    {
                        result.Id = idValue.Value<string>();
                    }
                    if (obj.TryGetValue("@type", out var typeValue))
                    {
                        result.Type = typeValue.Value<string>();
                    }
                    if (obj.TryGetValue("@container", out var containerValue))
                    {
                        result.Container = containerValue.Value<string>();
                    }
                }
                else if (token is JValue value)
                {
                    result.Id = value.Value<string>();
                }
                return result;
            }

            public JToken ToJson()
            {
                if (Type == null && Container == null)
                {
                    return new JValue(Id ?? string.Empty);
                }
                else
                {
                    var obj = new JObject();
                    if (!string.IsNullOrEmpty(Id))
                    {
                        obj.Add("@id", Id);
                    }
                    if (!string.IsNullOrEmpty(Type))
                    {
                        obj.Add("@type", Type);
                    }
                    if (Container == "@list" || Container == "@set")
                    {
                        obj.Add("@container", Container);
                    }
                    return obj;
                }
            }
        }
    }
}
