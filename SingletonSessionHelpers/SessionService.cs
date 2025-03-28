﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TransactionHelpers;

namespace SingletonSessionHelpers.Abstraction;

/// <summary>
/// Implementation for all singleton session service.
/// </summary>
public abstract partial class SessionService : ISessionService
{
    #region Events

    /// <summary>
    /// Event args for InitializationFailed event.
    /// </summary>
    public class InitializationFailedEventArgs : EventArgs
    {
        /// <summary>
        /// The exception.
        /// </summary>
        public Exception Exception { get; }

        internal InitializationFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }

    /// <summary>
    /// Event args for UpdateFailed event.
    /// </summary>
    public class UpdateFailedEventArgs : EventArgs
    {
        /// <summary>
        /// The exception.
        /// </summary>
        public Exception Exception { get; }

        internal UpdateFailedEventArgs(Exception exception)
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

    /// <inheritdoc/>
    public event EventHandler? InitializingOrUpdating;

    /// <inheritdoc/>
    public event EventHandler? InitializedOrUpdated;

    /// <summary>
    /// Event invoker for <see cref="Initializing"/> event.
    /// </summary>
    protected virtual void OnInitializing()
    {
        Initializing?.Invoke(this, EventArgs.Empty);
        InitializingOrUpdating?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Event invoker for <see cref="Initialized"/> event.
    /// </summary>
    protected virtual void OnInitialized()
    {
        Initialized?.Invoke(this, EventArgs.Empty);
        InitializedOrUpdated?.Invoke(this, EventArgs.Empty);
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
        InitializingOrUpdating?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Event invoker for <see cref="Updated"/> event.
    /// </summary>
    protected virtual void OnUpdated()
    {
        Updated?.Invoke(this, EventArgs.Empty);
        InitializedOrUpdated?.Invoke(this, EventArgs.Empty);
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

    /// <inheritdoc/>
    public DateTimeOffset LastUpdate { get; private set; }

    internal List<ISessionService> SubscribedSessionServices { get; } = new();

    internal List<ISessionService> AsyncSubscribedSessionServices { get; } = new();

    private readonly SemaphoreSlim initializerLocker = new(1, 1);

    private readonly SemaphoreSlim updateLocker = new(1, 1);

    #endregion

    #region Initialize Logic

    /// <summary>
    /// Provides an overridable method for pre initialization phase of the session.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    protected virtual ValueTask<Result> PreInitializeAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result>(new Result());
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
    protected virtual ValueTask<Result> PostInitializeAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result>(new Result());
    }

    /// <inheritdoc/>
    public async ValueTask<Result> InitializeAsync(CancellationToken cancellationToken = default)
    {
        Result result = new();

        try
        {
            await initializerLocker.WaitAsync(cancellationToken);
        }
        catch { }

        if (cancellationToken.IsCancellationRequested)
        {
            return result.WithError(new OperationCanceledException());
        }

        try
        {
            if (IsInitialized)
            {
                return result;
            }

            IsInitializing = true;

            OnInitializing();

            (await PreInitializeAsync(cancellationToken)).ThrowIfError();

            (await PreInitializeOrUpdateAsync(cancellationToken)).ThrowIfError();

            if (SubscribedSessionServices.Count > 0 || AsyncSubscribedSessionServices.Count > 0)
            {
                List<Task<Result>> tasks = new()
                {
                    Task.Run(async delegate
                    {
                        Result taskResult = new();

                        foreach (var service in SubscribedSessionServices)
                        {
                            taskResult.WithResult(taskResult);
                            taskResult.WithResult(await service.InitializeAsync(cancellationToken));

                            if (taskResult.IsError)
                            {
                                return taskResult;
                            }
                        }

                        return taskResult;
                    })
                };

                foreach (var service in AsyncSubscribedSessionServices)
                {
                    tasks.Add(service.InitializeAsync(cancellationToken).AsTask());
                }

                foreach (var subResult in await Task.WhenAll(tasks.ToArray()))
                {
                    if (subResult.IsError)
                    {
                        return subResult;
                    }
                }
            }

            (await PostInitializeOrUpdateAsync(cancellationToken)).ThrowIfError();

            (await PostInitializeAsync(cancellationToken)).ThrowIfError();

            IsInitialized = true;

            OnInitialized();
        }
        catch (Exception ex)
        {
            OnInitializationFailed(ex);
            result.WithError(ex);
        }
        finally
        {
            IsInitializing = false;
            initializerLocker.Release();
        }

        return result;
    }

    /// <inheritdoc/>
    public async ValueTask InitializeAsync(Func<Result, Task<bool>> retryCallback, CancellationToken cancellationToken = default)
    {
        bool retry = false;
        do
        {
            Result result = await InitializeAsync(cancellationToken);
            if (result.IsError)
            {
                retry = await retryCallback(result);
            }
        }
        while (retry);
    }

    /// <inheritdoc/>
    public async void Initialize(CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async void Initialize(Func<Result, Task<bool>> retryCallback, CancellationToken cancellationToken = default)
    {
        await InitializeAsync(retryCallback, cancellationToken);
    }

    #endregion

    #region Update Logic

    /// <summary>
    /// Provides an overridable method for pre update phase of the session.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    protected virtual ValueTask<Result> PreUpdateAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result>(new Result());
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
    protected virtual ValueTask<Result> PostUpdateAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result>(new Result());
    }

    /// <inheritdoc/>
    public async ValueTask<Result> UpdateAsync(bool initializeFirst, CancellationToken cancellationToken = default)
    {
        Result result = new();

        try
        {
            await updateLocker.WaitAsync(cancellationToken);
        }
        catch { }

        try
        {
            if (cancellationToken.IsCancellationRequested)
            {
                result.WithError(new OperationCanceledException());
                return result;
            }

            if (!IsInitialized && initializeFirst)
            {
                result.WithResult(result);
                result.WithResult(await InitializeAsync(cancellationToken));
                if (result.IsError)
                {
                    return result;
                }
            }

            IsUpdating = true;

            OnUpdating();

            (await PreUpdateAsync(cancellationToken)).ThrowIfError();

            (await PreInitializeOrUpdateAsync(cancellationToken)).ThrowIfError();

            if (SubscribedSessionServices.Count > 0 || AsyncSubscribedSessionServices.Count > 0)
            {
                List<Task<Result>> tasks = new()
                {
                    Task.Run(async delegate
                    {
                        Result taskResult = new();

                        foreach (var service in SubscribedSessionServices)
                        {
                            taskResult.WithResult(taskResult);
                            taskResult.WithResult(await service.UpdateAsync(initializeFirst, cancellationToken));

                            if (taskResult.IsError)
                            {
                                return taskResult;
                            }
                        }

                        return taskResult;
                    })
                };

                foreach (var service in AsyncSubscribedSessionServices)
                {
                    tasks.Add(service.UpdateAsync(initializeFirst, cancellationToken).AsTask());
                }

                foreach (var subResult in await Task.WhenAll(tasks.ToArray()))
                {
                    if (subResult.IsError)
                    {
                        return subResult;
                    }
                }
            }

            (await PostInitializeOrUpdateAsync(cancellationToken)).ThrowIfError();

            (await PostUpdateAsync(cancellationToken)).ThrowIfError();

            OnUpdated();
        }
        catch (Exception ex)
        {
            OnUpdateFailed(ex);
            result.WithError(ex);
        }
        finally
        {
            LastUpdate = DateTimeOffset.Now;
            IsUpdating = false;
            updateLocker.Release();
        }

        return result;
    }

    /// <inheritdoc/>
    public ValueTask<Result> UpdateAsync(CancellationToken cancellationToken = default)
    {
        return UpdateAsync(true, cancellationToken);
    }

    /// <inheritdoc/>
    public async ValueTask UpdateAsync(Func<Result, Task<bool>> retryCallback, CancellationToken cancellationToken = default)
    {
        bool retry = false;
        do
        {
            Result result = await UpdateAsync(cancellationToken);
            if (result.IsError)
            {
                retry = await retryCallback(result);
            }
        }
        while (retry);
    }

    /// <inheritdoc/>
    public async ValueTask UpdateAsync(bool initializeFirst, Func<Result, Task<bool>> retryCallback, CancellationToken cancellationToken = default)
    {
        bool retry = false;
        do
        {
            Result result = await UpdateAsync(initializeFirst, cancellationToken);
            if (result.IsError)
            {
                retry = await retryCallback(result);
            }
        }
        while (retry);
    }

    /// <inheritdoc/>
    public async void Update(CancellationToken cancellationToken = default)
    {
        await UpdateAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async void Update(bool initializeFirst, CancellationToken cancellationToken = default)
    {
        await UpdateAsync(initializeFirst, cancellationToken);
    }

    /// <inheritdoc/>
    public async void Update(Func<Result, Task<bool>> retryCallback, CancellationToken cancellationToken = default)
    {
        await UpdateAsync(retryCallback, cancellationToken);
    }

    /// <inheritdoc/>
    public async void Update(bool initializeFirst, Func<Result, Task<bool>> retryCallback, CancellationToken cancellationToken = default)
    {
        await UpdateAsync(initializeFirst, retryCallback, cancellationToken);
    }

    #endregion

    #region Initialize or Update Logic

    /// <summary>
    /// Provides an overridable method for pre initialize or update phase of the session.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    protected virtual ValueTask<Result> PreInitializeOrUpdateAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result>(new Result());
    }

    /// <summary>
    /// Provides an overridable method for post initialize or update phase of the session.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token for the created <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// The created <see cref="ValueTask"/>.
    /// </returns>
    protected virtual ValueTask<Result> PostInitializeOrUpdateAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result>(new Result());
    }

    #endregion

    #region Methods

    /// <summary>
    /// Subscribe the <see cref="ISessionService"/> with initialize and update actions.
    /// </summary>
    /// <param name="sessionService"> The <see cref="ISessionService"/> to subscribe. </param>
    /// <param name="isAsync"> Sets <c>true</c> whether the service will process actions in async mode; otherwise, <c>false</c>. </param>
    public void SubscribeService(ISessionService sessionService, bool isAsync = true)
    {
        if (isAsync)
        {
            AsyncSubscribedSessionServices.Add(sessionService);
        }
        else
        {
            SubscribedSessionServices.Add(sessionService);
        }
    }

    /// <summary>
    /// Unsubscribe the <see cref="ISessionService"/> from initialize and update actions.
    /// </summary>
    /// <param name="sessionService"> The <see cref="ISessionService"/> to unsubscribe. </param>
    public void UnsubscribeService(ISessionService sessionService)
    {
        SubscribedSessionServices.Remove(sessionService);
        AsyncSubscribedSessionServices.Remove(sessionService);
    }

    /// <inheritdoc/>
    public IEnumerable<(bool isAsync, ISessionService sessionService)> GetSubscribedSessionServices()
    {
        List<(bool isAsync, ISessionService sessionService)> services = new();

        services.AddRange(SubscribedSessionServices.Select(i => (false, i)));
        services.AddRange(AsyncSubscribedSessionServices.Select(i => (true, i)));

        return services;
    }

    #endregion
}
