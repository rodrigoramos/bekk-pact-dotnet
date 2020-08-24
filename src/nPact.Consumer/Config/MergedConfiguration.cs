using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using nPact.Common.Contracts;

namespace nPact.Consumer.Config
{
    class MergedConfiguration : IConsumerConfiguration
    {
        private readonly IEnumerable<IConsumerConfiguration> _configurations;

        public MergedConfiguration(params IConsumerConfiguration[] configurations)
        {
            _configurations = configurations.Where(c => c!=null).Reverse().ToList();
        }

        private T GetValue<T>(Func<IConsumerConfiguration,T> property) => _configurations.Select(property).FirstOrDefault(v => !object.Equals(v, default(T)));

        public Uri MockServiceBaseUri => GetValue(c => c.MockServiceBaseUri);

        public Uri BrokerUri => GetValue(c => c.BrokerUri);

        public Action<string> Log => GetValue(c => c.Log);
        public LogLevel? LogLevel => GetValue(c => c.LogLevel);

        public string PublishPath => GetValue(c => c.PublishPath);
        public string LogFile => GetValue(c => c.LogFile);

        public string BrokerUserName => GetValue(c => c.BrokerUserName);

        public string BrokerPassword => GetValue(c => c.BrokerPassword);

        public StringComparison? BodyKeyStringComparison => GetValue(c => c.BodyKeyStringComparison);

        /// <summary>
        /// Merges two configurations, letting <paramref name="right"/> override <paramref name="left"/>.
        /// </summary>
        /// <param name="left">The original config. May be <c>null</c>.</param>
        /// <param name="right">The overriding config. May be <c>null</c>.</param>
        /// <returns>If any parameter is null, the method may return the other parameter. Otherwise a merged configuration object.</returns>
        public static IConsumerConfiguration MergeConfigs(IConsumerConfiguration left, IConsumerConfiguration right)
        {
            if(left == null)
            {
                return right;
            }
            else
            {
                return right == null ? left : new MergedConfiguration(left, right);
            }
        }
    }

}