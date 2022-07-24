using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SingletonSessionHelpers.Abstraction;

public interface ISessionService : IAsyncDisposable
{
    ValueTask InitializeAsync(CancellationToken cancellationToken = default);

    ValueTask UpdateAsync(CancellationToken cancellationToken = default);
}
