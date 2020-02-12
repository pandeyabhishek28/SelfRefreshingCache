# SelfRefreshingCache
SelfRefreshingCache
Description: A self-refreshing cache class according to the given details. 

Specified is a constructor and one public method:

    public class SelfRefreshingCache<TResult>
    {
        public SelfRefreshingCache(ILogger logger,
                                   int refreshPeriodSeconds,
                                   int validityOfResultSeconds, 
                                   Func<TResult> createFunction)
        {
            // ...
        }

        public TResult GetOrCreate()
        {
            // ...
        }
    }

Constructor parameters:
•	logger: logger instance to report errors
•	refreshPeriodMs: interval of automatic background refresh (in seconds)
•	validityOfResultSeconds: how long can we keep returning one result. This property has its importance when createFunction fails during refresh – in that case we keep returning the previously created result until its validity expires.
•	createFunction: function, that creates the TResult object. CreateFunction can be e.g. a download function from Web, Database, or it can be some CPU heavy calculation.

Behaviour:
•	GetOrCreate() function:
  o	gets or creates the result
  o	if called for the first time, then also starts automatic refreshes
  o	in reasonable cases, e.g. when the result cannot be obtained, throws an exception
•	The class is:
  o	lazy: until GetOrCreate() is called for the first time nothing happens in the background
  o	thread safe
