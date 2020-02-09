using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using Toggl.Shared;

namespace Toggl.Networking.Network
{
    internal sealed class Response : IResponse
    {
        private const string bucketSizeHeaderName = "x-toggl-bucket-size";

        public string RawData { get; }
        public bool IsSuccess { get; }
        public string ContentType { get; }
        public HttpStatusCode StatusCode { get; }
        public HttpHeaders Headers { get; }

        public bool IsJson => ContentType == "application/json";

        public Response(string rawData, bool isSuccess, string contentType, HttpHeaders headers, HttpStatusCode statusCode)
        {
            Ensure.Argument.IsNotNull(rawData, nameof(rawData));
            Ensure.Argument.IsNotNull(contentType, nameof(contentType));
            Ensure.Argument.IsNotNull(headers, nameof(headers));

            RawData = rawData;
            IsSuccess = isSuccess;
            ContentType = contentType;
            StatusCode = statusCode;
            Headers = headers;
        }

        public bool TryGetBucketSizeFromHeaders(out uint size)
        {
            size = 0;

            if (Headers.TryGetValues(bucketSizeHeaderName, out var values))
            {
                var value = values.FirstOrDefault();
                return value != null && uint.TryParse(value, out size);
            }

            return false;
        }
    }
}
