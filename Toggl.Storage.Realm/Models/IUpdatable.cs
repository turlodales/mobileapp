namespace Toggl.Storage.Realm.Models
{
    internal interface IUpdatable : IPushable
    {
        void PrepareForSyncing();
        void UpdateSucceeded();
    }
}
