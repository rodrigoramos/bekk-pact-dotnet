using System;

namespace nPact.Common.Exceptions
{
    public class PactBrokerException : PactException
    {
        public PactBrokerException(string message) : base(message)
        {
        }

        public PactBrokerException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}