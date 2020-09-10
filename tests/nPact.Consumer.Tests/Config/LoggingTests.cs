using System;
using nPact.Common.Contracts;
using nPact.Common.Extensions;
using Xunit;

namespace nPact.Consumer.Tests.Config
{
    [Collection("Configuration tests")]
    public class LoggingTests
    {
        [Theory]
        [InlineData(LogLevel.Verbose, LogLevel.Verbose, true)]
        [InlineData(LogLevel.Info, LogLevel.Verbose, false)]
        [InlineData(LogLevel.Error, LogLevel.Error, true)]
        [InlineData(LogLevel.Scarce, LogLevel.Info, false)]
        [InlineData(LogLevel.Scarce, LogLevel.Scarce, true)]
        [InlineData(LogLevel.Scarce, LogLevel.Error, true)]
        public void Logging_WithLogLevelInfo_OnlyOutputsWhenAtLevelOrHigher(LogLevel configLevel, LogLevel msgLevel, bool expectedWrite)
        {
            var config = nPact.Consumer.Config.Configuration.With.Log(null).LogLevel(configLevel);
            var result = string.Empty;
            config.LogSafe(msgLevel, "Should Not Write This");
            config = nPact.Consumer.Config.Configuration.With.Log(txt => result += txt).LogLevel(configLevel);
            config.LogSafe(msgLevel, "a");
            config.LogSafe(msgLevel, () => "b");
            var expected = expectedWrite ? "ab" : string.Empty;
            Assert.Equal(expected, result); 
        }
    }
}