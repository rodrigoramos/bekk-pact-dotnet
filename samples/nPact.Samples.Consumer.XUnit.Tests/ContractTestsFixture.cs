using System;
using System.IO;
using nPact.Consumer.Config;
using nPact.Consumer.Server;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace nPact.Samples.Consumer.XUnit.Tests
{
    class ContractTestsFixture : IDisposable
    {
        private readonly string _pactsLocation = Path.Combine("..", "..", "..", "sample-pacts");
        private readonly string _logsLocation = Path.Combine("..", "..", "..", "contract.log");

        private readonly Context _serverContext;

        public ContractTestsFixture(IMessageSink diagnosticMessageSink)
        {
            var configuration = Configuration.With
                .LogLevel(Common.Contracts.LogLevel.Verbose)
                .MockServiceBaseUri("http://localhost:5001")
                .LogFile(_logsLocation)
                .Log(logMessage => diagnosticMessageSink.OnMessage(new DiagnosticMessage(logMessage)))
                .PublishPath(_pactsLocation);

            _serverContext = new Context(configuration)
                .ForConsumer("nPact.Samples.Consumer");
        }
        
        public void Dispose() => _serverContext?.Dispose();
    }
}