namespace KeyBasedAuthenticationTemplate_API.Common.Exceptions
{
    // Named distinctly from System.UnauthorizedAccessException so the global
    // exception handler can map it to 401 without catching framework-internal cases.
    public class UnauthorizedAppException : Exception
    {
        public UnauthorizedAppException(string message) : base(message)
        {
        }
    }
}
