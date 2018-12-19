using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace LinkedDataProcessor
{
    class ReadContext
    {
        private IDictionary<string, string> _namespaces = new Dictionary<string, string>();

        private string _base;

        public ReadContext(JObject obj)
        {
            foreach (var property in obj)
            {
                var value = property.Value.Value<string>();

                if (property.Key == "@base")
                {
                    _base = value;
                }
                else
                {
                    _namespaces[property.Key] = value;
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
                    return _base + name;
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
