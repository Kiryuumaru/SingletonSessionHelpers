using SingletonSessionHelpers.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SingletonSessionHelpers.Abstraction;

/// <summary>
/// Interface declaration for disposable singleton session service.
/// </summary>
public interface IDisposableSessionService : ISessionService, IDisposable, IAsyncDisposable
{

}
