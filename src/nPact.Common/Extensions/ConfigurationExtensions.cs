using System;
using nPact.Common.Contracts;
using nPact.Common.Exceptions;

namespace nPact.Common.Extensions
{
    public static class ConfigurationExtensions
    {
        public static void LogSafe(this Contracts.IConfiguration config, LogLevel level, string text)
        {
            if(config == null || level > config.GetLevel() || text == null) return;
            config.Log?.Invoke(text);
            LogToFile(config, level, text);
        }
        public static void LogSafe(this Contracts.IConfiguration config, LogLevel level, Func<string> logMsg)
        {
            if(config == null || level > config.GetLevel() || logMsg == null) return;
            var text = logMsg.Invoke();
            config.Log?.Invoke(text);
            LogToFile(config, level, text);
        }
        private static void LogToFile(Contracts.IConfiguration config, LogLevel level, string text)
        {
            var path = config.LogFile;
            if(string.IsNullOrWhiteSpace(path)) return;
            var txt = $"{DateTime.Now:o} {level}: {text}{Environment.NewLine}";
            try
            {
                System.IO.File.AppendAllText(path, txt);
            }
            catch(Exception e)
            {
                throw new ConfigurationException($"Couldn't write to log file at {path} ({e.Message}).", config);
            }
        }
        public static LogLevel GetLevel(this Contracts.IConfiguration configuration) => (configuration?.LogLevel).GetValueOrDefault(LogLevel.Scarce);
    }
}