using System;

namespace Toggl.Storage.Realm.Sync
{
    internal static class BackupHelper
    {
        public static bool ClearBackupIf(bool isClean, Action clear)
        {
            if (isClean)
                clear();

            return isClean;
        }
    }
}