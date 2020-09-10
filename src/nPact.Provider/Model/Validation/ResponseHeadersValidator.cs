using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using nPact.Common.Contracts;
using Newtonsoft.Json.Linq;

namespace nPact.Provider.Model.Validation
{
    class ResponseHeadersValidator
    {
        public IEnumerable<string> Validate(Response expected, HttpContentHeaders actual)
        {
            return ValidateHeaders(expected.Headers, actual);
        }
        private IEnumerable<string> ValidateHeaders(IDictionary<string,string> expected, HttpContentHeaders actual)
        {
            foreach(var expectedHeader in expected){
                var actualHeader = actual
                    .Where(a => a.Key.Equals(expectedHeader.Key, StringComparison.OrdinalIgnoreCase));
                if (!actualHeader.Any())
                {
                    yield return $"Header {expectedHeader.Key} is missing";
                }
                else
                {
                    var value = string.Join(",", actualHeader.SelectMany(h => h.Value));
                    if (!expectedHeader.Value.Equals(value))
                    {
                        yield return $"Header {expectedHeader.Key} had value {value}. Expected {expectedHeader.Value}.";
                    }
                }
            }
        }

        private string GetHeader(string header, IDictionary<string,string> headers)
        {
            var values =  headers.Where(h => h.Key.Equals(header, StringComparison.OrdinalIgnoreCase))
                                 .Select(h => h.Value).ToList();
            if(values.Any()) return string.Join(",", values);
            return null;
        }
    }
}