using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SingletonSessionHelpers.Utilities;

/// <summary>
/// Event args for operation that provides a retry option.
/// </summary>
public class RetryIfErrorArgs
{
    /// <summary>
    /// The exception of the error.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets or sets <c>true</c> if the operation will retry; otherwise, <c>false</c>.
    /// </summary>
    public bool Retry { get; set; } = true;

    /// <summary>
    /// Creates a new instance for <see cref="RetryIfErrorArgs"/>.
    /// </summary>
    /// <param name="exception">
    /// The exception of the error.
    /// </param>
    public RetryIfErrorArgs(Exception exception)
    {
        Exception = exception;
    }
}
