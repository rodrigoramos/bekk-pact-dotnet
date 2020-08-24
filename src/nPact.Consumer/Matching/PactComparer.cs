using System;
using System.Linq;
using nPact.Common.Contracts;
using Newtonsoft.Json.Linq;
using nPact.Consumer.Contracts;

namespace nPact.Consumer.Matching
{
    class PactComparer
    {
        protected IPactRequestDefinition Template { get; }
        protected IConsumerConfiguration Config { get; }

        public PactComparer(IPactRequestDefinition template, IConsumerConfiguration config)
        {
            Template = template ?? throw new ArgumentNullException(nameof(template));
            Config = config;
        }
        public virtual bool Matches(IPactRequestDefinition request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (Template.HttpVerb != request.HttpVerb) return false;
            if (!CompareAcceptEmptyAsNull(Template.RequestPath, request.RequestPath)) return false;
            if (!CompareAcceptEmptyAsNull(Template.Query, request.Query)) return false;
            foreach (var header in Template.RequestHeaders)
            {
                if (request.RequestHeaders[header.Key]?.Equals(header.Value) != true) return false;
            }
            var bodyComparison = new BodyComparer(Template, Config);
            return bodyComparison.Matches(request);
        }
        public virtual JObject DiffGram(IPactRequestDefinition request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            dynamic diff = new JObject();
            if (Template.HttpVerb != request.HttpVerb) diff.Add("HttpVerb", GetDiff(Template.HttpVerb, request.HttpVerb));
            if (!CompareAcceptEmptyAsNull(Template.RequestPath, request.RequestPath)) diff.Add("Path", GetDiff(Template.RequestPath, request.RequestPath));
            if (!CompareAcceptEmptyAsNull(Template.Query, request.Query)) diff.Add("Query", GetDiff(Template.Query, request.Query));
            var headers = Template.RequestHeaders.Where(expected => true != request.RequestHeaders[expected.Key]?.Equals(expected.Value)).ToList();
            if(headers.Any()){
                dynamic headersDiff = new JObject();
                foreach(var header in headers)
                {
                    headersDiff.Add(header.Key, GetDiff(header.Value, request.RequestHeaders[header.Key]));
                }
                diff.Add("headers", headersDiff);
            }
            var bodyComparison = new BodyComparer(Template, Config);
            var bodies = bodyComparison.DiffGram(request);
            if(bodies != null)
            {
                diff.Add("body", bodies);
            }
            return diff;
        }

        protected JObject GetDiff(string expected, string actual)
        {
            dynamic diff = new JObject();
            diff.Add("expected", expected);
            diff.Add("actual", actual);
            return diff;
        }
      
        protected JObject GetDiff(JToken expected, JToken actual)
        {
            dynamic diff = new JObject();
            diff.Add("expected", expected);
            diff.Add("actual", actual);
            return diff;
        }

        protected bool CompareAcceptEmptyAsNull(string left, string right)
        {
            if(string.IsNullOrEmpty(left)) return string.IsNullOrEmpty(right);
            return left.Equals(right);
        }
    }
}