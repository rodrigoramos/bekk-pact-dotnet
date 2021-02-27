using System.Collections;
using nPact.Common.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace nPact.Consumer.Rendering
{
    public sealed class JsonBody : IJsonable
    {
        private readonly object _body;

        public JsonBody(object body) => _body = body;

        public JContainer Render() => _body == null ? null : Render(_body);

        private static JContainer Render(object body)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return body switch
            {
                string serialized => JObject.Parse(serialized),
                IJsonable json => json.Render(),
                IDictionary => JObject.FromObject(body, JsonSerializer.Create(settings)),
                IEnumerable => JArray.FromObject(body, JsonSerializer.Create(settings)),
                _ => JObject.FromObject(body, JsonSerializer.Create(settings))
            };
        }
        
        public override string ToString() => Render().ToString();
    }
}