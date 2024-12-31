namespace LocalAI.NET.LMStudio.Models
{
    public class LmStudioException : Exception
    {
        public string Provider { get; }
        public int? StatusCode { get; }
        public string? ResponseContent { get; }

        public LmStudioException(
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

        public LmStudioException(string message) : base(message)
        {
            Provider = "LMStudio";
        }

        public LmStudioException(string message, Exception innerException) 
            : base(message, innerException)
        {
            Provider = "LMStudio";
        }
    }
}