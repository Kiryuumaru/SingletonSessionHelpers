using DisposableHelpers.Attributes;
using SingletonSessionHelpers.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SingletonSessionHelpers.Abstraction;

/// <summary>
/// Implementation for all singleton session service.
/// </summary>
[AsyncDisposable]
public abstract partial class SessionService : ISessionService
{
    #region Events

    /// <summary>
    /// Event args for <see cref="InitializationFailed"/> event.
    /// </summary>
    public class InitializationFailedEventArgs : EventArgs
    {
        /// <summary>
        /// The exception of the error.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Creates a new instance for <see cref="InitializationFailedEventArgs"/>.
        /// </summary>
        /// <param name="exception">
        /// The exception of the error.
        /// </param>
        public InitializationFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }

    /// <summary>
    /// Event args for <see cref="UpdateFailed"/> event.
    /// </summary>
    public class UpdateFailedEventArgs : EventArgs
    {
        /// <summary>
        /// The exception of the error.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Creates a new instance for <see cref="UpdateFailedEventArgs"/>.
        /// </summary>
        /// <param name="exception">
        /// The exception of the error.
        /// </param>
        public UpdateFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }

    /// <inheritdoc/>
    public event EventHandler? Initializing;

    /// <inheritdoc/>
    public event EventHandler? Initialized;

    /// <inheritdoc/>
    public event EventHandler<InitializationFailedEventArgs>? InitializationFailed;

    /// <inheritdoc/>
    public event EventHandler? Updating;

    /// <inheritdoc/>
    public event EventHandler? Updated;

    /// <inheritdoc/>
    public event EventHandler<UpdateFailedEventArgs>? UpdateFailed;

    /// <summary>
    /// Event invoker for <see cref="Initializing"/> event.
    /// </summary>
    protected virtual void OnInitializing()
    {
        Initializing?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Event invoker for <see cref="Initialized"/> event.
    /// </summary>
    protected virtual void OnInitialized()
    {
        Initialized?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Event invoker for <see cref="InitializationFailed"/> event.
    /// </summary>
    /// <param name="exception">
    /// The exception of the error.
    /// </param>
    protected virtual void OnInitializationFailed(Exception exception)
    {
        InitializationFailed?.Invoke(this, new InitializationFailedEventArgs(exception));
    }

    /// <summary>
    /// Event invoker for <see cref="Updating"/> event.
    /// </summary>
    protected virtual void OnUpdating()
    {
        Updating?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Event invoker for <see cref="Updated"/> event.
    /// </summary>
    protected virtual void OnUpdated()
    {
        Updated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Event invoker for <see cref="UpdateFailed"/> event.
    /// </summary>
    /// <param name="exception">
    /// The exception of the error.
    /// </param>
    protected virtual void OnUpdateFailed(Exception exception)
    {
        UpdateFailed?.Invoke(this, new UpdateFailedEventArgs(exception));
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public bool IsInitialized { get; private set; }

    /// <inheritdoc/>
    public bool IsInitializing { get; private set; }

    /// <inheritdoc/>
    public bool IsUpdating { get; private set; }

    #endregion

    #region Initialize Logic

    private Task? initializeHolder;

    /// <summary>
    /// Provides an overridable method for pre initialization phase of the session.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    protected virtual ValueTask PreInitializeAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask(Task.CompletedTask);
    }

    /// <summary>
    /// Provides an overridable method for post initialization phase of the session.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    protected virtual ValueTask PostInitializeAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask(Task.CompletedTask);
    }

    /// <inheritdoc/>
    public async ValueTask InitializeAsync(Func<RetryIfErrorArgs, Task>? onError, CancellationToken cancellationToken = default)
    {
        if (IsInitialized)
        {
            return;
        }

        IsInitializing = true;

        await TaskHelpers.SingleTaskInvoker(() => TaskHelpers.RetryIfError(async delegate
        {
            OnInitializing();

            try
            {
                await PreInitializeAsync(cancellationToken);
                await PostInitializeAsync(cancellationToken);

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                OnInitializationFailed(ex);
                throw;
            }
            finally
            {
                IsInitializing = false;
                OnInitialized();
            }

        }, async ex =>
        {
            if (onError != null)
            {
                await onError.Invoke(ex);
            }

        }, cancellationToken), ref initializeHolder, cancellationToken);
    }

    /// <inheritdoc/>
    public ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        return InitializeAsync(args =>
        {
            throw args.Exception;

        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async void Initialize(CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
    }

    #endregion

    #region Update Logic

    private Task? updateHolder;

    /// <summary>
    /// Provides an overridable method for pre update phase of the session.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    protected virtual ValueTask PreUpdateAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask(Task.CompletedTask);
    }

    /// <summary>
    /// Provides an overridable method for post update phase of the session.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    protected virtual ValueTask PostUpdateAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask(Task.CompletedTask);
    }

    /// <inheritdoc/>
    public async ValueTask UpdateAsync(Func<RetryIfErrorArgs, Task>? onError, CancellationToken cancellationToken = default)
    {
        IsUpdating = true;

        await TaskHelpers.SingleTaskInvoker(() => TaskHelpers.RetryIfError(async delegate
        {
            OnUpdating();

            try
            {
                await PreUpdateAsync(cancellationToken);
                await PostUpdateAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                OnUpdateFailed(ex);
                throw;
            }
            finally
            {
                IsUpdating = false;
                OnUpdated();
            }

        }, async ex =>
        {
            if (onError != null)
            {
                await onError.Invoke(ex);
            }

        }, cancellationToken), ref updateHolder, cancellationToken);
    }

    /// <inheritdoc/>
    public ValueTask UpdateAsync(CancellationToken cancellationToken = default)
    {
        return UpdateAsync(args =>
        {
            throw args.Exception;

        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async void Update(CancellationToken cancellationToken = default)
    {
        await UpdateAsync(cancellationToken);
    }

    #endregion

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
