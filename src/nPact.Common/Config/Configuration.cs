using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using nPact.Common.Contracts;
using nPact.Common.Exceptions;

namespace nPact.Common.Config
{
    public abstract class Configuration<T> : IConfiguration where T: class
    {
        protected Configuration(IConfiguration inner)
        {
            Inner = inner;
            Log(Console.WriteLine);
            comparison = StringComparison.CurrentCultureIgnoreCase;
        }
        private Uri brokerUri;
        private string brokerUserName;
        private string brokerPassword;
        private string publishPath;
        private Action<string> log;
        private LogLevel logLevel = Contracts.LogLevel.Scarce;
        private string logFile;
        protected IConfiguration Inner { get; }
        /// <summary>
        /// Read configuration from a json file.
        /// </summary>
        public T ConfigurationFile(string filePath)
        {
            try
            {
                using(var stream = File.OpenText(filePath))
                using(var reader = new JsonTextReader(stream))
                {
                    var json = JToken.ReadFrom(reader);
                    var pact = json["nPact"]?["Pact"];
                    if(pact != null)
                    {
                        ReadFromJson(pact);
                        Read(pact, nameof(IConfiguration.BrokerUri), (Uri uri) => BrokerUri(uri));
                        Read(pact, nameof(IConfiguration.BrokerUserName), (v) => { brokerUserName = v; return this as T;});
                        Read(pact, nameof(IConfiguration.BrokerPassword), (v) => { brokerPassword = v; return this as T;});
                        Read(pact, nameof(IConfiguration.PublishPath), PublishPath);
                        ReadEnum<LogLevel>(pact, nameof(IConfiguration.LogLevel), LogLevel);
                        Read(pact, nameof(IConfiguration.LogFile), LogFile);
                    }
                }
                return this as T;
            }
            catch(FileNotFoundException e)
            {
                throw new ConfigurationException("Configuration file not found.", this, e);
            }
        }

        protected void Read<TData>(JToken json, string key, Func<TData,T> setter)
        {
            var value = json.Value<TData>(key);
            if(!Equals(value, default(TData))) setter(value);
        }
        protected void Read(JToken json, string key, Func<string,T> setter)
        {
            var value = json.Value<string>(key);
            if(!string.IsNullOrWhiteSpace(value)) setter(value);
        }
        protected void Read(JToken json, string key, Func<Uri,T> setter)
        {
            Read(json, key, (url) => {
                if(!string.IsNullOrWhiteSpace(url)) setter(new Uri(url));
                return this as T;
            });
        }
        protected void ReadEnum<TData>(JToken json, string key, Func<TData,T> setter)
        {
            Read(json, key, (v) => setter((TData)Enum.Parse(typeof(TData),v)));
        }

        protected abstract void ReadFromJson(JToken json);

        /// <summary>
        /// Sets the values of <see cref="IConfiguration.BrokerUserName" /> and <see cref="IConfiguration.BrokerPassword" />.
        /// </summary>
        public T BrokerCredentials(string userName, string password)
        {
            brokerUserName = userName;
            brokerPassword = password;
            return this as T;
        }
        string IConfiguration.BrokerUserName => Inner?.BrokerUserName ?? brokerUserName;
        string IConfiguration.BrokerPassword => Inner?.BrokerPassword ?? brokerPassword;
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.BrokerUri"/>
        /// </summary>
        public T BrokerUri(Uri uri)
        {
            brokerUri = uri;
            return this as T;
        }
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.BrokerUri"/>.
        /// Must be parsable to an absolute uri.
        /// </summary>
        public T BrokerUrl(string url) => BrokerUri(new Uri(url));
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.Log"/>.
        /// </summary>
        public T Log(Action<string> log){
            this.log = log;
            return this as T;
        }
        Action<string> IConfiguration.Log => Inner?.Log ?? log;
        Uri IConfiguration.BrokerUri => Inner?.BrokerUri ?? brokerUri;
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.PublishPath"/>.
        /// </summary>
        public T PublishPath(string path)
        {
            publishPath = path;
            return this as T;
        }
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.PublishPath"/>.
        /// </summary>
        /// <param name="path">A path appended to the temp path.</param>
        public T PublishPathInTemp(string path = null) => PublishPath(path == null ? Path.GetTempPath() : Path.Combine(Path.GetTempPath(), path));
        string IConfiguration.PublishPath => Inner?.PublishPath ?? publishPath;
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.LogLevel"/>.
        /// </summary>
        public T LogLevel(LogLevel level)
        {
            logLevel = level;
            return this as T;
        }
        LogLevel? IConfiguration.LogLevel => Inner?.LogLevel ?? logLevel;
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.LogFile"/>.
        /// </summary>
        public T LogFile(string path)
        {
            logFile = path;
            return this as T;
        }
        /// <summary>
        /// Sets the value of <see cref="IConfiguration.LogFile"/>.
        /// </summary>
        /// <param name="filename">The filename of the logfile in the temp folder.</param>
        public T LogFileInTemp(string filename) => LogFile(Path.Combine(Path.GetTempPath(), filename));
        string IConfiguration.LogFile => Inner?.LogFile ?? logFile;

        private StringComparison comparison;
        public T Comparison(StringComparison comparison)
        {
            this.comparison = comparison;
            return this as T;
        }
        StringComparison? IConfiguration.BodyKeyStringComparison => Inner?.BodyKeyStringComparison ?? comparison;
    }
}