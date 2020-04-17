namespace Toggl.Networking.Sync.Push
{
    public interface IEntityActionResult<out T>
    {
        long Id { get; }
        IActionResult<T> Result { get; }
    }
}
