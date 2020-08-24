using System.IO;
using System.Text;
using nPact.Common.Contracts;
using nPact.Consumer.Config;
using Newtonsoft.Json.Linq;
using Xunit;

namespace nPact.Consumer.Tests.Config
{
    [Collection("Configuration tests")]
    public class JsonConfigTests
    {
        [Fact]
        public void LoadConfigFile_PopulatesProperties()
        {
            const string brokerUri = "https://www.bekk.no:7896/";
            const string userName = "some_user";
            const string mockUri = "http://localhost:9000/";
            const string logFilePath = "somePath";
            var json = new JObject(
                new JProperty("Bekk",
                    new JObject(
                        new JProperty("Pact", 
                            new JObject(
                                new JProperty("BrokerUserName", userName),
                                new JProperty("BrokerUri", brokerUri),
                                new JProperty("LogLevel", "Info"),
                                new JProperty("Consumer", 
                                    new JObject(
                                        new JProperty("MockServiceBaseUri", mockUri)
                                    ))
                            )))));
            
            var filePath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(filePath, json.ToString());
                IConsumerConfiguration config = Configuration.With
                    .LogFile(logFilePath)
                    .BrokerUrl("http://shouldBeOverwritten:80")
                    .ConfigurationFile(filePath);

                Assert.Equal(brokerUri, config.BrokerUri.ToString());
                Assert.Equal(userName, config.BrokerUserName);
                Assert.Equal(mockUri, config.MockServiceBaseUri.ToString());
                Assert.Equal(logFilePath, config.LogFile);
                Assert.Equal(LogLevel.Info, config.LogLevel);
            }
            finally
            {
                File.Delete(filePath);
            }            
        }
    }
}