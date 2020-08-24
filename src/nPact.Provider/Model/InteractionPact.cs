using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using nPact.Common.Contracts;
using nPact.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using nPact.Provider.Contracts;
using nPact.Provider.Model.Validation;

namespace nPact.Provider.Model
{
    class InteractionPact : IPact
    {
        private readonly Interaction interaction;
        private readonly JObject json;

        public InteractionPact(Interaction interaction, IProviderConfiguration config, JObject json)
        {
            this.interaction = interaction;
            Configuration = config;
            Description = interaction.Description;
            this.json = json;
        }
        public string ProviderState => interaction.ProviderState;

        public async Task<ITestResult> Verify(HttpClient client)
        {
            Configuration.LogSafe(LogLevel.Scarce, ToString());
            Configuration.LogSafe(LogLevel.Verbose, $"Provider state: {ProviderState}");
            var response = await client.SendAsync(interaction.Request.BuildMessage());
            var expected = interaction.Response;
            var errors = new Result(ToString(), interaction, expected);
            if (response.StatusCode != expected.Status)
            {
                errors.Add(ValidationTypes.StatusCode,$"Status code was {response.StatusCode}. Expected {expected.Status}.");
            }
            errors.Add(ValidationTypes.Headers, new ResponseHeadersValidator().Validate(expected, response.Content.Headers));
            errors.Add(ValidationTypes.Body, await ValidateBody(response.Content, expected));
            errors.Add(response);
            return errors;
        }

        public IProviderConfiguration Configuration { get; }

        public string Description { get; }

        private async Task<string> ValidateBody(HttpContent actual, Response expected)
        {
            var contentType = GetHeader("content-type", expected.Headers)?.Split(';').Select(p => p.Trim()).ToArray();
            if(contentType != null && contentType.Any() && contentType[0] == "application/json"){
                try
                {
                    var actualAsString = await actual.ReadAsStringAsync();
                    Configuration.LogSafe(LogLevel.Verbose, $"Response body: \n{actualAsString}");
                    return new ResponseBodyJsonValidator(Configuration).Validate(expected.Body, actualAsString);
                }
                catch (JsonReaderException exception)
                {
                    return $"Error reading body ({exception.Message})";
                }
            }
            else
            {
                throw new NotImplementedException($"Only content type json is implemented. This seems to be {string.Join(", ", contentType)}");
            }
        }

        private string GetHeader(string header, IDictionary<string,string> headers)
        {
            var values =  headers.Where(h => h.Key.Equals(header, StringComparison.OrdinalIgnoreCase))
                                 .Select(h => h.Value).ToList();
            if(values.Any()) return string.Join(",", values);
            return null;
        }

        public override string ToString() => $"{interaction.Consumer}: {interaction.Request.Method} {interaction.Request.Path} [{interaction.Description}]";
        string IPact.ToString(bool asJson) => asJson ? json.ToString() : ToString();
    }
}