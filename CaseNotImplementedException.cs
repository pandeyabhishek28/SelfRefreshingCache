using System;

namespace Assignment.SelfRefreshingCache
{
    public class CaseNotImplementedException : Exception
    {
        public CaseNotImplementedException(string message) : base(message)
        {

        }

        public CaseNotImplementedException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}
