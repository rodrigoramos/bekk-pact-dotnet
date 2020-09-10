using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace nPact.Provider.Model
{
    class Request
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public Newtonsoft.Json.Linq.JContainer Body { get; set; }

        public HttpRequestMessage BuildMessage()
        {
            var message = new HttpRequestMessage();
            message.Method = new HttpMethod(Method);
            foreach(var header in Headers)
            {
                message.Headers.Add(header.Key, header.Value);
            }
            message.RequestUri = new Uri(Path, UriKind.Relative);
            if(Body != null)
            {
                var header = Headers.TryGetValue("Content-Type", out var contentType) ? contentType : null;
                switch(header?.Split(new[]{';'}).FirstOrDefault())
                {
                    case "application/json":
                        message.Content = new StringContent(Body.ToString(), Encoding.UTF8);
                        break;
                    case "application/x-www-form-urlencoded":
                    case "":
                    case null:
                        message.Content = new FormUrlEncodedContent(Body.ToObject<Dictionary<string, string>>());
                        break;
                    default:
                        throw new NotImplementedException($"Content type {header} is not implemented.");
                }
            }
            return message;
        }
    }
}