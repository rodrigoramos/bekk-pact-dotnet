using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace nPact.Provider.Model
{
    public class Request
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public Newtonsoft.Json.Linq.JContainer Body { get; set; }

        public HttpRequestMessage BuildMessage()
        {
            var message = new HttpRequestMessage
            {
                Method = new HttpMethod(Method)
            };

            foreach (var (key, value) in Headers)
            {
                // HttpRequestHeader.TryParse()
                // message.Headers.
                if (!string.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                    message.Headers.Add(key, value);
            }

            message.RequestUri = new Uri(Path, UriKind.Relative);

            if (Body == null) return message;

            var (_, contentTypeValue) =
                Headers.FirstOrDefault(x => string.Equals(x.Key, "Content-Type", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(contentTypeValue) ||
                contentTypeValue.Contains("application/x-www-form-urlencoded",
                    StringComparison.OrdinalIgnoreCase))
                message.Content = new FormUrlEncodedContent(Body.ToObject<Dictionary<string, string>>());
            else if (contentTypeValue.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                message.Content = new StringContent(Body.ToString(), Encoding.UTF8, "application/json");
            else
                throw new NotImplementedException($"Content type {contentTypeValue} is not implemented.");

            return message;
        }
    }
}