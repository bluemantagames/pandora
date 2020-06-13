using RestSharp;
using System.Net;

namespace Pandora.Network.Data
{
    public class ApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; private set; }
        public T Body { get; private set; } = default(T);
        public ApiError Error { get; private set; } = null;

        public ApiResponse(HttpStatusCode statusCode, T body)
        {
            StatusCode = statusCode;
            Body = body;
        }

        public ApiResponse(HttpStatusCode statusCode, ApiError error)
        {
            StatusCode = statusCode;
            Error = error;
        }
    }
}