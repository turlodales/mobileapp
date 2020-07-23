namespace Toggl.Networking.Sync.Push
{
    public class DeleteActionResult<T> : IEntityActionResult<T>
    {
        public long Id { get; private set; }
        public IActionResult<T> Result { get; private set; }

        public DeleteActionResult(long id, IActionResult<T> result)
        {
            Id = id;
            Result = result;
        }
    }
}
