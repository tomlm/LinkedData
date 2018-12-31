using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace LinkedDataProcessor
{
    internal class ReadContext
    {
        private IDictionary<string, Uri> _namespaces = new Dictionary<string, Uri>();

        private Uri _base;

        public ReadContext(JToken token)
        {
            if (token is JValue val)
            {
                _base = new Uri(token.ToString().TrimEnd('#') + '#');
            }
            else if (token is JObject obj)
            {
                foreach (var property in obj)
                {
                    var value = property.Value.Value<string>();

                    if (property.Key == "@base")
                    {
                        _base = new Uri(value.TrimEnd('#') + '#');
                    }
                    else
                    {
                        _namespaces[property.Key] = new Uri(value);
                    }
                }
            }
        }

        public string GetFullName(string name)
        {
            var parts = name.Split(':');
            if (parts.Length == 2)
            {
                if (_namespaces.TryGetValue(parts[0], out var value))
                {
                    return value + parts[1];
                }
                else
                {
                    // must be a URI - validate if that is true (all URIs should be http:-))
                    return name;
                }
            }
            else
            {
                if (_base != null)
                {
                    return _base.ToString() + name;
                }
                else
                {
                    // names should be URIs so we shouldn't be here
                    return name;
                }

            }
        }
    }
}
