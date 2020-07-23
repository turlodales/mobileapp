namespace Toggl.Storage.Realm.Models
{
    internal interface IPushable
    {
        void PushFailed(string errorMessage);
    }
}
