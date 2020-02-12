using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Assignment.SelfRefreshingCache
{
    public class SelfRefreshingCache<TResult> : IDisposable
    {
        private readonly ILogger _logger;
        private readonly int _refreshPeriodInSeconds;
        private readonly int _validityOfResultInSeconds;
        private readonly Func<TResult> _createFunction;
        private readonly Stopwatch _stopwatch;
        private readonly object _resultLockObject;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private TResult _result;
        private Timer _timer;
        private bool _disposed;

        public SelfRefreshingCache(ILogger logger,
                                   int refreshPeriodInSeconds,
                                   int validityOfResultInSeconds,
                                   Func<TResult> createFunction)
        {
            _logger = logger;
            this._refreshPeriodInSeconds = refreshPeriodInSeconds;
            this._validityOfResultInSeconds = validityOfResultInSeconds;
            this._createFunction = createFunction ?? throw new ArgumentNullException("createFunction");
            _stopwatch = new Stopwatch();
            _disposed = false;
            _resultLockObject = new object();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public TResult GetOrCreate()
        {
            if (_timer == null)
            {
                // As timer is null so we have to excute the function, We can set Timer's DueTime to 0 but that will not 
                // stop this thread to move forward and if it move forward then there is no result as this is the very first call
                // to this function, so for the first time we are excuting the function and registering the timer
                // so from next time it will excute the function as per the given period.

                var dueTime = _refreshPeriodInSeconds * 1000; // converting it into miliseconds

                _timer = new Timer(new TimerCallback(ExcuteCreateFunction),
                             null,
                             dueTime,
                             dueTime);

                ExcuteCreateFunction(null);
            }

            if (_stopwatch.Elapsed.TotalSeconds <= _refreshPeriodInSeconds)
            {
                return _result;
            }
            else
            {
                if (_stopwatch.Elapsed.TotalSeconds <= _validityOfResultInSeconds)
                {
                    return _result;
                }
                else
                {
                    var exception = new CaseNotImplementedException("Elapsed.TotalSeconds > ValidityOfResultInSeconds");
                    _logger.LogError(exception, "Elapsed.TotalSeconds is greater than the validity of result in seconds.");
                    throw exception;
                }
            }
        }

        private void ExcuteCreateFunction(object state)
        {
            try
            {
                // Timer t = (Timer)state;  we can get the Timer here and do something if required.
                // Yes, there is a another way for passing the token with the excuting task
                // Just making it simple here
                if (_cancellationTokenSource.IsCancellationRequested) return;
                lock (_resultLockObject)
                {
                    _result = _createFunction();
                    _stopwatch.Restart();
                }
                // As we have created a new result then just reset the stop watch. Also this will help us to use the 
                // _validityOfResultInSeconds field in case we hit an exception in any call to _createFunction our _stopwatch timing will be 
                // preserverd.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occured while creating new result.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _cancellationTokenSource.Cancel();
                _timer.Dispose();
                _cancellationTokenSource.Dispose();
            }

            _disposed = true;
        }
    }
}
