using System;

namespace SingletonSessionHelpers.Abstraction;

/// <summary>
/// Interface declaration for disposable singleton session service.
/// </summary>
public interface IDisposableSessionService : ISessionService, IDisposable, IAsyncDisposable
{

}
