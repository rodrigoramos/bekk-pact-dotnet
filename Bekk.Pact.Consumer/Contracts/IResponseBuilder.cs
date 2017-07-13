﻿using System.Net;

namespace Bekk.Pact.Consumer.Contracts
{
    public interface IResponseBuilder
    {
        IResponseBuilder WithStatus(HttpStatusCode statusCode);
        IResponseBuilder WithStatus(int statusCode);
        IResponseBuilder WithHeader(string key, params string[] values);
        IResponseBuilder WithBody(object body);
        IPact InPact();
    }
}