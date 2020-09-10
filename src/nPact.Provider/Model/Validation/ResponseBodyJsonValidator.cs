using System.Linq;
using nPact.Common.Contracts;
using nPact.Common.Extensions;
using Newtonsoft.Json.Linq;

namespace nPact.Provider.Model.Validation
{
    public class ResponseBodyJsonValidator
    {
        private readonly IProviderConfiguration configuration;
        public ResponseBodyJsonValidator(IProviderConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public string Validate(JContainer expected, string actual)
        {
            if(string.IsNullOrWhiteSpace(actual))
            {
                return expected.IsNullOrEmpty() ? null : "Body is not supposed to be empty.";
            }
            switch (expected)
            {
                case JObject o: return ValidateBodyAsObject(actual, o);
                case JArray a: return ValidateBodyAsArray(actual, a);
            }
            return null;
        }
        private string ValidateBodyAsObject(string actual, JObject expected)
        {
            JObject actualJson;
            try
            {
                actualJson = JObject.Parse(actual);
            }
            catch(Newtonsoft.Json.JsonReaderException)
            {
                return "Body is not parsable to object";
            }
            return expected.AsJEnumerable().Select(token => {
                var actualToken = actualJson.GetValue(token.Path, configuration.BodyKeyStringComparison.GetValueOrDefault());
                var expectedValue = expected.GetValue(token.Path);
                return  ValidateTokens(actualToken, expectedValue);
            }).FirstOrDefault(r => r != null);
        }

        private string ValidateBodyAsArray(string actual, JArray expected)
        {
            JArray actualJson;
            try
            {
                actualJson = JArray.Parse(actual);
            }
            catch(Newtonsoft.Json.JsonReaderException)
            {
                return "Body is not parsable to array";
            }
            return ValidateArrays(actualJson, expected);
        }

        private string ValidateArrays(JArray actual, JArray expected)
        {
            if (!expected.HasValues)
            {
                if (actual.HasValues)
                {
                    return $"Array is supposed to be empty at {expected.Path} in body.";
                }
                return null;
            }
            if (!actual.HasValues)
            {
                return $"Array is not supposed to be empty at {expected.Path} in body.";
            }
            var e = expected.GetEnumerator();
            var a = actual.GetEnumerator();
            while (e.MoveNext())
            {
                if (!a.MoveNext()) return $"Element not found in array. Expected {e.Current} at {expected.Path}.";
                var error = ValidateTokens(a.Current, e.Current);
                if (error != null) return error;
            }
            if (a.MoveNext()) return $"Unexpected element found in array {a.Current} at {expected.Path}.";
            return null;
        }
        private string ValidateTokens(JToken actual, JToken expected)
        {
            if (actual.IsNull() && expected.IsNull()) return null;
            if (actual.IsNull() && !expected.IsNull()) return $"Cannot find {expected.Path} in body.";
            if (!actual.IsNull() && expected.IsNull()) return $"Expected null, but found {actual} at {actual.Path} in body.";
            if (actual.Type != expected.Type) return $"Expected {expected}, but found {actual} at {expected.Path} in body.";
            switch(expected)
            {
                case JObject o:
                {
                    return o.Properties().Select(p => ValidateTokens(actual[p.Name],p.Value)).FirstOrDefault(r => r!=null);
                }
                case JArray a:
                {
                    return ValidateArrays((JArray)actual, a);
                }
                default:
                {
                    if(actual.ToString() != expected.ToString()) return $"Not match at {expected.Path} in body. Expected: {expected} but received {actual}."; 
                    return null;
                }
            }
        }
    }
}