using System;
using nPact.Consumer.Config;
using nPact.Common.Contracts;
using Xunit;

namespace nPact.Consumer.Tests.Config
{
    [Collection("Configuration tests")]
    public class EnvironmentVariableConfigTests : IDisposable
    {
        public void Dispose()
        {
            Environment.SetEnvironmentVariable("nPact:Pact:BrokerPassword", null);
            Environment.SetEnvironmentVariable("nPact:Pact:LogLevel", null);
            Environment.SetEnvironmentVariable("nPact:Pact:Consumer:MockServiceBaseUri", null);
            Environment.SetEnvironmentVariable("nPact:Pact:PublishPath", null);
            Environment.SetEnvironmentVariable("nPact:Pact:LogFile", null);
            Environment.SetEnvironmentVariable("nPact:Pact:LogLevel", null);
            Environment.SetEnvironmentVariable("nPact__Pact__BrokerUserName", null);            
            Environment.SetEnvironmentVariable("nPact__Pact__Consumer__MockServiceBaseUri", null);            
            Environment.SetEnvironmentVariable("nPact__Pact__BrokerUri", null);
        }

        [Fact]
        public void FromEnvironmentVariables_ReturnsObjectReadFromEnvironment()
        {
            Environment.SetEnvironmentVariable("nPact:Pact:BrokerPassword", "test0");
            Environment.SetEnvironmentVariable("nPact:Pact:LogLevel", "Error");
            Environment.SetEnvironmentVariable("nPact:Pact:Consumer:MockServiceBaseUri", "http://localhost:12");
            var target = Configuration.FromEnvironmentVartiables();

            Assert.Equal("test0", target.BrokerPassword);
            Assert.Equal(new Uri("http://localhost:12"), target.MockServiceBaseUri);
            Assert.Equal(LogLevel.Error, target.LogLevel);
        }

        [Fact]
        public void With_SettingValuesInCode_EnvironmentVariablesOverrides()
        {
            Environment.SetEnvironmentVariable("nPact:Pact:PublishPath", "expectedPath");
            Environment.SetEnvironmentVariable("nPact:Pact:LogFile", "expectedLogFile");
            Environment.SetEnvironmentVariable("nPact:Pact:LogLevel", "Info");
            Environment.SetEnvironmentVariable("nPact:Pact:Consumer:MockServiceBaseUri", "http://localhost:42");

            IConsumerConfiguration target = Configuration.With
                .PublishPath("Not expected")
                .LogFile("Not expected")
                .LogLevel(LogLevel.Verbose);

            Assert.Equal("expectedPath", target.PublishPath);
            Assert.Equal("expectedLogFile", target.LogFile);
            Assert.Equal(LogLevel.Info, target.LogLevel);
            Assert.Equal("http://localhost:42/", target.MockServiceBaseUri?.ToString());
        }


        [Fact]
        public void FromEnvironmentVariables_UsingUnderscore_ReturnsObjectReadFromEnvironment()
        {
            Environment.SetEnvironmentVariable("nPact__Pact__BrokerUserName", "test1");
            Environment.SetEnvironmentVariable("nPact__Pact__Consumer__MockServiceBaseUri", "http://localhost:21");
            Environment.SetEnvironmentVariable("nPact__Pact__BrokerUri", "https://pact-broker-url/");
            

            var target = Configuration.FromEnvironmentVartiables();

            Assert.Equal("test1", target.BrokerUserName);
            Assert.Equal("http://localhost:21/", target.MockServiceBaseUri?.ToString());
            Assert.Equal("https://pact-broker-url/", target.BrokerUri?.ToString());
        }
    }
}