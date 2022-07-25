using SingletonSessionHelpers.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SingletonSessionHelpers.Abstraction;

/// <summary>
/// Interface declaration for all singleton session service.
/// </summary>
public interface ISessionService : IAsyncDisposable
{
    /// <summary>
    /// Initializes the session service.
    /// </summary>
    /// <param name="onError">
    /// Callback for the initialization error to provide option to retry the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    ValueTask InitializeAsync(Func<RetryIfErrorArgs, Task>? onError, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the session service.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    ValueTask InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the session service and forget.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    void Initialize(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the session service.
    /// </summary>
    /// <param name="onError">
    /// Callback for the update error to provide option to retry the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    ValueTask UpdateAsync(Func<RetryIfErrorArgs, Task>? onError, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the session service.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    ValueTask UpdateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the session service and forget.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    void Update(CancellationToken cancellationToken = default);
}
