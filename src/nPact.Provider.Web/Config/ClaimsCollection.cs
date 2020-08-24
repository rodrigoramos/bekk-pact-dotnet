using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;

namespace nPact.Provider.Web.Config
{
    public class ClaimsCollection : IEnumerable<Claim>
    {
        private List<Claim> claims;
        private ClaimsCollection()
        {
            claims = new List<Claim>();
        }

        public static ClaimsCollection With(string type, string value) => new ClaimsCollection().And(type, value);

        public ClaimsCollection And(string type, string value)
        {
            claims.Add(new Claim(type, value));
            return this;
        }

        public IEnumerator<Claim> GetEnumerator()
        {
            return claims.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}