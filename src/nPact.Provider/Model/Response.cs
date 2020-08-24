using System.Collections.Generic;
using System.Net;

namespace nPact.Provider.Model
{
    class Response
    {
        public HttpStatusCode Status { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public Newtonsoft.Json.Linq.JContainer Body { get; set; }
    }
}