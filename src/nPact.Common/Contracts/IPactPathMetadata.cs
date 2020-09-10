using System;

namespace nPact.Common.Contracts
{
    public interface IPactPathMetadata
    {
        string Provider { get; }
        string Consumer { get; }
        Version Version { get; }
    }
}