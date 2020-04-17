namespace Toggl.Networking.Sync.Push
{
    public class SuccessResult<T> : IActionResult<T>
    {
        public bool Success => true;
    }
}
