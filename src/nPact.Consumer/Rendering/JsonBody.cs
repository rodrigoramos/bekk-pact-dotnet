using System;
using nPact.Common.Contracts;
using nPact.Common.Extensions;
using nPact.Consumer.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace nPact.Consumer.Rendering
{
    class JsonBody : IJsonable
    {
        private object body;

        public JsonBody(object body)
        {
            this.body = body;
        }

        public JContainer Render()
        {
            if (body == null) return null;
            return Render(body);
        }

        private JContainer Render(object body)
        {
            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            switch (body)
            {
                case IJsonable json:
                    return json.Render();
                case string serialized:
                    return JObject.Parse(serialized);
                case Array array:
                    return JArray.FromObject(body, JsonSerializer.Create(settings));
                default:
                    return JObject.FromObject(body, JsonSerializer.Create(settings));
            }
        }

        
        public override string ToString() => Render().ToString();
    }
}