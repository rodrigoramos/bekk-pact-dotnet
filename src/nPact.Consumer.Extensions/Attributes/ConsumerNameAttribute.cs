using System;

namespace nPact.Consumer.Extensions.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public class ConsumerNameAttribute : Attribute
    {
        public string Name { get; }
        public ConsumerNameAttribute(string name)
        {
            Name = name;
        }
        public override string ToString() => Name;
    }
}