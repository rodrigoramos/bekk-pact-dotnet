using System;
using System.Linq;
using nPact.Common.Contracts;
using nPact.Common.Extensions;
using nPact.Common.Utils;
using Newtonsoft.Json.Linq;
using nPact.Consumer.Contracts;

namespace nPact.Consumer.Matching
{
    class BodyComparer : PactComparer
    {
        private readonly JsonNodeMap templateMap;
        private JsonNodeMap requestMap;

        public BodyComparer(IPactRequestDefinition template, IConsumerConfiguration config) 
            : base (template, config)
        {
            templateMap = new JsonNodeMap(template.RequestBody?.Render(), config);
        }   

        public override bool Matches(IPactRequestDefinition request)
        {
            var expected = Template.RequestBody?.Render();
            var actual = request.RequestBody?.Render();
            requestMap = new JsonNodeMap(actual, Config);
            if(expected == null && actual == null) return true;
            if(expected == null || actual == null) return false;
            switch (expected)
            {
                case JObject o: return CompareAsObject(o, actual as JObject);
                case JArray a: return CompareAsArray(a, actual as JArray);
                default: throw new NotImplementedException("Unknown type "+expected.GetType());
            }
        }

        public override JObject DiffGram(IPactRequestDefinition request)
        {
            if(Matches(request)) return null;       
            var expected = Template.RequestBody?.Render();
            var actual = request.RequestBody?.Render();
            requestMap = new JsonNodeMap(actual, Config);
            if(expected == null || actual == null)
            {
                return GetDiff(expected, actual);
            }
            var diff = new JObject();
            switch (expected)
            {
                case JObject o: 
                {
                    DiffAsObject(o, actual as JObject, diff);
                    break;
                }
                case JArray a: 
                {
                    DiffAsArray(a, actual as JArray, diff);
                    break;
                }
                default: throw new NotImplementedException("Unknown type "+expected.GetType());
            }
            return diff;
        }


        private void DiffAsObject(JObject expected, JObject actual, JObject result)
        {
            if(actual == null)
            {
                result.Add(expected.Path, GetDiff(expected, actual));
            }
            else
            {
                foreach(var expectedToken in expected.AsJEnumerable())
                {
                    var actualValue = requestMap[expectedToken];
                    var expectedValue = templateMap[expectedToken];
                    DiffTokens(expectedValue, actualValue, result);
                }
                foreach(var unexpectedToken in actual.AsJEnumerable().Where(a => templateMap[a] == null))
                {
                    var diff = unexpectedToken is JProperty ? ((JProperty)unexpectedToken).Value : unexpectedToken;
                    result.Add(unexpectedToken.Path, GetDiff(null, diff));
                }
            }
        }

        private void DiffAsArray(JArray expected, JArray actual, JObject result)
        {
            if(actual == null || !expected.HasValues && actual.HasValues)
            {
                result.Add(expected.Path, GetDiff(expected, actual));
            }
            var e = expected.GetEnumerator();
            var a = actual.GetEnumerator();
            while (e.MoveNext())
            {
                if (!a.MoveNext()) 
                {
                    result.Add(e.Current.Path, GetDiff(e.Current, null));
                }
                else
                {
                    DiffTokens(e.Current, a.Current, result);
                }
            }
            while(a.MoveNext())
            {
                result.Add(a.Current.Path, GetDiff(null, a.Current));
            }
        }
        private void DiffTokens(JToken expected, JToken actual, JObject result)
        {
            var actualIsNull = actual.IsNull();
            var expectedIsNull = expected.IsNull();
            if(actualIsNull && expectedIsNull) return;
            if(actualIsNull || expectedIsNull || actual.Type != expected.Type)
            {
                result.Add(expected.Path, GetDiff(expected, actual)); 
                return;
            }
            switch(expected)
            {
                case JObject o:
                {
                    DiffAsObject(o, actual as JObject, result);
                    break;
                }
                case JArray a:
                {
                    DiffAsArray(a, actual as JArray, result);
                    break;
                }
                case JProperty p:
                {
                    DiffTokens(p.Value, (actual as JProperty)?.Value, result);
                    break;
                }
                case JValue v:
                {
                    if(!v.Equals(actual as JValue))
                    {
                        result.Add(expected.Path, GetDiff(v.ToString(), actual.ToString()));
                    }
                    break;
                }
                default:
                {
                    throw new NotImplementedException("Unknown type "+expected.GetType());
                }
            }
        }
        private bool CompareAsArray(JArray expected, JArray actual)
        {
            if(actual == null) return false;
            if(!expected.HasValues) return !actual.HasValues;
            var e = expected.GetEnumerator();
            var a = actual.GetEnumerator();
            while (e.MoveNext())
            {
                if (!a.MoveNext()) return false;
                if(!CompareTokens(e.Current, a.Current)) return false;
            }
            return !a.MoveNext();
        }

        private bool CompareAsObject(JObject expected, JObject actual)
        {
            if(actual == null) return false;
            return 
            expected.AsJEnumerable()
                .All(expectedToken => CompareTokens(
                    templateMap[expectedToken],
                    requestMap[expectedToken]))
            &&
            actual.AsJEnumerable()
                .All(actualToken => templateMap[actualToken] != null);
        }

        private bool CompareTokens(JToken expected, JToken actual)
        {
            if (actual.IsNull() && expected.IsNull()) return true;
            if (actual.IsNull() && !expected.IsNull()) return false;
            if (!actual.IsNull() && expected.IsNull()) return false;
            if (actual.Type != expected.Type) return false;
            switch(expected)
            {
                case JObject o:
                {
                    return CompareAsObject(o, actual as JObject);
                }
                case JArray a:
                {
                    return CompareAsArray(a, actual as JArray);
                }
                case JValue v:
                {
                    var av = (JValue) actual;
                    return object.Equals(v, av);
                }
                case JProperty p:
                {
                    return CompareTokens(p.Value, (actual as JProperty)?.Value);
                }
                default:
                {
                    return (actual.ToString() == expected.ToString());
                }
            }
        }
    }
}