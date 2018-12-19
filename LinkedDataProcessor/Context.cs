using System.Collections.Generic;

namespace LinkedDataProcessor
{
    public class Context
    {
        public IDictionary<string, string> Namespaces { get; } = new Dictionary<string, string>();
    }
}
