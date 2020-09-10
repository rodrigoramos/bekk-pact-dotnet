using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using nPact.Common.Contracts;

namespace nPact.Common.Utils
{
    public class HeaderCollection: IHeaderCollection
    {
        private Dictionary<string, string> headers = new Dictionary<string, string>();
        public string this[string key]
        {
            get
            {
                return headers.TryGetValue(key.ToLower(), out var value) ? value : null;
            }
            set
            {
                var normalizedKey = key.ToLower();
                if(headers.ContainsKey(normalizedKey))
                {
                    if(string.IsNullOrWhiteSpace(value))
                    {
                        headers.Remove(normalizedKey);
                    }
                    else
                    {
                        headers[normalizedKey] += $", {value}";
                    }
                }
                else
                {
                    if(!string.IsNullOrWhiteSpace(value)) headers.Add(normalizedKey, value);
                }
            }
        }
        public IHeaderCollection Add(string key, params string[] values)
        {
            VerifyKey(key);
            var value = string.Join(", ", values);
            if(string.IsNullOrEmpty(value)) return this;
            var normalizedKey = key.ToLower();
            if(headers.ContainsKey(normalizedKey))
            {
                headers[normalizedKey] += $", {value}";
            }
            else
            {
                headers.Add(normalizedKey, value);
            }
            return this;
        }
        public IHeaderCollection ParseAndAdd(string header)
        {
            if(string.IsNullOrWhiteSpace(header)) return this;
            var separator = header.IndexOf(":");
            if(separator < 1) throw new ArgumentException($"Cannot parse header \"{header}\"", nameof(header));
            var key = header.Substring(0, separator).Trim();
            var value = header.Substring(separator+1).Trim();
            return Add(key, value);
        }

        public IHeaderCollection AddIfAbsent(string key, params string[] values) => headers.ContainsKey(key) ? this : Add(key, values);

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return headers.Keys.Select((k)=>new KeyValuePair<string, string>(k, headers[k])).GetEnumerator();
        }

        private void VerifyKey(string key)
        {
            if(string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key), "Header key is empty.");
            if(Regex.IsMatch(key, @"\s+")) throw new ArgumentException("Header key may not contain whitespace", nameof(key));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString() => 
            string.Join(Environment.NewLine, headers.Select(hdr => $"{hdr.Key}: {hdr.Value}"));
    }
}