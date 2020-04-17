namespace Toggl.Networking.Sync.Push
{
    public interface IActionResult<out T>
    {
        bool Success { get; }
    }
}
