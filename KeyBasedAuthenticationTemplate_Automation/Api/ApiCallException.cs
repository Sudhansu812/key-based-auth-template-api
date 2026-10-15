using System.Net;

namespace KeyBasedAuthenticationTemplate_Automation.Api
{
    public class ApiCallException : Exception
    {
        public ApiCallException(string message, HttpStatusCode? statusCode = null, Exception? innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode? StatusCode { get; }
    }
}
