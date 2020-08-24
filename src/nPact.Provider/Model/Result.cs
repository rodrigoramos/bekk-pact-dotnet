using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
            this.errors = errors.ToList(); ;
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

        public override string ToString() => Success ? $"Ok ({Title})" : 
            string.Concat(
                $"Validation has failed for {Title} from {Consumer}:",
                Environment.NewLine,
                new string('-', 3),
                Environment.NewLine,
                string.Join(Environment.NewLine, errors));

    }
}