using System;
using nPact.Consumer.Config;
using nPact.Consumer.Server;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace nPact.Samples.Consumer.XUnit.Tests
{
    public class ContractTestsFixture : IDisposable
    {
        private const string PactsLocation = @"..\..\..\..\sample-pacts";
        private const string LogsLocation = @"..\..\..\contract.log";

        private readonly Context _serverContext;

        public ContractTestsFixture(IMessageSink diagnosticMessageSink)
        {
            var configuration = Configuration.With
                .LogLevel(Common.Contracts.LogLevel.Verbose)
                .MockServiceBaseUri("http://localhost:5001")
                .LogFile(LogsLocation)
                .Log(logMessage => diagnosticMessageSink.OnMessage(new DiagnosticMessage(logMessage)))
                .PublishPath(PactsLocation);

            _serverContext = new Context(configuration)
                .ForConsumer("nPact.Samples.Consumer");
        }
        
        public void Dispose() => _serverContext?.Dispose();
    }
}