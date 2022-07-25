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

/// <summary>
/// The non-generic base class for the singleton session service.
/// </summary>
public abstract partial class SingletonSession : SessionService
{
    internal SingletonSession() { }
}

/// <summary>
/// The base class for a singleton session service.
/// </summary>
/// <typeparam name="TConfig">
/// The type of the config used by the singleton session service.
/// </typeparam>
public abstract partial class SingletonSession<TConfig> : SingletonSession
    where TConfig : SingletonSessionConfig
{
    /// <summary>
    /// Gets the <typeparamref name="TConfig"/> of the singleton session service.
    /// </summary>
    public TConfig Config { get; }

    /// <summary>
    /// Creates an instance of the <see cref="SingletonSession{TConfig}"/>.
    /// </summary>
    /// <param name="config">
    /// The <typeparamref name="TConfig"/> fot the singleton session service.
    /// </param>
    public SingletonSession(TConfig config)
    {
        Config = config;
    }
}
