namespace Toggl.Networking.Sync.Push
{
    public class ErrorResult<T> : IActionResult<T>
    {
        public ErrorMessage ErrorMessage { get; set; }

        public bool Success => false;

        public ErrorResult(ErrorMessage error)
        {
            ErrorMessage = error;
        }
    }
}
