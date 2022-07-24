using DisposableHelpers.Attributes;
using SingletonSessionHelpers.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SingletonSessionHelpers.Abstraction;

[AsyncDisposable]
public abstract partial class SessionService : ISessionService
{
    #region Events

    public class InitializationFailedEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public InitializationFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }

    public class UpdateFailedEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public UpdateFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }

    public event EventHandler? Initializing;

    public event EventHandler? Initialized;

    public event EventHandler<InitializationFailedEventArgs>? InitializationFailed;

    public event EventHandler? Updating;

    public event EventHandler? Updated;

    public event EventHandler<UpdateFailedEventArgs>? UpdateFailed;

    private void OnInitializing()
    {
        Initializing?.Invoke(this, EventArgs.Empty);
    }

    private void OnInitialized()
    {
        Initialized?.Invoke(this, EventArgs.Empty);
    }

    private void OnInitializationFailed(Exception exception)
    {
        InitializationFailed?.Invoke(this, new InitializationFailedEventArgs(exception));
    }

    private void OnUpdating()
    {
        Updating?.Invoke(this, EventArgs.Empty);
    }

    private void OnUpdated()
    {
        Updated?.Invoke(this, EventArgs.Empty);
    }

    private void OnUpdateFailed(Exception exception)
    {
        UpdateFailed?.Invoke(this, new UpdateFailedEventArgs(exception));
    }

    #endregion

    #region Properties

    public bool IsInitialized { get; private set; }

    public bool IsInitializing { get; private set; }

    public bool IsUpdating { get; private set; }

    public DateTimeOffset? LastUpdated { get; private set; }

    #endregion

    #region Initialize Logic

    private Task? initializeHolder;

    protected virtual ValueTask PreInitializeAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask(Task.CompletedTask);
    }

    protected virtual ValueTask PostInitializeAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask(Task.CompletedTask);
    }

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

    public ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        return InitializeAsync(args =>
        {
            throw args.Exception;

        }, cancellationToken);
    }

    public async void Initialize(CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
    }

    #endregion

    #region Update Logic

    private Task? updateHolder;

    protected virtual ValueTask PreUpdateAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask(Task.CompletedTask);
    }

    protected virtual ValueTask PostUpdateAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask(Task.CompletedTask);
    }

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

                LastUpdated = DateTimeOffset.Now;
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

    public ValueTask UpdateAsync(CancellationToken cancellationToken = default)
    {
        return UpdateAsync(args =>
        {
            throw args.Exception;

        }, cancellationToken);
    }

    public async void Update(CancellationToken cancellationToken = default)
    {
        await UpdateAsync(cancellationToken);
    }

    #endregion

    #region Disposable Logic

    protected virtual ValueTask PreDisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    protected virtual ValueTask PostDisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    public async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await PreDisposeAsync();
            await PostDisposeAsync();
        }
    }

    public async void Dispose(bool disposing)
    {
        await DisposeAsync(disposing);
    }

    #endregion
}
