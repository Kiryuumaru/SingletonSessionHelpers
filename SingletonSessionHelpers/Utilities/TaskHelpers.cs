using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SingletonSessionHelpers.Utilities;

internal static class TaskHelpers
{
    public static Task SingleTaskInvoker(Func<Task?> task, ref Task? taskHolder, CancellationToken cancellationToken = default)
    {
        if (taskHolder == null || taskHolder.IsCompleted)
        {
            taskHolder = Task.Run(task, cancellationToken);

            return taskHolder;
        }
        else
        {
            return taskHolder.ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    throw task.Exception;
                }
            }, cancellationToken);
        }
    }

    public static async Task RetryIfError(Func<Task> task, Func<RetryIfErrorArgs, Task> error, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            try
            {
                await Task.Run(task, cancellationToken);

                return;
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                else
                {
                    RetryIfErrorArgs args = new(ex);
                    await error.Invoke(args);
                    if (!args.Retry)
                    {
                        return;
                    }
                }
            }
        }
    }
}
