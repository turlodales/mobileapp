namespace Toggl.Networking.Sync.Push
{
    public class SuccessPayloadResult<T> : IActionResult<T>
    {
        public T Payload { get; }

        public bool Success => true;

        public SuccessPayloadResult(T payload)
        {
            Payload = payload;
        }
    }
}
