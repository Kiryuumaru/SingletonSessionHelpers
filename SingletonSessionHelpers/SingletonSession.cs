using DisposableHelpers;
using DisposableHelpers.Attributes;
using SingletonSessionHelpers.Abstraction;
using SingletonSessionHelpers.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SingletonSessionHelpers;

public abstract partial class SingletonSession : SessionService
{
    internal SingletonSession() { }
}

public abstract partial class SingletonSession<TConfig> : SingletonSession
    where TConfig : SingletonSessionConfig
{
    public TConfig Config { get; }

    public SingletonSession(TConfig config)
    {
        Config = config;
    }
}
