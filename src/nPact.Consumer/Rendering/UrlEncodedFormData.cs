using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using nPact.Common.Contracts;
using Newtonsoft.Json.Linq;

namespace nPact.Consumer.Rendering
{
    class UrlEncodedFormData : IJsonable
    {
        private readonly string content;
        public UrlEncodedFormData(string content)
        {
            this.content = content;
        }
        
        public JContainer Render() => JObject.FromObject(Parse());

        private Dictionary<string, string> Parse() => 
            content.Split(new []{'&'},StringSplitOptions.RemoveEmptyEntries)
             .Select(entry => entry.Split(new []{'='}))
             .ToDictionary(entry => WebUtility.UrlDecode(entry[0]), entry => WebUtility.UrlDecode(entry[1]));

        public override string ToString() => content;
    }
}