namespace Toggl.Networking.Sync.Push
{
    public class CreateActionResult<T> : IEntityActionResult<T>
    {
        public long Id { get; private set; }
        public IActionResult<T> Result { get; private set; }

        public CreateActionResult(long id, IActionResult<T> result)
        {
            Id = id;
            Result = result;
        }
    }
}
