using nPact.Common.Contracts;
using nPact.Common.Utils;
using Newtonsoft.Json.Linq;
using nPact.Consumer.Contracts;

namespace nPact.Consumer.Server
{
    class UnknownResponse : IPactResponseDefinition
    {
        private readonly IHeaderCollection _headers;
        private UnknownResponse(IHeaderCollection headers=null)
        {
            _headers = headers??new HeaderCollection().Add("Content-Type", "application/json; charset=utf-8");
            _headers.Add("Warning", "Request was not recognized as one of the registered pacts.");
        }
        public  UnknownResponse(params JObject[] contents): this()
        {

            switch(contents.Length)
            {
                case 0: break;
                case 1: ResponseBody = new Jsonable(contents[0]); break;
                default: ResponseBody = new Jsonable(new JArray(contents)); break;
            }
        }
        public IHeaderCollection ResponseHeaders => _headers;

        public int? ResponseStatusCode => 501;

        public IJsonable ResponseBody { get; }
    }
}