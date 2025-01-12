namespace SpongeEngine.LMStudioSharp.Models
{
    public class Exception : System.Exception
    {
        public string Provider { get; }
        public int? StatusCode { get; }
        public string? ResponseContent { get; }

        public Exception(
            string message,
            string provider,
            int? statusCode = null,
            string? responseContent = null) 
            : base(message)
        {
            Provider = provider;
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }

        public Exception(string message) : base(message)
        {
            Provider = "LMStudio";
        }

        public Exception(string message, System.Exception innerException) 
            : base(message, innerException)
        {
            Provider = "LMStudio";
        }
    }
}