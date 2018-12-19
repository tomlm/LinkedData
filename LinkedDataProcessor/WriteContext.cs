using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LinkedDataProcessor
{
    class WriteContext
    {
        public WriteContext(Context context)
        {
            Context = context;
        }

        public Context Context { get; }

        public ISet<string> Subjects { get; } = new HashSet<string>();

        public string GetName(string name)
        {
            var parts = name.Split('#');
            if (parts.Length == 2)
            {
                if (Context.Namespaces.TryGetValue(parts[0] + '#', out var value))
                {
                    if (value == "@base")
                    {
                        return parts[1];
                    }
                    else
                    {
                        return value + ':' + parts[1];
                    }
                }

                return parts[1];
            }

            return name;
        }

        public JObject ToJson()
        {
            var context = new JObject();

            foreach (var item in Context.Namespaces)
            {
                context.Add(item.Value, item.Key);
            }

            return context;
        }
    }
}
