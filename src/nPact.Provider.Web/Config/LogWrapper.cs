using System;
using nPact.Common.Contracts;
using nPact.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace nPact.Provider.Web.Config
{
    public class LogWrapper : ILoggerProvider, ILogger
    {
        private IProviderConfiguration configuration;

        public LogWrapper(IProviderConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return this;
        }
        
        public void Dispose()
        {

        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            switch (configuration.GetLevel()){
                case nPact.Common.Contracts.LogLevel.Error: return logLevel > Microsoft.Extensions.Logging.LogLevel.Critical;
                case nPact.Common.Contracts.LogLevel.Scarce: return logLevel >= Microsoft.Extensions.Logging.LogLevel.Warning;
                case nPact.Common.Contracts.LogLevel.Info: return logLevel >=  Microsoft.Extensions.Logging.LogLevel.Information;
                default: return true;
            }
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            configuration.LogSafe(Convert(logLevel), formatter(state, exception));
        }

        private nPact.Common.Contracts.LogLevel Convert(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            if(logLevel < Microsoft.Extensions.Logging.LogLevel.Information) return nPact.Common.Contracts.LogLevel.Verbose;
            if(logLevel == Microsoft.Extensions.Logging.LogLevel.Information) return nPact.Common.Contracts.LogLevel.Info;
            if(logLevel == Microsoft.Extensions.Logging.LogLevel.Warning) return nPact.Common.Contracts.LogLevel.Scarce;
            return nPact.Common.Contracts.LogLevel.Error;
        }
    }
}