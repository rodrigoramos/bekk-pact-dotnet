using System;
using System.Linq;
using System.Text;
using nPact.Common.Contracts;
using nPact.Common.Extensions;
using Newtonsoft.Json.Linq;

namespace nPact.Provider.Model.Validation
{
    public class ResponseBodyJsonValidator
    {
        private readonly IProviderConfiguration _configuration;

        public ResponseBodyJsonValidator(IProviderConfiguration configuration)
            => _configuration = configuration;

        public string Validate(JContainer expected, string actual)
        {
            if (string.IsNullOrWhiteSpace(actual))
                return expected.IsNullOrEmpty() ? null : "Body is not supposed to be empty.";

            return expected switch
            {
                JObject o => ValidateBodyAsObject(actual, o),
                JArray a => ValidateBodyAsArray(actual, a),
                _ => null
            };
        }

        private string ValidateBodyAsObject(string actual, JObject expected)
        {
            JObject actualJson;
            try
            {
                actualJson = JObject.Parse(actual);
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                return "Body is not parsable to object";
            }

            var expectedTokens = expected.AsJEnumerable();

            var validationMessages = expectedTokens
                .Select(token =>
                {
                    var actualToken =
                        actualJson.GetValue(token.Path, _configuration.BodyKeyStringComparison.GetValueOrDefault());
                    var expectedValue = expected.GetValue(token.Path);
                    return ValidateTokens(actualToken, expectedValue);
                }).Where(r => r != null)
                .ToArray();

            return validationMessages.Any()
                ? validationMessages.Aggregate((f, s) => $"{f}{Environment.NewLine}{s}")
                : null;
        }

        private string ValidateBodyAsArray(string actual, JArray expected)
        {
            JArray actualJson;
            try
            {
                actualJson = JArray.Parse(actual);
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                return "Body is not parsable to array";
            }

            return ValidateArrays(actualJson, expected);
        }

        private string ValidateArrays(JArray actual, JArray expected)
        {
            if (!expected.HasValues)
                return actual.HasValues ? $"Array is supposed to be empty at {expected.Path} in body." : null;

            if (!actual.HasValues)
                return $"Array is not supposed to be empty at {expected.Path} in body.";

            var errors = new StringBuilder();
            using var e = expected.GetEnumerator();
            using var a = actual.GetEnumerator();
            while (e.MoveNext())
            {
                if (!a.MoveNext()) return $"Element not found in array. Expected {e.Current} at {expected.Path}.";
                var error = ValidateTokens(a.Current, e.Current);
                if (error != null)
                    errors.AppendLine(error);
            }

            if (a.MoveNext())
                errors.AppendLine($"Unexpected element found in array {a.Current} at {expected.Path}.");

            return errors.Length > 0 ? errors.ToString() : null;
        }

        private string ValidateTokens(JToken actual, JToken expected)
        {
            if (actual.IsNull() && expected.IsNull()) return null;
            if (actual.IsNull() && !expected.IsNull()) return $"Cannot find {expected.Path} in body.";
            if (!actual.IsNull() && expected.IsNull())
                return $"Expected null, but found {actual} at {actual.Path} in body.";

            if (actual.Type != expected.Type)
                return $"Expected {expected}, but found {actual} at {expected.Path} in body.";

            switch (expected)
            {
                case JObject o:
                {
                    var errors = o.Properties()
                        .Select(p => ValidateTokens(actual[p.Name], p.Value))
                        .Where(r => r != null)
                        .ToArray();

                    return errors.Any()
                        ? errors.Aggregate((f, s) => $"{f}{Environment.NewLine}{s}")
                        : null;
                }
                case JArray a:
                    return ValidateArrays((JArray) actual, a);
                default:
                    return actual.ToString() != expected.ToString()
                        ? $"Not match at {expected.Path} in body. Expected: {expected} but received {actual}."
                        : null;
            }
        }
    }
}