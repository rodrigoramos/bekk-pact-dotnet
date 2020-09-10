using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using nPact.Common.Contracts;

namespace nPact.Common.Utils
{
    public class JsonNodeMap
    {
        private IDictionary<string,JToken> map = new Dictionary<string,JToken>();
        private readonly StringComparison comparison;

        public JsonNodeMap(JToken root, IConfiguration config)
        {
            comparison = config.BodyKeyStringComparison.GetValueOrDefault();
            Map(root);
        }

        public JToken this[string path]
        {
            get
            {
                if(path == null) return null;
                var key = map.Keys.FirstOrDefault(k => string.Equals(path, k, comparison));
                return key != null ? map[key] : null;
            }
        }

        public JToken this[JToken path] => this[path?.Path];

        private void Map(JToken json)
        {
            if(json == null || json.Path == null || map.ContainsKey(json.Path)) return;
            map.Add(json.Path, json);
            var container = json as JContainer;
            if(container != null)
            {
                foreach(var child in container.AsJEnumerable())
                {
                    Map(child);
                }
            }
        }
    }
}