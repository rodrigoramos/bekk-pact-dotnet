using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using nPact.Provider.Model;
using Xunit;

namespace nPact.Provider.Tests.Model
{
    public class RequestTest
    {
        // TODO: Criar testes em questão de case para Headers
        // TODO: Corrigir e implementar separação de cabeçalhos para o conteúdo e para a request
        [Fact]
        public void RequestWithContentTypeJson()
        {
            const string json = @"{ 'id': 99, 'name': 'Willian Knickersen' }";

            var request = new Request
            {
                Method = "GET",
                Path = "/uri-fortest",
                Headers = new Dictionary<string, string>
                {
                    {"content-type", "application/json; charset=utf-8"}
                },
                Body = JObject.Parse(json)
            };

            var message = request.BuildMessage();

            message.Content.Headers.Should().BeEquivalentTo(new Dictionary<string, string[]>
            {
                {"Content-Type", new[] {"application/json; charset=utf-8"}}
            });
        }


        [Fact]
        public void RequestWithCustomHeaders()
        {
            const string json = @"{ 'id': 99, 'name': 'Willian Knickersen' }";

            var request = new Request
            {
                Method = "GET",
                Path = "/uri-fortest",
                Headers = new Dictionary<string, string>
                {
                    {"content-type", "application/json; charset=utf-8"},
                    {"X-Correlation-ID", System.Guid.NewGuid().ToString()},
                    {"__ABC_ANTI_CSRF_LOGIN__", "312.123.123-12"}
                },
                Body = JObject.Parse(json)
            };

            var message = request.BuildMessage();

            message.Content.Headers.Should().BeEquivalentTo(new Dictionary<string, string[]>
            {
                {"Content-Type", new[] {"application/json; charset=utf-8"}}
            });
        }
    }
}