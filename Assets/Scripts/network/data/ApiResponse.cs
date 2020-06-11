using RestSharp;

namespace Pandora.Network.Data
{
    public class ApiResponse<T>
    {
        public IRestResponse<T> RestResponse { get; private set; } = null;
        public T Body { get; private set; } = default(T);
        public ApiError Error { get; private set; } = null;

        public ApiResponse(IRestResponse<T> restResponse, T body)
        {
            RestResponse = restResponse;
            Body = body;
        }

        public ApiResponse(IRestResponse<T> restResponse, ApiError error)
        {
            RestResponse = restResponse;
            Error = error;
        }
    }
}