using System;
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
}
