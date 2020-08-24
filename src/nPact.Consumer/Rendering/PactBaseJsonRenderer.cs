using nPact.Common.Contracts;
using Newtonsoft.Json.Linq;

namespace nPact.Consumer.Rendering
{
    abstract class PactBaseJsonRenderer
    {
        protected JObject RenderHeaders(IHeaderCollection headers)
        {
            var obj = new JObject();
            foreach (var header in headers)
            {
                obj.Add(header.Key, header.Value);
            }
            return obj;
        }

        public abstract JObject Render();
        public override string ToString()
        {
            return Render().ToString();
        }
    }
}