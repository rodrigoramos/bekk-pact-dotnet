using Newtonsoft.Json.Linq;

namespace nPact.Common.Extensions
{
    public static class JsonExtensions
    {
        public static bool IsNull(this JToken token)=> token == null || token.Type == JTokenType.Null || token.Type == JTokenType.None;
        public static bool IsNullOrEmpty(this JToken token) => token.IsNull() ||
                (token.Type == JTokenType.Array && !token.HasValues) ||
                (token.Type == JTokenType.Object && !token.HasValues) ||
                (token.Type == JTokenType.String && token.ToString() == string.Empty);
    }
}