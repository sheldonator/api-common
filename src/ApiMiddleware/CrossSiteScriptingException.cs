using System;

namespace ApiMiddleware
{
    public class CrossSiteScriptingException : Exception
    {
        public CrossSiteScriptingException(string message) : base(message)
        {

        }

        public CrossSiteScriptingException(Exception ex) : base(ex.Message)
        {

        }
    }
}