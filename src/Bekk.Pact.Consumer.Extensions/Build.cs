using System;
using System.Diagnostics;
using System.Reflection;
using Bekk.Pact.Consumer.Attributes;
using Bekk.Pact.Consumer.Builders;
using Bekk.Pact.Consumer.Contracts;
using Bekk.Pact.Consumer.Server;

namespace Bekk.Pact.Consumer
{
    public static class Build
    {
        public static IPactBuilder Pact(string description)
        {
            var stack = new StackTrace();
            var caller = stack.GetFrame(1).GetMethod();
            return PactBuilder.Build(description)
                .Between(GetAttribute<ProviderNameAttribute>(caller)?.Name)
                .And(GetAttribute<ConsumerNameAttribute>(caller)?.Name);
        }
        private static T GetAttribute<T>(MethodBase method) where T:Attribute
        {
            var attr = method.GetCustomAttribute<T>();
            if(attr != null) return attr;
            attr = method.DeclaringType.GetCustomAttribute<T>();
            if(attr != null) return attr;
            return method.DeclaringType.Assembly.GetCustomAttribute<T>();
        }
    }
}