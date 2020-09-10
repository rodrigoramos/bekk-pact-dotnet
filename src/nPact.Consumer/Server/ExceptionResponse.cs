using nPact.Common.Contracts;
using nPact.Common.Utils;
using nPact.Consumer.Contracts;
using nPact.Consumer.Rendering;

namespace nPact.Consumer.Server
{
    class ExceptionResponse : IPactResponseDefinition
    {
        public ExceptionResponse(System.Exception exception)
        {
            if (exception == null)
            {
                throw new System.ArgumentNullException(nameof(exception));
            }

            ResponseStatusCode = 500;
            ResponseHeaders.Add("Exception-Type", exception.GetType().ToString());
            ResponseHeaders.Add("Exception-Message", exception.Message);
            ResponseHeaders.Add("Content-Type", "application/json; charset=utf-8");
            ResponseBody = new JsonBody(new
            {
                Message = exception.Message,
                Type = exception.GetType(),
                StackTrace = exception.StackTrace
            });
        }
        public IHeaderCollection ResponseHeaders { get; } = new HeaderCollection();

        public int? ResponseStatusCode { get; }

        public IJsonable ResponseBody { get; }
    }
}