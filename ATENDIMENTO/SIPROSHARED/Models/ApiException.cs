namespace SIPROSHARED.Models
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public string ErrorMessage { get; }

        public ApiException(int statusCode, string errorMessage)
        {
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
        }

    }
}
