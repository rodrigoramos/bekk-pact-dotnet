using System;
using Newtonsoft.Json.Linq;
using nPact.Common.Contracts;

namespace nPact.Common.Utils
{
    public class Jsonable : IJsonable
    {
        private readonly Func<JContainer> render;
        public Jsonable(Func<JContainer> render)
        {
            this.render = render;
        }
        public Jsonable(JContainer json) : this(()=>json){}
        public Jsonable(string json)
        {
            render = () => (JContainer) JToken.Parse(json);
        }
        public JContainer Render() => render();
    }
}