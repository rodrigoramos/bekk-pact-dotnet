using Newtonsoft.Json.Linq;
using nPact.Consumer.Contracts;

namespace nPact.Consumer.Rendering
{
    class PactRequestJsonRenderer : PactBaseJsonRenderer
    {
        private readonly IPactRequestDefinition _pact;

        public PactRequestJsonRenderer(IPactRequestDefinition pact)
        {
            _pact = pact;
        }
        public override JObject Render()
        {
            dynamic json = new JObject();
            json.method = _pact.HttpVerb;
            json.path = _pact.RequestPath + _pact.Query;
            json.headers = RenderHeaders(_pact.RequestHeaders);
            if(_pact.RequestBody != null)
            {
                json.body = _pact.RequestBody.Render();
            }
            return json;
        }
    }
}