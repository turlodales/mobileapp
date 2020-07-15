namespace Toggl.Storage.Realm.Models
{
    internal interface ISyncable<T>
    {
        void SaveSyncResult(T entity, Realms.Realm realm);
    }
}
