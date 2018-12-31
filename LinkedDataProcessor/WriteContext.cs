using System.Collections.Generic;

namespace LinkedDataProcessor
{
    internal class WriteContext
    {
        private IDictionary<string, string> _namespaces = new Dictionary<string, string>();

        public WriteContext(Context context)
        {
            Context = context;

            foreach (var term in context.Terms)
            {
                if (!string.IsNullOrEmpty(term.Value.Id))
                {
                    _namespaces[term.Value.Id] = term.Key;
                }
            }
            if (!string.IsNullOrEmpty(Context.Base))
            {
                _namespaces[Context.Base] = "@base";
            }
        }

        public Context Context { get; }

        public ISet<string> Subjects { get; } = new HashSet<string>();

        public string GetName(string name)
        {
            if (name.StartsWith(Context.Base))
            {
                return name.Substring(Context.Base.Length + 1);
            }
            return name;
            //var parts = name.Split('#');
            //if (parts.Length == 2)
            //{
            //    if (_namespaces.TryGetValue(parts[0] + '#', out var value))
            //    {
            //        if (value == "@base")
            //        {
            //            return parts[1];
            //        }
            //        else
            //        {
            //            return value + ':' + parts[1];
            //        }
            //    }
            //    else if (new Uri(parts[0]) == new Uri(this.Context.Base))
            //    {
            //        return parts[1];
            //    }
            //}

            //return name;
        }
    }
}
