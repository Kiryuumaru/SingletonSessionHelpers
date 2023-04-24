﻿using SingletonSessionHelpers.Utilities;
using System;
using System.Collections.Generic;
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

    /// <inheritdoc/>
    public event EventHandler? Initializing;

    /// <inheritdoc/>
    public event EventHandler? Initialized;

    /// <inheritdoc/>
    public event EventHandler<Exception>? InitializationFailed;

    /// <inheritdoc/>
    public event EventHandler? Updating;

    /// <inheritdoc/>
    public event EventHandler? Updated;

    /// <inheritdoc/>
    public event EventHandler<Exception>? UpdateFailed;

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
        InitializationFailed?.Invoke(this, exception);
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
        UpdateFailed?.Invoke(this, exception);
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public bool IsInitialized { get; private set; }

    /// <inheritdoc/>
    public bool IsInitializing { get; private set; }

    /// <inheritdoc/>
    public bool IsUpdating { get; private set; }

    internal List<ISessionService> SubscribedSessionServices { get; } = new();

    internal List<ISessionService> AsyncSubscribedSessionServices { get; } = new();

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
    protected virtual ValueTask<Response> PreInitializeAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<Response>(new Response());
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
    protected virtual ValueTask<Response> PostInitializeAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<Response>(new Response());
    }

    /// <inheritdoc/>
    public async ValueTask<Response> InitializeAsync(CancellationToken cancellationToken = default)
    {
        Response response = new();

        if (IsInitialized)
        {
            return response;
        }

        IsInitializing = true;

        try
        {
            await TaskHelpers.SingleTaskInvoker(async delegate
            {
                try
                {
                    OnInitializing();

                    (await PreInitializeAsync(cancellationToken)).ThrowIfError();

                    (await PreInitializeOrUpdateAsync(cancellationToken)).ThrowIfError();

                    List<Task<Response>> tasks = new()
                    {
                        Task.Run(async delegate
                        {
                            Response taskResponse = new();

                            foreach (var service in SubscribedSessionServices)
                            {
                                taskResponse.Append(await service.InitializeAsync(cancellationToken));

                                if (taskResponse.IsError)
                                {
                                    return taskResponse;
                                }
                            }

                            return taskResponse;
                        })
                    };

                    foreach (var service in AsyncSubscribedSessionServices)
                    {
                        tasks.Add(service.InitializeAsync(cancellationToken).AsTask());
                    }

                    foreach (var result in await Task.WhenAll(tasks.ToArray()))
                    {
                        result.ThrowIfError();
                    }

                    (await PostInitializeOrUpdateAsync(cancellationToken)).ThrowIfError();

                    (await PostInitializeAsync(cancellationToken)).ThrowIfError();

                    IsInitialized = true;

                    OnInitialized();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    IsInitializing = false;
                }

            }, ref initializeHolder, cancellationToken);
        }
        catch (Exception ex)
        {
            OnInitializationFailed(ex);
            response.Append(ex);
        }

        return response;
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
    protected virtual ValueTask<Response> PreUpdateAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<Response>(new Response());
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
    protected virtual ValueTask<Response> PostUpdateAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<Response>(new Response());
    }

    /// <inheritdoc/>
    public async ValueTask<Response> UpdateAsync(CancellationToken cancellationToken = default)
    {
        Response response = new();

        IsUpdating = true;

        try
        {
            await TaskHelpers.SingleTaskInvoker(async delegate
            {
                OnUpdating();

                try
                {
                    (await PreUpdateAsync(cancellationToken)).ThrowIfError();

                    (await PreInitializeOrUpdateAsync(cancellationToken)).ThrowIfError();

                    List<Task<Response>> tasks = new()
                    {
                        Task.Run(async delegate
                        {
                            Response taskResponse = new();

                            foreach (var service in SubscribedSessionServices)
                            {
                                taskResponse.Append(await service.UpdateAsync(cancellationToken));

                                if (taskResponse.IsError)
                                {
                                    return taskResponse;
                                }
                            }

                            return taskResponse;
                        })
                    };

                    foreach (var service in AsyncSubscribedSessionServices)
                    {
                        tasks.Add(service.UpdateAsync(cancellationToken).AsTask());
                    }

                    foreach (var result in await Task.WhenAll(tasks.ToArray()))
                    {
                        result.ThrowIfError();
                    }

                    (await PostInitializeOrUpdateAsync(cancellationToken)).ThrowIfError();

                    (await PostUpdateAsync(cancellationToken)).ThrowIfError();

                    OnUpdated();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    IsUpdating = false;
                }

            }, ref updateHolder, cancellationToken);
        }
        catch (Exception ex)
        {
            OnUpdateFailed(ex);
            response.Append(ex);
        }

        return response;
    }

    /// <inheritdoc/>
    public async void Update(CancellationToken cancellationToken = default)
    {
        await UpdateAsync(cancellationToken);
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
    protected virtual ValueTask<Response> PreInitializeOrUpdateAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<Response>(new Response());
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
    protected virtual ValueTask<Response> PostInitializeOrUpdateAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<Response>(new Response());
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

    #endregion
}
