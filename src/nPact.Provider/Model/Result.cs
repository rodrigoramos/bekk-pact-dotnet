using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using nPact.Common.Contracts;
using nPact.Provider.Contracts;

namespace nPact.Provider.Model
{
    class Result : ITestResult
    {
        private readonly List<string> errors;
        private readonly Response expected;

        public Result(string title, IPactInformation info, Response expected) : this(title, info, ValidationTypes.None)
        {
            this.expected = expected;
        }

        public Result(string title, IPactInformation info, ValidationTypes types, params string[] errors)
        {
            this.errors = errors.ToList();
            ErrorTypes = types;
            Title = title;
            Description = info.Description;
            Consumer = info.Consumer;
            ProviderState = info.ProviderState;
        }

        public bool Success => ErrorTypes == ValidationTypes.None;

        public void Add(ValidationTypes type, string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                ErrorTypes |= type;
                errors.Add(error);
            }
        }

        public void Add(ValidationTypes type, IEnumerable<string> errors)
        {
            if (errors != null)
            {
                foreach (var error in errors)
                {
                    Add(type, error);
                }
            }
        }

        public void Add(HttpResponseMessage response)
        {
            ActualResponse = response;
        }

        public ValidationTypes ErrorTypes { get; private set; }
        public string Title { get; }

        public string ExpectedResponseBody => expected.Body.ToString();

        public HttpResponseMessage ActualResponse { get; private set; }

        public string Consumer { get; }

        public string Description { get; }

        public string ProviderState { get; }

        public override string ToString()
        {
            if (Success)
                return $"Ok ({Title})";

            var actualResponseLogTask = LogActualResponse(ActualResponse);
            actualResponseLogTask.Wait();

            return string.Concat(
                $"Validation has failed for {Title} from {Consumer}:",
                Environment.NewLine,
                new string('-', 3),
                Environment.NewLine,
                string.Join(Environment.NewLine, errors),
                Environment.NewLine,
                new string('-', 3),
                Environment.NewLine,
                actualResponseLogTask.Result,
                new string('-', 3),
                Environment.NewLine,
                LogExpectedResponse(expected),
                Environment.NewLine
            );
        }

        private static async Task<string> LogActualResponse(HttpResponseMessage responseMessage)
        {
            return new StringBuilder($"Actual Response{Environment.NewLine}")
                .Append("Status Code: ").AppendLine(responseMessage.StatusCode.ToString())
                .Append("Reason Phrase: ").AppendLine(responseMessage.ReasonPhrase)
                .AppendLine("Headers").Append(responseMessage.Headers)
                .Append(responseMessage.Content.Headers.ToString())
                .Append("Content: ").AppendLine(await responseMessage.Content.ReadAsStringAsync())
                .ToString();
        }

        private static string LogExpectedResponse(Response expectedResponse)
        {
            Func<IDictionary<string, string>, string> printDictionary = dict =>
                (!dict.Any()
                    ? string.Empty
                    : dict.Select(x => $"{x.Key}: {x.Value}")
                        .Aggregate((f, s) => $"{f}{Environment.NewLine}{s}"));

            return new StringBuilder($"Expected Response{Environment.NewLine}")
                .Append("Status Code: ").AppendLine(expectedResponse.Status.ToString())
                .AppendLine("Headers").AppendLine(printDictionary(expectedResponse.Headers))
                .Append("Content: ").AppendLine(expectedResponse.Body?.ToString())
                .ToString();
        }
    }
}