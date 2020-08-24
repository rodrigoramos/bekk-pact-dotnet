using System;
using nPact.Common.Exceptions;

namespace nPact.Provider.Web.Setup
{
    public class SetupException : PactException
    {
        public SetupException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}