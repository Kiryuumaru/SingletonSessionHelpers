using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TransactionHelpers;
using static SingletonSessionHelpers.Abstraction.SessionService;

namespace SingletonSessionHelpers.Abstraction;

/// <summary>
/// Interface declaration for singleton session service.
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Event invoked if the session is in initialization phase.
    /// </summary>
    event EventHandler? Initializing;

    /// <summary>
    /// Event invoked if the session is initialized.
    /// </summary>
    event EventHandler? Initialized;

    /// <summary>
    /// Event invoked if the session initialization has failed.
    /// </summary>
    event EventHandler<InitializationFailedEventArgs>? InitializationFailed;

    /// <summary>
    /// Event invoked if the session is updating.
    /// </summary>
    event EventHandler? Updating;

    /// <summary>
    /// Event invoked if the session is updated.
    /// </summary>
    event EventHandler? Updated;

    /// <summary>
    /// Event invoked if the session update has failed.
    /// </summary>
    event EventHandler<UpdateFailedEventArgs>? UpdateFailed;

    /// <summary>
    /// Event invoked if the session is initializing or updating.
    /// </summary>
    public event EventHandler? InitializingOrUpdating;

    /// <summary>
    /// Event invoked if the session is initialized or updated.
    /// </summary>
    public event EventHandler? InitializedOrUpdated;

    /// <summary>
    /// Gets <c>true</c> if the session is initialized; otherwise, <c>false</c>.
    /// </summary>
    public bool IsInitialized { get; }

    /// <summary>
    /// Gets <c>true</c> if the session is initializing; otherwise, <c>false</c>.
    /// </summary>
    public bool IsInitializing { get; }

    /// <summary>
    /// Gets <c>true</c> if the session is updating; otherwise, <c>false</c>.
    /// </summary>
    public bool IsUpdating { get; }

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> of the last update.
    /// </summary>
    public DateTimeOffset LastUpdate { get; }

    /// <summary>
    /// Initializes the session service.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    ValueTask<Result> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the session service.
    /// </summary>
    /// <param name="retryCallback">
    /// The callback if the operation responded with errors.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    ValueTask InitializeAsync(Func<Result, Task<bool>> retryCallback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the session service and forget.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    void Initialize(CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the session service and forget.
    /// </summary>
    /// <param name="retryCallback">
    /// The callback if the operation responded with errors.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    void Initialize(Func<Result, Task<bool>> retryCallback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the session service.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    ValueTask<Result> UpdateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the session service.
    /// </summary>
    /// <param name="initializeFirst">
    /// Sets to <c>true</c> whether the update will initialize the service first if not already initialized; otherwise, <c>false</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    ValueTask<Result> UpdateAsync(bool initializeFirst, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the session service.
    /// </summary>
    /// <param name="retryCallback">
    /// The callback if the operation responded with errors.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    ValueTask UpdateAsync(Func<Result, Task<bool>> retryCallback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the session service.
    /// </summary>
    /// <param name="initializeFirst">
    /// Sets to <c>true</c> whether the update will initialize the service first if not already initialized; otherwise, <c>false</c>.
    /// </param>
    /// <param name="retryCallback">
    /// The callback if the operation responded with errors.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    ValueTask UpdateAsync(bool initializeFirst, Func<Result, Task<bool>> retryCallback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the session service and forget.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    void Update(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the session service and forget.
    /// </summary>
    /// <param name="initializeFirst">
    /// Sets to <c>true</c> whether the update will initialize the service first if not already initialized; otherwise, <c>false</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    void Update(bool initializeFirst, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the session service and forget.
    /// </summary>
    /// <param name="retryCallback">
    /// The callback if the operation responded with errors.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    void Update(Func<Result, Task<bool>> retryCallback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the session service and forget.
    /// </summary>
    /// <param name="initializeFirst">
    /// Sets to <c>true</c> whether the update will initialize the service first if not already initialized; otherwise, <c>false</c>.
    /// </param>
    /// <param name="retryCallback">
    /// The callback if the operation responded with errors.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    void Update(bool initializeFirst, Func<Result, Task<bool>> retryCallback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all subscribed session services.
    /// </summary>
    IEnumerable<(bool isAsync, ISessionService sessionService)> GetSubscribedSessionServices();
}
