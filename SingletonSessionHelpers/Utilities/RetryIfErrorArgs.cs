using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SingletonSessionHelpers.Utilities;

public class RetryIfErrorArgs
{
    public Exception Exception { get; }

    public bool Retry { get; set; } = true;

    public RetryIfErrorArgs(Exception exception)
    {
        Exception = exception;
    }
}
