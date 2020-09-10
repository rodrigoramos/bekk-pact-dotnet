using System;
using System.Collections.Generic;
using System.Linq;
using nPact.Common.Exceptions;
using nPact.Provider.Contracts;

namespace nPact.Provider.Exceptions
{
    public class AssertionFailedException : PactException
    {
        public AssertionFailedException(IEnumerable<ITestResult> results)
            :base(GenerateError(results))
        {
            TestResults = results;
        }

        public IEnumerable<ITestResult> TestResults { get; }

        private static string GenerateError(IEnumerable<ITestResult> results)
        {
            var errors = results.Where(r => ! r.Success).ToList();
            if(!errors.Any()) throw new ArgumentException("No test results without success.", nameof(results));
            var messages = string.Join(Environment.NewLine, errors.Select(e => e.ToString()));
            return $"Assertions has failed for {errors.Count()} pacts. {messages}";
        }
    }
}