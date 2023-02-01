using SingletonSessionHelpers.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DisposableHelpers.Attributes;

namespace SingletonSessionHelpers.Abstraction;

/// <summary>
/// Implementation for all singleton session service.
/// </summary>
[AsyncDisposable]
public abstract partial class DisposableSessionService : SessionService, IDisposableSessionService
{
    #region Disposable Logic

    /// <summary>
    /// Provides an overridable method for pre despose phase of the session.
    /// </summary>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    protected virtual ValueTask PreDisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    /// <summary>
    /// Provides an overridable method for post despose phase of the session.
    /// </summary>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    protected virtual ValueTask PostDisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    /// <inheritdoc/>
    protected async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await PreDisposeAsync();

            foreach (var service in SubscribedSessionServices)
            {
                if (service is IDisposableSessionService sessionService)
                {
                    await sessionService.DisposeAsync();
                }
            }

            await PostDisposeAsync();
        }
    }

    /// <inheritdoc/>
    public async void Dispose()
    {
        await DisposeAsync();
    }

    #endregion
}
