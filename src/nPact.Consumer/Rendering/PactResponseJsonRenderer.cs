using Newtonsoft.Json.Linq;
using nPact.Consumer.Contracts;

namespace nPact.Consumer.Rendering
{
    class PactResponseJsonRenderer : PactBaseJsonRenderer
    {
        private readonly IPactResponseDefinition _pact;

        public PactResponseJsonRenderer(IPactResponseDefinition pact)
        {
            _pact = pact;
        }

        public override JObject Render()
        {
            dynamic json = new JObject();
            json.status = _pact.ResponseStatusCode;
            json.headers = RenderHeaders(_pact.ResponseHeaders);
            if(_pact.ResponseBody != null)
            {
                json.body = _pact.ResponseBody.Render();
            }
            return json;
        }
    }
}