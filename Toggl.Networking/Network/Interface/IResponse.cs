using System.Net;
using System.Net.Http.Headers;

namespace Toggl.Networking.Network
{
    internal interface IResponse
    {
        string RawData { get; }

        bool IsSuccess { get; }

        string ContentType { get; }

        HttpStatusCode StatusCode { get; }

        HttpHeaders Headers { get; }

        bool IsJson { get; }

        bool TryGetBucketSizeFromHeaders(out uint size);
    }
}
