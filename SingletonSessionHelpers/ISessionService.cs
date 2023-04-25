using System;
using System.Threading;
using System.Threading.Tasks;
using TransactionHelpers;

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
    event EventHandler<Exception>? InitializationFailed;

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
    event EventHandler<Exception>? UpdateFailed;

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
    /// Initializes the session service.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    ValueTask<Response> InitializeAsync(CancellationToken cancellationToken = default);

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
    /// <param name="initializeFirst">
    /// Sets to <c>true</c> whether the update will initialize the service first if not already initialized; otherwise, <c>false</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    ValueTask<Response> UpdateAsync(bool initializeFirst = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the session service and forget.
    /// </summary>
    /// <param name="initializeFirst">
    /// Sets to <c>true</c> whether the update will initialize the service first if not already initialized; otherwise, <c>false</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token for the operation.
    /// </param>
    void Update(bool initializeFirst = true, CancellationToken cancellationToken = default);
}
