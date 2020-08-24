using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace nPact.Consumer.Extensions
{
    [JsonObject]
    public class FormData
    {
        private readonly Dictionary<string, string> values = new Dictionary<string, string>();
        private FormData()
        {
        }
        public static FormData With<T>(string key, T data) => new FormData().And(key, data);
        public FormData And<T>(string key, T value)
        {
            string serialized;
            if(Nullable.GetUnderlyingType(typeof(T)) != null && value == null) 
            {
                serialized = null;
            }
            else
            switch(value)
            {
                case string str:
                {
                    serialized = str;
                    break;
                }
                case int i:
                {
                    serialized = i.ToString(CultureInfo.InvariantCulture);
                    break;
                }
                case double d:
                {
                    serialized = d.ToString("G", CultureInfo.InvariantCulture);
                    break;
                }
                case DateTime date:
                {
                    serialized = date.ToString("s");
                    break;
                }
                default:
                {
                    serialized = value?.ToString();
                    break;
                }
            }
            values.Add(key, serialized);
            return this;
        }
        public static explicit operator Dictionary<string, string>(FormData data) => data.values;
    }
}